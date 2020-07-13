using System;
using System.Collections.Generic;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine.AddressableAssets
{
    public sealed class AddressableGameObjectPooler : MonoBehaviour, IAsyncPool<GameObject>
    {
#if UNITY_OBJECTPOOLING_ADDRESSABLES

        [SerializeField]
        private Transform poolRoot = null;

        [Space]
        [SerializeField]
        private List<PoolItem> items = new List<PoolItem>();

        public IReadOnlyList<PoolItem> Items
            => this.items;

        private readonly ItemMap itemMap = new ItemMap();
        private readonly GameObjectListMap listMap = new GameObjectListMap();
        private readonly List<GameObjectList> lists = new List<GameObjectList>();

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

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask PrepoolAsync()
#else
        public async Task PrepoolAsync()
#endif
        {
            if (this.isPrepooled)
                return;

            if (!this.poolRoot)
                this.poolRoot = this.transform;

            for (var i = 0; i < this.items.Count; i++)
            {
                if (!ValidateItemAt(i))
                    continue;

                var item = this.Items[i];

                if (!this.itemMap.ContainsKey(item.Key))
                    this.itemMap.Add(item.Key, item);

                var list = new GameObjectList();

                for (var k = 0; k < item.PrepoolAmount; k++)
                {
                    var obj = await Instantiate(item, k);
                    list.Add(obj);
                }

                this.listMap.Add(item.Key, list);
            }

            this.isPrepooled = true;
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

        [Obsolete("This method has been deprecated. Use GetAsync instead.")]
        GameObject IGetOnlyPool<GameObject>.Get(string key)
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
                Debug.LogWarning("Key is empty", this);
                return null;
            }

            if (!this.listMap.TryGetValue(key, out var list))
            {
                Debug.LogWarning($"Key={key} does not exist", this);
                return null;
            }

            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];

                if (item && !item.activeInHierarchy)
                    return item;
            }

            if (!this.itemMap.TryGetValue(key, out var poolItem))
            {
                Debug.LogWarning($"Key={key} is not defined", this);
                return null;
            }

            if (!this.poolRoot)
            {
                Debug.LogWarning("Pool root is null", this);
                return null;
            }

            var obj = await Instantiate(poolItem, list.Count);
            list.Add(obj);

            return obj;
    }

#if UNITY_OBJECTPOOLING_UNITASK
        private async UniTask<GameObject> Instantiate(PoolItem item, int number)
#else
        private async Task<GameObject> Instantiate(PoolItem item, int number)
#endif
        {
            var obj = await AddressableGameObjectInstantiator.InstantiateAsync(item.Object, this.poolRoot, true);
            obj.name = $"{item.Key}-{number}";
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
            foreach (var item in this.items)
            {
                if (!this.listMap.TryGetValue(item.Key, out var list))
                    continue;

                for (var i = list.Count - 1; i >= 0; i--)
                {
                    item.Object.ReleaseInstance(list[i]);
                }
            }

            foreach (var list in this.listMap.Values)
            {
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i])
                        Destroy(list[i]);
                }
            }

            this.listMap.Clear();
            this.lists.Clear();
        }

        [Serializable]
        public class PoolItem
        {
            public string Key;
            public AssetReferenceGameObject Object;
            public int PrepoolAmount;
        }

        [Serializable]
        private class ItemMap : Dictionary<string, PoolItem> { }

        [Serializable]
        private class GameObjectListMap : Dictionary<string, GameObjectList> { }

        [Serializable]
        private class GameObjectList : List<GameObject> { }

#endif
    }
}