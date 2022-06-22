#if UNITY_OBJECTPOOLING_ADDRESSABLES

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Unity.ObjectPooling.AddressableAssets
{
    public static partial class DefaultReleaseHandlers
    {
        public static readonly AssetHandler Asset = new AssetHandler();

        public static readonly InstanceHandler Instance = new InstanceHandler();

        public struct AssetHandler : IKeyedReleaseHandler
        {
            public void Release(string key, GameObject obj)
            {
#if UNITY_OBJECTPOOLING_ADDRESSABLES_MANAGER
                AddressablesManager.ReleaseAsset(key);
#else
                Addressables.Release(obj);
#endif
            }
        }

        public struct InstanceHandler : IKeyedReleaseHandler
        {
            public void Release(string key, GameObject obj)
            {
#if UNITY_OBJECTPOOLING_ADDRESSABLES_MANAGER
                AddressablesManager.ReleaseInstance(key, obj);
#else
                Addressables.ReleaseInstance(obj);
#endif
            }
        }
    }
}

#endif