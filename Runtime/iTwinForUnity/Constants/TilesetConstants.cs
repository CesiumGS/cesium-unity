using UnityEngine;

public static class TilesetConstants
{
    // Color definitions
    public static readonly Color HeaderBgColor = new Color(0.15f, 0.15f, 0.15f);
    public static readonly Color CardBgColor = new Color(0.22f, 0.22f, 0.22f);
    public static readonly Color CardBgHoverColor = new Color(0.26f, 0.26f, 0.26f);
    public static readonly Color AccentColor = new Color(0.2f, 0.6f, 0.9f);
    public static readonly Color PrimaryTextColor = new Color(0.9f, 0.9f, 0.9f);
    public static readonly Color SecondaryTextColor = new Color(0.7f, 0.7f, 0.7f);
    
    // UI dimensions
    public const int ITEMS_PER_PAGE = 3;
    public const int THUMBNAIL_SIZE = 80;
    public const int CARD_PADDING = 12;
    public const int SEARCH_BOX_HEIGHT = 28;
    public const int BUTTON_HEIGHT = 24;
    
    // Animation timing
    public const float SPINNER_FRAME_RATE = 0.15f;
    public const int MAX_DESCRIPTION_LENGTH = 200;
    public const int MAX_ID_DISPLAY_LENGTH = 12;
    
    // Spinner frames
    public static readonly string[] SpinnerFrames = new string[] { "◐", "◓", "◑", "◒" };
    
    // Search and keyboard shortcuts
    public const string SEARCH_CONTROL_NAME = "TilesetSearchField";
}
