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
        // 基本情報（名前とアクティブ状態）を構築
        string status = parent.gameObject.activeSelf ? "" : " (inactive)";
        string line = indent + " " + parent.name + status;

        // RectTransformコンポーネントを取得試行
        var rectTransform = parent.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // RectTransformが存在する場合、座標とサイズ情報を追記
            // :F0フォーマットで小数点以下を非表示にする
            string rectInfo = $" [x:{rectTransform.anchoredPosition.x:F0}, y:{rectTransform.anchoredPosition.y:F0}, w:{rectTransform.sizeDelta.x:F0}, h:{rectTransform.sizeDelta.y:F0}]";
            line += rectInfo;
        }

        sb.AppendLine(line);

        // 再帰呼び出しで、次のインデントにハイフンを追加する
        foreach (Transform child in parent) {
            AppendChildren(child, indent + "-", sb);
        }
    }
}