using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InteractiveButton : MonoBehaviour, IPointerClickHandler
{
    private Button button;
    private static UIActionDispatcher dispatcher;
    private static DialogManager dialogManager;

    void Awake() {
        button = GetComponent<Button>();
    }

    // --- サウンド再生機能のみをここに追加 ---
    private void PlaySoundFromName(string[] nameParts) {
        if (nameParts.Length < 2) { return; }
        string soundType = nameParts[nameParts.Length - 1];
        switch (soundType) {
            case "OK": AudioManager.i.PlayOKSe(); break;
            case "Cancel": AudioManager.i.PlayCancelSe(); break;
            case "Slide": case "SlideOut": AudioManager.i.PlayslideoutSe(); break;
            default: AudioManager.i.PlayClickSe(); break;
        }
    }
    // ------------------------------------

    public void OnPointerClick(PointerEventData eventData) {
        if (!button.interactable) return;

        string[] parts = gameObject.name.Split('_');

        // --- 追加したサウンド再生処理をここで呼び出す ---
        PlaySoundFromName(parts);
        // ------------------------------------------

        // ▼▼▼ ここから下は、あなたのオリジナルのコードを一切変更していません ▼▼▼
        if (parts.Length < 2) return;
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