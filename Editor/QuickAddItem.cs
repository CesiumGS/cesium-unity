namespace CesiumForUnity
{
    public enum QuickAddItemType
    {
        BlankTileset,
        DynamicCamera,
        CartographicPolygon,
        IonTileset
    }

    public class QuickAddItem
    {
        public QuickAddItemType type;
        public string name;
        public string tooltip;
        public string tilesetName;
        public long tilesetId;
        public string overlayName;
        public long overlayId;

        public QuickAddItem()
        {
        }

        public QuickAddItem(
            QuickAddItemType type,
            string name,
            string tooltip,
            string tilesetName,
            long tilesetId,
            string overlayName,
            long overlayId)
        {
            this.type = type;
            this.name = name;
            this.tooltip = tooltip;
            this.tilesetName = tilesetName;
            this.tilesetId = tilesetId;
            this.overlayName = overlayName;
            this.overlayId = overlayId;
        }
    }
}
