# Mesh Export System

This directory contains components responsible for exporting mesh data from Bentley's iTwin platform, including progress tracking, file management, and integration with the main workflow system.

## Overview

The Mesh Export System provides:
- Asynchronous mesh export from iTwin iModels
- Progress tracking and status monitoring
- Error handling and retry capabilities
- File format management and conversion
- Integration with Bentley's mesh export APIs

## Architecture

The mesh export system follows an asynchronous, status-based architecture:
- **Export Clients**: Handle communication with Bentley's export APIs
- **Progress Tracking**: Monitor export status and provide user feedback
- **File Management**: Handle downloaded files and format conversion
- **Error Handling**: Robust error recovery and user notification

## Directory Structure

```
MeshExport/
├── README.md               # This file - Mesh export system overview
└── MeshExportClient.cs    # Primary mesh export client and workflow orchestrator
```

## Core Components

### MeshExportClient.cs
**Purpose**: Primary client for orchestrating the complete mesh export workflow from initiation to final file download.

**Key Responsibilities:**
- Initiating mesh export requests with Bentley's APIs
- Monitoring export progress with polling mechanism
- Handling export status transitions (queued, processing, completed, failed)
- Downloading completed export files
- Managing export timeouts and error conditions
- Providing progress callbacks to UI components

**Export Workflow:**
1. **Export Initiation**: Start export request for specific iModel and changeset
2. **Status Polling**: Regularly check export progress and status
3. **Progress Reporting**: Update UI with current export status and progress
4. **Completion Handling**: Download files when export completes successfully
5. **Error Recovery**: Handle failures with appropriate retry logic

**Key Methods:**
- `GetOrStartExportCoroutine()`: Checks for existing exports or starts new ones
- `PollExportStatusCoroutine()`: Monitors export progress with configurable intervals
- `HandleExportComplete()`: Processes successful export completion
- `HandleExportError()`: Manages error states and recovery options

## Export Process Flow

### 1. Export Request
```
User initiates export -> Validate parameters -> Send export request -> Receive export ID
```

### 2. Status Monitoring
```
Poll export status -> Check progress -> Update UI -> Continue until complete/failed
```

### 3. Completion Handling
```
Export complete -> Download files -> Validate downloads -> Notify user -> Cleanup
```

## Data Models

### Export Status Types
The system handles various export states:
- **Queued**: Export request accepted and waiting in queue
- **Processing**: Export is actively being processed
- **Completed**: Export finished successfully, files ready for download
- **Failed**: Export encountered an error and could not complete
- **Cancelled**: Export was cancelled by user or system
- **Timeout**: Export exceeded maximum processing time

### Export Response Models
```csharp
[Serializable]
public class ExportResponse
{
    public string exportId;         // Unique identifier for this export
    public string status;          // Current export status
    public int progressPercentage; // Completion percentage (0-100)
    public string downloadUrl;     // URL for downloading completed files
    public DateTime createdDate;   // When export was initiated
    public DateTime? completedDate; // When export finished (if applicable)
    public string errorMessage;    // Error description if failed
}
```

## API Integration

### Bentley Mesh Export API
The system integrates with Bentley's mesh export APIs:
- **Export Endpoint**: Initiates new mesh export requests
- **Status Endpoint**: Retrieves current export status and progress
- **Download Endpoint**: Provides access to completed export files
- **List Endpoint**: Retrieves existing exports for an iModel

### Authentication Requirements
All API calls require proper authentication:
- Bearer token authentication using OAuth 2.0 access tokens
- Automatic token refresh when tokens expire
- Graceful handling of authentication errors
- Secure token storage and transmission

### Rate Limiting
The system respects API rate limits:
- Configurable polling intervals to avoid excessive requests
- Exponential backoff on rate limit responses (HTTP 429)
- Queue management for multiple concurrent exports
- Priority handling for different export types

## Error Handling

### Network Errors
- Connection timeout handling with configurable retry attempts
- HTTP error code interpretation and user-friendly messaging
- Network availability detection and offline graceful degradation

### Export Errors
- Detailed error message parsing from API responses
- Export failure categorization (temporary vs permanent failures)
- Automatic retry for transient failures
- User notification with actionable error information

### File Download Errors
- Verification of downloaded file integrity
- Partial download recovery and resume capability
- Disk space validation before download initiation
- File permission and access error handling

