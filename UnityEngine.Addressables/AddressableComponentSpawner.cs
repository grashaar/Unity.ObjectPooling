using System.Collections.Generic;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine.AddressableAssets
{
    [RequireComponent(typeof(AddressableGameObjectPoolerManager), typeof(AddressableGameObjectPooler))]
    public abstract class AddressableComponentSpawner<T> : MonoBehaviour, IAsyncPool<T>
        where T : Component
    {
        [HideInInspector]
        [SerializeField]
        private AddressableGameObjectPoolerManager manager = null;

        [HideInInspector]
        [SerializeField]
        private AddressableGameObjectPooler pooler = null;

        protected AddressableGameObjectPoolerManager Manager => this.manager;

        protected AddressableGameObjectPooler Pooler => this.pooler;

        public ReadList<string> Keys => this.keys;

        private readonly List<string> keys = new List<string>();

        protected virtual void Awake()
        {
            this.manager = GetComponent<AddressableGameObjectPoolerManager>();
            this.pooler = GetComponent<AddressableGameObjectPooler>();

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

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask InitializeAsync()
#else
        public async Task InitializeAsync(bool silent = false)
#endif
        {
            RefreshKeys();

            this.manager.Initialize(silent);
            await this.manager.PrepoolAsync();
        }

        public void Deinitialize()
        {
            this.manager.DestroyAll();

            OnDeinitialize();
        }

        public bool ContainsKey(string key)
            => this.keys.Contains(key);

        public void RegisterPoolItem(string key, AssetReferenceGameObject objectToPool, int prepoolAmount)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (this.keys.Contains(key))
                return;

            if (objectToPool == null)
                return;

            this.keys.Add(key);
            this.pooler.Register(new AddressableGameObjectPooler.PoolItem {
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

        public void DeregisterAllPoolItems()
        {
            this.keys.Clear();
            this.pooler.DeregisterAll();
        }

        [System.Obsolete("This method has been deprecated. Use GetAsync instead.")]
        T IGetOnlyPool<T>.Get(string key)
        {
            throw new System.NotImplementedException();
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public virtual async UniTask<T> GetAsync(string key)
#else
        public virtual async Task<T> GetAsync(string key)
#endif
        {
            var gameObject = await this.manager.GetAsync(key);

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