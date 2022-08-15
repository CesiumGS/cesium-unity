using Reinterop;
using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{

    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnity::Cesium3DTilesetImpl", "Cesium3DTilesetImpl.h")]
    public partial class Cesium3DTileset : MonoBehaviour
    {
        public partial void Start();
        public partial void Update();

        void OnEnable()
        {
            // In the Editor, Update will only be called when something
            // changes. We need to call it continuously to allow tiles to
            // load.
            // TODO: we could be more careful about only calling Update when
            //       it's really needed.
            if (Application.isEditor && !EditorApplication.isPlaying)
            {
                EditorApplication.update += Update;
            }
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
        }
    }
}
