using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    [FilePath("Cesium/CesiumRuntimeSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class CesiumRuntimeSettings : ScriptableObject
    {
        private static readonly string _filePath = "Assets/Settings/Cesium/CesiumRuntimeSettings.asset";

        [SerializeField]
        private string _defaultAccessTokenId;

        [SerializeField]
        private string _defaultAccessToken;

        internal static CesiumRuntimeSettings GetSettings()
        {
            CesiumRuntimeSettings settings =
                AssetDatabase.LoadAssetAtPath<CesiumRuntimeSettings>(_filePath);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<CesiumRuntimeSettings>();
                settings._defaultAccessTokenId = "";
                settings._defaultAccessToken = "";
                AssetDatabase.CreateAsset(settings, _filePath);
                AssetDatabase.SaveAssetIfDirty(settings);
            }

            return settings;
        }

        public static bool HasDefaultIonAccessTokenId()
        {
            return false;/*
            CesiumRuntimeSettings settings = CesiumRuntimeSettings.GetSettings();
            return string.IsNullOrEmpty(settings._defaultAccessTokenId);*/
        }

        public static bool HasDefaultIonAccessToken()
        {
            return false;/*
            CesiumRuntimeSettings settings = CesiumRuntimeSettings.GetSettings();
            return string.IsNullOrEmpty(settings._defaultAccessToken);*/
        }

        public static string GetDefaultIonAccessTokenId()
        {
            return "";/*
            CesiumRuntimeSettings settings = CesiumRuntimeSettings.GetSettings();
            return settings._defaultAccessTokenId;*/
        }

        public static string GetDefaultIonAccessToken()
        {
            return "";/*
            CesiumRuntimeSettings settings = CesiumRuntimeSettings.GetSettings();
            return settings._defaultAccessToken;*/
        }

        public static void SetDefaultIonAccessTokenId(string id)
        {
            /*CesiumRuntimeSettings settings = CesiumRuntimeSettings.GetSettings();
            settings._defaultAccessTokenId = id;
            AssetDatabase.SaveAssetIfDirty(settings);*/
        }

        public static void SetDefaultIonAccessToken(string token)
        {
            /*CesiumRuntimeSettings settings = CesiumRuntimeSettings.GetSettings();
            settings._defaultAccessToken = token;
            AssetDatabase.SaveAssetIfDirty(settings);*/
        }
    }
}