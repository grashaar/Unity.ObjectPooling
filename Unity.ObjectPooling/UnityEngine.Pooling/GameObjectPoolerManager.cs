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

        [SerializeField]
        private bool silent = false;

        private readonly PoolerMap poolerMap = new PoolerMap();

        private void Awake()
        {
            if (this.initializeOnAwake)
                Initialize();
        }

        public void Initialize(bool silent = false)
        {
            this.silent = silent;

            if (!this.poolersRoot)
            {
                this.poolersRoot = this.gameObject;
            }

            var poolers = this.poolersRoot.GetComponentsInChildren<GameObjectPooler>();

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

        public void Prepool()
        {
            foreach (var pooler in this.poolerMap.Values)
            {
                if (!pooler)
                    continue;

                pooler.Prepool();
            }
        }

        public void ReturnAll()
        {
            foreach (var pooler in this.poolerMap.Values)
            {
                if (!pooler)
                    continue;

                pooler.ReturnAll();
            }
        }

        public GameObject Get(string key)
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

            var obj = pooler.Get(key);

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

        public void DestroyAll()
        {
            foreach (var pooler in this.poolerMap.Values)
            {
                pooler.DestroyAll();
            }
        }

        [Serializable]
        private class PoolerMap : Dictionary<string, GameObjectPooler> { }
    }
}