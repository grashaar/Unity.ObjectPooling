using System;
using UnityEngine;

namespace Unity.ObjectPooling
{
    [Serializable]
    public class PoolItem
    {
        public string Key;
        public GameObject ObjectToPool;
        public int PrepoolAmount;
    }
}