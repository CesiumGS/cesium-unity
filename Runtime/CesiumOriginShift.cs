using UnityEngine;
using CesiumForUnity;
using System;

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
                this.UpdateFromEcef(georeference, new CesiumVector3()
                {
                    x = anchor.ecefX,
                    y = anchor.ecefY,
                    z = anchor.ecefZ,
                });
                return;
            }

            Vector3 position = this.transform.position;
            CesiumVector3 ecef = georeference.TransformUnityWorldPositionToEarthCenteredEarthFixed(new CesiumVector3()
            {
                x = position.x,
                y = position.y,
                z = position.z
            });
            this.UpdateFromEcef(georeference, ecef);
        }

        private void UpdateFromEcef(CesiumGeoreference georeference, CesiumVector3 ecef)
        {
            georeference.SetOriginEarthCenteredEarthFixed(ecef.x, ecef.y, ecef.z);
            // TODO: account for a transform on the CesiumGeoreference
        }
    }
}
