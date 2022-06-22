#if UNITY_OBJECTPOOLING_ADDRESSABLES
#if UNITY_OBJECTPOOLING_ADDRESSABLES_1_17

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Unity.ObjectPooling.AddressableAssets
{
    [RequireComponent(typeof(AddressableGameObjectPooler))]
    public partial class AddressableComponentSpawner<T> : IKeyedPool<T>
    {
        public void Initialize(bool silent = false)
        {
            RefreshKeys();

            this.pooler.Silent = silent;
            this.pooler.PrepareItemMap();
            this.pooler.Prepool();
        }

        public T Get(string key)
        {
            var gameObject = this.pooler.Get(key);

            if (!gameObject)
                return null;

            var behaviour = gameObject.GetComponent<T>();

            if (behaviour)
            {
                gameObject.SetActive(true);
            }

            return behaviour;
        }
    }
}

#endif
#endif