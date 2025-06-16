// AccountLinker.cs
using UnityEngine;
using TMPro;

public class AccountLinker : MonoBehaviour {
    [Header("State Contents")]
    [SerializeField] private GameObject contentUnlinked;
    [SerializeField] private GameObject contentLinked;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject linkButton;
    [SerializeField] private GameObject unlinkButton;


    async void OnEnable(){
        UpdateUIState();
        statusText.text = "";
        await AuthenticationManager.Instance.EnsureGoogleSignInStateAsync();
}
    private void UpdateUIState() {
        bool isLinked = AuthenticationManager.Instance.IsLinkedWithGoogle();
        contentUnlinked.SetActive(!isLinked);
        contentLinked.SetActive(isLinked);
    }

    public void OnLinkButtonPress() {
        _ = RequestHandler.FromUI(async () => {
            bool success = await AuthenticationManager.Instance.LinkWithGoogleAsync();
            if (success) {
                statusText.text ="<color=#28a745>連携が完了しました。</color>";
                UpdateUIState();
            } else {
                statusText.text ="<color=red>連携に失敗しました。</color>";
            }
        });
    }

    public void OnUnlinkButtonPress() {
        _ = RequestHandler.FromUI(async () => {
            bool success = await AuthenticationManager.Instance.UnlinkFromGoogleAsync();
            if (success) {
                statusText.text ="<color=#28a745>連携を解除しました。</color>";
                UpdateUIState();
            } else {
                statusText.text ="<color=red>連携の解除に失敗しました。</color>";
            }
        });
    }

    public void OnCloseButtonPressed() {
        UpdateUIState();
        DialogManager.Instance.AnimatePopupClose(this.gameObject);
    }
}