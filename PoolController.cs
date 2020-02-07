using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Async;

namespace Unity.ObjectPooling
{
    public class PoolController : MonoBehaviour, IPool<GameObject>
    {
        [SerializeField]
        private Transform poolRoot = null;

        [Space]
        [SerializeField]
        private List<PoolItem> items = new List<PoolItem>();

        public IReadOnlyList<PoolItem> Items
            => this.items;

        private readonly PoolItemMap itemMap = new PoolItemMap();
        private readonly ObjectListMap objectMap = new ObjectListMap();
        private readonly List<ObjectList> tempList = new List<ObjectList>();

        private bool isPrepooled = false;

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
        }

        public void Deregister(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            var index = this.items.FindIndex(x => string.Equals(x.Key, key));

            if (index >= 0)
                this.items.RemoveAt(index);
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

        public UniTask<bool> Prepool()
        {
            if (this.isPrepooled)
                return UniTask.FromResult(true);

            if (!this.poolRoot)
            {
                this.poolRoot = this.transform;
            }

            for (var i = 0; i < this.items.Count; i++)
            {
                if (!ValidateItemAt(i))
                    continue;

                var item = this.Items[i];

                if (!this.itemMap.ContainsKey(item.Key))
                    this.itemMap.Add(item.Key, item);

                var list = new ObjectList();

                for (var k = 0; k < item.PrepoolAmount; k++)
                {
                    var obj = Instantiate(item.ObjectToPool);
                    obj.name = $"{item.ObjectToPool.name}-{k}";
                    obj.transform.SetParent(this.poolRoot, true);
                    obj.SetActive(false);

                    list.Add(obj);
                }

                this.objectMap.Add(item.Key, list);
            }

            this.isPrepooled = true;
            return UniTask.FromResult(true);
        }

        public void ReturnAll()
        {
            this.tempList.Clear();
            this.tempList.AddRange(this.objectMap.Values);

            for (var i = 0; i < this.tempList.Count; i++)
            {
                if (this.tempList[i] == null)
                    continue;

                var list = this.tempList[i];

                for (var k = 0; k < list.Count; k++)
                {
                    if (list[k] && list[k].activeSelf)
                        list[k].SetActive(false);
                }
            }

            this.tempList.Clear();
        }

        public GameObject Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Key is empty", this);
                return null;
            }

            if (!this.objectMap.ContainsKey(key))
            {
                Debug.LogWarning($"Key={key} does not exist", this);
                return null;
            }

            var list = this.objectMap[key];

            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];

                if (item && !item.activeInHierarchy)
                    return item;
            }

            if (!this.itemMap.ContainsKey(key))
            {
                Debug.LogWarning($"Key={key} is not defined", this);
                return null;
            }

            if (!this.poolRoot)
            {
                Debug.LogWarning("Pool root is null", this);
                return null;
            }

            var poolItem = this.itemMap[key];

            var obj = Instantiate(poolItem.ObjectToPool);
            obj.name = $"{poolItem.ObjectToPool.name}-{list.Count}";
            obj.transform.SetParent(this.poolRoot, true);
            obj.SetActive(false);

            list.Add(obj);

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

            if (this.objectMap.ContainsKey(item.Key))
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

        [Serializable]
        private class PoolItemMap : Dictionary<string, PoolItem> { }

        [Serializable]
        private class ObjectListMap : Dictionary<string, ObjectList> { }

        [Serializable]
        private class ObjectList : List<GameObject> { }
    }
}