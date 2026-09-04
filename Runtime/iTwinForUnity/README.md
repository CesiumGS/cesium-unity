# iTwin Unity Runtime Components

This document provides a comprehensive overview of the iTwin Unity plugin's runtime architecture and components.

## Overview

The iTwin Unity Runtime contains all components that can be used at runtime in Unity, including MonoBehaviours, data models, and utility classes that don't depend on UnityEditor APIs. This assembly is designed to work in both the Unity Editor and in built Unity applications.

## Architecture Principles

The Runtime assembly follows several key architectural principles:

### Runtime-Safe Design
- No dependencies on UnityEditor APIs
- Components work in both Editor and built applications  
- Compatible with all Unity build targets

### Component-Based Design
- Classes are broken down into focused, single-responsibility components
- Each component handles a specific aspect of functionality
- Components communicate through well-defined interfaces

### Separation of Concerns
- Data models are separate from business logic
- Scene components are separate from utility functions
- Clear boundaries between different functional areas

### Architectural Layers
- **Data Layer**: Serializable data models and constants
- **Component Layer**: Unity MonoBehaviours and scene components
- **Service Layer**: Runtime-safe utilities and mesh export functionality
- **Infrastructure Layer**: Constants, configurations, and cross-cutting concerns

## Assembly Structure

```
Runtime/
├── Runtime.asmdef                     # Assembly definition (references: CesiumRuntime)
├── README.md                          # This documentation
├── BentleyTilesetMetadata.cs          # Runtime MonoBehaviour component
├── Constants/                         # Application constants and configurations
│   ├── TilesetConstants.cs            # Tileset-related constants
│   └── AuthConstants.cs               # Authentication constants
├── DataModels/                        # Data transfer objects and models
│   └── BentleyDataModels.cs           # Core data models (iTwin, iModel, etc.)
├── MeshExport/                        # Runtime mesh export functionality
│   ├── README.md                      # Mesh export documentation
│   └── MeshExportClient.cs            # Runtime-compatible export client
└── Utils/                             # Runtime-safe utility classes
    └── (Runtime utility methods)      # Helper methods for runtime use
```

## Key Components

### BentleyTilesetMetadata
**File**: `BentleyTilesetMetadata.cs`  
**Type**: MonoBehaviour Component

A Unity component that stores iTwin metadata associated with Cesium3DTileset objects. This component:
- Stores iTwin project and iModel information
- Maintains changeset details and export metadata
- Provides inspector-friendly serialized fields
- Integrates with Cesium for Unity runtime

### Data Models (`/DataModels`)
**Type**: Serializable Data Classes

Core data structures that represent iTwin platform entities:
- `ITwin`: Represents an iTwin project
- `IModel`: Represents an iModel within an iTwin
- `Changeset`: Represents a version/changeset of an iModel
- Response models for API communication

### Constants (`/Constants`)
**Type**: Static Configuration Classes

Application-wide constants and configuration values:
- Authentication endpoints and parameters
- Tileset management constants
- API URLs and default values

### Mesh Export (`/MeshExport`)
**Type**: Service Classes

Runtime-compatible mesh export functionality:
- API communication for mesh export requests
- Progress tracking and status monitoring
- URL generation for exported tilesets

## Assembly Dependencies

### External Dependencies
- **CesiumRuntime**: Integration with Cesium for Unity's runtime components
- **Unity.Mathematics**: For mathematical operations
- **Newtonsoft.Json**: JSON serialization/deserialization

### Usage Guidelines
- All classes in this assembly must be runtime-safe
- No `using UnityEditor;` statements allowed
- Components should work in both Editor and built applications
- Use `#if UNITY_EDITOR` guards only for non-essential Editor-specific optimizations

## Integration with Editor Assembly

The Runtime assembly is referenced by the Editor assembly (`iTwinForUnity.Editor`) which provides:
- Custom inspectors for Runtime components
- Editor windows and UI functionality
- Authentication and project browsing
- Advanced tileset management tools

For questions about the runtime architecture or implementation details, refer to the XML documentation comments in the code or consult the Editor assembly documentation for UI-related functionality.
