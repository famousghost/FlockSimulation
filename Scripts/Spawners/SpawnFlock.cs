namespace McFlockSystem
{
    using UnityEngine;

    public sealed class SpawnFlock : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private GameObject _Prefab;
        [SerializeField] private FlockArea _SpawnArea;
        [SerializeField] private int _BoidsCount;
        #endregion Inspector Variables

        #region Unity Methods
        void Start()
        {
            for (int i = 0; i < _BoidsCount; ++i)
            {
                Vector3 minVert = _SpawnArea.GetMinVert();
                Vector3 maxVert = _SpawnArea.GetMaxVert();
                Vector3 randomPos = new Vector3(Random.Range(minVert.x, maxVert.x), Random.Range(minVert.y, maxVert.y), Random.Range(minVert.z, maxVert.z));
                var obj = Instantiate(_Prefab, randomPos, Quaternion.identity);
                obj.transform.forward = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_SpawnArea.transform.position, _SpawnArea.transform.localScale);
        }
        #endregion Unity Methods

        #region Private Methods

        #endregion Private Methods
    }
}//McFlockSystem
