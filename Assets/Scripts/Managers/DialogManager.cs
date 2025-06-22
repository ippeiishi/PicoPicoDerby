using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;

public class DialogManager : MonoBehaviour {
    public static DialogManager Instance { get; private set; }

    [SerializeField] private Transform dialogsParent;
    [SerializeField] private GameObject accountSettingsDialog;

    private Dictionary<GameObject, Action> okButtonActions = new Dictionary<GameObject, Action>();
    private const string FRAME_PATH = "UI/Dialogs/Frames/Dialog_Frame";
    private const string CONTENTS_PATH = "UI/Dialogs/Contents/";
    private const float ANIM_DURATION = 0.3f;

    void Awake() { Instance = this; }
    void Start() { UIActionDispatcher.Instance.OnRequestOpen += HandleOpenRequest; }
    void OnDestroy() { if (UIActionDispatcher.Instance != null) { UIActionDispatcher.Instance.OnRequestOpen -= HandleOpenRequest; } }

    public bool IsDynamicDialogObject(Transform objectTransform) {
        return objectTransform.IsChildOf(dialogsParent);
    }

    public void HandleDynamicButtonClick(GameObject buttonObject) {
        Transform panelTransform = buttonObject.transform.parent.parent;
        GameObject dialogInstance = panelTransform.parent.gameObject;
        string buttonName = buttonObject.name;

        if (buttonName.Contains("OK")) {
            if (okButtonActions.TryGetValue(dialogInstance, out Action action)) {
                action?.Invoke();
            } else {
                AnimatePopupClose(dialogInstance);
            }
        } else if (buttonName.Contains("Close") || buttonName.Contains("Cancel")) {
            AnimatePopupClose(dialogInstance);
        }
    }

    private void HandleOpenRequest(string targetName, GameObject clickedButton) {
        if (clickedButton != null) {
            switch (targetName) {
                case "SoundSettings": ShowSoundSettingsDialog(clickedButton); break;
                case "ReturnToTitle": ShowReturnToTitleDialog(clickedButton); break;
                case "ConfirmDeleteData": ShowConfirmDeleteDataDialog(clickedButton); break;
                case "AccountSettings": AnimatePopupOpen(accountSettingsDialog); break;
            }
        } else {
            switch (targetName) {
                case "NetworkError": ShowErrorDialog("ネットワークエラー", "インターネットに接続できませんでした。接続を確認して、もう一度お試しください。"); break;
                case "ServerError": ShowErrorDialog("サーバーエラー", "サーバーとの通信に失敗しました。時間をおいてから、もう一度お試しください。"); break;
                case "DeviceConflictError": ShowErrorDialog("お知らせ", "新しい端末へのセーブデータの移行を確認しました。この端末ではこれ以上プレイできません。"); break;
                case "DataNotFoundError": ShowErrorDialog("エラー", "セーブデータの取得に失敗しました。タイトルに戻ります。"); break;
            }
        }
    }

    public void ShowErrorDialog(string title, string message, string footerPath = CONTENTS_PATH + "Content_Footer_BackToTitle") {
        GameObject frameInstance = InstantiatePrefab(FRAME_PATH);
        GameObject messageInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Message");
        GameObject footerInstance = InstantiatePrefab(footerPath);

        AssembleDialog(frameInstance, title, messageInstance, footerInstance);
        messageInstance.GetComponentInChildren<TextMeshProUGUI>().text = message;
    }

    private void ShowSoundSettingsDialog(GameObject clickedButton) {
        GameObject frameInstance = InstantiatePrefab(FRAME_PATH);
        GameObject contentInstance = InstantiatePrefab(CONTENTS_PATH + "Content_SoundSettings");
        GameObject footerInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Footer_OK");

        Slider bgmSlider = contentInstance.transform.Find("BGMSlider").GetComponent<Slider>();
        Slider seSlider = contentInstance.transform.Find("SESlider").GetComponent<Slider>();
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        seSlider.value = PlayerPrefs.GetFloat("SEVolume", 1.0f);
        bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        seSlider.onValueChanged.AddListener(AudioManager.Instance.SetSEVolume);

        AssembleDialog(frameInstance, clickedButton, contentInstance, footerInstance);
    }

    private void ShowReturnToTitleDialog(GameObject clickedButton) {
        GameObject frameInstance = InstantiatePrefab(FRAME_PATH);
        GameObject messageInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Message");
        GameObject footerInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Footer_YesNo");

        okButtonActions[frameInstance] = () => { UIActionDispatcher.Instance.DispatchSystemAction("ReloadScene"); };
        AssembleDialog(frameInstance, clickedButton, messageInstance, footerInstance);
        messageInstance.GetComponentInChildren<TextMeshProUGUI>().text = "タイトルに戻りますか？";
    }

