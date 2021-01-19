# Unity Object Pooling

## Changelog

### 1.4.0
- Upgrade `Unity Supplements` package to version `2.5.3`
- Change the namespace of pooling classes
- Restructure the package

### 1.3.0
- Upgrade `Unity Supplements` package to version `2.3.1`
- Remove `IPool<T>` and `Pool<T>` since they are existing in the `Unity Supplements` package
- Add `IKeyedPool<T>`, `IAsyncKeyedPool<T>` interfaces
- Refactor async methods of `ComponentPool` and `GameObjectPool` into `AsyncComponentPool` and `AsyncGameObjectPool`
- Remove `BehaviourPool` since it is redundant
- Pools now implement either keyed or no-keyed interface, not both
- `ActiveItems` property is renamed `ActiveObjects`


## Dependencies

- [Unity Supplements 2.5.3+](https://openupm.com/packages/com.laicasaane.unity-supplements/)

## Notes

- Automatically switch to UniTask if the package is present.
- Automatically enable Addressables-related classes if the package is present.
- Automatically use [Unity Addressables Manager](https://openupm.com/packages/com.laicasaane.unity-addressables-manager/) if the package is present.

## API

### System.Collections.Pooling
- [IKeyedPool\<T>](./Unity.ObjectPooling/System.Collections.Pooling/IKeyedPool.cs)
- [IAsyncPool\<T>](./Unity.ObjectPooling/System.Collections.Pooling/IAsyncPool.cs)
- [IAsyncKeyedPool\<T>](./Unity.ObjectPooling/System.Collections.Pooling/IAsyncKeyedPool.cs)

### UnityEngine
- [IInstantiator\<T>](./Unity.ObjectPooling/UnityEngine/IInstantiator.cs)
- [AsyncInstantiator](./Unity.ObjectPooling/UnityEngine/AsyncInstantiator.cs)
- [IReturnInactive](./Unity.ObjectPooling/UnityEngine/IReturnInactive.cs)

### UnityEngine.Pooling
- [ComponentPool\<T>](./Unity.ObjectPooling/UnityEngine.Pooling/ComponentPool.cs)
- [AsyncComponentPool\<T>](./Unity.ObjectPooling/UnityEngine.Pooling/AsyncComponentPool.cs)
- [GameObjectPool](./Unity.ObjectPooling/UnityEngine.Pooling/GameObjectPool.cs)
- [AsyncGameObjectPool](./Unity.ObjectPooling/UnityEngine.Pooling/AsyncGameObjectPool.cs)
- [GameObjectPooler](./Unity.ObjectPooling/UnityEngine.Pooling/GameObjectPooler.cs)
- [GameObjectPoolerManager](./Unity.ObjectPooling/UnityEngine.Pooling/GameObjectPoolerManager.cs)
- [ComponentSpawner](./Unity.ObjectPooling/UnityEngine.Pooling/ComponentSpawner.cs)
- [GameObjectSpawner](./Unity.ObjectPooling/UnityEngine.Pooling/GameObjectSpawner.cs)

### UnityEngine.AddressableAssets
- [AddressableGameObjectInstantiator](./Unity.ObjectPooling/UnityEngine.Addressables/AddressableGameObjectInstantiator.cs)

### UnityEngine.AddressableAssets.Pooling
- [AddressableComponentSpawner](./Unity.ObjectPooling/UnityEngine.Addressables.Pooling/AddressableComponentSpawner.cs)
- [AddressableGameObjectPooler](./Unity.ObjectPooling/UnityEngine.Addressables.Pooling/AddressableGameObjectPooler.cs)
- [AddressableGameObjectPoolerManager](./Unity.ObjectPooling/UnityEngine.Addressables.Pooling/AddressableGameObjectPoolerManager.cs)
- [AddressableGameObjectSpawner](./Unity.ObjectPooling/UnityEngine.Addressables.Pooling/AddressableGameObjectSpawner.cs)