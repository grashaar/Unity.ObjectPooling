#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace System.Collections.Generic
{
    public interface IGetOnlyAsyncPool<T>
    {
#if UNITY_OBJECTPOOLING_UNITASK
        UniTask<T> GetAsync(string key = null);
#else
        Task<T> GetAsync(string key = null);
#endif
    }

    public interface IAsyncPool<T> : IGetOnlyAsyncPool<T>, IPool<T>
    {
    }
}