// AccountLinker.cs
using UnityEngine;
using TMPro;
using System;

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
    }// [AccountLinker.cs]
// [AccountLinker.cs]

public void OnLinkButtonPress() {
    _ = RequestHandler.FromUI(async () => {
        // AuthenticationManagerからの戻り値を受け取る
        var result = await AuthenticationManager.Instance.LinkWithGoogleAsync();

        // 結果に応じてUIを更新
        switch (result) {
            case AuthenticationManager.LinkResult.Success:
                statusText.text = "<color=#28a745>連携が完了しました。</color>";
                UpdateUIState();
                break;
            case AuthenticationManager.LinkResult.AccountAlreadyLinked:
                statusText.text = "<color=red>選択したGoogleアカウントは別のセーブデータに連携済みです。</color>";
                break;
            case AuthenticationManager.LinkResult.Cancelled:
                statusText.text = "連携がキャンセルされました。";
                break;
            case AuthenticationManager.LinkResult.Failed:
                statusText.text = "<color=red>連携に失敗しました。</color>";
                break;
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