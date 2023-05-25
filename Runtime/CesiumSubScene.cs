using System;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A sub-scene with its own georeference origin. When a game object with a <see cref="CesiumOriginShift"/>
    /// comes close to a sub-scene, that sub-scene is activated and all other sub-scenes are deactivated.
    /// This allows relatively normal Unity scenes to be designed at multiple locations on the globe.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When determining the distance to a sub-scene, only the <see cref="CesiumGeoreference"/>'s Transform
    /// is considered. The Transform associated with the sub-scene is ignored. The sub-scene transform _will_
    /// affect the transformation of the objects inside it once activated, however.
    /// </para>
    /// </remarks>
    [ExecuteInEditMode]
    [AddComponentMenu("Cesium/Cesium Sub Scene")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public class CesiumSubScene : MonoBehaviour
    {
        [SerializeField]
        private double _activationRadius = 1000;

        /// <summary>
        /// The maximum distance at which to activate this sub-scene, in meters.
        /// </summary>
        /// <remarks>
        /// Even if the <see cref="CesiumOriginShift"/> game object is inside this activation radius, this
        /// sub-scene may still not be activated if another sub-scene is closer.
        /// </remarks>
        public double activationRadius
        {
            get => this._activationRadius;
            set
            {
                this._activationRadius = value;
            }
        }

        [SerializeField]
        private bool _showActivationRadius = true;

        /// <summary>
        /// Whether to show the activation radius as a wireframe sphere in the Editor.
        /// </summary>
        public bool showActivationRadius
        {
            get => this._showActivationRadius;
            set => this._showActivationRadius = value;
        }

        [SerializeField]
        private CesiumGeoreferenceOriginAuthority _originAuthority =
            CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;

        /// <summary>
        /// Identifies which set of coordinates authoritatively defines the origin
        /// of this sub-scene.
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

        // The resolved georeference containing this globe anchor. This is just a cache
        // of `GetComponentInParent<CesiumGeoreference>()`.
        [NonSerialized]
        internal CesiumGeoreference _parentGeoreference;

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

        private void DetachFromParentIfNeeded()
        {
            if (this._parentGeoreference != null)
            {
                this._parentGeoreference.changed -= this.OnParentChanged;
                this._parentGeoreference = null;
            }
        }

        private void UpdateParentReference()
        {
            DetachFromParentIfNeeded();

            this._parentGeoreference = this.GetComponentInParent<CesiumGeoreference>();

            if (this._parentGeoreference != null)
            {
                this._parentGeoreference.Initialize();
                this._parentGeoreference.changed += this.OnParentChanged;
            }
        }

        /// <summary>
        /// Called by the Editor when the script is loaded or a value changes in the Inspector.
        /// Use this to perform an action after a value changes in the Inspector; for example, 
        /// making sure that data stays within a certain range.
        /// </summary>
        private void OnValidate()
        {
            this.UpdateOrigin();
        }

        /// <summary>
        /// Called by the Editor when the user chooses to "reset" the component.
        /// The implementation here makes sure the newly-reset values for the serialized
        /// properties are applied.
        /// </summary>
        private void Reset()
        {
            this.UpdateParentReference();
        }

        private void OnEnable()
        {
            this.UpdateParentReference();

            // If not under a georef, nothing to do
            if (this._parentGeoreference == null)
                throw new InvalidOperationException(
                    "CesiumSubScene is not nested inside a game object with a CesiumGeoreference.");

            // When this sub-scene is enabled, all others are disabled.
            CesiumSubScene[] subscenes = this._parentGeoreference.GetComponentsInChildren<CesiumSubScene>();
            foreach (CesiumSubScene scene in subscenes)
            {
                if (scene == this)
                    continue;
                scene.gameObject.SetActive(false);
            }

            this.UpdateOrigin();
        }

        private void OnParentChanged()
        {
            if (!this.isActiveAndEnabled)
                return;

            this.UpdateParentReference();

            // If not under a georef, nothing to do
            if (this._parentGeoreference == null)
            {
                throw new InvalidOperationException(
                    "CesiumSubScene should have been nested inside a game object with a CesiumGeoreference.");
            }

            // Update our origin to our parent georef, maintain our origin authority,
            // and copy both sets of reference coordinates. No need to calculate any of this again
            this._longitude = this._parentGeoreference.longitude;
            this._latitude = this._parentGeoreference.latitude;
            this._height = this._parentGeoreference.height;

            this._ecefX = this._parentGeoreference.ecefX;
            this._ecefY = this._parentGeoreference.ecefY;
            this._ecefZ = this._parentGeoreference.ecefZ;
        }

        private void OnDisable()
        {
            DetachFromParentIfNeeded();
        }

        private void OnDestroy()
        {
            DetachFromParentIfNeeded();
        }

        private void UpdateOtherCoordinates()
        {
            if (this._originAuthority == CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight)
            {
                double3 ecef = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(
                    this._longitude,
                    this._latitude,
                    this._height
                ));

                this._ecefX = ecef.x;
                this._ecefY = ecef.y;
                this._ecefZ = ecef.z;
            }

            if (this._originAuthority == CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed)
            {
                double3 llh = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(new double3(
                    this._ecefX,
                    this._ecefY,
                    this._ecefZ
                ));

                this._longitude = llh.x;
                this._latitude = llh.y;
                this._height = llh.z;
            }
        }

        /// <summary>
        /// Recomputes the coordinate system based on an updated origin. It is usually not
        /// necessary to call this directly as it is called automatically when needed.
        /// </summary>
        public void UpdateOrigin()
        {
            UpdateOtherCoordinates();

            if (this.isActiveAndEnabled)
            {
                this.UpdateParentReference();

                if (this._parentGeoreference == null)
                    throw new InvalidOperationException("CesiumSubScene is not nested inside a game object with a CesiumGeoreference.");

                if (this.originAuthority == CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed)
                    this._parentGeoreference.SetOriginEarthCenteredEarthFixed(
                        this._ecefX,
                        this._ecefY,
                        this._ecefZ);
                else
                    this._parentGeoreference.SetOriginLongitudeLatitudeHeight(
                        this._longitude,
                        this._latitude,
                        this._height);
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (this._showActivationRadius)
            {
                // TODO: would be nice to draw a better wireframe sphere.
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(this.transform.position, (float)this._activationRadius);
            }
        }
        #endif
    }
}
