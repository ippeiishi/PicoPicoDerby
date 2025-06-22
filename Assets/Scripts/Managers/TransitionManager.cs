using UnityEngine;
using DG.Tweening;
using System;

public class TransitionManager : MonoBehaviour {
    public static TransitionManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject transitionContainer;
    [SerializeField] private Transform spriteMaskTransform;

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 0.4f;
    [SerializeField] private float initialScale = 800f;

    public event Action OnTransitionStart;
    public event Action OnTransitionEnd;

    private bool isTransitioning = false;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void Play(Action onTransitionMidpoint, Action onTransitionComplete = null) {
        if (isTransitioning) {
            return;
        }
        isTransitioning = true;

        OnTransitionStart?.Invoke(); // ★開始イベントを発行

        transitionContainer.SetActive(true);
        spriteMaskTransform.localScale = new Vector3(initialScale, initialScale, 1f);

        spriteMaskTransform.DOScale(0, transitionDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                onTransitionMidpoint?.Invoke();

                spriteMaskTransform.DOScale(initialScale, transitionDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        transitionContainer.SetActive(false);
                        isTransitioning = false;
                        onTransitionComplete?.Invoke();
                        
                        OnTransitionEnd?.Invoke(); // ★終了イベントを発行
                    });
            });
    }
}