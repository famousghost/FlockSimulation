namespace McFlockSystem
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.Rendering;

    public enum CalculationTypes
    {
        CPU = 0,
        GPU = 1,
        GPU_ASYNC = 2,
        FULL_GPU = 3
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
        [SerializeField] private float _FlockRadius;
        [SerializeField] private float _MaxVelocity;
        [SerializeField] private bool _Initialized;

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
            Boids = new List<Boid>();
            Obstacles = new List<Obstacle>();
        }

        private void Start()
        {
            _FlockShaderKernelIndex = _FlockSimulationComputeShader.FindKernel(_FlockShaderKernelName);
            _FlockSimulationComputeShader.GetKernelThreadGroupSizes(_FlockShaderKernelIndex, out _KernelThreadSizeX, out _, out _);
            InitializeBuffers();
        }

        private void OnValidate()
        {
            _CalculationTypes = GetComponent<SpawnFlock>().CalculationTypes;
            PrepareAvoidancePointsBuffer();
            PrepareFlockDataBuffer();
            _FlockSimulationComputeShader.SetInt(_AvoidancePointsAmountId, _AvoidancePoints.Count);
            _FlockSimulationComputeShader.SetBuffer(_FlockShaderKernelIndex, _AvoidancePointsBufferId, _AvoidancePointsBuffer);
            _FlockSimulationComputeShader.SetConstantBuffer(_FlockForcesConstantBufferId, _ForcesBuffer, 0, sizeof(float) * 6);
            if (!PrepareObstaclesBuffer())
            {
                return;
            }
        }

        private void Update()
        {
            if (_CalculationTypes == CalculationTypes.CPU)
            {
                FlockSimulationCPU();
            }
            if(_CalculationTypes == CalculationTypes.GPU || _CalculationTypes == CalculationTypes.GPU_ASYNC || _CalculationTypes == CalculationTypes.FULL_GPU)
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
            /*List<Vector3> points = new List<Vector3>();
            foreach(var boid in Boids)
            {
                points = WallAvoidance(boid);
                Gizmos.color = Color.yellow;
                foreach (var point in points)
                {
                    Gizmos.DrawSphere(point, 1.0f);
                }
            }
            foreach (var boid in Boids)
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
        private CalculationTypes _CalculationTypes;

        //TODO: Remove this debug buffer
        private ComputeBuffer _BoidsBufferTestDebug;
        private ComputeBuffer _BoidsBuffer;
        private ComputeBuffer _AvoidancePointsBuffer;
        private ComputeBuffer _ObstaclesBuffer;
        private ComputeBuffer _ForcesBuffer;

        private List<Vector4> _AvoidancePoints;
        private BoidsStructureBuffer[] _BoidsBufferList;
        private uint _KernelThreadSizeX;

        private int _FlockShaderKernelIndex;

        private readonly int _BoidsBufferId = Shader.PropertyToID("_Boids");
        private readonly int _ObstalceBufferId = Shader.PropertyToID("_Obstacles");
        private readonly int _AvoidancePointsBufferId = Shader.PropertyToID("_AvoidancePoints");
        private readonly int _FlockForcesConstantBufferId = Shader.PropertyToID("FlockData");
        private readonly int _BoidsAmountId = Shader.PropertyToID("_BoidsAmount");
        private readonly int _ObstacleAmountId = Shader.PropertyToID("_ObstacleAmount");
        private readonly int _AvoidancePointsAmountId = Shader.PropertyToID("_AvoidancePointAmount");
        private readonly int _DeltaTimeId = Shader.PropertyToID("_DeltaTime");

        private readonly string _FlockShaderKernelName = "FlockSimulation";
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
            _FlockSimulationComputeShader.SetFloat(_DeltaTimeId, Time.deltaTime);
            _FlockSimulationComputeShader.Dispatch(_FlockShaderKernelIndex, Boids.Count / (int)_KernelThreadSizeX, 1, 1);
            if(_CalculationTypes == CalculationTypes.FULL_GPU)
            {
                return;
            }
            if (_CalculationTypes == CalculationTypes.GPU)
            {
                _BoidsBuffer.GetData(_BoidsBufferList);

                UpdateBoidsGPU();
            }
            else if (_CalculationTypes == CalculationTypes.GPU_ASYNC)
            {
                var asyncRequest = AsyncGPUReadback.Request(_BoidsBuffer,
                    (AsyncGPUReadbackRequest request) =>
                    {
                        if (Boids == null && Boids.Count == 0)
                        {
                            return;
                        }
                        request.GetData<BoidsStructureBuffer>().CopyTo(_BoidsBufferList);

                        UpdateBoidsGPU();
                    }
                );
                asyncRequest.Update();
            }
        }

        private void UpdateBoidsGPU()
        {
            int i = 0;
            foreach (var boid in Boids)
            {
                if(boid == null)
                {
                    continue;
                }
                var acceleration = _BoidsBufferList[i].Acceleration;
                Vector3 position = _BoidsBufferList[i].WorldPosition;
                Vector3 forward = _BoidsBufferList[i].Velocity.normalized;
                boid.UpdateBoidGPU(position, forward);
                ++i;
            }
        }

        private void PrepareBoidsBuffer()
        {
            if(_Initialized)
            {
                return;
            }
            _Initialized = true;
            if (_BoidsBufferList == null)
            {
                _BoidsBufferList = new BoidsStructureBuffer[Boids.Count];
                for (int i = 0; i < Boids.Count; ++i)
                {
                    _BoidsBufferList[i] = new BoidsStructureBuffer();
                }
            }
            for (int i = 0; i < Boids.Count; ++i)
            {
                SetupBoidBuffer(i);
                if (_CalculationTypes == CalculationTypes.FULL_GPU)
                {
                    Boids[i].transform.position = Vector3.zero;
                    Boids[i].transform.forward = Vector3.forward;
                }
            }
            _BoidsBuffer.SetData(_BoidsBufferList);
        }

        private void SetupBoidBuffer(int index)
        {
            var boid = Boids[index];
            _BoidsBufferList[index].WorldPosition.Set(boid.Position.x, boid.Position.y, boid.Position.z, 1.0f);
            _BoidsBufferList[index].Size.Set(boid.Size.x, boid.Size.y, boid.Size.z, 1.0f);
            _BoidsBufferList[index].Velocity.Set(boid.Velocity.x, boid.Velocity.y, boid.Velocity.z, 1.0f);
            _BoidsBufferList[index].Acceleration.Set(boid.Acceleration.x, boid.Acceleration.y, boid.Acceleration.z, 1.0f);
            _BoidsBufferList[index].LocalToWorld = boid.Transform.localToWorldMatrix;
        }

        private bool PrepareObstaclesBuffer()
        {
            if(Obstacles == null || Obstacles.Count == 0)
            {
                return false;
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
            return true;
        }

        private void PrepareAvoidancePointsBuffer()
        {
            _AvoidancePoints = new List<Vector4>();
            GenerateAvoidancePoints();
            _AvoidancePointsBuffer = new ComputeBuffer(_AvoidancePoints.Count, sizeof(float) * 4, ComputeBufferType.Structured);
            _AvoidancePointsBuffer.SetData(_AvoidancePoints.ToArray());
        }

        private void PrepareFlockDataBuffer()
        {
            _ForcesBuffer = new ComputeBuffer(8, sizeof(float), ComputeBufferType.Constant);
            float[] forceData = { _CohesionStrength, _SeparationStrength, _AligmentStrength, _WallAvoidanceStrength, _MaxAngle, _MaxRayLength, _FlockRadius, _MaxVelocity};
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
                    if (Mathf.Acos(Vector3.Dot(point, Vector3.forward)) <= _MaxAngle * Mathf.Deg2Rad)
                    {
                        _AvoidancePoints.Add(new Vector4(point.x, point.y, point.z, 1.0f));
                    }
                }
            }
        }

        private void InitializeBuffers()
        {
            if (Boids != null && Boids.Count != 0)
            {
                _BoidsBuffer = new ComputeBuffer(Boids.Count, Marshal.SizeOf<BoidsStructureBuffer>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
                _BoidsBufferTestDebug = new ComputeBuffer(Boids.Count, sizeof(float), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
                _FlockSimulationComputeShader.SetInt(_BoidsAmountId, Boids.Count);
                _FlockSimulationComputeShader.SetBuffer(_FlockShaderKernelIndex, _BoidsBufferId, _BoidsBuffer);
                var shaderMaterial = GetComponent<SpawnFlock>().SharedMaterial;
               
                shaderMaterial.SetBuffer(_BoidsBufferId, _BoidsBuffer);

            }
            PrepareAvoidancePointsBuffer();
            PrepareFlockDataBuffer();
            _FlockSimulationComputeShader.SetInt(_AvoidancePointsAmountId, _AvoidancePoints.Count);
            _FlockSimulationComputeShader.SetBuffer(_FlockShaderKernelIndex, _AvoidancePointsBufferId, _AvoidancePointsBuffer);
            _FlockSimulationComputeShader.SetConstantBuffer(_FlockForcesConstantBufferId, _ForcesBuffer, 0, sizeof(float) * 6);
            if (!PrepareObstaclesBuffer())
            {
                return;
            }
            _FlockSimulationComputeShader.SetInt(_ObstacleAmountId, Obstacles.Count);
            _FlockSimulationComputeShader.SetBuffer(_FlockShaderKernelIndex, _ObstalceBufferId, _ObstaclesBuffer);
        }

        #endregion Private Methods

        #region Debug Methods
        public struct BoxHit
        {
            public Vector3 hitPoint;
            public float depth;
            public bool hit;
        }

        List<Vector3> WallAvoidance(Boid boid)
        {
            List<Vector3> hitPoints = new List<Vector3>();
            for (int i = 0; i < _AvoidancePoints.Count; ++i)
            {
                for (int j = 0; j < Obstacles.Count; ++j)
                {
                    BoxHit boxHit = BoxRaycast(boid, _AvoidancePoints[i], Obstacles[j]);
                    if (boxHit.hit)
                    {
                        hitPoints.Add(boxHit.hitPoint);
                    }
                }
            }
            return hitPoints;
        }

        BoxHit BoxRaycast(Boid boid, Vector4 avoidancePoint, Obstacle obstacle)
        {
            BoxHit boxHit;
            boxHit.hit = false;
            boxHit.hitPoint = new Vector3(0.0f, 0.0f, 0.0f);
            boxHit.depth = 0.0f;

            Vector3 avoidancePointWS = boid.transform.TransformPoint(avoidancePoint);

            Vector3 worldDir = (avoidancePointWS - boid.transform.position).normalized;

            
            if (Mathf.Acos(Vector3.Dot(worldDir, boid.transform.forward)) >= _MaxAngle * Mathf.Deg2Rad)
            {
                return boxHit;
            }

            Vector3 p = (Vector3)obstacle.Position - boid.transform.position;

            float x = Vector3.Dot((Vector3)obstacle.Rotation.GetRow(0), p);
            float y = Vector3.Dot((Vector3)obstacle.Rotation.GetRow(1), p);
            float z = Vector3.Dot((Vector3)obstacle.Rotation.GetRow(2), p);

            float rdX = Vector3.Dot((Vector3)obstacle.Rotation.GetRow(0), worldDir);
            float rdY = Vector3.Dot((Vector3)obstacle.Rotation.GetRow(1), worldDir);
            float rdZ = Vector3.Dot((Vector3)obstacle.Rotation.GetRow(2), worldDir);

            Vector3 size = obstacle.Size * 0.5f;

            Vector3 minOBB = new Vector3(x - size.x, y - size.y, z - size.z);
            Vector3 maxOBB = new Vector3(x + size.x, y + size.y, z + size.z);

            Vector3 newRay = new Vector3(rdX, rdY, rdZ);

            Vector3 pMin = VectorDiv(minOBB, newRay);
            Vector3 pMax = VectorDiv(maxOBB, newRay);

            Vector3 tMin = new Vector3(Mathf.Min(pMin.x, pMax.x), Mathf.Min(pMin.y, pMax.y), Mathf.Min(pMin.z, pMax.z));
            Vector3 tMax = new Vector3(Mathf.Max(pMin.x, pMax.x), Mathf.Max(pMin.y, pMax.y), Mathf.Max(pMin.z, pMax.z));

            float maxMin = Mathf.Max(Mathf.Max(tMin.x, tMin.y), tMin.z);

            float minMax = Mathf.Min(Mathf.Min(tMax.x, tMax.y), tMax.z);

            if (minMax <= 0.0f)
            {
                return boxHit;
            }

            if (minMax <= maxMin)
            {
                return boxHit;
            }
            if (maxMin <= 0.0f)
            {
                if (minMax > _MaxRayLength)
                {
                    return boxHit;
                }
                boxHit.hit = true;
                boxHit.hitPoint = boid.transform.position + worldDir * minMax;
                boxHit.depth = minMax;
                return boxHit;
            }

            if (maxMin > _MaxRayLength)
            {
                return boxHit;
            }
            boxHit.hit = true;
            boxHit.hitPoint = boid.transform.position + worldDir * maxMin;
            boxHit.depth = maxMin;
            return boxHit;
        }

        private Vector3 VectorDiv(Vector3 a, Vector3 b)
        {
            if (b.x == 0.0f || b.y == 0.0f || b.z == 0.0f)
            {
                return Vector3.zero;
            }
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        #endregion Debug Methods
    }
}//McFlockSystem
