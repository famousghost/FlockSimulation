namespace McFlockSystem
{
    using System.Collections.Generic;
    using TMPro;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class Boid : MonoBehaviour
    {
        #region Inspector Variables
        [Header("Debug")]
        [SerializeField] private bool _EnableDebug;

        [Header("Wall Rays Setup")]
        [SerializeField] private int _FlockRadius;
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
        public Vector3 Velocity => _Veclocity;
        #endregion Public Variables

        #region Public Methods
        public void UpdateBoid()
        {
            _Veclocity += _Acceleration * Time.deltaTime;
            _Veclocity = (_Veclocity.magnitude > _MaxVelocity) ? (_Veclocity.normalized * _MaxVelocity) : _Veclocity;
            transform.position += _Veclocity * Time.deltaTime;
            transform.forward = _Veclocity.normalized;
        }

        public void AvoidWalls(float force)
        {
            _HitPoints.Clear();
            _Veclocity += AvoidWallVector();
            _Veclocity *= _MaxVelocity;
        }

        public void UpdateAccelaration(Vector3 accelaration)
        {
            _Acceleration += accelaration * Time.deltaTime;
        }

        public bool Aligment(ref Vector3 accelartion, Boid boid)
        {
            Vector3 velocity = Vector3.zero;
            if((boid.transform.position - transform.position).magnitude > _FlockRadius)
            {
                return false;
            }
            velocity += boid.Velocity;
            accelartion += velocity;
            return true;
        }

        public bool Cohesion(ref Vector3 position, Boid boid)
        {
            if ((boid.transform.position - transform.position).magnitude > _FlockRadius)
            {
                return false;
            }
            position += boid.transform.position;
            return true;
        }

        public bool Separation(ref Vector3 dir, Boid boid)
        {
            Vector3 dirToBoid = (transform.position - boid.transform.position);
            if (dirToBoid.magnitude > _FlockRadius)
            {
                return false;
            }

            dir += dirToBoid;
            return true;
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

        private Vector3 AvoidWallVector()
        {
            Vector3 force = Vector3.zero;
            for(int i = 0; i < _AvoidanceRays.Count; ++i)
            {
                RaycastHit hit;
                Vector3 dir = transform.TransformDirection(_AvoidanceRays[i]);
                Ray ray = new Ray(transform.position, dir.normalized);
                if (Physics.Raycast(ray, out hit, _MaxRayLength))
                {
                    force += hit.normal / (hit.distance * hit.distance + 1.0f);
                }
            }
            return force;
        }

        #endregion Private Methods
    }
}//McFlockSystem