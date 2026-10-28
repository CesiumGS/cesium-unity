# Releasing a new version of Cesium for Unity {#release-guide}

This is the process we follow when releasing a new version of Cesium for Unity on GitHub.
<!--! [TOC] -->

## Prepare Cesium Native for Release

[cesium-native](https://github.com/CesiumGS/cesium-native) is used as a git submodule of cesium-unity. So the first step in releasing Cesium for Unity is to make sure that Cesium Native is ready for release. See the [Cesium Native release guide](#native-release-process) for instructions. You may want to wait to create the new Cesium Native tag until after you have tested the Cesium for Unity release.

## Prepare Cesium for Unity for Release

1. Call the `/prepare-release` GitHub Copilot skill using the next version number (`../.github/skills/prepare-release/SKILL.md`). 
2. Review the resulting diff. In particular, confirm that the "from" and "to" version numbers in the cesium-native note match the cesium-native release you're actually shipping, since that release may not be tagged yet.
3. Commit these changes. You can push them directly to `main`.
4. Before continuing, verify that CI passes for all platforms.

## Optimistically upload the release packages to the GitHub releases page

Cesium for Unity's release package is fairly large, so it can take awhile to upload it to the GitHub Releases page. For that reason, it is helpful to start the process now, even though we haven't tested the release yet.

The `Draft Release` GitHub Actions workflow does this for you. It copies the package directly from the build artifacts to a draft release, so it never passes through your local machine.

1. Go to the [Draft Release workflow](https://github.com/CesiumGS/cesium-unity/actions/workflows/draftRelease.yml) and click "Run workflow".
2. Enter the release version, for example `1.26.0`, leave the other inputs at their defaults, and click "Run workflow".
   - The workflow publishes the most recent _successful_ build of `main`, so it will fail if CI hasn't passed yet.
   - To publish a different build, enter its run ID in the `run_id` input.
3. Wait for the workflow to finish. It will:
   - Download the combined package from that build, for example `com.cesium.unity-1.26.0.tgz`, and verify its name matches the requested release version.
   - Build the release notes from the `CHANGES.md` section for this version.
   - Create a draft release named "Cesium for Unity v1.26.0" with the package attached, tagged `v1.26.0` against the exact commit that was built. The tag itself is not created until you publish the release.
4. Visit the [releases page](https://github.com/CesiumGS/cesium-unity/releases), expand the "Assets" section of the new draft, and confirm that the package is attached and that the release notes look correct.
   - To preview the notes before running the workflow, use `npm run release-notes -- --version 1.26.0`.

If the uploaded package is later found to have problems during testing, re-run the workflow. It updates the existing draft in place, and refuses to modify the release once it has been published.

We used to download the package, extract it by hand, and then compare the SHA256 of the uploaded asset against the build artifact, to prove that it hadn't been modified while it was on a local machine. The workflow copies the package straight from the build artifact to the release, so there is no longer a local copy to verify.

## Update the "Cesium for Unity Samples" Project

1. Clone a fresh copy of [cesium-unity-samples](https://github.com/CesiumGS/cesium-unity-samples) to a new directory.
2. Before opening the project, go to the Packages folder and open `Packages/manifest.json` in a text editor. Find the `com.cesium.unity` dependency under the `"dependencies"` object and change the version to the version number of the in-progress release.
3. Open `ProjectSettings/ProjectSettings.asset` and find the `bundleVersion` key (under `PlayerSettings`). Set this to the version number of the in-progress release.
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
