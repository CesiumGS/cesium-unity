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
        /// The `Transform` attached to the same object is the only authority for the position of
        /// this object.
        /// </summary>
        None,

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
        /// The <see cref="CesiumGlobeAnchor.unityWorldX"/>, <see cref="CesiumGlobeAnchor.unityWorldY"/>,
        /// and <see cref="CesiumGlobeAnchor.unityWorldZ"/> properties authoritatively define the position
        /// of this object.
        /// </summary>
        UnityWorldCoordinates,

        /// <summary>
        /// The <see cref="CesiumGlobeAnchor.unityLocalX"/>, <see cref="CesiumGlobeAnchor.unityLocalY"/>,
        /// and <see cref="CesiumGlobeAnchor.unityLocalZ"/> properties authoritatively define the position
        /// of this object.
        /// </summary>
        UnityLocalCoordinates
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
    /// Otherwise, this component will throw an exception at `Start`.
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
    /// </remarks>
    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumGlobeAnchorImpl", "CesiumGlobeAnchorImpl.h")]
    public partial class CesiumGlobeAnchor : MonoBehaviour
    {
#region User-editable properties

        [SerializeField]
        private bool _adjustOrientationForGlobeWhenMoving = true;

        public bool adjustOrientationForGlobeWhenMoving
        {
            get => this._adjustOrientationForGlobeWhenMoving;
            set => this._adjustOrientationForGlobeWhenMoving = value;
        }

        [SerializeField]
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
        private CesiumGlobeAnchorPositionAuthority _positionAuthority = CesiumGlobeAnchorPositionAuthority.None;

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

        public double ecefZ
        {
            get => this._ecefZ;
            set
            {
                this._ecefZ = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed;
            }
        }

        [SerializeField]
        private double _unityWorldX = 0.0;

        public double unityWorldX
        {
            get => this._unityWorldX;
            set
            {
                this._unityWorldX = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.UnityWorldCoordinates;
            }
        }

        [SerializeField]
        private double _unityWorldY = 0.0;

        public double unityWorldY
        {
            get => this._unityWorldY;
            set
            {
                this._unityWorldY = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.UnityWorldCoordinates;
            }
        }

        [SerializeField]
        private double _unityWorldZ = 0.0;

        public double unityWorldZ
        {
            get => this._unityWorldZ;
            set
            {
                this._unityWorldZ = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.UnityLocalCoordinates;
            }
        }

        [SerializeField]
        private double _unityLocalX = 0.0;

        public double unityLocalX
        {
            get => this._unityLocalX;
            set
            {
                this._unityLocalX = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.UnityLocalCoordinates;
            }
        }

        [SerializeField]
        private double _unityLocalY = 0.0;

        public double unityLocalY
        {
            get => this._unityLocalY;
            set
            {
                this._unityLocalY = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.UnityLocalCoordinates;
            }
        }

        [SerializeField]
        private double _unityLocalZ = 0.0;

        public double unityLocalZ
        {
            get => this._unityLocalZ;
            set
            {
                this._unityLocalZ = value;
                this.positionAuthority = CesiumGlobeAnchorPositionAuthority.UnityLocalCoordinates;
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
            this.positionAuthority = CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight;
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
            this.positionAuthority = CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed;
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
            this._unityWorldX = x;
            this._unityWorldY = y;
            this._unityWorldZ = z;
            this.positionAuthority = CesiumGlobeAnchorPositionAuthority.UnityWorldCoordinates;
        }

        /// <summary>
        /// Sets the position of this object to particular X, Y, Z coordinates in the
        /// local coordinate system defined by this object's parent.
        /// </summary>
        /// <remarks>
        /// Calling this method is more efficient than setting the properties individually.
        /// </remarks>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public void SetPositionUnityLocal(double x, double y, double z)
        {
            this._unityLocalX = x;
            this._unityLocalY = y;
            this._unityLocalZ = z;
            this.positionAuthority = CesiumGlobeAnchorPositionAuthority.UnityLocalCoordinates;
        }

        /// <summary>
        /// Synchronizes the properties of this `CesiumGlobeAnchor`.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is usually not necessary to call this method because it is called automatically when
        /// needed. However, it may be be useful in special situations or when the timing of the
        /// automatic calls is not ideal. It performs the following actions.
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
            if (this._lastPropertiesAreValid && this._lastLocalToWorld != this.transform.localToWorldMatrix)
                this.UpdateGlobePositionFromTransform();
            else
                this.UpdateGlobePosition(this._positionAuthority);
        }

#endregion

#region Private properties

        private bool _lastPropertiesAreValid = false;
        private double _lastPositionEcefX = 0.0;
        private double _lastPositionEcefY = 0.0;
        private double _lastPositionEcefZ = 0.0;
        // TODO: use just the position instead of the entire transform?
        private Matrix4x4 _lastLocalToWorld;

#endregion

#region Unity Messages

        private void Start()
        {
            this.Sync();
        }

        private void OnEnable()
        {
            // We must do this in OnEnable instead of Start because Start doesn't re-run on domain reload,
            // so if we only did it in Start, then in the Editor, transform change detection would stop
            // working whenever source files were changed.
            this.StartOrStopDetectingTransformChanges();
        }

#endregion

#region Coroutines

        public void StartOrStopDetectingTransformChanges()
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
            WaitUntil waitForChanges = new WaitUntil(() => this._lastPropertiesAreValid && !this.transform.localToWorldMatrix.Equals(this._lastLocalToWorld));

            while (true)
            {
                yield return waitForChanges;
                this.UpdateGlobePositionFromTransform();
            }
        }

