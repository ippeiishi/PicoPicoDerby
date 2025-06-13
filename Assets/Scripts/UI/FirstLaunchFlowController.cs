using UnityEngine;
using TMPro;
using System.Threading.Tasks;

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
        if (!NetworkChecker.IsOnline()) {return;}
        GameFlowManager.Instance.SetLoadingScreenActive(true);

        bool ugsSuccess = await UGSInitializationManager.Instance.InitializeUGSIfNeeded();
        if (!ugsSuccess) {
            UIActionDispatcher.Instance.DispatchOpenRequest("ServerError", null);
            GameFlowManager.Instance.SetLoadingScreenActive(false);
            return;
        }

        string username = usernameInputField.text.Trim();
        
        GameFlowManager.Instance.CreateNewUserAndStartGame(username, () =>
        {
            welcomeMessageText.text = $"{username}さん、ようこそ！";
            SwitchState(FlowState.Welcome);
            GameFlowManager.Instance.SetLoadingScreenActive(false);
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