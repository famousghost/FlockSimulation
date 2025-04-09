namespace McFlockSystem
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Unity.VisualScripting;
    using UnityEngine;

    public enum CalculationTypes
    {
        CPU = 0,
        GPU = 1,
        GPU_Octree = 2
    }

    public sealed class Flock : MonoBehaviour
    {
        #region Inspector Variables
        [Header("Debug visualization")]
        [SerializeField] private bool _QTreeOptimizationEnabled;
        [SerializeField] private bool _EnableBoidCheckVisualization;

        [Header("Necessary elements")]
        [SerializeField] private QtreeManager _QtreeManager;
        [SerializeField] private FlockArea _FlockArea;

        [Header("Flock parameters")]
        [SerializeField] private float _CohesionStrength;
        [SerializeField] private float _SeparationStrength;
        [SerializeField] private float _AligmentStrength;
        [SerializeField] private float _WallAvoidanceStrength;
        [Header("ComputeShader")]
        [SerializeField] private ComputeShader _FlockSimulationComputeShader;
        [SerializeField] private int _SpherePitchSize;
        [SerializeField] private int _SphereYawSize;
        [SerializeField] private float _MaxAngle;
        [SerializeField] private float _MaxRayLength;

        [Header("Configuration")]
        [SerializeField] private CalculationTypes _CalculationTypes;

        #endregion Inspector Variables

        #region Public Variables
        public List<Boid> Boids;
        public List<Obstacle> Obstacles;
        public static Flock Instance;
        #endregion

        #region Public Methods
        public void AddBois(Boid boid)
        {
            if(Boids == null)
            {
                Boids = new List<Boid>();
            }
            if(Boids.Contains(boid))
            {
                return;
            }
            Boids.Add(boid);
        }

        public void RemoveBoid(Boid boid)
        {
            if (Boids == null)
            {
                Boids = new List<Boid>();
            }
            if (!Boids.Contains(boid))
            {
                return;
            }
            Boids.Remove(boid);
        }

        public void AddObstacle(Obstacle obstacle)
        {
            if (Obstacles == null)
            {
                Obstacles = new List<Obstacle>();
            }
            if(Obstacles.Contains(obstacle))
            {
                return;
            }
            Obstacles.Add(obstacle);
        }

        public void RemoveObstacle(Obstacle obstacle)
        {
            if (Obstacles == null)
            {
                Obstacles = new List<Obstacle>();
            }
            if(!Obstacles.Contains(obstacle))
            {
                return;
            }
            Obstacles.Remove(obstacle);
        }
        #endregion Public Methods

        #region Unity Methods
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _FlockShaderKernelIndex = _FlockSimulationComputeShader.FindKernel(_FlockShaderKernelName);
            InitializeBuffers();
        }

        private void OnValidate()
        {
            InitializeBuffers();
        }

        private void Update()
        {
            if (_CalculationTypes == CalculationTypes.CPU)
            {
                FlockSimulationCPU();
            }
            if(_CalculationTypes == CalculationTypes.GPU)
            {
                FlockSimulationGPU();
            }
        }

        private void OnDestroy()
        {
            //TODO: Delete unnecessary buffers
            /*
            _BoidsBuffer.Dispose();
            _BoidsBuffer = null;
            _ForcesBuffer.Dispose();
            _ForcesBuffer = null;
            _ObstaclesBuffer.Dispose();
            _ObstaclesBuffer = null;
            _AvoidancePointsBuffer.Dispose();
            _AvoidancePointsBuffer = null;
            */
        }

        private void OnDrawGizmos()
        {
            /*foreach (var boid in Boids)
            {
                for (int j = 0; j < _AvoidancePoints.Count; ++j)
                {
                    Gizmos.color = Color.red;
                    var worldSpacePoint = (Vector3)(boid.transform.localToWorldMatrix * _AvoidancePoints[j]);
                    var dir = (worldSpacePoint - boid.transform.position).normalized;
                    if (Mathf.Acos(Vector3.Dot(dir, boid.transform.forward)) <= _MaxAngle * Mathf.Deg2Rad)
                    {
                        Gizmos.DrawLine(boid.transform.position, boid.transform.localToWorldMatrix * _AvoidancePoints[j]);
                    }
                }
            }*/
            if(!_EnableBoidCheckVisualization)
            {
                return;
            }
            foreach(var boid in Boids)
            {
                var otherBoids = _QTreeOptimizationEnabled ? _QtreeManager.CollectClosesBoids(boid) : Boids;
                //Debug.Log($"[OnDrawGizmos] otherBoids.count = {otherBoids.Count}");
                foreach(var otherBoid in otherBoids)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(boid.transform.position, otherBoid.transform.position);
                }
            }
        }


        #endregion Unity Methods

        #region Private Variables
        private ComputeBuffer _BoidsBuffer;
        private ComputeBuffer _AvoidancePointsBuffer;
        private ComputeBuffer _ObstaclesBuffer;
        private ComputeBuffer _ForcesBuffer;

        private List<Vector4> _AvoidancePoints;

        private int _FlockShaderKernelIndex;

        private readonly int _BoidsBufferId = Shader.PropertyToID("_Boids");
        private readonly int _ObstalceBufferId = Shader.PropertyToID("_Obstacles");
        private readonly int _AvoidancePointsBufferId = Shader.PropertyToID("_AvoidancePoints");
        private readonly int _FlockForcesConstantBufferId = Shader.PropertyToID("FlockData");
        private readonly int _BoidsAmountId = Shader.PropertyToID("_BoidsAmount");
        private readonly int _ObstacleAmountId = Shader.PropertyToID("_ObstacleAmount");
        private readonly int _AvoidancePointsAmountId = Shader.PropertyToID("_AvoidancePointAmount");

        private readonly string _FlockShaderKernelName = "FlockSimulation";

        private BoidsStructureBuffer[] _BoidsReadBufferData;
        #endregion Private Variables

        #region Private Methods
        private void FlockSimulationCPU()
        {
            if (_QTreeOptimizationEnabled)
            {
                _QtreeManager.Initialize();
                foreach (var boid in Boids)
                {
                    _QtreeManager.AddBoid(boid);
                }
            }
            foreach (var boid in Boids)
            {
                var otherBoids = _QTreeOptimizationEnabled ? _QtreeManager.CollectClosesBoids(boid) : Boids;
                //Debug.Log($"[Update] otherBoids.count = {otherBoids.Count}");
                int totalAmount = 0;
                Vector3 aligementAccelaration = Vector3.zero;
                Vector3 cohesionPosition = Vector3.zero;
                Vector3 separationDir = Vector3.zero;
                foreach (var otherBoid in otherBoids)
                {
                    if (boid == otherBoid)
                    {
                        continue;
                    }

                    if (!boid.IsClosestBoid(otherBoid))
                    {
                        continue;
                    }
                    totalAmount++;

                    boid.Aligment(ref aligementAccelaration, otherBoid);

                    boid.Cohesion(ref cohesionPosition, otherBoid);

                    boid.Separation(ref separationDir, otherBoid);
                }

                if (totalAmount > 0)
                {

                    boid.UpdateAccelaration((aligementAccelaration / totalAmount).normalized * _AligmentStrength);
                    boid.UpdateAccelaration((cohesionPosition / totalAmount - boid.transform.position).normalized * _CohesionStrength);
                    boid.UpdateAccelaration(((separationDir / totalAmount)).normalized * _SeparationStrength);
                }
                boid.AvoidWalls(_FlockArea, _WallAvoidanceStrength);
                boid.UpdateBoid();

            }
        }

        private void FlockSimulationGPU()
        {
            PrepareBoidsBuffer();
            PrepareObstaclesBuffer();
            _FlockSimulationComputeShader.SetInt(_BoidsAmountId, Boids.Count);
            _FlockSimulationComputeShader.SetBuffer(_FlockShaderKernelIndex, _BoidsBufferId, _BoidsBuffer);
            _FlockSimulationComputeShader.SetInt(_ObstacleAmountId, Obstacles.Count);
            _FlockSimulationComputeShader.SetBuffer(_FlockShaderKernelIndex, _ObstalceBufferId, _ObstaclesBuffer);
            uint kernelX = 0u;
            _FlockSimulationComputeShader.GetKernelThreadGroupSizes(_FlockShaderKernelIndex, out kernelX, out _, out _);
            _FlockSimulationComputeShader.Dispatch(_FlockShaderKernelIndex, Boids.Count / (int)kernelX, 1, 1);
            _BoidsReadBufferData = new BoidsStructureBuffer[Boids.Count];
            _BoidsBuffer.GetData(_BoidsReadBufferData);
            _BoidsBuffer.Dispose();


            int i = 0;
            foreach(var boid in Boids)
            {
                var acceleration = _BoidsReadBufferData[i].Acceleration;
                boid.UpdateAccelaration(new Vector3(acceleration.x, acceleration.y, acceleration.z));
                boid.UpdateBoid();
                ++i;
            }
            
        }

        private void PrepareBoidsBuffer()
        {
            _BoidsBuffer = new ComputeBuffer(Boids.Count, Marshal.SizeOf<BoidsStructureBuffer>(), ComputeBufferType.Structured);
            List<BoidsStructureBuffer> boidsStructBuffers = new List<BoidsStructureBuffer>(); 
            foreach(var boid in Boids)
            {
                boidsStructBuffers.Add(new BoidsStructureBuffer(
                    new Vector4(boid.transform.position.x, boid.transform.position.y, boid.transform.position.z, boid.FlockRadius),
                    new Vector4(boid.transform.forward.x, boid.transform.forward.y, boid.transform.forward.z, 1.0f),
                    new Vector4(boid.Velocity.x, boid.Velocity.y, boid.Velocity.z, 1.0f),
                    new Vector4(boid.Acceleration.x, boid.Acceleration.y, boid.Acceleration.z, 1.0f),
                    Vector4.zero,
                    boid.transform.localToWorldMatrix));
            }
            _BoidsBuffer.SetData(boidsStructBuffers.ToArray());
        }

        private void PrepareObstaclesBuffer()
        {
            if(Obstacles == null || Obstacles.Count == 0)
            {
                return;
            }
            _ObstaclesBuffer = new ComputeBuffer(Obstacles.Count, Marshal.SizeOf<ObstaclesBuffer>(), ComputeBufferType.Structured);
            List<ObstaclesBuffer> obstacleBuffer = new List<ObstaclesBuffer>();
            foreach (var obstacle in Obstacles)
            {
                obstacleBuffer.Add(new ObstaclesBuffer(new Vector4(obstacle.Position.x, obstacle.Position.y, obstacle.Position.z, (int)obstacle.Type), 
                                                       obstacle.Size,
                                                       obstacle.Rotation));
            }
            _ObstaclesBuffer.SetData(obstacleBuffer.ToArray());
        }

        private void PrepareAvoidancePointsBuffer()
        {
            if(_AvoidancePoints == null)
            {
                _AvoidancePoints = new List<Vector4>();
            }
            GenerateAvoidancePoints();
            _AvoidancePointsBuffer = new ComputeBuffer(_AvoidancePoints.Count, sizeof(float) * 4, ComputeBufferType.Structured);
            _AvoidancePointsBuffer.SetData(_AvoidancePoints.ToArray());
        }

        private void PrepareFlockDataBuffer()
        {
            _ForcesBuffer = new ComputeBuffer(6, sizeof(float), ComputeBufferType.Constant);
            float[] forceData = { _CohesionStrength, _SeparationStrength, _AligmentStrength, _WallAvoidanceStrength, _MaxAngle, _MaxRayLength};
            _ForcesBuffer.SetData(forceData);
        }

        private void GenerateAvoidancePoints()
        {
            for (int pitch = 0; pitch < _SpherePitchSize; ++pitch)
            {
                for (int yaw = 0; yaw < _SphereYawSize; ++yaw)
                {
                    float yawAngle = (float)yaw / _SphereYawSize;
                    float pitchAngle = (float)pitch / _SpherePitchSize;
                    float x = Mathf.Cos(yawAngle * 2.0f * Mathf.PI) * Mathf.Cos(pitchAngle * 2.0f * Mathf.PI);
                    float y = Mathf.Sin(pitchAngle * 2.0f * Mathf.PI);
                    float z = Mathf.Sin(yawAngle * 2.0f * Mathf.PI) * Mathf.Cos(pitchAngle * 2.0f * Mathf.PI);
                    Vector3 point = new Vector3(x, y, z);
                    _AvoidancePoints.Add(new Vector4(point.x, point.y, point.z, 1.0f));
                }
            }
        }

        private void InitializeBuffers()
        {
            PrepareAvoidancePointsBuffer();
            PrepareFlockDataBuffer();
            _FlockSimulationComputeShader.SetInt(_AvoidancePointsAmountId, _AvoidancePoints.Count);
            _FlockSimulationComputeShader.SetBuffer(_FlockShaderKernelIndex, _AvoidancePointsBufferId, _AvoidancePointsBuffer);
            _FlockSimulationComputeShader.SetConstantBuffer(_FlockForcesConstantBufferId, _ForcesBuffer, 0, sizeof(float) * 6);
        }

        #endregion Private Methods
    }
}//McFlockSystem
