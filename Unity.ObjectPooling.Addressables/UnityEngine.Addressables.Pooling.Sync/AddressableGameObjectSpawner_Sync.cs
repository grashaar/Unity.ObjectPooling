#if UNITY_OBJECTPOOLING_ADDRESSABLES
#if UNITY_OBJECTPOOLING_ADDRESSABLES_1_17

using System.Collections.Pooling;

namespace UnityEngine.AddressableAssets.Pooling
{
    [RequireComponent(typeof(AddressableGameObjectPooler))]
    public partial class AddressableGameObjectSpawner : IKeyedPool<GameObject>
    {
        public void Initialize(bool silent = false)
        {
            RefreshKeys();

            this.pooler.Silent = silent;
            this.pooler.PrepareItemMap();
            this.pooler.Prepool();
        }

        public GameObject Get(string key)
        {
            var gameObject = this.pooler.Get(key);

            if (gameObject)
            {
                gameObject.SetActive(true);
            }

            return gameObject;
        }
    }
}

#endif
#endif