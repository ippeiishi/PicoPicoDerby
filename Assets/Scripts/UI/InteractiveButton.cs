// InteractiveButton.cs
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
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!button.interactable) return;

        string[] parts = gameObject.name.Split('_');
        if (parts.Length < 2) return;
        string actionType = parts[1];

        // 先にDialogManagerのインスタンスを確保
        if (dialogManager == null) {
            dialogManager = FindObjectOfType<DialogManager>();
        }

        // 動的ダイアログ内のボタンだが、ActionTypeが"System"でない場合に限り、
        // DialogManagerに処理を委譲する
        if (dialogManager.IsDynamicDialogObject(transform) && actionType != "System") {
            dialogManager.HandleDynamicButtonClick(gameObject);
            return;
        }

        // ActionTypeが"System"の場合、または静的UIのボタンの場合は、ここから先の処理に進む
        if (dispatcher == null) {
            dispatcher = FindObjectOfType<UIActionDispatcher>();
        }

        string targetName = (parts.Length > 2 && !string.IsNullOrEmpty(parts[2])) ? parts[2] : "";

        switch (actionType) {
            case "Open":
            case "SlideIn":
                dispatcher.DispatchOpenRequest(targetName, gameObject);
                break;
            case "Close":
            case "SlideOut":
                Transform container = transform.parent?.parent?.parent;
                if (container != null) {
                    dispatcher.DispatchCloseRequest(container.gameObject, actionType);
                }
                break;
            case "Request":
                dispatcher.DispatchGameFlowAction(targetName);
                break;
            case "System": // ← このcaseを追加
                dispatcher.DispatchSystemAction(targetName);
                break;
        }
    }
}