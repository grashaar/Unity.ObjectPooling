namespace Unity.ObjectPooling
{
    public interface IGetOnlyPool<T>
    {
        T Get(string key = null);
    }

    public interface IReturnOnlyPool<T>
    {
        void Return(T item);
    }

    public interface IPool<T> : IReturnOnlyPool<T>, IGetOnlyPool<T>
    {
        void ReturnAll();
    }
}