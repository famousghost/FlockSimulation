namespace McFlockSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class Boid : MonoBehaviour
    {
        #region Inspector Variables
        [Header("Debug")]
        [SerializeField] private bool _EnableDebug;

        [Header("Wall Rays Setup")]
        [SerializeField] private float _FlockRadius;
        [SerializeField] private int _SpherePitchSize;
        [SerializeField] private int _SphereYawSize;
        [SerializeField] private float _DebugRadius;
        [SerializeField, Range(0.0f, 180.0f)] private float _MaxAngle;
        [SerializeField] private float _MaxRayLength;

        [Header("Boid Setup")]
        [SerializeField] private float _InitialVelocity;
        [SerializeField] private float _MaxVelocity;
        #endregion Inspector Variables

        #region Public Variables
        public Vector3 Position => _Transform.position;

        public Vector3 Froward => _Transform.forward;

        public Transform Transform => _Transform;

        public Vector3 Acceleration => _Acceleration;

        public Vector3 Velocity => _Veclocity;

        public float FlockRadius => _FlockRadius;

        public float MaxRayLength => _MaxRayLength;
        #endregion Public Variables

        #region Public Methods
        public void UpdateBoid()
        {
            _Veclocity += _Acceleration * Time.deltaTime * Time.deltaTime;
            _Veclocity = _Veclocity.normalized * _MaxVelocity;
            transform.position += _Veclocity * Time.deltaTime;
            transform.forward = _Veclocity.normalized;
        }

        public void AvoidWalls(FlockArea flockArea, float force)
        {
            _HitPoints.Clear();
            _Acceleration += AvoidWallVector(flockArea) * force;
        }

        public void UpdateAccelaration(Vector3 accelaration)
        {
            _Acceleration += accelaration;
        }

        public bool IsClosestBoid(Boid boid)
        {
            var dir = boid.transform.position - transform.position;
            if(Vector3.Dot(dir, dir) > (_FlockRadius * _FlockRadius))
            {
                return false;
            }
            return true;
        }

        public void Aligment(ref Vector3 accelartion, Boid boid)
        {
            accelartion += boid.Velocity;
        }

        public void Cohesion(ref Vector3 position, Boid boid)
        {
            position += boid.transform.position;
        }

        public void Separation(ref Vector3 dir, Boid boid)
        {
            Vector3 dirToBoid = transform.position - boid.transform.position;
            dir += dirToBoid;
        }

        #endregion Public Methods

        #region Unity Methods
        private void Awake()
        {
            _AvoidanceRays = new List<Vector3>();
            _HitPoints = new List<Vector3>();
        }

        private void OnEnable()
        {
            _Transform = transform;
            if (Flock.Instance != null)
            {
                Flock.Instance.AddBois(this);
            }
        }

        private void OnDisable()
        {
            if (Flock.Instance != null)
            {
                Flock.Instance.RemoveBoid(this);
            }
        }

        private void Start()
        {
            if (_AvoidanceRays == null)
            {
                _AvoidanceRays = new List<Vector3>();
            }
            _AvoidanceRays.Clear();
            Init();
            _Veclocity = transform.forward * _InitialVelocity;
        }

        private void OnValidate()
        {
            if (_AvoidanceRays == null)
            {
                _AvoidanceRays = new List<Vector3>();
            }
            _AvoidanceRays.Clear();
            Init();
        }

        private void OnDrawGizmos()
        {
            if(!_EnableDebug)
            {
                return;
            }
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1000.0f);
            if (_AvoidanceRays == null || _AvoidanceRays.Count == 0)
            {
                return;
            }
            Gizmos.color = Color.red;
            for (int i = 0; i < _AvoidanceRays.Count; ++i)
            {
                Gizmos.DrawLine(transform.position, transform.position + transform.localToWorldMatrix.MultiplyVector(_AvoidanceRays[i]) * _MaxRayLength);
            }
            if(_HitPoints == null)
            {
                return;
            }
            Gizmos.color = Color.blue;
            for(int i = 0; i < _HitPoints.Count; ++i)
            {
                Gizmos.DrawSphere(_HitPoints[i], 0.1f);
            }
        }
        #endregion Unity Methods

        #region Private Variables 
        private List<Vector3> _HitPoints;
        private List<Vector3> _AvoidanceRays;

        private Vector3 _Veclocity;
        private Vector3 _Acceleration;
        private Transform _Transform;
        #endregion Private Variables

        #region Private Methods
        private void Init()
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
                    Vector3 dir = (point - transform.worldToLocalMatrix.MultiplyPoint(transform.position)).normalized;
                    if (Mathf.Acos(Vector3.Dot(dir, transform.worldToLocalMatrix.MultiplyVector(transform.forward))) <= (_MaxAngle * Mathf.Deg2Rad))
                    {
                        _AvoidanceRays.Add(dir);
                    }
                }
            }
        }

        private Vector3 AvoidWallVector(FlockArea flockArea)
        {
            Vector3 force = Vector3.zero;
            for(int i = 0; i < _AvoidanceRays.Count; ++i)
            {
                RaycastHit hit;
                Vector3 dir = transform.TransformDirection(_AvoidanceRays[i]);
                Ray ray = new Ray(transform.position, dir.normalized);
                if (Physics.Raycast(ray, out hit, _MaxRayLength))
                {
                    if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Boid"))
                    {
                        continue;
                    }
                    if(_EnableDebug)
                    {
                        _HitPoints.Add(hit.point);
                    }
                    force = force + ((transform.position - hit.point).normalized / (hit.distance * hit.distance + 1.0f));
                }
                var areaHit = flockArea.BoxRaycast(ray, _MaxRayLength);
                if (areaHit.hit)
                {
                    if (_EnableDebug)
                    {
                        _HitPoints.Add(areaHit.point);
                    }
                    force = force + ((transform.position - areaHit.point).normalized / (areaHit.distance * areaHit.distance + 1.0f));
                }
            }
            return force;
        }

        #endregion Private Methods
    }
}//McFlockSystem