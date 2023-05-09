# Releasing a new version of Cesium for Unity

This is the process we follow when releasing a new version of Cesium for Unity on GitHub.

## Test the release candidate

* Verify that the cesium-native submodule in the `extern` directory references the expected commit of cesium-native. Update it if necessary. Verify that CI has completed successfully for that commit of cesium-native.
* Wait for CI to complete for the main branch. Verify that it does so successfully.
* Download the `Combined Package` for the `main` branch of cesium-unity. Extract the tarball (`.tgz`) file.
* Clone a fresh copy of [cesium-unity-samples](https://github.com/CesiumGS/cesium-unity-samples) to a new directory.
* Before opening the project, go to the Packages folder and open `manifest.json` in a text editor. It should contain the Cesium for Unity package as a scoped registry.
```
"scopedRegistries": [
    {
      "name": "Cesium",
      "url": "https://unity.pkg.cesium.com",
      "scopes": [
        "com.cesium.unity"
      ]
    }
  ]
```
Remove this entry. Then, find the `com.cesium.unity` dependency under the `"dependencies"` object. Delete this entry as well.
* Launch the project in the Unity Editor. When prompted, click `Enter Safe Mode`, since all of the Cesium objects and scripts will be missing.
* Go to Window -> Package Manager. Click on the add icon in the top left corner and select "Add package from tarball". Navigate to where you extracted the tarball, then select it and click "Open".
* Assuming there are no more compilation errors, Unity should automatically exit Safe Mode once it processes the package. You should see Cesium for Unity installed in the Package Manager window.
* Open each scene in Assets -> CesiumForUnitySamples -> Scenes and verify it works correctly:
  * Does it open without crashing?
  * Does it look correct?
  * Press Play. Does it work as expected? The billboard in each level should give you a good idea of what to expect.
* Test on other platforms and other versions of Unity if you can. If you can't (e.g., you don't have a Mac), post a message on Slack asking others to give it at least a quick smoke test.

If all of the above goes well, you're ready to release Cesium for Unity.

## Update CHANGES.md and tag the cesium-native and cesium-unity releases

While doing the steps below, make sure no new changes are going into either cesium-unity or cesium-native that may invalidate the testing you did above. If new changes go in, it's ok, but you should either retest with those changes or make sure they are not included in the release.

* Change the version of the Cesium for Unity to the new three digit, dot-delimited version number in Cesium for Unity's `package.json`. Use [Semantic Versioning](https://semver.org/) to pick the version number.
* Verify that cesium-native's CHANGES.md is complete and accurate.
* Verify that cesium-unity's CHANGES.md is complete and accurate.
* Verify again that cesium-native CI has completed successfully on all platforms.
* Verify again that the submodule reference in cesium-unity references the correct commit of cesium-native.
* Verify again that cesium-unity CI has completed successfully on all platforms.
* Tag the cesium-native release, e.g., `git tag -a v0.2.0 -m "0.2.0 release"`
* Push the tag to github: `git push origin v0.2.0`
* Tag the cesium-unity release, e.g., `git tag -a v1.1.0 -m "1.1.0 release"`
* Push the tag to github: `git push origin v1.1.0`

# Publish the release on GitHub

* Wait for the release tag CI build to complete.
* Download the tag's `Combined Package.zip`. Find it by switching to the tag in the GitHub UI, clicking the green tick in the header, and then clicking the Details button next to any of the intermediate jobs. Then, click Summary and find the `.zip` under Artifacts. While you're here, copy the download URL because you'll need it later.
* Extract the tarball from the zip.
* Create a new release on GitHub: https://github.com/CesiumGS/cesium-unity/releases/new. Copy the changelog into it. Follow the format used in previous release. Upload the tarball that you extracted above.

# Update the Package Registry

Follow the instructions in the internal Cesium guide for updating Cesium for Unity in the Package Registry. Then, remove the tarball from the samples project. Restore the scoped registry and confirm that the package works with the new version.

# Update Cesium for Unity Samples

Assuming you tested the release candidate as described above, you should have [cesium-unity-samples](https://github.com/CesiumGS/cesium-unity-samples) using the updated package. You'll use this to push updates to the project.

## Update ion Access Tokens and Project

1. Create a new branch of cesium-unity-samples. 
2. Change the `bundleVersion` property in `ProjectSettings/ProjectSettings.asset` to reflect the new version of the Samples project.
3. Delete the Cesium for Unity Samples token for the release before last, which should expire close to the present date.
4. Create a new access token using the CesiumJS ion account. 
   * The name of the token should match "Cesium for Unity Samples x.x.x - Delete on September 1st, 2021". The expiry date should be two months later than present. 
   * The scope of the token should be "assets:read" for all assets.
5. Copy the access token you just created.
6. Paste the new token into the `_defaultIonAccessToken` property in [Assets/CesiumSettings/Resources/CesiumRuntimeSettings.asset](https://github.com/CesiumGS/cesium-unity-samples/blob/main/Assets/CesiumSettings/Resources/CesiumRuntimeSettings.asset).
7. Open cesium-unity-samples in Unity.
8. If the package update has replaced any Cesium scripts or prefabs that already exist in one of the scenes, e.g., DynamicCamera, replace the old version of the prefab with the new version, and test the scene with the play button to make sure everything is working. If you're unsure whether the package update has resulted in anything that needs to be changed in the Samples, ask the team. 
9. Visit every scene again to make sure that the view is correct and that nothing appears to be broken or missing. 
10. For every scene involving multiple locations or sub-scenes, e.g., 04_CesiumSubScenes or 06_CesiumPointClouds, make sure that flying to each location works.
11. Commit and push your changes. Create a PR to merge to `main` and tag a reviewer.

## Publish the Cesium for Unity Samples release on GitHub

After the update has been merged to `main`, do the following:
1. Pull and check out the latest version of `main` from GitHub, and then tag the new release by doing the following:
  * `git tag -a v1.10.0 -m "v1.10.0 release"`
  * `git push origin v1.10.0`
2. Switch to the tag in the GitHub UI by visiting the repo, https://github.com/CesiumGS/cesium-unity-samples, clicking the combo box where it says "main", switching to the Tags tab, and selecting the new tag that you created above.
3. Create a new release on GitHub: https://github.com/CesiumGS/cesium-unity-samples/releases/new. Select the tag you created above. Add a changelog in the body to describe recent updates. Follow the format used in previous release.
4. Publish the release as a _draft_, and then go the [releases](https://github.com/CesiumGS/cesium-unity-samples/releases). Download the "Source Code (zip)" asset. Rename it to `CesiumForUnitySamples-v1.2.0.zip`, replacing `v1.2.0` with the correct version number for this release.
5. Edit the draft release and attach the renamed ZIP file to it.
6. Publish the release.
