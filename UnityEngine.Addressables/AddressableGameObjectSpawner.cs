using System.Collections.Generic;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine.AddressableAssets
{
    [RequireComponent(typeof(AddressableGameObjectPoolerManager), typeof(AddressableGameObjectPooler))]
    public class AddressableGameObjectSpawner : MonoBehaviour, IAsyncPool<GameObject>
    {
        [HideInInspector]
        [SerializeField]
        private AddressableGameObjectPoolerManager manager = null;

        [HideInInspector]
        [SerializeField]
        private AddressableGameObjectPooler controller = null;

        private readonly List<string> keys
            = new List<string>();

        private void OnValidate()
        {
            this.manager = GetComponent<AddressableGameObjectPoolerManager>();
            this.controller = GetComponent<AddressableGameObjectPooler>();
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

            foreach (var key in this.keys)
            {
                AddressablesManager.ReleaseAsset(key);
            }

            this.keys.Clear();

            OnDeinitialize();
        }

        protected void RegisterPoolItem(string key, AssetReferenceGameObject objectToPool, int prepoolAmount)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (this.keys.Contains(key))
                return;

            if (objectToPool == null)
                return;

            this.keys.Add(key);

            this.controller.Register(new AddressableGameObjectPooler.PoolItem {
                Key = key,
                Object = objectToPool,
                PrepoolAmount = prepoolAmount
            });
        }

        [System.Obsolete("This method has been deprecated. Use GetAsync instead.")]
        GameObject IGetOnlyPool<GameObject>.Get(string key)
        {
            throw new System.NotImplementedException();
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public virtual async UniTask<GameObject> GetAsync(string key)
#else
        public virtual async Task<GameObject> GetAsync(string key)
#endif
        {
            var gameObject = await this.manager.GetAsync(key);

            if (gameObject)
            {
                gameObject.SetActive(true);
            }

            return gameObject;
        }

        public void Return(GameObject item)
        {
            if (!item)
                return;

            this.manager.Return(item.gameObject);
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
            => this.controller.ReturnAll();

        protected virtual void OnDeinitialize() { }
    }
}