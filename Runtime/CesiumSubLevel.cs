using UnityEngine;

namespace CesiumForUnity
{
    [ExecuteInEditMode]
    public class CesiumSubLevel : MonoBehaviour
    {
        private void OnEnable()
        {
            // When this sub-level is enabled, all others are disabled.
            CesiumGeoreference georeference = this.GetComponentInParent<CesiumGeoreference>();
            CesiumSubLevel[] sublevels = georeference.GetComponentsInChildren<CesiumSubLevel>();
            foreach (CesiumSubLevel level in sublevels)
            {
                if (level == this)
                    continue;
                level.gameObject.SetActive(false);
            }
        }
    }
}
