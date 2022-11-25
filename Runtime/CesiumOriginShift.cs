using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

namespace CesiumForUnity
{
    /// <summary>
    /// This component is typically attached to a camera, and it automatically updates the CesiumGeoreference
    /// to keep its origin near the location of the camera. This improves rendering precision by keeping the
    /// coordinate values of objects that are near the camera as small as possible. A game object with this
    /// component must be nested inside a <see cref="CesiumGeoreference"/>, and it must also have a
    /// <see cref="CesiumGlobeAnchor"/>. It is essential to add a <see cref="CesiumGlobeAnchor"/> to all
    /// other objects in the scene as well; otherwise, they will appear to move when the origin is shifted.
    /// </summary>
    [RequireComponent(typeof(CesiumGlobeAnchor))]
    public class CesiumOriginShift : MonoBehaviour
    {
        void LateUpdate()
        {
            CesiumGeoreference georeference = this.GetComponentInParent<CesiumGeoreference>();

            CesiumGlobeAnchor anchor = this.GetComponent<CesiumGlobeAnchor>();
            if (anchor != null && anchor.positionAuthority != CesiumGlobeAnchorPositionAuthority.None)
            {
                this.UpdateFromEcef(georeference, new double3(
                    anchor.ecefX,
                    anchor.ecefY,
                    anchor.ecefZ
                ));
                return;
            }

            Vector3 position = this.transform.position;
            double3 ecef = georeference.TransformUnityWorldPositionToEarthCenteredEarthFixed(new double3(
                position.x,
                position.y,
                position.z
            ));
            this.UpdateFromEcef(georeference, ecef);
        }

        private List<CesiumSubScene> _sublevelsScratch = new List<CesiumSubScene>();

        private void UpdateFromEcef(CesiumGeoreference georeference, double3 ecef)
        {
            CesiumSubScene closestLevel = null;
            double distanceSquaredToClosest = double.MaxValue;

            // Are we inside a sub-level?
            georeference.GetComponentsInChildren<CesiumSubScene>(true, this._sublevelsScratch);
            foreach (CesiumSubScene level in this._sublevelsScratch)
            {
                // TODO: Make sure ECEF position is actually up-to-date
                double x = level.ecefX - ecef.x;
                double y = level.ecefY - ecef.y;
                double z = level.ecefZ - ecef.z;
                double distanceSquared = x * x + y * y + z * z;
                if (distanceSquared > level.activationRadius * level.activationRadius)
                    // We're outside this level's activation radius
                    continue;

                if (closestLevel == null || distanceSquared < distanceSquaredToClosest)
                {
                    closestLevel = level;
                    distanceSquaredToClosest = distanceSquared;
                }
            }

            if (closestLevel != null)
            {
                if (!closestLevel.isActiveAndEnabled)
                {
                    // Setting a level active will automatically disable all other levels.
                    closestLevel.gameObject.SetActive(true);
                    closestLevel.enabled = true;
                }
            }
            else
            {
                // Deactivate all active sub-levels
                foreach (CesiumSubScene level in this._sublevelsScratch)
                {
                    if (level.isActiveAndEnabled)
                        level.gameObject.SetActive(false);
                }

                // Update the origin continuously.
                // TODO: account for a transform on the CesiumGeoreference
                georeference.SetOriginEarthCenteredEarthFixed(ecef.x, ecef.y, ecef.z);
            }
        }
    }
}
