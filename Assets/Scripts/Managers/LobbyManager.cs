using UnityEngine;
using UnityEngine.UI;
using TMPro;
using uPalette.Generated;
using uPalette.Runtime.Core;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public enum GameMode { Training, Legend, CustomRace, Race }

    [Header("Stage Modes")]
    [SerializeField] private GameObject modeTraining;
    [SerializeField] private GameObject modeLegend;
    [SerializeField] private GameObject modeCustomRace;
    [SerializeField] private GameObject stageRace;
    // [SerializeField] private GameObject worldRace; // <- 削除

    [Header("Tab Buttons")]
    [SerializeField] private Button trainingModeTab;
    [SerializeField] private Button legendModeTab;
    [SerializeField] private Button customRaceTab;

    private Color selectedBgColor;
    private Color selectedTextColor;
    private Color unselectedBgColor;
    private Color unselectedTextColor;

    private class TabVisualElements
    {
        public Button Button { get; }
        public Image BgImage { get; }
        public TextMeshProUGUI Text { get; }

        public TabVisualElements(Button button)
        {
            Button = button;
            BgImage = button.GetComponent<Image>();
            Text = button.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private List<TabVisualElements> tabs;
    private Button currentSelectedTab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

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

    void Start()
    {
        trainingModeTab.onClick.AddListener(() => OnTabSelected(trainingModeTab));
        legendModeTab.onClick.AddListener(() => OnTabSelected(legendModeTab));
        customRaceTab.onClick.AddListener(() => OnTabSelected(customRaceTab));

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.OnTransitionStart += OnTransitionStart;
            TransitionManager.Instance.OnTransitionEnd += OnTransitionEnd;
        }

        currentSelectedTab = trainingModeTab;
        modeTraining.SetActive(true);
        modeLegend.SetActive(false);
        modeCustomRace.SetActive(false);
        stageRace.SetActive(false);
        
        UpdateAllTabVisuals(currentSelectedTab);
    }

    void OnDestroy()
    {
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.OnTransitionStart -= OnTransitionStart;
            TransitionManager.Instance.OnTransitionEnd -= OnTransitionEnd;
        }
    }

    private void OnTabSelected(Button selectedButton)
    {
        if (currentSelectedTab == selectedButton)
        {
            return;
        }
        
        currentSelectedTab = selectedButton;
        GameMode targetMode = GameMode.Training;
        if (selectedButton == legendModeTab) targetMode = GameMode.Legend;
        if (selectedButton == customRaceTab) targetMode = GameMode.CustomRace;

        GameMode finalTargetMode = targetMode;
        TransitionManager.Instance.Play(
            onTransitionMidpoint: () => {
                SwitchMode(finalTargetMode, true);
                UpdateAllTabVisuals(selectedButton);
            }
        );
    }

    public void SwitchMode(GameMode mode, bool isTabSwitch = false)
    {
        modeTraining.SetActive(mode == GameMode.Training);
        modeLegend.SetActive(mode == GameMode.Legend);
        modeCustomRace.SetActive(mode == GameMode.CustomRace);
        stageRace.SetActive(mode == GameMode.Race);
        // worldRace.SetActive(mode == GameMode.Race); // <- 削除

        if (mode == GameMode.Race)
        {
            UIManager.Instance.ShowHeader(false);
            UIManager.Instance.HideAllFooters();
        }
        else
        {
            UIManager.Instance.ShowHeader(true);
            UIManager.Instance.ShowLobbyFooter();
        }

        if (isTabSwitch)
        {
            if (mode == GameMode.Training) currentSelectedTab = trainingModeTab;
            else if (mode == GameMode.Legend) currentSelectedTab = legendModeTab;
            else if (mode == GameMode.CustomRace) currentSelectedTab = customRaceTab;
            UpdateAllTabVisuals(currentSelectedTab);
        }
    }

    private void OnTransitionStart() { SetTabsInteractable(false); }
    private void OnTransitionEnd() { SetTabsInteractable(true); }

    private void UpdateAllTabVisuals(Button selectedButton)
    {
        foreach (var tab in tabs)
        {
            bool isSelected = (tab.Button == selectedButton);
            UpdateTabVisual(tab, isSelected);
        }
    }

    private void UpdateTabVisual(TabVisualElements tab, bool isSelected)
    {
        tab.BgImage.color = isSelected ? selectedBgColor : unselectedBgColor;
        tab.Text.color = isSelected ? selectedTextColor : unselectedTextColor;
    }

    private void SetTabsInteractable(bool isInteractable)
    {
        foreach (var tab in tabs)
        {
            tab.Button.interactable = isInteractable;
        }
    }
}