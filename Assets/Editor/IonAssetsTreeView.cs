using Reinterop;
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CesiumForUnity
{
    public enum IonAssetsColumn
    {
        Name = 0,
        Type = 1,
        DateAdded = 2,
    }

    [ReinteropNativeImplementation("CesiumForUnityNative::IonAssetsTreeViewImpl", "IonAssetsTreeViewImpl.h")]
    public partial class IonAssetsTreeView : TreeView
    {
        public IonAssetsTreeView(TreeViewState assetsTreeState, MultiColumnHeader header)
            : base(assetsTreeState, header)
        {
            CreateImplementation();
        }

        protected override TreeViewItem BuildRoot()
        {
            int rootId = 0;
            int rootDepth = -1;
            return new TreeViewItem(rootId, rootDepth, "Root");
        }

        protected override partial IList<TreeViewItem> BuildRows(TreeViewItem root);

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int index = 0; index < args.GetNumVisibleColumns(); ++index)
            {
                CellGUI(args.GetCellRect(index), args.item, (IonAssetsColumn)index);
            }
        }

        private partial void CellGUI(Rect cellRect, TreeViewItem item, IonAssetsColumn column);

        public partial void Refresh();
    }
}

