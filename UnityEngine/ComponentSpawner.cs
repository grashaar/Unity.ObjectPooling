using System.Collections.Generic;

namespace UnityEngine
{
    [RequireComponent(typeof(GameObjectPoolerManager), typeof(GameObjectPooler))]
    public abstract class ComponentSpawner<T> : MonoBehaviour, IPool<T>
        where T : Component
    {
        [HideInInspector]
        [SerializeField]
        private GameObjectPoolerManager manager = null;

        [HideInInspector]
        [SerializeField]
        private GameObjectPooler pooler = null;

        private readonly List<string> keys
            = new List<string>();

        private void OnValidate()
        {
            this.manager = GetComponent<GameObjectPoolerManager>();
            this.pooler = GetComponent<GameObjectPooler>();
        }

        protected void Initialize()
        {
            this.manager.Initialize();
            this.manager.Prepool();
        }

        public void Deinitialize()
        {
            this.manager.Deinitialize();
            this.keys.Clear();

            OnDeinitialize();
        }

        protected bool ContainsKey(string key)
            => this.keys.Contains(key);

        protected void RegisterPoolItem(string key, GameObject objectToPool, int prepoolAmount)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (this.keys.Contains(key))
                return;

            if (!objectToPool)
                return;

            this.keys.Add(key);

            this.pooler.Register(new GameObjectPooler.PoolItem {
                Key = key,
                Object = objectToPool,
                PrepoolAmount = prepoolAmount
            });
        }

        public virtual T Get(string key)
        {
            var gameObject = this.manager.Get(key);

            if (!gameObject)
                return null;

            var behaviour = gameObject.GetComponent<T>();

            if (behaviour)
            {
                gameObject.SetActive(true);
            }

            return behaviour;
        }

        public void Return(T item)
        {
            if (!item)
                return;

            this.manager.Return(item.gameObject);
        }

        public void Return(params T[] items)
        {
            foreach (var item in items)
            {
                Return(item);
            }
        }

        public void Return(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Return(item);
            }
        }

        public void ReturnAll()
            => this.manager.ReturnAll();

        protected virtual void OnDeinitialize() { }
    }
}