using UnityEngine;
using TMPro;

/// <summary>
/// レースの距離に応じて、コース上のハロン棒を自動的に配置する。
/// </summary>
public class CourseBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _hurlonPolePrefab;

    [Header("Scene References")]
    [SerializeField] private RectTransform _containerCourse;
    // --- 定数 ---
    private const int PIXELS_PER_METRE = 10;
    private const float POLE_Y_POSITION = 70f;

    void Start(){
        for (var i = 1; i < 40; i++){
            float poleXPosition = i * -200 * PIXELS_PER_METRE;
            GameObject poleInstance = Instantiate(_hurlonPolePrefab, _containerCourse);
            RectTransform poleRect = poleInstance.GetComponent<RectTransform>();
            poleRect.anchoredPosition = new Vector2(poleXPosition, POLE_Y_POSITION);
            TextMeshProUGUI poleText = poleInstance.GetComponentInChildren<TextMeshProUGUI>();
            poleText.text = (i * 2).ToString();

        }
    }
}