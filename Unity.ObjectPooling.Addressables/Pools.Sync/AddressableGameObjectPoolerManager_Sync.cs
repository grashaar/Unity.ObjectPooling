#if UNITY_OBJECTPOOLING_ADDRESSABLES
#if UNITY_OBJECTPOOLING_ADDRESSABLES_1_17

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Unity.ObjectPooling.AddressableAssets
{
    public partial class AddressableGameObjectPoolerManager : IKeyedPool<GameObject>
    {
        public void Prepool()
        {
            foreach (var pool in this.poolerMap.Values)
            {
                if (!pool)
                    continue;

                pool.Prepool();
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
    }
}

#endif
#endif