using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// Holds Cesium settings used at runtime.
    /// </summary>
    public sealed class CesiumRuntimeSettings : ScriptableObject
    {
        private static readonly string _settingsName = "CesiumSettings";
        private static readonly string _filePath =
            "Assets/" + _settingsName + "/Resources/CesiumRuntimeSettings.asset";

        private static CesiumRuntimeSettings _instance;

        /// <summary>
        /// Gets the singleton instance of this class. If the project does not yet contain an instance,
        /// one is created and added at the specified file path.
        /// </summary>
        public static CesiumRuntimeSettings instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                #if UNITY_EDITOR
                _instance = AssetDatabase.LoadAssetAtPath(_filePath, typeof(CesiumRuntimeSettings))
                        as CesiumRuntimeSettings;
                #else
                _instance =
                    Resources.Load("CesiumRuntimeSettings") as CesiumRuntimeSettings;
                #endif

                #if UNITY_EDITOR
                if (_instance == null)
                {
                    // Create the necessary folders if they don't already exist.
                    if (!AssetDatabase.IsValidFolder("Assets/" + _settingsName))
                    {
                        AssetDatabase.CreateFolder("Assets", _settingsName);
                    }

                    if (!AssetDatabase.IsValidFolder("Assets/" + _settingsName + "/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets/" + _settingsName, "Resources");
                    }

                    string typeString = "t:"+ typeof(CesiumRuntimeSettings).Name;

                    string[] instanceGUIDS = AssetDatabase.FindAssets(typeString);

                    // If a CesiumRuntimeSettings asset is found outside of the preferred
                    // file path, move it to the correct location.
                    if (instanceGUIDS.Length > 0)
                    {
                        if (instanceGUIDS.Length > 1)
                        {
                            Debug.LogWarning("Found multiple CesiumRuntimeSettings assets " +
                                "in the project folder. The first asset found will be used.");
                        }
                        
                        string oldPath = AssetDatabase.GUIDToAssetPath(instanceGUIDS[0]);
                        _instance =
                                AssetDatabase.LoadAssetAtPath(oldPath, typeof(CesiumRuntimeSettings))
                                    as CesiumRuntimeSettings;
                        if(_instance != null)
                        {
                            string result = AssetDatabase.MoveAsset(oldPath, _filePath);
                            AssetDatabase.Refresh();
                            if (string.IsNullOrEmpty(result))
                            {
                                Debug.LogWarning("A CesiumRuntimeSettings asset was found outside " +
                                    "the Assets/" + _settingsName + "/Resources folder and has been moved " +
                                    "appropriately.");

                                return _instance;
                            }
                            else
                            {
                                Debug.LogWarning("A CesiumRuntimeSettings asset was found outside " +
                                    "the Assets/" + _settingsName + "/Resources folder, but could not " +
                                    "be moved to the appropriate location. A new settings asset will be " +
                                    "created instead.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("An invalid CesiumRuntimeSettings asset was found " +
                                "outside the Assets/" + _settingsName + "/Resources folder. " +
                                "A new settings asset will be created instead.");
                        }
                    }
                }
                #endif

                if (_instance == null)
                {
                    // Create an instance even if the game is not running in the editor
                    // to prevent a crash.
                    _instance = ScriptableObject.CreateInstance<CesiumRuntimeSettings>();
                    #if UNITY_EDITOR
                    AssetDatabase.CreateAsset(_instance, _filePath);
                    AssetDatabase.Refresh();
                    #else
                    Debug.LogError("Cannot find a CesiumRuntimeSettings asset. " +
                        "Any assets that use the project's default token will not load.");
                    #endif
                }

                return _instance;
            }
        }

        [SerializeField]
        private string _defaultIonAccessTokenID = "";

        /// <summary>
        /// The ID of the default Cesium ion access token to use within the project.
        /// </summary>
        public static string defaultIonAccessTokenID
        {
            get => instance._defaultIonAccessTokenID;
            #if UNITY_EDITOR
            set
            {
                instance._defaultIonAccessTokenID = value;
                EditorUtility.SetDirty(_instance);
                AssetDatabase.SaveAssetIfDirty(_instance);
                AssetDatabase.Refresh();
            }
            #endif
        }

        [SerializeField]
        private string _defaultIonAccessToken = "";

        /// <summary>
        /// The default Cesium ion access token value to use within the project.
        /// </summary>
        public static string defaultIonAccessToken
        {
            get => instance._defaultIonAccessToken;
            #if UNITY_EDITOR
            set
            {
                instance._defaultIonAccessToken = value;
                EditorUtility.SetDirty(_instance);
                AssetDatabase.SaveAssetIfDirty(_instance);
                AssetDatabase.Refresh();
            }
            #endif
        }

        [SerializeField]
        [Tooltip("The number of requests to handle before each prune of old cached results from the database. Must restart Unity to apply changes.")]
        private int _requestsPerCachePrune = 10000;

        /// <summary>
        ///  The number of requests to handle before each prune of old cached results from the database.
        /// </summary>
        public static int requestsPerCachePrune
        {
            get => instance._requestsPerCachePrune;
        }

        [SerializeField]
        [Tooltip("The maximum number of items should be kept in the Sqlite database after pruning. Must restart Unity to apply changes.")]
        private ulong  _maxItems = 4096;
        /// <summary>
        /// The maximum number of items should be kept in the Sqlite database after pruning.
        /// </summary>
        public static ulong maxItems
        {
            get => instance._maxItems;
        }
    }
}
