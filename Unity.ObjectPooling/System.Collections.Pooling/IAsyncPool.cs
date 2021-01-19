#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace System.Collections.Pooling
{
    public interface IAsyncGetOnlyPool<T>
    {
#if UNITY_OBJECTPOOLING_UNITASK
        UniTask<T> GetAsync();
#else
        Task<T> GetAsync();
#endif
    }

    public interface IAsyncPool<T> : IAsyncGetOnlyPool<T>, IReturnOnlyPool<T>
    {
    }
}