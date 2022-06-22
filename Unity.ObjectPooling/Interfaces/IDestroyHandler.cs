using UnityEngine;

namespace Unity.ObjectPooling
{
    public interface IDestroyHandler
    {
        void Destroy(GameObject obj);
    }
}