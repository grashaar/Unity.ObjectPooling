﻿#if UNITY_OBJECTPOOLING_ADDRESSABLES

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Unity.ObjectPooling.AddressableAssets
{
    public static partial class AddressableGameObjectInstantiator
    {
#if UNITY_OBJECTPOOLING_UNITASK

        public static async UniTask<GameObject> InstantiateAsync(AssetReferenceGameObject reference, Transform parent = null, bool instantiateInWorldSpace = false)
        {
#if UNITY_OBJECTPOOLING_ADDRESSABLES_MANAGER
            var obj = await AddressablesManager.InstantiateAsync(reference, parent, instantiateInWorldSpace);
#else
            var obj = await reference.InstantiateAsync(parent, instantiateInWorldSpace);
#endif
            return obj;
        }

#else

        public static async Task<GameObject> InstantiateAsync(AssetReferenceGameObject reference, Transform parent = null, bool instantiateInWorldSpace = false)
        {
#if UNITY_OBJECTPOOLING_ADDRESSABLES_MANAGER
            var obj = await AddressablesManager.InstantiateAsync(reference, parent, instantiateInWorldSpace);
            return obj;
#else
            var operation = reference.InstantiateAsync(parent, instantiateInWorldSpace);
            await operation.Task;
            return operation.Result;
#endif
        }

#endif

        public static void ReleaseInstance(AssetReferenceGameObject reference, GameObject obj)
        {
#if UNITY_OBJECTPOOLING_ADDRESSABLES_MANAGER
            AddressablesManager.ReleaseInstance(reference, obj);
#else
            if (reference != null && obj)
                reference.ReleaseInstance(obj);
#endif
        }
    }
}

#endif