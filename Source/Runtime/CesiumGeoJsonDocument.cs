using Reinterop;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents a parsed GeoJSON document.
    /// </summary>
    /// <remarks>
    /// A GeoJSON document contains a hierarchy of GeoJSON objects starting
    /// with a root object. This class provides methods to load GeoJSON from
    /// various sources and access the parsed content.
    /// </remarks>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumGeoJsonDocumentImpl",
        "CesiumGeoJsonDocumentImpl.h")]
    public partial class CesiumGeoJsonDocument
    {
        /// <summary>
        /// Internal constructor used by native code.
        /// </summary>
        internal CesiumGeoJsonDocument()
        {
            CreateImplementation();
        }
        /// <summary>
        /// Checks whether this GeoJSON document is valid.
        /// </summary>
        /// <returns>True if the document is valid and has been successfully parsed; false otherwise.</returns>
        public partial bool IsValid();

        /// <summary>
        /// Gets the root object of this GeoJSON document.
        /// </summary>
        /// <returns>The root GeoJSON object, or null if the document is invalid.</returns>
        public partial CesiumGeoJsonObject GetRootObject();

        /// <summary>
        /// Parses a GeoJSON document from a string.
        /// </summary>
        /// <param name="geoJsonString">The GeoJSON string to parse.</param>
        /// <returns>A new CesiumGeoJsonDocument, or null if parsing failed.</returns>
        public static CesiumGeoJsonDocument Parse(string geoJsonString)
        {
            var document = new CesiumGeoJsonDocument();
            if (document.ParseInternal(geoJsonString))
            {
                return document;
            }
            return null;
        }

        /// <summary>
        /// Internal method to parse GeoJSON string into this document.
        /// </summary>
        internal partial bool ParseInternal(string geoJsonString);

        /// <summary>
        /// Loads a GeoJSON document from a URL asynchronously.
        /// </summary>
        /// <param name="url">The URL to load the GeoJSON from.</param>
        /// <returns>A task that resolves to the loaded CesiumGeoJsonDocument, or null if loading failed.</returns>
        public static async Task<CesiumGeoJsonDocument> LoadFromUrlAsync(string url)
        {
            var tcs = new TaskCompletionSource<CesiumGeoJsonDocument>();
            LoadFromUrl(url, (document) =>
            {
                tcs.SetResult(document);
            });
            return await tcs.Task;
        }

        /// <summary>
        /// Loads a GeoJSON document from a URL.
        /// </summary>
        /// <param name="url">The URL to load the GeoJSON from.</param>
        /// <param name="callback">A callback that will be invoked when loading completes.</param>
        private static partial void LoadFromUrl(string url, Action<CesiumGeoJsonDocument> callback);

        /// <summary>
        /// Loads a GeoJSON document from Cesium ion asynchronously.
        /// </summary>
        /// <param name="ionAssetId">The Cesium ion asset ID.</param>
        /// <param name="ionAccessToken">The Cesium ion access token. If empty, the default token will be used.</param>
        /// <param name="ionServer">The Cesium ion server to use. If null, the default server will be used.</param>
        /// <returns>A task that resolves to the loaded CesiumGeoJsonDocument, or null if loading failed.</returns>
        public static async Task<CesiumGeoJsonDocument> LoadFromCesiumIonAsync(
            long ionAssetId,
            string ionAccessToken = "",
            CesiumIonServer ionServer = null)
        {
            var tcs = new TaskCompletionSource<CesiumGeoJsonDocument>();
            string serverUrl = ionServer != null ? ionServer.apiUrl : CesiumIonServer.defaultServer.apiUrl;
            string token = !string.IsNullOrEmpty(ionAccessToken)
                ? ionAccessToken
                : (ionServer != null ? ionServer.defaultIonAccessToken : CesiumIonServer.defaultServer.defaultIonAccessToken);

            LoadFromCesiumIon(ionAssetId, token, serverUrl, (document) =>
            {
                tcs.SetResult(document);
            });
            return await tcs.Task;
        }

        /// <summary>
        /// Loads a GeoJSON document from Cesium ion.
        /// </summary>
        /// <param name="ionAssetId">The Cesium ion asset ID.</param>
        /// <param name="ionAccessToken">The Cesium ion access token.</param>
        /// <param name="ionApiUrl">The Cesium ion API URL.</param>
        /// <param name="callback">A callback that will be invoked when loading completes.</param>
        private static partial void LoadFromCesiumIon(
            long ionAssetId,
            string ionAccessToken,
            string ionApiUrl,
            Action<CesiumGeoJsonDocument> callback);

        /// <summary>
        /// Releases native resources associated with this GeoJSON document.
        /// </summary>
        internal partial void DisposeNative();

        /// <summary>
        /// Finalizer to release native resources.
        /// </summary>
        ~CesiumGeoJsonDocument()
        {
            DisposeNative();
        }
    }
}
