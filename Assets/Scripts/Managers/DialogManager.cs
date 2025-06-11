using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class DialogManager : MonoBehaviour {
    public static DialogManager Instance { get; private set; }

    [SerializeField] private Transform dialogsParent;

    private const string FRAME_PATH = "UI/Dialogs/Frames/Dialog_Frame";
    private const string CONTENTS_PATH = "UI/Dialogs/Contents/";
    private const float ANIM_DURATION = 0.3f;

    void Awake() {
        Instance = this;
    }

    public bool IsDynamicDialogObject(Transform objectTransform) {
        return objectTransform.IsChildOf(dialogsParent);
    }
    
    void Start() {
        if (UIActionDispatcher.Instance != null) {
            // 新しいイベントのシグネチャに合わせて購読する
            UIActionDispatcher.Instance.OnRequestOpen += HandleOpenRequest;
        }
    }

    void OnDestroy() {
        if (UIActionDispatcher.Instance != null) {
            UIActionDispatcher.Instance.OnRequestOpen -= HandleOpenRequest;
        }
    }
    
      public void HandleDynamicButtonClick(GameObject buttonObject) {
        Transform panelTransform = buttonObject.transform.parent.parent;
        GameObject dialogInstance = panelTransform.parent.gameObject;

        string buttonName = buttonObject.name;
        if (buttonName.Contains("Close") || buttonName.Contains("Cancel")) {
            AnimatePopupClose(dialogInstance);
        } else if (buttonName.Contains("OK")) {
            AnimatePopupClose(dialogInstance);
        }
    }
    // HandleOpenRequestがGameObjectを受け取るように変更
    private void HandleOpenRequest(string targetName, GameObject clickedButton) {
        switch (targetName) {
            case "SoundSettings":
                ShowSoundSettingsDialog(clickedButton);
                break;
            case "ReturnToTitle":
                ShowReturnToTitleDialog(clickedButton);
                break;
        }
    }
    
    // 各Show...メソッドもGameObjectを受け取って、AssembleDialogに渡す
    private void ShowSoundSettingsDialog(GameObject clickedButton) {
        GameObject frameInstance = InstantiatePrefab(FRAME_PATH);
        GameObject contentInstance = InstantiatePrefab(CONTENTS_PATH + "Content_SoundSettings");
        GameObject footerInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Footer_OK");
        if (frameInstance == null) return;
        AssembleDialog(frameInstance, clickedButton, contentInstance, footerInstance);
    }

    private void ShowReturnToTitleDialog(GameObject clickedButton) {
        GameObject frameInstance = InstantiatePrefab(FRAME_PATH);
        GameObject messageInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Message");
        GameObject footerInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Footer_YesNo");
        if (frameInstance == null) return;
        AssembleDialog(frameInstance, clickedButton, messageInstance, footerInstance);
        
        var messageText = messageInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (messageText != null) {
            messageText.text = "アプリを再起動してタイトルに戻ります。";
        }
    }

    private GameObject InstantiatePrefab(string path) {
        var prefab = Resources.Load<GameObject>(path);
        if (prefab == null) return null;
        return Instantiate(prefab, dialogsParent);
    }
    
    // AssembleDialogが、ハードコードされたtitleの代わりにclickedButtonを受け取る
    private void AssembleDialog(GameObject frameInstance, GameObject clickedButton, params GameObject[] contentInstances) {
        Transform panel = frameInstance.transform.Find("Panel_Dialog");
        if (panel == null) {
            Destroy(frameInstance);
            return;
        }

        // --- タイトルを動的に設定するロジック ---
        var buttonText = clickedButton.transform.GetChild(0)?.GetComponent<TextMeshProUGUI>();
        var titleText = panel.transform.Find("Header")?.GetChild(0)?.GetComponent<TextMeshProUGUI>();

        if (buttonText != null && titleText != null) {
            titleText.text = buttonText.text;
        }

        foreach (var content in contentInstances) {
            if (content != null) {
                content.transform.SetParent(panel, false);
            }
        }
        AnimatePopupOpen(frameInstance);
    }

        private void AnimatePopupOpen(GameObject target) {
        Transform panel = target.transform.Find("Panel_Dialog");
        Image bgImage = target.transform.Find("BG_Overlay")?.GetComponent<Image>();
        CanvasGroup panelCg = panel.GetComponent<CanvasGroup>();
        target.SetActive(true);
        bgImage.color = new Color(bgImage.color.r, bgImage.color.g, bgImage.color.b, 0);
        bgImage.DOFade(0.5f, ANIM_DURATION);
        panelCg.alpha = 0;
        panelCg.DOFade(1, ANIM_DURATION).SetEase(Ease.OutQuad);
        panel.DOScale(0, 0);
         var buttons = target.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons) { b.interactable = false; }
        panel.DOScale(1, ANIM_DURATION).SetEase(Ease.OutBack).onComplete = () => {
            foreach (var b in buttons) { b.interactable = true; }
        };
    }

    private void AnimatePopupClose(GameObject target) {
        Transform panel = target.transform.Find("Panel_Dialog");
        Image bgImage = target.transform.Find("BG_Overlay")?.GetComponent<Image>();
        CanvasGroup panelCg = panel.GetComponent<CanvasGroup>();
        var buttons = target.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons) { b.interactable = false; }
        panelCg.DOFade(0, ANIM_DURATION).SetEase(Ease.InQuad);
        bgImage.DOFade(0, ANIM_DURATION);
        panel.DOScale(0, ANIM_DURATION).SetEase(Ease.InBack).onComplete = () => {
            Destroy(target);
        };
    }
}