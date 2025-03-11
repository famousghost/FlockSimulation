namespace McFlockSystem
{
    using UnityEngine;

    public sealed class FlockArea : MonoBehaviour
    {
        #region Inspector Variables
        [Range(0.0f, 0.5f)]
        [SerializeField] private float _MaxSize = 0.5f;
        #endregion Inspector Variables

        #region Public Methods
        public Vector3 GetMinVert()
        {
            Vector3 pos = transform.position;
            Vector3 localPos = transform.localScale;
            return pos - localPos * _MaxSize;
        }

        public Vector3 GetMaxVert()
        {
            Vector3 pos = transform.position;
            Vector3 localPos = transform.localScale;
            return pos + localPos * _MaxSize;
        }
        #endregion Public Methods
    }
}
