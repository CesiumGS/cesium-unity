using CesiumForUnity;
using NUnit.Framework;
using UnityEngine;

public class TestIonTokenTroubleshootingWindow
{
    [Test]
    public void ShowWindowForGeoJsonDocumentRasterOverlayDoesNotThrow()
    {
        GameObject go = new GameObject("GeoJsonOverlay");
        CesiumGeoJsonDocumentRasterOverlay overlay = go.AddComponent<CesiumGeoJsonDocumentRasterOverlay>();

        try
        {
            Assert.DoesNotThrow(() =>
            {
                IonTokenTroubleshootingWindow.ShowWindow(overlay, false);
            });

            CesiumIonAsset asset = new CesiumIonAsset(overlay);
            Assert.IsFalse(asset.IsNull());
            Assert.AreEqual(overlay, asset.geoJsonOverlay);
            Assert.AreEqual(overlay.ionServer, asset.geoJsonOverlay.ionServer);
        }
        finally
        {
            IonTokenTroubleshootingWindow[] openWindows = Resources.FindObjectsOfTypeAll<IonTokenTroubleshootingWindow>();
            foreach (IonTokenTroubleshootingWindow window in openWindows)
            {
                window.Close();
            }

            Object.DestroyImmediate(go);
        }
    }
}
