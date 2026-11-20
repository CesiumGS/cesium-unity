# Editor Windows

This directory contains Unity Editor window implementations that provide the main user interface for the iTwin Unity plugin. These windows enable users to browse iTwins, select projects, manage authentication, and export mesh data.

## Overview

The Editor Windows layer represents the presentation tier of the application architecture. It provides:
- User-friendly interfaces for complex workflows
- Component-based UI architecture for maintainability
- Responsive design that adapts to different window sizes
- Visual feedback for long-running operations

## Architecture

The Editor Windows follow a component-based architecture:
- **Main Windows**: Primary editor windows that host the overall workflow
- **UI Components**: Reusable UI components that handle specific aspects of the interface
- **Controllers**: Manage window state and coordinate between components

## Directory Structure

```
EditorWindows/
├── README.md                           # This file - Editor windows overview
├── BentleyWorkflowEditorWindow.cs     # Main workflow editor window
├── BentleyTilesetsWindow.cs           # Specialized tileset management window
├── Components/                         # Reusable UI components
│   ├── AuthenticationComponent.cs     # Authentication UI and controls
│   ├── CesiumIntegrationComponent.cs  # Cesium-specific integration UI
│   ├── EmptyStateComponent.cs         # Empty state and loading displays
│   ├── ExportComponent.cs             # Mesh export controls and progress
│   ├── PaginationComponent.cs         # Pagination controls for large datasets
│   ├── ProjectBrowserComponent.cs     # iTwin and iModel browsing interface
│   ├── SearchComponent.cs             # Search functionality for projects
│   ├── TilesetCardRenderer.cs         # Individual tileset card rendering
│   ├── WelcomeComponent.cs            # Welcome screen and initial setup
│   └── WorkflowStepIndicatorComponent.cs # Visual workflow progress indicator
└── Controllers/                        # Window state and workflow management
    ├── TilesetsWindowController.cs     # Tileset window state management
    └── WorkflowStateManager.cs         # Main workflow state coordination
```

## Main Windows

### BentleyWorkflowEditorWindow.cs
The primary editor window that provides a complete workflow for:
- User authentication with Bentley's iTwin platform
- Browsing and selecting iTwins and iModels
- Viewing project details and changesets
- Initiating and monitoring mesh exports
- Integrating with Cesium for Unity for 3D visualization

**Key Features:**
- Step-by-step workflow with visual progress indicators
- Responsive design that adapts to window resizing
- Error handling with user-friendly messages
- Background operation support with progress tracking

### BentleyTilesetsWindow.cs
A specialized window focused on tileset management:
- Advanced tileset browsing and filtering
- Bulk operations on multiple tilesets
- Detailed tileset metadata viewing
- Integration with the main workflow window

## UI Components

### Authentication Components

#### AuthenticationComponent.cs
**Purpose**: Handles all authentication-related UI elements and user interactions.

**Features:**
- Client ID configuration and validation
- Login/logout controls with visual feedback
- Token expiration warnings and refresh capabilities
- Secure storage of authentication preferences
- Responsive layout that adapts to authentication status

**UI Elements:**
- Client ID input field with validation
- Login/logout buttons with appropriate states
- Token expiration display with color-coded warnings
- Loading indicators during authentication processes

### Project Management Components

#### ProjectBrowserComponent.cs
**Purpose**: Provides browsing interface for iTwins and iModels with advanced features.

**Features:**
- Paginated display of large project lists
- Thumbnail loading with performance optimization
- Project details on-demand loading
- Search integration for quick project location
- Responsive card-based layout

**Performance Optimizations:**
- Lazy loading of thumbnails and details
- UI repaint throttling during bulk operations
- Memory-efficient pagination with configurable page sizes

#### SearchComponent.cs
**Purpose**: Implements search functionality across iTwins and iModels.

**Features:**
- Real-time search with debouncing
- Multiple search criteria support
- Search result highlighting
- Integration with pagination for large result sets

### Workflow Components

#### WorkflowStepIndicatorComponent.cs
**Purpose**: Provides visual feedback about the current step in the workflow process.

**Features:**
- Step-by-step progress visualization
- Current step highlighting
- Completion status indicators
- Navigation between completed steps

