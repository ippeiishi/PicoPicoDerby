using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

public class UsernameFormController : MonoBehaviour {
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private Button okButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    private const int MaxCharacterLength = 8;
    private StringInfo stringInfo;
    private static readonly string[] prohibitedWords = { "fuck", "shit", "cunt", "nigger", "bitch" };
    private const string AllowedCharsPattern = @"^[a-zA-Z0-9_\- \u3040-\u309F\u30A0-\u30FF\u4E00-\u9FFF\uF900-\uFAFF]+$";
    private Color defaultFeedbackColor;

    void Awake() {
        stringInfo = new StringInfo();
        if (feedbackText != null) {
            defaultFeedbackColor = feedbackText.color;
        }
    }

    void OnEnable() {
        usernameInputField.onValueChanged.AddListener(ValidateInput);
        ValidateInput(usernameInputField.text);
    }

    void OnDisable() {
        usernameInputField.onValueChanged.RemoveListener(ValidateInput);
    }

    private void ValidateInput(string currentInput) {
        string trimmedInput = currentInput.Trim();
        string error = GetValidationError(currentInput, trimmedInput);

        if (error != null) {
            feedbackText.color = Color.red;
            feedbackText.text = error;
            okButton.interactable = false;
        } else {
            feedbackText.color = defaultFeedbackColor;
            stringInfo.String = currentInput;
            int currentLength = stringInfo.LengthInTextElements;
            feedbackText.text = $"{currentLength}/{MaxCharacterLength}";
            okButton.interactable = true;
        }
    }

    private string GetValidationError(string originalInput, string trimmedInput) {
        if (string.IsNullOrEmpty(trimmedInput)) return "プレイヤー名を入力してください";
        
        stringInfo.String = originalInput;
        if (stringInfo.LengthInTextElements > MaxCharacterLength) return $"文字数制限 ({MaxCharacterLength}文字) を超えています";
        
        if (!string.IsNullOrEmpty(trimmedInput) && !Regex.IsMatch(trimmedInput, AllowedCharsPattern)) return "使用できない文字や記号が含まれています";
        
        if (originalInput.Any(char.IsControl)) return "使用できない文字が含まれています (制御文字)";
        
        if (Regex.IsMatch(trimmedInput, @"\s{2,}")) return "スペースを連続して使用することはできません";
        
        string lowerInput = trimmedInput.ToLowerInvariant();
        if (prohibitedWords.Any(word => lowerInput.Contains(word))) return "名前に使用できない単語が含まれています";
        
        return null;
    }
}