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
        if (!button.interactable) {
            return;
        }

        string[] parts = gameObject.name.Split('_');
        // ★ サウンド再生ロジックを削除

        if (parts.Length < 2) {
            return;
        }
        string actionType = parts[1];

        if (dialogManager == null) {
            dialogManager = FindObjectOfType<DialogManager>();
        }

        if (dialogManager.IsDynamicDialogObject(transform) && actionType != "System") {
            dialogManager.HandleDynamicButtonClick(gameObject);
            return;
        }

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
                Debug.Log($"Closing container for action type: {actionType}");
                Transform container = transform.parent?.parent?.parent;
                if (container != null) {
                    dispatcher.DispatchCloseRequest(container.gameObject, actionType);
                }
                break;
            case "Request":
                dispatcher.DispatchGameFlowAction(targetName);
                break;
            case "System":
                dispatcher.DispatchSystemAction(targetName);
                break;
        }
    }
}