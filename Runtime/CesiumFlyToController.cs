using System;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A controller that can smoothly fly to locations around the globe while 
    /// offering control over the characteristics of its flights.
    /// </summary>
    /// <remarks>
    /// This controller is compatible with CesiumCameraController. During flights,
    /// it will disable inputs on CesiumCameraController as necessary, such as
    /// camera rotation with the mouse.
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("Cesium/Cesium Fly To Controller")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public class CesiumFlyToController : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve _flyToAltitudeProfileCurve;

        /// <summary>
        /// A curve that dictates what percentage of the max altitude the controller 
        /// should take at a given time on the curve.
        /// </summary>
        /// <remarks>
        /// This curve must be kept in the 0 to 1 range on both axes. The 
        /// <see cref="CesiumFlyToController.flyToMaximumAltitudeCurve"/>
        /// dictates the actual max altitude at each point along the curve.
        /// </remarks>
        public AnimationCurve flyToAltitudeProfileCurve
        {
            get => this._flyToAltitudeProfileCurve;
            set => this._flyToAltitudeProfileCurve = value;
        }

        [SerializeField]
        private AnimationCurve _flyToProgressCurve;

        /// <summary>
        /// A curve that is used to determine the progress percentage for all the other 
        /// curves.
        /// </summary>
        public AnimationCurve flyToProgressCurve
        {
            get => this._flyToProgressCurve;
            set => this._flyToProgressCurve = value;
        }

        [SerializeField]
        private AnimationCurve _flyToMaximumAltitudeCurve;

        /// <summary>
        /// A curve that dictates the maximum altitude at each point along the curve.
        /// </summary>
        /// <remarks>
        /// This can be used in conjuction with 
        /// <see cref="CesiumFlyToController.flyToAltitudeProfileCurve"/> to allow the 
        /// controller to take some altitude during the flight.
        /// </remarks>
        public AnimationCurve flyToMaximumAltitudeCurve
        {
            get => this._flyToMaximumAltitudeCurve;
            set => this._flyToMaximumAltitudeCurve = value;
        }

        [SerializeField]
        [Min(0.0f)]
        private double _flyToDuration = 5.0;

        /// <summary>
        /// The length in seconds that the flight should last.
        /// </summary>
        public double flyToDuration
        {
            get => this._flyToDuration;
            set => this._flyToDuration = Math.Max(value, 0.0);
        }

        #region Deprecated Functionality
        [Min(0.0f)]
        private double _flyToGranularityDegrees = 0.01;

        /// <summary>
        /// The granularity in degrees with which keypoints should be generated for the 
        /// flight interpolation. This value should be greater than 0.0, otherwise
        /// the controller will not take flight.
        /// </summary>
        /// <remarks>
        /// This represents the difference in degrees between each keypoint on the flight path.
        /// The lower the value, the more keypoints are generated, and the smoother the flight
        /// interpolation will be.
        /// </remarks>
        [Obsolete("CesiumFlyToController no longer works using keypoints. This value has no effect.")]
        public double flyToGranularityDegrees
        {
            get => this._flyToGranularityDegrees;
            set
            {
                this._flyToGranularityDegrees = Math.Max(value, 0.0);
                if (this._flyToGranularityDegrees == 0.0)
                {
                    Debug.LogWarning(
                        "FlyToGranularityDegrees must be set to a non-zero value. " +
                        "Otherwise, CesiumFlyToController will not fly to any " +
                        "specified location.");
                }
            }
        }
        #endregion

        /// <summary>
        /// Encapsulates a method that is called whenever the controller finishes flying.
        /// </summary>
        public delegate void CompletedFlightDelegate();

        /// <summary>
        /// An event that is raised when the controller finishes flying.
        /// </summary>
        public event CompletedFlightDelegate OnFlightComplete;

        /// <summary>
        /// Encapsulates a method that is called whenever the controller's flight is interrupted.
        /// </summary>
        public delegate void InterruptedFlightDelegate();

        /// <summary>
        /// An event that is raised when the controller's flight is interrupted.
        /// </summary>
        public event InterruptedFlightDelegate OnFlightInterrupted;

        private CesiumGeoreference _georeference;
        private CesiumGlobeAnchor _globeAnchor;
        private CesiumSimplePlanarEllipsoidCurve _flightPath;
        private double _flightPathLength;

        private CesiumCameraController _cameraController;

        private Quaternion _sourceRotation;
        private Quaternion _destinationRotation;

        private double3 _destinationECEF;
        private double3 _previousPositionECEF;

        private double _maxHeight;

        private double _currentFlyToTime = 0.0;

        private bool _flyingToLocation = false;
        private bool _canInterruptFlight = true;

        void Awake()
        {
            this._georeference = this.gameObject.GetComponentInParent<CesiumGeoreference>();
            if (this._georeference == null)
            {
                Debug.LogError(
                    "CesiumFlyToController must be nested under a game object " +
                    "with a CesiumGeoreference.");
            }

            this._globeAnchor = this.gameObject.GetComponentInParent<CesiumGlobeAnchor>();
            if (this._globeAnchor == null)
            {
                Debug.LogError("CesiumFlyToController expects a CesiumGlobeAnchor to be attached to itself or to a parent");
            }

            CesiumOriginShift originShift = this._globeAnchor.GetComponent<CesiumOriginShift>();
            if (originShift == null)
            {
                Debug.LogError("CesiumFlyToController expects a CesiumOriginShift to be attached to the CesiumGlobeAnchor above it");
            }

            // If a CesiumCameraController is present, CesiumFlyToController will account for
            // its inputs and modify it during flights.
            this._cameraController = this.gameObject.GetComponent<CesiumCameraController>();
        }

        void Update()
        {
            if (this._flyingToLocation)
            {
                this.HandleFlightStep(Time.deltaTime);
            }
        }

