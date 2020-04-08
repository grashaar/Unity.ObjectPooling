using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.ObjectPooling
{
    public class PoolManager<T> : IPool<T> where T : class, new()
    {
        public Segment<T> ActiveItems
            => this.activeItems;

        private readonly List<T> activeItems = new List<T>();
        private readonly Queue<T> pool = new Queue<T>();

        public Task<bool> Prepool(int count)
        {
            for (var i = 0; i < count; i++)
            {
                this.pool.Enqueue(new T());
            }

            return Task.FromResult(true);
        }

        public void Return(T item)
        {
            if (item == null)
                return;

            if (this.activeItems.Contains(item))
                this.activeItems.Remove(item);

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
