# Tileset Management System

This directory contains components specifically designed for managing 3D tilesets within the Unity environment, including metadata handling, scene operations, and integration with Cesium for Unity.

## Overview

The Tileset Management System provides:
- Metadata management for 3D tilesets
- Custom Unity Editor interfaces for tileset configuration
- Scene integration and GameObject management
- Integration with Cesium for Unity for 3D visualization
- Persistence and serialization of tileset settings

## Architecture

The tileset system follows Unity's ScriptableObject pattern for persistent data management:
- **Metadata Components**: Store tileset configuration and properties
- **Editor Components**: Provide Unity Editor interfaces for tileset management
- **Management Components**: Handle tileset lifecycle and operations

## Directory Structure

```
Tilesets/
├── README.md                           # This file - Tileset system overview
├── BentleyTilesetMetadata.cs          # Core tileset metadata ScriptableObject
├── BentleyTilesetMetadataEditor.cs    # Custom Unity Editor for tileset metadata
└── Management/                         # Tileset management and operations
    └── [Future tileset management components]
```

## Core Components

### BentleyTilesetMetadata.cs
**Purpose**: ScriptableObject that stores persistent metadata and configuration for individual tilesets.

**Key Properties:**
- Tileset identification and source information
- Display configuration (name, description, visibility settings)
- Cesium integration parameters
- Performance and rendering settings
- Export and import configuration

**Features:**
- Serializable for Unity asset system integration
- Validation of configuration parameters
- Default value management
- Integration with Unity's asset reference system

**Usage Pattern:**
```csharp
// Creating a new tileset metadata asset
var metadata = CreateInstance<BentleyTilesetMetadata>();
metadata.tilesetUrl = "https://example.com/tileset.json";
metadata.displayName = "My Tileset";
AssetDatabase.CreateAsset(metadata, "Assets/TilesetMetadata/MyTileset.asset");
```

### BentleyTilesetMetadataEditor.cs
**Purpose**: Custom Unity Editor inspector that provides a user-friendly interface for configuring tileset metadata.

**Features:**
- Intuitive property editing with validation
- Real-time preview of configuration changes
- Integration with Unity's undo system
- Help text and documentation for configuration options
- Visual indicators for required and optional properties

**Key Editor Sections:**
- Basic tileset information (URL, name, description)
- Display and rendering settings
- Cesium integration configuration
- Performance optimization parameters
- Advanced settings and debugging options

**UI Enhancements:**
- Custom property drawers for complex data types
- Validation feedback with error and warning messages
- Contextual help and tooltips
- Organized property groups with foldout sections

## Integration Points

### With Cesium for Unity
The tileset system integrates seamlessly with Cesium for Unity:
- Automatic creation of Cesium3DTileset components
- Configuration of Cesium-specific properties
- Handling of coordinate system transformations
- Performance optimization settings for large tilesets

### With Authentication System
Tilesets may require authenticated access:
- Integration with Bentley authentication for private tilesets
- Automatic token refresh for long-running visualizations
- Secure handling of authentication headers for tileset requests

### With Editor Windows
Tileset management integrates with the main workflow:
- Selection and configuration from the main workflow window
- Bulk operations on multiple tilesets
- Import/export functionality for tileset configurations

## Data Management

### Persistence
Tileset metadata is persisted using Unity's asset system:
- ScriptableObject assets store configuration data
- Version control friendly text-based serialization
- Integration with Unity's asset reference system
- Support for asset bundles and builds

### Validation
Comprehensive validation ensures data integrity:
- URL format validation for tileset endpoints
- Required property checking
- Range validation for numeric parameters
- Dependency validation for related assets

### Serialization
Custom serialization handles complex data types:
- Coordinate system parameters
- Authentication tokens (securely)
- Performance settings and optimization flags
- Custom property collections

## Performance Considerations

### Memory Management
- Lazy loading of tileset data
- Proper disposal of Unity resources
- Efficient caching of frequently accessed data
- Memory pool usage for temporary objects

### Rendering Optimization
- Level-of-detail (LOD) configuration
- Culling and visibility optimization
- Texture and material management
- GPU memory usage monitoring

### Loading Performance
- Asynchronous tileset loading
- Progress tracking and user feedback
- Error handling and recovery
- Bandwidth optimization for large tilesets

## Development Guidelines

### Adding New Metadata Properties
1. Add property to BentleyTilesetMetadata with appropriate attributes
2. Update BentleyTilesetMetadataEditor to include UI for the new property
3. Add validation logic if required
4. Update documentation and tooltips
5. Test serialization and version compatibility

### Custom Property Drawers
When adding complex property types:
1. Create custom PropertyDrawer classes
2. Provide intuitive UI for data input
3. Include validation and error feedback
4. Support Unity's undo system
5. Test across different Unity Editor themes

### Editor Integration
For new editor functionality:
1. Follow Unity's EditorWindow patterns
2. Provide proper undo/redo support
3. Handle asset modification detection
4. Include comprehensive error handling
5. Test with large numbers of tileset assets

## Common Patterns

### Metadata Creation Pattern
```csharp
[MenuItem("Assets/Create/Bentley/Tileset Metadata")]
public static void CreateTilesetMetadata()
{
    var metadata = CreateInstance<BentleyTilesetMetadata>();
    metadata.Initialize(); // Set default values
    
    string path = AssetDatabase.GetAssetPath(Selection.activeObject);
    if (string.IsNullOrEmpty(path))
        path = "Assets";
    
    string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/New Tileset.asset");
    AssetDatabase.CreateAsset(metadata, assetPath);
    AssetDatabase.SaveAssets();
    
    Selection.activeObject = metadata;
}
```

### Validation Pattern
```csharp
public bool ValidateConfiguration(out List<string> errors)
{
    errors = new List<string>();
    
    if (string.IsNullOrEmpty(tilesetUrl))
        errors.Add("Tileset URL is required");
    
    if (!Uri.IsWellFormedUriString(tilesetUrl, UriKind.Absolute))
        errors.Add("Tileset URL must be a valid absolute URL");
    
    return errors.Count == 0;
}
```

### Editor Property Layout Pattern
```csharp
public override void OnInspectorGUI()
{
    serializedObject.Update();
    
    EditorGUILayout.BeginVertical("box");
    EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
    
    EditorGUILayout.PropertyField(serializedObject.FindProperty("tilesetUrl"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
    
    EditorGUILayout.EndVertical();
    
    if (serializedObject.ApplyModifiedProperties())
    {
        // Handle property changes
        ValidateAndUpdateUI();
    }
}
```

## Future Extensions

The tileset system is designed for extensibility:
- Support for additional tileset formats beyond Cesium 3D Tiles
- Advanced analytics and performance monitoring
- Batch processing and automation tools
- Integration with external tileset management services
- Custom rendering pipelines and effects