#if UNITY_EDITOR
        // Ensures required components are present in the editor.
        private void Reset()
        {
            CesiumGlobeAnchor anchor = this.gameObject.GetComponentInParent<CesiumGlobeAnchor>();
            if(anchor == null)
            {
                anchor = this.gameObject.AddComponent<CesiumGlobeAnchor>();
                Debug.LogWarning("CesiumFlyToController missing a CesiumGlobeAnchor - adding");
            }

            CesiumOriginShift origin = anchor.GetComponent<CesiumOriginShift>();
            if(origin == null)
            {
                origin = anchor.gameObject.AddComponent<CesiumOriginShift>();
                Debug.LogWarning("CesiumFlyToController missing a CesiumOriginShift - adding");
            }
        }
#endif

        /// <summary>
        /// Whether this controller detects movement from other controllers on the game object.
        /// </summary>
        /// <remarks>
        /// This function is only called if CesiumFlyToController is in flight. When the controller
        /// is in flight, CesiumGlobeAnchor.detectTransformChanges is disabled. This means that
        /// any changes to the transform will only be detected if those changes were synced to
        /// the globe anchor by another controller.
        /// </remarks>
        /// <returns>Whether or not movement was detected.</returns>
        private bool DetectMovementInput()
        {
            double3 currentPositionECEF = this._globeAnchor.positionGlobeFixed;
            bool3 positionEquality = currentPositionECEF == this._previousPositionECEF;
            return !positionEquality.x || !positionEquality.y || !positionEquality.z;
        }

        /// <summary>
        /// Advance the controller's flight based on the given time delta.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This function requires the CesiumGeoreference to be valid. If it is not valid,
        /// then this function will do nothing.
        /// </para>
        /// <para>
        /// The given delta will be added to the _currentFlyTime, and the position
        /// and orientation will be computed by interpolating the _keypoints
        /// and _flyToSourceRotation/_flyToDestinationRotation based on this time.
        /// </para>
        /// <para>
        /// The position will be set via the globe anchor's setter, while the
        /// orientation is assigned directly to the transform.
        /// </para>
        /// </remarks>
        /// <param name="deltaTime"> The time delta in seconds.</param>
        private void HandleFlightStep(float deltaTime)
        {
            if (!this._flyingToLocation || this._flightPath == null)
            {
                return;
            }

            if (this._georeference == null)
            {
                this._flyingToLocation = false;
                return;
            }

            if (this._canInterruptFlight && this.DetectMovementInput())
            {
                this.InterruptFlight();
                return;
            }

            this._currentFlyToTime += deltaTime;

            double flyPercentage = 0.0;
            if (this._currentFlyToTime >= this._flyToDuration)
            {
                flyPercentage = 1.0;
            }
            else if (this._flyToProgressCurve != null && this._flyToProgressCurve.length > 0)
            {
                // Sample the progress curve if we have one
                flyPercentage = math.clamp(this._flyToProgressCurve.Evaluate((float)(this._currentFlyToTime / this._flyToDuration)), 0.0, 1.0);
            }
            else
            {
                flyPercentage = this._currentFlyToTime / this._flyToDuration;
            }

            // If we're have it to the end of the flight, or if the flight we're taking isn't actually moving or rotating us at all, we're done.
            if (flyPercentage == 1.0 || (this._flightPathLength == 0.0 && this._sourceRotation == this._destinationRotation))
            {
                this.CompleteFlight();
                return;
            }

            // Calculate the height above the surface. If we have a profile curve, use it as well.
            double altituteOffset = 0.0;
            if (this._maxHeight != 0.0 && this.flyToAltitudeProfileCurve != null && this.flyToAltitudeProfileCurve.length > 0)
            {
                double curveOffset = this._maxHeight * this.flyToAltitudeProfileCurve.Evaluate((float)flyPercentage);
                altituteOffset += curveOffset;
            }

            // Update position.
            double3 currentPosition = this._flightPath.GetPosition(flyPercentage, altituteOffset);
            this._previousPositionECEF = currentPosition;
            this._globeAnchor.positionGlobeFixed = currentPosition;

            Quaternion currentQuat = Quaternion.Slerp(this._sourceRotation, this._destinationRotation, (float)flyPercentage);
            this._globeAnchor.rotationEastUpNorth = currentQuat;
        }

        private void CompleteFlight()
        {
            this._globeAnchor.positionGlobeFixed = _destinationECEF;
            this._globeAnchor.rotationEastUpNorth = this._destinationRotation;

            this._flyingToLocation = false;
            this._currentFlyToTime = 0.0;

            this._globeAnchor.adjustOrientationForGlobeWhenMoving = true;
            this._globeAnchor.detectTransformChanges = true;

            if (this._cameraController != null)
            {
                this._cameraController.enableMovement = true;
                this._cameraController.enableRotation = true;
            }

            if (this.OnFlightComplete != null)
            {
                this.OnFlightComplete();
            }
        }

        private void InterruptFlight()
        {
            this._flyingToLocation = false;

            // Set the controller's roll to 0.0
            Vector3 angles = this.transform.eulerAngles;
            angles.z = 0.0f;
            this.transform.eulerAngles = angles;

            this._globeAnchor.adjustOrientationForGlobeWhenMoving = true;
            this._globeAnchor.detectTransformChanges = true;

            if (this._cameraController != null)
            {
                this._cameraController.enableMovement = true;
                this._cameraController.enableRotation = true;
            }

            if (this.OnFlightInterrupted != null)
            {
                this.OnFlightInterrupted();
            }
        }

        private void ComputeFlightPath(
            double3 sourceECEF,
            double3 destinationECEF,
            float yawAtDestination,
            float pitchAtDestination)
        {
            // The source and destination rotations are expressed in East-Up-North coordinates.
            pitchAtDestination = Mathf.Clamp(pitchAtDestination, -89.99f, 89.99f);
            this._sourceRotation = this._globeAnchor.transform.rotation;
            this._destinationRotation = Quaternion.Euler(pitchAtDestination, yawAtDestination, 0.0f);
            this._destinationECEF = destinationECEF;

            this._flightPath = CesiumSimplePlanarEllipsoidCurve.FromEarthCenteredEarthFixedCoordinates(sourceECEF, destinationECEF);
            this._flightPathLength = math.length(sourceECEF - destinationECEF);

            this._maxHeight = 0.0;
            if (this._flyToAltitudeProfileCurve != null && this._flyToMaximumAltitudeCurve.length > 0)
            {
                this._maxHeight = 30000.0;
                if (this._flyToMaximumAltitudeCurve != null && this._flyToMaximumAltitudeCurve.length > 0)
                {
                    this._maxHeight = this._flyToMaximumAltitudeCurve.Evaluate((float)this._flightPathLength);
                }
            }

            this._previousPositionECEF = sourceECEF;
            this._flyingToLocation = true;
        }

        /// <summary>
        /// Begin a smooth flight to the given Earth-Centered, Earth-Fixed (ECEF) 
        /// destination such that the controller ends at the specified yaw and pitch.
        /// </summary>
        /// <remarks>
        /// The characteristics of the flight can be configured with 
        /// <see cref="CesiumFlyToController.flyToAltitudeProfileCurve"/>,
        /// <see cref="CesiumFlyToController.flyToProgressCurve"/>, 
        /// <see cref="CesiumFlyToController.flyToMaximumAltitudeCurve"/>,
        /// <see cref="CesiumFlyToController.flyToDuration"/>, and
        /// <see cref="CesiumFlyToController.flyToGranularityDegrees"/>.
        /// </remarks>
        /// <param name="destination">The destination in ECEF coordinates.</param>
        /// <param name="yawAtDestination">The yaw of the controller at the destination.</param>
        /// <param name="pitchAtDestination">The pitch of the controller at the destination.</param>
        /// <param name="canInterruptByMoving">Whether the flight can be interrupted with movement inputs.</param>
        public void FlyToLocationEarthCenteredEarthFixed(
            double3 destination,
            float yawAtDestination,
            float pitchAtDestination,
            bool canInterruptByMoving)
        {
            if (this._flyingToLocation)
            {
                return;
            }

            pitchAtDestination = Mathf.Clamp(pitchAtDestination, -89.99f, 89.99f);

            // Compute source location in ECEF
            double3 source = this._globeAnchor.positionGlobeFixed;

            this.ComputeFlightPath(source, destination, yawAtDestination, pitchAtDestination);

            // Indicate that the controller will be flying from now
            this._flyingToLocation = true;
            this._canInterruptFlight = canInterruptByMoving;
            this._globeAnchor.adjustOrientationForGlobeWhenMoving = false;
            this._globeAnchor.detectTransformChanges = false;

            if (this._cameraController != null)
            {
                this._cameraController.enableMovement = canInterruptByMoving;
                this._cameraController.enableRotation = false;
            }
        }

        /// <summary>
        /// Begin a smooth flight to the given Earth-Centered, Earth-Fixed (ECEF) 
        /// destination such that the controller ends at the specified yaw and pitch.
        /// </summary>
        /// <remarks>
        /// The characteristics of the flight can be configured with 
        /// <see cref="CesiumFlyToController.flyToAltitudeProfileCurve"/>,
        /// <see cref="CesiumFlyToController.flyToProgressCurve"/>, 
        /// <see cref="CesiumFlyToController.flyToMaximumAltitudeCurve"/>,
        /// <see cref="CesiumFlyToController.flyToDuration"/>, and
        /// <see cref="CesiumFlyToController.flyToGranularityDegrees"/>.
        /// </remarks>
        /// <param name="destination">The destination in ECEF coordinates.</param>
        /// <param name="yawAtDestination">The yaw of the controller at the destination.</param>
        /// <param name="pitchAtDestination">The pitch of the controller at the destination.</param>
        /// <param name="canInterruptByMoving">Whether the flight can be interrupted with movement inputs.</param>
        public void FlyToLocationEarthCenteredEarthFixed(
            Vector3 destination,
            float yawAtDestination,
            float pitchAtDestination,
            bool canInterruptByMoving)
        {
            this.FlyToLocationEarthCenteredEarthFixed(
                new double3()
                {
                    x = destination.x,
                    y = destination.y,
                    z = destination.z,
                },
                yawAtDestination,
                pitchAtDestination,
                canInterruptByMoving);
        }

        /// <summary>
        /// Begin a smooth flight to the given WGS84 longitude in degrees (x),
        /// latitude in degrees (y), and height in meters (z) such that the controller ends 
        /// at the specified yaw and pitch.
        /// </summary>
        /// <remarks>
        /// The characteristics of the flight can be configured with 
        /// <see cref="CesiumFlyToController.flyToAltitudeProfileCurve"/>,
        /// <see cref="CesiumFlyToController.flyToProgressCurve"/>, 
        /// <see cref="CesiumFlyToController.flyToMaximumAltitudeCurve"/>,
        /// <see cref="CesiumFlyToController.flyToDuration"/>, and
        /// <see cref="CesiumFlyToController.flyToGranularityDegrees"/>.
        /// </remarks>
        /// <param name="destination">The longitude (x), latitude (y), and height (z) of the destination.</param>
        /// <param name="yawAtDestination">The yaw of the controller at the destination.</param>
        /// <param name="pitchAtDestination">The pitch of the controller at the destination.</param>
        /// <param name="canInterruptByMoving">Whether the flight can be interrupted with movement inputs.</param>
        public void FlyToLocationLongitudeLatitudeHeight(
            double3 destination,
            float yawAtDestination,
            float pitchAtDestination,
            bool canInterruptByMoving)
        {
            double3 destinationECEF =
                CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(destination);

            this.FlyToLocationEarthCenteredEarthFixed(
                destinationECEF,
                yawAtDestination,
                pitchAtDestination,
                canInterruptByMoving);
        }

        /// <summary>
        /// Begin a smooth flight to the given WGS84 longitude in degrees (x),
        /// latitude in degrees (y), and height in meters (z) such that the controller ends 
        /// at the specified yaw and pitch.
        /// </summary>
        /// <remarks>
        /// The characteristics of the flight can be configured with 
        /// <see cref="CesiumFlyToController.flyToAltitudeProfileCurve"/>,
        /// <see cref="CesiumFlyToController.flyToProgressCurve"/>, 
        /// <see cref="CesiumFlyToController.flyToMaximumAltitudeCurve"/>,
        /// <see cref="CesiumFlyToController.flyToDuration"/>, and
        /// <see cref="CesiumFlyToController.flyToGranularityDegrees"/>.
        /// </remarks>
        /// <param name="destination">The longitude (x), latitude (y), and height (z) of the destination.</param>
        /// <param name="yawAtDestination">The yaw of the controller at the destination.</param>
        /// <param name="pitchAtDestination">The pitch of the controller at the destination.</param>
        /// <param name="canInterruptByMoving">Whether the flight can be interrupted with movement inputs.</param>
        public void FlyToLocationLongitudeLatitudeHeight(
            Vector3 destination,
            float yawAtDestination,
            float pitchAtDestination,
            bool canInterruptByMoving)
        {
            double3 destinationCoordinates = new double3()
            {
                x = destination.x,
                y = destination.y,
                z = destination.z
            };
            double3 destinationECEF =
                CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(
                    destinationCoordinates);

            this.FlyToLocationEarthCenteredEarthFixed(
                destinationECEF,
                yawAtDestination,
                pitchAtDestination,
                canInterruptByMoving);
        }
    }
}
