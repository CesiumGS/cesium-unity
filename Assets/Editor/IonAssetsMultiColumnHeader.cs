using Reinterop;
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

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
            // TODO
            _treeView.Refresh();
        }
    }

}