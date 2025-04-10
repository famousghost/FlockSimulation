namespace McFlockSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class SphereCubeCollision : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private Obstacle _Obstacle;
        [SerializeField] private float _MaxAngle;
        [SerializeField] private float _MaxRayLength;
        [SerializeField] private GameObject _Sphere;
        [SerializeField] private GameObject _Cube;
        #endregion Inspector Variables

        #region Unity Methods
        private void Update()
        {
            if(SphereCollision(_Sphere.transform, _Cube.transform))
            {
                Debug.Log("Collides");
            }
        }

        private void OnDrawGizmos()
        {
            var boxHit = BoxRaycast(_Sphere.transform, Vector3.forward * 5.0f, _Obstacle);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_Sphere.transform.position, _Sphere.transform.forward * 5.0f);
            Gizmos.DrawSphere(boxHit.hitPoint, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_ClosesPoint, 0.1f);
            Gizmos.DrawLine(_Sphere.transform.position, _ClosesPoint);
        }
        #endregion Unity Methods

        #region Private Variables
        private Vector3 _ClosesPoint;
        #endregion Private Variables

        #region Private Methods

        public bool SphereCollision(Transform sphere, Transform cube)
        {
            Vector3 minVert = GetMinVertex(cube);
            Vector3 maxVert = GetMaxVertex(cube);
            float x = FindClosestToZero(sphere.position.x, minVert.x, maxVert.x);
            float y = FindClosestToZero(sphere.position.y, minVert.y, maxVert.y);
            float z = FindClosestToZero(sphere.position.z, minVert.z, maxVert.z);
            _ClosesPoint = new Vector3(x, y, z);
            Vector3 sphereToClosestPoint = _ClosesPoint - sphere.position;
            float radius = sphere.localScale.x * 0.5f;
            return Vector3.Dot(sphereToClosestPoint, sphereToClosestPoint) <= (radius * radius);
        }

        private Vector3 GetMaxVertex(Transform cube)
        {
            Vector3 pos = cube.position;
            Vector3 localScale = cube.localScale;
            return pos + localScale * 0.5f;

        }

        private Vector3 GetMinVertex(Transform cube)
        {
            Vector3 pos = cube.position;
            Vector3 localScale = cube.localScale;
            return pos - localScale * 0.5f;
        }

        private float FindClosestToZero(float pos, float minVert, float maxVert)
        {
            if(pos >= minVert && pos <= maxVert)
            {
                return pos;
            }
            if(pos <= minVert)
            {
                return minVert;
            }
            return maxVert;
        }
        #endregion Private Methods

        #region Debug Methods

        public struct BoxHit
        {
            public Vector3 hitPoint;
            public float depth;
            public bool hit;
        }


        BoxHit BoxRaycast(Transform boid, Vector4 avoidancePoint, Obstacle obstacle)
        {
            BoxHit boxHit;
            boxHit.hit = false;
            boxHit.hitPoint = new Vector3(0.0f, 0.0f, 0.0f);
            boxHit.depth = 0.0f;

            Vector3 avoidancePointWS = boid.transform.TransformPoint(avoidancePoint);

            Vector3 worldDir = boid.transform.forward;//(avoidancePointWS - boid.transform.position).normalized

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
}
