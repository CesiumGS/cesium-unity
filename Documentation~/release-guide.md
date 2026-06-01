# Releasing a new version of Cesium for Unity {#release-guide}

This is the process we follow when releasing a new version of Cesium for Unity on GitHub.
<!--! [TOC] -->

## Prepare Cesium Native for Release

[cesium-native](https://github.com/CesiumGS/cesium-native) is used as a git submodule of cesium-unity. So the first step in releasing Cesium for Unity is to make sure that Cesium Native is ready for release. See the [Cesium Native release guide](#native-release-process) for instructions. You may want to wait to create the new Cesium Native tag until after you have tested the Cesium for Unity release.

## Prepare Cesium for Unity for Release

1. Update the cesium-native submodule reference to point to the latest commit that you pushed above.
   - Enter the `native~/extern/cesium-native` directory and pull the latest changes and checkout the main branch as normal.
   - To update the submodule reference, from the cesium-unity root, run: `git add native~/extern/cesium-native`
2. Verify that `CHANGES.md` is complete and accurate.
   - Give the header of the section containing the latest changes an appropriate version number and date.
   - Diff main against the previous released version. This helps catch changes that are missing from the changelog, as well as changelog entries that were accidentally added to the wrong section.
   - Use [Semantic Versioning](https://semver.org/) to pick the version number.
   - Don't forget to add a note to the end of the version section indicating the cesium-native version number change. Use a previous version as the template for this and update the "from" and "to" version numbers accordingly.
3. Change the version in `package.json`.
4. Commit these changes. You can push them directly to `main`.
5. Before continuing, verify that CI passes for all platforms.

## Optimistically upload the release packages to the GitHub releases page

Cesium for Unity's release package is fairly large, so it can take awhile to upload it to the GitHub Releases page. For that reason, it is helpful to start the process now, even though we haven't tested the release yet.

1. Go to https://github.com/CesiumGS/cesium-unity/actions.
2. Click the most recent build of the `main` branch (it should be near the top). This should be the build for the commit you pushed in the previous section. If it's still running, wait. If it failed, you'll need to fix it.
3. Scroll down to **Artifacts**.
4. Download the artifact named "Combined Package".
5. Extract the `com.cesium.unity-1.21.0.tgz` (or similar) from the `Combined Package.zip` file. On macOS this is a pain to do, because Finder's unzip insists on also extracting the contents of the .tgz file. The best approach is probably to use `unzip` from the command-line.
6. Create a new release by visiting https://github.com/CesiumGS/cesium-unity/releases/new.
7. Leave the release tag blank for now, as we haven't created it yet.
8. Set `Release Title` to "Cesium for Unity v1.21.0", updating the version number as appropriate.
9. Copy the "release notes" section from a previous release, which you can find by visiting https://github.com/CesiumGS/cesium-unity/releases/latest and clicking the Edit button. Be careful not to change or save the previous release!
10. Update the version numbers as appropriate in the top section. Replace the changelog section with the actual changelog entries from this release. Copy it from `CHANGES.md`.
11. Upload the .tgz file by dragging it into the "Attach binaries" box.
12. Click Save Draft. Be careful not to publish it yet.
13. Visit the [releases page](https://github.com/CesiumGS/cesium-unity/releases) and you'll see the new draft release at the top. Expand the "Assets" section you should see the file you uploaded above.
14. Take a moment to verify that the SHA256 listed for each file is identical to the SHA256 for the same file that was printed at the end of the build by the "Print SHA256 of combined package" job. If they are, this proves the release package hasn't been inadvertently modified while you had a copy of it on your local machine. If they're different, stop the release process immediately!

If the uploaded package is later found to have problems during testing, this step will need to be repeated.

## Update the "Cesium for Unity Samples" Project

1. Clone a fresh copy of [cesium-unity-samples](https://github.com/CesiumGS/cesium-unity-samples) to a new directory.
2. Before opening the project, go to the Packages folder and open `Packages/manifest.json` in a text editor. Find the `com.cesium.unity` dependency under the `"dependencies"` object and change the version to the version number of the in-progress release.
3. Open `ProjectSettings/ProjectSettings.asset` and find the `bundleVersion` key (under `PlayerSettings`). Set this to the version number in the in-progress release.
4. Create a new Cesium ion token for this release under the "CesiumJS" account:
   - Visit https://ion.cesium.com.
   - Log in using your own `@cesium.com` email address.
   - Click your name in the top-right corner and choose "Switch to CesiumJS". If you don't see this option, ask someone on the ion team to add you to the CesiumJS account.
   - Click "Access Tokens".
   - Create a new token named "Cesium for Unity Samples vX.Y.Z - Delete on MONTH DAY, YEAR", using real values for the version number and date. The date should be the date of the release two months out, using the month's name rather than its number.
   - Enable _only_ the `assets:read` and `geocode` scopes on the new token.
   - Set the other properties as follows (these should be the defaults):
     - `Allow URLs`: "All Urls"
     - `Resources`: "All assets"
5. Open `Assets/CesiumSettings/Resources/CesiumIonServers/ion.cesium.com.asset` in a text editor and change the `defaultIonAccessToken` property to the new token you created above.
6. Commit the above changes but do _not_ push it yet. If you do, anyone trying to use cesium-unity-samples will be broken until the new version is published.

## Test the release candidate

1. Launch the project in the Unity Editor. When prompted, click `Enter Safe Mode`, since all of the Cesium objects and scripts will be missing because the specified package version doesn't exist yet.
2. Go to Window -> Package Manager. Click on the add icon in the top left corner and select "Add package from tarball". Navigate to where you extracted the tarball, then select it and click "Open".
3. Assuming there are no more compilation errors, Unity should automatically exit Safe Mode once it processes the package. You should see Cesium for Unity installed in the Package Manager window.
4. Open each scene in Assets -> CesiumForUnitySamples -> Scenes and verify it works correctly:
   * Does it open without crashing?
   * Does it look correct?
   * Press Play. Does it work as expected? The billboard in each level should give you a good idea of what to expect.
5. Test on other platforms and other versions of Unity if you can. If you can't (e.g., you don't have a Mac), post a message on Teams asking others to give it at least a quick smoke test.

If all of the above goes well, you're ready to release Cesium for Unity.

## Publish the release on GitHub

1. Tag the cesium-native release if you haven't already, and push the tag. See the [Cesium Native release guide](#native-release-process) for instructions.
2. Tag the cesium-unity release, and push the tag. Be sure you're tagging the exact commit that you tested.
   - `git tag -a v1.21.0 -m "v1.21.0 release"`
   - `git push origin v1.21.0`
3. Publish the release on GitHub.
   - Visit https://github.com/CesiumGS/cesium-unity/releases.
   - Find the Draft release you previously created and click its Edit button.
   - Select the tag you just created and pushed.
   - Double-check that the other details look good.
   - Click "Publish release".
4. Publish the reference documentation. A CI job automatically publishes the documentation to the web site at https://cesium.com/learn/cesium-unity/ref-doc/ when it is merged into the [cesium.com](https://github.com/CesiumGS/cesium-unity/tree/cesium.com) branch. So do the following:
   - `git checkout cesium.com`
   - `git pull --ff-only`
   - `git merge v1.21.0 --ff-only`
   - `git push`
   - `git checkout main`

## Update the Package Registry

Follow the instructions in the `README.md` file in the internal `cesium-unity-package-registry` repo to publish the new release to the package registry.

## Publish the Cesium for Unity Samples Project

1. Verify that the package registry is working.
   - Clean your Samples project directory: `git clean -d -f -f -x`
   - Open the project in Unity. Verify that it finds and downloads the new package successfully.
   - Note that it can take up to 15 minutes for the new package to be available after publishing it to the registry.
2. Push your changes to the Samples repo that you committed earlier.
3. Tag the Samples project release:
   - `git tag -a v1.21.0 -m "v1.21.0 release"`
   - `git push origin v1.21.0`
4. Create a new release on GitHub: https://github.com/CesiumGS/cesium-unity-samples/releases/new. Select the tag you created above. Add a changelog in the body to describe recent updates. Follow the format used in previous release.
5. Publish the release as a _draft_, and then go the [releases](https://github.com/CesiumGS/cesium-unity-samples/releases). Download the "Source Code (zip)" asset. Rename it to `CesiumForUnitySamples-v1.21.0.zip`, replacing `v1.21.0` with the correct version number for this release.
5. Edit the draft release and attach the renamed ZIP file to it.
6. Publish the release.
