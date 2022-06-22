#if UNITY_OBJECTPOOLING_ADDRESSABLES
#if UNITY_OBJECTPOOLING_ADDRESSABLES_1_17

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Unity.ObjectPooling.AddressableAssets
{
    public static partial class AddressableGameObjectInstantiator
    {
        public static GameObject Instantiate(AssetReferenceGameObject reference, Transform parent = null, bool instantiateInWorldSpace = false)
        {
#if UNITY_OBJECTPOOLING_ADDRESSABLES_MANAGER
            var obj = AddressablesManager.InstantiateSync(reference, parent, instantiateInWorldSpace);
#else
            var handle = reference.InstantiateAsync(parent, instantiateInWorldSpace);
            var obj = handle.WaitForCompletion();
#endif
            return obj;
        }
    }
}

#endif
#endif