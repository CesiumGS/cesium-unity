using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

namespace CesiumForUnity
{
    /// <summary>
    /// Automatically shifts the origin of a <see cref="CesiumGeoreference"/> as the object to which
    /// it is attached moves. This improves rendering precision by keeping coordinate values small.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component is typically attached to a camera, and it automatically updates the <see cref="CesiumGeoreference"/>
    /// to keep its origin near the location of the camera. This improves rendering precision by keeping the
    /// coordinate values of objects that are near the camera as small as possible. A game object with this
    /// component must be nested inside a <see cref="CesiumGeoreference"/>, and it must also have a
    /// <see cref="CesiumGlobeAnchor"/>. It is essential to add a <see cref="CesiumGlobeAnchor"/> to all
    /// other objects in the scene as well; otherwise, they will appear to move when the origin is shifted.
    /// </para>
    /// <para>
    /// This component also switches between <see cref="CesiumSubScene"/> instances based on the distance
    /// to them. When inside a sub-scene, the origin shifting described above is not performed. This allows
    /// relatively normal Unity scenes to be defined at different locations on the globe.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(CesiumGlobeAnchor))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Cesium/Cesium Origin Shift")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public class CesiumOriginShift : MonoBehaviour
    {
        void LateUpdate()
        {
            CesiumGeoreference georeference = this.GetComponentInParent<CesiumGeoreference>();

            CesiumGlobeAnchor anchor = this.GetComponent<CesiumGlobeAnchor>();

            // The RequireComponent attribute should ensure the globe anchor exists, but it may not be active.
            if (anchor == null || !anchor.isActiveAndEnabled)
            {
                Debug.LogWarning("CesiumOriginShift is doing nothing because its CesiumGlobeAnchor component is missing or disabled.");
                return;
            }

            this.UpdateFromEcef(georeference, anchor.positionGlobeFixed);
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

                    Physics.SyncTransforms();
                }
            }
            else
            {
                bool deactivatedAnySublevel = false;

                // Deactivate all active sub-levels
                foreach (CesiumSubScene level in this._sublevelsScratch)
                {
                    if (level.isActiveAndEnabled)
                    {
                        level.gameObject.SetActive(false);
                        deactivatedAnySublevel = true;
                    }
                }

                // Update the origin continuously.
                georeference.SetOriginEarthCenteredEarthFixed(ecef.x, ecef.y, ecef.z);

                if (deactivatedAnySublevel)
                {
                    Physics.SyncTransforms();
                }
            }
        }
    }
}
