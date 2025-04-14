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
        #region Inspector Variables
        [SerializeField] private bool _UpdateEachFrame;
        #endregion Inspector Variables

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

        private void Awake()
        {
            Position = transform.position;
            Size = new Vector4(transform.localScale.x, transform.localScale.y, transform.localScale.z, FlockArea ? 1.0f : 0.0f);
            Rotation.SetRow(0, transform.right);
            Rotation.SetRow(1, transform.up);
            Rotation.SetRow(2, transform.forward);
        }

        private void Update()
        {
            if (!_UpdateEachFrame)
            {
                return;
            }
            Position = transform.position;
            Size = new Vector4(transform.localScale.x, transform.localScale.y, transform.localScale.z, FlockArea ? 1.0f : 0.0f);
            Rotation.SetRow(0, transform.right);
            Rotation.SetRow(1, transform.up);
            Rotation.SetRow(2, transform.forward);
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