#endregion

#region Updaters

        private void UpdateGlobePosition(CesiumGlobeAnchorPositionAuthority previousAuthority)
        {
            CesiumGeoreference georeference = this.gameObject.GetComponentInParent<CesiumGeoreference>();
            if (georeference == null)
                throw new InvalidOperationException("CesiumGlobeAnchor is not nested inside a game object with a CesiumGeoreference.");

            // If there's no authoritative position, copy the position from the Transform.
            if (this.positionAuthority == CesiumGlobeAnchorPositionAuthority.None)
            {
                Vector3 position = this.transform.localPosition;
                this._unityLocalX = position.x;
                this._unityLocalY = position.y;
                this._unityLocalZ = position.z;
                this._positionAuthority = CesiumGlobeAnchorPositionAuthority.UnityLocalCoordinates;
            }

            // Convert the authoritative position to ECEF
            double3 ecef;
            switch (this.positionAuthority)
            {
                case CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight:
                    ecef = CesiumTransforms.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(
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
                case CesiumGlobeAnchorPositionAuthority.UnityWorldCoordinates:
                    ecef = georeference.TransformUnityWorldPositionToEarthCenteredEarthFixed(new double3(
                        this.unityWorldX,
                        this.unityWorldY,
                        this.unityWorldZ
                    ));
                    break;
                case CesiumGlobeAnchorPositionAuthority.UnityLocalCoordinates:
                    ecef = georeference.TransformUnityLocalPositionToEarthCenteredEarthFixed(this.transform.parent, new double3(
                        this.unityLocalX,
                        this.unityLocalY,
                        this.unityLocalZ
                    ));
                    break;
                default:
                    throw new InvalidOperationException("Unknown value for positionAuthority.");
            }

            // Update the non-authoritative fields with the new position.
            // TODO: it might be more efficient to lazily update these if/when they're accessed, at least outside the Editor.
            if (this.positionAuthority != CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight)
            {
                double3 llh = CesiumTransforms.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);
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

            if (this.positionAuthority != CesiumGlobeAnchorPositionAuthority.UnityWorldCoordinates)
            {
                double3 unityWorld = georeference.TransformEarthCenteredEarthFixedPositionToUnityWorld(ecef);
                this._unityWorldX = unityWorld.x;
                this._unityWorldY = unityWorld.y;
                this._unityWorldZ = unityWorld.z;
            }

            if (this.positionAuthority != CesiumGlobeAnchorPositionAuthority.UnityLocalCoordinates)
            {
                double3 unityLocal = georeference.TransformEarthCenteredEarthFixedPositionToUnityLocal(this.transform.parent, ecef);
                this._unityLocalX = unityLocal.x;
                this._unityLocalY = unityLocal.y;
                this._unityLocalZ = unityLocal.z;
            }

            // If the ECEF position changes, update the orientation based on the
            // new position on the globe (if desired).
            if (this.adjustOrientationForGlobeWhenMoving &&
                previousAuthority != CesiumGlobeAnchorPositionAuthority.None &&
                this._lastPropertiesAreValid &&
                (this._lastPositionEcefX != this._ecefX || this._lastPositionEcefY != this._ecefY || this._lastPositionEcefZ != this._ecefZ))
            {
                double3 oldPosition = new double3(
                    this._lastPositionEcefX,
                    this._lastPositionEcefY,
                    this._lastPositionEcefZ
                );
                double3 newPosition = new double3(this._ecefX, this._ecefY, this._ecefZ);
                CesiumGlobeAnchor.AdjustOrientation(this, oldPosition, newPosition);
            }

            // Set the object's transform with the new position
            this.gameObject.transform.localPosition = new Vector3((float)this._unityLocalX, (float)this._unityLocalY, (float)this._unityLocalZ);
            this._lastLocalToWorld = this.transform.localToWorldMatrix;
            this._lastPositionEcefX = this._ecefX;
            this._lastPositionEcefY = this._ecefY;
            this._lastPositionEcefZ = this._ecefZ;
            this._lastPropertiesAreValid = true;
        }

        private void UpdateGlobePositionFromTransform()
        {
            this.positionAuthority = CesiumGlobeAnchorPositionAuthority.None;
        }

        // This is static so that CesiumGlobeAnchor does not need finalization.
        private static partial void AdjustOrientation(CesiumGlobeAnchor anchor, double3 oldPositionEcef, double3 newPositionEcef);

#endregion
    }
}
