# Change Log

### v1.6.4 - 2023-10-26

##### Additions :tada:

- Added "Google Photorealistic 3D Tiles" to the Quick Add panel.

### v1.6.3 - 2023-10-02

##### Fixes :wrench:

- Made the project compatible with macOS versions as old as 10.13 by setting the `CMAKE_OSX_DEPLOYMENT_TARGET` variable to `10.13`.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.27.2 to v0.27.3. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v1.6.2 - 2023-09-20

##### Fixes :wrench:

- Fixed a bug that caused compilation errors in packaged game builds for iOS.
- Fixed a bug that caused Apple Silicon binaries to be missing from packaged games for macOS.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.27.1 to v0.27.2. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v1.6.1 - 2023-09-03

##### Fixes :wrench:

- Fixed a bug that prevented editor windows from functioning when `com.unity.vectorgraphics` package was installed.

This release also fixes an important bug by updating [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.27.0 to v0.27.1. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v1.6.0

##### Additions :tada:

- Added support for Universal Windows Platform (UWP), which is required to build applications for the Holo Lens 2.
- Added `ComputeLoadProgress` function to estimate the percentage of the 3D tileset that has been loaded for the current view.

##### Fixes :wrench:

- Fixed a bug that prevented building on iOS.
- Fixed a bug where KTX tilesets did not display properly on iOS devices due to a missing check for ETC1 texture format.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.26.0 to v0.27.0. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v1.5.0 - 2023-08-01

##### Fixes :wrench:

- Fixed a bug that could lead to incorrect textures when a KTX2 image did not include a complete mip chain.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.25.1 to v0.26.0. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v1.4.0 - 2023-07-03

##### Additions :tada:

- Added `OnTileGameObjectCreated` event to `Cesium3DTileset` class, which allows customizing the Tile GameObjects as they are loaded.
- KTX2 compressed textures now remain compressed all the way to the GPU, reducing GPU memory usage.

##### Fixes :wrench:

- Fixed how the occlusion strength is used in the default tileset shader which was causing shadows to be too dark.
- Fixed a bug that caused a prefab with a `CesiumGlobeAnchor` to lose its position after save/reload.
- Fixed a `MissingReferenceException` when entering Play mode with "Domain Reload" disabled. This would also prevent tilesets with raster overlays from appearing at all in Play mode.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.25.0 to v0.25.1. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v1.3.1 - 2023-06-06

##### Fixes :wrench:

- Fixed a bug that prevented building on iOS.

### v1.3.0 - 2023-06-01

##### Additions :tada:

- Cesium components now appear under the "Cesium" category in the Component menu. Previously were under "Scripts > Cesium for Unity"
- Cesium components now display the Cesium logo as their icon, rather than the default Unity script icon.

##### Fixes :wrench:

- Fixed a bug where `Cesium3DTileset` would not reflect changes made to the properties of its opaque material in the Editor.
- Fixed a bug that could cause missing textures when using two raster overlays with the same projection on a single tileset.
- Fixed a bug where changing the origin on a `CesiumGeoreference` would not propogate these changes to the active `CesiumSubScene`, if one exists.
- Reduced the amount of extraneous camera rotation in the `CesiumCameraController` after a frame hitch by using `Time.smoothDeltaTime`.
- Fixed a bug that caused mipmaps to be generated for textures that shouldn't be mipmapped, sometimes leading to cracks between tiles and other problems.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.24.0 to v0.25.0. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v1.2.0 - 2023-05-09

##### Additions :tada:

- Added a `scale` property to `CesiumGeoreference`. This allows the entire globe to be scaled up or down with better precision than the scale property on the georeference's `Transform`.

##### Fixes :wrench:

- Fixed a bug that caused tiles to be displaced when changing the transform of a `CesiumGeoreference` at runtime.
- Fixed a bug that caused primitive numbers to be negative in the names of tile game objects when the tile mesh had multiple primitives.

### v1.1.0 - 2023-05-01

##### Breaking Changes :mega:

- `CesiumObjectPool` is no longer accessible from outside the CesiumRuntime assembly.

##### Additions :tada:

- Added support for primitives with the `TRIANGLE_STRIP` and `TRIANGLE_FAN` topology types.
- Missing normals are now generated as "flat" normals by default, as required by the glTF specification. An option on `Cesium3DTileset` allows the user to request smooth normals instead, which will improve performance for most meshes by reducing geometry duplication.
- Moved mipmap generation from the main thread to a worker thread.

##### Fixes :wrench:

- Added dependencies on the ShaderGraph and InputSystem packages to resolve material / script compilation errors.
- Fixed another bug where `CesiumCameraController` tried to access a non-existent input in the legacy input system.
- Removed an extra "delimiter" added to the end of on-screen credits in some cases.
- Fixed a memory leak of `Mesh` objects when entering and exiting Play mode in the Unity Editor.
- Fixed a crash that happened when attempting to create physics meshes for degenerate triangle meshes.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.23.0 to v0.24.0. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v1.0.0 - 2023-04-03

##### Additions :tada:

- Added support for Unity's built-in render pipeline.
- Added `CesiumPointCloudShading`, which allows point cloud tilesets to be rendered with attenuation based on geometric error. Attenuation is currently only supported in the Universal Render Pipeline (URP).
- `GameObject` instances created for the tiles in a `Cesium3DTileset` now inherit the `layer` of the parent tileset.
- Added the `CesiumTileExcluder` abstract class. By creating a class derived from `CesiumTileExcluder`, then adding it to a `Cesium3DTileset`'s game object, you can implement custom rules for excluding tiles in the `Cesium3DTileset` from loading and rendering.
- Added setting in `CesiumRuntimeSettings` to configure the maximum number of responses to keep in the request cache.
- Added setting in `CesiumRuntimeSettings` to configure the number of reads from the cache database before each prune.

##### Fixes :wrench:

- Fixed a bug that prevented the use of pre-existing mipmaps, such as those loaded from KTX2.
- Fixed a bug where `CesiumCameraController` tried to access non-existent inputs in the legacy input system.
- Fixed a bug that could cause a crash when using the search box with a dataset already selected in the Cesium ion Assets window.
- Fixed a bug that prevented sorting the contents of the Cesium ion Assets window in descending order.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.22.1 to v0.23.0. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v0.3.1

##### Fixes :wrench:

- Fixed a bug introduced in v0.3.0 that caused an exception when attempting load Cesium3DTilesets from a local file.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.22.0 to v0.22.1. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

### v0.3.0

##### Breaking Changes :mega:

- Removed the `positionAuthority`, `unityX`, `unityY`, and `unityZ` properties from `CesiumGlobeAnchor`. Also removed the `SetPositionUnity` method.
- Removed the `CesiumGlobeAnchorPositionAuthority` enum. The authoritative position is now always found in the `positionGlobeFixed` property. The object's Unity world position can be obtained from its `Transform`.
- Marked the `longitude`, `latitude`, and `height` properties on `CesiumGlobeAnchor` as obsolete. Use the `longitudeLatitudeHeight` property instead.
- Marked the `ecefX`, `ecefY`, and `ecefZ` properties on `CesiumGlobeAnchor` as obsolete. Use the `positionGlobeFixed` property instead.
- Marked `SetPositionLongitudeLatitudeHeight` and `SetPositionEarthCenteredEarthFixed` methods on `CesiumGlobeAnchor` as obsolete. Set the `longitudeLatitudeHeight` or `positionGlobeFixed` property instead.
- Replaced `MetadataProperty` with `CesiumFeature`. Metadata features are now separated based on feature tables where properties are accessed by name.
- Replaced `CesiumMetadata.GetProperties` with `CesiumMetadata.GetFeatures`, which returns an array of `CesiumFeature`s.

##### Additions :tada:

- Added support for rendering point clouds (`pnts`).
- `CesiumGlobeAnchor` now stores a precise, globe-relative orientation and scale in addition to position.
- Added `localToGlobeFixedMatrix`, `longitudeLatitudeHeight`, `positionGlobeFixed`, `rotationGlobeFixed`, `rotationEastUpNorth`, `scaleGlobeFixed`, and `scaleEastUpNorth` properties to `CesiumGlobeAnchor`.
- Added the `Restart` method to `CesiumGlobeAnchor`, which can be use to reinitialize the component from its serialized values.
- Moved the Cesium tileset shaders from the `Shader Graphs` shader category to the new `Cesium` shader category.
- Added `CesiumDebugColorizeTilesRasterOverlay` to visualize how a tileset is divided into tiles.

##### Fixes :wrench:

- Fixed a bug that prevented caching of 3D Tiles and overlay requests.
- Fixed a bug that could cause the Cesium ion Token Troubleshooting panel to crash the Unity Editor.
- Added a workaround for a crash in the Burst Compiler (bcl.exe) in Unity 2022.2 when using il2cpp.
- Fixed a bug that could cause incorrect metadata to be associated with a feature, especially in Draco-encoded tiles.

In addition to the above, this release updates [cesium-native](https://github.com/CesiumGS/cesium-native) from v0.21.3 to v0.22.0. See the [changelog](https://github.com/CesiumGS/cesium-native/blob/main/CHANGES.md) for a complete list of changes in cesium-native.

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
