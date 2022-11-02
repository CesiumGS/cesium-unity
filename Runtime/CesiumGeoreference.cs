using System;
using System.Collections.Generic;
using Reinterop;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumGeoreferenceImpl", "CesiumGeoreferenceImpl.h")]
    public partial class CesiumGeoreference : MonoBehaviour, INotifyOfChanges
    {
        [SerializeField]
        [Header("Origin")]
        [Tooltip("The latitude of the origin in degrees, in the range [-90, 90].")]
        [NotifyOfChanges]
        private double _latitude = 39.736401;

        public double latitude
        {
            get => this._latitude;
            set
            {
                this._latitude = value;
                this.UpdateOrigin();
            }
        }
        
        [SerializeField]
        [Tooltip("The longitude of the origin in degrees, in the range [-180, 180].")]
        [NotifyOfChanges]
        private double _longitude = -105.25737;

        public double longitude
        {
            get => this._longitude;
            set
            {
                this._longitude = value;
                this.UpdateOrigin();
            }
        }

        [SerializeField]
        [Tooltip("The height of the origin in meters above the ellipsoid (usually WGS84). " +
                 "Do not confuse this with a geoid height or height above mean sea level, which " +
                 "can be tens of meters higher or lower depending on where in the world the " +
                 "origin is located.")]
        [NotifyOfChanges]
        private double _height = 2250.0;

        public double height
        {
            get => this._height;
            set
            {
                this._height = value;
                this.UpdateOrigin();
            }
        }

        [Tooltip("An event raised when the georeference changes.")]
        public event Action? changed;

        void INotifyOfChanges.NotifyPropertyChanged(SerializedProperty property)
        {
#if UNITY_EDITOR
            switch (property.name)
            {
                case "_longitude":
                case "_latitude":
                case "_height":
                    this.UpdateOrigin();
                    break;
            }

            EditorApplication.QueuePlayerLoopUpdate();
#endif
        }

        public void UpdateOrigin()
        {
            this.RecalculateOrigin();

            if (this.changed != null)
            {
                this.changed();
            }
        }

        private partial void RecalculateOrigin();

        private void OnEnable()
        {
            this.InitializeOrigin();
        }

        /// <summary>
        /// Transform a Unity world position to Earth-Centered, Earth-Fixed (ECEF) coordinates.
        /// </summary>
        /// <param name="unityWorldPosition">The Unity world position to convert.</param>
        /// <returns>The ECEF coordinates in meters.</returns>
        public partial CesiumVector3 TransformUnityWorldPositionToEarthCenteredEarthFixed(CesiumVector3 unityWorldPosition);

        /// <summary>
        /// Transform an Earth-Centered, Earth-Fixed position to Unity world coordinates.
        /// </summary>
        /// <param name="earthCenteredEarthFixed">The ECEF coordinates in meters.</param>
        /// <returns>The corresponding Unity world coordinates.</returns>
        public partial CesiumVector3 TransformEarthCenteredEarthFixedPositionToUnityWorld(CesiumVector3 earthCenteredEarthFixed);

        private partial void InitializeOrigin();
    }
}