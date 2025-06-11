using UnityEngine;
using System;

public class UIActionDispatcher : MonoBehaviour {
    public static UIActionDispatcher Instance { get; private set; }

    // stringだけでなく、GameObjectも一緒に渡せるようにイベントを拡張
    public event Action<string, GameObject> OnRequestOpen;
    public event Action<GameObject, string> OnRequestClose;

    void Awake() {
        Instance = this;
    }

    // DispatcherもGameObjectを受け取り、そのままイベントに流す
    public void DispatchOpenRequest(string targetName, GameObject clickedButton) {
        OnRequestOpen?.Invoke(targetName, clickedButton);
    }

    public void DispatchCloseRequest(GameObject panelToClose, string closeType) {
        OnRequestClose?.Invoke(panelToClose, closeType);
    }
}