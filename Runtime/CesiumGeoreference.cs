using System;
using System.Collections.Generic;
using Reinterop;
using Unity.Mathematics;
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

    /// <summary>
    /// Controls how global geospatial coordinates are mapped to coordinates in the Unity scene.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Internally, Cesium uses a global Earth-centered,
    /// Earth-fixed (ECEF) ellipsoid-centered coordinate system, where the ellipsoid
    /// is usually the World Geodetic System 1984(WGS84) ellipsoid. This is a
    /// right - handed system centered at the Earth's center of mass, where +X is in
    /// the direction of the intersection of the Equator and the Prime Meridian(zero
    /// degrees longitude), +Y is in the direction of the intersection of the Equator
    /// and +90 degrees longitude, and +Z is through the North Pole.
    /// </para>
    /// <para>
    /// This component controls how this coordinate system is mapped into the Unity world.
    /// </para>
    /// <para>
    /// This component should be added to a GameObject that is a parent of all GameObjects
    /// with the <see cref="Cesium3DTileset"/> or <see cref="CesiumGlobeAnchor"/> components.
    /// See the documentation for these component types to learn how they are affected by
    /// the <see cref="CesiumGeoreference"/>.
    /// </para>
    /// </remarks>
    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumGeoreferenceImpl", "CesiumGeoreferenceImpl.h")]
    public partial class CesiumGeoreference : MonoBehaviour
    {
        [SerializeField]
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
        public event Action changed;

        private void OnValidate()
        {
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;
            this.transform.localScale = new Vector3(1, 1, 1);
            this.transform.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
        }

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
                double3 ecef =
                    CesiumTransforms.LongitudeLatitudeHeightToEarthCenteredEarthFixed(
                        new double3(this._longitude, this._latitude, this._height));
                this._ecefX = ecef.x;
                this._ecefY = ecef.y;
                this._ecefZ = ecef.z;
            }
            else if (this._originAuthority == CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed)
            {
                double3 llh =
                    CesiumTransforms.EarthCenteredEarthFixedToLongitudeLatitudeHeight(
                        new double3(this._ecefX, this._ecefY, this._ecefZ));
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
        public partial double3
            TransformUnityWorldPositionToEarthCenteredEarthFixed(double3 unityWorldPosition);

        /// <summary>
        /// Transforms a local position expressed relative to a parent transform into Earth-Centered,
        /// Earth-Fixed (ECEF) coordinates.
        /// </summary>
        /// <param name="parent">The parent transform in which the local position is defined.</param>
        /// <param name="unityLocalPosition">The local position to convert.</param>
        /// <returns>The ECEF coordinates in meters.</returns>
        public partial double3 TransformUnityLocalPositionToEarthCenteredEarthFixed(Transform parent, double3 unityLocalPosition);

        /// <summary>
        /// Transform an Earth-Centered, Earth-Fixed position to Unity world coordinates.
        /// </summary>
        /// <param name="earthCenteredEarthFixed">The ECEF coordinates in meters.</param>
        /// <returns>The corresponding Unity world coordinates.</returns>
        public partial double3
            TransformEarthCenteredEarthFixedPositionToUnityWorld(double3 earthCenteredEarthFixed);

        /// <summary>
        /// Transform an Earth-Centered, Earth-Fixed position to local coordinates relative to a parent transform.
        /// </summary>
        /// <param name="parent">The parent transform in which the local position is defined.</param>
        /// <param name="earthCenteredEarthFixed">The ECEF coordinates in meters</param>
        /// <returns>The corresponding Unity local coordinates.</returns>
        public partial double3 TransformEarthCenteredEarthFixedPositionToUnityLocal(Transform parent, double3 earthCenteredEarthFixed);

        /// <summary>
        /// Transform a Unity world direction to a direction in Earth-Centered, Earth-Fixed (ECEF) coordinates.
        /// </summary>
        /// <param name="unityWorldDirection">The Unity world direction to convert.</param>
        /// <returns>The ECEF direction.</returns>
        public partial double3
            TransformUnityWorldDirectionToEarthCenteredEarthFixed(double3 unityWorldDirection);

        /// <summary>
        /// Transform an Earth-Centered, Earth-Fixed direction to Unity world coordinates.
        /// </summary>
        /// <param name="earthCenteredEarthFixedDirection">The direction in ECEF coordinates.</param>
        /// <returns>The corresponding Unity world direction.</returns>
        public partial double3
            TransformEarthCenteredEarthFixedDirectionToUnityWorld(double3 earthCenteredEarthFixedDirection);
    }
}