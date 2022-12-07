using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// A camera controller that can easily move around the globe while maintaining a
    /// sensible orientation. As the camera moves across the horizon, it automatically
    /// changes its own up direction such that the world always looks right-side up.
    /// </summary>
    public class CesiumCameraController : MonoBehaviour
    {
        #region User-editable properties

        [SerializeField]
        private bool _enableAltitudeProfileCurve = false;

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
        private bool _enableProgressCurve = false;

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
        private bool _enableMaximumAltitudeCurve = false;

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

        private CharacterController _controller;
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

        #region MonoBehaviour Functions

        void OnEnable()
        {
            this._controller = this.gameObject.GetComponent<CharacterController>();
            
            if(this._controller == null)
            {
                this._controller = this.gameObject.AddComponent<CharacterController>();
            }

            this._controller.radius = 1.0f;
            this._controller.height = 1.0f;
            this._controller.center = Vector3.zero;
            this._controller.detectCollisions = true;

            this._georeference = this.gameObject.GetComponentInParent<CesiumGeoreference>();
            
            if (this._georeference == null)
            {
                Debug.LogError(
                    "CesiumCameraController must be nested under a game object " +
                    "with a CesiumGeoreference.");
            }

            if(this.gameObject.GetComponent<CesiumOriginShift>() == null)
            {
                this.gameObject.AddComponent<CesiumOriginShift>();
            }

            // Adding a CesiumOriginShift also adds a CesiumGlobeAnchor automatically.
            this._globeAnchor = this.gameObject.GetComponent<CesiumGlobeAnchor>();

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

        #region Player movement

        private float _mouseSensitivity = 0.2f;
        private float _movementSpeed = 24.0f;

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

            if (!this._flyingToLocation)
            {
                // The rotation inputs may seem flipped, but this is correct.
                // The X-movement of the mouse corresponds to rotation around the Y-axis,
                // while the Y-movement of the mouse corresponds to rotation around the X-axis.
                this.Rotate(inputRotateAxisY, inputRotateAxisX);
            }

            this.MoveForward(inputForward);
            this.MoveRight(inputRight);
            this.MoveUp(inputUp);
        }

        /// <summary>
        /// Rotate the camera around its local X- and Y-axes with the specified amounts.
        /// </summary>
        /// <remarks>
        /// Rotation around the X-axis corresponds to vertical rotation (i.e. looking up or down).
        /// Rotation around the Y-axis corresponds to lateral rotation (i.e. looking left or right).
        /// </remarks>
        /// <param name="valueX">The amount to rotate around the X-axis.</param>
        /// <param name="valueY">The amount to rotate around the Y-axis.</param>
        void Rotate(float valueX, float valueY)
        {
            if(valueX == 0.0f && valueY == 0.0f)
            {
                return;
            }

            // Rotation around the X-axis occurs counter-clockwise, so the look range
            // maps to [270, 360] degrees for the upper quarter-sphere of motion, and
            // [0, 90] degrees for the lower. Euler angles only work with positive values,
            // so map the [0, 90] range to [360, 450] so the entire range is [270, 450].
            // This makes it easy to clamp the values.
            float rotationX = this.transform.localEulerAngles.x;
            if(rotationX <= 90.0f)
            {
                rotationX += 360.0f;
            }

            float newRotationX = Mathf.Clamp(rotationX - valueX, 270.0f, 450.0f);
            float newRotationY = this.transform.localEulerAngles.y + valueY;
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

            double3 positionECEF = new double3()
            {
                x = this._globeAnchor.ecefX,
                y = this._globeAnchor.ecefY,
                z = this._globeAnchor.ecefZ,
            };
            double3 upECEF = CesiumEllipsoid.GeodeticSurfaceNormal(positionECEF);
            double3 upUnity =
                this._georeference.TransformEarthCenteredEarthFixedDirectionToUnity(upECEF);

            this.MoveAlongVector((float3)upUnity, value);
        }

        private void MoveAlongVector(Vector3 vector, float value)
        {
            this._controller.Move(vector * value * Time.deltaTime * this._movementSpeed);

            if (this._flyingToLocation && this._canInterruptFlight)
            {
                InterruptFlight();
            }
        }

        #endregion

        #region Fly-To helper functions
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
            if (this._georeference == null || this._keypoints.Count == 0)
            {
                this._flyingToLocation = false;
                return;
            }

            this._currentFlyToTime += (double)deltaTime;

            // If we reached the end, set actual destination location and orientation
            if (this._currentFlyToTime >= this._flyToDuration)
            {
                double3 finalPoint = this._keypoints[this._keypoints.Count - 1];
                this._globeAnchor.SetPositionEarthCenteredEarthFixed(
                    finalPoint.x,
                    finalPoint.y,
                    finalPoint.z);

                this.transform.rotation = this._flyToDestinationRotation;

                this._flyingToLocation = false;
                this._currentFlyToTime = 0.0;

                if (OnFlightComplete != null)
                {
                    OnFlightComplete();
                }

                return;
            }

            // We're currently in flight. Interpolate the position and orientation:
            double percentage = this._currentFlyToTime / this._flyToDuration;

            // In order to accelerate at start and slow down at end, we use a progress
            // profile curve
            double flyPercentage = percentage;
            if (this._enableProgressCurve)
            {
                flyPercentage = Math.Clamp(
                    (double)this._flyToProgressCurve.Evaluate((float)percentage),
                    0.0,
                    1.0);
            }

            Debug.Log(flyPercentage);
            // Find the keypoint indexes corresponding to the current percentage
            double keypointValue = flyPercentage * (this._keypoints.Count - 1);
            int lastKeypointIndex = (int)Math.Floor(keypointValue);
            double segmentPercentage = keypointValue - lastKeypointIndex;
            int nextKeypointIndex = lastKeypointIndex + 1;

            // Get the current position by interpolating linearly between those two points
            double3 lastPosition = this._keypoints[lastKeypointIndex];
            double3 nextPosition = this._keypoints[nextKeypointIndex];
            double3 currentPosition = math.lerp(lastPosition, nextPosition, segmentPercentage);
            this._globeAnchor.SetPositionEarthCenteredEarthFixed(
                currentPosition.x,
                currentPosition.y,
                currentPosition.z);

            // Interpolate rotation in the EUN frame. The local EUN rotation will
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

            this._globeAnchor.adjustOrientationForGlobeWhenMoving = true;
            this._globeAnchor.detectTransformChanges = true;

            if (OnFlightInterrupted != null)
            {
                OnFlightInterrupted();
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

            int steps = Math.Max(
                (int)(flyTotalAngle / (Mathf.Deg2Rad * this.flyToGranularityDegrees)) - 1,
                0);

            this._keypoints.Clear();
            this._currentFlyToTime = 0.0;

            if (flyTotalAngle == 0.0f &&
                this._flyToSourceRotation == this._flyToDestinationRotation)
            {
                return;
            }

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
            double3? scaled = CesiumEllipsoid.ScaleToGeodeticSurface(sourceECEF);
            if (scaled != null)
            {
                sourceAltitude = math.length(sourceECEF - (double3)scaled);
            }

            scaled = CesiumEllipsoid.ScaleToGeodeticSurface(destinationECEF);
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
                scaled = CesiumEllipsoid.ScaleToGeodeticSurface((double3)rotated);
                if (scaled != null)
                {
                    double3 scaledValue = (double3)scaled;
                    double3 upVector = math.normalize(scaledValue);

                    // Add an altitude if we have a profile curve for it
                    double offsetAltitude = 0;
                    if (this._enableAltitudeProfileCurve)
                    {
                        double maxAltitude = 30000;
                        if (this._enableMaximumAltitudeCurve)
                        {
                            maxAltitude = (double)
                                    this.flyToMaximumAltitudeCurve.Evaluate((float)flyToDistance);
                        }
                        offsetAltitude = (double)maxAltitude * this.flyToAltitudeProfileCurve.Evaluate((float)percentage);
                    }

                    double3 point = scaledValue + upVector * (altitude + offsetAltitude);
                    this._keypoints.Add(point);
                }
            }

            this._keypoints.Add(destinationECEF);
        }

        #endregion

        #region Fly-To public API

        internal void FlyToLocationEarthCenteredEarthFixed(
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

        /// <summary>
        /// Begin a smooth camera flight to the given Earth-Centered, Earth-Fixed (ECEF) 
        /// destination such that the camera ends at the specified yaw and pitch.
        /// </summary>
        /// <remarks>
        /// The characteristics of the flight can be configured with 
        /// <see cref="CesiumCameraController.flyToAltitudeProfileCurve"/>,
        /// <see cref="CesiumCameraController.flyToProgressCurve"/>, 
        /// <see cref="CesiumCameraController.flyToMaximumAltitudeCurve"/>,
        /// <see cref="CesiumCameraController.flyToDuration"/>, and
        /// <see cref="CesiumCameraController.flyToGranularityDegrees"/>.
        /// </remarks>
        /// <param name="destination">The destination in ECEF coordinates.</param>
        /// <param name="yawAtDestination">The yaw of the camera at the destination.</param>
        /// <param name="pitchAtDestination">The pitch of the camera at the destination.</param>
        /// <param name="canInterruptByMoving">Whether the camera flight can be interrupted with movement inputs.</param>
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

        internal void FlyToLocationLongitudeLatitudeHeight(
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

        /// <summary>
        /// Begin a smooth camera flight to the given  WGS84 longitude in degrees (x),
        /// latitude in degrees (y), and height in meters (z) such that the camera ends 
        /// at the specified yaw and pitch.
        /// </summary>
        /// <remarks>
        /// The characteristics of the flight can be configured with 
        /// <see cref="CesiumCameraController.flyToAltitudeProfileCurve"/>,
        /// <see cref="CesiumCameraController.flyToProgressCurve"/>, 
        /// <see cref="CesiumCameraController.flyToMaximumAltitudeCurve"/>,
        /// <see cref="CesiumCameraController.flyToDuration"/>, and
        /// <see cref="CesiumCameraController.flyToGranularityDegrees"/>.
        /// </remarks>
        /// <param name="destination">The longitude (x), latitude (y), and height (z) of the destination.</param>
        /// <param name="yawAtDestination">The yaw of the camera at the destination.</param>
        /// <param name="pitchAtDestination">The pitch of the camera at the destination.</param>
        /// <param name="canInterruptByMoving">Whether the camera flight can be interrupted with movement inputs.</param>
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
