using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using System;

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
        Debug.Log("Continue button pressed. (Logic not implemented yet)");
    }

    public async void OnConfirmCreateDataButtonPressed() {
        if (!NetworkChecker.IsOnline()) { return; }
        GameFlowManager.Instance.SetLoadingScreenActive(true);

        try {
            // 1. UGSの初期化
            bool ugsSuccess = await UGSInitializationManager.Instance.InitializeUGSIfNeeded();
            if (!ugsSuccess) { throw new Exception("UGS Initialization Failed."); }

            // 2. 匿名認証
            await AuthenticationManager.Instance.SignInAnonymouslyIfNeeded();

            // 3. クラウドに初期データを作成・保存
            string username = usernameInputField.text.Trim();
            await CloudSaveManager.Instance.CreateAndSaveInitialDataAsync(username);

            // 4. 初回起動完了フラグを立てる (CloudSaveManager経由で)
            CloudSaveManager.Instance.SetFirstLaunchCompleted();

            // 5. UIを更新
            welcomeMessageText.text = $"{username}さん、ようこそ！";
            SwitchState(FlowState.Welcome);
            
            // 6. GameFlowManagerにフロー完了を通知
            GameFlowManager.Instance.NotifyFirstLaunchComplete();
        } catch (Exception e) {
            Debug.LogError($"Failed to create new user: {e}");
            UIActionDispatcher.Instance.DispatchOpenRequest("ServerError", null);
        } finally {
            GameFlowManager.Instance.SetLoadingScreenActive(false);
        }
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