namespace UnityEngine
{
    public interface IInstantiator<T>
    {
        T Instantiate();
    }
}