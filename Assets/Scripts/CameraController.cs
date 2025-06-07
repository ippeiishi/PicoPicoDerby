using UnityEngine;

public class CameraController : MonoBehaviour {
    private float aspectWidth = 360.0f;
    private float aspectHeight = 640.0f;  // ここを640に変更
    private Camera attachedCamera;
    private Vector2Int lastScreenSize;

    void Awake() {
        attachedCamera = GetComponent<Camera>();
        AdjustCamera();
    }

    void Update() {
        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);

        // Check if the screen size has changed
        if (screenSize != lastScreenSize) {
            AdjustCamera();
            lastScreenSize = screenSize;
        }
    }

    public void AdjustCamera() {
        float targetAspect = aspectWidth / aspectHeight;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;
        Rect rect = new Rect(0, 0, 1, 1);

        if (scaleHeight < 1.0f) {
            rect.height = scaleHeight;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        } else {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.x = (1.0f - scaleWidth) / 2.0f;
        }

        attachedCamera.rect = rect;

        // Apply a solid black background color outside of the camera view
        attachedCamera.clearFlags = CameraClearFlags.SolidColor;
        attachedCamera.backgroundColor = Color.black;
    }
}
