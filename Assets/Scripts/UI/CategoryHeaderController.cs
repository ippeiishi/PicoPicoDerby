using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using uPalette.Generated;
using uPalette.Runtime.Core;

[RequireComponent(typeof(Button))]
public class CategoryHeaderController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private GameObject tipsContainer;
    [SerializeField] private Transform arrowIcon;

    private Button button;
    private bool isExpanded = false;
    private RectTransform rootContentRectTransform;
    public Transform TipsContainer => tipsContainer.transform;

    void Awake(){
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleContainer);
        tipsContainer.SetActive(false);
        RotateArrow(false, 0f); // 0秒で即時反映
    }

    void Start(){
        TipsManager tipsManagerInstance = transform.root.GetComponentInChildren<TipsManager>();
        rootContentRectTransform = tipsManagerInstance.GetComponent<RectTransform>();
        tipsContainer.SetActive(true);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>()); 
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContentRectTransform);
        Canvas.ForceUpdateCanvases();
        tipsContainer.SetActive(false);
    }

    public void Setup(string categoryName){
        headerText.text = categoryName;
    }
    private void ToggleContainer(){
        isExpanded = !isExpanded;
        tipsContainer.SetActive(isExpanded);
        RotateArrow(isExpanded,0.2f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContentRectTransform);
    }

    private void RotateArrow(bool isOpen, float duration)
    {
        float targetZ = isExpanded ? -90f : 0f;
        arrowIcon.DORotate(new Vector3(0, 0, targetZ), duration).SetEase(Ease.OutQuad);
           arrowIcon.transform.GetChild(0).GetComponent<Image>();
        var colorPalette = PaletteStore.Instance.ColorPalette;
        Color targetColor = isOpen
                    ? colorPalette.GetActiveValue(ColorEntry.BtnOK.ToEntryId()).Value
                    : colorPalette.GetActiveValue(ColorEntry.UIOff.ToEntryId()).Value; 
         arrowIcon.transform.GetChild(0).GetComponent<Image>().DOColor(targetColor, duration);
    }
}