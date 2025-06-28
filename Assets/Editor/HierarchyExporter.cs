using UnityEngine;
using UnityEditor;
using System.Text;

public class HierarchyExporter {
    [MenuItem("GameObject/Copy Hierarchy to Clipboard (JSON)", false, 40)]
    private static void CopyHierarchy() {
        var go = Selection.activeGameObject;
        if (go == null) {
            Debug.LogWarning("Please select a GameObject in the hierarchy.");
            return;
        }

        var sb = new StringBuilder();
        // ルートオブジェクトの処理
        AppendNodeAsJson(go.transform, null, sb);
        // 子オブジェクトの処理
        AppendChildrenAsJson(go.transform, sb);
        
        EditorGUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log($"Hierarchy of '{go.name}' copied to clipboard as JSON Lines.");
    }

    private static void AppendChildrenAsJson(Transform parent, StringBuilder sb) {
        foreach (Transform child in parent) {
            // 子ノードを追加
            AppendNodeAsJson(child, parent.name, sb);
            // さらにその子を再帰的に処理
            AppendChildrenAsJson(child, sb);
        }
    }

    private static void AppendNodeAsJson(Transform node, string parentName, StringBuilder sb) {
        // JSONのキーと値はダブルクォートで囲む必要があるため、エスケープ文字を使用
        sb.Append("{ ");
        sb.Append($"\"name\": \"{node.name}\", ");
        
        // parentNameがnullでない場合（ルートオブジェクトでない場合）のみparentキーを追加
        if (parentName != null) {
            sb.Append($"\"parent\": \"{parentName}\", ");
        } else {
            sb.Append($"\"parent\": null, ");
        }

        sb.Append($"\"active\": {node.gameObject.activeSelf.ToString().ToLower()}");

        var rectTransform = node.GetComponent<RectTransform>();
        if (rectTransform != null) {
            sb.Append(", \"rect\": { ");
            sb.Append($"\"x\": {rectTransform.anchoredPosition.x:F0}, ");
            sb.Append($"\"y\": {rectTransform.anchoredPosition.y:F0}, ");
            sb.Append($"\"w\": {rectTransform.sizeDelta.x:F0}, ");
            sb.Append($"\"h\": {rectTransform.sizeDelta.y:F0}");
            sb.Append(" }");
        }

        sb.AppendLine(" }");
    }
}