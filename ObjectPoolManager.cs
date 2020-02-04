using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Async;

namespace Unity.ObjectPooling
{
    public class ObjectPoolManager : MonoBehaviour, IPool<GameObject>
    {
        [SerializeField]
        private GameObject poolsRoot = null;

        private readonly ObjectPoolMap poolMap = new ObjectPoolMap();
        private bool isPrepooled = false;

        protected void Awake()
        {
            if (!this.poolsRoot)
            {
                this.poolsRoot = this.gameObject;
            }

            var pools = this.poolsRoot.GetComponentsInChildren<PoolController>();

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

                    if (this.poolMap.ContainsKey(item.Key))
                    {
                        Debug.LogWarning($"Pool key={item.Key} has already been existing", pool);
                        continue;
                    }

                    pool.Initialize();
                    this.poolMap.Add(item.Key, pool);
                }
            }
        }

        public async UniTask Prepool()
        {
            if (this.isPrepooled)
                return;

            foreach (var pool in this.poolMap.Values)
            {
                if (!pool)
                    continue;

                await pool.Prepool();
            }

            this.isPrepooled = true;
        }

        public void ReturnAll()
        {
            foreach (var pool in this.poolMap.Values)
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

            if (!this.poolMap.ContainsKey(key))
            {
                Debug.LogWarning($"Key={key} does not exist");
                return null;
            }

            var obj = this.poolMap[key].Get(key);

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

        [Serializable]
        private class ObjectPoolMap : Dictionary<string, PoolController> { }
    }
}