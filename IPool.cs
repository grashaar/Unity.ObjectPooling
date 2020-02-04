﻿using System.Collections.Generic;

namespace Unity.ObjectPooling
{
    public interface IGetOnlyPool<T>
    {
        T Get(string key = null);
    }

    public interface IReturnOnlyPool<T>
    {
        void Return(T item);

        void Return(params T[] items);

        void Return(IEnumerable<T> items);
    }

    public interface IPool<T> : IReturnOnlyPool<T>, IGetOnlyPool<T>
    {
        void ReturnAll();
    }
}