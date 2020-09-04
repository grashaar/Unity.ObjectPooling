using System;
using System.Collections.Generic;

#if UNITY_OBJECTPOOLING_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace UnityEngine
{
    public sealed class AsyncGameObjectPool : IAsyncPool<GameObject>, IReturnInactive
    {
        public ReadList<GameObject> ActiveObjects => this.activeObjects;

        private readonly List<GameObject> activeObjects = new List<GameObject>();
        private readonly Queue<GameObject> pool = new Queue<GameObject>();
        private readonly AsyncInstantiator<GameObject> asyncInstantiator;

        public AsyncGameObjectPool(AsyncInstantiator<GameObject> instantiator)
        {
            this.asyncInstantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask PrepoolAsync(int count)
#else
        public async Task PrepoolAsync(int count)
#endif
        {
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

            if (this.activeObjects.Contains(item))
                this.activeObjects.Remove(item);

            if (item.activeSelf)
                item.SetActive(false);

            if (!this.pool.Contains(item))
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
            for (var i = this.activeObjects.Count - 1; i >= 0; i--)
            {
                var item = this.activeObjects[i];
                this.activeObjects.RemoveAt(i);

                if (item.activeSelf)
                    item.SetActive(false);

                if (!this.pool.Contains(item))
                    this.pool.Enqueue(item);
            }
        }

        public void ReturnInactive()
        {
            var cache = ListPool<GameObject>.Get();

            for (var i = 0; i < this.activeObjects.Count; i++)
            {
                var obj = this.activeObjects[i];

                if (obj && !obj.activeSelf)
                    cache.Add(obj);
            }

            Return(cache);
            ListPool<GameObject>.Return(cache);
        }

#if UNITY_OBJECTPOOLING_UNITASK
        public async UniTask<GameObject> GetAsync()
#else
        public async Task<GameObject> GetAsync()
#endif
        {
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
                this.activeObjects.Add(item);

            return item;
        }
    }
}
