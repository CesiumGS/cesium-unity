using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CesiumForUnity
{
    public class CesiumCameraController : MonoBehaviour
    {
        #region User-editable properties

        [SerializeField]
        [Tooltip("This curve dictates what percentage of the max altitude the " +
            "camera should take at a given time on the curve." +
            "\n\n" +
            "This curve must be kept in the 0 to 1 range on both axes. The " +
            "\"Fly To Maximum Altitude Curve\" dictates the actual max " +
            "altitude at each point along the curve.")]
        private AnimationCurve _flyToAltitudeProfileCurve;

        /// <summary>
        /// A curve that dictates what percentage of the max altitude the camera 
        /// should take at a given time on the curve.
        /// </summary>
        /// <remarks>
        /// This curve must be kept in the 0 to 1 range on both axes. The 
        /// <see cref="CesiumCameraController.flyToMaximumAltitudeCurve"/>
        /// dictates the actual max altitude at each point along the curve.
        /// </remarks>
        public AnimationCurve flyToAltitudeProfileCurve
        {
            get => this._flyToAltitudeProfileCurve;
            set => this._flyToAltitudeProfileCurve = value;
        }

        [SerializeField]
        [Tooltip("This curve is used to determine the progress percentage for " +
            "all the other curves. This allows us to accelerate and deaccelerate " +
            "as wanted throughout the curve.")]
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
        [Tooltip("This curve dictates the maximum altitude at each point along " +
            "the curve." +
            "\n\n" +
            "This can be used in conjunction with the \"Fly To Altitude Profile " +
            "Curve\" to allow the camera to take some altitude during the flight.")]
        private AnimationCurve _flyToMaximumAltitudeCurve;

        /// <summary>
        /// A curve that dictates the maximum altitude at each point along the curve.
        /// </summary>
        /// <remarks>
        /// This can be used in conjuction with 
        /// <see cref="CesiumCameraController.flyToAltitudeProfileCurve"/> to allow the 
        /// camera to take some altitude during the flight.
        /// </remarks>
        public AnimationCurve flyToMaximumAltitudeCurve
        {
            get => this._flyToMaximumAltitudeCurve;
            set => this._flyToMaximumAltitudeCurve = value;
        }

        [SerializeField]
        [Tooltip("The length in seconds that the flight should last.")]
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
        [Tooltip("The granularity in degrees with which keypoints should be generated " +
            "for the flight interpolation.")]
        [Min(0.0f)]
        private double _flyToGranularityDegrees = 0.01;

        /// <summary>
        /// The granularity in degrees with which keypoints should be generated for the 
        /// flight interpolation.
        /// </summary>
        public double flyToGranularityDegrees
        {
            get => this._flyToGranularityDegrees;
            set => this._flyToGranularityDegrees = Math.Max(value, 0.0);
        }

        /// <summary>
        /// Encapsulates a method that is called whenever the camera finishes flying.
        /// </summary>
        public delegate void CompletedFlightDelegate();

        /// <summary>
        /// An event that is raised when the camera finishes flying.
        /// </summary>
        public static event CompletedFlightDelegate OnFlightComplete;

        /// <summary>
        /// Encapsulates a method that is called whenever the camera's flight is interrupted.
        /// </summary>
        public delegate void InterruptedFlightDelegate();

        /// <summary>
        /// An event that is raised when the camera's flight is interrupted.
        /// </summary>
        public static event InterruptedFlightDelegate OnFlightInterrupted;

        #endregion

        #region Private variables

        private CesiumGeoreference _georeference;
        private CesiumGlobeAnchor _globeAnchor;

        private List<double3> _keypoints = new List<double3>();

        private double _currentFlyToTime = 0.0;
        private Quaternion _flyToSourceRotation = Quaternion.identity;
        private Quaternion _flyToDestinationRotation = Quaternion.identity;

        private bool _flyingToLocation = false;
        private bool _canInterruptFlight = true;

        #endregion

        #region Input configuration

#if ENABLE_INPUT_SYSTEM
        InputAction lookAction;
        InputAction moveAction;
        InputAction moveUpAction;

        void ConfigureInputs()
        {
            InputActionMap map = new InputActionMap("Cesium Camera Controller");

            lookAction = map.AddAction("look", binding: "<Mouse>/delta");
            lookAction.AddBinding("<Gamepad>/rightStick").WithProcessor("scaleVector2(x=15, y=15)");

            moveAction = map.AddAction("move", binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            moveUpAction = map.AddAction("moveUp");
            moveUpAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/space")
                .With("Down", "<Keyboard>/c")
                .With("Up", "<Keyboard>/e")
                .With("Down", "<Keyboard>/q")
                .With("Up", "<Gamepad>/rightshoulder")
                .With("Down", "<Gamepad>/leftshoulder");

            moveAction.Enable();
            lookAction.Enable();
            moveUpAction.Enable();
        }
#endif

        #endregion

        #region Monobehaviour Functions

        void OnEnable()
        {
            this._georeference = this.gameObject.GetComponentInParent<CesiumGeoreference>();
            if (this._georeference == null)
            {
                Debug.LogError(
                    "CesiumCameraController must be nested under a game object " +
                    "with a CesiumGeoreference.");
            }

            this._globeAnchor = this.gameObject.GetComponent<CesiumGlobeAnchor>();
            if (this._globeAnchor == null)
            {
                Debug.LogError(
                    "CesiumCameraController must also have a CesiumGlobeAnchor component to work.");
            }

            #if ENABLE_INPUT_SYSTEM
            ConfigureInputs();
            #endif
        }

        void Update()
        {
            HandlePlayerInputs();

            if (this._flyingToLocation)
            {
                HandleFlightStep(Time.deltaTime);
            }
        }

        #endregion

        #region Movement

        private float _mouseSensitivity = 0.2f;

        private void HandlePlayerInputs()
        {
            #if ENABLE_INPUT_SYSTEM
            Vector2 lookDelta = lookAction.ReadValue<Vector2>();
            float inputRotateAxisX = lookDelta.x * this._mouseSensitivity;
            float inputRotateAxisY = lookDelta.y * this._mouseSensitivity;

            Vector2 moveDelta = moveAction.ReadValue<Vector2>();
            float inputForward = moveDelta.y;
            float inputRight = moveDelta.x;

            float inputUp = moveUpAction.ReadValue<Vector2>().y;

            #else
            float inputRotateAxisX = Input.GetAxis("Mouse X") * this._mouseSensitivity;
            inputRotateAxisX += Input.GetAxis("Controller Right Stick X");

            float inputRotateAxisY = Input.GetAxis("Mouse Y") * this._mouseSensitivity;
            inputRotateAxisY += Input.GetAxis("Controller Right Stick Y");

            float inputForward = Input.GetAxis("Vertical");
            float inputRight = Input.GetAxis("Horizontal");
            float inputUp = Input.GetAxis("YAxis");
#endif

            // These inputs may seem flipped, but rotation around the Y-axis
            // corresponds to lateral rotation, while rotation around the X-axis
            // corresponds to vertical rotation.
            this.Rotate(inputRotateAxisY, inputRotateAxisX);
            this.MoveForward(inputForward);
            this.MoveRight(inputRight);
            this.MoveUp(inputUp);
        }

        void Rotate(float valueX, float valueY)
        {
            if(valueX == 0.0f && valueY == 0.0f)
            {
                return;
            }

            float newRotationX = this.transform.localEulerAngles.x - valueX;
            newRotationX = Mathf.Clamp(newRotationX, 0.0f, 180.0f); // TODO: positive rotation goes down towards ground, not up.

            float newRotationY = this.transform.localEulerAngles.y + valueY;

            // Weird clamping code due to weird Euler angle mapping...
            /*if (rotationX <= 90.0f && newRotationX >= 0.0f)
                newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
            if (rotationX >= 270.0f)
                newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);
            */

            this.transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, this.transform.localEulerAngles.z);
        }

        void MoveRight(float value)
        {
            if(value == 0.0f)
            {
                return;
            }

            this.MoveAlongVector(this.transform.right, value);
        }

        void MoveForward(float value)
        {
            if(value == 0.0f)
            {
                return;
            }

            this.MoveAlongVector(this.transform.forward, value);
        }

        void MoveUp(float value)
        {
            if (value == 0.0f || this._globeAnchor == null)
            {
                return;
            }

            /*double3 positionECEF = new double3()
            {
                x = this._globeAnchor.ecefX,
                y = this._globeAnchor.ecefY,
                z = this._globeAnchor.ecefZ,
            };
            double3 upECEF = CesiumEllipsoid.GeodeticSurfaceNormal(positionECEF);
            double3 upUnity =
                this._georeference.TransformEarthCenteredEarthFixedDirectionToUnity(upECEF);

            this.MoveAlongVector((float3)upUnity, value);*/
        }

        private void MoveAlongVector(Vector3 vector, float value)
        {
            this.transform.position += vector * value;

            if (this._flyingToLocation && this._canInterruptFlight)
            {
                InterruptFlight();
            }
        }

        #endregion

        #region Fly-To private helpers
        /// <summary>
        /// Advance the camera flight based on the given time delta.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This function requires the CesiumGlobeAnchor to be valid. If it is not valid,
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
            if (this._globeAnchor == null || this._keypoints.Count == 0)
            {
                this._flyingToLocation = false;
                return;
            }

            this._currentFlyToTime += (double)deltaTime;

            // If we reached the end, set actual destination location and orientation
            if (this._currentFlyToTime >= this.flyToDuration)
            {
                double3 finalPoint = this._keypoints[this._keypoints.Count - 1];
                this._globeAnchor.SetPositionEarthCenteredEarthFixed(
                    finalPoint.x,
                    finalPoint.y,
                    finalPoint.z);

                this.transform.rotation = this._flyToDestinationRotation;

                this._flyingToLocation = false;
                this._currentFlyToTime = 0.0;

                OnFlightComplete();

                return;
            }

            // We're currently in flight. Interpolate the position and orientation:
            double percentage = this._currentFlyToTime / this.flyToDuration;

            // In order to accelerate at start and slow down at end, we use a progress
            // profile curve
            double flyPercentage = percentage;
            if (this.flyToProgressCurve != null)
            {
                flyPercentage = Math.Clamp(
                    (double)this.flyToProgressCurve.Evaluate((float)percentage),
                    0.0,
                    1.0);
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
            // Set Location
            this._globeAnchor.SetPositionEarthCenteredEarthFixed(
                currentPosition.x,
                currentPosition.y,
                currentPosition.z);

            // Interpolate rotation in the ESU frame. The local ESU rotation will
            // be transformed to the appropriate world rotation as we fly.
            this.transform.rotation = Quaternion.Slerp(
                this._flyToSourceRotation,
                this._flyToDestinationRotation,
                (float)flyPercentage);
        }

        private void InterruptFlight()
        {
            this._flyingToLocation = false;

            // Set the camera roll to 0.0
            Vector3 angles = this.transform.eulerAngles;
            angles.z = 0.0f;
            this.transform.eulerAngles = angles;

            OnFlightInterrupted();
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

            float3 normalizedSource = (float3)math.normalize(sourceECEF);
            float3 normalizedDestination = (float3)math.normalize(destinationECEF);
            Quaternion flyQuat =
                Quaternion.FromToRotation(normalizedSource, normalizedDestination);

            float flyTotalAngle = 0.0f;
            Vector3 flyRotationAxis = Vector3.zero;
            flyQuat.ToAngleAxis(out flyTotalAngle, out flyRotationAxis);

            int steps = Math.Max(
                (int)(flyTotalAngle / Mathf.Deg2Rad * this.flyToGranularityDegrees) - 1,
                0);
            this._keypoints.Clear();
            this._currentFlyToTime = 0.0;

            if (flyTotalAngle == 0.0f &&
                this._flyToSourceRotation == this._flyToDestinationRotation)
            {
                return;
            }

            // We don't create a curve projected along the ellipsoid because we want to take
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
            double3? scaled = null;// CesiumEllipsoid.ScaleToGeodeticSurface(sourceECEF);
            if (scaled != null)
            {
                sourceAltitude = math.length(sourceECEF - (double3)scaled);
            }

            scaled = null;// CesiumEllipsoid.ScaleToGeodeticSurface(destinationECEF);
            if (scaled != null)
            {
                destinationAltitude = math.length(destinationECEF - (double3)scaled);
            }

            // Get distance between source and destination points to compute a wanted
            // altitude from the curve.
            double flyToDistance = math.length(destinationECEF - sourceECEF);

            this._keypoints.Add(sourceECEF);
            for (int step = 1; step <= steps; step++)
            {
                double stepDouble = (double)step;
                double percentage = stepDouble / (steps + 1);
                double altitude = math.lerp(sourceAltitude, destinationAltitude, percentage);
                double phi = Mathf.Deg2Rad * this.flyToGranularityDegrees * stepDouble;

                float3 rotated = Quaternion.AngleAxis((float)phi, flyRotationAxis) * (float3)sourceUpVector;
                scaled = null;// CesiumEllipsoid.ScaleToGeodeticSurface((double3)rotated);
                if (scaled != null)
                {
                    double3 upVector = math.normalize((double3)scaled);

                    // Add an altitude if we have a profile curve for it
                    double offsetAltitude = 0;
                    if (this.flyToAltitudeProfileCurve != null)
                    {
                        double maxAltitude = 30000;
                        if (this.flyToMaximumAltitudeCurve != null)
                        {
                            maxAltitude = (double)
                                    this.flyToMaximumAltitudeCurve.Evaluate((float)flyToDistance);
                        }
                        offsetAltitude = (double)maxAltitude * this.flyToAltitudeProfileCurve.Evaluate((float)percentage);
                    }

                    double3 point = (double3)scaled + upVector * (altitude + offsetAltitude);
                    this._keypoints.Add(point);
                }
            }

            this._keypoints.Add(destinationECEF);
        }

        #endregion

        #region Fly-To public API

        public void FlyToLocationEarthCenteredEarthFixed(
            double3 destination,
            float yawAtDestination,
            float pitchAtDestination,
            bool canInterruptByMoving)
        {
            if (this._flyingToLocation || this._globeAnchor == null)
            {
                return;
            }

            pitchAtDestination = Mathf.Clamp(pitchAtDestination, -89.99f, 89.99f);

            // Compute source location in ECEF
            double3 source = new double3()
            {
                x = this._globeAnchor.ecefX,
                y = this._globeAnchor.ecefY,
                z = this._globeAnchor.ecefZ
            };

            this.ComputeFlightPath(source, destination, yawAtDestination, pitchAtDestination);

            // Indicate that the camera will be flying from now
            this._flyingToLocation = true;
            this._canInterruptFlight = canInterruptByMoving;
        }

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

        public void FlyToLocationLongitudeLatitudeHeight(
            double3 destination,
            float yawAtDestination,
            float pitchAtDestination,
            bool canInterruptByMoving)
        {
            double3 destinationECEF =
                CesiumTransforms.LongitudeLatitudeHeightToEarthCenteredEarthFixed(destination);

            this.FlyToLocationEarthCenteredEarthFixed(
                destinationECEF,
                yawAtDestination,
                pitchAtDestination,
                canInterruptByMoving);
        }


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
                CesiumTransforms.LongitudeLatitudeHeightToEarthCenteredEarthFixed(
                    destinationCoordinates);

            this.FlyToLocationEarthCenteredEarthFixed(
                destinationECEF,
                yawAtDestination,
                pitchAtDestination,
                canInterruptByMoving);
        }

        #endregion
    }
}
