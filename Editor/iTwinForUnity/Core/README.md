# Core Components

This directory contains the core business logic components that form the foundation of the iTwin Unity plugin. These components handle the essential functionality independent of UI concerns.

## Overview

The Core layer represents the business logic tier of the application architecture. It contains:
- Core services for API communication
- Business logic components
- Workflow orchestration
- Data management and processing

## Architecture Principles

### Separation of Concerns
Core components are isolated from UI logic, making them:
- Easier to test independently
- Reusable across different UI implementations
- More maintainable and less prone to breaking changes

### Dependency Injection
Core components receive their dependencies through constructors, enabling:
- Better testability through dependency mocking
- Loose coupling between components
- Easier maintenance and refactoring

### Single Responsibility
Each core component has a clearly defined, single responsibility:
- Services handle external API communication
- Managers handle data state and lifecycle
- Processors handle data transformation and validation

## Directory Structure

```
Core/
├── README.md                   # This file - Core architecture overview
└── Services/                   # External API communication services
    ├── README.md              # Services documentation
    └── BentleyAPIClient.cs    # Primary API communication service
```

## Component Types

### Services
Services handle communication with external systems and APIs. They:
- Abstract API complexity from other components
- Handle authentication and authorization
- Manage error handling and retry logic
- Provide consistent interfaces for data access

### Future Extensions
As the plugin grows, additional component types may be added:
- **Managers**: Handle data lifecycle and state management
- **Processors**: Transform and validate data
- **Validators**: Ensure data integrity and business rules
- **Factories**: Create complex objects with proper initialization

## Integration with Other Layers

### With Presentation Layer (EditorWindows)
- Core services are injected into editor windows
- Editor windows call service methods and handle UI updates based on results
- Core components notify UI through callbacks or state updates

### With Authentication Layer
- Core services use authentication managers for API authorization
- Services handle token refresh and authentication errors gracefully

### With Common Layer
- Core components use shared data models and constants
- Utility functions are leveraged for common operations

## Development Guidelines

### Adding New Core Components
1. Identify the single responsibility of the new component
2. Define clear interfaces and dependencies
3. Implement comprehensive error handling
4. Add XML documentation for all public APIs
5. Follow the established naming conventions
6. Update this README with component documentation

### Testing Considerations
- Core components should be easily unit testable
- Dependencies should be injectable for mocking
- Business logic should be isolated from Unity-specific code where possible

### Performance
- Core components should be efficient and not block the UI
- Use asynchronous operations for long-running tasks
- Consider memory usage and object lifecycle management

## Common Patterns

### Coroutine-Based Async Operations
Most external operations use Unity coroutines:
```csharp
public IEnumerator ProcessDataCoroutine()
{
    // Async operation implementation
    yield return operation;
    // Handle results
}
```

### Dependency Injection Pattern
Core components receive dependencies through constructors:
```csharp
public class ServiceClass
{
    private IDependency dependency;
    
    public ServiceClass(IDependency dependency)
    {
        this.dependency = dependency;
    }
}
```

### Error Handling Pattern
Consistent error handling with user-friendly messages:
```csharp
if (result.IsSuccess)
{
    // Handle success
}
else
{
    // Log technical details
    Debug.LogError($"Technical error: {result.Error}");
    // Show user-friendly message
    statusMessage = "A user-friendly error message";
}
```