    private void ShowConfirmDeleteDataDialog(GameObject clickedButton) {
        GameObject frameInstance = InstantiatePrefab(FRAME_PATH);
        GameObject messageInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Message");
        GameObject footerInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Footer_YesNo");

        okButtonActions[frameInstance] = () => { ShowFinalConfirmDeleteDataDialog(); };
        AssembleDialog(frameInstance, "データ削除", messageInstance, footerInstance);
        messageInstance.GetComponentInChildren<TextMeshProUGUI>().text = "本当によろしいですか？\nこの操作は取り消せません。";
    }

    private void ShowFinalConfirmDeleteDataDialog() {
        GameObject frameInstance = InstantiatePrefab(FRAME_PATH);
        GameObject messageInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Message");
        GameObject footerInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Footer_YesNo");

        okButtonActions[frameInstance] = async () => {
            GameFlowManager.Instance.SetLoadingScreenActive(true);
            if (AuthenticationManager.Instance.IsLinkedWithGoogle()) {
                await AuthenticationService.Instance.UnlinkGoogleAsync();
            }
            bool wipeSuccess = await DataManager.Instance.ExecuteFullDataWipeAsync();
            AuthenticationManager.Instance.SignOut();
            GameFlowManager.Instance.SetLoadingScreenActive(false);

            if (wipeSuccess) {
                ShowDeleteCompleteDialog("データは正常に削除されました。\nタイトルに戻ります。");
            } else {
                ShowDeleteCompleteDialog("データの削除に失敗しました。\nタイトルに戻ります。");
            }
        };

        AssembleDialog(frameInstance, "最終確認", messageInstance, footerInstance);
        messageInstance.GetComponentInChildren<TextMeshProUGUI>().text = "セーブデータを完全に削除します。\n本当の本当に、よろしいですか？";
    }

    private void ShowDeleteCompleteDialog(string message) {
        GameObject frameInstance = InstantiatePrefab(FRAME_PATH);
        GameObject messageInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Message");
        GameObject footerInstance = InstantiatePrefab(CONTENTS_PATH + "Content_Footer_BackToTitle");

        AssembleDialog(frameInstance, "処理完了", messageInstance, footerInstance);
        messageInstance.GetComponentInChildren<TextMeshProUGUI>().text = message;
    }

    private GameObject InstantiatePrefab(string path) {
        var prefab = Resources.Load<GameObject>(path);
        if (prefab == null) { return null; }
        return Instantiate(prefab, dialogsParent);
    }

    public void AssembleDialog(GameObject frameInstance, GameObject clickedButton, params GameObject[] contentInstances) {
        var buttonTextComponent = clickedButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        AssembleDialogInternal(frameInstance, buttonTextComponent.text, contentInstances);
    }

    public void AssembleDialog(GameObject frameInstance, string title, params GameObject[] contentInstances) {
        AssembleDialogInternal(frameInstance, title, contentInstances);
    }

    private void AssembleDialogInternal(GameObject frameInstance, string title, params GameObject[] contentInstances) {
        Transform panel = frameInstance.transform.Find("Panel_Dialog");
        panel.transform.Find("Header").GetChild(0).GetComponent<TextMeshProUGUI>().text = title;

        foreach (var content in contentInstances) {
            content.transform.SetParent(panel, false);
        }
        AnimatePopupOpen(frameInstance);
    }

    public void AnimatePopupOpen(GameObject target, Action onComplete = null) {
        Transform panel = target.transform.Find("Panel_Dialog");
        Image bgImage = target.transform.Find("BG_Overlay").GetComponent<Image>();
        CanvasGroup panelCg = panel.GetComponent<CanvasGroup>();

        target.SetActive(true);
        bgImage.color = new Color(bgImage.color.r, bgImage.color.g, bgImage.color.b, 0);
        bgImage.DOFade(0.5f, ANIM_DURATION);

        panelCg.alpha = 0;
        panelCg.DOFade(1, ANIM_DURATION).SetEase(Ease.OutQuad);

        panel.localScale = Vector3.zero;
        var buttons = target.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons) { b.interactable = false; }

        panel.DOScale(1, ANIM_DURATION).SetEase(Ease.OutBack).onComplete = () => {
            foreach (var b in buttons) { b.interactable = true; }
            onComplete?.Invoke();
        };
    }

    public void AnimatePopupClose(GameObject target) {
        Transform panel = target.transform.Find("Panel_Dialog");
        Image bgImage = target.transform.Find("BG_Overlay").GetComponent<Image>();
        CanvasGroup panelCg = panel.GetComponent<CanvasGroup>();

        var buttons = target.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons) { b.interactable = false; }

        panelCg.DOFade(0, ANIM_DURATION).SetEase(Ease.InQuad);
        bgImage.DOFade(0, ANIM_DURATION);
        panel.DOScale(0, ANIM_DURATION).SetEase(Ease.InBack).onComplete = () => {
            if (okButtonActions.ContainsKey(target)) { okButtonActions.Remove(target); }
            if (IsDynamicDialogObject(target.transform)) {
                Destroy(target);
            } else {
                target.SetActive(false);
            }
        };
    }
}