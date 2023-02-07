Guidelines and tips for creating MonoBehaviours in Cesium for Unity.

## Avoid implementing non-static methods in C++

If you don't need C++-specific state, static methods are _much_ more efficient.

## Do initialization in OnEnable

If you need to use some other Cesium component, its OnEnable might not be called yet. So initialize it explicitly.

## Backward compatibility

## Serialization

| *Characteristic*              | *`[SerializeField]`* | *No attribute* | *`[NonSerialized]`* |
|-------------------------------|----------------------|--------------|-------------------|
| Saved / Loaded with the Scene | ✅                   | ❌          | ❌                |
| Preserved on script change / AppDomain reload | ✅   | ✅          | ❌                |
| Transferring from Edit mode to Play mode      | ✅   | ❌          | ❌                |
