using System;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [InitializeOnLoad]
    public static class CesiumEditorUtility
    {
        static CesiumEditorUtility()
        {
            EditorApplication.update += UpdateIonSession;

            Cesium3DTileset.OnCesium3DTilesetLoadFailure +=
                HandleCesium3DTilesetLoadFailure;
            CesiumRasterOverlay.OnCesiumRasterOverlayLoadFailure +=
                HandleCesiumRasterOverlayLoadFailure;
        }

        static void UpdateIonSession()
        {
            try
            {
                CesiumIonSession.Ion().Tick();
            }
            // Don't let a missing / out-of-sync native DLL crash everything.
            catch (DllNotFoundException)
            {
            }
            catch (TypeInitializationException)
            {
            }
        }

        static void
        HandleCesium3DTilesetLoadFailure(Cesium3DTilesetLoadFailureDetails details)
        {
            if (details.tileset == null)
            {
                return;
            }

            // Don't open a troubleshooting panel during play mode.
            if (EditorApplication.isPlaying)
            {
                return;
            }

            // Check for a 401 connecting to Cesium ion, which means the token is invalid
            // (or perhaps the asset ID is). Also check for a 404, because ion returns 404
            // when the token is valid but not authorized for the asset.
            if (details.type == Cesium3DTilesetLoadType.CesiumIon
                && (details.httpStatusCode == 401 || details.httpStatusCode == 404))
            {
                IonTokenTroubleshootingWindow.ShowWindow(details.tileset, true);
            }
        }

        static void
        HandleCesiumRasterOverlayLoadFailure(CesiumRasterOverlayLoadFailureDetails details)
        {
            if (details.overlay == null)
            {
                return;
            }

            // Don't open a troubleshooting panel during play mode.
            if (EditorApplication.isPlaying)
            {
                return;
            }

            // Check for a 401 connecting to Cesium ion, which means the token is invalid
            // (or perhaps the asset ID is). Also check for a 404, because ion returns 404
            // when the token is valid but not authorized for the asset.
            if (details.type == CesiumRasterOverlayLoadType.CesiumIon
                && (details.httpStatusCode == 401 || details.httpStatusCode == 404))
            {
                IonTokenTroubleshootingWindow.ShowWindow(details.overlay, true);
            }
            else
            {
                Debug.Log(details.message);
            }
        }

        public static Cesium3DTileset FindFirstTileset()
        {
            Cesium3DTileset[] tilesets =
                UnityEngine.Object.FindObjectsOfType<Cesium3DTileset>(true);
            for (int i = 0; i < tilesets.Length; i++)
            {
                Cesium3DTileset tileset = tilesets[i];
                if (tileset != null)
                {
                    return tileset;
                }
            }

            return null;
        }

        public static Cesium3DTileset FindFirstTilesetWithAssetID(long assetID)
        {
            Cesium3DTileset[] tilesets =
                UnityEngine.Object.FindObjectsOfType<Cesium3DTileset>(true);
            for (int i = 0; i < tilesets.Length; i++)
            {
                Cesium3DTileset tileset = tilesets[i];
                if (tileset != null && tileset.ionAssetID == assetID)
                {
                    return tileset;
                }
            }

            return null;
        }

        public static CesiumGeoreference FindFirstGeoreference()
        {
            CesiumGeoreference[] georeferences =
               UnityEngine.Object.FindObjectsOfType<CesiumGeoreference>(true);
            for (int i = 0; i < georeferences.Length; i++)
            {
                CesiumGeoreference georeference = georeferences[i];
                if (georeference != null)
                {
                    return georeference;
                }
            }

            return null;
        }

        public static Cesium3DTileset CreateTileset(string name, long assetID)
        {
            // Find a georeference in the scene, or create one if none exist.
            CesiumGeoreference georeference = CesiumEditorUtility.FindFirstGeoreference();
            if (georeference == null)
            {
                GameObject georeferenceGameObject = new GameObject("CesiumGeoreference");
                georeference =
                    georeferenceGameObject.AddComponent<CesiumGeoreference>();
                Undo.RegisterCreatedObjectUndo(georeferenceGameObject, "Create Georeference");
            }

            GameObject tilesetGameObject = new GameObject(name);
            tilesetGameObject.transform.SetParent(georeference.gameObject.transform);

            Cesium3DTileset tileset = tilesetGameObject.AddComponent<Cesium3DTileset>();
            tileset.name = name;
            tileset.ionAssetID = assetID;

            Undo.RegisterCreatedObjectUndo(tilesetGameObject, "Create Tileset");

            return tileset;
        }

        public static CesiumIonRasterOverlay
            AddBaseOverlayToTileset(Cesium3DTileset tileset, long assetID)
        {
            GameObject gameObject = tileset.gameObject;
            CesiumRasterOverlay overlay = gameObject.GetComponent<CesiumRasterOverlay>();
            if (overlay != null)
            {
                Undo.DestroyObjectImmediate(overlay);
            }

            CesiumIonRasterOverlay ionOverlay = Undo.AddComponent<CesiumIonRasterOverlay>(gameObject);
            ionOverlay.ionAssetID = assetID;

            return ionOverlay;
        }

        private static double3
        TransformCameraPositionToEarthCenteredEarthFixed(CesiumGeoreference georeference)
        {
            Camera camera = SceneView.lastActiveSceneView.camera;

            // Find the camera position in the Georeference's reference frame.
            Vector3 position = camera.transform.position;
            position = georeference.transform.worldToLocalMatrix *
                new Vector4(position.x, position.y, position.z, 1.0f);

            double3 positionUnity = new double3(
                position.x,
                position.y,
                position.z
            );

            return georeference.TransformUnityPositionToEarthCenteredEarthFixed(
                positionUnity);
        }

        private static void SetSceneViewPositionRotation(
            Vector3 position,
            Quaternion rotation)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            sceneView.pivot =
                position + sceneView.camera.transform.forward * sceneView.cameraDistance;
            sceneView.rotation = rotation;
            SceneView.lastActiveSceneView.Repaint();
        }

        public static void PlaceGeoreferenceAtCameraPosition(CesiumGeoreference georeference)
        {
            Undo.RecordObject(georeference, "Place Georeference Origin at Camera Position");

            // Disable all sub-scenes before repositioning the georeference.
            CesiumSubScene[] subScenes =
                georeference.gameObject.GetComponentsInChildren<CesiumSubScene>();
            for (int i = 0; i < subScenes.Length; i++)
            {
                subScenes[i].gameObject.SetActive(false);
            }

            // Want to restore current forward direction, relative to the globe.
            // Remember that "the globe" in Unity world coordinates is defined by both the georeference origin and its Transform.
            Vector3 forward = SceneView.lastActiveSceneView.camera.transform.forward;
            forward = georeference.transform.worldToLocalMatrix * new Vector4(forward.x, forward.y, forward.z, 0.0f);
            double3 cameraForwardEcef =
                georeference.TransformUnityDirectionToEarthCenteredEarthFixed(
                  new double3(
                    forward.x,
                    forward.y,
                    forward.z));

            double3 positionECEF =
                CesiumEditorUtility.TransformCameraPositionToEarthCenteredEarthFixed(georeference);
            georeference.SetOriginEarthCenteredEarthFixed(
                positionECEF.x,
                positionECEF.y,
                positionECEF.z);

            double3 newCameraForwardUnity =
                georeference.TransformEarthCenteredEarthFixedDirectionToUnity(cameraForwardEcef);

            // Teleport the camera back to the georeference's position so it stays
            // at the middle of the subscene. Restore the original forward direction, wrt the globe.
            // Always use +Y (Vector3.up) as the up direction, even if a Transform on the CesiumGeoreference tilts the globe.
            CesiumEditorUtility.SetSceneViewPositionRotation(
                georeference.transform.position,
                Quaternion.LookRotation(
                    georeference.transform.localToWorldMatrix * new Vector4(
                      (float)newCameraForwardUnity.x,
                      (float)newCameraForwardUnity.y,
                      (float)newCameraForwardUnity.z,
                      0.0f),
                    Vector3.up));
        }

        public static CesiumSubScene CreateSubScene(CesiumGeoreference georeference)
        {
            CesiumEditorUtility.PlaceGeoreferenceAtCameraPosition(georeference);
            double3 positionECEF = new double3(
                georeference.ecefX,
                georeference.ecefY,
                georeference.ecefZ
            );

            GameObject subSceneGameObject = new GameObject("New Sub-Scene");
            subSceneGameObject.transform.parent = georeference.transform;
            Undo.RegisterCreatedObjectUndo(subSceneGameObject, "Create Sub-Scene");

            CesiumSubScene subScene = subSceneGameObject.AddComponent<CesiumSubScene>();
            subScene.SetOriginEarthCenteredEarthFixed(
                positionECEF.x,
                positionECEF.y,
                positionECEF.z);

            // Prompt the user to rename the subscene once the hierarchy has updated.
            Selection.activeGameObject = subSceneGameObject;
            EditorApplication.hierarchyChanged += RenameObject;

            return subScene;
        }

        public static void PlaceSubSceneAtCameraPosition(CesiumSubScene subscene)
        {
            CesiumGeoreference georeference =
                subscene.gameObject.GetComponentInParent<CesiumGeoreference>();
            if (georeference == null)
            {
                throw new InvalidOperationException("CesiumSubScene is not nested inside a game " +
                    "object with a CesiumGeoreference.");
            }

            Undo.RecordObject(subscene, "Place Sub-Scene Origin at Camera Position");

            double3 positionECEF =
                CesiumEditorUtility.TransformCameraPositionToEarthCenteredEarthFixed(georeference);
            subscene.SetOriginEarthCenteredEarthFixed(
                    positionECEF.x,
                    positionECEF.y,
                    positionECEF.z);
            CesiumEditorUtility.SetSceneViewPositionRotation(
                Vector3.zero, SceneView.lastActiveSceneView.rotation);
        }

        public static void RenameObject()
        {
            EditorApplication.hierarchyChanged -= RenameObject;
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            EditorApplication.ExecuteMenuItem("Edit/Rename");
        }

        public static CesiumCameraController CreateDynamicCamera()
        {
            CesiumCameraController dynamicCamera =
                UnityEngine.Object.FindObjectOfType<CesiumCameraController>(true);
            if (dynamicCamera != null)
            {
                return dynamicCamera;
            }

            // Find a georeference in the scene, or create one if none exist.
            CesiumGeoreference georeference = CesiumEditorUtility.FindFirstGeoreference();
            if (georeference == null)
            {
                GameObject georeferenceGameObject = new GameObject("CesiumGeoreference");
                georeference =
                    georeferenceGameObject.AddComponent<CesiumGeoreference>();
                Undo.RegisterCreatedObjectUndo(georeferenceGameObject, "Create Georeference");
            }

            GameObject dynamicCameraPrefab = Resources.Load<GameObject>("DynamicCamera");
            GameObject dynamicCameraObject =
                UnityEngine.Object.Instantiate(dynamicCameraPrefab);
            dynamicCameraObject.name = "DynamicCamera";
            dynamicCameraObject.transform.parent = georeference.gameObject.transform;

            Undo.RegisterCreatedObjectUndo(dynamicCameraObject, "Create DynamicCamera");

            return dynamicCameraObject.GetComponent<CesiumCameraController>();
        }

        private static Regex _findLineBreakSets = new Regex("(\r?\n)+[ \t]*");

        /// <summary>
        /// Apply some very basic formatting to the tooltip:
        /// - Trim whitespace from the beginning of the string and from the beginning of each new line
        /// - Remove single newlines, replace them with a space.
        /// - Leave double newlines, these start a new paragraph.
        /// </summary>
        /// <param name="tooltip">The original tooltip.</param>
        /// <returns>The formatted tooltip.</returns>
        public static string FormatTooltip(string tooltip)
        {
            return _findLineBreakSets.Replace(tooltip.Trim(), (match) =>
            {
                int newlineCount = 0;
                string matchString = match.Value;
                for (int i = 0; i < matchString.Length; ++i)
                {
                    if (matchString[i] == '\n')
                        ++newlineCount;
                }

                if (newlineCount == 1)
                    return " ";
                else // two or more newlines
                    return Environment.NewLine + Environment.NewLine;
            });
        }
    }
}
