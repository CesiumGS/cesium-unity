# Core Services

This directory contains the core business logic services that handle communication with external APIs and orchestrate major workflows.

## Overview

Core services are responsible for:
- Communicating with Bentley's iTwin platform APIs
- Managing data flow between UI components and external services
- Orchestrating complex workflows like authentication and mesh export
- Handling error states and retry logic
- Abstracting API complexity from UI components

## Architecture

Services in this directory follow these principles:
- **Single Responsibility**: Each service handles one specific area of functionality
- **Dependency Injection**: Services receive their dependencies through constructors
- **Asynchronous Operations**: All external API calls use Unity coroutines
- **Error Handling**: Robust error handling with user-friendly error messages
- **State Management**: Services update parent components about operation status

## Services

### BentleyAPIClient.cs
The primary service for communicating with Bentley's iTwin platform APIs.

**Responsibilities:**
- Fetching iTwins, iModels, and changesets from Bentley's APIs
- Managing thumbnail downloads with rate limiting
- Handling API authentication and authorization headers
- Orchestrating the complete mesh export workflow
- Managing pagination for large datasets

**Key Features:**
- Rate limiting protection to avoid API throttling
- Optimized thumbnail loading with caching
- Comprehensive error handling and retry logic
- Performance optimizations for UI responsiveness

**Usage:**
This service is typically instantiated by editor windows and used to fetch data from Bentley's platform. All methods return Unity coroutines that can be started using EditorCoroutineUtility.

## Error Handling

All services implement consistent error handling patterns:
- Network errors are logged and user-friendly messages are displayed
- Rate limiting (HTTP 429) responses trigger automatic retry with backoff
- Authentication errors propagate to the parent window for token refresh
- Parsing errors are caught and logged without crashing the application

## Performance Considerations

Services implement several performance optimizations:
- Thumbnail loading includes deduplication to avoid redundant requests
- UI repaint throttling prevents excessive refreshes during bulk operations
- Pagination support for large datasets
- Async operations prevent blocking the Unity Editor UI

## Extension Points

When adding new services:
1. Follow the existing dependency injection pattern
2. Use Unity coroutines for all asynchronous operations
3. Implement comprehensive error handling
4. Update parent components about operation status
5. Consider performance implications for UI responsiveness
6. Add XML documentation for all public methods
