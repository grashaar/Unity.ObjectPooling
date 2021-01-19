using System;
using System.Collections.Generic;
using System.Collections.Pooling;

namespace UnityEngine.Pooling
{
    public sealed class GameObjectPoolerManager : MonoBehaviour, IKeyedPool<GameObject>
    {
        [SerializeField]
        private GameObject poolersRoot = null;

        [SerializeField]
        private bool initializeOnAwake = true;

        private readonly PoolerMap poolerMap = new PoolerMap();

        private void Awake()
        {
            if (this.initializeOnAwake)
                Initialize();
        }

        public void Initialize(bool silent = false)
        {
            if (!this.poolersRoot)
            {
                this.poolersRoot = this.gameObject;
            }

            var pools = this.poolersRoot.GetComponentsInChildren<GameObjectPooler>();

            for (var i = 0; i < pools.Length; i++)
            {
                var pool = pools[i];
                var items = pool.Items;

                for (var k = 0; k < items.Count; k++)
                {
                    if (items[k] == null)
                        continue;

                    var item = items[k];

                    if (string.IsNullOrEmpty(item.Key))
                    {
                        if (!silent)
                            Debug.LogWarning($"Pool key at index={k} is empty", pool);

                        continue;
                    }

                    if (this.poolerMap.ContainsKey(item.Key))
                    {
                        if (!silent)
                            Debug.LogWarning($"Pool key={item.Key} has already been existing", pool);

                        continue;
                    }

                    pool.PrepareItemMap();
                    this.poolerMap.Add(item.Key, pool);
                }
            }
        }

        public void Prepool()
        {
            foreach (var pool in this.poolerMap.Values)
            {
                if (!pool)
                    continue;

                pool.Prepool();
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

        public GameObject Get(string key)
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

            var obj = pooler.Get(key);

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

        public void DestroyAll()
        {
            foreach (var pool in this.poolerMap.Values)
            {
                pool.DestroyAll();
            }

            this.poolerMap.Clear();
        }

        [Serializable]
        private class PoolerMap : Dictionary<string, GameObjectPooler> { }
    }
}