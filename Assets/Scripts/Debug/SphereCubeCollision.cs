namespace McFlockSystem
{
    using UnityEngine;

    public sealed class SphereCubeCollision : MonoBehaviour
    {
        #region Inspector Variables
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
    }
}
