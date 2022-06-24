using System.Collections.Generic;
using System.Collections.Pooling;

namespace UnityEngine.Pooling
{
    [RequireComponent(typeof(GameObjectPooler))]
    public abstract class ComponentSpawner<T> : MonoBehaviour, IKeyedPool<T> where T : Component
    {
        [HideInInspector]
        [SerializeField]
        private GameObjectPooler pooler = null;

        protected GameObjectPooler Pooler => this.pooler;

        public ReadList<string> Keys => this.keys;

        private readonly List<string> keys = new List<string>();

        protected virtual void Awake()
        {
            this.pooler = GetComponent<GameObjectPooler>();

            RefreshKeys();
        }

        public void RefreshKeys()
        {
            this.keys.Clear();

            foreach (var item in this.pooler.Items)
            {
                if (!this.keys.Contains(item.Key))
                    this.keys.Add(item.Key);
            }
        }

        public void Initialize(bool silent = false)
        {
            RefreshKeys();

            this.pooler.Silent = silent;
            this.pooler.PrepareItemMap();
            this.pooler.Prepool();
        }

        public void Deinitialize()
        {
            this.pooler.DestroyAll();

            OnDeinitialize();
        }

        public bool ContainsKey(string key)
            => this.keys.Contains(key);

        public void RegisterPoolItem(string key, GameObject objectToPool, int prepoolAmount)
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

        public void DeregisterPoolItem(string key)
        {
            var index = this.keys.FindIndex(x => string.Equals(x, key));

            if (index >= 0)
                this.keys.RemoveAt(index);

            this.pooler.Deregister(key);
        }
        /// <summary>
        /// Deregister all pool items but not release them
        /// </summary>
        public void DeregisterAllPoolItems()
        {
            this.keys.Clear();
            this.pooler.DeregisterAll();
        }
        /// <summary>
        /// Deregister all pool items and release them by a <see cref="TReleaseHandler"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="releaseHandler"></param>
        /// <typeparam name="TReleaseHandler"></typeparam>
        public void DeregisterPoolItem<TReleaseHandler>(string key, TReleaseHandler releaseHandler)
            where TReleaseHandler : IKeyedReleaseHandler
        {
            var index = this.keys.FindIndex(x => string.Equals(x, key));

            if (index >= 0)
                this.keys.RemoveAt(index);

            this.pooler.Deregister(key, releaseHandler);
        }

        public void DeregisterAllPoolItems<TReleaseHandler>(TReleaseHandler releaseHandler)
            where TReleaseHandler : IKeyedReleaseHandler
        {
            this.keys.Clear();
            this.pooler.DeregisterAll(releaseHandler);
        }

        public virtual T Get(string key)
        {
            var gameObject = this.pooler.Get(key);

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

            this.pooler.Return(item.gameObject);
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
            => this.pooler.ReturnAll();

        protected virtual void OnDeinitialize() { }
    }
}