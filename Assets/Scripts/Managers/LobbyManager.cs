using UnityEngine;
using UnityEngine.UI;
using TMPro;
using uPalette.Generated;
using uPalette.Runtime.Core;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour {
    [Header("Tab Buttons")]
    [SerializeField] private Button trainingModeTab;
    [SerializeField] private Button legendModeTab;
    [SerializeField] private Button customRaceTab;

    [Header("Content Panels")]
    [SerializeField] private GameObject trainingModeContent;
    [SerializeField] private GameObject legendModeContent;
    [SerializeField] private GameObject customRaceContent;

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

        // ★イベント購読
        TransitionManager.Instance.OnTransitionStart += OnTransitionStart;
        TransitionManager.Instance.OnTransitionEnd += OnTransitionEnd;

        currentSelectedTab = trainingModeTab;
        trainingModeContent.SetActive(true);
        legendModeContent.SetActive(false);
        customRaceContent.SetActive(false);
        UpdateAllTabVisuals(currentSelectedTab);
    }

    void OnDestroy() {
        // ★イベント購読解除
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

        // ボタンの無効化/有効化はイベント経由に任せる
        TransitionManager.Instance.Play(
            onTransitionMidpoint: () => {
                trainingModeContent.SetActive(selectedButton == trainingModeTab);
                legendModeContent.SetActive(selectedButton == legendModeTab);
                customRaceContent.SetActive(selectedButton == customRaceTab);
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