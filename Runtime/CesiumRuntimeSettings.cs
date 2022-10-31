using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    public sealed class CesiumRuntimeSettings : ScriptableObject
    {
        private static readonly string _filePath =
            "Assets/Settings/Resources/CesiumRuntimeSettings.asset";

        private static CesiumRuntimeSettings? _instance;

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
                    if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Settings");
                    }

                    if (!AssetDatabase.IsValidFolder("Assets/Settings/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets/Settings", "Resources");
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
                                    "the Assets/Settings/Resources folder and has been moved " +
                                    "appropriately.");

                                return _instance;
                            }
                            else
                            {
                                Debug.LogWarning("A CesiumRuntimeSettings asset was found outside " +
                                    "the Assets/Settings/Resources folder, but could not be moved " +
                                    "to the appropriate location. A new settings asset will be " +
                                    "created instead.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("An invalid CesiumRuntimeSettings asset was found " +
                                "outside the Assets/Settings/Resources folder. A new settings " +
                                "asset will be created instead.");
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
                    Debug.LogError("Cannot find a CesiumRuntimeSettings asset " +
                        "Any assets that use the project's default token will not load.");
                    #endif
                }

                return _instance;
            }
        }

        [SerializeField]
        private string _defaultIonAccessTokenID = "";

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
    }
}