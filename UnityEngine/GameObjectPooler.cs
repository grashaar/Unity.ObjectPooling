using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public sealed class GameObjectPooler : MonoBehaviour, IKeyedPool<GameObject>
    {
        [SerializeField]
        private Transform poolRoot = null;

        [Space]
        [SerializeField]
        private List<PoolItem> items = new List<PoolItem>();

        public ReadList<PoolItem> Items => this.items;

        private readonly ItemMap itemMap = new ItemMap();
        private readonly GameObjectListMap listMap = new GameObjectListMap();
        private readonly List<GameObjectList> lists = new List<GameObjectList>();
        private readonly List<PoolItem> prepoolList = new List<PoolItem>();

        private void Awake()
        {
            this.prepoolList.AddRange(this.items);
        }

        private Transform GetPoolRoot()
            => this.poolRoot ? this.poolRoot : this.transform;

        public void Register(PoolItem item)
        {
            if (item == null)
                return;

            var index = this.items.FindIndex(x => string.Equals(x.Key, item.Key));

            if (index < 0)
                this.items.Add(item);
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
                    Debug.LogWarning($"Pool key at index={i} is empty", this);
                    continue;
                }

                if (this.itemMap.ContainsKey(item.Key))
                {
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

            for (var i = 0; i < this.prepoolList.Count; i++)
            {
                if (!ValidateItemAt(i))
                    continue;

                var item = this.prepoolList[i];

                if (!this.itemMap.ContainsKey(item.Key))
                    this.itemMap.Add(item.Key, item);

                var list = new GameObjectList();

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
            this.lists.Clear();
            this.lists.AddRange(this.listMap.Values);

            for (var i = 0; i < this.lists.Count; i++)
            {
                if (this.lists[i] == null)
                    continue;

                var list = this.lists[i];

                for (var k = 0; k < list.Count; k++)
                {
                    if (list[k] && list[k].activeSelf)
                        list[k].SetActive(false);
                }
            }

            this.lists.Clear();
        }

        public GameObject Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
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
                Debug.LogWarning($"Key={key} is not defined", this);
                return null;
            }

            if (!GetPoolRoot())
            {
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
                Debug.LogWarning($"Pool key at index={i} is empty", this);
                return false;
            }

            if (this.listMap.ContainsKey(item.Key))
            {
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
            foreach (var list in this.listMap.Values)
            {
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    Destroy(list[i]);
                }
            }

            this.listMap.Clear();
            this.lists.Clear();
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