using UnityEngine;
using UnityEngine.UI;
using TMPro;
using uPalette.Generated;
using uPalette.Runtime.Core;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour {
    // ★変更点: ヘッダーのコメントをより正確な "Stage Modes" に変更
    [Header("Stage Modes")] 
    // ★変更点: 変数名をContentからModeに変更し、参照先をMode_オブジェクトにする
    [SerializeField] private GameObject modeTraining;
    [SerializeField] private GameObject modeLegend;
    [SerializeField] private GameObject modeCustomRace;

    [Header("Tab Buttons")]
    [SerializeField] private Button trainingModeTab;
    [SerializeField] private Button legendModeTab;
    [SerializeField] private Button customRaceTab;

    private Color selectedBgColor;
    private Color selectedTextColor;
    private Color unselectedBgColor;
    private Color unselectedTextColor;

    private class TabVisualElements {
        public Button Button { get; }
        public Image BgImage { get; }
        public TextMeshProUGUI Text { get; }

        public TabVisualElements(Button button) {
            Button = button;
            BgImage = button.GetComponent<Image>();
            Text = button.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private List<TabVisualElements> tabs;
    private Button currentSelectedTab;

    void Awake() {
        var colorPalette = PaletteStore.Instance.ColorPalette;
        selectedBgColor = colorPalette.GetActiveValue(ColorEntry.BtnOK.ToEntryId()).Value;
        selectedTextColor = colorPalette.GetActiveValue(ColorEntry.UIPureWhite.ToEntryId()).Value;
        unselectedBgColor = colorPalette.GetActiveValue(ColorEntry.UIPanelBase.ToEntryId()).Value;
        unselectedTextColor = colorPalette.GetActiveValue(ColorEntry.TextMain.ToEntryId()).Value;

        tabs = new List<TabVisualElements> {
            new TabVisualElements(trainingModeTab),
            new TabVisualElements(legendModeTab),
            new TabVisualElements(customRaceTab)
        };
    }

    void Start() {
        trainingModeTab.onClick.AddListener(() => OnTabSelected(trainingModeTab));
        legendModeTab.onClick.AddListener(() => OnTabSelected(legendModeTab));
        customRaceTab.onClick.AddListener(() => OnTabSelected(customRaceTab));

        if (TransitionManager.Instance != null) {
            TransitionManager.Instance.OnTransitionStart += OnTransitionStart;
            TransitionManager.Instance.OnTransitionEnd += OnTransitionEnd;
        }

        currentSelectedTab = trainingModeTab;
        // ★変更点: 初期表示をModeオブジェクトに
        modeTraining.SetActive(true);
        modeLegend.SetActive(false);
        modeCustomRace.SetActive(false);
        UpdateAllTabVisuals(currentSelectedTab);
    }

    void OnDestroy() {
        if (TransitionManager.Instance != null) {
            TransitionManager.Instance.OnTransitionStart -= OnTransitionStart;
            TransitionManager.Instance.OnTransitionEnd -= OnTransitionEnd;
        }
    }

    private void OnTabSelected(Button selectedButton) {
        if (currentSelectedTab == selectedButton) {
            return;
        }
        
        currentSelectedTab = selectedButton;

        TransitionManager.Instance.Play(
            onTransitionMidpoint: () => {
                // ★変更点: 切り替え対象をModeオブジェクトに
                modeTraining.SetActive(selectedButton == trainingModeTab);
                modeLegend.SetActive(selectedButton == legendModeTab);
                modeCustomRace.SetActive(selectedButton == customRaceTab);
                UpdateAllTabVisuals(selectedButton);
            }
        );
    }

    private void OnTransitionStart() { SetTabsInteractable(false); }
    private void OnTransitionEnd() { SetTabsInteractable(true); }

    private void UpdateAllTabVisuals(Button selectedButton) {
        foreach (var tab in tabs) {
            bool isSelected = (tab.Button == selectedButton);
            UpdateTabVisual(tab, isSelected);
        }
    }

    private void UpdateTabVisual(TabVisualElements tab, bool isSelected) {
        tab.BgImage.color = isSelected ? selectedBgColor : unselectedBgColor;
        tab.Text.color = isSelected ? selectedTextColor : unselectedTextColor;
    }

    private void SetTabsInteractable(bool isInteractable) {
        foreach (var tab in tabs) {
            tab.Button.interactable = isInteractable;
        }
    }
}