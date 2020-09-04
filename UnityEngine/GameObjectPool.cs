using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public sealed class GameObjectPool: IPool<GameObject>, IReturnInactive
    {
        public ReadList<GameObject> ActiveObjects => this.activeObjects;

        private readonly List<GameObject> activeObjects = new List<GameObject>();
        private readonly Queue<GameObject> pool = new Queue<GameObject>();
        private readonly IInstantiator<GameObject> instantiator;

        public GameObjectPool(IInstantiator<GameObject> instantiator)
        {
            this.instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

        public void Prepool(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var item = this.instantiator.Instantiate();

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

        public GameObject Get()
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
                item = this.instantiator.Instantiate();
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

                if (!item)
                    continue;

                Object.Destroy(item);
            }
        }
    }
}
