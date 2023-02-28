Guidelines and tips for creating MonoBehaviours in Cesium for Unity.

## Avoid implementing non-static methods in C++

If you don't need C++-specific state, static methods are _much_ more efficient. Consider adding the `staticOnly=true` parameter to the `[ReinteropNativeImplementation]` attribute, so that you - and other developers in the future - can't accidentally create C++ state.

## Lifecycle and Methods to Implement.

* Do initialization in `OnEnable`. Avoid `Start` and `Awake`. The reason is that `Start` and `Awake` are _not_ invoked on domain reload, such as when editing a script, but `OnEnable` is. So by using only `OnEnable`, we can essentially treat a domain reload the same as a normal reload of the scene.

* `OnEnable` for different components may be called in arbitrary order. So if you're implementing an `OnEnable` that requires some other component to already be initialized, you _must_ trigger this initialization manually. For example, if your `OnEnable` needs to use a `CesiumGeoreference`, call the georeference's `Initialize` method near the beginning of your `OnEnable`.

* Implement `ICesiumRestartable` and its `Restart` method. This method is called by the UI when Unity has updated the serialized fields in unspecified ways, and so the state of the object should be recreated from the serialized fields.

* In most cases, the implementation of `OnEnable` should simply call `Restart`.

* Implement `Reset`, usually by simply calling `Restart`. This method is called when Unity has directly written to the serialized fields to reset them, and so the `Restart` method is the right way to synchronize the object's state.

* What's the difference between `Restart` (e.g. on `CesiumGlobeAnchor`) and `Initialize` (e.g. on `CesiumGeoreference`)? `Restart` completely recreates the object's state from the current values of the serialized fields. The current state is assumed to be invalid and thrown away. `Initialize`, on the other hand, prepares the object for first use but does nothing if the object is already initialized.

* `OnEnable` and `OnDisable` usually work as a pair, with initialization happening in `OnEnable` and cleanup happening in `OnDisable`. Unity guarantees that `OnDisable` is called before an enabled component is destroyed. But be careful if some of the initialization can happen elsewhere other than `OnEnable`, such as in `Reset`, `Restart`, or `Initialize`. If that happens for a disabled component, `OnDisable` will never be called and so the cleanup may never happen. The solution is to skip some or all of the initialization work if the component is not `isActiveAndEnabled`.


## Serialization

Carefully consider every field that you add to the class. In general, only the essential fields necessary to reconstruct the state of the object should by marked `[SerializeField]`. Cached and derived fields should instead be marked `[NonSerialized]`. Fields without any attribute should be extremely rare.

| *Characteristic*              | *`[SerializeField]`* | *No attribute* | *`[NonSerialized]`* |
|-------------------------------|----------------------|--------------|-------------------|
| Saved / Loaded with the Scene | ✅                   | ❌          | ❌                |
| Preserved on script change / AppDomain reload | ✅   | ✅          | ❌                |
| Transfers from Edit mode to Play mode      | ✅   | ❌          | ❌                |

## Backward compatibility

Unity's serialization system is extremely simplistic (presumably in the name of performance), and makes backward compatibility difficult. Our solution is powerful but a bit involved. Here's how it works.

Let's say we want to implement backward compatible loading for `CesiumGlobeAnchor` instances saved with Cesium for Unity v0.2.0. First, we add a new `CesiumGlobeAnchorBackwardCompatibility0dot2dot0.cs` file, but don't launch Unity yet! In that file, define a new class named `CesiumGlobeAnchorBackwardCompatibility0dot2dot0`, derived from the original class, `CesiumGlobeAnchor` and implementing the interface `IBackwardCompatibilityComponent<CesiumGlobeAnchor>`. Add the following attributes to the class declaration:

* `[ExecuteInEditMode]` - So that the class's `OnEnable` (which we'll write shortly) is called in the Editor.
* `[AddComponentMenu("")]` - So that this class will not show up in the "Add Component" panel in the Editor.
* `[DefaultExecutionOrder(-1000000)]` - So that this class's `OnEnable` runs really early, before other classes.

```cs
[ExecuteInEditMode]
[AddComponentMenu("")]
[DefaultExecutionOrder(-1000000)]
internal class CesiumGlobeAnchorBackwardCompatibility0dot2dot0 : CesiumGlobeAnchor, IBackwardCompatibilityComponent<CesiumGlobeAnchor>
{
    // ...
}
```

Define all of the properties that existed in the old version of the `CesiumGlobeAnchor`, but rename them by appending the version (`0dot2dot0`) to the end. Add a `[FormerlySerializedAs]` attribute with the previous name. For example:

```cs
[FormerlySerializedAs("_adjustOrientationForGlobeWhenMoving")]
public bool _adjustOrientationForGlobeWhenMoving0dot2dot0 = false;

[FormerlySerializedAs("_positionAuthority")]
public CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot2dot0 _positionAuthority0dot2dot0 = CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot2dot0.None;

[FormerlySerializedAs("_latitude")]
public double _latitude0dot2dot0 = 0.0;
```

