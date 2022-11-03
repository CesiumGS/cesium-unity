using UnityEngine;
using CesiumForUnity;
using System;

namespace CesiumForUnity
{

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
