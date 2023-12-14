using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Defines a Cesium ion Server. This may be the public (SaaS) Cesium ion server at
    /// ion.cesium.com, or it may be a self-hosted instance.
    /// </summary>
    [CreateAssetMenu(fileName = "CesiumIonServer", menuName = "Cesium/Cesium ion Server")]
    [AddComponentMenu("Cesium/Cesium ion Server")]
    public class CesiumIonServer : ScriptableObject
    {
        /// <summary>
        /// The main URL of the Cesium ion server. For example, the server URL for the
        /// public Cesium ion is https://ion.cesium.com.
        /// </summary>
        public string serverUrl = "https://ion.cesium.com";

        /// <summary>
        /// The URL of the main API endpoint of the Cesium ion server. For example, for
        /// the default, public Cesium ion server, this is `https://api.cesium.com`. If
        /// left blank, the API URL is automatically inferred from the Server URL.
        /// </summary>
        public string apiUrl = "https://api.cesium.com";

        /// <summary>
        /// The application ID to use to log in to this server using OAuth2. This
        /// OAuth2 application must be configured on the server with the exact URL
        /// `http://127.0.0.1/cesium-for-unreal/oauth2/callback`.
        /// </summary>
        public long oauth2ApplicationID = 381;

        /// <summary>
        /// The ID of the default access token to use to access Cesium ion assets at
        /// runtime. This property may be an empty string, in which case the ID is
        /// found by searching the logged-in Cesium ion account for the
        /// DefaultIonAccessToken.
        /// </summary>
        public string defaultIonAccessTokenId;

        /// <summary>
        /// The default token used to access Cesium ion assets at runtime. This token
        /// is embedded in packaged games for use at runtime.
        /// </summary>
        public string defaultIonAccessToken;

        // The server URL for which we are currently attempting to deduce an API URL. When this property
        // is null, we're not currently loading a config.json in order to deduce an API URL for the server.
        // When this property is different from `serverUrl`, a config.json load is in progress, but it's
        // obsolete.
        internal string serverUrlThatIsLoadingApiUrl = null;

        /// <summary>
        /// Gets the default Cesium ion Server (ion.cesium.com).
        /// </summary>
        /// <remarks>
        /// It is expected to be found at `Assets/CesiumSettings/CesiumIonServers/ion.cesium.com`.
        /// In the Editor, it will be created if it does not already exist, so this method always
        /// returns a valid instance. At runtime, this method returns null if the object does not
        /// exist.
        /// </remarks>
        public static CesiumIonServer defaultServer
        {
            get
            {
#if UNITY_EDITOR
                CesiumIonServer result = AssetDatabase.LoadAssetAtPath(
                    "Assets/CesiumSettings/Resources/CesiumIonServers/ion.cesium.com.asset",
                    typeof(CesiumIonServer)) as CesiumIonServer;
#else
                CesiumIonServer result = Resources.Load<CesiumIonServer>("CesiumIonServers/ion.cesium.com");
#endif

#if UNITY_EDITOR
                if (result == null)
                {
                    if (!AssetDatabase.IsValidFolder("Assets/CesiumSettings"))
                        AssetDatabase.CreateFolder("Assets", "CesiumSettings");
                    if (!AssetDatabase.IsValidFolder("Assets/CesiumSettings/Resources"))
                        AssetDatabase.CreateFolder("Assets/CesiumSettings", "Resources");
                    if (!AssetDatabase.IsValidFolder("Assets/CesiumSettings/Resources/CesiumIonServers"))
                        AssetDatabase.CreateFolder("Assets/CesiumSettings/Resources", "CesiumIonServers");
                    result = ScriptableObject.CreateInstance<CesiumIonServer>();

#pragma warning disable 618
                    if (!string.IsNullOrEmpty(CesiumRuntimeSettings.defaultIonAccessTokenID))
                    {
                        if (string.IsNullOrEmpty(result.defaultIonAccessTokenId))
                        {
                            result.defaultIonAccessTokenId = CesiumRuntimeSettings.defaultIonAccessTokenID;
                        }
                        CesiumRuntimeSettings.defaultIonAccessTokenID = "";
                    }

                    if (!string.IsNullOrEmpty(CesiumRuntimeSettings.defaultIonAccessToken))
                    {
                        if (string.IsNullOrEmpty(result.defaultIonAccessToken))
                        {
                            result.defaultIonAccessToken = CesiumRuntimeSettings.defaultIonAccessToken;
                        }
                        CesiumRuntimeSettings.defaultIonAccessToken = "";
                    }

#pragma warning restore 618

                    AssetDatabase.CreateAsset(result, "Assets/CesiumSettings/Resources/CesiumIonServers/ion.cesium.com.asset");
                    AssetDatabase.Refresh();
                }
#endif

                return result;
            }
        }

        /// <summary>
        /// Gets the current Cesium ion server that should be assigned to newly-created objects.
        /// </summary>
        public static CesiumIonServer serverForNewObjects
        {
            get
            {
                if (_serverForNewObjects == null)
                    return defaultServer;
                else
                    return _serverForNewObjects;
            }
            set
            {
                _serverForNewObjects = value;
            }
        }

        private static CesiumIonServer _serverForNewObjects;
    }
}
