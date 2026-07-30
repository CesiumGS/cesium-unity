#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;

#if UNITY_6000_2_OR_NEWER
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;
#endif

// Class for bridging the gap between templated and non-templated
// versions of TreeView et al., for the benefit of Reinterop.

namespace CesiumForUnity
{
    public  class CesiumTreeViewState : TreeViewState
    {
        public CesiumTreeViewState()
        {
        }
    }
}
#endif
