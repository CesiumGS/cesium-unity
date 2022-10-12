using Reinterop;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::IonAssetsTreeViewImpl", "IonAssetsTreeViewImpl.h")]
    public partial class IonAssetsTreeView : TreeView
    {
        private TreeViewState _treeState;

        private MultiColumnHeader _header;
        private MultiColumnHeaderState _headerState;

        public IonAssetsTreeView(TreeViewState assetsTreeState) : base(assetsTreeState)
        {
            _treeState = assetsTreeState;

            _headerState = new MultiColumnHeaderState(BuildHeaderColumns());
            _header = new MultiColumnHeader(_headerState);

            _header.ResizeToFit();

            CreateImplementation();
        }

        private MultiColumnHeaderState.Column[] BuildHeaderColumns()
        {
            string[] columnNames = { "Name", "Type", "Date added" };
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[columnNames.Length];

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false,
                    autoResize = true,
                    minWidth = 150.0f,
                    canSort = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    headerContent = new GUIContent(columnNames[i]),
                    headerTextAlignment = TextAlignment.Left
                };
            }

            return columns;
        }

        protected override TreeViewItem BuildRoot()
        {
            int rootId = -1;
            int rootDepth = -1;
            return new TreeViewItem(rootId, rootDepth);
        }

        protected override partial IList<TreeViewItem> BuildRows(TreeViewItem root);
    }
}

