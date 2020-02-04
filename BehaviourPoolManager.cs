using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Async;

namespace Unity.ObjectPooling
{
    public class BehaviourPoolManager<T> : IPool<T> where T : MonoBehaviour
    {
        public Segment<T> ActiveItems
            => this.activeItems;

        private readonly List<T> activeItems = new List<T>();
        private readonly Queue<T> pool = new Queue<T>();
        private readonly IInstantiateObject<T> instantiator;

        public BehaviourPoolManager(IInstantiateObject<T> instantiator)
        {
            this.instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

        public UniTask<bool> Prepool(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var item = this.instantiator.Instantiate();

                if (!item)
                    continue;

                item.gameObject.SetActive(false);
                this.pool.Enqueue(item);
            }

            return UniTask.FromResult(true);
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
    }
}
