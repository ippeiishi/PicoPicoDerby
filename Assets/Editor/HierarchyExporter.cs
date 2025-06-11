using UnityEngine;
using UnityEditor;
using System.Text;

public class HierarchyExporter {
    [MenuItem("GameObject/Copy Hierarchy to Clipboard", false, 40)]
    private static void CopyHierarchy() {
        var go = Selection.activeGameObject;
        if (go == null) {
            Debug.LogWarning("Please select a GameObject in the hierarchy.");
            return;
        }

        var sb = new StringBuilder();
        // 初回呼び出し時に、最初のインデントとして"-"を渡す
        AppendChildren(go.transform, "-", sb);
        EditorGUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log($"Hierarchy of '{go.name}' copied to clipboard.");
    }

    private static void AppendChildren(Transform parent, string indent, StringBuilder sb) {
        // Inactiveなオブジェクトも、同じ行に表示する
        string status = parent.gameObject.activeSelf ? "" : " (inactive)";
        sb.AppendLine(indent + " " + parent.name + status);

        // 再帰呼び出しで、次のインデントにハイフンを追加する
        foreach (Transform child in parent) {
            AppendChildren(child, indent + "-", sb);
        }
    }
}