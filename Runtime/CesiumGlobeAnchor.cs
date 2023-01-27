using Reinterop;
using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// Identifies the set of the coordinates that authoritatively define
    /// the position of this object.
    /// </summary>
    public enum CesiumGlobeAnchorPositionAuthority
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
    }

    /// <summary>
    /// Anchors this game object to the globe. An anchored game object can be placed anywhere on the globe with
    /// high precision, and it will stay in its proper place on the globe when the
    /// <see cref="CesiumGeoreference"/> origin changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A game object with this component _must_ be nested inside a <see cref="CesiumGeoreference"/>. That is,
    /// the game object itself, or one of its ancestors, must have a <see cref="CesiumGeoreference"/> attached.
    /// Otherwise, this component will throw an exception in `OnEnable`.
    /// </para>
    /// <para>
    /// An anchored game object is still allowed to move. It may be moved either by setting the position
    /// properties on this instance, or by updating the game object's `Transform`. If the object is
    /// expected to move outside of the Editor via a `Transform` change, be sure that the
    /// <see cref="detectTransformChanges"/> property is set to true so that this instance updates
    /// accordingly.
    /// </para>
    /// <para>
    /// When this component is moved relative to the globe and
    /// <see cref="adjustOrientationForGlobeWhenMoving"/> is enabled, the orientation of the game object will
    /// also be updated in order to keep the object upright.
    /// </para>
    /// <para>
    /// In most cases, the Transform of the ancestors of the GameObject that contains this component should
    /// be an identity transform: 0 position, 0 rotation, 1 scale. Otherwise, this globe anchor's position will
    /// be misaligned with other globe-aligned objects. However, it is sometimes useful to purposely offset an
    /// object or group of objects.
    /// </para>
    /// </remarks>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumGlobeAnchorImpl", "CesiumGlobeAnchorImpl.h")]
    public partial class CesiumGlobeAnchor : MonoBehaviour
    {
        #region User-editable properties

        [SerializeField]
        private bool _adjustOrientationForGlobeWhenMoving = true;

        /// <summary>
        /// Whether to adjust the game object's orientation based on globe curvature as the
        /// game object moves.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Earth is not flat, so as we move across its surface, the direction of
        /// "up" changes. If we ignore this fact and leave an object's orientation
        /// unchanged as it moves over the globe surface, the object will become
        /// increasingly tilted and eventually be completely upside-down when we arrive
        /// at the opposite side of the globe.
        /// </para>
        /// <para>
        /// When this setting is enabled, this component will automatically apply a
        /// rotation to the game object to account for globe curvature any time the game
        /// object's position on the globe changes.
        /// </para>
        /// <para>
        /// This property should usually be enabled, but it may be useful to disable it
        /// when your application already accounts for globe curvature itself when it
        /// updates a game object's position and orientation, because in that case the
        /// game object would be over-rotated.
        /// </para>
        /// </remarks>
        public bool adjustOrientationForGlobeWhenMoving
        {
            get => this._adjustOrientationForGlobeWhenMoving;
            set => this._adjustOrientationForGlobeWhenMoving = value;
        }

        [SerializeField]
        private bool _detectTransformChanges = true;

        /// <summary>
        /// Whether to automatically detect changes in the game object's <code>Transform</code>
        /// and update the precise globe coordinates accordingly.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this property to true runs a coroutine to poll for an update to the <code>Transform</code>
        /// so that it can be reflected in the precise coordinates. When this is property is false,
        /// the coroutine and polling are not necessary, but the precise coordinates may become out of sync
        /// with the <code>Transform</code>.
        /// </para>
        /// <para>
        /// When this is false, you can still directly set the precise coordinates, and the <code>Transform</code>
        /// will update accordingly. You can also call <see cref="Sync"/> after setting the <code>Transform</code>
        /// to manually update the precise coordinates.
        /// </para>
        /// </remarks>
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
        private CesiumGlobeAnchorPositionAuthority _positionAuthority = CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight;

        /// <summary>
        /// Identifies the set of coordinates that authoritatively define the position of this object.
        /// </summary>
        public CesiumGlobeAnchorPositionAuthority positionAuthority
        {
            get => this._positionAuthority;
            set
            {
                CesiumGlobeAnchorPositionAuthority previousAuthority = this._positionAuthority;
                this._positionAuthority = value;
                this.UpdateGlobePosition(previousAuthority);
            }
        }

        [SerializeField]
        private double _latitude = 0.0;

        /// <summary>
        /// The latitude in degrees, in the range -90 to 90. This property is ignored
        /// unless <see cref="positionAuthority"/> is
        /// <see cref="CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight"/>.
        /// Setting this property changes the <see cref="positionAuthority"/> accordingly.
        /// </summary>
        public double latitude
        {
            get => this._latitude;
            set
            {
                this._latitude = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight;
            }
        }

        [SerializeField]
        private double _longitude = 0.0;

        /// <summary>
        /// The longitude in degrees, in the range -180 to 180. This property is ignored
        /// unless <see cref="positionAuthority"/> is
        /// <see cref="CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight"/>.
        /// Setting this property changes the <see cref="positionAuthority"/> accordingly.
        /// </summary>
        public double longitude
        {
            get => this._longitude;
            set
            {
                this._longitude = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight;
            }
        }

        [SerializeField]
        private double _height = 0.0;

        /// <summary>
        /// The height in meters above the ellipsoid. Do not confuse this with a geoid height
        /// or height above mean sea level, which can be tens of meters higher or lower
        /// depending on where in the world the object is located. This property is ignored
        /// unless <see cref="positionAuthority"/> is
        /// <see cref="CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight"/>.
        /// Setting this property changes the <see cref="positionAuthority"/> accordingly.
        /// </summary>
        public double height
        {
            get => this._height;
            set
            {
                this._height = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight;
            }
        }

        [SerializeField]
        private double _ecefX = 6378137.0;

        /// <summary>
        /// The Earth-Centered, Earth-Fixed X coordinate in meters. This property is ignored
        /// unless <see cref="positionAuthority"/> is
        /// <see cref="CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed"/>.
        /// Setting this property changes the <see cref="positionAuthority"/> accordingly.
        /// </summary>
        public double ecefX
        {
            get => this._ecefX;
            set
            {
                this._ecefX = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed;
            }
        }

        [SerializeField]
        private double _ecefY = 0.0;

        /// <summary>
        /// The Earth-Centered, Earth-Fixed Y coordinate in meters. This property is ignored
        /// unless <see cref="positionAuthority"/> is
        /// <see cref="CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed"/>.
        /// Setting this property changes the <see cref="positionAuthority"/> accordingly.
        /// </summary>
        public double ecefY
        {
            get => this._ecefY;
            set
            {
                this._ecefY = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed;
            }
        }

        [SerializeField]
        private double _ecefZ = 0.0;

        /// <summary>
        /// The Earth-Centered, Earth-Fixed Z coordinate in meters. This property is ignored
        /// unless <see cref="positionAuthority"/> is
        /// <see cref="CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed"/>.
        /// Setting this property changes the <see cref="positionAuthority"/> accordingly.
        /// </summary>
        public double ecefZ
        {
            get => this._ecefZ;
            set
            {
                this._ecefZ = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed;
            }
        }

        #endregion

        #region Set Helpers

        /// <summary>
        /// Sets the position of this object to a particular <see cref="longitude"/>,
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
        public void SetPositionLongitudeLatitudeHeight(double longitude, double latitude, double height)
        {
            this._longitude = longitude;
            this._latitude = latitude;
            this._height = height;
            this.positionAuthority = CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight;
        }

        /// <summary>
        /// Sets the position of this object to particular <see cref="ecefX"/>, <see cref="ecefY"/>,
        /// <see cref="ecefZ"/> coordinates in the Earth-Centered, Earth-Fixed (ECEF) frame.
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
            this.positionAuthority = CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed;
        }

        /// <summary>
        /// Synchronizes the properties of this `CesiumGlobeAnchor`.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is usually not necessary to call this method because it is called automatically when
        /// needed. However, it may be be useful in special situations or when the timing of the
        /// automatic calls is not ideal.
        /// </para>
        /// <para>
        /// For example, if the Transform is updated via a coroutine,
        /// then the globe anchor's check for a change in the Transform may run _before_ the change,
        /// in which case the new Transform won't be noted until the _next_ frame. Calling this
        /// method after modifying the Transform will avoid the one frame delay.
        /// </para>
        /// <para>This method performs the following actions:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// If none of the position properties on this instance have been set, it updates them all
        /// from the game object's current `Transform`.
        /// </item>
        /// <item>
        /// If the game object's `Transform` has changed since the last time `Start`, this method,
        /// or a position setter was called, it updates all of this instance's position
        /// properties from the current transform and sets <see cref="positionAuthority"/> to
        /// `UnityWorldCoordinates`. This works even if <see cref="detectTransformChanges"/> is
        /// disabled. It will also update the object's orientation if
        /// <see cref="adjustOrientationForGlobeWhenMoving"/> is enabled.
        /// </item>
        /// <item>
        /// Recomputes the _other_ position properties from the authoritative set. They may be different
        /// if, for example, the `CesiumGeoreference` origin has changed. If this results in the position
        /// on the globe changing, the object's orientation will be updated as well if
        /// <see cref="adjustOrientationForGlobeWhenMoving"/> is enabled.
        /// </item>
        /// </list>
        /// </remarks>
        public void Sync()
        {
            if (this._lastLocalToWorld != null && this._lastLocalToWorld != this.transform.localToWorldMatrix)
                this._initializeFromTransform = true;

            this.UpdateGlobePosition(this._positionAuthority);
        }

        #endregion

        #region Private properties

        // When this is true, the Transform will be used to populate all other fields the next
        // time <see cref="UpdateGlobePosition"/> is called. Afterward, its value is reset to
        // false. It is initially set to true for a brand new globe anchor and can be set to
        // true again by the the transform change detection coroutine or when `Sync`
        // detects a change in the Transform.
        //
        // This is serialized because we want to keep using the saved globe position after load,
        // not recompute it (less precisely) from the Transform.
        [SerializeField]
        private bool _initializeFromTransform = true;

        // The last known ECEF position, used to detect movement of the object so that
        // its rotation can be updated when `adjustOrientationForGlobeWhenMoving` is enabled.
        // This is null before OnEnable.
        [NonSerialized]
        private double3? _lastPositionEcef = null;

        // The last known Transform, used to detect changes in the Transform so that
        // the precise globe coordinates can be recomputed from it. This is null before OnEnable.
        [NonSerialized]
        private Matrix4x4? _lastLocalToWorld = null;

        // The resolved georeference containing this globe anchor. This is just a cache
        // of `GetComponentInParent<CesiumGeoreference>()`.
        [NonSerialized]
        private CesiumGeoreference _georeference;

        #endregion

        #region Unity Messages

        private void UpdateGeoreference()
        {
            if (this._georeference != null)
                this._georeference.RemoveGlobeAnchor(this);

            this._georeference = this.gameObject.GetComponentInParent<CesiumGeoreference>();

            if (this._georeference != null)
            {
                this._georeference.Initialize();
                this._georeference.AddGlobeAnchor(this);
            }
        }

        private void OnEnable()
        {
            this.UpdateGeoreference();
            this.Sync();
            this.StartOrStopDetectingTransformChanges();
        }

        private void OnDisable()
        {
            if (this._georeference != null)
                this._georeference.RemoveGlobeAnchor(this);
            this._georeference = null;
        }

        private void OnTransformParentChanged()
        {
            this.UpdateGeoreference();
            this.Sync();
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

            // Can't start a coroutine on an inactive game object
            if (!this.isActiveAndEnabled)
                start = false;

            if (start)
                this.StartCoroutine("DetectTransformChanges");
        }

        private IEnumerator DetectTransformChanges()
        {
            // Detect changes in the Transform component.
            // We don't use Transform.hasChanged because we can't control when it is reset to false.
            WaitUntil waitForChanges = new WaitUntil(() => this._lastLocalToWorld != null && !this.transform.localToWorldMatrix.Equals(this._lastLocalToWorld));

            while (true)
            {
                yield return waitForChanges;
                this.UpdateGlobePositionFromTransform();
            }
        }

        #endregion

        #region Updaters

        private void UpdateGlobePosition(CesiumGlobeAnchorPositionAuthority? previousAuthority)
        {
            if (this._georeference == null)
            {
                this.UpdateGeoreference();
                if (this._georeference == null)
                    throw new InvalidOperationException("CesiumGlobeAnchor is not nested inside a game object with a CesiumGeoreference.");
            }

            double3 ecef;
            if (this._initializeFromTransform)
            {
                // There's no authoritative position, so copy the position from the Transform.
                Vector3 position = this.transform.localPosition;
                ecef = this._georeference.TransformUnityPositionToEarthCenteredEarthFixed(new double3(
                    position.x,
                    position.y,
                    position.z
                ));
            }
            else
            {
                // Convert the authoritative position to ECEF
                switch (this.positionAuthority)
                {
                    case CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight:
                        ecef = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(
                            this.longitude,
                            this.latitude,
                            this.height
                        ));
                        break;
                    case CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed:
                        ecef.x = this.ecefX;
                        ecef.y = this.ecefY;
                        ecef.z = this.ecefZ;
                        break;
                    default:
                        throw new InvalidOperationException("Unknown value for positionAuthority.");
                }
            }

            // Update the non-authoritative fields with the new position.
            // TODO: it might be more efficient to lazily update these if/when they're accessed, at least outside the Editor.
            if (this.positionAuthority != CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight)
            {
                double3 llh = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);
                this._longitude = llh.x;
                this._latitude = llh.y;
                this._height = llh.z;
            }

            if (this.positionAuthority != CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed)
            {
                this._ecefX = ecef.x;
                this._ecefY = ecef.y;
                this._ecefZ = ecef.z;
            }

            // If the ECEF position changes, update the orientation based on the
            // new position on the globe (if desired).
            if (this.adjustOrientationForGlobeWhenMoving &&
                this._lastPositionEcef != null &&
                (this._lastPositionEcef.Value.x != this._ecefX || this._lastPositionEcef.Value.y != this._ecefY || this._lastPositionEcef.Value.z != this._ecefZ))
            {
                double3 newPosition = new double3(this._ecefX, this._ecefY, this._ecefZ);
                CesiumGlobeAnchor.AdjustOrientation(this, this._lastPositionEcef.Value, newPosition);
            }

            double3 unityLocal = this._georeference.TransformEarthCenteredEarthFixedPositionToUnity(ecef);

            // Set the object's transform with the new position
            this.gameObject.transform.localPosition = new Vector3((float)unityLocal.x, (float)unityLocal.y, (float)unityLocal.z);
            this._lastLocalToWorld = this.transform.localToWorldMatrix;
            this._lastPositionEcef = new double3(this._ecefX, this._ecefY, this._ecefZ);
            this._initializeFromTransform = false;
        }

        private void UpdateGlobePositionFromTransform()
        {
            this._initializeFromTransform = true;
            this.UpdateGlobePosition(this._positionAuthority);
        }

        // This is static so that CesiumGlobeAnchor does not need finalization.
        private static partial void AdjustOrientation(CesiumGlobeAnchor anchor, double3 oldPositionEcef, double3 newPositionEcef);

        #endregion
    }
}
