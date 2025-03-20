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

        #endregion Public Variables

        #region Public Methods

        #endregion Public Methods

        #region Unity Methods
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
