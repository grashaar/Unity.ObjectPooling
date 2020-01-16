using System.Collections.Generic;
using UniRx.Async;

namespace Unity.ObjectPooling
{
    public class PoolManager<T> : IPool<T> where T : class, new()
    {
        private readonly List<T> activeItems = new List<T>();

        public Segment<T> ActiveItems
            => this.activeItems;

        private readonly Queue<T> pool = new Queue<T>();

        public UniTask<bool> Prepool(int count)
        {
            for (var i = 0; i < count; i++)
            {
                this.pool.Enqueue(new T());
            }

            return UniTask.FromResult(true);
        }

        public void Return(T item)
        {
            if (item == null)
                return;

            if (this.activeItems.Contains(item))
                this.activeItems.Remove(item);

            this.pool.Enqueue(item);
        }

        public void ReturnAll()
        {
            for (var i = this.activeItems.Count - 1; i >= 0; i--)
            {
                var item = this.activeItems[i];
                this.activeItems.RemoveAt(i);
                this.pool.Enqueue(item);
            }
        }

        public T Get(string key = null)
        {
            T item;

            if (this.pool.Count > 0)
            {
                item = this.pool.Dequeue();
            }
            else
            {
                item = new T();
            }

            this.activeItems.Add(item);
            return item;
        }
    }
}
