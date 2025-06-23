using UnityEngine;
using UnityEngine.UI;
using TMPro;
using uPalette.Generated;
using uPalette.Runtime.Core;
using uPalette.Runtime.Core.Synchronizer.Color;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonEnhancer : MonoBehaviour, IPointerDownHandler {
    private Button button;
    
    private GraphicColorSynchronizer bgSynchronizer;
    private List<GraphicColorSynchronizer> textSynchronizers = new List<GraphicColorSynchronizer>();
    private List<GraphicColorSynchronizer> imageSynchronizers = new List<GraphicColorSynchronizer>();
    
    private Color commonOffBgColor;
    private Color commonOffFgColor;
    
    private bool previousInteractableState;

    public void OnPointerDown(PointerEventData eventData) {
        if (!button.interactable) {
            return;
        }
        if (AudioManager.Instance != null) {
            string soundToPlay = GetSoundNameFromObjectName();
            AudioManager.Instance.PlaySE(soundToPlay);
        }
    }

    private string GetSoundNameFromObjectName() {
        string objectName = gameObject.name.Replace("(Clone)", "");
        
        string[] parts = objectName.Split('_');
        
        if (parts.Length >= 4) {
            return parts[3];
        }
        
        return "Click";
    }

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

        var colorPalette = PaletteStore.Instance.ColorPalette;
        commonOffBgColor = colorPalette.GetActiveValue(ColorEntry.UIOffBG.ToEntryId()).Value;
        commonOffFgColor = colorPalette.GetActiveValue(ColorEntry.UIOff.ToEntryId()).Value;
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
            if (bgSynchronizer != null) bgSynchronizer.enabled = true;
            foreach (var sync in textSynchronizers) sync.enabled = true;
            foreach (var sync in imageSynchronizers) sync.enabled = true;
        } else {
            if (bgSynchronizer != null) bgSynchronizer.enabled = false;
            foreach (var sync in textSynchronizers) sync.enabled = false;
            foreach (var sync in imageSynchronizers) sync.enabled = false;

            var backgroundImage = GetComponent<Image>();
            Color newBgColor = commonOffBgColor;
            newBgColor.a = backgroundImage.color.a;
            backgroundImage.color = newBgColor;
            
            foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(true)) {
                if(text.gameObject == gameObject) continue;
                text.color = commonOffFgColor;
            }
            
            foreach (var img in GetComponentsInChildren<Image>(true)) {
                if (img.gameObject == gameObject) continue;
                Color newImgColor = commonOffFgColor;
                newImgColor.a = img.color.a;
                img.color = newImgColor;
            }
        }
    }
}