Guidelines and tips for creating MonoBehaviours in Cesium for Unity.

## Avoid implementing non-static methods in C++

If you don't need C++-specific state, static methods are _much_ more efficient. Consider adding the `staticOnly=true` parameter to the `[ReinteropNativeImplementation]` attribute, so that you - and other developers in the future - can't accidentally create C++ state.

## Lifecycle and Methods to Implement.

* Do initialization in `OnEnable`. Avoid `Start` and `Awake`. The reason is that `Start` and `Awake` are _not_ invoked on domain reload, such as when editing a script, but `OnEnable` is. So by using only `OnEnable`, we can essentially treat a domain reload the same as a normal reload of the scene.

* `OnEnable` for different components may be called in arbitrary order. So if you're implementing an `OnEnable` that requires some other component to already be initialized, you _must_ trigger this initialization manually. For example, if your `OnEnable` needs to use a `CesiumGeoreference`, call the georeference's `Initialize` method near the beginning of your `OnEnable`. In most cases, `OnEnable` should simply call `Restart` (see below).

* Implement `ICesiumRestartable` and its `Restart` method. This method is called by the UI when Unity has updated the serialized fields in unspecified ways, and so the state of the object should be recreated from the serialized fields.

* Implement `Reset`, usually by simply calling `Restart`. This method is called when Unity has directly written to the serialized fields to reset them, and so the `Restart` method is the right way to synchronize the object's state.

* What's the difference between `Restart` (e.g. on `CesiumGlobeAnchor`) and `Initialize` (e.g. on `CesiumGeoreference`)? `Restart` completely recreates the object's state from the current values of the serialized fields. The current state is assumed to be invalid and thrown away. `Initialize`, on the other hand, prepares the object for use but does nothing if the object is already ready.

What if Initialize is called on a disabled object?


## Serialization

Carefully consider every field that you add to the class. In general, only the essential fields necessary to reconstruct the state of the object should by marked `[SerializeField]`. Cached and derived fields should instead be marked `[NonSerialized]`. Fields without any attribute should be extremely rare.

| *Characteristic*              | *`[SerializeField]`* | *No attribute* | *`[NonSerialized]`* |
|-------------------------------|----------------------|--------------|-------------------|
| Saved / Loaded with the Scene | ✅                   | ❌          | ❌                |
| Preserved on script change / AppDomain reload | ✅   | ✅          | ❌                |
| Transfers from Edit mode to Play mode      | ✅   | ❌          | ❌                |

## Backward compatibility
