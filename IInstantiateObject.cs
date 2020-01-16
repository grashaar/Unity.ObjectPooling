namespace Unity.ObjectPooling
{
    public interface IInstantiateObject<T>
    {
        T Instantiate();
    }
}