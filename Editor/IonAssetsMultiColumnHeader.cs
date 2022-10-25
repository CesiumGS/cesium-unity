using UnityEditor.IMGUI.Controls;

namespace CesiumForUnity
{
    public class IonAssetsMultiColumnHeader : MultiColumnHeader
    {
        IonAssetsTreeView _treeView;

        public IonAssetsMultiColumnHeader(
            MultiColumnHeaderState state,
            IonAssetsTreeView treeView) :
            base(state)
        {
            _treeView = treeView;
            ResizeToFit();
        }

        protected override void ColumnHeaderClicked(
            MultiColumnHeaderState.Column column,
            int columnIndex)
        {
            if (sortedColumnIndex == columnIndex)
            {
                if (column.sortedAscending)
                {
                    column.sortedAscending = false;
                }
                else
                {
                    // Reset the sorting method.
                    column.sortedAscending = true;

                    // Remove the sorting entirely.
                    sortedColumnIndex = -1;
                }
            }
            else
            {
                sortedColumnIndex = columnIndex;
            }
        }

        protected override void OnSortingChanged()
        {
            base.OnSortingChanged();
            _treeView.Refresh();
        }
    }

}