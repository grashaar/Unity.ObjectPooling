#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine
{
    public interface IAsyncInstantiator<T>
    {
#if UNITY_OBJECTPOOLING_UNITASK
        UniTask<T> InstantiateAsync();
#else
        Task<T> InstantiateAsync();
#endif
    }
}