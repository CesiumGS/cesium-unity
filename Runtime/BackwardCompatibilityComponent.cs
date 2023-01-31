using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    [ExecuteInEditMode]
    [AddComponentMenu("")]
    [DefaultExecutionOrder(-1000000)]
    public abstract class BackwardCompatibilityComponent : MonoBehaviour
    {
#if UNITY_EDITOR
        void OnEnable()
        {
            GameObject go = this.gameObject;

            Debug.Log("Upgrading " + this.UpgradedComponent + " on game object " + go.name + " from Cesium for Unity " + this.UpgradedVersion + ".");

            try
            {
                this.Upgrade();
                //Helpers.Destroy(this);

                // Unity seems to ignore SetDirty from within OnEnable. So do it later.
                EditorApplication.CallbackFunction markDirty = null;
                markDirty = new EditorApplication.CallbackFunction(() =>
                {
                    EditorUtility.SetDirty(go);
                    EditorApplication.update -= markDirty;
                });

                EditorApplication.update += markDirty;
            }
            catch (Exception e)
            {
                Debug.LogError("Upgrading failed with an exception: " + e.ToString());
            }
        }
#endif

        protected abstract string UpgradedComponent
        {
            get;
        }

        protected abstract string UpgradedVersion
        {
            get;
        }

        protected abstract void Upgrade();
    }
}
