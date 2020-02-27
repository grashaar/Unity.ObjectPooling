using UnityEngine;

namespace Unity.ObjectPooling
{
    public class BehaviourPoolManager<T> : ComponentPoolManager<T> where T : MonoBehaviour
    {
        public BehaviourPoolManager(IInstantiateObject<T> instantiator) : base(instantiator) { }
    }
}
