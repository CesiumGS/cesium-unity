using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{

    [ExecuteInEditMode]
    public abstract class AbstractBaseCesium3DTileset : MonoBehaviour
    {
        public abstract void Start();
        public abstract void Update();

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
