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
    /// An anchored game object is still allowed to move. It may be moved either by setting
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
    /// be misaligned with other globe-aligned objects that are transformed differently. However, it is
    /// sometimes useful to purposely offset an object or group of objects.
    /// </para>
    /// </remarks>
    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumGlobeAnchorImpl", "CesiumGlobeAnchorImpl.h", staticOnly: true)]
    [AddComponentMenu("Cesium/Cesium Globe Anchor")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumGlobeAnchor : MonoBehaviour, ICesiumRestartable
    {
        #region Fields

        // These fields are marked internal so that they can be accessed from C++.
        // See https://github.com/CesiumGS/cesium-unity/issues/210

        [SerializeField]
        internal bool _adjustOrientationForGlobeWhenMoving = true;

        [SerializeField]
        internal bool _detectTransformChanges = true;

        [SerializeField]
        internal double4x4 _localToGlobeFixedMatrix = double4x4.identity;

        // True if _localToGlobeFixedMatrix has a valid value.
        // False if it has not yet been initialized from the Transform.
        [SerializeField]
        internal bool _localToGlobeFixedMatrixIsValid = false;

        // The last known local Transform, used to detect changes in the Transform so that
        // the precise globe coordinates can be recomputed from it. These are invalid before OnEnable.
        [NonSerialized]
        internal bool _lastLocalsAreValid = false;

        [NonSerialized]
        internal Vector3 _lastLocalPosition;

        [NonSerialized]
        internal Quaternion _lastLocalRotation;

        [NonSerialized]
        internal Vector3 _lastLocalScale;

        // The resolved georeference containing this globe anchor. This is just a cache
        // of `GetComponentInParent<CesiumGeoreference>()`.
        [NonSerialized]
        internal CesiumGeoreference _georeference;

        #endregion

        #region User-editable properties

        /// <summary>
        /// Gets or sets whether to adjust the game object's orientation based on globe curvature as the
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

        /// <summary>
        /// Gets or sets whether to automatically detect changes in the game object's <code>Transform</code>
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

        /// <summary>
        /// Gets or sets the 4x4 transformation matrix from this game object's local coordinate system
        /// to the Earth-Centered, Earth-Fixed (ECEF) coordinate system.
        /// </summary>
        /// <remarks>
        /// The ECEF coordinate system is a right-handed system located at the center of the Earth.
        /// The +X axis points to the intersection of the Equator and Prime Meridian (zero degrees
        /// longitude). The +Y axis points to the intersection of the Equator and +90 degrees
        /// longitude. The +Z axis points up through the North Pole.
        /// </remarks>
        public double4x4 localToGlobeFixedMatrix
        {
            get
            {
                this.InitializeEcefIfNeeded();
                return this._localToGlobeFixedMatrix;
            }
            set
            {
                this.InitializeEcefIfNeeded();
                this.UpdateEcef(value);
            }
        }

        /// <summary>
        /// Gets or sets the longitude, latitude, and height of this object. The Longitude (X) is in the range -180 to 180 degrees.
        /// The Latitude (Y) is in the range -90 to 90 degrees. The Height (Z) is measured in meters above the WGS84
        /// ellipsoid. Do not confused an ellipsoidal height with a geoid height or height above mean sea level, which
        /// can be tens of meters higher or lower depending on where in the world the object is located.
        /// </summary>
        /// <remarks>
        /// When the position is set via this property, it is internally converted to and stored in Earth-Centered,
        /// Earth-Fixed Coordinates. As a result, getting this property will not necessarily return
        /// the exact value that was set.
        /// </remarks>
        public double3 longitudeLatitudeHeight
        {
            get => CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(this.positionGlobeFixed);
            set
            {
                this.positionGlobeFixed = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(value);
            }
        }

        /// <summary>
        /// Gets or sets the game object's position in the Earth-Centered, Earth-Fixed (ECEF) coordinates in meters.
        /// </summary>
        /// <remarks>
        /// See <see cref="localToGlobeFixedMatrix"/> for an explanation of the ECEF coordinate system.
        /// </remarks>
        public double3 positionGlobeFixed
        {
            get => this.localToGlobeFixedMatrix.c3.xyz;
            set
            {
                double4x4 newModelToEcef = this.localToGlobeFixedMatrix;
                newModelToEcef.c3 = new double4(value.x, value.y, value.z, 1.0);
                this.localToGlobeFixedMatrix = newModelToEcef;
            }
        }

        /// <summary>
        /// Gets or sets the rotation from the game object's coordinate system to the
        /// Earth-Centered, Earth-Fixed axes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <see cref="localToGlobeFixedMatrix"/> for an explanation of the ECEF coordinate system.
        /// </para>
        /// <para>
        /// When the rotation is set via this property, it is internally converted to and stored in the
        /// <see cref="localToGlobeFixedMatrix"/> property. As a result, getting this property will not
        /// necessarily return the exact value that was set.
        /// </para>
        /// </remarks>
        public quaternion rotationGlobeFixed
        {
            get
            {
                double3 translation;
                quaternion rotation;
                double3 scale;

                Helpers.MatrixToTranslationRotationAndScale(this.localToGlobeFixedMatrix, out translation, out rotation, out scale);

                return rotation;
            }
            set
            {
                double3 translation;
                quaternion rotation;
                double3 scale;

                Helpers.MatrixToTranslationRotationAndScale(this.localToGlobeFixedMatrix, out translation, out rotation, out scale);
                this.localToGlobeFixedMatrix = Helpers.TranslationRotationAndScaleToMatrix(translation, value, scale);
            }
        }

        /// <summary>
        /// Gets or sets the rotation from the game object's coordinate system to a local coordinate system
        /// centered on this object where the +X points in the local East direction, the +Y axis points in
        /// the local Up direction, and the +Z axis points in the local North direction.
        /// </summary>
        /// <remarks>
        /// When the rotation is set via this property, it is internally converted to and stored in the
        /// <see cref="localToGlobeFixedMatrix"/> property. As a result, getting this property will not
        /// necessarily return the exact value that was set.
        /// </remarks>
        public quaternion rotationEastUpNorth
        {
            get
            {
                this.InitializeEcefIfNeeded();
                return this.GetLocalToEastUpNorthRotation();
            }
            set
            {
                this.InitializeEcefIfNeeded();
                this.SetLocalToEastUpNorthRotation(value);
            }
        }

        /// <summary>
        /// Gets the scale from the game object's coordinate system to the Earth-Centered,
        /// Earth-Fixed coordinate system. Because ECEF is right-handed and Unity is left-handed,
        /// this scale will almost always be negative.
        /// </summary>
        /// <remarks>
        /// See <see cref="localToGlobeFixedMatrix"/> for an explanation of the ECEF coordinate system.
        /// </remarks>
        public double3 scaleGlobeFixed
        {
            get
            {
                double3 translation;
                quaternion rotation;
                double3 scale;

                Helpers.MatrixToTranslationRotationAndScale(this.localToGlobeFixedMatrix, out translation, out rotation, out scale);

                return scale;
            }
            set
            {
                double3 translation;
                quaternion rotation;
                double3 scale;

                Helpers.MatrixToTranslationRotationAndScale(this.localToGlobeFixedMatrix, out translation, out rotation, out scale);
                this.localToGlobeFixedMatrix = Helpers.TranslationRotationAndScaleToMatrix(translation, rotation, value);
            }
        }

        /// <summary>
        /// Gets or sets the scale from the game object's coordinate system to a local coordinate system
        /// centered on this object where the +X points in the local East direction, the +Y axis points in
        /// the local Up direction, and the +Z axis points in the local North direction.
        /// </summary>
        /// <remarks>
        /// When the rotation is set via this property, it is internally converted to and stored in the
        /// <see cref="localToGlobeFixedMatrix"/> property. As a result, getting this property will not
        /// necessarily return the exact value that was set.
        /// </remarks>
        public double3 scaleEastUpNorth
        {
            get => -this.scaleGlobeFixed;
            set => this.scaleGlobeFixed = -value;
        }

        #endregion

        #region Deprecated Functionality

        [Obsolete("Use positionGlobeFixed.x instead.")]
        public double ecefX
        {
            get => this.positionGlobeFixed.x;
            set
            {
                double3 position = this.positionGlobeFixed;
                position.x = value;
                this.positionGlobeFixed = position;
            }
        }

        [Obsolete("Use positionGlobeFixed.y instead.")]
        public double ecefY
        {
            get => this.positionGlobeFixed.y;
            set
            {
                double3 position = this.positionGlobeFixed;
                position.y = value;
                this.positionGlobeFixed = position;
            }
        }

        [Obsolete("Use positionGlobeFixed.z instead.")]
        public double ecefZ
        {
            get => this.positionGlobeFixed.z;
            set
            {
                double3 position = this.positionGlobeFixed;
                position.z = value;
                this.positionGlobeFixed = position;
            }
        }

        [Obsolete("Use longitudeLatitudeHeight.x instead.")]
        public double longitude
        {
            get => this.longitudeLatitudeHeight.x;
            set
            {
                double3 position = this.longitudeLatitudeHeight;
                position.x = value;
                this.longitudeLatitudeHeight = position;
            }
        }

        [Obsolete("Use longitudeLatitudeHeight.y instead.")]
        public double latitude
        {
            get => this.longitudeLatitudeHeight.y;
            set
            {
                double3 position = this.longitudeLatitudeHeight;
                position.y = value;
                this.longitudeLatitudeHeight = position;
            }
        }

        [Obsolete("Use longitudeLatitudeHeight.z instead.")]
        public double height
        {
            get => this.longitudeLatitudeHeight.z;
            set
            {
                double3 position = this.longitudeLatitudeHeight;
                position.z = value;
                this.longitudeLatitudeHeight = position;
            }
        }

        [Obsolete("Set the longitudeLatitudeHeight property instead.")]
        public void SetPositionLongitudeLatitudeHeight(double longitude, double latitude, double height)
        {
            this.longitudeLatitudeHeight = new double3(longitude, latitude, height);
        }

         
        [Obsolete("Set the positionGlobeFixed property instead.")]
        public void SetPositionEarthCenteredEarthFixed(double x, double y, double z)
        {
            this.positionGlobeFixed = new double3(x, y, z);
        }

        #endregion

        #region Public Methods

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
        /// If the <see cref="localToGlobeFixedMatrix"/> transform has not yet been set, it is computed
        /// from the game object's current `Transform`.
        /// </item>
        /// <item>
        /// If the game object's `Transform` has changed since the last time `OnEnable`,
        /// or if a position setter was called, this method updates all of this instance's position
        /// properties from the current transform. This works even if <see cref="detectTransformChanges"/> is
        /// disabled. It will also update the object's orientation if
        /// <see cref="adjustOrientationForGlobeWhenMoving"/> is enabled.
        /// </item>
        /// <item>
        /// If the origin of the <see cref="CesiumGeoreference"/> has changed, the game object's `Transform` is
        /// updated based on the <see cref="localToGlobeFixedMatrix"/> transform and the new georeference origin.
        /// </item>
        /// </list>
        /// </remarks>
        public void Sync()
        {
            // If we don't have a local -> globe fixed matrix yet, we must update from the Transform
            bool updateFromTransform = !this._localToGlobeFixedMatrixIsValid;
            if (!updateFromTransform && this._lastLocalsAreValid)
            {
                // We may also need to update from the Transform if it has changed
                // since the last time we computed the local -> globe fixed matrix.
                updateFromTransform =
                    this._lastLocalPosition != this.transform.localPosition ||
                    this._lastLocalRotation != this.transform.localRotation ||
                    this._lastLocalScale != this.transform.localScale;
            }

            if (updateFromTransform)
                this.UpdateEcefFromTransform();
            else
                this.UpdateEcef(this._localToGlobeFixedMatrix);
        }

        /// <summary>
        /// Completely re-initializes the state of this object from its serialized properties. This is called automatically
        /// by `OnEnable` and is not usually necessary to call directly. It can sometimes be useful after Unity has
        /// modified the private, serializable fields of this instance. For example: after an undo or redo operation.
        /// </summary>
        public void Restart()
        {
            this.UpdateGeoreference();

            if (this._georeference == null)
                Debug.LogWarning($"{this.gameObject.name} is not nested inside a game object with a CesiumGeoreference. Most of its CesiumGlobeAnchor functionality will not work.");

            this.Sync();
            this.StartOrStopDetectingTransformChanges();
        }

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
                if (this.isActiveAndEnabled)
                    this._georeference.AddGlobeAnchor(this);
            }
        }

        internal void UpdateGeoreferenceIfNecessary()
        {
            if (this._georeference == null)
                this.UpdateGeoreference();
        }

        private void OnEnable()
        {
            this.Restart();
        }

        /// <summary>
        /// Called by the Editor when the user chooses to "reset" the component.
        /// The implementation here makes sure the newly-reset values for the serialized
        /// properties are applied.
        /// </summary>
        private void Reset()
        {
            this.Restart();
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
            WaitUntil waitForChanges = new WaitUntil(() => this._lastLocalsAreValid && (
                this.transform.localPosition != this._lastLocalPosition ||
                this.transform.localRotation != this._lastLocalRotation ||
                this.transform.localScale != this._lastLocalScale));

            while (true)
            {
                yield return waitForChanges;
                this.UpdateEcefFromTransform();
            }
        }

        #endregion

        #region Updaters

        private void InitializeEcefIfNeeded()
        {
            if (!this._localToGlobeFixedMatrixIsValid)
                this.UpdateEcefFromTransform();
        }

        private void UpdateEcef(double4x4 newModelToEcef)
        {
            this.UpdateGeoreferenceIfNecessary();
            this.SetNewLocalToGlobeFixedMatrix(newModelToEcef);
        }

        private void UpdateEcefFromTransform()
        {
            this.UpdateGeoreferenceIfNecessary();
            if (this._georeference != null)
                this.SetNewLocalToGlobeFixedMatrixFromTransform();
#if UNITY_EDITOR
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
        }

        /// <summary>
        /// Sets a new model-to-ECEF matrix, rotating it for the new globe position if
        /// <see cref="adjustOrientationForGlobeWhenMoving"/> is true and also updating
        /// the object's Transform. Be sure the georeference is initialized before
        /// calling this method.
        /// </summary>
        /// <param name="newLocalToGlobeFixedMatrix">The new transformation matrix from the local coordinate system to Earth-Centered, Earth-Fixed.</param>
        private partial void SetNewLocalToGlobeFixedMatrix(double4x4 newLocalToGlobeFixedMatrix);

        /// <summary>
        /// Sets a new model-to-ECEF matrix from the current value of the object's
        /// Transform. Also rotates the object for the new globe position if
        /// <see cref="adjustOrientationForGlobeWhenMoving"/> is true. Be sure that the
        /// georeference is intialized before calling this method.
        /// </summary>
        private partial void SetNewLocalToGlobeFixedMatrixFromTransform();

        /// <summary>
        /// Gets the current rotation from the local axes to a set of axes where
        /// +X points East, +Y points Up, and +Z points East at the object's position.
        /// Be sure that the georeference is initialized before calling this method.
        /// </summary>
        /// <returns>The rotation.</returns>
        private partial quaternion GetLocalToEastUpNorthRotation();

        /// <summary>
        /// Sets the current rotation from the model's axes to a set of axes where
        /// +X points East, +Y points Up, and +Z points East at the object's position.
        /// Be sure that the georeference is initialized before calling this method.
        /// </summary>
        /// <param name="newRotation">The new rotation.</param>
        private partial void SetLocalToEastUpNorthRotation(quaternion newRotation);

        #endregion
    }
}
