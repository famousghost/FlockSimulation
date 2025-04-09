namespace McFlockSystem
{
    using UnityEngine;

    public enum ObstacleType
    {
        SPHERE = 0,
        BOX = 1,
        CUSTOM = 2
    }

    public sealed class Obstacle : MonoBehaviour
    {
        #region Public Variables
        public ObstacleType Type;
        public Vector4 Position;
        public Vector4 Size;
        public Matrix4x4 Rotation;
        public bool FlockArea;

        #endregion Public Variables

        #region Public Methods

        #endregion Public Methods

        #region Unity Methods

        private void Update()
        {
            Position = transform.position;
            Size = new Vector4(transform.localScale.x, transform.localScale.y, transform.localScale.z, FlockArea ? 1.0f : 0.0f);
            Rotation = Matrix4x4.TRS(Vector3.zero, transform.localRotation, Vector3.one);
        }
        private void OnEnable()
        {
            if(Flock.Instance == null)
            {
                return;
            }
            Flock.Instance.AddObstacle(this);
        }

        private void OnDisable()
        {
            if(Flock.Instance == null)
            {
                return;
            }
            Flock.Instance.RemoveObstacle(this);
        }
        #endregion Unity Methods

        #region Private Variables

        #endregion Private Variables

        #region Private Methods

        #endregion Privtae Methods
    }
}//McFlockSystem
