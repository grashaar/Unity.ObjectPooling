using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ObjectPooling
{
    public sealed class GameObjectPooler : MonoBehaviour, IKeyedPool<GameObject>
    {
        [SerializeField]
        private Transform poolRoot = null;

        [SerializeField, Tooltip("Disable warning logs")]
        private bool silent = false;

        [Space]
        [SerializeField]
        private List<PoolItem> items = new List<PoolItem>();

        public bool Silent
        {
            get => this.silent;
            set => this.silent = value;
        }

        public IReadOnlyList<PoolItem> Items => this.items;

        private readonly ItemMap itemMap = new ItemMap();
        private readonly GameObjectListMap listMap = new GameObjectListMap();
        private readonly List<PoolItem> prepoolList = new List<PoolItem>();

        private void Awake()
        {
            this.prepoolList.AddRange(this.items);
        }

        private Transform GetPoolRoot()
        {
            if (!this.poolRoot)
                this.poolRoot = this.transform;

            return this.poolRoot;
        }

        public void Register(PoolItem item)
        {
            if (item == null)
                return;

            var index = this.items.FindIndex(x => string.Equals(x.Key, item.Key));

            if (index < 0)
                this.items.Add(item);

            index = this.prepoolList.FindIndex(x => string.Equals(x.Key, item.Key));

            if (index < 0)
                this.prepoolList.Add(item);
        }

        public void Deregister(PoolItem item)
        {
            if (item == null)
                return;

            var index = this.items.FindIndex(x => string.Equals(x.Key, item.Key));

            if (index >= 0)
                this.items.RemoveAt(index);

            index = this.prepoolList.FindIndex(x => string.Equals(x.Key, item.Key));

            if (index >= 0)
                this.prepoolList.RemoveAt(index);
        }

        public void Deregister(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            var index = this.items.FindIndex(x => string.Equals(x.Key, key));

            if (index >= 0)
                this.items.RemoveAt(index);

            index = this.prepoolList.FindIndex(x => string.Equals(x.Key, key));

            if (index >= 0)
                this.prepoolList.RemoveAt(index);
        }

        public void DeregisterAll()
        {
            this.items.Clear();
            this.prepoolList.Clear();
        }

        public void Deregister<TReleaseHandler>(PoolItem item, TReleaseHandler releaseHandler)
            where TReleaseHandler : IKeyedReleaseHandler
        {
            if (item == null)
                return;

            var index = -1;

            for (var i = 0; i < this.items.Count; i++)
            {
                var x = this.items[i];

                if (x == null)
                    continue;

                if (!string.Equals(item.Key, x.Key))
                    continue;

                index = i;
                releaseHandler?.Release(x.Key, x.Object);
            }

            if (index >= 0)
                this.items.RemoveAt(index);

            index = this.prepoolList.FindIndex(x => x != null && string.Equals(x.Key, item.Key));

            if (index >= 0)
                this.prepoolList.RemoveAt(index);
        }

        public void Deregister<TReleaseHandler>(string key, TReleaseHandler releaseHandler)
            where TReleaseHandler : IKeyedReleaseHandler
        {
            if (string.IsNullOrEmpty(key))
                return;

            var index = -1;

            for (var i = 0; i < this.items.Count; i++)
            {
                var x = this.items[i];

                if (x == null)
                    continue;

                if (!string.Equals(key, x.Key))
                    continue;

                index = i;
                releaseHandler?.Release(key, x.Object);
            }

            if (index >= 0)
                this.items.RemoveAt(index);

            index = this.prepoolList.FindIndex(x => x != null && string.Equals(x.Key, key));

            if (index >= 0)
                this.prepoolList.RemoveAt(index);
        }

        public void DeregisterAll<TReleaseHandler>(TReleaseHandler releaseHandler)
            where TReleaseHandler : IKeyedReleaseHandler
        {
            for (var i = 0; i < this.items.Count; i++)
            {
                var item = this.items[i];

                if (item == null)
                    continue;

                releaseHandler?.Release(item.Key, item.Object);
            }

            this.items.Clear();
            this.prepoolList.Clear();
        }

        public void PrepareItemMap()
        {
            this.itemMap.Clear();

            for (var i = 0; i < this.items.Count; i++)
            {
                if (this.items[i] == null)
                    continue;

                var item = this.items[i];

                if (string.IsNullOrEmpty(item.Key))
                {
                    if (!this.silent)
                        Debug.LogWarning($"Pool key at index={i} is empty", this);

                    continue;
                }

                if (this.itemMap.ContainsKey(item.Key))
                {
                    if (!this.silent)
                        Debug.LogWarning($"Pool key={item.Key} has already been existing", this);

                    continue;
                }

                this.itemMap.Add(item.Key, item);
            }
        }

        public void PrepoolItems()
        {
            this.prepoolList.Clear();
            this.prepoolList.AddRange(this.items);

            Prepool();
        }

        public void Prepool()
        {
            if (this.prepoolList.Count <= 0)
                return;

            var pool = Pool.Provider.Pool<GameObjectList>();

            for (var i = 0; i < this.prepoolList.Count; i++)
            {
                if (!ValidateItemAt(i))
                    continue;

                var item = this.prepoolList[i];

                if (!this.itemMap.ContainsKey(item.Key))
                    this.itemMap.Add(item.Key, item);

                var list =  pool.Get();

                for (var k = 0; k < item.PrepoolAmount; k++)
                {
                    var obj = Instantiate(item, k);

                    if (obj)
                        list.Add(obj);
                }

                this.listMap.Add(item.Key, list);
            }

            this.prepoolList.Clear();
        }

        public void ReturnAll()
        {
            var keys = Pool.Provider.List<string>();
            keys.AddRange(this.listMap.Keys);

            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];

                if (!this.listMap.TryGetValue(key, out var list))
                    continue;

                for (var k = 0; k < list.Count; k++)
                {
                    if (list[k] && list[k].activeSelf)
                        list[k].SetActive(false);
                }
            }

            Pool.Provider.Return(keys);
        }

        public GameObject Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                if (!this.silent)
                    Debug.LogWarning("Key is empty", this);

                return null;
            }

            var existed = this.listMap.TryGetValue(key, out var list);

            if (existed)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var item = list[i];

                    if (item && !item.activeInHierarchy)
                        return item;
                }
            }

            if (!this.itemMap.TryGetValue(key, out var poolItem))
            {
                if (!this.silent)
                    Debug.LogWarning($"Key={key} is not defined", this);

                return null;
            }

            if (!GetPoolRoot())
            {
                if (!this.silent)
                    Debug.LogWarning("Pool root is null", this);

                return null;
            }

            if (!existed)
            {
                list = new GameObjectList();
                this.listMap.Add(key, list);
            }

            var obj = Instantiate(poolItem, list.Count);

            if (obj)
                list.Add(obj);

            return obj;
        }

        private GameObject Instantiate(PoolItem item, int number)
        {
            if (!item.Object)
            {
                Debug.LogError($"Cannot instantiate null object of key={item.Key}", this);
                return null;
            }

            var obj = Instantiate(item.Object);
            obj.name = $"{item.Key}-{number}";
            obj.transform.SetParent(GetPoolRoot(), true);
            obj.SetActive(false);

            return obj;
        }

        private bool ValidateItemAt(int i)
        {
            if (this.items[i] == null)
                return false;

            var item = this.items[i];

            if (string.IsNullOrEmpty(item.Key))
            {
                if (!this.silent)
                    Debug.LogWarning($"Pool key at index={i} is empty", this);

                return false;
            }

            if (this.listMap.ContainsKey(item.Key))
            {
                if (!this.silent)
                    Debug.LogWarning($"Pool key={item.Key} has already been existing", this);

                return false;
            }

            return true;
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

        public void Return<U>(U items) where U : IEnumerable<GameObject>
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
            var pool = Pool.Provider.Pool<GameObjectList>();
            var keys = Pool.Provider.List<string>();
            keys.AddRange(this.listMap.Keys);

            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];

                if (!this.listMap.TryGetValue(key, out var list))
                    continue;

                for (var j = list.Count - 1; j >= 0; j--)
                {
                    Destroy(list[j]);
                }

                this.listMap.Remove(key);
                pool.Return(list);
            }

            Pool.Provider.Return(keys);
        }

        [Serializable]
        public class PoolItem
        {
            [SerializeField]
            private string key = string.Empty;

            public GameObject Object;

            [SerializeField, Min(0)]
            private int prepoolAmount;

            public string Key
            {
                get => this.key;
                set => this.key = value ?? string.Empty;
            }

            public int PrepoolAmount
            {
                get => this.prepoolAmount;
                set => this.prepoolAmount = Mathf.Max(value, 0);
            }
        }

        [Serializable]
        private class ItemMap : Dictionary<string, PoolItem> { }

        [Serializable]
        private class GameObjectListMap : Dictionary<string, GameObjectList> { }

        [Serializable]
        private class GameObjectList : List<GameObject> { }
    }
}