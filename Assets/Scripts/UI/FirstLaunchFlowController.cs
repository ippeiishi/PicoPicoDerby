using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;
using static AuthenticationManager;

public class FirstLaunchFlowController : MonoBehaviour {
    [Header("Content Panels")]
    [SerializeField] private GameObject newOrContinueContent;
    [SerializeField] private GameObject newOrContinueFooter;
    [SerializeField] private GameObject createUserDataContent;
    [SerializeField] private GameObject createUserDataFooter;
    [SerializeField] private GameObject welcomeContent;
    [SerializeField] private GameObject welcomeFooter;

    [Header("UI References")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TextMeshProUGUI welcomeMessageText;

    private enum FlowState {
        NewOrContinue,
        CreateUserData,
        Welcome
    }

    void OnEnable() {
        SwitchState(FlowState.NewOrContinue);
    }

    private void SwitchState(FlowState newState) {
        newOrContinueContent.SetActive(false);
        newOrContinueFooter.SetActive(false);
        createUserDataContent.SetActive(false);
        createUserDataFooter.SetActive(false);
        welcomeContent.SetActive(false);
        welcomeFooter.SetActive(false);
        switch (newState) {
            case FlowState.NewOrContinue:
                newOrContinueContent.SetActive(true);
                newOrContinueFooter.SetActive(true);
                break;
            case FlowState.CreateUserData:
                createUserDataContent.SetActive(true);
                createUserDataFooter.SetActive(true);
                break;
            case FlowState.Welcome:
                welcomeContent.SetActive(true);
                welcomeFooter.SetActive(true);
                break;
        }
    }

    public void OnNewGameButtonPressed() {
        SwitchState(FlowState.CreateUserData);
    }

    public void OnContinueButtonPressed() {
        _ = ContinueFlowAsync();
    }

    private Task ContinueFlowAsync() {
        return RequestHandler.FromUI(async () => {
            RecoveryResult result = await AuthenticationManager.Instance.SignInWithGoogleForRecoveryAsync();

            switch (result) {
                case RecoveryResult.Success:
                    await RemoteConfigManager.Instance.FetchConfigsAsync();
                    await CloudSaveManager.Instance.UpdateDeviceIDAfterRecoveryAsync();
                    CloudSaveManager.Instance.SetFirstLaunchCompleted();

                    // ユーザー名を取得し、Welcome画面に切り替える
                    string username = CloudSaveManager.Instance.CurrentPlayerData.username;
                    welcomeMessageText.text = $"{username}さん、おかえりなさい！";
                    SwitchState(FlowState.Welcome);
                    GameFlowManager.Instance.NotifyFirstLaunchComplete();
                    break;

                case RecoveryResult.AccountNotFound:
                    GameFlowManager.Instance.SetLoadingScreenActive(false);
                    AuthenticationManager.Instance.SignOut();
                    DialogManager.Instance.ShowErrorDialog(
                        "データが見つかりません",
                        "このGoogleアカウントに紐づくゲームデータが見つかりませんでした。",
                        "UI/Dialogs/Contents/Content_Footer_BackToTitle"
                    );
                    break;

                case RecoveryResult.Failure:
                    GameFlowManager.Instance.SetLoadingScreenActive(false);
                    AuthenticationManager.Instance.SignOut();
                    UIActionDispatcher.Instance.DispatchOpenRequest("ServerError", null);
                    break;
            }
        });
    }

    public void OnConfirmCreateDataButtonPressed() {
        _ = RequestHandler.FromUI(async () => {
            string username = usernameInputField.text.Trim();
            if (string.IsNullOrEmpty(username)) {
                Debug.LogWarning("Username is empty.");
                return;
            }
            await AuthenticationManager.Instance.SignInAnonymouslyIfNeeded();
            await RemoteConfigManager.Instance.FetchConfigsAsync();
            string initialDataJson = RemoteConfigManager.Instance.DefaultPlayerDataJson;
            await CloudSaveManager.Instance.CreateAndSaveInitialDataAsync(username, initialDataJson);
            CloudSaveManager.Instance.SetFirstLaunchCompleted();
            welcomeMessageText.text = $"{username}さん、ようこそ！";
            SwitchState(FlowState.Welcome);
            GameFlowManager.Instance.NotifyFirstLaunchComplete();
        });
    }

    public void OnBackToNewOrContinueButtonPressed() {
        SwitchState(FlowState.NewOrContinue);
    }

    public void OnStartGameButtonPressed() {
        OnCloseButtonPressed();
    }

    public void OnCloseButtonPressed() {
        DialogManager.Instance.AnimatePopupClose(this.gameObject);
    }
}