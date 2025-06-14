using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class TipsManager : MonoBehaviour {
    [Header("Prefabs")]
    [SerializeField] private GameObject categoryHeaderPrefab;
    [SerializeField] private GameObject tipItemPrefab;

    [Header("Container")]
    [SerializeField] private Transform contentContainer;

    void Awake() {
        if (contentContainer == null) { contentContainer = transform; }
    }

    void OnEnable() {
        InitializeAndDisplayTips();
    }

    private void InitializeAndDisplayTips() {
        if (contentContainer.childCount > 0) { return; }

        string tipsJson = RemoteConfigManager.Instance.TipsJson;
        if (string.IsNullOrEmpty(tipsJson) || tipsJson == "[]") { return; }

        var allTips = JsonConvert.DeserializeObject<List<TipEntry>>(tipsJson);
        var groupedTips = allTips
            .GroupBy(tip => tip.category)
            .Select(group => new CategorizedTips(group.Key) { tips = group.ToList() })
            .ToList();

        BuildUI(groupedTips);
    }

    private void BuildUI(List<CategorizedTips> categorizedTipsList) {
        foreach (Transform child in contentContainer) {
            Destroy(child.gameObject);
        }

        foreach (var categoryData in categorizedTipsList) {
            GameObject categoryObj = Instantiate(categoryHeaderPrefab, contentContainer);
            var categoryController = categoryObj.GetComponent<CategoryHeaderController>();
            categoryController.Setup(categoryData.categoryName);

            foreach (var tipData in categoryData.tips) {
                GameObject tipObj = Instantiate(tipItemPrefab, categoryController.TipsContainer);
                tipObj.GetComponent<TipItemController>().Setup(tipData);
            }
        }
    }
}