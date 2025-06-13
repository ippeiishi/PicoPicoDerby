using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro; // TextMeshProを使うために必要

public class ModalManager : MonoBehaviour {
    [Header("UI Containers")]
    [SerializeField] private GameObject menuModal;
    [SerializeField] private GameObject subModalsContainer;
    [SerializeField] private Transform subModalContentsParent;
    
    private const float ANIM_DURATION = 0.3f;
    private const float SLIDE_DISTANCE = 370f;
    private const float BG_TARGET_ALPHA = 0.5f;

    void Start() {
        if (UIActionDispatcher.Instance != null) {
            // DialogManagerと同様に、GameObjectを受け取るイベントを購読する
            UIActionDispatcher.Instance.OnRequestOpen += HandleOpenRequest;
            UIActionDispatcher.Instance.OnRequestClose += HandleCloseRequest;
        }
    }

    void OnDestroy() {
        if (UIActionDispatcher.Instance != null) {
            UIActionDispatcher.Instance.OnRequestOpen -= HandleOpenRequest;
            UIActionDispatcher.Instance.OnRequestClose -= HandleCloseRequest;
        }
    }

    // HandleOpenRequestがGameObjectを受け取るように変更
    private void HandleOpenRequest(string targetName, GameObject clickedButton) {
        if (targetName == "Menu") {
            // メインメニューにはタイトルがないので、clickedButtonは不要
            AnimateSlideIn(menuModal, null); 
        } else {
            OpenSubModal(targetName, clickedButton);
        }
    }

    private void HandleCloseRequest(GameObject panelToClose, string closeType) {
        AnimateSlideOut(panelToClose);
    }

    // OpenSubModalもGameObjectを受け取るように変更
    private void OpenSubModal(string targetName, GameObject clickedButton) {
        string contentNameToFind = targetName.Replace("Modal", "Content");
        Transform targetContent = subModalContentsParent.Find(contentNameToFind);

        if (targetContent != null) {
            foreach (Transform content in subModalContentsParent) {
                content.gameObject.SetActive(false);
            }
            targetContent.gameObject.SetActive(true);
            // AnimateSlideInにclickedButtonを渡す
            AnimateSlideIn(subModalsContainer, clickedButton);
        }
    }
    
    private void AnimateSlideIn(GameObject target, GameObject clickedButton) {
        Transform panel = target.transform.Find("Panel_Menu") ?? target.transform.Find("Panel_Full");
        Image bgImage = target.transform.Find("BG_Overlay")?.GetComponent<Image>();
        if (panel == null) return;
        
        if (clickedButton != null) {
            var buttonText = clickedButton.transform.GetChild(0)?.GetComponent<TextMeshProUGUI>();
            var titleText = panel.transform.Find("Header")?.GetChild(0)?.GetComponent<TextMeshProUGUI>();
            
            if (buttonText != null && titleText != null) {
                titleText.text = buttonText.text;
            }
        }
        
        var buttons = target.GetComponentsInChildren<Button>(true);

        target.SetActive(true);

        if (bgImage != null) {
            bgImage.color = new Color(bgImage.color.r, bgImage.color.g, bgImage.color.b, 0);
            bgImage.DOFade(BG_TARGET_ALPHA, ANIM_DURATION);
        }

        panel.localPosition = new Vector2(SLIDE_DISTANCE, 0);
        foreach (var b in buttons) { b.interactable = false; }

        panel.DOLocalMoveX(0, ANIM_DURATION).SetEase(Ease.OutQuad).onComplete = () => {
            foreach (var b in buttons) { b.interactable = true; }
        };
    }

    private void AnimateSlideOut(GameObject target) {
        Transform panel = target.transform.Find("Panel_Menu") ?? target.transform.Find("Panel_Full");
        Image bgImage = target.transform.Find("BG_Overlay")?.GetComponent<Image>();
        if (panel == null) return;

        var buttons = target.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons) { b.interactable = false; }

        if (bgImage != null) {
            bgImage.DOFade(0, ANIM_DURATION);
        }

        panel.DOLocalMoveX(SLIDE_DISTANCE, ANIM_DURATION).SetEase(Ease.InQuad).onComplete = () => {
            target.SetActive(false);
        };
    }
}