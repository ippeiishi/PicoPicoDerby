// UIActionInvoker.cs (修正版)
using UnityEngine;

public class UIActionInvoker : MonoBehaviour {
    private static UIActionDispatcher dispatcher;

    public void InvokeCloseRequest() {
        if (dispatcher == null) {
            dispatcher = FindObjectOfType<UIActionDispatcher>();
        }

        // 自分の名前を解析して、閉じるアニメーションのタイプを取得する
        // 例: "BG_Overlay_SlideOut" -> parts[2]は"SlideOut"
        string[] parts = gameObject.name.Split('_');
        string closeType = (parts.Length > 2) ? parts[2] : "Close"; // 名前がなければデフォルトで"Close"

        var container = this.transform.parent;
        if (container != null) {
            dispatcher.DispatchCloseRequest(container.gameObject, closeType);
            
        }
    }
}