using UnityEditor.IMGUI.Controls;

namespace CesiumForUnity
{
    public class IonAssetsMultiColumnHeader : MultiColumnHeader
    {
        private IonAssetsTreeView _treeView;
        private int _lastSortedColumnIndex;

        public IonAssetsMultiColumnHeader(
            MultiColumnHeaderState state,
            IonAssetsTreeView treeView) :
            base(state)
        {
            this._treeView = treeView;
            ResizeToFit();
        }

        protected override void ColumnHeaderClicked(
            MultiColumnHeaderState.Column column,
            int columnIndex)
        {
            if (this.sortedColumnIndex == columnIndex)
            {
                if (column.sortedAscending)
                {
                    column.sortedAscending = false;
                }
                else
                {
                    // Remove the sorting entirely.
                    sortedColumnIndex = -1;
                }
            }
            else
            {
                sortedColumnIndex = columnIndex;
                column.sortedAscending = true;
            }

            this._treeView.Refresh();
        }
    }

}