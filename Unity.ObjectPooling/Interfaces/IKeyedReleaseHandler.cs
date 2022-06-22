using UnityEngine;

namespace Unity.ObjectPooling
{
    public interface IKeyedReleaseHandler
    {
        void Release(string key, GameObject obj);
    }
}