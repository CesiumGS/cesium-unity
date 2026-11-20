using UnityEngine;
using System;

/// <summary>
/// Stores Bentley iTwin metadata associated with a Cesium3DTileset
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CesiumForUnity.Cesium3DTileset))]
public class BentleyTilesetMetadata : MonoBehaviour
{
    // Public accessible fields (visible in Inspector)
    [SerializeField] private string _iTwinId = string.Empty;
    [SerializeField] private string _iModelId = string.Empty;
    [SerializeField] private string _iModelName = string.Empty;
    [SerializeField] private string _iModelDescription = string.Empty;
    [SerializeField] private string _exportDate = string.Empty;

    // Private fields (for internal use)
    [SerializeField] private string _changesetId = string.Empty;
    [SerializeField] private string _iTwinName = string.Empty;
    [SerializeField] private string _changesetVersion = string.Empty;
    [SerializeField] private string _changesetDescription = string.Empty;
    [SerializeField] private string _changesetCreatedDate = string.Empty;
    [SerializeField] private string _exportUrl = string.Empty;
    // Thumbnail storage
    [SerializeField] private byte[] _iTwinThumbnailBytes = null;
    [SerializeField] private byte[] _iModelThumbnailBytes = null;

    // Public properties (accessible to scripts)
    /// <summary>
    /// The ID of the iTwin project this tileset was exported from
    /// </summary>
    public string iTwinId => _iTwinId;
    
    /// <summary>
    /// The ID of the iModel this tileset was exported from
    /// </summary>
    public string iModelId => _iModelId;
    
    /// <summary>
    /// The name of the iModel this tileset was exported from
    /// </summary>
    public string iModelName => _iModelName;
    
    /// <summary>
    /// The description of the iModel this tileset was exported from
    /// </summary>
    public string iModelDescription => _iModelDescription;
    
    /// <summary>
    /// The date when this tileset was exported
    /// </summary>
    public string exportDate => _exportDate;

    /// <summary>
    /// The ID of the changeset used for this export (empty string if latest)
    /// </summary>
    public string changesetId => _changesetId;

    /// <summary>
    /// Sets basic metadata values
    /// </summary>
    public void SetMetadata(string iTwinId, string iModelId, string iModelName, string iModelDescription, string changesetId = "")
    {
        _iTwinId = iTwinId;
        _iModelId = iModelId;
        _iModelName = iModelName;
        _iModelDescription = iModelDescription;
        _changesetId = changesetId;
        _exportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Sets extended metadata including thumbnails and additional details
    /// </summary>
    public void SetExtendedMetadata(
        string iTwinId, 
        string iTwinName,
        string iModelId, 
        string iModelName, 
        string iModelDescription,
        string changesetId,
        string changesetVersion,
        string changesetDescription,
        string changesetCreatedDate,
        string exportUrl,
        Texture2D iTwinThumbnail = null,
        Texture2D iModelThumbnail = null)
    {
        // Set basic metadata
        SetMetadata(iTwinId, iModelId, iModelName, iModelDescription, changesetId);
        
        // Set extended metadata
        _iTwinName = iTwinName ?? string.Empty;
        _changesetVersion = changesetVersion ?? string.Empty;
        _changesetDescription = changesetDescription ?? string.Empty;
        _changesetCreatedDate = changesetCreatedDate ?? string.Empty;
        _exportUrl = exportUrl ?? string.Empty;
        
        // Store thumbnails
        if (iTwinThumbnail != null)
            _iTwinThumbnailBytes = iTwinThumbnail.EncodeToPNG();
        
        if (iModelThumbnail != null)
            _iModelThumbnailBytes = iModelThumbnail.EncodeToPNG();
    }

    /// <summary>
    /// Gets the iTwin thumbnail if available
    /// </summary>
    public Texture2D GetITwinThumbnail()
    {
        if (_iTwinThumbnailBytes == null || _iTwinThumbnailBytes.Length == 0)
            return null;
            
        var tex = new Texture2D(2, 2);
        tex.LoadImage(_iTwinThumbnailBytes);
        return tex;
    }

    /// <summary>
    /// Gets the iModel thumbnail if available
    /// </summary>
    public Texture2D GetIModelThumbnail()
    {
        if (_iModelThumbnailBytes == null || _iModelThumbnailBytes.Length == 0)
            return null;
            
        var tex = new Texture2D(2, 2);
        tex.LoadImage(_iModelThumbnailBytes);
        return tex;
    }
}