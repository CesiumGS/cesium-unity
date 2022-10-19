
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    public class IonTokenSelector
    {
        private static readonly string _defaultTokenIdEditorKey =
            "CesiumIonDefaultAccessTokenId";
        private static readonly string _defaultTokenEditorKey =
            "CesiumIonDefaultAccessToken";

        public static bool HasDefaultTokenId()
        {
            return EditorPrefs.HasKey(_defaultTokenIdEditorKey);
        }

        public static bool HasDefaultToken()
        {
            return EditorPrefs.HasKey(_defaultTokenEditorKey);
        }

        public static string GetDefaultTokenId()
        {
            return EditorPrefs.GetString(_defaultTokenIdEditorKey);
        }

        public static string GetDefaultToken()
        {
            return EditorPrefs.GetString(_defaultTokenEditorKey);
        }

        public static void SetDefaultTokenId(string id)
        {
            EditorPrefs.SetString(_defaultTokenIdEditorKey, id);
        }

        public static void SetDefaultToken(string token)
        {
            EditorPrefs.SetString(_defaultTokenEditorKey, token);
        }

        public static string GetDefaultNewTokenName()
        {
            return Application.productName + " (Created by Cesium For Unity)";
        }
    }
}