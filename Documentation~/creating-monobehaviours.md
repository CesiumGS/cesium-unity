Guidelines and tips for creating MonoBehaviours in Cesium for Unity.

## Avoid implementing non-static methods in C++

If you don't need C++-specific state, static methods are _much_ more efficient. Consider adding the `staticOnly=true` parameter to the `[ReinteropNativeImplementation]` attribute, so that you - and other developers in the future - can't accidentally create C++ state.

## Life Cycle and Methods to Implement.

Do initialization in OnEnable

If you need to use some other Cesium component (like a `CesiumGeoreference`, for instance), its OnEnable might not be called yet. So initialize it explicitly.

Reset

## Serialization

| *Characteristic*              | *`[SerializeField]`* | *No attribute* | *`[NonSerialized]`* |
|-------------------------------|----------------------|--------------|-------------------|
| Saved / Loaded with the Scene | ✅                   | ❌          | ❌                |
| Preserved on script change / AppDomain reload | ✅   | ✅          | ❌                |
| Transfers from Edit mode to Play mode      | ✅   | ❌          | ❌                |

## Backward compatibility