## Performance Optimization

### Polling Strategy
- Adaptive polling intervals based on export complexity
- Reduced polling frequency for long-running exports
- Immediate status checks for recently initiated exports
- Background polling to avoid blocking UI interactions

### Memory Management
- Efficient handling of large export files
- Streaming downloads for large datasets
- Proper disposal of temporary resources
- Memory usage monitoring during export operations

### Concurrency
- Support for multiple simultaneous exports
- Thread-safe status tracking and updates
- Coordinated UI updates from background threads
- Resource sharing and conflict resolution

## Configuration

### Timeout Settings
```csharp
public static class ExportConstants
{
    public const int DEFAULT_POLL_INTERVAL_SECONDS = 30;
    public const int MAX_EXPORT_TIMEOUT_MINUTES = 60;
    public const int RETRY_ATTEMPTS_ON_FAILURE = 3;
    public const int EXPONENTIAL_BACKOFF_BASE_SECONDS = 2;
}
```

### File Format Support
The system supports various export formats:
- **3D Tiles**: Cesium 3D Tiles format for web visualization
- **glTF**: Standard 3D format for general use
- **OBJ**: Wavefront OBJ format for compatibility
- **Custom**: Bentley-specific formats as needed

## Integration Points

### With Authentication System
- Seamless integration with Bentley authentication
- Automatic token refresh during long-running exports
- Proper error handling for authentication failures
- Secure credential management throughout export process

### With UI Components
- Real-time progress updates to export UI components
- Status change notifications for workflow coordination
- Error reporting with user-friendly messages
- Completion callbacks for UI state updates

### With File System
- Managed download location selection
- File organization and naming conventions
- Integration with Unity's asset import system
- Cleanup of temporary files and resources

## Development Guidelines

### Adding New Export Types
1. Define export type constants and parameters
2. Implement API request formatting for the new type
3. Add status polling logic specific to the export type
4. Update error handling for type-specific failures
5. Test with various iModel sizes and complexity levels

### Extending Progress Tracking
1. Identify additional progress metrics to track
2. Update API response parsing for new progress data
3. Enhance UI feedback with additional progress information
4. Consider performance impact of increased polling detail

### Error Recovery Enhancement
1. Analyze common failure patterns
2. Implement specific recovery strategies for each failure type
3. Add user options for manual recovery actions
4. Update error messaging with clear resolution steps

## Common Patterns

### Export Initiation Pattern
```csharp
public IEnumerator StartExportCoroutine(string iModelId, string changesetId, Action<ExportResult> onComplete)
{
    var exportRequest = new ExportRequest
    {
        iModelId = iModelId,
        changesetId = changesetId,
        format = ExportFormat.CesiumTiles
    };
    
    yield return SendExportRequestCoroutine(exportRequest);
    yield return PollExportStatusCoroutine(exportRequest.exportId, onComplete);
}
```

### Status Polling Pattern
```csharp
private IEnumerator PollExportStatusCoroutine(string exportId, Action<ExportResult> onComplete)
{
    var startTime = DateTime.Now;
    
    while (DateTime.Now - startTime < TimeSpan.FromMinutes(MAX_EXPORT_TIMEOUT_MINUTES))
    {
        var status = yield return GetExportStatusCoroutine(exportId);
        
        if (status.IsComplete)
        {
            onComplete?.Invoke(status);
            yield break;
        }
        
        yield return new WaitForSeconds(GetPollInterval(status));
    }
    
    // Handle timeout
    onComplete?.Invoke(ExportResult.Timeout);
}
```

### Error Handling Pattern
```csharp
private void HandleExportError(ExportError error)
{
    switch (error.Type)
    {
        case ExportErrorType.Transient:
            // Retry with exponential backoff
            ScheduleRetry(error.RetryCount);
            break;
            
        case ExportErrorType.Authentication:
            // Refresh token and retry
            RefreshTokenAndRetry();
            break;
            
        case ExportErrorType.Permanent:
            // Notify user and stop
            NotifyUserOfPermanentFailure(error.Message);
            break;
    }
}
```

## Future Enhancements

The mesh export system is designed for extensibility:
- Support for additional export formats and parameters
- Advanced progress visualization and analytics
- Batch export capabilities for multiple iModels
- Integration with cloud storage services
- Custom post-processing workflows for exported data
