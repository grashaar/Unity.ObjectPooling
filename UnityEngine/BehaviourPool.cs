namespace UnityEngine
{
    public class BehaviourPool<T> : ComponentPool<T> where T : Behaviour
    {
        public BehaviourPool(IInstantiator<T> instantiator) : base(instantiator) { }

        public BehaviourPool(AsyncInstantiator<T> instantiator) : base(instantiator) { }
    }
}
