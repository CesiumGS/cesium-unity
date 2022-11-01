using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    [InitializeOnLoad]
    public static class CesiumEditorUtility
    {
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
        }

        public static Cesium3DTileset? FindFirstTileset()
        {
            Cesium3DTileset[] tilesets =
                Object.FindObjectsOfType<Cesium3DTileset>(true);
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
                Object.FindObjectsOfType<Cesium3DTileset>(true);
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
               Object.FindObjectsOfType<CesiumGeoreference>(true);
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
    }
}
