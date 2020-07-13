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

        private readonly List<string> keys
            = new List<string>();

        private void OnValidate()
        {
            this.manager = GetComponent<AddressableGameObjectPoolerManager>();
            this.pooler = GetComponent<AddressableGameObjectPooler>();
        }

#if UNITY_OBJECTPOOLING_UNITASK
        protected async UniTask InitializeAsync()
#else
        protected async Task InitializeAsync()
#endif
        {
            this.manager.Initialize();
            await this.manager.PrepoolAsync();
        }

        public void Deinitialize()
        {
            this.manager.Deinitialize();
            this.keys.Clear();

            OnDeinitialize();
        }

        protected bool ContainsKey(string key)
            => this.keys.Contains(key);

        protected void RegisterPoolItem(string key, AssetReferenceGameObject objectToPool, int prepoolAmount)
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