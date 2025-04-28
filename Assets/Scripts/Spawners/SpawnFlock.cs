namespace McFlockSystem
{
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;

    public sealed class SpawnFlock : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private CalculationTypes _CalculateTypes;
        [SerializeField] private GameObject _Prefab_CPU_GPU_SYNC;
        [SerializeField] private GameObject _Prefab_FULL_GPU;
        [SerializeField] private FlockArea _SpawnArea;
        [SerializeField] private int _BoidsCount;
        #endregion Inspector Variables

        #region Public Variables
        public CalculationTypes CalculationTypes => _CalculateTypes;
        public Material SharedMaterial;
        #endregion Public Variables

        #region Unity Methods
        void Start()
        {
            _MaterialPropertyBlock = new MaterialPropertyBlock();
            for (int i = 0; i < _BoidsCount; ++i)
            {
                Vector3 minVert = _SpawnArea.GetMinVert();
                Vector3 maxVert = _SpawnArea.GetMaxVert();
                Vector3 randomPos = new Vector3(Random.Range(minVert.x, maxVert.x), Random.Range(minVert.y, maxVert.y), Random.Range(minVert.z, maxVert.z));
                var obj = Instantiate((_CalculateTypes == CalculationTypes.FULL_GPU) ? _Prefab_FULL_GPU : _Prefab_CPU_GPU_SYNC, randomPos, Quaternion.identity);
                obj.GetComponent<MeshFilter>().sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000.0f);

                _MaterialPropertyBlock.SetInt(_IndexID, i);
                SharedMaterial = obj.GetComponent<MeshRenderer>().sharedMaterial;
                obj.GetComponent<MeshRenderer>().SetPropertyBlock(_MaterialPropertyBlock);

                obj.transform.position = randomPos;
                obj.transform.forward = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_SpawnArea.transform.position, _SpawnArea.transform.localScale);
        }
        #endregion Unity Methods

        #region Private Variables

        private readonly int _IndexID = Shader.PropertyToID("_Index");

        #endregion Private Variables

        #region Private Methods
        private MaterialPropertyBlock _MaterialPropertyBlock;
        #endregion Private Methods
    }
}//McFlockSystem
