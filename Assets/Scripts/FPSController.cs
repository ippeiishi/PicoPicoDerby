using UnityEngine;
using TMPro;
using UnityEngine.Rendering;

public class FPSController : MonoBehaviour
{
    public TextMeshProUGUI Text;
    [SerializeField]
    private float m_updateInterval = 0.5f;

    private float m_accum;
    private int m_frames;
    private float m_timeleft;
    private float m_fps;

    public int targetFrameRate = 30;
    private float deltaTime = 0.0f;
    private float adjustmentThreshold = 0.5f; // この時間以上フレームレートが高い場合に調整を開始
    private float elapsedHighFrameRateTime = 0.0f;

    public void Awake()
    {
        Graphics.activeTier = GraphicsTier.Tier1;
    }

    private void Start()
    {
        Application.targetFrameRate = targetFrameRate;
    }

    private void Update()
    {
        // FPS Counter Logic
        m_timeleft -= Time.deltaTime;
        m_accum += Time.timeScale / Time.deltaTime;
        m_frames++;

        if (m_timeleft <= 0)
        {
            m_fps = m_accum / m_frames;
            m_timeleft = m_updateInterval;
            m_accum = 0;
            m_frames = 0;
        }

        // Dynamic Frame Rate Adjuster Logic
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        if (fps > targetFrameRate * 1.5) // フレームレートが1.5倍以上の場合
        {
            elapsedHighFrameRateTime += Time.unscaledDeltaTime;

            if (elapsedHighFrameRateTime > adjustmentThreshold)
            {
                Application.targetFrameRate = targetFrameRate / 2;
            }
        }
        else
        {
            elapsedHighFrameRateTime = 0.0f;
            Application.targetFrameRate = targetFrameRate;
        }
    }

    private void OnGUI()
    {
        Text.text = "FPS: " + m_fps.ToString("f2");
    }
}
