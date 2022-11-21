using Reinterop;
using System;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CesiumForUnity
{
    public class GlobeAwareController : MonoBehaviour
    {
        #region User-editable properties

        [Tooltip("This curve dictates what percentage of the max altitude the " +
            "game object should take at a given time on the curve." +
            "\n\n" +
            "This curve must be kept in the 0 to 1 range on both axes. The " +
            "\"Fly To Maximum Altitude Curve\" dictates the actual max " +
            "altitude at each point along the curve.")]
        public AnimationCurve? flyToAltitudeProfileCurve;

        [Tooltip("This curve is used to determine the progress percentage for " +
            "all the other curves. This allows us to accelerate and deaccelerate " +
            "as wanted throughout the curve.")]
        public AnimationCurve? flyToProgressCurve;

        [Tooltip("This curve dictates the maximum altitude at each point along " +
            "the curve." +
            "\n\n" +
            "This can be used in conjunction with the \"Fly To Altitude Profile " +
            "Curve\" to allow the game object to take some altitude during the flight.")]
        public AnimationCurve? flyToMaximumAltitudeCurve;

        [Tooltip("The length in seconds that the flight should last.")]
        [Min(0.0f)]
        public double flyToDuration = 5.0;

        [Tooltip("The granularity in degrees with which keypoints should be generated " +
            "for the flight interpolation.")]
        [Min(0.0f)]
        public double flyToGranularityDegrees = 0.01;

        /**
         * A delegate that will be called whenever the pawn finishes flying
         *
         */
        //FCompletedFlight OnFlightComplete;

        /**
         * A delegate that will be called when a pawn's flying is interrupted
         *
         */
        //FInterruptedFlight OnFlightInterrupt;
        #endregion

        #region Private variables

        private CesiumGeoreference? _georeference;
        private CesiumGlobeAnchor? _globeAnchor;

        private List<CesiumVector3> _keypoints = new List<CesiumVector3>();

        private double _currentFlyToTime = 0.0;
        private Quaternion _flyToSourceRotation = Quaternion.identity;
        private Quaternion _flyToDestinationRotation = Quaternion.identity;

        private bool _flyingToLocation = false;
        private bool _canInterruptFlight = true;

        #endregion

        #region Input configuration

#if USE_INPUT_SYSTEM
        InputAction lookAction;
        InputAction moveAction;
        InputAction speedAction;
        InputAction yMoveAction;
#endif

        void ConfigureInputs()
        {

        }
        #endregion

        void OnEnable()
        {
            this._georeference = this.gameObject.GetComponentInParent<CesiumGeoreference>();
            if (this._georeference == null)
            {
                Debug.LogError(
                    "GlobeAwareController's game object must be nested under a game object " +
                    "with a CesiumGeoreference.");
            }

            this._globeAnchor = this.gameObject.GetComponent<CesiumGlobeAnchor>();
            if (this._globeAnchor == null)
            {
                Debug.LogError(
                    "GlobeAwareController's game object needs a CesiumGlobeAnchor component.");
            }
        }

        #region Fly-To Updaters

        void Update()
        {
            if (this._flyingToLocation)
            {
                HandleFlightStep(Time.deltaTime);
            }
        }

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
        /// 
        private void HandleFlightStep(float deltaTime)
        {
            if (this._globeAnchor == null || this._keypoints.Count == 0)
            {
                this._flyingToLocation = false;
                return;
            }

            this._currentFlyToTime += (double)deltaTime;

            // If we reached the end, set actual destination location and orientation
            if(this._currentFlyToTime >= this.flyToDuration)
            {
                CesiumVector3 finalPoint = this._keypoints[this._keypoints.Count - 1];
                this._globeAnchor.SetPositionEarthCenteredEarthFixed(
                    finalPoint.x,
                    finalPoint.y,
                    finalPoint.z);

                this.transform.rotation = this._flyToDestinationRotation;
                
                this._flyingToLocation = false;
                this._currentFlyToTime = 0.0;

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
            CesiumVector3 lastPosition = this._keypoints[lastKeypointIndex];
            CesiumVector3 nextPosition = this._keypoints[nextKeypointIndex];
            CesiumVector3 currentPosition =
                CesiumVector3.Lerp(lastPosition, nextPosition, segmentPercentage);
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

        #endregion

        #region Fly-To public API

        public void FlyToLocationEarthCenteredEarthFixed(
            CesiumVector3 destination,
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
            CesiumVector3 source = new CesiumVector3()
            {
                x = this._globeAnchor.ecefX,
                y = this._globeAnchor.ecefY,
                z = this._globeAnchor.ecefZ
            };

            // The source and destination rotations are expressed in East-South-Up
            // coordinates.
            this._flyToSourceRotation = transform.rotation;
            this._flyToDestinationRotation =
                Quaternion.Euler(pitchAtDestination, yawAtDestination, 0);

            Quaternion flyQuat = Quaternion.FromToRotation(
                CesiumVector3.Normalize(source).ToVector3(),
                CesiumVector3.Normalize(destination).ToVector3());

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

            // We will not create a curve projected along the ellipsoid as we want to take
            // altitude while flying. The radius of the current point will evolve as
            // follows:
            //  - Project the point on the ellipsoid: will give a default radius
            //  depending on ellipsoid location.
            //  - Interpolate the altitudes: get the source/destination altitudes, and make a
            //  linear interpolation between them. This will allow for flying from / to any
            //  point smoothly.
            //  - Add a flight profile offset /-\ defined by a curve.
            
            // Compute global radius at source and destination points
            double sourceRadius = CesiumVector3.Length(source);
            CesiumVector3 sourceUpVector = source;

            // Compute actual altitude at source and destination points by scaling on
            // ellipsoid.
            double sourceAltitude = 0.0, destinationAltitude = 0.0;
            CesiumVector3 scaled = 
                CesiumTransforms.ScaleCartesianToEllipsoidGeodeticSurface(source);
            if(scaled != null)
            {
                CesiumVector3 difference = source - scaled;

                sourceAltitude = CesiumVector3.Length(difference);
            }

            scaled =
               CesiumTransforms.ScaleCartesianToEllipsoidGeodeticSurface(destination);
            if (scaled != null)
            {
                CesiumVector3 difference = destination - scaled;

                destinationAltitude = CesiumVector3.Length(difference);
            }
        }

        public void FlyToLocationEarthCenteredEarthFixed(
            Vector3 destination,
            float yawAtDestination,
            float pitchAtDestination,
            bool canInterruptByMoving)
        {
            this.FlyToLocationEarthCenteredEarthFixed(
                new CesiumVector3()
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
            CesiumVector3 destination,
            float yawAtDestination,
            float pitchAtDestination,
            bool canInterruptByMoving)
        {
            CesiumVector3 destinationECEF =
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
            CesiumVector3 destinationCoordinates = new CesiumVector3()
            {
                x = destination.x,
                y = destination.y,
                z = destination.z
            };
            CesiumVector3 destinationECEF =
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
