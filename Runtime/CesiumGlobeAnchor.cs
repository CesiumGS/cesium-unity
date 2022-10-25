using Reinterop;
using System;
using System.Collections;
using System.Reflection;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Identifies the set of the coordinates that authoritatively define
    /// the position of this object.
    /// </summary>
    public enum CesiumGlobeAnchorAuthority
    {
        /// <summary>
        /// The <see cref="CesiumGlobeAnchor.longitude"/>, <see cref="CesiumGlobeAnchor.latitude"/>,
        /// and <see cref="CesiumGlobeAnchor.height"/> properties authoritatively define the position
        /// of this object.
        /// </summary>
        LongitudeLatitudeHeight,

        /// <summary>
        /// The <see cref="CesiumGlobeAnchor.ecefX"/>, <see cref="CesiumGlobeAnchor.ecefY"/>,
        /// and <see cref="CesiumGlobeAnchor.ecefZ"/> properties authoritatively define the position
        /// of this object.
        /// </summary>
        EarthCenteredEarthFixed,

        /// <summary>
        /// The <see cref="CesiumGlobeAnchor.unityX"/>, <see cref="CesiumGlobeAnchor.unityY"/>,
        /// and <see cref="CesiumGlobeAnchor.unityZ"/> properties authoritatively define the position
        /// of this object.
        /// </summary>
        UnityWorldCoordinates
    }

    [ExecuteInEditMode]
    public class CesiumGlobeAnchor : MonoBehaviour, INotifyOfChanges
    {
        #region User-editable properties

        [SerializeField]
        [Tooltip("Whether to adjust the game object's orientation based on globe curvature as the game object moves.\n" +
                 "\n" +
                 "The Earth is not flat, so as we move across its surface, the direction of \"up\" changes. " +
                 "If we ignore this fact and leave an object's orientation unchanged as it moves over the " +
                 "globe surface, the object will become increasingly tilted and eventually be completely " +
                 "upside-down when we arrive at the opposite side of the globe.\n" +
                 "\n" +
                 "When this setting is enabled, this component will automatically apply a rotation to the " +
                 "Transform to account for globe curvature any time the game object's position on the " +
                 "globe changes.\n" +
                 "\n" +
                 "This property should usually be enabled, but it may be useful to disable it when your " +
                 "application already accounts for globe curvature itself when it updates a game " +
                 "object's transform, because in that case game object would be over-rotated.")]
        private bool _adjustOrientationForGlobeWhenMoving = true;

        public bool adjustOrientationForGlobeWhenMoving
        {
            get => this._adjustOrientationForGlobeWhenMoving;
            set => this._adjustOrientationForGlobeWhenMoving = value;
        }

        [SerializeField]
        [Tooltip("Whether this component should detect changes to the Transform component, such as " +
                 "from physics, and update the precise coordinates accordingly. Disabling this option " +
                 "improves performance for game objects that will not move. Transform changes are " +
                 "always detected in Edit mode, no matter the state of this flag.")]
        [NotifyOfChanges]
        private bool _detectTransformChanges = true;

        public bool detectTransformChanges
        {
            get => this._detectTransformChanges;
            set
            {
                this._detectTransformChanges = value;
                this.StartOrStopDetectingTransformChanges();
            }
        }

        [SerializeField]
        [Tooltip("The set of coordinates that authoritatively define the position of this game object.")]
        [NotifyOfChanges]
        private CesiumGlobeAnchorAuthority _positionAuthority = CesiumGlobeAnchorAuthority.LongitudeLatitudeHeight;

        public CesiumGlobeAnchorAuthority positionAuthority
        {
            get => this._positionAuthority;
            set
            {
                this._positionAuthority = value;
                this.UpdateTransformFromGlobePosition();
            }
        }

        [SerializeField]
        [Header("Position (Longitude Latitude Height)")]
        [Tooltip("The latitude of this game object in degrees, in the range [-90, 90].")]
        [NotifyOfChanges]
        private double _latitude = 0.0;

        public double latitude
        {
            get => this._latitude;
            set
            {
                this._latitude = value;
                this.positionAuthority = CesiumGlobeAnchorAuthority.LongitudeLatitudeHeight;
            }
        }

        [SerializeField]
        [Tooltip("The longitude of this game object in degrees, in the range [-180, 180].")]
        [NotifyOfChanges]
        private double _longitude = 0.0;

        public double longitude
        {
            get => this._longitude;
            set
            {
                this._longitude = value;
                this.positionAuthority = CesiumGlobeAnchorAuthority.LongitudeLatitudeHeight;
            }
        }

        [SerializeField]
        [Tooltip("The height of this game object in meters above the ellipsoid (usually WGS84). " +
                 "Do not confuse this with a geoid height or height above mean sea level, which " +
                 "can be tens of meters higher or lower depending on where in the world the " +
                 "object is located.")]
        [NotifyOfChanges]
        private double _height = 0.0;

        public double height
        {
            get => this._height;
            set
            {
                this._height = value;
                this.positionAuthority = CesiumGlobeAnchorAuthority.LongitudeLatitudeHeight;
            }
        }

        [SerializeField]
        [Header("Position (Earth Centered, Earth Fixed)")]
        [Tooltip("The Earth-Centered, Earth-Fixed X-coordinate of this game object in meters.\n" +
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
                this.positionAuthority = CesiumGlobeAnchorAuthority.EarthCenteredEarthFixed;
            }
        }

        [SerializeField]
        [Tooltip("The Earth-Centered, Earth-Fixed Y-coordinate of this game object in meters.\n" +
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
                this.positionAuthority = CesiumGlobeAnchorAuthority.EarthCenteredEarthFixed;
            }
        }

        [SerializeField]
        [Tooltip("The Earth-Centered, Earth-Fixed Z-coordinate of this game object in meters.\n" +
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
                this.positionAuthority = CesiumGlobeAnchorAuthority.EarthCenteredEarthFixed;
            }
        }

        [SerializeField]
        [Header("Position (Unity coordinates)")]
        [Tooltip("The Unity world X coordinate of this game object. This is the same as the Transform's X coordinate but expressed in 64-bit (double) precision.")]
        [NotifyOfChanges]
        private double _unityX = 0.0;

        public double unityX
        {
            get => this._unityX;
            set
            {
                this._unityX = value;
                this.positionAuthority = CesiumGlobeAnchorAuthority.UnityWorldCoordinates;
            }
        }

        [SerializeField]
        [Tooltip("The Unity world Y coordinate of this game object. This is the same as the Transform's Y coordinate but expressed in 64-bit (double) precision.")]
        [NotifyOfChanges]
        private double _unityY = 0.0;

        public double unityY
        {
            get => this._unityY;
            set
            {
                this._unityY = value;
                this.positionAuthority = CesiumGlobeAnchorAuthority.UnityWorldCoordinates;
            }
        }

        [SerializeField]
        [Tooltip("The Unity world Z coordinate of this game object. This is the same as the Transform's Y coordinate but expressed in 64-bit (double) precision.")]
        [NotifyOfChanges]
        private double _unityZ = 0.0;

        public double unityZ
        {
            get => this._unityZ;
            set
            {
                this._unityZ = value;
                this.positionAuthority = CesiumGlobeAnchorAuthority.UnityWorldCoordinates;
            }
        }

        #endregion

        #region Set Helpers

        /// <summary>
        /// Sets the position of this object to a particular longitude, latitude, and height.
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
        public void SetPositionLongitudeLatitudeHeight(double longitude, double latitude, double height)
        {
            this._longitude = longitude;
            this._latitude = latitude;
            this._height = height;
            this.positionAuthority = CesiumGlobeAnchorAuthority.LongitudeLatitudeHeight;
        }

        /// <summary>
        /// Sets the position of this object to particular X, Y, Z coordinates in the
        /// Earth-Centered, Earth-Fixed (ECEF) frame.
        /// </summary>
        /// <remarks>
        /// Calling this method is more efficient than setting the properties individually.
        /// </remarks>
        /// <param name="x">The X coordinate in meters.</param>
        /// <param name="y">The Y coordinate in meters.</param>
        /// <param name="z">The Z coordinate in meters.</param>
        public void SetPositionEarthCenteredEarthFixed(double x, double y, double z)
        {
            this._ecefX = x;
            this._ecefY = y;
            this._ecefZ = z;
            this.positionAuthority = CesiumGlobeAnchorAuthority.EarthCenteredEarthFixed;
        }

        /// <summary>
        /// Sets the position of this object to particular X, Y, Z coordinates in the
        /// Unity world coordinate system.
        /// </summary>
        /// <remarks>
        /// Calling this method is more efficient than setting the properties individually.
        /// </remarks>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public void SetPositionUnityWorld(double x, double y, double z)
        {
            this._unityX = x;
            this._unityY = y;
            this._unityZ = z;
            this.positionAuthority = CesiumGlobeAnchorAuthority.UnityWorldCoordinates;
        }

        #endregion

        #region Private properties

        [SerializeField]
        [HideInInspector]
        private double _lastPositionEcefX = 0.0;

        [SerializeField]
        [HideInInspector]
        private double _lastPositionEcefY = 0.0;

        [SerializeField]
        [HideInInspector]
        private double _lastPositionEcefZ = 0.0;

        private Matrix4x4 _lastLocalToWorld;

        #endregion

        #region INotifyOfChanges implementation

        void INotifyOfChanges.NotifyPropertyChanged(SerializedProperty property)
        {
#if UNITY_EDITOR
            switch (property.name)
            {
                case "_longitude":
                case "_latitude":
                case "_height":
                    this._positionAuthority = CesiumGlobeAnchorAuthority.LongitudeLatitudeHeight;
                    this.UpdateTransformFromGlobePosition();
                    break;
                case "_ecefX":
                case "_ecefY":
                case "_ecefZ":
                    this._positionAuthority = CesiumGlobeAnchorAuthority.EarthCenteredEarthFixed;
                    this.UpdateTransformFromGlobePosition();
                    break;
                case "_unityX":
                case "_unityY":
                case "_unityZ":
                    this._positionAuthority = CesiumGlobeAnchorAuthority.UnityWorldCoordinates;
                    this.UpdateTransformFromGlobePosition();
                    break;
                case "_detectTransformChanges":
                    this.StartOrStopDetectingTransformChanges();
                    break;
            }

            EditorApplication.QueuePlayerLoopUpdate();
#endif
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            this._lastLocalToWorld = this.transform.localToWorldMatrix;
            this.StartOrStopDetectingTransformChanges();
        }

        #endregion

        #region Coroutines

        private void StartOrStopDetectingTransformChanges()
        {
            this.StopCoroutine("DetectTransformChanges");

            bool start = this._detectTransformChanges;

#if UNITY_EDITOR
            // Always detect changes in Edit mode.
            if (!EditorApplication.isPlaying)
                start = true;
#endif

            if (start)
                this.StartCoroutine("DetectTransformChanges");
        }

        private IEnumerator DetectTransformChanges()
        {
            // Detect changes in the Transform component.
            // We don't use Transform.hasChanged because we can't control when it is reset to false.
            WaitUntil waitForChanges = new WaitUntil(() => !this.transform.localToWorldMatrix.Equals(this._lastLocalToWorld));

            while (true)
            {
                yield return waitForChanges;
                this.UpdateGlobePositionFromTransform();
                this._lastLocalToWorld = this.transform.localToWorldMatrix;
            }
        }

        #endregion

        #region Updaters

        private void UpdateTransformFromGlobePosition()
        {
            CesiumGeoreference? georeference = this.gameObject.GetComponentInParent<CesiumGeoreference>();
            if (georeference == null)
                throw new InvalidOperationException("CesiumGlobeAnchor is not nested inside a game object with a CesiumGeoreference.");

            // Convert the authoritative position to ECEF
            CesiumVector3 ecef;
            switch (this.positionAuthority)
            {
                case CesiumGlobeAnchorAuthority.LongitudeLatitudeHeight:
                    ecef = CesiumTransforms.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new CesiumVector3()
                    {
                        x = this.longitude,
                        y = this.latitude,
                        z = this.height
                    });
                    break;
                case CesiumGlobeAnchorAuthority.EarthCenteredEarthFixed:
                    ecef.x = this.ecefX;
                    ecef.y = this.ecefY;
                    ecef.z = this.ecefZ;
                    break;
                case CesiumGlobeAnchorAuthority.UnityWorldCoordinates:
                    ecef = georeference.TransformUnityWorldPositionToEarthCenteredEarthFixed(new CesiumVector3()
                    {
                        x = this.unityX,
                        y = this.unityY,
                        z = this.unityZ
                    });
                    break;
                default:
                    throw new InvalidOperationException("Unknown value for positionAuthority.");
            }

            // Update the orientation based on the new position (if desired)
            if (this.adjustOrientationForGlobeWhenMoving)
            {
                // TODO
            }

            // Update the non-authoritative fields with the new position.
            // TODO: it might be more efficient to lazily update these if/when they're accessed, at least outside the Editor.
            if (this.positionAuthority != CesiumGlobeAnchorAuthority.LongitudeLatitudeHeight)
            {
                CesiumVector3 llh = CesiumTransforms.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);
                this._longitude = llh.x;
                this._latitude = llh.y;
                this._height = llh.z;
            }

            if (this.positionAuthority != CesiumGlobeAnchorAuthority.EarthCenteredEarthFixed)
            {
                this._ecefX = ecef.x;
                this._ecefY = ecef.y;
                this._ecefZ = ecef.z;
            }

            if (this.positionAuthority != CesiumGlobeAnchorAuthority.UnityWorldCoordinates)
            {
                CesiumVector3 unityWorld = georeference.TransformEarthCenteredEarthFixedPositionToUnityWorld(ecef);
                this._unityX = unityWorld.x;
                this._unityY = unityWorld.y;
                this._unityZ = unityWorld.z;
            }

            // Set the object's transform with the new position
            this.gameObject.transform.position = new Vector3((float)this._unityX, (float)this._unityY, (float)this._unityZ);
        }

        private void UpdateGlobePositionFromTransform()
        {
            Vector3 position = this.transform.position;
            this._unityX = position.x;
            this._unityY = position.y;
            this._unityZ = position.z;
            this._positionAuthority = CesiumGlobeAnchorAuthority.UnityWorldCoordinates;
            this.UpdateTransformFromGlobePosition();
        }

        internal void UpdateGeoreference()
        {
            // If Unity world coordinates are authoritative, switch to ECEF.
            //   This will cause the Unity world to be recomputed from the existing
            //   ECEF using the updated transform.
            // Otherwise, just recompute the Unity coordinates from the existing authority.
            // TODO: small optimization opportunity: no need to recompute LLH or ECEF when the other
            //       is authoritative, because they won't have changed.
            if (this.positionAuthority == CesiumGlobeAnchorAuthority.UnityWorldCoordinates)
                this.positionAuthority = CesiumGlobeAnchorAuthority.EarthCenteredEarthFixed;
            else
                this.UpdateTransformFromGlobePosition();
        }

        #endregion
    }
}
