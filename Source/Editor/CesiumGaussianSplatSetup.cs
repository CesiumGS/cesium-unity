#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace CesiumForUnity
{
    /// <summary>
    /// Detects and fixes the render pipeline configuration required by the
    /// Gsplat package to render Gaussian splats.
    /// </summary>
    [InitializeOnLoad]
    public static class CesiumGaussianSplatSetup
    {
        public enum RenderPipelineType
        {
            BuiltIn,
            Universal,
            HighDefinition
        }

        private const string _gsplatAssemblyName = "Gsplat";
        private const string _gsplatUrpFeatureTypeName = "Gsplat.GsplatURPFeature";
        private const string _gsplatHdrpPassTypeName = "Gsplat.GsplatHDRPPass";

        private const string _urpAssetTypeName =
            "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset";
        private const string _hdrpAssetTypeName =
            "UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset";

        private const string _hdrpAssemblyName = "Unity.RenderPipelines.HighDefinition.Runtime";
        private const string _customPassVolumeTypeName =
            "UnityEngine.Rendering.HighDefinition.CustomPassVolume";
        private const string _customPassInjectionPointTypeName =
            "UnityEngine.Rendering.HighDefinition.CustomPassInjectionPoint";
        private const string _beforeTransparentInjectionPoint = "BeforeTransparent";

        private const string _neverAskPrefKey =
            "CesiumForUnity.GaussianSplats.NeverAskToConfigureRenderPipeline";
        private const string _urpDeclinedPrefKeyPrefix =
            "CesiumForUnity.GaussianSplats.DeclinedUrpSetup.";
        private const string _hdrpDeclinedSessionKey =
            "CesiumForUnity.GaussianSplats.DeclinedHdrpSetup";

        private const string _dialogTitle = "Cesium for Unity: Gaussian Splat Support";

        // Render backends supported by gsplat-unity.
        public static readonly GraphicsDeviceType[] supportedGraphicsDeviceTypes =
        {
            GraphicsDeviceType.Direct3D12,
            GraphicsDeviceType.Metal,
            GraphicsDeviceType.Vulkan
        };

        private static bool _isPromptQueued = false;

        static CesiumGaussianSplatSetup()
        {
            QueuePrompt();
            EditorSceneManager.sceneOpened += (scene, mode) => QueuePrompt();
            EditorSceneManager.newSceneCreated += (scene, setup, mode) => QueuePrompt();
            RenderPipelineManager.activeRenderPipelineTypeChanged += QueuePrompt;
        }

        public static RenderPipelineType GetActiveRenderPipelineType()
        {
            return GetRenderPipelineType(GraphicsSettings.currentRenderPipeline);
        }

        public static bool IsCurrentGraphicsDeviceSupported()
        {
            return IsGraphicsDeviceTypeSupported(SystemInfo.graphicsDeviceType);
        }

        public static bool IsGraphicsDeviceTypeSupported(GraphicsDeviceType deviceType)
        {
            return Array.IndexOf(supportedGraphicsDeviceTypes, deviceType) >= 0;
        }

        public static GraphicsDeviceType[] GetBuildTargetGraphicsDeviceTypes(BuildTarget target)
        {
            return PlayerSettings.GetGraphicsAPIs(target) ?? new GraphicsDeviceType[0];
        }

        public static string GetBuildTargetGraphicsApiWarning(out bool isError)
        {
            isError = false;

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            GraphicsDeviceType[] apis = GetBuildTargetGraphicsDeviceTypes(target);
            if (apis.Length == 0)
            {
                return null;
            }

            GraphicsDeviceType[] unsupported = apis.Where(api => !IsGraphicsDeviceTypeSupported(api)).ToArray();
            if (unsupported.Length == 0)
            {
                return null;
            }

            string supportedList = string.Join(", ", supportedGraphicsDeviceTypes);
            string unsupportedList = string.Join(", ", unsupported);

            if (unsupported.Length == apis.Length)
            {
                isError = true;
                return $"None of the graphics APIs configured for the {target} build target ({unsupportedList}) support Gaussian splats, so they " +
                    $"will not be rendered in builds. Supported graphics APIs are: {supportedList}.";
            }

            if (!IsGraphicsDeviceTypeSupported(apis[0]))
            {
                isError = true;
                return $"The primary graphics API for the {target} build target ({apis[0]}) does not support Gaussian splats, so they will not be rendered " +
                    $"in builds by default. Move a supported graphics API ({supportedList}) to the top of the list in the Player Settings.";
            }

            return $"Some fallback graphics APIs for the {target} build target ({unsupportedList}) do not support Gaussian splats. Devices that fall back to " +
                $"these graphics APIs will not render Gaussian splats.";
        }

        /// <summary>
        /// Returns true if the active render pipeline needs to be modified before
        /// Gaussian splats can be rendered.
        /// </summary>
        public static bool IsRenderPipelineSetupRequired()
        {
            switch (GetActiveRenderPipelineType())
            {
                case RenderPipelineType.Universal:
                    return FindType(_gsplatUrpFeatureTypeName, _gsplatAssemblyName) != null
                        && GetUrpRendererDataMissingGsplatFeature().Count > 0;
                case RenderPipelineType.HighDefinition:
                    return FindType(_gsplatHdrpPassTypeName, _gsplatAssemblyName) != null
                        && !IsHdrpSceneConfigured();
                default:
                    return false;
            }
        }

        public static string GetRenderPipelineSetupDescription()
        {
            switch (GetActiveRenderPipelineType())
            {
                case RenderPipelineType.Universal:
                    List<ScriptableObject> rendererData = GetUrpRendererDataMissingGsplatFeature();
                    string assetList = string.Join("\n", rendererData.Select(
                        data => " - " + GetAssetDisplayName(data)));
                    return "Gaussian splats require the Gsplat URP Feature to be added to " +
                        "your Universal Renderer Data. The following assets are missing it:\n" +
                        assetList;
                case RenderPipelineType.HighDefinition:
                    return "Gaussian splats require a Gsplat HDRP Pass to be added to a " +
                        "Custom Pass Volume in your scene, with its injection point set to " +
                        "Before Transparent.";
                default:
                    return "";
            }
        }

        public static bool ApplyRenderPipelineSetup()
        {
            try
            {
                switch (GetActiveRenderPipelineType())
                {
                    case RenderPipelineType.Universal:
                        AddGsplatUrpFeatures();
                        break;
                    case RenderPipelineType.HighDefinition:
                        AddGsplatHdrpPass();
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            bool success = !IsRenderPipelineSetupRequired();
            if (!success)
            {
                Debug.LogWarning(
                    "Unable to automatically set up the render pipeline for Gaussian splats. " + GetRenderPipelineSetupDescription());
            }

            return success;
        }

        private static void AddGsplatUrpFeatures()
        {
            Type featureType = FindType(_gsplatUrpFeatureTypeName, _gsplatAssemblyName);
            if (featureType == null)
            {
                return;
            }

            foreach (ScriptableObject rendererData in GetUrpRendererDataMissingGsplatFeature())
            {
                ScriptableObject feature = ScriptableObject.CreateInstance(featureType);
                feature.name = featureType.Name;
                Undo.RegisterCreatedObjectUndo(feature, "Add Gsplat URP Feature");

                bool isPersistent = EditorUtility.IsPersistent(rendererData);
                if (isPersistent)
                {
                    AssetDatabase.AddObjectToAsset(feature, rendererData);
                }
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out string _, out long localId);

                using (SerializedObject serializedData = new SerializedObject(rendererData))
                {
                    SerializedProperty features = serializedData.FindProperty("m_RendererFeatures");
                    SerializedProperty featureMap = serializedData.FindProperty("m_RendererFeatureMap");
                    if (features == null || featureMap == null)
                    {
                        Debug.LogWarning(
                            $"Unable to add the Gsplat URP Feature to {GetAssetDisplayName(rendererData)} - can't find expected properties.");
                        continue;
                    }

                    features.arraySize++;
                    features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = feature;
                    featureMap.arraySize++;
                    featureMap.GetArrayElementAtIndex(featureMap.arraySize - 1).longValue = localId;
                    serializedData.ApplyModifiedProperties();
                }

                // ScriptableRendererData.SetDirty invalidates the renderer so it is
                // recreated with the new feature.
                MethodInfo setDirty = FindDeclaredMethod(
                    rendererData.GetType(), "ScriptableRendererData", "SetDirty");
                setDirty?.Invoke(rendererData, null);
                EditorUtility.SetDirty(rendererData);
                if (isPersistent)
                {
                    AssetDatabase.SaveAssetIfDirty(rendererData);
                }

                Debug.Log($"Added the Gsplat URP Feature to {GetAssetDisplayName(rendererData)}.");
            }
        }

        private static void AddGsplatHdrpPass()
        {
            Type passType = FindType(_gsplatHdrpPassTypeName, _gsplatAssemblyName);
            Type volumeType = FindType(_customPassVolumeTypeName, _hdrpAssemblyName);
            Type injectionPointType = FindType(_customPassInjectionPointTypeName, _hdrpAssemblyName);
            if (passType == null || volumeType == null || injectionPointType == null)
            {
                return;
            }

            object beforeTransparent = Enum.Parse(injectionPointType, _beforeTransparentInjectionPoint);

            // Prefer an existing global Before Transparent volume, so we don't
            // compete with it on HDRP versions that only run one volume per
            // injection point.
            Component volume = FindActiveCustomPassVolumes(volumeType)
                .FirstOrDefault(v =>
                    Equals(GetMemberValue(v, "injectionPoint"), beforeTransparent) &&
                    !Equals(GetMemberValue(v, "isGlobal"), false));

            if (volume != null)
            {
                Undo.RecordObject(volume, "Add Gsplat HDRP Pass");
            }
            else
            {
                GameObject go = new GameObject("Gsplat Custom Pass");
                Undo.RegisterCreatedObjectUndo(go, "Add Gsplat HDRP Pass");
                volume = go.AddComponent(volumeType);
                SetMemberValue(volume, "isGlobal", true);
                SetMemberValue(volume, "injectionPoint", beforeTransparent);
            }

            object pass = null;
            MethodInfo addPassOfType = volumeType.GetMethod(
                "AddPassOfType",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(Type) },
                null);
            if (addPassOfType != null)
            {
                pass = addPassOfType.Invoke(volume, new object[] { passType });
            }
            else if (GetMemberValue(volume, "customPasses") is IList passes)
            {
                pass = Activator.CreateInstance(passType);
                passes.Add(pass);
            }

            if (pass != null)
            {
                SetMemberValue(pass, "name", "Gsplat HDRP Pass");
            }

            EditorUtility.SetDirty(volume);
            EditorSceneManager.MarkSceneDirty(volume.gameObject.scene);

            Debug.Log($"Added a Gsplat HDRP Pass to the Custom Pass Volume on \"{volume.gameObject.name}\".");
        }

        private static void QueuePrompt()
        {
            if (_isPromptQueued)
            {
                return;
            }

            _isPromptQueued = true;
            EditorApplication.delayCall += PromptIfRequired;
        }

        private static void PromptIfRequired()
        {
            _isPromptQueued = false;

            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorPrefs.GetBool(_neverAskPrefKey, false))
            {
                return;
            }

            RenderPipelineType pipelineType = GetActiveRenderPipelineType();
            if (pipelineType == RenderPipelineType.Universal &&
                EditorPrefs.GetBool(GetUrpDeclinedPrefKey(), false))
            {
                return;
            }

            if (pipelineType == RenderPipelineType.HighDefinition &&
                SessionState.GetBool(_hdrpDeclinedSessionKey, false))
            {
                return;
            }

            if (!IsRenderPipelineSetupRequired())
            {
                return;
            }

            int choice = EditorUtility.DisplayDialogComplex(
                _dialogTitle,
                GetRenderPipelineSetupDescription() +
                "\n\nWould you like Cesium for Unity to make this change automatically?",
                "Apply",
                "No",
                "Don't Ask Me Again");

            switch (choice)
            {
                case 0:
                    ApplyRenderPipelineSetup();
                    break;
                case 1:
                    if (pipelineType == RenderPipelineType.Universal)
                    {
                        EditorPrefs.SetBool(GetUrpDeclinedPrefKey(), true);
                    }
                    else
                    {
                        SessionState.SetBool(_hdrpDeclinedSessionKey, true);
                    }
                    break;
                case 2:
                    EditorPrefs.SetBool(_neverAskPrefKey, true);
                    break;
            }
        }

        // No straightforward way to store prefs in just this project, so get a project-specific key to use with EditorPrefs.
        private static string GetUrpDeclinedPrefKey()
        {
            return _urpDeclinedPrefKeyPrefix + PlayerSettings.productGUID.ToString("N");
        }

        private static RenderPipelineType GetRenderPipelineType(RenderPipelineAsset asset)
        {
            if (asset == null)
            {
                return RenderPipelineType.BuiltIn;
            }
            if (IsOrDerivesFrom(asset.GetType(), _urpAssetTypeName))
            {
                return RenderPipelineType.Universal;
            }
            if (IsOrDerivesFrom(asset.GetType(), _hdrpAssetTypeName))
            {
                return RenderPipelineType.HighDefinition;
            }
            Debug.LogWarning("Can't determine the render pipeline type for the current asset.");
            return RenderPipelineType.BuiltIn;
        }

        private static IEnumerable<RenderPipelineAsset> GetAllUrpAssets()
        {
            List<RenderPipelineAsset> assets = new List<RenderPipelineAsset>
            {
                GraphicsSettings.currentRenderPipeline,
                GraphicsSettings.defaultRenderPipeline
            };

            int qualityLevelCount = QualitySettings.names.Length;
            for (int i = 0; i < qualityLevelCount; i++)
            {
                assets.Add(QualitySettings.GetRenderPipelineAssetAt(i));
            }

            return assets
                .Where(asset => GetRenderPipelineType(asset) == RenderPipelineType.Universal)
                .Distinct();
        }

        private static List<ScriptableObject> GetUrpRendererDataMissingGsplatFeature()
        {
            List<ScriptableObject> result = new List<ScriptableObject>();

            Type featureType = FindType(_gsplatUrpFeatureTypeName, _gsplatAssemblyName);
            if (featureType == null)
            {
                return result;
            }

            HashSet<ScriptableObject> visited = new HashSet<ScriptableObject>();
            foreach (RenderPipelineAsset urpAsset in GetAllUrpAssets())
            {
                using (SerializedObject serializedAsset = new SerializedObject(urpAsset))
                {
                    SerializedProperty dataList = serializedAsset.FindProperty("m_RendererDataList");
                    if (dataList == null || !dataList.isArray)
                    {
                        continue;
                    }

                    for (int i = 0; i < dataList.arraySize; i++)
                    {
                        ScriptableObject rendererData =
                            dataList.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableObject;
                        if (rendererData == null || !visited.Add(rendererData))
                        {
                            continue;
                        }

                        if (!HasRendererFeature(rendererData, featureType))
                        {
                            result.Add(rendererData);
                        }
                    }
                }
            }

            return result;
        }

        private static bool HasRendererFeature(ScriptableObject rendererData, Type featureType)
        {
            using (SerializedObject serializedData = new SerializedObject(rendererData))
            {
                SerializedProperty features = serializedData.FindProperty("m_RendererFeatures");
                if (features == null || !features.isArray)
                {
                    // We don't know how to inspect this renderer, so don't nag about it.
                    return true;
                }

                for (int i = 0; i < features.arraySize; i++)
                {
                    if (featureType.IsInstanceOfType(features.GetArrayElementAtIndex(i).objectReferenceValue))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsHdrpSceneConfigured()
        {
            Type passType = FindType(_gsplatHdrpPassTypeName, _gsplatAssemblyName);
            Type volumeType = FindType(_customPassVolumeTypeName, _hdrpAssemblyName);
            Type injectionPointType = FindType(_customPassInjectionPointTypeName, _hdrpAssemblyName);
            if (passType == null || volumeType == null || injectionPointType == null)
            {
                // Without these types we can't verify or apply the setup.
                return true;
            }

            object beforeTransparent = Enum.Parse(injectionPointType, _beforeTransparentInjectionPoint);

            foreach (Component volume in FindActiveCustomPassVolumes(volumeType))
            {
                if (!Equals(GetMemberValue(volume, "injectionPoint"), beforeTransparent))
                {
                    continue;
                }

                if (GetMemberValue(volume, "customPasses") is IEnumerable passes)
                {
                    foreach (object pass in passes)
                    {
                        if (passType.IsInstanceOfType(pass) &&
                            !Equals(GetMemberValue(pass, "enabled"), false))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static IEnumerable<Component> FindActiveCustomPassVolumes(Type volumeType)
        {
            return UnityEngine.Object.FindObjectsByType(
                    volumeType,
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .OfType<Behaviour>()
                .Where(volume => volume.isActiveAndEnabled);
        }

        private static Type FindType(string typeName, string assemblyName)
        {
            return Type.GetType(typeName + ", " + assemblyName, false);
        }

        private static bool IsOrDerivesFrom(Type type, string baseTypeFullName)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                if (current.FullName == baseTypeFullName)
                {
                    return true;
                }
            }
            return false;
        }

        private static MethodInfo FindDeclaredMethod(Type type, string declaringTypeName, string methodName)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                if (current.Name == declaringTypeName)
                {
                    return current.GetMethod(
                        methodName,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                        null,
                        Type.EmptyTypes,
                        null);
                }
            }
            return null;
        }

        private static object GetMemberValue(object target, string memberName)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            Type type = target.GetType();

            FieldInfo field = type.GetField(memberName, flags);
            if (field != null)
            {
                return field.GetValue(target);
            }

            PropertyInfo property = type.GetProperty(memberName, flags);
            if (property != null && property.CanRead)
            {
                return property.GetValue(target);
            }

            return null;
        }

        private static void SetMemberValue(object target, string memberName, object value)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            Type type = target.GetType();

            FieldInfo field = type.GetField(memberName, flags);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            PropertyInfo property = type.GetProperty(memberName, flags);
            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value);
            }
        }

        private static string GetAssetDisplayName(UnityEngine.Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            return string.IsNullOrEmpty(path) ? asset.name : path;
        }
    }
}
#endif
