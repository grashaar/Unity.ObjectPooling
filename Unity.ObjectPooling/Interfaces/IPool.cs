using System.Collections.Generic;

namespace Unity.ObjectPooling
{
    public interface IGetOnlyPool<T>
    {
        T Get();
    }

    public interface IReturnOnlyPool<T>
    {
        void Return(T item);

        void Return(params T[] items);

        void Return<U>(U items) where U : IEnumerable<T>;
    }

    public interface IPool<T> : IReturnOnlyPool<T>, IGetOnlyPool<T>
    {
    }
}
