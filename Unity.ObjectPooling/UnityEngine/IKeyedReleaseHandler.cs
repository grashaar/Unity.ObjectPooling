namespace UnityEngine
{
    public interface IKeyedReleaseHandler
    {
        void Release(string key, GameObject obj);
    }
}