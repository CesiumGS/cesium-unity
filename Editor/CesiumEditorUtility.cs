using System;
using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    [InitializeOnLoad]
    public static class CesiumEditorUtility
    {
        public static class InspectorGUI
        {
            public static void ClampedIntField(
                SerializedProperty property, int min, int max, GUIContent label)
            {
                if (property.propertyType == SerializedPropertyType.Integer)
                {
                    int value = EditorGUILayout.IntField(label, property.intValue);
                    property.intValue = Math.Clamp(value, min, max);
                }
                else
                {
                    EditorGUILayout.LabelField(
                        label.text, "Use ClampedIntField for int only.");
                }
            }

            public static void ClampedFloatField(
                SerializedProperty property, float min, float max, GUIContent label)
            {
                if (property.propertyType == SerializedPropertyType.Float)
                {
                    float value = EditorGUILayout.FloatField(label, property.floatValue);
                    property.floatValue = Math.Clamp(value, min, max);
                }
                else
                {
                    EditorGUILayout.LabelField(
                        label.text, "Use ClampedFloatField for float only.");
                }
            }

            public static void ClampedDoubleField(
                SerializedProperty property, double min, double max, GUIContent label)
            {
                // SerializedPropertyType.Float is used for both float and double;
                // SerializedPropertyType.Double does not exist.
                if (property.propertyType == SerializedPropertyType.Float)
                {
                    double value = EditorGUILayout.DoubleField(label, property.doubleValue);
                    property.doubleValue = Math.Clamp(value, min, max);
                }
                else
                {
                    EditorGUILayout.LabelField(
                        label.text, "Use ClampedDoubleField for double only.");
                }
            }
        }

        static CesiumEditorUtility()
        {
            Cesium3DTileset.OnCesium3DTilesetLoadFailure +=
                HandleCesium3DTilesetLoadFailure;
            CesiumRasterOverlay.OnCesiumRasterOverlayLoadFailure +=
                HandleCesiumRasterOverlayLoadFailure;
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

            Debug.Log(details.message);
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

            Debug.Log(details.message);
        }

        public static Cesium3DTileset? FindFirstTileset()
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

        public static Cesium3DTileset? FindFirstTilesetWithAssetID(long assetID)
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

        public static CesiumGeoreference? FindFirstGeoreference()
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
            // Find a georeference in the scene, or create one if none exists.
            CesiumGeoreference? georeference = CesiumEditorUtility.FindFirstGeoreference();
            if (georeference == null)
            {
                GameObject georeferenceGameObject =
                    new GameObject("CesiumGeoreference");
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
            CesiumIonRasterOverlay overlay = gameObject.GetComponent<CesiumIonRasterOverlay>();
            if (overlay != null)
            {
                Undo.RecordObject(overlay, "Update Base Overlay of Tileset");
            }
            else
            {
                overlay = Undo.AddComponent<CesiumIonRasterOverlay>(gameObject);
            }

            overlay.ionAssetID = assetID;

            return overlay;
        }

        private static CesiumVector3
            TransformCameraPositionToEarthCenteredEarthFixed(CesiumGeoreference georeference)
        {
            Vector3 pivot = SceneView.lastActiveSceneView.pivot;
            CesiumVector3 positionUnity = new CesiumVector3()
            {
                x = pivot.x,
                y = pivot.y,
                z = pivot.z
            };

            return georeference.TransformUnityWorldPositionToEarthCenteredEarthFixed(positionUnity);
        }

        public static void
            PlaceGeoreferenceAtCameraPosition(CesiumGeoreference georeference)
        {
            Undo.RecordObject(georeference, "Place Georeference at Camera Position");
            CesiumVector3 positionECEF =
                CesiumEditorUtility.TransformCameraPositionToEarthCenteredEarthFixed(georeference);
            georeference.SetOriginEarthCenteredEarthFixed(
                positionECEF.x,
                positionECEF.y,
                positionECEF.z);

            SceneView.lastActiveSceneView.pivot = Vector3.zero;
            SceneView.lastActiveSceneView.Repaint();
        }

        public static CesiumSubScene CreateSubScene(CesiumGeoreference georeference)
        {
            GameObject subSceneGameObject = new GameObject();
            subSceneGameObject.transform.parent = georeference.gameObject.transform;
            Undo.RegisterCreatedObjectUndo(subSceneGameObject, "Create Sub-Scene");

            CesiumSubScene subScene = subSceneGameObject.AddComponent<CesiumSubScene>();
            CesiumVector3 positionECEF =
                CesiumEditorUtility.TransformCameraPositionToEarthCenteredEarthFixed(georeference);
            subScene.SetOriginEarthCenteredEarthFixed(
                positionECEF.x,
                positionECEF.y,
                positionECEF.z);

            Selection.activeGameObject = subSceneGameObject;

            // Prompt the user to rename the subscene once the hierarchy has updated.
            EditorApplication.hierarchyChanged += RenameObject;

            return subScene;
        }

        public static void RenameObject()
        {
            EditorApplication.hierarchyChanged -= RenameObject;
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            EditorApplication.ExecuteMenuItem("Edit/Rename");
        }
    }
}
