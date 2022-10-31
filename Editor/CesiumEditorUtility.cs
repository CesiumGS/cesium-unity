using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    public static class CesiumEditorUtility
    {
        public static Cesium3DTileset FindFirstTileset()
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

        public static Cesium3DTileset FindFirstTilesetWithAssetID(long assetID)
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

        public static CesiumGeoreference FindFirstGeoreference()
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
            CesiumGeoreference georeference = CesiumEditorUtility.FindFirstGeoreference();
            if(georeference == null) { 
                GameObject georeferenceGameObject =
                    new GameObject("CesiumGeoreference");
                georeference =
                    georeferenceGameObject.AddComponent<CesiumGeoreference>();
            }

            GameObject tilesetGameObject = new GameObject(name);
            tilesetGameObject.transform.SetParent(georeference.gameObject.transform);

            Cesium3DTileset tileset = tilesetGameObject.AddComponent<Cesium3DTileset>();
            tileset.ionAssetID = assetID;

            return tileset;
        }

        public static CesiumIonRasterOverlay
            AddBaseOverlayToTileset(Cesium3DTileset tileset, long assetID)
        {
            GameObject gameObject = tileset.gameObject;
            Undo.RecordObject(gameObject, "Add Base Overlay to Tileset");
            CesiumIonRasterOverlay overlay = gameObject.GetComponent<CesiumIonRasterOverlay>();
            if (overlay != null)
            {
                Object.DestroyImmediate(overlay);
            }

            overlay = gameObject.AddComponent<CesiumIonRasterOverlay>();
            overlay.ionAssetID = assetID;

            return overlay;
        }
    }
}
