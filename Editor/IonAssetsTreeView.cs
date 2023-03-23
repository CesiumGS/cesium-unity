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

    public class IonAssetDetails
    {
        private string _name;
        private string _type;
        private long _id;
        private string _description;
        private string _attribution;

        public IonAssetDetails(string name, string type, long id, string description, string attribution)
        {
            this._name = name;
            this._type = type;
            this._id = id;
            this._description = description;
            this._attribution = attribution;
        }

        public string name
        {
            get => this._name;
        }

        public string type
        {
            get => this._type;
        }

        public long id
        {
            get => this._id;
        }

        public string description
        {
            get => this._description;
        }

        public string attribution
        {
            get => this._attribution;
        }

        private static Dictionary<string, string> typeLookup = new Dictionary<string, string>
        {
            { "3DTILES", "3D Tiles" },
            { "GLTF", "glTF" },
            { "IMAGERY", "Imagery" },
            { "TERRAIN", "Terrain" },
            { "CZML", "CZML" },
            { "KML", "KML" },
            { "GEOJSON", "GeoJSON" }
        };

        public static string FormatType(string type)
        {
            string value;
            if (typeLookup.TryGetValue(type, out value))
            {
                return value;
            }
            return "(Unknown)";
        }

        public static string FormatDate(string assetDate)
        {
            DateTime date = new DateTime();
            bool success = DateTime.TryParse(
                assetDate,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out date);

            if (!success)
            {
                Debug.Log("Could not parse date " + assetDate);
            }

            return date.ToString("yyyy-MM-dd");
        }
    }

    [ReinteropNativeImplementation("CesiumForUnityNative::IonAssetsTreeViewImpl", "IonAssetsTreeViewImpl.h")]
    public partial class IonAssetsTreeView : TreeView
    {
        private MultiColumnHeaderState _headerState;

        public IonAssetsTreeView(TreeViewState assetsTreeState)
            : base(assetsTreeState)
        {
            BuildMultiColumnHeader();
            CreateImplementation();
        }

        private void BuildMultiColumnHeader()
        {
            string[] columnNames = new string[Enum.GetNames(typeof(IonAssetsColumn)).Length];
            columnNames[(int)IonAssetsColumn.Name] = "Name";
            columnNames[(int)IonAssetsColumn.Type] = "Type";
            columnNames[(int)IonAssetsColumn.DateAdded] = "Date added";

            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[columnNames.Length];

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false,
                    autoResize = true,
                    minWidth = 135.0f,
                    canSort = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    headerContent = new GUIContent(columnNames[i]),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                };
            }

            this._headerState = new MultiColumnHeaderState(columns);
            multiColumnHeader = new IonAssetsMultiColumnHeader(this._headerState, this);
        }

        protected override TreeViewItem BuildRoot()
        {
            int rootId = 0;
            int rootDepth = -1;
            return new TreeViewItem(rootId, rootDepth, "Root");
        }

        public partial int GetAssetsCount();

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            int count = GetAssetsCount();
            IList<TreeViewItem> rows = new List<TreeViewItem>();
            // All items are counted as children of the root item, such that when displayed
            // they appear in a list.
            const int itemDepth = 0;

            for (int i = 0; i < count; i++)
            {
                // The root of the tree is typically assigned as 0, so all of the ids
                // have to be offset by 1. Otherwise, the selection behavior of the TreeView
                // may be inaccurate.
                TreeViewItem assetItem = new TreeViewItem(i + 1, itemDepth);
                rows.Add(assetItem);
                root.AddChild(assetItem);
            }

            return rows;
        }

        private partial string GetAssetName(int index);
        private partial string GetAssetType(int index);
        private partial long GetAssetID(int index);
        private partial string GetAssetDescription(int index);
        private partial string GetAssetAttribution(int index);

        public IonAssetDetails GetAssetDetails(int treeId)
        {
            int index = treeId - 1;
            string name = GetAssetName(index);
            string type = GetAssetType(index);
            long id = GetAssetID(index);
            string description = GetAssetDescription(index);
            string attribution = GetAssetAttribution(index);

            return new IonAssetDetails(name, type, id, description, attribution);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int index = 0; index < args.GetNumVisibleColumns(); ++index)
            {
                int assetIndex = args.item.id - 1;
                CellGUI(args.GetCellRect(index), assetIndex, (IonAssetsColumn)index);
            }
        }

        private partial void CellGUI(Rect cellRect, int assetIndex, IonAssetsColumn column);

        public partial void Refresh();

        protected override void SearchChanged(string newSearch)
        {
            this.Refresh();
            this.SetSelection(new List<int>());
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        public partial void AddAssetToLevel(int index);

        public partial void AddOverlayToTerrain(int index);
    }
}

