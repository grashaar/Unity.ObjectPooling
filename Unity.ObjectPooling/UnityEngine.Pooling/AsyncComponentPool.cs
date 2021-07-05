using System;
using System.Collections.Generic;
using System.Collections.Pooling;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine.Pooling
{
    public class AsyncComponentPool<T> : IAsyncPool<T>, IReturnInactive where T : Component
    {
        protected static readonly Type ComponentType = typeof(T);

        public ReadList<T> ActiveObjects => this.activeObjects;

        private readonly List<T> activeObjects = new List<T>();
        private readonly Queue<T> pool = new Queue<T>();
        private readonly AsyncInstantiator<T> instantiator;

        public AsyncComponentPool(AsyncInstantiator<T> instantiator)
        {
            this.instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask PrepoolAsync(int count)
#else
        public async Task PrepoolAsync(int count)
#endif
        {
            for (var i = 0; i < count; i++)
            {
                var item = await this.instantiator.InstantiateAsync();

                if (!item)
                    continue;

                item.gameObject.SetActive(false);
                this.pool.Enqueue(item);
            }
        }

        public void Return(T item)
        {
            if (!item)
                return;

            if (this.activeObjects.Contains(item))
                this.activeObjects.Remove(item);

            if (item.gameObject.activeSelf)
                item.gameObject.SetActive(false);

            if (!this.pool.Contains(item))
                this.pool.Enqueue(item);
        }

        public void Return(params T[] items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                Return(item);
            }
        }

        public void Return(IEnumerable<T> items)
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
            for (var i = this.activeObjects.Count - 1; i >= 0; i--)
            {
                var item = this.activeObjects[i];
                this.activeObjects.RemoveAt(i);

                if (item.gameObject.activeSelf)
                    item.gameObject.SetActive(false);

                if (!this.pool.Contains(item))
                    this.pool.Enqueue(item);
            }
        }

        public void ReturnInactive()
        {
            var cache = ListPool<T>.Get();

            for (var i = 0; i < this.activeObjects.Count; i++)
            {
                var obj = this.activeObjects[i];

                if (obj && obj.gameObject && !obj.gameObject.activeSelf)
                    cache.Add(obj);
            }

            Return(cache);
            ListPool<T>.Return(cache);
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask<T> GetAsync()
#else
        public async Task<T> GetAsync()
#endif
        {
            T item;

            if (this.pool.Count > 0)
            {
                item = this.pool.Dequeue();
                item.transform.position = Vector3.zero;
                item.gameObject.SetActive(true);
            }
            else
            {
                item = await this.instantiator.InstantiateAsync();
            }

            if (item)
                this.activeObjects.Add(item);

            return item;
        }

        public void DestroyAll()
        {
            ReturnAll();

            while (this.pool.Count > 0)
            {
                var item = this.pool.Dequeue();

                if (!item || !item.gameObject)
                    continue;

                Object.Destroy(item.gameObject);
            }
        }

        public void DestroyAll<THandler>(THandler handler) where THandler : IDestroyHandler
        {
            ReturnAll();

            while (this.pool.Count > 0)
            {
                var item = this.pool.Dequeue();

                if (!item || !item.gameObject)
                    continue;

                handler.Destroy(item.gameObject);
            }
        }
    }
}
