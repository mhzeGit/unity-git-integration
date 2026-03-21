# Unity Git Integration

A Unity Editor tool that brings Git source control into the editor with a visual commit graph, branch management, asset history, and diff viewing — all without leaving Unity.

---

## Features

- Visual commit graph with colored branch lanes (Fork-style layout)
- Browse and switch between local and remote branches
- View changed files per commit with an inline diff viewer
- Working tree panel showing staged and unstaged changes
- Per-asset Git history — right-click any asset to view its full commit log
- Working diff viewer for any modified asset
- Configuration panel for user name, email, and remote URLs
- Keyboard shortcuts: `Ctrl+H` (asset history), `Ctrl+D` (working diff), `Ctrl+Alt+G` (open window)

---

## Requirements

- Unity 6000.1 or later
- Git must be installed and available on the system PATH

---

## Installation

### Option A — Unity Package Manager (recommended)

1. Open **Window → Package Manager**
2. Click the **+** button → **Add package from git URL...**
3. Enter the repository URL and click **Add**

### Option B — Manual

1. Download or clone this repository
2. Copy the `unity-git-integration` folder into your project's `Packages/` directory
3. Unity will automatically detect and import the package

---

## How It Works

### Opening the Window

Go to **Tools → Git Integration** (or press `Ctrl+Alt+G`) to open the main Git window.

The window has three sections:

- **Sidebar** — lists local branches, remote branches, and workspace views
- **Commit graph** — shows the full branch history with colored lane lines; click any commit to see its details and changed files
- **Detail panel** — shows commit metadata, changed files, and an inline diff for any selected file

Switch between **History** and **Working Tree** in the sidebar to toggle between the commit log and your current uncommitted changes.

### Asset History & Diff

Right-click any asset in the Project window to access:

| Menu Item | Shortcut | Action |
|---|---|---|
| **Git → View History** | `Ctrl+H` | Opens the full commit history for that file |
| **Git → View Working Diff** | `Ctrl+D` | Shows the current uncommitted diff for that file |

### Configuration

Open the configuration panel from within the Git window to view or edit your Git user name, email, and remote URLs.

---

## Author

Made by [mhze](mailto:mhze.uk@gmail.com)
