using System;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine
{
    public abstract class AsyncInstantiator<T>
    {
#if UNITY_OBJECTPOOLING_UNITASK
        public virtual async UniTask<T> InstantiateAsync()
        {
            await UniTask.Yield();
            throw new NotImplementedException();
        }
#else
        public virtual async Task<T> InstantiateAsync()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }
#endif
    }
}