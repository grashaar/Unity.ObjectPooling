# Unity Object Pooling

## Dependencies

- [Unity Supplements 2.0.0+](https://openupm.com/packages/com.laicasaane.unity-supplements/)

## Notes

- Automatically switch to UniTask if the package is present.
- Automatically enable Addressables-related classes if the package is present.

## API
- System.Collections.Generic
    - [IPool\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/System.Collections.Generic/IPool.cs)
    - [IAsyncPool\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/System.Collections.Generic/IAsyncPool.cs)
    - [Pool\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/System.Collections.Generic/Pool.cs)

- UnityEngine
    - [ComponentPool\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/ComponentPool.cs)
    - [BehaviourPool\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/BehaviourPool.cs)
    - [GameObjectPool](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/GameObjectPool.cs)
    - [IInstantiator\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/IInstantiator.cs)
    - [AsyncInstantiator](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/AsyncInstantiator.cs)
    - [GameObjectPooler](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/GameObjectPooler.cs)
    - [GameObjectPoolerManager](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/GameObjectPoolerManager.cs)
    - [GameObjectSpawner](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/GameObjectSpawner.cs)
    - [ComponentSpawner](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/ComponentSpawner.cs)

- UnityEngine.AddressableAssets
    - [AddressableGameObjectPooler](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine.Addressables/AddressableGameObjectPooler.cs)
    - [AddressableGameObjectPoolerManager](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine.Addressables/AddressableGameObjectPoolerManager.cs)
    - [AddressableGameObjectInstantiator](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine.Addressables/AddressableGameObjectInstantiator.cs)
    - [AddressableGameObjectSpawner](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine.Addressables/AddressableGameObjectSpawner.cs)
    - [AddressableComponentSpawner](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine.Addressables/AddressableComponentSpawner.cs)