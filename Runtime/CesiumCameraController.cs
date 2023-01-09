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
    /// A camera controller that can easily move around and view the globe while 
    /// maintaining a sensible orientation. As the camera moves across the horizon, 
    /// it automatically changes its own up direction such that the world always 
    /// looks right-side up.
    /// </summary>
    [RequireComponent(typeof(CesiumOriginShift))]
    public class CesiumCameraController : MonoBehaviour
    {
        #region User-editable properties

        [SerializeField]
        private bool _enableDynamicSpeed = true;

        /// <summary>
        /// Whether to enable dynamic speed on this controller. If enabled, 
        /// the controller's speed will change dynamically based on elevation 
        /// and other factors.
        /// </summary>
        public bool enableDynamicSpeed
        {
            get => this._enableDynamicSpeed;
            set => this._enableDynamicSpeed = value;
        }

        [SerializeField]
        [Min(0.0f)]
        private float _dynamicSpeedMinHeight = 20.0f;

        /// <summary>
        /// The minimum height where dynamic speed starts to take effect.
        /// Below this height, the speed will be set to the object's height 
        /// from the Earth, which makes it move slowly when it is right above a tileset.
        /// </summary>
        public float dynamicSpeedMinHeight
        {
            get => this._dynamicSpeedMinHeight;
            set => this._dynamicSpeedMinHeight = Mathf.Max(value, 0.0f);
        }

        [SerializeField]
        private bool _enableDynamicClippingPlanes = true;

        /// <summary>
        /// Whether to dynamically adjust the camera's clipping planes so that
        /// the globe will not be clipped from far away. Objects that are close 
        /// to the camera but far above the globe in space may not appear.
        /// </summary>
        public bool enableDynamicClippingPlanes
        {
            get => this._enableDynamicClippingPlanes;
            set => this._enableDynamicClippingPlanes = value;
        }

        [SerializeField]
        [Min(0.0f)]
        private float _dynamicClippingPlanesMinHeight = 10000.0f;

        /// <summary>
        /// The height to start dynamically adjusting the camera's clipping planes. 
        /// Below this height, the clipping planes will be set to their initial values.
        /// </summary>
        public float dynamicClippingPlanesMinHeight
        {
            get => this._dynamicClippingPlanesMinHeight;
            set => this._dynamicClippingPlanesMinHeight = Mathf.Max(value, 0.0f);
        }

        [SerializeField]
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
        /// <see cref="CesiumCameraController.flyToAltitudeProfileCurve"/> to allow the 
        /// camera to take some altitude during the flight.
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
        public event CompletedFlightDelegate OnFlightComplete;

        /// <summary>
        /// Encapsulates a method that is called whenever the camera's flight is interrupted.
        /// </summary>
        public delegate void InterruptedFlightDelegate();

        /// <summary>
        /// An event that is raised when the camera's flight is interrupted.
        /// </summary>
        public event InterruptedFlightDelegate OnFlightInterrupted;

        #endregion

        #region Private variables

        private Camera _camera;
        private float _initialNearClipPlane;
        private float _initialFarClipPlane;

        private CharacterController _controller;
        private CesiumGeoreference _georeference;
        private CesiumGlobeAnchor _globeAnchor;

        private Vector3 _velocity = Vector3.zero;
        private float _lookSpeed = 10.0f;

        // These numbers are borrowed from Cesium for Unreal.
        private float _acceleration = 20000.0f;
        private float _deceleration = 9999999959.0f;
        private float _maxRaycastDistance = 1000 * 1000; // 1000 km;

        private float _maxSpeed = 100.0f; // Maximum speed with the speed multiplier applied.
        private float _maxSpeedPreMultiplier = 0.0f; // Max speed without the multiplier applied.
        private AnimationCurve _maxSpeedCurve;

        private float _speedMultiplier = 1.0f;
        private float _speedMultiplierIncrement = 1.5f;

        private List<double3> _keypoints = new List<double3>();

        private double _currentFlyToTime = 0.0;
        private Quaternion _flyToSourceRotation = Quaternion.identity;
        private Quaternion _flyToDestinationRotation = Quaternion.identity;

        private bool _flyingToLocation = false;
        private bool _canInterruptFlight = true;

        // If the near clip gets too large, Unity will throw errors. Keeping it 
        // at this value works fine even when the far clip plane gets large.
        private float _maximumNearClipPlane = 1000.0f;
        private float _maximumFarClipPlane = 500000000.0f;

        // The maximum ratio that the far clip plane is allowed to be larger
        // than the near clip plane. The near clip plane is set so that this
        // ratio is never exceeded.
        private float _maximumNearToFarRatio = 100000.0f;

        #endregion

        #region Input configuration

        #if ENABLE_INPUT_SYSTEM
        InputAction lookAction;
        InputAction moveAction;
        InputAction moveUpAction;
        InputAction speedChangeAction;
        InputAction speedResetAction;
        InputAction toggleDynamicSpeedAction;

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
                .With("Up", "<Gamepad>/rightTrigger")
                .With("Down", "<Gamepad>/leftTrigger");

            speedChangeAction = map.AddAction("speedChange", binding: "<Mouse>/scroll");
            speedChangeAction.AddCompositeBinding("Dpad")
                .With("Up", "<Gamepad>/rightShoulder")
                .With("Down", "<Gamepad>/leftShoulder");

            speedResetAction = map.AddAction("speedReset", binding: "<Mouse>/middleButton");
            speedResetAction.AddBinding("<Gamepad>/buttonNorth");

            toggleDynamicSpeedAction =
                map.AddAction("toggleDynamicSpeed", binding: "<Keyboard>/g");
            toggleDynamicSpeedAction.AddBinding("<Gamepad>/buttonEast");

            moveAction.Enable();
            lookAction.Enable();
            moveUpAction.Enable();
            speedChangeAction.Enable();
            speedResetAction.Enable();
            toggleDynamicSpeedAction.Enable();
        }
        #endif

        #endregion

        #region Initialization

        void InitializeCamera()
        {
            this._camera = this.gameObject.GetComponent<Camera>();
            if (this._camera == null)
            {
                this._camera = this.gameObject.AddComponent<Camera>();
            }

            this._initialNearClipPlane = this._camera.nearClipPlane;
            this._initialFarClipPlane = this._camera.farClipPlane;
        }

        void InitializeController()
        {
            if (this.gameObject.GetComponent<CharacterController>() != null)
            {
                Debug.LogWarning("A CharacterController component was manually " +
                    "added to the CesiumCameraController's game object. " +
                    "This may interfere with the CesiumCameraController's movement.");

                this._controller = this.gameObject.GetComponent<CharacterController>();
            }
            else
            {
                this._controller = this.gameObject.AddComponent<CharacterController>();
                this._controller.hideFlags = HideFlags.HideInInspector;
            }

            this._controller.radius = 1.0f;
            this._controller.height = 1.0f;
            this._controller.center = Vector3.zero;
            this._controller.detectCollisions = true;
        }

        /// <summary>
        /// Creates a curve to control the bounds of the maximum speed before it is
        /// multiplied by the speed multiplier. This prevents the camera from achieving 
        /// an unreasonably low or high speed.
        /// </summary>
        private void CreateMaxSpeedCurve()
        {
            // This creates a curve that is linear between the first two keys,
            // then smoothly interpolated between the last two keys.
            Keyframe[] keyframes = {
                new Keyframe(0.0f, 4.0f),
                new Keyframe(10000000.0f, 10000000.0f),
                new Keyframe(13000000.0f, 2000000.0f)
            };

            keyframes[0].weightedMode = WeightedMode.Out;
            keyframes[0].outTangent = keyframes[1].value / keyframes[0].value;
            keyframes[0].outWeight = 0.0f;

            keyframes[1].weightedMode = WeightedMode.In;
            keyframes[1].inWeight = 0.0f;
            keyframes[1].inTangent = keyframes[1].value / keyframes[0].value;
            keyframes[1].outTangent = 0.0f;

            keyframes[2].inTangent = 0.0f;

            this._maxSpeedCurve = new AnimationCurve(keyframes);
            this._maxSpeedCurve.preWrapMode = WrapMode.ClampForever;
            this._maxSpeedCurve.postWrapMode = WrapMode.ClampForever;
        }

        void Awake()
        {
            this._georeference = this.gameObject.GetComponentInParent<CesiumGeoreference>();
            if (this._georeference == null)
            {
                Debug.LogError(
                    "CesiumCameraController must be nested under a game object " +
                    "with a CesiumGeoreference.");
            }

            // CesiumOriginShift will add a CesiumGlobeAnchor automatically.
            this._globeAnchor = this.gameObject.GetComponent<CesiumGlobeAnchor>();

            this.InitializeCamera();
            this.InitializeController();
            this.CreateMaxSpeedCurve();

            #if ENABLE_INPUT_SYSTEM
            this.ConfigureInputs();
            #endif
        }

        #endregion

        #region Update

        void Update()
        {
            this.HandlePlayerInputs();

            if (this._flyingToLocation)
            {
                this.HandleFlightStep(Time.deltaTime);
            }

            if (this._enableDynamicClippingPlanes)
            {
                this.UpdateClippingPlanes();
            }
        }

        #endregion

        #region Raycasting helpers

        private bool RaycastTowardsEarthCenter(out float hitDistance)
        {
            double3 center =
                this._georeference.TransformEarthCenteredEarthFixedPositionToUnity(new double3(0.0));

            RaycastHit hitInfo;
            if (Physics.Linecast(this.transform.position, (float3)center, out hitInfo))
            {
                hitDistance = Vector3.Distance(this.transform.position, hitInfo.point);
                return true;
            }

            hitDistance = 0.0f;
            return false;
        }

        private bool RaycastAlongForwardVector(float raycastDistance, out float hitDistance)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(
                this.transform.position,
                this.transform.forward,
                out hitInfo,
                raycastDistance))
            {
                hitDistance = Vector3.Distance(this.transform.position, hitInfo.point);
                return true;
            }

            hitDistance = 0.0f;
            return false;
        }

        #endregion

        #region Player movement

        private void HandlePlayerInputs()
        {
            #if ENABLE_INPUT_SYSTEM
            Vector2 lookDelta = lookAction.ReadValue<Vector2>();
            float inputRotateHorizontal = lookDelta.x;
            float inputRotateVertical = lookDelta.y;

            Vector2 moveDelta = moveAction.ReadValue<Vector2>();
            float inputForward = moveDelta.y;
            float inputRight = moveDelta.x;

            float inputUp = moveUpAction.ReadValue<Vector2>().y;

            float inputSpeedChange = speedChangeAction.ReadValue<Vector2>().y;
            bool inputSpeedReset = speedResetAction.ReadValue<float>() > 0.5f;

            bool toggleDynamicSpeed = toggleDynamicSpeedAction.ReadValue<float>() > 0.5f;
            #else
            float inputRotateHorizontal = Input.GetAxis("Mouse X");
            inputRotateHorizontal += Input.GetAxis("Controller Right Stick X");

            float inputRotateVertical = Input.GetAxis("Mouse Y");
            inputRotateVertical += Input.GetAxis("Controller Right Stick Y");

            float inputForward = Input.GetAxis("Vertical");
            float inputRight = Input.GetAxis("Horizontal");
            float inputUp = Input.GetAxis("YAxis");

            float inputSpeedChange = Input.GetAxis("Mouse ScrollWheel");
            bool inputSpeedReset =
                Input.GetMouseButtonDown(2) || Input.GetKeyDown("joystick button 3");

            bool toggleDynamicSpeed =
                Input.GetKeyDown("g") || Input.GetKeyDown("joystick button 1");
            #endif

            Vector3 movementInput = new Vector3(inputRight, inputUp, inputForward);

            if (!this._flyingToLocation)
            {
                this.Rotate(inputRotateHorizontal, inputRotateVertical);
            }

            if (toggleDynamicSpeed)
            {
                this._enableDynamicSpeed = !this._enableDynamicSpeed;
            }

            if (inputSpeedReset ||
                (this._enableDynamicSpeed && movementInput == Vector3.zero))
            {
                this.ResetSpeedMultiplier();
            }
            else
            {
                this.HandleSpeedChange(inputSpeedChange);
            }

            bool canInterruptFlight = this._flyingToLocation && this._canInterruptFlight;

            if(canInterruptFlight && movementInput != Vector3.zero)
            {
                this.InterruptFlight();
            }

            if (!this._flyingToLocation)
            {
                this.Move(movementInput);
            }
        }

        private void HandleSpeedChange(float speedChangeInput)
        {
            if (this._enableDynamicSpeed)
            {
                this.UpdateDynamicSpeed();
            }
            else
            {
                this.SetMaxSpeed(100.0f);
            }

            if (speedChangeInput != 0.0f)
            {
                if (speedChangeInput > 0.0f)
                {
                    this._speedMultiplier *= this._speedMultiplierIncrement;
                }
                else
                {
                    this._speedMultiplier /= this._speedMultiplierIncrement;
                }

                float max = this._enableDynamicSpeed ? 50.0f : 50000.0f;
                this._speedMultiplier = Mathf.Clamp(this._speedMultiplier, 0.1f, max);
            }
        }

        /// <summary>
        /// Rotate the camera with the specified amounts.
        /// </summary>
        /// <remarks>
        /// Horizontal rotation (i.e. looking left or right) corresponds to rotation around the Y-axis.
        /// Vertical rotation (i.e. looking up or down) corresponds to rotation around the X-axis.
        /// </remarks>
        /// <param name="horizontalRotation">The amount to rotate horizontally, i.e. around the Y-axis.</param>
        /// <param name="verticalRotation">The amount to rotate vertically, i.e. around the X-axis.</param>
        private void Rotate(float horizontalRotation, float verticalRotation)
        {
            if (horizontalRotation == 0.0f && verticalRotation == 0.0f)
            {
                return;
            }

            float valueX = verticalRotation * this._lookSpeed * Time.deltaTime;
            float valueY = horizontalRotation * this._lookSpeed * Time.deltaTime;

            // Rotation around the X-axis occurs counter-clockwise, so the look range
            // maps to [270, 360] degrees for the upper quarter-sphere of motion, and
            // [0, 90] degrees for the lower. Euler angles only work with positive values,
            // so map the [0, 90] range to [360, 450] so the entire range is [270, 450].
            // This makes it easy to clamp the values.
            float rotationX = this.transform.localEulerAngles.x;
            if (rotationX <= 90.0f)
            {
                rotationX += 360.0f;
            }

            float newRotationX = Mathf.Clamp(rotationX - valueX, 270.0f, 450.0f);
            float newRotationY = this.transform.localEulerAngles.y + valueY;
            this.transform.localRotation =
                Quaternion.Euler(newRotationX, newRotationY, this.transform.localEulerAngles.z);
        }

        /// <summary>
        /// Moves the controller with the given player input.
        /// </summary>
        /// <remarks>
        /// The x-coordinate affects movement along the transform's right axis.
        /// The y-coordinate affects movement along the georeferenced up axis.
        /// The z-coordinate affects movement along the transform's forward axis.
        /// </remarks>
        /// <param name="movementInput">The player input.</param>
        private void Move(Vector3 movementInput)
        {
            Vector3 inputDirection =
                this.transform.right * movementInput.x + this.transform.forward * movementInput.z;

            if (this._georeference != null && this._globeAnchor != null)
            {
                double3 positionECEF = new double3()
                {
                    x = this._globeAnchor.ecefX,
                    y = this._globeAnchor.ecefY,
                    z = this._globeAnchor.ecefZ,
                };
                double3 upECEF = CesiumEllipsoid.GeodeticSurfaceNormal(positionECEF);
                double3 upUnity =
                    this._georeference.TransformEarthCenteredEarthFixedDirectionToUnity(upECEF);

                inputDirection = (float3)inputDirection + (float3)upUnity * movementInput.y;
            }

            if (inputDirection != Vector3.zero)
            {
                // If the controller was already moving, handle the direction change
                // separately from the magnitude of the velocity.
                if (this._velocity.magnitude > 0.0f)
                {
                    Vector3 directionChange = inputDirection - this._velocity.normalized;
                    this._velocity +=
                        directionChange * this._velocity.magnitude * Time.deltaTime;
                }

                this._velocity += inputDirection * this._acceleration * Time.deltaTime;
                this._velocity = Vector3.ClampMagnitude(this._velocity, this._maxSpeed);
            }
            else
            {
                // Decelerate
                float speed = Mathf.Max(
                    this._velocity.magnitude - this._deceleration * Time.deltaTime,
                    0.0f);

                this._velocity = Vector3.ClampMagnitude(this._velocity, speed);
            }

            this._controller.Move(this._velocity * Time.deltaTime);
        }

        #endregion

        #region Dynamic speed computation

        /// <summary>
        /// Gets the dynamic speed of the controller based on the camera's height from 
        /// the earth's center and its distance from objects along the forward vector.
        /// </summary>
        /// <param name="overrideSpeed">Whether the returned speed should override the 
        /// previous speed, even if the new value is lower.</param>
        /// <param name="newSpeed">The new dynamic speed of the controller.</param>
        /// <returns>Whether a valid speed value was found.</returns>
        private bool GetDynamicSpeed(out bool overrideSpeed, out float newSpeed)
        {
            if (this._georeference == null)
            {
                overrideSpeed = false;
                newSpeed = 0.0f;

                return false;
            }

            float height, viewDistance;

            // Raycast from the camera to the Earth's center and compute the distance.
            // Ignore the result if the height is approximately 0.
            if (!this.RaycastTowardsEarthCenter(out height) || height < 0.000001f)
            {
                overrideSpeed = false;
                newSpeed = 0.0f;

                return false;
            }

            // Also ignore the result if the speed will increase by too much at once.
            // This can be an issue when 3D tiles are loaded/unloaded from the scene.
            if (
                this._maxSpeedPreMultiplier > 0.5f &&
                (height / this._maxSpeedPreMultiplier) > 1000.0f)
            {
                overrideSpeed = false;
                newSpeed = 0.0f;

                return false;
            }

            // Raycast along the camera's view (forward) vector.
            float raycastDistance = 
                Mathf.Clamp(this._maxSpeed * 3.0f, 0.0f, this._maxRaycastDistance);

            // If the raycast does not hit, then only override speed if the height
            // is lower than the maximum threshold. Otherwise, if both raycasts hit,
            // always override the speed.
            if (!this.RaycastAlongForwardVector(raycastDistance, out viewDistance) ||
                viewDistance < 0.000001f)
            {
                overrideSpeed = height <= this._dynamicSpeedMinHeight;
            }
            else
            {
                overrideSpeed = true;
            }

            // Set the speed to be the height of the camera from the Earth's center.
            newSpeed = height;

            return true;
        }

        private void ResetSpeedMultiplier()
        {
            this._speedMultiplier = 1.0f;
        }

        private void SetMaxSpeed(float speed)
        {
            float actualSpeed = this._maxSpeedCurve.Evaluate(speed);
            this._maxSpeed = this._speedMultiplier * actualSpeed;
            this._acceleration =
                Mathf.Clamp(this._maxSpeed * 5.0f, 20000.0f, 10000000.0f);
        }

        private void UpdateDynamicSpeed()
        {
            bool overrideSpeed;
            float newSpeed;
            if (this.GetDynamicSpeed(out overrideSpeed, out newSpeed))
            {
                if (overrideSpeed || newSpeed >= this._maxSpeedPreMultiplier)
                {
                    this._maxSpeedPreMultiplier = newSpeed;
                }
            }

            this.SetMaxSpeed(this._maxSpeedPreMultiplier);
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

            if(Mathf.Approximately((float)flyPercentage, 1.0f))
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

        private void CompleteFlight()
        {
            double3 finalPoint = this._keypoints[this._keypoints.Count - 1];
            this._globeAnchor.SetPositionEarthCenteredEarthFixed(
                finalPoint.x,
                finalPoint.y,
                finalPoint.z);

            this.transform.rotation = this._flyToDestinationRotation;

            this._flyingToLocation = false;
            this._currentFlyToTime = 0.0;

            this._globeAnchor.adjustOrientationForGlobeWhenMoving = true;

            if (this.OnFlightComplete != null)
            {
                this.OnFlightComplete();
            }
        }

        private void InterruptFlight()
        {
            this._flyingToLocation = false;

            // Set the camera roll to 0.0
            Vector3 angles = this.transform.eulerAngles;
            angles.z = 0.0f;
            this.transform.eulerAngles = angles;

            this._globeAnchor.adjustOrientationForGlobeWhenMoving = true;

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

        #endregion

        #region Fly-To public API

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
            this._globeAnchor.adjustOrientationForGlobeWhenMoving = false;
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

        #region Dynamic clipping plane adjustment

        private void UpdateClippingPlanes()
        {
            if (this._camera == null)
            {
                return;
            }

            // Raycast from the camera to the Earth's center and compute the distance.
            float height = 0.0f;
            if (!this.RaycastTowardsEarthCenter(out height))
            {
                return;
            }

            float nearClipPlane = this._initialNearClipPlane;
            float farClipPlane = this._initialFarClipPlane;

            if (height >= this._dynamicClippingPlanesMinHeight)
            {
                farClipPlane = height + (float)(2.0 * CesiumEllipsoid.GetMaximumRadius());
                farClipPlane = Mathf.Min(farClipPlane, this._maximumFarClipPlane);

                float farClipRatio = farClipPlane / this._maximumNearToFarRatio;

                if (farClipRatio > nearClipPlane)
                {
                    nearClipPlane = Mathf.Min(farClipRatio, this._maximumNearClipPlane);
                }
                else
                {
                    nearClipPlane = 10.0f;
                }
            }

            this._camera.nearClipPlane = nearClipPlane;
            this._camera.farClipPlane = farClipPlane;
        }

        #endregion
    }
}
