#if UNITY_OBJECTPOOLING_ADDRESSABLES

using System.Collections.Generic;
using System.Collections.Pooling;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine.AddressableAssets.Pooling
{
    /// <summary>
    /// Controls the lifetime of pooled game objects.
    /// </summary>
    [RequireComponent(typeof(AddressableGameObjectPooler))]
    public partial class AddressableGameObjectSpawner : MonoBehaviour, IAsyncKeyedPool<GameObject>
    {
        [HideInInspector]
        [SerializeField]
        private AddressableGameObjectPooler pooler = null;

        protected AddressableGameObjectPooler Pooler => this.pooler;

        public ReadList<string> Keys => this.keys;

        private readonly List<string> keys = new List<string>();

        protected virtual void Awake()
        {
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
        public async UniTask InitializeAsync(bool silent = false)
#else
        public async Task InitializeAsync(bool silent = false)
#endif
        {
            RefreshKeys();

            this.pooler.Silent = silent;
            this.pooler.PrepareItemMap();
            await this.pooler.PrepoolAsync();
        }
        /// <summary>
        /// Release all pooled items.
        /// </summary>
        public void Deinitialize()
        {
            this.pooler.DestroyAll();

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

#if UNITY_OBJECTPOOLING_UNITASK
        public virtual async UniTask<GameObject> GetAsync(string key)
#else
        public virtual async Task<GameObject> GetAsync(string key)
#endif
        {
            var go = await this.pooler.GetAsync(key);

            if (go)
            {
                go.SetActive(true);
            }

            return go;
        }
        

        public void Return(GameObject item)
        {
            if (!item)
                return;

            this.pooler.Return(item.gameObject);
        }

        public void Return(params GameObject[] items)
        {
            foreach (var item in items)
            {
                Return(item);
            }
        }

        public void Return(IEnumerable<GameObject> items)
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

#endif