If a field is an enum that has been eliminated entirely, or if its enum values were changed from the old version, declare the old enum type nested inside the backward compatibility class. Since `CesiumGlobeAnchorPositionAuthority` was removed from `CesiumGlobeAnchor`, a backwards-compatible enum is defined in `CesiumGlobeAnchorBackwardCompatibility0dot2dot0`:


```cs
public enum CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot2dot0
{
    None,
    LongitudeLatitudeHeight,
    EarthCenteredEarthFixed,
    UnityCoordinates
}
```

Next, declare an `Editor` class, nested inside `CesiumGlobeAnchorBackwardCompatibility0dot2dot0`, that merely provides an Upgrade button, and an `OnEnable` that automatically upgrades. Put both inside an ifdef for `UNITY_EDITOR`:

```cs
#if UNITY_EDITOR
    [CustomEditor(typeof(CesiumGlobeAnchorBackwardCompatibility0dot2dot0))]
    internal class CesiumGlobeAnchorBackwardCompatibility0dot2dot0Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Upgrade"))
            {
                CesiumGlobeAnchorBackwardCompatibility0dot2dot0 o = (CesiumGlobeAnchorBackwardCompatibility0dot2dot0)this.target;
                CesiumBackwardCompatibility<CesiumGlobeAnchor>.Upgrade(o);
            }
        }
    }

    void OnEnable()
    {
        CesiumBackwardCompatibility<CesiumGlobeAnchor>.Upgrade(this);
    }
#endif
```

Finally, implement `IBackwardCompatibilityComponent<CesiumGlobeAnchor>` to write the upgrade logic itself:

```cs
public string VersionToBeUpgraded => "v0.2.0";

public void Upgrade(GameObject gameObject, CesiumGlobeAnchor upgraded)
{
    // Temporarily disable orientation adjustment so that we can set the position without
    // risking rotating the object.
    upgraded.adjustOrientationForGlobeWhenMoving = false;
    upgraded.detectTransformChanges = false;
    
    switch (this._positionAuthority0dot2dot0)
    {
        case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot2dot0.None:
            // This shouldn't happen, but if it does, just leave the position at the default.
            break;
        case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot2dot0.LongitudeLatitudeHeight:
            upgraded.longitudeLatitudeHeight = new double3(this._longitude0dot2dot0, this._latitude0dot2dot0, this._height0dot2dot0);
            break;
        case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot2dot0.EarthCenteredEarthFixed:
            upgraded.positionGlobeFixed = new double3(this._ecefX0dot2dot0, this._ecefY0dot2dot0, this._ecefZ0dot2dot0);
            break;
        case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot2dot0.UnityCoordinates:
            // Any backward compatibility for CesiumGeoreference must have a more negative
            // DefaultExecutionOrder so that the real CesiumGeoreference is created first.
            // If this component is not nested inside a CesiumGeoreference, converting Unity coordinates
            // to ECEF is impossible, so just keep the default position.
            CesiumGeoreference georeference = this.GetComponentInParent<CesiumGeoreference>();
            if (georeference != null)
            {
                georeference.Initialize();
                double3 ecef = georeference.TransformUnityPositionToEarthCenteredEarthFixed(new double3(this._unityX0dot2dot0, this._unityY0dot2dot0, this._unityZ0dot2dot0));
                upgraded.positionGlobeFixed = ecef;
            }
            break;
    }

    upgraded.adjustOrientationForGlobeWhenMoving = this._adjustOrientationForGlobeWhenMoving0dot2dot0;
    upgraded.detectTransformChanges = this._detectTransformChanges0dot2dot0;
}
```

Finally, rename `CesiumGlobeAnchor.cs.meta` to `CesiumGlobeAnchorBackwardCompatibility0dot2dot0.cs.meta`. Now launch Unity. Unity will create a new `CesiumGlobeAnchor.cs.meta` with a new GUID. Any scenes saved with a `CesiumGlobeAnchor` using the _old_ GUID will instead end up loading a `CesiumGlobeAnchorBackwardCompatibility0dot2dot0` instead. When we invoke `CesiumBackwardCompatibility<CesiumGlobeAnchor>.Upgrade()` on the new backward compatibility component, either when this object is enabled in the Editor or when the user explicitly clicks the `Upgrade` button in a prefab, a few things happen:

* A component of the new type (`CesiumGlobeAnchor`) is created.
* The `Upgrade` method is called to initialize the new `CesiumGlobeAnchor` from the backward compatibility instance.
* The mapping between the old (`CesiumGlobeAnchorBackwardCompatibility0dot2dot0`) and the new (`CesiumGlobeAnchor`) instances is added to a dictionary, and a call to `UpdateScene` is registered for the next Editor tick.

In the next Editor tick, after one or more backward compatible components have been upgraded, the following happens:

* We walk through every component on every game object in all open scenes, looking for serialized fields contain a reference to any of the old, backward compatible instances.
* For each we find, we replace it with a reference to the new instance.
* The old backward compatible instances are destroyed.
* The new components are moved up or down in their game object's component list so that they're in the same position as the backward compatible instance they replace.
* Modified game objects are marked dirty, so the Editor will prompt the user to save the changes.
