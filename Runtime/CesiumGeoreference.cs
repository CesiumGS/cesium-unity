using System;
using System.Collections.Generic;
using Reinterop;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Identifies the set of the coordinates that authoritatively define
    /// the origin of a <see cref="CesiumGeoreference"/>.
    /// </summary>
    public enum CesiumGeoreferenceOriginAuthority
    {
        /// <summary>
        /// The <see cref="CesiumGeoreference.longitude"/>, <see cref="CesiumGeoreference.latitude"/>,
        /// and <see cref="CesiumGeoreference.height"/> properties authoritatively define the position
        /// of this object.
        /// </summary>
        LongitudeLatitudeHeight,

        /// <summary>
        /// The <see cref="CesiumGeoreference.ecefX"/>, <see cref="CesiumGeoreference.ecefY"/>,
        /// and <see cref="CesiumGeoreference.ecefZ"/> properties authoritatively define the position
        /// of this object.
        /// </summary>
        EarthCenteredEarthFixed
    }

    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumGeoreferenceImpl", "CesiumGeoreferenceImpl.h")]
    public partial class CesiumGeoreference : MonoBehaviour, INotifyOfChanges
    {
        [SerializeField]
        [Tooltip("The set of coordinates that authoritatively define the origin of this georeference.")]
        [NotifyOfChanges]
        private CesiumGeoreferenceOriginAuthority _originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;

        public CesiumGeoreferenceOriginAuthority originAuthority
        {
            get => this._originAuthority;
            set
            {
                this._originAuthority = value;
                this.UpdateOrigin();
            }
        }

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

        [SerializeField]
        [Header("Position (Earth Centered, Earth Fixed)")]
        [Tooltip("The Earth-Centered, Earth-Fixed X-coordinate of the origin of this georeference in meters.\n" +
                 "In the ECEF coordinate system, the origin is at the center of the Earth \n" +
                 "and the positive X axis points toward where the Prime Meridian crosses the Equator.")]
        [NotifyOfChanges]
        private double _ecefX = 6378137.0;

        public double ecefX
        {
            get => this._ecefX;
            set
            {
                this._ecefX = value;
                this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
            }
        }

        [SerializeField]
        [Tooltip("The Earth-Centered, Earth-Fixed Y-coordinate of the origin of this georeference in meters.\n" +
                 "In the ECEF coordinate system, the origin is at the center of the Earth \n" +
                 "and the positive Y axis points toward the Equator at 90 degrees longitude.")]
        [NotifyOfChanges]
        private double _ecefY = 0.0;

        public double ecefY
        {
            get => this._ecefY;
            set
            {
                this._ecefY = value;
                this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
            }
        }

        [SerializeField]
        [Tooltip("The Earth-Centered, Earth-Fixed Z-coordinate of the origin of this georeference in meters.\n" +
         "In the ECEF coordinate system, the origin is at the center of the Earth \n" +
         "and the positive Z axis points toward the North pole.")]
        [NotifyOfChanges]
        private double _ecefZ = 0.0;

        public double ecefZ
        {
            get => this._ecefZ;
            set
            {
                this._ecefZ = value;
                this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
            }
        }

        [Tooltip("An event raised when the georeference changes.")]
        public event Action? changed;

        public void SetOriginEarthCenteredEarthFixed(double x, double y, double z)
        {
            this._ecefX = x;
            this._ecefY = y;
            this._ecefZ = z;
            this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
        }

        public void SetOriginLongitudeLatitudeHeight(double longitude, double latitude, double height)
        {
            this._longitude = longitude;
            this._latitude = latitude;
            this._height = height;
            this.originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;
        }

        void INotifyOfChanges.NotifyPropertyChanged(SerializedProperty property)
        {
#if UNITY_EDITOR
            switch (property.name)
            {
                case "_longitude":
                case "_latitude":
                case "_height":
                    this.originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;
                    break;
                case "_ecefX":
                case "_ecefY":
                case "_ecefZ":
                    this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
                    break;
            }

            EditorApplication.QueuePlayerLoopUpdate();
#endif
        }

        public void UpdateOrigin()
        {
            this.RecalculateOrigin();
            this.UpdateOtherCoordinates();

            if (this.changed != null)
            {
                this.changed();
            }
        }

        /// <summary>
        /// Initializes the C++ side of the georeference transformation, without regard for the
        /// previous state (if any).
        /// </summary>
        private partial void InitializeOrigin();

        /// <summary>
        /// Updates to a new origin, shifting and rotating objects with CesiumGlobeAnchor
        /// behaviors accordingly.
        /// </summary>
        private partial void RecalculateOrigin();

        private void OnEnable()
        {
            // We must initialize the origin in OnEnable because Unity does
            // not always call Awake at the appropriate time for `ExecuteInEditMode`
            // components like this one.
            this.InitializeOrigin();
            this.UpdateOtherCoordinates();
        }

        private void UpdateOtherCoordinates()
        {
            if (this._originAuthority == CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight)
            {
                CesiumVector3 ecef = CesiumTransforms.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new CesiumVector3()
                {
                    x = this._longitude,
                    y = this._latitude,
                    z = this._height
                });
                this._ecefX = ecef.x;
                this._ecefY = ecef.y;
                this._ecefZ = ecef.z;
            }
            else if (this._originAuthority == CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed)
            {
                CesiumVector3 llh = CesiumTransforms.EarthCenteredEarthFixedToLongitudeLatitudeHeight(new CesiumVector3()
                {
                    x = this._ecefX,
                    y = this._ecefY,
                    z = this._ecefZ
                });
                this._longitude = llh.x;
                this._latitude = llh.y;
                this._height = llh.z;
            }
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
    }
}