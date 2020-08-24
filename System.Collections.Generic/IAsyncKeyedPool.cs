#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace System.Collections.Generic
{
    public interface IAsyncKeyedGetOnlyPool<T>
    {
#if UNITY_OBJECTPOOLING_UNITASK
        UniTask<T> GetAsync(string key);
#else
        Task<T> GetAsync(string key);
#endif
    }

    public interface IAsyncKeyedPool<T> : IAsyncKeyedGetOnlyPool<T>, IReturnOnlyPool<T>
    {
    }
}