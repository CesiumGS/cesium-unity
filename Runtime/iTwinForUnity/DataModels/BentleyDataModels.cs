using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an iTwin project from Bentley's iTwin platform.
/// An iTwin is a digital twin infrastructure project that can contain multiple iModels.
/// </summary>
[Serializable]
public class ITwin
{
    /// <summary>
    /// Unique identifier for the iTwin project
    /// </summary>
    public string id;
    
    /// <summary>
    /// Human-readable display name of the iTwin project
    /// </summary>
    public string displayName;
    
    /// <summary>
    /// Thumbnail image for the iTwin project (loaded asynchronously)
    /// </summary>
    [NonSerialized] public Texture2D thumbnail;
    
    /// <summary>
    /// Indicates whether the thumbnail is currently being loaded
    /// </summary>
    [NonSerialized] public bool loadingThumbnail;
    
    /// <summary>
    /// Flag to track if thumbnail loading has been attempted (regardless of success)
    /// </summary>
    [NonSerialized] public bool thumbnailLoaded = false;
}

/// <summary>
/// Represents an iModel within an iTwin project.
/// An iModel is a specific data model that contains 3D data and can have multiple changesets.
/// </summary>
[Serializable]
public class IModel
{
    /// <summary>
    /// Unique identifier for the iModel
    /// </summary>
    public string id;
    
    /// <summary>
    /// Human-readable display name of the iModel
    /// </summary>
    public string displayName;
    
    /// <summary>
    /// Detailed description of the iModel (loaded asynchronously)
    /// </summary>
    public string description;
    
    /// <summary>
    /// Thumbnail image for the iModel (loaded asynchronously)
    /// </summary>
    [NonSerialized] public Texture2D thumbnail;
    
    /// <summary>
    /// Indicates whether the thumbnail is currently being loaded
    /// </summary>
    [NonSerialized] public bool loadingThumbnail;
    
    /// <summary>
    /// Indicates whether the detailed information is currently being loaded
    /// </summary>
    [NonSerialized] public bool loadingDetails;
    
    /// <summary>
    /// Flag to track if detail loading has been attempted (regardless of success)
    /// </summary>
    [NonSerialized] public bool detailsLoaded = false;
    
    /// <summary>
    /// Flag to track if thumbnail loading has been attempted (regardless of success)
    /// </summary>
    [NonSerialized] public bool thumbnailLoaded = false;
    
    /// <summary>
    /// Indicates whether the changesets are currently being loaded
    /// </summary>
    [NonSerialized] public bool loadingChangesets;
    
    /// <summary>
    /// Collection of changesets associated with this iModel (loaded asynchronously)
    /// </summary>
    [NonSerialized] public List<ChangeSet> changesets = new List<ChangeSet>();
    
    /// <summary>
    /// Index of the currently selected changeset (0 = latest/newest)
    /// </summary>
    [NonSerialized] public int selectedChangesetIndex = 0;
}

/// <summary>
/// Represents a changeset within an iModel.
/// A changeset is a version of the iModel data at a specific point in time.
/// </summary>
[Serializable]
public class ChangeSet 
{ 
    /// <summary>
    /// Unique identifier for the changeset
    /// </summary>
    public string id; 
    
    /// <summary>
    /// Description of the changes made in this changeset
    /// </summary>
    public string description; 
    
    /// <summary>
    /// Version string identifying this changeset
    /// </summary>
    public string version; 
    
    /// <summary>
    /// Date and time when this changeset was created
    /// </summary>
    public DateTime createdDate; 
}

/// <summary>
/// Response wrapper for API calls that return multiple iTwin projects
/// </summary>
[Serializable]
public class ITwinsResponse 
{ 
    /// <summary>
    /// Collection of iTwin projects returned by the API
    /// </summary>
    public List<ITwin> iTwins; 
}

/// <summary>
/// Response wrapper for API calls that return multiple iModels
/// </summary>
[Serializable]
public class IModelsResponse 
{ 
    /// <summary>
    /// Collection of iModels returned by the API
    /// </summary>
    public List<IModel> iModels; 
}

/// <summary>
/// Response wrapper for API calls that return detailed information about a single iModel
/// </summary>
[Serializable]
public class IModelDetailsResponse 
{ 
    /// <summary>
    /// Detailed iModel information
    /// </summary>
    public IModelDetail iModel;
    
    /// <summary>
    /// Detailed iModel information structure matching the API response format
    /// </summary>
    [Serializable]
    public class IModelDetail
    {
        /// <summary>
        /// Unique identifier for the iModel
        /// </summary>
        public string id;
        
        /// <summary>
        /// Human-readable display name of the iModel
        /// </summary>
        public string displayName;
        
        /// <summary>
        /// Detailed description of the iModel
        /// </summary>
        public string description;
    }
}

/// <summary>
/// Response wrapper for API calls that return multiple changesets
/// </summary>
[Serializable]
public class ChangeSetsResponse 
{ 
    /// <summary>
    /// Collection of changesets returned by the API
    /// </summary>
    public List<ChangeSet> changesets; 
}
