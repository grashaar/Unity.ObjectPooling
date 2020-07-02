﻿using System;
using System.Collections.Generic;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine.AddressableAssets
{
    public sealed class AddressableGameObjectPoolerManager : MonoBehaviour, IAsyncPool<GameObject>
    {
#if UNITY_OBJECTPOOLING_ADDRESSABLES
        [SerializeField]
        private GameObject poolersRoot = null;

        [SerializeField]
        private bool initializeOnAwake = true;

        private readonly PoolerMap poolerMap = new PoolerMap();
        private bool isPrepooled = false;

        private void Awake()
        {
            if (this.initializeOnAwake)
                Initialize();
        }

        public void Initialize()
        {
            if (!this.poolersRoot)
            {
                this.poolersRoot = this.gameObject;
            }

            var pools = this.poolersRoot.GetComponentsInChildren<AddressableGameObjectPooler>();

            for (var i = 0; i < pools.Length; i++)
            {
                var pool = pools[i];
                var items = pool.Items;

                if (items == null)
                    continue;

                for (var k = 0; k < items.Count; k++)
                {
                    if (items[k] == null)
                        continue;

                    var item = items[k];

                    if (string.IsNullOrEmpty(item.Key))
                    {
                        Debug.LogWarning($"Pool key at index={k} is empty", pool);
                        continue;
                    }

                    if (this.poolerMap.ContainsKey(item.Key))
                    {
                        Debug.LogWarning($"Pool key={item.Key} has already been existing", pool);
                        continue;
                    }

                    pool.PrepareItemMap();
                    this.poolerMap.Add(item.Key, pool);
                }
            }
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask PrepoolAsync()
#else
        public async Task PrepoolAsync()
#endif
        {
            if (this.isPrepooled)
                return;

            foreach (var pool in this.poolerMap.Values)
            {
                if (!pool)
                    continue;

                await pool.PrepoolAsync();
            }

            this.isPrepooled = true;
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

        [Obsolete("This method has been deprecated. Use GetAsync instead.")]
        public GameObject Get(string key)
        {
            throw new NotImplementedException();
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask<GameObject> GetAsync(string key)
#else
        public async Task<GameObject> GetAsync(string key)
#endif
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Key is empty");
                return null;
            }

            if (!this.poolerMap.TryGetValue(key, out var pooler))
            {
                Debug.LogWarning($"Key={key} does not exist");
                return null;
            }

            var obj = await pooler.GetAsync(key);

            if (!obj)
            {
                Debug.LogWarning($"Cannot spawn {key}");
            }

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

        public void Deinitialize()
        {
            foreach (var pool in this.poolerMap.Values)
            {
                pool.DestroyAll();
            }

            this.poolerMap.Clear();
        }

        [Serializable]
        private class PoolerMap : Dictionary<string, AddressableGameObjectPooler> { }
#endif
    }
}