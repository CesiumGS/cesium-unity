using System;
using System.Collections.Generic;
using System.Security.Principal;
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
    /// This component should be added to a GameObject that is a parent of all GameObjects
    /// with the <see cref="Cesium3DTileset"/> or <see cref="CesiumGlobeAnchor"/> components.
    /// </para>
    /// <para>
    /// Internally, Cesium uses a global Earth-centered,
    /// Earth-fixed (ECEF) ellipsoid-centered coordinate system, where the ellipsoid
    /// is usually the World Geodetic System 1984 (WGS84) ellipsoid. This is a
    /// right-handed system centered at the Earth's center of mass, where +X is in
    /// the direction of the intersection of the Equator and the Prime Meridian (zero
    /// degrees longitude), +Y is in the direction of the intersection of the Equator
    /// and +90 degrees longitude, and +Z is through the North Pole.
    /// </para>
    /// <para>
    /// This component controls how this coordinate system is mapped into the Unity world.
    /// It creates a left-handed, Unity-friendly coordinate system centered at the specified
    /// georeference origin, where +X points East, +Y points up, and +Z points North, and
    /// transforms coordinates between that coordinate system and ECEF.
    /// </para>
    /// <para>
    /// The Unity <code>Transform</code> is applied _after_ the georeference transformation,
    /// in the normal way. For example, if a <see cref="Cesium3DTileset"/>'s <code>position</code>
    /// property is set to <code>(5000.0, 100.0, 0.0)</code>, then that tileset will be shifted
    /// East 5000 units and up 100 units. If the <see cref="CesiumGeoreference"/> has a scale of
    /// 0.5, the entire globe will be half of its normal size in the Unity world.
    /// </para>
    /// </remarks>
    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumGeoreferenceImpl", "CesiumGeoreferenceImpl.h")]
    public partial class CesiumGeoreference : MonoBehaviour
    {
        [SerializeField]
        private CesiumGeoreferenceOriginAuthority _originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;

        /// <summary>
        /// Identifies which set of coordinates authoritatively defines the origin
        /// of this georeference.
        /// </summary>
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

        /// <summary>
        /// The latitude of the origin of the coordinate system, in degrees, in the range -90 to 90.
        /// This property is ignored unless <see cref="originAuthority"/> is
        /// <see cref="CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight"/>.
        /// Setting this property changes the <see cref="originAuthority"/> accordingly.
        /// </summary>
        public double latitude
        {
            get => this._latitude;
            set
            {
                this._latitude = value;
                this.originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;
            }
        }
        
        [SerializeField]
        private double _longitude = -105.25737;

        /// <summary>
        /// The longitude of the origin of the coordinate system, in degrees, in the range -180 to 180.
        /// This property is ignored unless <see cref="originAuthority"/> is
        /// <see cref="CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight"/>.
        /// Setting this property changes the <see cref="originAuthority"/> accordingly.
        /// </summary>
        public double longitude
        {
            get => this._longitude;
            set
            {
                this._longitude = value;
                this.originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;
            }
        }

        [SerializeField]
        private double _height = 2250.0;

        /// <summary>
        /// The height in the origin of the coordinate system, in meters above the ellipsoid. Do not
        /// confuse this with a geoid height or height above mean sea level, which can be tens of
        /// meters higher or lower depending on where in the world the object is located. This
        /// property is ignored unless <see cref="originAuthority"/> is
        /// <see cref="CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight"/>.
        /// Setting this property changes the <see cref="originAuthority"/> accordingly.
        /// </summary>
        public double height
        {
            get => this._height;
            set
            {
                this._height = value;
                this.originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;
            }
        }

        [SerializeField]
        private double _ecefX = 6378137.0;

        /// <summary>
        /// The Earth-Centered, Earth-Fixed X coordinate of the origin of the coordinate system, in meters.
        /// This property is ignored unless <see cref="originAuthority"/> is
        /// <see cref="CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed"/>.
        /// Setting this property changes the <see cref="originAuthority"/> accordingly.
        /// </summary>
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

        /// <summary>
        /// The Earth-Centered, Earth-Fixed Y coordinate of the origin of the coordinate system, in meters.
        /// This property is ignored unless <see cref="originAuthority"/> is
        /// <see cref="CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed"/>.
        /// Setting this property changes the <see cref="originAuthority"/> accordingly.
        /// </summary>
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

        /// <summary>
        /// The Earth-Centered, Earth-Fixed Z coordinate of the origin of the coordinate system, in meters.
        /// This property is ignored unless <see cref="originAuthority"/> is
        /// <see cref="CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed"/>.
        /// Setting this property changes the <see cref="originAuthority"/> accordingly.
        /// </summary>
        public double ecefZ
        {
            get => this._ecefZ;
            set
            {
                this._ecefZ = value;
                this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
            }
        }

        /// <summary>
        /// An event raised when the georeference changes.
        /// </summary>
        [Tooltip("An event raised when the georeference origin changes.")]
        public event Action changed;

        /// <summary>
        /// Sets the origin of the coordinate system to particular <see cref="ecefX"/>, <see cref="ecefY"/>,
        /// <see cref="ecefZ"/> coordinates in the Earth-Centered, Earth-Fixed (ECEF) frame.
        /// </summary>
        /// <remarks>
        /// Calling this method is more efficient than setting the properties individually.
        /// </remarks>
        /// <param name="x">The X coordinate in meters.</param>
        /// <param name="y">The Y coordinate in meters.</param>
        /// <param name="z">The Z coordinate in meters.</param>
        public void SetOriginEarthCenteredEarthFixed(double x, double y, double z)
        {
            this._ecefX = x;
            this._ecefY = y;
            this._ecefZ = z;
            this.originAuthority = CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed;
        }

        /// <summary>
        /// Sets the origin of the coordinate system to a particular <see cref="longitude"/>,
        /// <see cref="latitude"/>, and <see cref="height"/>.
        /// </summary>
        /// <remarks>
        /// Calling this method is more efficient than setting the properties individually.
        /// </remarks>
        /// <param name="longitude">The longitude in degrees, in the range -180 to 180.</param>
        /// <param name="latitude">The latitude in degrees, in the range -90 to 90.</param>
        /// <param name="height">
        /// The height in meters above the ellipsoid. Do not confuse this with a geoid height
        /// or height above mean sea level, which can be tens of meters higher or lower
        /// depending on where in the world the object is located.
        /// </param>
        public void SetOriginLongitudeLatitudeHeight(double longitude, double latitude, double height)
        {
            this._longitude = longitude;
            this._latitude = latitude;
            this._height = height;
            this.originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;
        }

        /// <summary>
        /// Register a globe anchor with this georeference. When the georeference origin changes,
        /// the registered anchor will be updated accordingly.
        /// </summary>
        /// <remarks>
        /// This is called automatically by <see cref="CesiumGlobeAnchor.OnEnable"/> and usually
        /// does not need to be called directly.
        /// </remarks>
        /// <param name="globeAnchor">The globe anchor.</param>
        public void AddGlobeAnchor(CesiumGlobeAnchor globeAnchor)
        {
            this._globeAnchors.Add(globeAnchor);
        }

        /// <summary>
        /// Deregisters a globe anchor with this georeference, so the globe anchor will no longer
        /// be updated when the georeference origin changes.
        /// </summary>
        /// <remarks>
        /// This is called automatically by <see cref="CesiumGlobeAnchor.OnDisable"/> and usually
        /// does not need to be called directly.
        /// </remarks>
        /// <param name="globeAnchor">The globe anchor.</param>
        public void RemoveGlobeAnchor(CesiumGlobeAnchor globeAnchor)
        {
            this._globeAnchors.Remove(globeAnchor);
        }

        /// <summary>
        /// Initializes this georeference so that other objects may use it to locate the globe in the world.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is not usually necessary to call this directly because it is called automatically in
        /// <see cref="OnEnable"/>. However, when other objects in the same scene need to use this
        /// georeference from their own <code>OnEnable</code>, it is possible that the other object's
        /// OnEnable will be called before the georeference's, leading to incorrect georeferencing.
        /// Calling this method explicitly will avoid this problem.
        /// </para>
        /// <para>
        /// However, this method should _not_ be called prior to the scene's <code>OnEnable</code> phase.
        /// </para>
        /// <para>
        /// If this component has already been initialized, this method does nothing.
        /// </para>
        /// </remarks>
        public void Initialize()
        {
            if (!this._isInitialized)
            {
                this._isInitialized = true;
                this.UpdateOtherCoordinates();
                this.RecalculateOrigin();
            }
        }

        private Dictionary<Transform, float3x3?> _parentTransformsScratch = new Dictionary<Transform, float3x3?>();

        /// <summary>
        /// Recomputes the coordinate system based on an updated origin. It is usually not
        /// necessary to call this directly as it is called automatically when needed.
        /// </summary>
        public void UpdateOrigin()
        {
            if (!this._isInitialized)
                throw new InvalidOperationException("The origin of a CesiumGeoreference must not be set before its Initialize method is called, either explicitly or via OnEnable.");

            this.UpdateOtherCoordinates();

            double3x3? oldLocalToNewLocalDouble = this.RecalculateOrigin();
            if (oldLocalToNewLocalDouble == null)
            {
                // Origin didn't change meaningfully.
                return;
            }

            float3x3 oldLocalToNewLocal = (float3x3)oldLocalToNewLocalDouble.Value;

            //List<CesiumGlobeAnchor> anchors = this._globeAnchorsScratch;
            Dictionary<Transform, float3x3?> parentTransforms = this._parentTransformsScratch;
            parentTransforms.Clear();
            //this.GetComponentsInChildren<CesiumGlobeAnchor>(true, anchors);

            foreach (CesiumGlobeAnchor anchor in this._globeAnchors)
            {
                if (anchor == null)
                    continue;

                Transform transform = anchor.transform;
                Transform parent = transform.parent;

                // Globe anchors are very likely to share common parents, so avoid repeatedly
                // asking for the same world-to-local matrix. It's also very likely that this
                // world-to-old-local matrix is identity, so optimize for that case.
                float3x3? worldToOldLocal;
                if (!parentTransforms.TryGetValue(parent, out worldToOldLocal))
                {
                    float3x3 worldToLocal = Helpers.ToMathematicsFloat3x3(parent.worldToLocalMatrix);
                    if (worldToLocal.Equals(float3x3.identity))
                        worldToOldLocal = null;
                    else
                        worldToOldLocal = worldToLocal;
                    parentTransforms.Add(parent, worldToOldLocal);
                }

                float3x3 modelToOldWorld = Helpers.ToMathematicsFloat3x3(transform.localToWorldMatrix);
                float3x3 modelToNew = worldToOldLocal.HasValue
                    ? math.mul(math.mul(oldLocalToNewLocal, worldToOldLocal.Value), modelToOldWorld)
                    : math.mul(oldLocalToNewLocal, modelToOldWorld);

                Quaternion rotation;
                Vector3 scale;
                Helpers.MatrixToRotationAndScale(modelToNew, out rotation, out scale);

                transform.localRotation = rotation;
                //transform.localScale = scale;

                CesiumGlobeAnchorPositionAuthority authority = anchor.positionAuthority;

                // Re-assign the (probably unchanged) authority to recompute Unity
                // coordinates with the new georeference.
                anchor.positionAuthority = authority;
            }

            if (this.changed != null)
            {
                this.changed();
            }
        }

        /// <summary>
        /// Updates to a new origin, shifting and rotating objects with CesiumGlobeAnchor
        /// behaviors accordingly.
        /// </summary>
        private partial double3x3? RecalculateOrigin();

        private void OnEnable()
        {
            this.Initialize();
        }

        private void UpdateOtherCoordinates()
        {
            if (this._originAuthority == CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight)
            {
                double3 ecef =
                    CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(
                        new double3(this._longitude, this._latitude, this._height));
                this._ecefX = ecef.x;
                this._ecefY = ecef.y;
                this._ecefZ = ecef.z;
            }
            else if (this._originAuthority == CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed)
            {
                double3 llh =
                    CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(
                        new double3(this._ecefX, this._ecefY, this._ecefZ));
                this._longitude = llh.x;
                this._latitude = llh.y;
                this._height = llh.z;
            }
        }

        /// <summary>
        /// Transform a Unity position to Earth-Centered, Earth-Fixed (ECEF) coordinates. The position should generally
        /// not be a Unity _world_ position, but rather a position expressed in some parent GameObject's reference frame as
        /// defined by its Transform. This way, the chain of Unity transforms places and orients the "globe" in the
        /// Unity world.
        /// </summary>
        /// <param name="unityPosition">The Unity position to convert.</param>
        /// <returns>The ECEF coordinates in meters.</returns>
        public partial double3
            TransformUnityPositionToEarthCenteredEarthFixed(double3 unityPosition);

        /// <summary>
        /// Transform an Earth-Centered, Earth-Fixed position to Unity coordinates. The resulting position should generally
        /// not be interpreted as a Unity _world_ position, but rather a position expressed in some parent
        /// GameObject's reference frame as defined by its Transform. This way, the chain of Unity transforms
        /// places and orients the "globe" in the Unity world.
        /// </summary>
        /// <param name="earthCenteredEarthFixed">The ECEF coordinates in meters.</param>
        /// <returns>The corresponding Unity coordinates.</returns>
        public partial double3
            TransformEarthCenteredEarthFixedPositionToUnity(double3 earthCenteredEarthFixed);

        /// <summary>
        /// Transform a Unity direction to a direction in Earth-Centered, Earth-Fixed (ECEF) coordinates. The
        /// direction should generally not be a Unity _world_ direction, but rather a direction expressed in
        /// some parent GameObject's reference frame as defined by its Transform. This way, the chain of
        /// Unity transforms orients the "globe" in the Unity world.
        /// </summary>
        /// <param name="unityDirection">The Unity direction to convert.</param>
        /// <returns>The ECEF direction.</returns>
        public partial double3
            TransformUnityDirectionToEarthCenteredEarthFixed(double3 unityDirection);

        /// <summary>
        /// Transform an Earth-Centered, Earth-Fixed direction to Unity coordinates. The resulting direction
        /// should generally not be interpreted as a Unity _world_ direction, but rather a direction expressed
        /// in some parent GameObject's reference frame as defined by its Transform. This way, the chain of
        /// Unity transforms orients the "globe" in the Unity world.
        /// </summary>
        /// <param name="earthCenteredEarthFixedDirection">The direction in ECEF coordinates.</param>
        /// <returns>The corresponding Unity direction.</returns>
        public partial double3
            TransformEarthCenteredEarthFixedDirectionToUnity(double3 earthCenteredEarthFixedDirection);

        [NonSerialized]
        private bool _isInitialized = false;

        [NonSerialized]
        private HashSet<CesiumGlobeAnchor> _globeAnchors = new HashSet<CesiumGlobeAnchor>();
    }
}