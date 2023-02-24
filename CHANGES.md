# Change Log

### v0.3.0

##### Breaking Changes :mega:

- Removed the `positionAuthority`, `unityX`, `unityY`, and `unityZ` properties from `CesiumGlobeAnchor`. Also removed the `SetPositionUnity` method. The authoritative position is now always found in the `positionGlobeFixed` property. The object's Unity world position can be obtained from its `Transform`.
- Marked the `longitude`, `latitude`, and `height` properties on `CesiumGlobeAnchor` as obsolete. Use the `longitudeLatitudeHeight` property instead.
- Marked the `ecefX`, `ecefY`, and `ecefZ` properties on `CesiumGlobeAnchor` as obsolete. Use the `positionGlobeFixed` property instead.
- Marked `SetPositionLongitudeLatitudeHeight` and `SetPositionEarthCenteredEarthFixed` methods on `CesiumGlobeAnchor` as obsolete. Set the `longitudeLatitudeHeight` or `positionGlobeFixed` property instead.

##### Additions :tada:

- Added support for rendering point clouds (`pnts`).
- Metadata features are now separated based on feature tables. Properties can now be accessed by name.
- `CesiumGlobeAnchor` now stores a precise, globe-relative orientation and scale in addition to position.
- Added `localToGlobeFixedMatrix`, `longitudeLatitudeHeight`, `positionGlobeFixed`, `rotationGlobeFixed`, `rotationEastUpNorth`, `scaleGlobeFixed`, and `scaleEastUpNorth` properties to `CesiumGlobeAnchor`.
- Added the `Restart` method to `CesiumGlobeAnchor`, which can be use to reinitialize the component from its serialized values.
- Copy all response headers from UnityWebRequest to enable caching.

##### Fixes :wrench:

- Recategorized the Cesium tileset shaders from the `Shader Graphs` shader category to the new `Cesium` shader category. 
- Fixed a bug that could cause the Cesium ion Token Troubleshooting panel to crash the Unity Editor.

### v0.2.0

##### Breaking Changes :mega:

- Renamed `CesiumTransforms` to `CesiumWgs84Ellipsoid`.

##### Additions :tada:

- Added `CesiumCameraController`, a globe-aware controller that adapts its speed and clipping planes based on its height from the globe.
- Added `CesiumFlyToController`, a controller that can smoothly fly to locations across the globe.
- Added an option to add a `DynamicCamera` from the Cesium panel to the scene. The `DynamicCamera` contains `CesiumCameraController` and `CesiumFlyToController` components and offers easy navigation of the globe.
- Added support for building to iOS.
- Added support for building to Android x86-64 devices like the Magic Leap 2.

##### Fixes :wrench:

- Fixed a bug where `CesiumGeoreference`, `CesiumGlobeAnchor`, and `CesiumSubScene` would not properly update when their values were changed by undos or pasted values.
- `CesiumRuntimeSettings` is now stored in `Assets/CesiumSettings/Resources` instead of `Assets/Settings/Resources`.
- Added an explicit `Physics.SyncTransforms` when `CesiumOriginShift` activates or deactivates sub-scenes, avoiding a brief period of potentially very incorrect collisions.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.21.1 to v0.21.3. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v0.1.2

##### Fixes :wrench:

- Fixed a bug that caused Cesium for Unity to fail to compile in Unity 2022.2 and potentially in other scenarios.
- Fixed a bug that led to an exception when the project name included characters outside the printable ASCII range.

### v0.1.1

##### Fixes :wrench:

- Fixed a bug that caused raster overlay tiles to be missing or mixed up, especially when zooming in close and then back out.

### v0.1.0

The initial release of Cesium for Unity!
