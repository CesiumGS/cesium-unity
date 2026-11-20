# iTwin Mesh Exporter for Unity

A Unity Editor extension that enables you to browse your Bentley iTwin projects, select iModels and changesets, and export 3D mesh data directly from the iTwin Platform using the [Mesh‑Export API](https://developer.bentley.com/apis/mesh-export/). The exported mesh can be loaded into your Unity scene using [Cesium for Unity](https://cesium.com/platform/cesium-for-unity/).

## Features

- **OAuth2 PKCE Authentication**: Secure login with your Bentley account using the official OAuth2 PKCE flow.
- **Project & Model Browser**: Browse your iTwin projects and iModels, with search and pagination.
- **Changeset Selection**: Choose a specific changeset/version of your iModel to export.
- **One-Click Mesh Export**: Start and monitor mesh export jobs from the Unity Editor.
- **Automatic Tileset URL Generation**: Get a ready-to-use `tileset.json` URL for Cesium.
- **Cesium for Unity Integration**: Assign the exported mesh URL to a `Cesium3DTileset` in your scene, or create a new tileset directly from the tool.
- **Tilesets Manager Window**: Browse, search, and manage all iModels tilesets in your scene.
- **Tileset Metadata Inspector**: Inspect iTwin/iModel metadata for each tileset directly in the Unity Inspector.

---

## Configuration

### Register an iTwin App

1. Sign in at the [iTwin Platform Developer Portal](https://developer.bentley.com/).
2. Under **My Apps**, click **Register New App** → **Desktop / Mobile**.
3. Add a **Redirect URI** matching your editor listener (default: `http://localhost:58789/`).
4. Grant the `itwin-platform` scope.

### Configure Redirect URI

- Default listener URI: `http://localhost:58789/`
- To customize, use the **Advanced Settings** section in the Mesh Export tool's Authentication panel and update the same URI in your app's Redirect URIs list.

---

## Usage

### Mesh Export Tool

![Mesh Export Tool Demo](docs/demo-mesh-export.gif)

Open the tool via **Bentley → Mesh Export** in the Unity Editor.

#### 1. Authentication

- Enter your **Client ID** (from your registered iTwin app) and click **Save Client ID**.
- Click **Login to Bentley** to start the OAuth2 PKCE flow.
- Sign in via the browser and grant permissions. Upon success, return to Unity.
- The tool displays your login status and token expiry.

#### 2. Select Data

- **Fetch iTwins**: Click to load your iTwin projects.
- **Browse iModels**: Select a project to view its iModels.
- **Choose Changeset**: Pick a changeset/version for export (or use the latest).

#### 3. Export Mesh

- Click **Start Export Workflow** to begin the mesh export process.
- The tool starts an export job and polls for completion.
- When finished, it generates a **Tileset URL** (`tileset.json`) for Cesium.

#### 4. Cesium Integration

- Click in `Create Cesium Tileset` button to load the exported mesh into your scene.
- Optionally, in `Advanced Options`, click **Apply URL to Existing Tileset** to load the exported mesh into your existent GameObject.

### Tilesets Manager

![Tilesets Manager Demo](docs/demo-tilesets-manager.gif)

Access via **Bentley → Tilesets** in the Unity Editor.

- **Browse Tilesets**: View all iTwin tilesets in your scene with thumbnails and metadata.
- **Search**: Filter tilesets by name or description.
- **Quick Actions**: `Select` in hierarchy, `focus` in scene view, or `browse` tileset metadata.
- **Bentley Viewer Integration**: Open any iModel directly in the Bentley iTwin Viewer, maintaining full connection with the Bentley ecosystem.

---