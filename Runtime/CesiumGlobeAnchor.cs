using Reinterop;
using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UIElements;

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
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumGlobeAnchorImpl", "CesiumGlobeAnchorImpl.h")]
    public partial class CesiumGlobeAnchor : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private bool _adjustOrientationForGlobeWhenMoving = true;

        [SerializeField]
        private bool _detectTransformChanges = true;

        [SerializeField]
        private double4x4 _modelToEcef = double4x4.identity;

        // True if _localToEcef has a valid value.
        // False if it has not yet been initialized from the Transform.
        [SerializeField]
        private bool _modelToEcefIsValid = false;

        // The last known Transform, used to detect changes in the Transform so that
        // the precise globe coordinates can be recomputed from it. This is null before OnEnable.
        [NonSerialized]
        private Matrix4x4? _lastLocalToWorld = null;

        // The resolved georeference containing this globe anchor. This is just a cache
        // of `GetComponentInParent<CesiumGeoreference>()`.
        [NonSerialized]
        private CesiumGeoreference _georeference;

        #endregion

        #region User-editable properties

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

        public double4x4 modelToEcef
        {
            get
            {
                this.InitializeEcefIfNeeded();
                return this._modelToEcef;
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
        public double3 longitudeLatitudeHeight
        {
            get => CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(this.ecefPosition);
            set
            {
                this.ecefPosition = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(value);
            }
        }

        /// <summary>
        /// Gets or sets the position in the Earth-Centered, Earth-Fixed (ECEF) coordinates in meters. The ECEF coordinate
        /// system is a right-handed system located at the center of the Earth. The +X axis points to the intersection
        /// of the Equator and Prime Meridian (zero degrees longitude). The +Y axis points to the intersection of
        /// the Equator and +90 degrees longitude. The +Z axis points up through the North Pole.
        /// </summary>
        public double3 ecefPosition
        {
            get => this.modelToEcef.c3.xyz;
            set
            {
                this.InitializeEcefIfNeeded();
                double4x4 newModelToEcef = this._modelToEcef;
                newModelToEcef.c3 = new double4(value.x, value.y, value.z, 1.0);
                this.modelToEcef = newModelToEcef;
            }
        }

        /// <summary>
        /// Gets or sets the rotation from the object's coordinate system to the
        /// Earth-Centered, Earth-Fixed axes.
        /// </summary>
        public quaternion localToEcefRotation
        {
            get
            {
                this.InitializeEcefIfNeeded();

                double3 translation;
                quaternion rotation;
                double3 scale;

                Helpers.MatrixToTranslationRotationAndScale(this._modelToEcef, out translation, out rotation, out scale);

                return rotation;
            }
            //set
            //{
            //    this.InitializeEcefIfNeeded();
            //    this.UpdateGlobeRotation(value);
            //}
        }

        public double3 localToEcefScale
        {
            get
            {
                this.InitializeEcefIfNeeded();

                double3 translation;
                quaternion rotation;
                double3 scale;

                Helpers.MatrixToTranslationRotationAndScale(this._modelToEcef, out translation, out rotation, out scale);

                return scale;
            }
            //set
            //{
            //    this.InitializeEcefIfNeeded();
            //    this.UpdateGlobeScale(value);
            //}
        }

        #endregion

        #region Set Helpers

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
        /// If none of the ECEF properties on this instance have been set, it updates them all
        /// from the game object's current `Transform`.
        /// </item>
        /// <item>
        /// If the game object's `Transform` has changed since the last time `OnEnable`, this method,
        /// or a position setter was called, it updates all of this instance's position
        /// properties from the current transform. This works even if <see cref="detectTransformChanges"/> is
        /// disabled. It will also update the object's orientation if
        /// <see cref="adjustOrientationForGlobeWhenMoving"/> is enabled.
        /// </item>
        /// </list>
        /// </remarks>
        public void Sync()
        {
            if (!this._modelToEcefIsValid || (this._lastLocalToWorld != null && this._lastLocalToWorld != this.transform.localToWorldMatrix))
            {
                this.UpdateEcefFromTransform();
                return;
            }

            this.UpdateEcef(this._modelToEcef);
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
                this.UpdateEcefFromTransform();
            }
        }

        #endregion

        #region Updaters

        private void InitializeEcefIfNeeded()
        {
            if (!this._modelToEcefIsValid)
            {
                this.UpdateGeoreference();
                this.UpdateEcefFromTransform();
                this._modelToEcefIsValid = true;
            }
        }

        private void UpdateEcef(double4x4 newModelToEcef)
        {
            if (this._georeference == null)
            {
                this.UpdateGeoreference();
                if (this._georeference == null)
                    throw new InvalidOperationException("CesiumGlobeAnchor is not nested inside a game object with a CesiumGeoreference.");
            }

            // Update the Transform
            double4x4 modelToLocal = math.mul(this._georeference.ecefToLocalMatrix, newModelToEcef);

            double3 localPosition;
            quaternion localRotation;
            double3 localScale;
            Helpers.MatrixToTranslationRotationAndScale(
                modelToLocal,
                out localPosition,
                out localRotation,
                out localScale);

            Transform transform = this.transform;
            transform.localPosition = (float3)localPosition;
            transform.localRotation = localRotation;
            transform.localScale = (float3)localScale;

            this._lastLocalToWorld = this.transform.localToWorldMatrix;

            // If the ECEF position changes, update the orientation based on the
            // new position on the globe (if enabled).
            if (this.adjustOrientationForGlobeWhenMoving &&
                this._modelToEcefIsValid)
            {
                double3 oldPosition = this._modelToEcef.c3.xyz;
                double3 newPosition = newModelToEcef.c3.xyz;
                if (!oldPosition.Equals(newPosition))
                {
                    CesiumGlobeAnchor.AdjustOrientation(this, oldPosition, newPosition);
                }
            }

            this._modelToEcef = newModelToEcef;
        }

        private void UpdateEcefFromTransform()
        {
            if (this._georeference == null)
            {
                this.UpdateGeoreference();
                if (this._georeference == null)
                    throw new InvalidOperationException("CesiumGlobeAnchor is not nested inside a game object with a CesiumGeoreference.");
            }

            Transform transform = this.transform;

            // Compute the local transformation matrix, model -> local
            double4x4 modelToLocal = Helpers.ToMathematics(Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale));

            // Get the local -> ECEF transformation from the CesiumGeoreference
            double4x4 localToEcef = this._georeference.localToEcefMatrix;

            // Multiply to get a model -> ECEF transformation
            double4x4 modelToEcef = math.mul(localToEcef, modelToLocal);

            this.UpdateEcef(modelToEcef);
        }

        // This is static so that CesiumGlobeAnchor does not need finalization.
        private static partial void AdjustOrientation(CesiumGlobeAnchor anchor, double3 oldPositionEcef, double3 newPositionEcef);

        #endregion
    }
}