#### ExportComponent.cs
**Purpose**: Manages the mesh export process with comprehensive progress tracking.

**Features:**
- Export configuration controls
- Real-time progress monitoring
- Error handling and retry capabilities
- Export result validation and feedback
- Integration with external export services

### Utility Components

#### PaginationComponent.cs
**Purpose**: Provides consistent pagination controls across different data views.

**Features:**
- Configurable page sizes
- Navigation controls (first, previous, next, last)
- Page number display with current position
- Integration with data loading states

#### EmptyStateComponent.cs
**Purpose**: Displays appropriate messages and actions when data is not available.

**Features:**
- Context-specific empty state messages
- Loading state indicators with animated spinners
- Error state displays with recovery actions
- Customizable call-to-action buttons

#### TilesetCardRenderer.cs
**Purpose**: Renders individual tileset information in a consistent card format.

**Features:**
- Responsive card layout
- Thumbnail display with loading states
- Metadata display with formatting
- Action buttons for tileset operations

## Controllers

### WorkflowStateManager.cs
**Purpose**: Manages the overall state of the main workflow window.

**Responsibilities:**
- Coordinating state transitions between workflow steps
- Managing communication between UI components
- Handling error states and recovery
- Persisting workflow state across sessions

### TilesetsWindowController.cs
**Purpose**: Controls the specialized tileset management window.

**Responsibilities:**
- Managing tileset data loading and filtering
- Coordinating bulk operations
- Handling window-specific state management
- Integration with the main workflow

## Design Patterns

### Component-Based Architecture
Each UI component is self-contained and responsible for:
- Its own rendering logic
- User interaction handling
- State management for its specific functionality
- Communication with parent windows through defined interfaces

### Observer Pattern
Components observe changes in:
- Authentication state
- Data loading states
- User selections
- Error conditions

### Command Pattern
User actions are encapsulated as commands:
- Authentication requests
- Data refresh operations
- Export initiation
- Navigation between workflow steps

## Performance Considerations

### UI Responsiveness
- Long-running operations use Unity coroutines to avoid blocking the UI
- Progress indicators provide feedback during operations
- UI updates are throttled to prevent excessive repaints

### Memory Management
- Thumbnails and large data sets are loaded on-demand
- Unused UI resources are properly disposed
- Pagination limits memory usage for large datasets

### Network Efficiency
- API requests are debounced to avoid excessive calls
- Thumbnail loading includes rate limiting
- Error retry logic prevents request flooding

## Styling and Theming

### Consistent Visual Design
- Shared style classes ensure consistent appearance
- Icon usage follows Unity Editor conventions
- Color schemes adapt to Unity's light/dark themes
- Responsive layouts work across different window sizes

### Accessibility
- Keyboard navigation support where applicable
- Screen reader compatible labels and descriptions
- High contrast support for visual elements
- Consistent focus indicators

## Development Guidelines

### Adding New Components
1. Identify the single responsibility of the component
2. Define clear interfaces for parent window communication
3. Implement proper state management
4. Add comprehensive error handling
5. Follow the established styling patterns
6. Include XML documentation for all public methods

### Modifying Existing Components
1. Ensure backward compatibility with parent windows
2. Test across different Unity Editor themes
3. Verify responsive behavior at different window sizes
4. Update documentation for any interface changes

### Performance Guidelines
- Use Unity's EditorGUILayout efficiently
- Minimize GUI calls in hot paths
- Implement proper resource disposal
- Consider memory implications of UI state

## Common Patterns

### Component Initialization
```csharp
public class ComponentName
{
    private ParentWindow parentWindow;
    
    public ComponentName(ParentWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    public void DrawComponent()
    {
        // Component rendering logic
    }
}
```

### Error Handling in UI
```csharp
if (errorState)
{
    EditorGUILayout.HelpBox("User-friendly error message", MessageType.Error);
    if (GUILayout.Button("Retry"))
    {
        // Retry logic
    }
}
```

### Responsive Layout
```csharp
EditorGUILayout.BeginHorizontal();
if (parentWindow.position.width > 400)
{
    // Wide layout
}
else
{
    // Compact layout
}
EditorGUILayout.EndHorizontal();
```
