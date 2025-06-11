using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InteractiveButton : MonoBehaviour, IPointerClickHandler {
    private Button button;
    private static UIActionDispatcher dispatcher;
    private static DialogManager dialogManager;

    void Awake() {
        button = GetComponent<Button>();
        // AwakeでFindするのをやめて、必要な時に一度だけ見つけるようにする
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!button.interactable) return;
        
        // --- ここで判定 ---
        if (dialogManager == null) {
            dialogManager = FindObjectOfType<DialogManager>();
        }

        // DialogManagerに「これってあなたの管理下？」と問い合わせる
        if (dialogManager.IsDynamicDialogObject(transform)) {
            dialogManager.HandleDynamicButtonClick(gameObject);
            return;
        }

        // --- 以下は静的なUI（モーダルなど）のボタンの処理 ---
        if (dispatcher == null) {
            dispatcher = FindObjectOfType<UIActionDispatcher>();
        }
        
        // (静的UIの処理は変更なし)
        #region Unchanged Code
        string[] parts = gameObject.name.Split('_');
        if (parts.Length < 2) return;

        string actionType = parts[1];
        
        switch (actionType) {
            case "Open":
            case "SlideIn":
                string targetName = (parts.Length > 2 && !string.IsNullOrEmpty(parts[2])) ? parts[2] : "";
                dispatcher.DispatchOpenRequest(targetName, gameObject);
                break;
            case "Close":
            case "SlideOut":
                Transform container = transform.parent?.parent?.parent;
                if (container != null) {
                    dispatcher.DispatchCloseRequest(container.gameObject, actionType);
                }
                break;
        }
        #endregion
    }
}