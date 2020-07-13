using System.Collections.Generic;

namespace UnityEngine
{
    [RequireComponent(typeof(GameObjectPoolerManager), typeof(GameObjectPooler))]
    public class GameObjectSpawner : MonoBehaviour, IPool<GameObject>
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

        public virtual GameObject Get(string key)
        {
            var gameObject = this.manager.Get(key);

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
            => this.pooler.ReturnAll();

        protected virtual void OnDeinitialize() { }
    }
}