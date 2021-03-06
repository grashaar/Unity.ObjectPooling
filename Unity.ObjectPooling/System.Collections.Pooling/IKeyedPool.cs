﻿namespace System.Collections.Pooling
{
    public interface IKeyedGetOnlyPool<T>
    {
        T Get(string key);
    }

    public interface IKeyedPool<T> : IReturnOnlyPool<T>, IKeyedGetOnlyPool<T>
    {
        void ReturnAll();
    }
}