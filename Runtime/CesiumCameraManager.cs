using Reinterop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Manages the set of cameras that are used for Cesium3DTileset culling and level-of-detail.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Cesium/Cesium Camera Manager")]
    [Icon("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public class CesiumCameraManager : MonoBehaviour
    {
        /// <summary>
        /// Gets the instance suitable for use with the given game object.
        /// </summary>
        /// <remarks>
        /// This method looks for an existing instance using `GetComponentInParent`. If it fails to find one, then
        /// it will create one. When one is created, it be added to the same `GameObject` that has the
        /// `CesiumGeoreference` (found using `GetComponentInParent` again) if there is one. If there is no
        /// `CesiumGeoreference`, the instance is added to the originally-provided `GameObject`.
        /// </remarks>
        /// <param name="gameObject">The game object.</param>
        /// <returns></returns>
        public static CesiumCameraManager GetOrCreate(GameObject gameObject)
        {
            if (gameObject == null) throw new ArgumentNullException("gameObject");

            CesiumCameraManager result = gameObject.GetComponentInParent<CesiumCameraManager>();
            if (result == null)
                return CesiumCameraManager.Create(gameObject);
            else
                return result;
        }

        public static CesiumCameraManager GetOrCreate(Component component)
        {
            if (component == null) throw new ArgumentNullException("component");

            return CesiumCameraManager.GetOrCreate(component.gameObject);
        }

        private static CesiumCameraManager Create(GameObject gameObject)
        {
            // Add it to the same game object that has the CesiumGeoreference, if any.
            // Otherwise, add it to the current game object.
            CesiumGeoreference georeference = gameObject.GetComponentInParent<CesiumGeoreference>();
            GameObject owner = georeference == null ? gameObject : georeference.gameObject;
            if (owner == null) return null;

            return owner.AddComponent<CesiumCameraManager>();
        }

        #region User-editable properties

        [SerializeField]
        private bool _useMainCamera = true;

        /// <summary>
        /// Determines whether the camera tagged `MainCamera` should be used for Cesium3DTileset
        /// culling and level-of-detail.
        /// </summary>
        public bool useMainCamera
        {
            get => this._useMainCamera;
            set
            {
                this._useMainCamera = value;
            }
        }

        [SerializeField]
        private bool _useSceneViewCameraInEditor = true;

        /// <summary>
        /// Determines whether the camera associated with the Editor's active scene view should be
        /// used for Cesium3DTileset culling and level-of-detail. In a game, this property has
        /// no effect.
        /// </summary>
        public bool useSceneViewCameraInEditor
        {
            get => this._useSceneViewCameraInEditor;
            set
            {
                this._useSceneViewCameraInEditor = value;
            }
        }

        [SerializeField]
        private List<Camera> _additionalCameras = new List<Camera>();

        /// <summary>
        /// Additional Cameras to use for Cesium3DTileset culling and level-of-detail. These cameras
        /// may be disabled, which is useful for creating a virtual camera that affects Cesium3DTileset
        /// but that is not actually rendered.
        /// </summary>
        public List<Camera> additionalCameras
        {
            get => this._additionalCameras;
        }

        #endregion
    }
}