// UIActionDispatcher.cs
using UnityEngine;
using System;

public class UIActionDispatcher : MonoBehaviour {
    public static UIActionDispatcher Instance { get; private set; }

    public event Action<string, GameObject> OnRequestOpen;
    public event Action<GameObject, string> OnRequestClose;
    public event Action<string> OnRequestGameFlowAction;
    public event Action<string> OnRequestSystemAction; // ← この行を追加

    void Awake() {
        Instance = this;
    }

    public void DispatchOpenRequest(string targetName, GameObject clickedButton) {
        OnRequestOpen?.Invoke(targetName, clickedButton);
    }

    public void DispatchCloseRequest(GameObject panelToClose, string closeType) {
        OnRequestClose?.Invoke(panelToClose, closeType);
    }
    
    public void DispatchGameFlowAction(string actionName) {
        OnRequestGameFlowAction?.Invoke(actionName);
    }

    public void DispatchSystemAction(string actionName) { // ← このメソッドを追加
        OnRequestSystemAction?.Invoke(actionName);
    }
}