using UnityEngine;
using TMPro;

public class CourseBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _prefabHurlonPole;

    [Header("Scene References")]
    [SerializeField] private RectTransform _containerHurlonPole;
    private const int PIXELS_PER_METRE = 10;
    private const float POLE_Y_POSITION = 50;
    private const int METRES_PER_POLE = 200;
    private const int POLE_LABEL_MULTIPLIER = 2;

    void Start()
    {
        for (var i = 1; i < 40; i++)
        {
            float poleXPosition = i * -METRES_PER_POLE * PIXELS_PER_METRE;
            GameObject poleInstance = Instantiate(_prefabHurlonPole, _containerHurlonPole);
            RectTransform poleRect = poleInstance.GetComponent<RectTransform>();
            poleRect.anchoredPosition = new Vector2(poleXPosition, POLE_Y_POSITION);
            TextMeshProUGUI poleText = poleInstance.GetComponentInChildren<TextMeshProUGUI>();
            poleText.text = (i * POLE_LABEL_MULTIPLIER).ToString();
        }
    }
    
    public void AdjustPoleTextDirection(float directionMultiplier) {
    foreach (Transform child in _containerHurlonPole) {
        var text = child.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) {
            Vector3 localScale = text.transform.localScale;
            localScale.x = Mathf.Abs(localScale.x) * directionMultiplier;
            text.transform.localScale = localScale;
        }
    }
}

}