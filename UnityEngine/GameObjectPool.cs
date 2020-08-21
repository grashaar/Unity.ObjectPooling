using System;
using System.Collections.Generic;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine
{
    public sealed class GameObjectPool: IPool<GameObject>
    {
        public ReadList<GameObject> ActiveItems => this.activeItems;

        private readonly List<GameObject> activeItems = new List<GameObject>();
        private readonly Queue<GameObject> pool = new Queue<GameObject>();
        private readonly IInstantiator<GameObject> instantiator;
        private readonly AsyncInstantiator<GameObject> asyncInstantiator;

        public GameObjectPool(IInstantiator<GameObject> instantiator)
        {
            this.instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

        public GameObjectPool(AsyncInstantiator<GameObject> instantiator)
        {
            this.asyncInstantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

        private void ValidateInstantiator()
        {
            if (this.instantiator == null)
                throw new InvalidOperationException($"This instance of {GetType().Name}" +
                                                    $" has not been initialized with any instance of" +
                                                    $" {nameof(IInstantiator<GameObject>)}<{nameof(GameObject)}>.");
        }

        private void ValidateAsyncInstantiator()
        {
            if (this.asyncInstantiator == null)
                throw new InvalidOperationException($"This instance of {GetType().Name}" +
                                                    $" has not been initialized with any instance of" +
                                                    $" {nameof(AsyncInstantiator<GameObject>)}<{nameof(GameObject)}>.");
        }

        public void Prepool(int count)
        {
            ValidateInstantiator();

            for (var i = 0; i < count; i++)
            {
                var item = this.instantiator.Instantiate();

                if (!item)
                    continue;

                item.SetActive(false);
                this.pool.Enqueue(item);
            }
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask PrepoolAsync(int count)
#else
        public async Task PrepoolAsync(int count)
#endif
        {
            ValidateAsyncInstantiator();

            for (var i = 0; i < count; i++)
            {
                var item = await this.asyncInstantiator.InstantiateAsync();

                if (!item)
                    continue;

                item.SetActive(false);
                this.pool.Enqueue(item);
            }
        }

        public void Return(GameObject item)
        {
            if (!item)
                return;

            if (this.activeItems.Contains(item))
                this.activeItems.Remove(item);

            item.SetActive(false);
            this.pool.Enqueue(item);
        }

        public void Return(params GameObject[] items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                Return(item);
            }
        }

        public void Return(IEnumerable<GameObject> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                Return(item);
            }
        }

        public void ReturnAll()
        {
            for (var i = this.activeItems.Count - 1; i >= 0; i--)
            {
                var item = this.activeItems[i];
                this.activeItems.RemoveAt(i);

                item.SetActive(false);
                this.pool.Enqueue(item);
            }
        }

        public GameObject Get(string key = null)
        {
            ValidateInstantiator();

            GameObject item;

            if (this.pool.Count > 0)
            {
                item = this.pool.Dequeue();
                item.transform.position = Vector3.zero;
                item.SetActive(true);
            }
            else
            {
                item = this.instantiator.Instantiate();
            }

            if (item)
                this.activeItems.Add(item);

            return item;
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask<GameObject> GetAsync(string key = null)
#else
        public async Task<GameObject> GetAsync(string key = null)
#endif
        {
            ValidateAsyncInstantiator();

            GameObject item;

            if (this.pool.Count > 0)
            {
                item = this.pool.Dequeue();
                item.transform.position = Vector3.zero;
                item.SetActive(true);
            }
            else
            {
                item = await this.asyncInstantiator.InstantiateAsync();
            }

            if (item)
                this.activeItems.Add(item);

            return item;
        }
    }
}
