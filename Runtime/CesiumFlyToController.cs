using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

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
    [RequireComponent(typeof(CesiumOriginShift))]
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

        [SerializeField]
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

        private CesiumCameraController _cameraController;

        private List<double3> _keypoints = new List<double3>();
        private double3 _lastPositionECEF = new double3(0.0, 0.0, 0.0);

        private double _currentFlyToTime = 0.0;
        private Quaternion _flyToSourceRotation = Quaternion.identity;
        private Quaternion _flyToDestinationRotation = Quaternion.identity;

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

            // CesiumOriginShift will add a CesiumGlobeAnchor automatically.
            this._globeAnchor = this.gameObject.GetComponent<CesiumGlobeAnchor>();

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
            bool3 positionEquality = currentPositionECEF == this._lastPositionECEF;
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
            if (this._georeference == null || this._keypoints.Count == 0)
            {
                this._flyingToLocation = false;
                return;
            }

            if (this._canInterruptFlight && this.DetectMovementInput())
            {
                this.InterruptFlight();
                return;
            }

            this._currentFlyToTime += (double)deltaTime;

            // If we reached the end, set actual destination location and orientation
            if (this._currentFlyToTime >= this._flyToDuration)
            {
                this.CompleteFlight();
                return;
            }

            // We're currently in flight. Interpolate the position and orientation:
            double percentage = this._currentFlyToTime / this._flyToDuration;

            // In order to accelerate at start and slow down at end, we use a progress
            // profile curve
            double flyPercentage = percentage;
            if (this._flyToProgressCurve != null && this._flyToProgressCurve.length > 0)
            {
                flyPercentage = Math.Clamp(
                    (double)this._flyToProgressCurve.Evaluate((float)percentage),
                    0.0,
                    1.0);
            }

            if (Mathf.Approximately((float)flyPercentage, 1.0f))
            {
                this.CompleteFlight();
                return;
            }

            // Find the keypoint indexes corresponding to the current percentage
            double keypointValue = flyPercentage * (this._keypoints.Count - 1);
            int lastKeypointIndex = (int)Math.Floor(keypointValue);
            double segmentPercentage = keypointValue - lastKeypointIndex;
            int nextKeypointIndex = lastKeypointIndex + 1;

            // Get the current position by interpolating linearly between those two points
            double3 lastPosition = this._keypoints[lastKeypointIndex];
            double3 nextPosition = this._keypoints[nextKeypointIndex];
            double3 currentPosition = math.lerp(lastPosition, nextPosition, segmentPercentage);
            this._globeAnchor.positionGlobeFixed = currentPosition;
            this._lastPositionECEF = currentPosition;

            // Interpolate rotation in the EUN frame. The local EUN rotation will
            // be transformed to the appropriate world rotation as we fly.
            this._globeAnchor.rotationEastUpNorth = Quaternion.Slerp(
                this._flyToSourceRotation,
                this._flyToDestinationRotation,
                (float)flyPercentage);
        }

        private void CompleteFlight()
        {
            double3 finalPoint = this._keypoints[this._keypoints.Count - 1];
            this._globeAnchor.positionGlobeFixed = finalPoint;
            
            this._globeAnchor.rotationEastUpNorth = this._flyToDestinationRotation;

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
            // The source and destination rotations are expressed in East-Up-North
            // coordinates.
            this._flyToSourceRotation = this.transform.rotation;
            this._flyToDestinationRotation =
                Quaternion.Euler(pitchAtDestination, yawAtDestination, 0);

            // Compute angle / axis transform and initialize key points
            float3 normalizedSource = (float3)math.normalize(sourceECEF);
            float3 normalizedDestination = (float3)math.normalize(destinationECEF);
            Quaternion flyQuat =
                Quaternion.FromToRotation(normalizedSource, normalizedDestination);

            float flyTotalAngle = 0.0f;
            Vector3 flyRotationAxis = Vector3.zero;
            flyQuat.ToAngleAxis(out flyTotalAngle, out flyRotationAxis);

            this._keypoints.Clear();
            this._currentFlyToTime = 0.0;

            if (this.flyToGranularityDegrees == 0.0)
            {
                Debug.LogError(
                    "CesiumFlyToController cannot fly when flyToGranularityDegrees " +
                    "is set to 0.");
                return;
            }

            if (flyTotalAngle == 0.0f &&
                this._flyToSourceRotation == this._flyToDestinationRotation)
            {
                return;
            }

            int steps = Math.Max(
                (int)(flyTotalAngle / (Mathf.Deg2Rad * this.flyToGranularityDegrees)) - 1,
                0);

            // We will not create a curve projected along the ellipsoid because we want to take
            // altitude while flying. The radius of the current point will evolve as follows:
            //  - Project the point on the ellipsoid: will give a default radius
            //  depending on ellipsoid location.
            //  - Interpolate the altitudes: get the source/destination altitudes, and make a
            //  linear interpolation between them. This will allow for flying from / to any
            //  point smoothly.
            //  - Add a flight profile offset /-\ defined by a curve.

            // Compute global radius at source and destination points
            double sourceRadius = math.length(sourceECEF);
            double3 sourceUpVector = sourceECEF;

            // Compute actual altitude at source and destination points by scaling on
            // ellipsoid.
            double sourceAltitude = 0.0, destinationAltitude = 0.0;
            double3? scaled = CesiumWgs84Ellipsoid.ScaleToGeodeticSurface(sourceECEF);
            if (scaled != null)
            {
                sourceAltitude = math.length(sourceECEF - (double3)scaled);
            }

            scaled = CesiumWgs84Ellipsoid.ScaleToGeodeticSurface(destinationECEF);
            if (scaled != null)
            {
                destinationAltitude = math.length(destinationECEF - (double3)scaled);
            }

            // Get distance between source and destination points to compute a wanted
            // altitude from the curve.
            double flyToDistance = math.length(destinationECEF - sourceECEF);

            this._keypoints.Add(sourceECEF);
            this._lastPositionECEF = sourceECEF;

            for (int step = 1; step <= steps; step++)
            {
                double stepDouble = (double)step;
                double percentage = stepDouble / (steps + 1);
                double altitude = math.lerp(sourceAltitude, destinationAltitude, percentage);
                double phi = Mathf.Deg2Rad * this.flyToGranularityDegrees * stepDouble;

                float3 rotated = Quaternion.AngleAxis((float)phi, flyRotationAxis) * (float3)sourceUpVector;
                scaled = CesiumWgs84Ellipsoid.ScaleToGeodeticSurface((double3)rotated);
                if (scaled != null)
                {
                    double3 scaledValue = (double3)scaled;
                    double3 upVector = math.normalize(scaledValue);

                    // Add an altitude if we have a profile curve for it
                    double offsetAltitude = 0;
                    if (this._flyToAltitudeProfileCurve != null && this._flyToAltitudeProfileCurve.length > 0)
                    {
                        double maxAltitude = 30000;
                        if (this._flyToMaximumAltitudeCurve != null && this._flyToMaximumAltitudeCurve.length > 0)
                        {
                            maxAltitude = (double)
                                    this._flyToMaximumAltitudeCurve.Evaluate((float)flyToDistance);
                        }
                        offsetAltitude =
                            (double)maxAltitude * this._flyToAltitudeProfileCurve.Evaluate((float)percentage);
                    }

                    double3 point = scaledValue + upVector * (altitude + offsetAltitude);
                    this._keypoints.Add(point);
                }
            }

            this._keypoints.Add(destinationECEF);
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
