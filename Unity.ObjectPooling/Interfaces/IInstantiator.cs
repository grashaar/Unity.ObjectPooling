namespace Unity.ObjectPooling
{
    public interface IInstantiator<T>
    {
        T Instantiate();
    }
}