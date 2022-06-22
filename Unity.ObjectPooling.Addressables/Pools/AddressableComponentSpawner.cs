#if UNITY_OBJECTPOOLING_ADDRESSABLES

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace Unity.ObjectPooling.AddressableAssets
{
    [RequireComponent(typeof(AddressableGameObjectPooler))]
    public abstract partial class AddressableComponentSpawner<T> : MonoBehaviour, IAsyncKeyedPool<T>
        where T : Component
    {
        [HideInInspector]
        [SerializeField]
        private AddressableGameObjectPooler pooler = null;

        protected AddressableGameObjectPooler Pooler => this.pooler;

        public IReadOnlyList<string> Keys => this.keys;

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
        public virtual async UniTask<T> GetAsync(string key)
#else
        public virtual async Task<T> GetAsync(string key)
#endif
        {
            var gameObject = await this.pooler.GetAsync(key);

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

        public void Return<U>(U items) where U : IEnumerable<T>
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