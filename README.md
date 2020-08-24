# Unity Object Pooling

## Changelog

### 1.3.0
- Upgrade `Unity Supplements` package to version `2.3.1`
- Remove `IPool<T>` and `Pool<T>` since they are existing in the `Unity Supplements` package
- Add `IKeyedPool<T>`, `IAsyncKeyedPool<T>` interfaces
- Refactor async methods of `ComponentPool` and `GameObjectPool` into `AsyncComponentPool` and `AsyncGameObjectPool`
- Remove `BehaviourPool` since it is redundant
- Pools now implement either keyed or no-keyed interface, not both
- `ActiveItems` property is renamed `ActiveObjects`


## Dependencies

- [Unity Supplements 2.3.1+](https://openupm.com/packages/com.laicasaane.unity-supplements/)

## Notes

- Automatically switch to UniTask if the package is present.
- Automatically enable Addressables-related classes if the package is present.
- Automatically use [Unity Addressables Manager](https://openupm.com/packages/com.laicasaane.unity-addressables-manager/) if the package is present.

## API
- System.Collections.Generic
    - [IKeyedPool\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/System.Collections.Generic/IKeyedPool.cs)
    - [IAsyncPool\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/System.Collections.Generic/IAsyncPool.cs)
    - [IAsyncKeyedPool\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/System.Collections.Generic/IAsyncKeyedPool.cs)

- UnityEngine
    - [IInstantiator\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/IInstantiator.cs)
    - [AsyncInstantiator](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/AsyncInstantiator.cs)
    - [ComponentPool\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/ComponentPool.cs)
    - [AsyncComponentPool\<T>](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/AsyncComponentPool.cs)
    - [GameObjectPool](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/GameObjectPool.cs)
    - [AsyncGameObjectPool](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/AsyncGameObjectPool.cs)
    - [GameObjectPooler](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/GameObjectPooler.cs)
    - [GameObjectPoolerManager](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/GameObjectPoolerManager.cs)
    - [GameObjectSpawner](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/GameObjectSpawner.cs)
    - [ComponentSpawner](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine/ComponentSpawner.cs)

- UnityEngine.AddressableAssets
    - [AddressableGameObjectInstantiator](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine.Addressables/AddressableGameObjectInstantiator.cs)
    - [AddressableGameObjectPooler](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine.Addressables/AddressableGameObjectPooler.cs)
    - [AddressableGameObjectPoolerManager](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine.Addressables/AddressableGameObjectPoolerManager.cs)
    - [AddressableGameObjectSpawner](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine.Addressables/AddressableGameObjectSpawner.cs)
    - [AddressableComponentSpawner](https://github.com/grashaar/Unity.ObjectPooling/blob/master/UnityEngine.Addressables/AddressableComponentSpawner.cs)