using System;
using System.Collections.Generic;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine
{
    public class ComponentPool<T> : IPool<T> where T : Component
    {
        protected static readonly Type ComponentType = typeof(T);

        public ReadList<T> ActiveItems => this.activeItems;

        private readonly List<T> activeItems = new List<T>();
        private readonly Queue<T> pool = new Queue<T>();
        private readonly IInstantiator<T> instantiator;
        private readonly AsyncInstantiator<T> asyncInstantiator;

        public ComponentPool(IInstantiator<T> instantiator)
        {
            this.instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

        public ComponentPool(AsyncInstantiator<T> instantiator)
        {
            this.asyncInstantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

        private void ValidateInstantiator()
        {
            if (this.instantiator == null)
                throw new InvalidOperationException($"This instance of {GetType().Name}" +
                                                    $" has not been initialized with any instance of" +
                                                    $" {nameof(IInstantiator<T>)}<{ComponentType.Name}>.");
        }

        private void ValidateAsyncInstantiator()
        {
            if (this.asyncInstantiator == null)
                throw new InvalidOperationException($"This instance of {GetType().Name}" +
                                                    $" has not been initialized with any instance of" +
                                                    $" {nameof(AsyncInstantiator<T>)}<{ComponentType.Name}>.");
        }

        public void Prepool(int count)
        {
            ValidateInstantiator();

            for (var i = 0; i < count; i++)
            {
                var item = this.instantiator.Instantiate();

                if (!item)
                    continue;

                item.gameObject.SetActive(false);
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

                item.gameObject.SetActive(false);
                this.pool.Enqueue(item);
            }
        }

        public void Return(T item)
        {
            if (!item)
                return;

            if (this.activeItems.Contains(item))
                this.activeItems.Remove(item);

            item.gameObject.SetActive(false);
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
            for (var i = this.activeItems.Count - 1; i >= 0; i--)
            {
                var item = this.activeItems[i];
                this.activeItems.RemoveAt(i);

                item.gameObject.SetActive(false);
                this.pool.Enqueue(item);
            }
        }

        public T Get(string key = null)
        {
            ValidateInstantiator();

            T item;

            if (this.pool.Count > 0)
            {
                item = this.pool.Dequeue();
                item.transform.position = Vector3.zero;
                item.gameObject.SetActive(true);
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
        public async UniTask<T> GetAsync(string key = null)
#else
        public async Task<T> GetAsync(string key = null)
#endif
        {
            ValidateAsyncInstantiator();

            T item;

            if (this.pool.Count > 0)
            {
                item = this.pool.Dequeue();
                item.transform.position = Vector3.zero;
                item.gameObject.SetActive(true);
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
