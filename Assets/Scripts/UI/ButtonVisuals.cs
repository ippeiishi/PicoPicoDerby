using UnityEngine;
using UnityEngine.UI;
using TMPro;
using uPalette.Generated;
using uPalette.Runtime.Core;
using uPalette.Runtime.Core.Synchronizer.Color;
using System.Collections.Generic;

[RequireComponent(typeof(Button))]
public class ButtonVisuals : MonoBehaviour {
    private Button button;
    
    private GraphicColorSynchronizer bgSynchronizer;
    private List<GraphicColorSynchronizer> textSynchronizers = new List<GraphicColorSynchronizer>();
    private List<GraphicColorSynchronizer> imageSynchronizers = new List<GraphicColorSynchronizer>();
    
    // --- OFF状態の色を、役割に応じて個別に保持 ---
    private Color commonOffBgColor;   // 背景用のOFF色
    private Color commonOffFgColor;   // 前景(テキスト/アイコン)用のOFF色
    
    private bool previousInteractableState;

    void Awake() {
        button = GetComponent<Button>();

        bgSynchronizer = GetComponent<GraphicColorSynchronizer>();
        
        foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(true)) {
            var sync = text.GetComponent<GraphicColorSynchronizer>();
            if (sync != null) textSynchronizers.Add(sync);
        }
        
        foreach (var img in GetComponentsInChildren<Image>(true)) {
            if (img.gameObject == this.gameObject) continue;
            var sync = img.GetComponent<GraphicColorSynchronizer>();
            if (sync != null) imageSynchronizers.Add(sync);
        }

        // uPaletteから、役割に応じた正しいOFF色を取得する
        var colorPalette = PaletteStore.Instance.ColorPalette;
        commonOffBgColor = colorPalette.GetActiveValue(ColorEntry.UIOffBG.ToEntryId()).Value; // ← 背景用のグレーを指定
        commonOffFgColor = colorPalette.GetActiveValue(ColorEntry.UIOff.ToEntryId()).Value;     // ← テキスト/アイコン用のグレーを指定
    }

    void Start() {
        previousInteractableState = button.interactable;
        UpdateVisualState(button.interactable);
    }

    void Update() {
        if (button.interactable != previousInteractableState) {
            previousInteractableState = button.interactable;
            UpdateVisualState(button.interactable);
        }
    }

    private void UpdateVisualState(bool isInteractable) {
        if (isInteractable) {
            // uPaletteに制御を返す
            if (bgSynchronizer != null) bgSynchronizer.enabled = true;
            foreach (var sync in textSynchronizers) sync.enabled = true;
            foreach (var sync in imageSynchronizers) sync.enabled = true;
        } else {
            // uPaletteから制御を奪う
            if (bgSynchronizer != null) bgSynchronizer.enabled = false;
            foreach (var sync in textSynchronizers) sync.enabled = false;
            foreach (var sync in imageSynchronizers) sync.enabled = false;

            // 強制的に、役割に応じたOFF色を適用する
            var backgroundImage = GetComponent<Image>();
            Color newBgColor = commonOffBgColor; // 背景用のOFF色
            newBgColor.a = backgroundImage.color.a;
            backgroundImage.color = newBgColor;
            
            foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(true)) {
                if(text.gameObject == gameObject) continue;
                text.color = commonOffFgColor; // 前景用のOFF色
            }
            
            foreach (var img in GetComponentsInChildren<Image>(true)) {
                if (img.gameObject == gameObject) continue;
                Color newImgColor = commonOffFgColor; // 前景用のOFF色
                newImgColor.a = img.color.a;
                img.color = newImgColor;
            }
        }
    }
}