using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using TMPro;

public class GameFlowManager : MonoBehaviour {
    public static GameFlowManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject firstLaunchDialog;
    
    [Header("Header UI")]
    [SerializeField] private GameObject header;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI gemText;

    void Awake() {
        Instance = this;
        titleScreen?.SetActive(true);
        loadingScreen?.SetActive(false);
        header?.SetActive(false);
    }

    void Start() {
        UIActionDispatcher.Instance.OnRequestGameFlowAction += HandleGameFlowAction;
        UIActionDispatcher.Instance.OnRequestSystemAction += HandleSystemAction;
    }

    void OnDestroy() {
        UIActionDispatcher.Instance.OnRequestGameFlowAction -= HandleGameFlowAction;
        UIActionDispatcher.Instance.OnRequestSystemAction -= HandleSystemAction;
    }

    private void OnApplicationQuit() {
        if (AuthenticationManager.Instance.IsLoggedIn) {
            _ = CloudSaveManager.Instance.SaveDataToCloudAsync();
            Debug.Log("OnApplicationQuit: Save process initiated.");
        }
    }

    private void HandleGameFlowAction(string actionName) {
        if (actionName == "StartFlow") {
            OnTitleScreenPressed();
        }
    }

    private void HandleSystemAction(string actionName) {
        if (actionName == "ReloadScene") {
            ReloadCurrentScene();
        } else if (actionName == "SaveDataToCloud") {
            ExecuteManualSave();
        }
    }

    public async void OnTitleScreenPressed() {
        if (!NetworkChecker.IsOnline()) { return; }
        SetLoadingScreenActive(true);

        try {
            bool ugsSuccess = await UGSInitializationManager.Instance.InitializeUGSIfNeeded();
            if (!ugsSuccess) { throw new Exception("UGS Initialization Failed."); }
            
            await AuthenticationManager.Instance.SignInAnonymouslyIfNeeded();

            if (CloudSaveManager.Instance.HasCompletedFirstLaunch()) {
                bool conflict = await CloudSaveManager.Instance.CheckForDeviceConflictAsync();
                if (conflict) {
                    UIActionDispatcher.Instance.DispatchOpenRequest("DeviceConflictError", null);
                    return;
                }

                bool loadSuccess = await CloudSaveManager.Instance.LoadDataFromCloudAsync();
                if (loadSuccess) {
                    titleScreen?.SetActive(false);
                    header?.SetActive(true);
                    UpdateHeaderUI();
                } else {
                    UIActionDispatcher.Instance.DispatchOpenRequest("DataNotFoundError", null);
                }
            } else {
                DialogManager.Instance.AnimatePopupOpen(firstLaunchDialog);
            }
        } catch (Exception e) {
            Debug.LogError($"Start Game Flow Error: {e}");
            UIActionDispatcher.Instance.DispatchOpenRequest("ServerError", null);
        } finally {
            SetLoadingScreenActive(false);
        }
    }
 
    public void NotifyFirstLaunchComplete() {
        titleScreen?.SetActive(false);
        header?.SetActive(true);
        UpdateHeaderUI();
    }

    private void ReloadCurrentScene() {
        AuthenticationManager.Instance.SignOut();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void SetLoadingScreenActive(bool isActive) {
        loadingScreen?.SetActive(isActive);
    }

    private void UpdateHeaderUI() {
        if (CloudSaveManager.Instance.CurrentPlayerData == null) { return; }
        PlayerData data = CloudSaveManager.Instance.CurrentPlayerData;
        moneyText.text = data.money.ToString();
        gemText.text = data.gem.ToString();
    }

    private async void ExecuteManualSave() {
        if (!NetworkChecker.IsOnline()) { return; }
        SetLoadingScreenActive(true);

        await CloudSaveManager.Instance.SaveDataToCloudAsync();

        SetLoadingScreenActive(false);
        
        string okFooterPath = "UI/Dialogs/Contents/Content_Footer_OK";
        DialogManager.Instance.ShowErrorDialog("セーブ完了", "ゲームデータをクラウドに保存しました。", okFooterPath);
    }
}