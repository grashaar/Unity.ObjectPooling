#if UNITY_OBJECTPOOLING_ADDRESSABLES
#if UNITY_OBJECTPOOLING_ADDRESSABLES_1_17

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Unity.ObjectPooling.AddressableAssets
{
    public sealed partial class AddressableGameObjectPooler : IKeyedPool<GameObject>
    {
        private GameObject Instantiate(PoolItem item, int number)
        {
            if (item.Object == null)
            {
                Debug.LogError($"Cannot instantiate null object of key={item.Key}", this);
                return null;
            }

            var obj = AddressableGameObjectInstantiator.Instantiate(item.Object, GetPoolRoot(), true);
            obj.name = $"{item.Key}-{number}";
            obj.SetActive(false);

            return obj;
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

                var list = pool.Get();

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
                list = Pool.Provider.Pool<GameObjectList>().Get();
                this.listMap.Add(key, list);
            }

            var obj = Instantiate(poolItem, list.Count);

            if (obj)
                list.Add(obj);

            return obj;
        }
    }
}

#endif
#endif