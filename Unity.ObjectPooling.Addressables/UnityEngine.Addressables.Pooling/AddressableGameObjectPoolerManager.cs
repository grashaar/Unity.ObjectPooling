#if UNITY_OBJECTPOOLING_ADDRESSABLES

using System;
using System.Collections.Generic;
using System.Collections.Pooling;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine.AddressableAssets.Pooling
{
    public sealed partial class AddressableGameObjectPoolerManager : MonoBehaviour, IAsyncKeyedPool<GameObject>
    {
        [SerializeField]
        private GameObject poolersRoot = null;

        [SerializeField]
        private bool initializeOnAwake = true;

        [SerializeField]
        private bool silent = false;

        private readonly PoolerMap poolerMap = new PoolerMap();

        private void Awake()
        {
            if (this.initializeOnAwake)
                Initialize();
        }
        /// <summary>
        /// Initialize the pooler manager, this will create all poolers and initialize them.
        /// </summary>
        /// <param name="silent"></param>
        public void Initialize(bool silent = false)
        {
            this.silent = silent;

            if (!this.poolersRoot)
            {
                this.poolersRoot = this.gameObject;
            }

            var poolers = this.poolersRoot.GetComponentsInChildren<AddressableGameObjectPooler>();

            for (var i = 0; i < poolers.Length; i++)
            {
                var pooler = poolers[i];

                if (!pooler)
                    continue;

                pooler.Silent = this.silent;

                var items = pooler.Items;

                for (var k = 0; k < items.Count; k++)
                {
                    var item = items[k];

                    if (item == null)
                        continue;

                    if (string.IsNullOrEmpty(item.Key))
                    {
                        if (!this.silent)
                            Debug.LogWarning($"Pooler key at index={k} is empty", pooler);

                        continue;
                    }

                    if (this.poolerMap.ContainsKey(item.Key))
                    {
                        if (!this.silent)
                            Debug.LogWarning($"Pooler key={item.Key} has already been existing", pooler);

                        continue;
                    }

                    this.poolerMap.Add(item.Key, pooler);
                }

                pooler.PrepareItemMap();
            }
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask PrepoolAsync()
#else
        public async Task PrepoolAsync()
#endif
        {
            foreach (var pool in this.poolerMap.Values)
            {
                if (!pool)
                    continue;

                await pool.PrepoolAsync();
            }
        }

        public void ReturnAll()
        {
            foreach (var pool in this.poolerMap.Values)
            {
                if (!pool)
                    continue;

                pool.ReturnAll();
            }
        }


#if !UNITY_OBJECTPOOLING_ADDRESSABLES_1_17
        [Obsolete("Install Addressables 1.17 or higher, or use GetAsync instead.")]
        public GameObject Get(string key)
        {
            throw new NotImplementedException();
        }
#endif

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask<GameObject> GetAsync(string key)
#else
        public async Task<GameObject> GetAsync(string key)
#endif
        {
            if (string.IsNullOrEmpty(key))
            {
                if (!this.silent)
                    Debug.LogWarning("Key is empty");

                return null;
            }

            if (!this.poolerMap.TryGetValue(key, out var pooler))
            {
                if (!this.silent)
                    Debug.LogWarning($"Key={key} does not exist");

                return null;
            }

            var obj = await pooler.GetAsync(key);

            if (!obj && !this.silent)
                Debug.LogWarning($"Cannot spawn {key}");

            return obj;
        }
#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask<GameObject> GetAsync(string key, Vector3 position, Quaternion rotation, Transform parent = null)
#else
        public async Task<GameObject> GetAsync(string key)
#endif
        {
            if (string.IsNullOrEmpty(key))
            {
                if (!this.silent)
                    Debug.LogWarning("Key is empty");

                return null;
            }

            if (!this.poolerMap.TryGetValue(key, out var pooler))
            {
                if (!this.silent)
                    Debug.LogWarning($"Key={key} does not exist");

                return null;
            }

            var obj = await pooler.GetAsync(key, position, rotation, parent);

            if (!obj && !this.silent)
                Debug.LogWarning($"Cannot spawn {key}");

            return obj;
        }
        public void Return(GameObject item)
        {
            if (item && item.activeSelf)
                item.SetActive(false);
        }

        public void Return(params GameObject[] items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                Return(item);
            }
        }

        public void Return(IEnumerable<GameObject> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                Return(item);
            }
        }
        /// <summary>
        /// Destroy all game objects in the poolers and release from memory.
        /// </summary>
        public void DestroyAll()
        {
            foreach (var pooler in this.poolerMap.Values)
            {
                pooler.DestroyAll();
            }
        }

        [Serializable]
        private class PoolerMap : Dictionary<string, AddressableGameObjectPooler> { }
    }
}

#endif