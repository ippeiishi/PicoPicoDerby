using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using uPalette.Generated;
using uPalette.Runtime.Core;

[RequireComponent(typeof(Button))]
public class TipItemController : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject descriptionBox;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform arrowIcon;

    private Button button;
    private bool isExpanded = false;
    private RectTransform rootContentRectTransform;

    void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleDescription);
        descriptionBox.SetActive(false);
        RotateArrow(false, 0f); // 0秒で即時反映
    }

    void Start() {
        TipsManager tipsManagerInstance = transform.root.GetComponentInChildren<TipsManager>();
        rootContentRectTransform = tipsManagerInstance.GetComponent<RectTransform>();
        descriptionBox.SetActive(true);
        Canvas.ForceUpdateCanvases();
        ForceRebuildLayouts();
        Canvas.ForceUpdateCanvases();
        descriptionBox.SetActive(false);
        ForceRebuildLayouts();
    }

    public void Setup(TipEntry tipData) {
        titleText.text = tipData.title;
        descriptionText.text = tipData.description;
    }

    private void ToggleDescription() {
        isExpanded = !isExpanded;
        descriptionBox.SetActive(isExpanded);
        RotateArrow(isExpanded,0.2f);
        ForceRebuildLayouts();
    }

    private void ForceRebuildLayouts() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.parent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContentRectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private void RotateArrow(bool isOpen, float duration){
        float targetZ = isExpanded ? -90f : 0f;
        arrowIcon.DORotate(new Vector3(0, 0, targetZ), duration).SetEase(Ease.OutQuad);
         var colorPalette = PaletteStore.Instance.ColorPalette;
        Color targetColor = isOpen
                    ? colorPalette.GetActiveValue(ColorEntry.BtnOK.ToEntryId()).Value
                    : colorPalette.GetActiveValue(ColorEntry.UIOff.ToEntryId()).Value; 
         arrowIcon.transform.GetChild(0).GetComponent<Image>().DOColor(targetColor, duration);
    }
}