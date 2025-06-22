using UnityEngine;
using UnityEngine.UI;

public class HeaderManager : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Button menuButton;

    void Start() {
        if (TransitionManager.Instance != null) {
            TransitionManager.Instance.OnTransitionStart += OnTransitionStart;
            TransitionManager.Instance.OnTransitionEnd += OnTransitionEnd;
        }
    }

    void OnDestroy() {
        if (TransitionManager.Instance != null) {
            TransitionManager.Instance.OnTransitionStart -= OnTransitionStart;
            TransitionManager.Instance.OnTransitionEnd -= OnTransitionEnd;
        }
    }

    private void OnTransitionStart() {
        if (menuButton != null) {
            menuButton.interactable = false;
        }
    }

    private void OnTransitionEnd() {
        if (menuButton != null) {
            menuButton.interactable = true;
        }
    }
}