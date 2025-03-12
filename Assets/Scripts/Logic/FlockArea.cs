namespace McFlockSystem
{
    using UnityEngine;
    using UnityEngine.UIElements;

    public struct BoxHit
    {
        public BoxHit(bool _hit, Vector3 _point, float _distance)
        {
            hit = _hit;
            point = _point;
            distance = _distance;
        }
        public bool hit;
        public Vector3 point;
        public float distance;
    }

    public sealed class FlockArea : MonoBehaviour
    {
        #region Inspector Variables
        [Range(0.0f, 0.5f)]
        [SerializeField] private float _MaxSize = 0.5f;
        #endregion Inspector Variables

        #region Public Methods
        public Vector3 GetMinVert()
        {
            return GetMinVertex(_MaxSize);
        }

        public Vector3 GetMaxVert()
        {
            return GetMaxVertex(_MaxSize);
        }

        public BoxHit BoxRaycast(Ray ray, float maxLength)
        {
            Vector3 minVert = GetMinVertex(0.5f);
            Vector3 maxVert = GetMaxVertex(0.5f);

            Vector3 pMin = VectorDiv(minVert - ray.origin, ray.direction);
            Vector3 pMax = VectorDiv(maxVert - ray.origin, ray.direction);

            Vector3 tMin = new Vector3(Mathf.Min(pMin.x, pMax.x), Mathf.Min(pMin.y, pMax.y), Mathf.Min(pMin.z, pMax.z));
            Vector3 tMax = new Vector3(Mathf.Max(pMin.x, pMax.x), Mathf.Max(pMin.y, pMax.y), Mathf.Max(pMin.z, pMax.z));

            float maxMin = Mathf.Max(tMin.z, Mathf.Max(tMin.x, tMin.y));
            float minMax = Mathf.Min(tMax.z, Mathf.Min(tMax.x, tMax.y));

            if (minMax <= 0.0f)
            {
                return new BoxHit(false, Vector3.zero, 0.0f);
            }

            if (minMax <= maxMin)
            {
                return new BoxHit(false, Vector3.zero, 0.0f);
            }
            if (maxMin <= 0.0f)
            {
                if (minMax > maxLength)
                {
                    return new BoxHit(false, Vector3.zero, 0.0f);
                }
                return new BoxHit(true, ray.origin + ray.direction * minMax, minMax);
            }

            if (maxMin > maxLength)
            {
                return new BoxHit(false, Vector3.zero, 0.0f);
            }
            return new BoxHit(true, ray.origin + ray.direction * maxMin, maxMin);
        }


        private Vector3 GetMaxVertex(float size)
        {
            Vector3 pos = transform.position;
            Vector3 localScale = transform.localScale;
            return pos + localScale * size;

        }

        private Vector3 GetMinVertex(float size)
        {
            Vector3 pos = transform.position;
            Vector3 localScale = transform.localScale;
            return pos - localScale * size;
        }

        private Vector3 VectorDiv(Vector3 a, Vector3 b)
        {
            if(b.x == 0.0f || b.y == 0.0f || b.z == 0.0f)
            {
                return Vector3.zero;
            }
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        #endregion Public Methods
    }
}
