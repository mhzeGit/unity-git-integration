using UnityEditor;
using UnityEngine;

namespace GitIntegration
{
    /// <summary>Adds Git context menu items to the Project window.</summary>
    public static class GitContextMenu
    {
        private const string MENU_PATH = "Assets/Git/View History %h";
        private const string MENU_DIFF  = "Assets/Git/View Working Diff %d";

        // View History

        [MenuItem(MENU_PATH, false, 1000)]
        private static void ViewHistory()
        {
            string assetPath = GetSelectedAssetPath();
            if (string.IsNullOrEmpty(assetPath))
            {
                EditorUtility.DisplayDialog("Git", "No asset selected.", "OK");
                return;
            }
            GitAssetHistoryWindow.ShowForAsset(assetPath);
        }

        [MenuItem(MENU_PATH, true)]
        private static bool ViewHistoryValidate()
        {
            return !string.IsNullOrEmpty(GetSelectedAssetPath()) && GitOperations.IsInsideRepo();
        }

        // Working Diff

        [MenuItem(MENU_DIFF, false, 1001)]
        private static void ViewWorkingDiff()
        {
            string assetPath = GetSelectedAssetPath();
            if (string.IsNullOrEmpty(assetPath))
            {
                EditorUtility.DisplayDialog("Git", "No asset selected.", "OK");
                return;
            }
            GitDiffViewerWindow.ShowWorkingDiff(assetPath);
        }

        [MenuItem(MENU_DIFF, true)]
        private static bool ViewWorkingDiffValidate()
        {
            return !string.IsNullOrEmpty(GetSelectedAssetPath()) && GitOperations.IsInsideRepo();
        }

        // Helper

        private static string GetSelectedAssetPath()
        {
            if (Selection.activeObject == null) return null;
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path)) return null;
            // Skip folders
            if (AssetDatabase.IsValidFolder(path)) return null;
            return path;
        }
    }
}
