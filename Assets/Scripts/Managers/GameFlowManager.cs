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
    [SerializeField] private GameObject accountSettingsDialog;
    [SerializeField] private GameObject lobbyScreen;

    // ★ Header UIセクションは不要になったため削除

    [Header("Header UI References (for data update)")] // ★役割を明確化
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI gemText;

    private bool _isConflictCheckInProgress = false;

    void Awake() {
        Instance = this;
    }

    void Start() {
        // ★ UIManagerを呼び出してUIの初期状態を設定
        UIManager.Instance.ShowHeader(false);
        UIManager.Instance.HideAllFooters();

        titleScreen.SetActive(true);
        loadingScreen.SetActive(false);
        lobbyScreen.SetActive(false);

        UIActionDispatcher.Instance.OnRequestGameFlowAction += HandleGameFlowAction;
        UIActionDispatcher.Instance.OnRequestSystemAction += HandleSystemAction;
    }

    void OnDestroy() {
        if (UIActionDispatcher.Instance != null) {
            UIActionDispatcher.Instance.OnRequestGameFlowAction -= HandleGameFlowAction;
            UIActionDispatcher.Instance.OnRequestSystemAction -= HandleSystemAction;
        }
    }

    private void OnApplicationQuit() {
        if (AuthenticationManager.Instance.IsLoggedIn) {
            _ = CheckConflictAndSaveOnQuit();
        }
    }

    private async Task CheckConflictAndSaveOnQuit() {
        if (_isConflictCheckInProgress) { return; }
        _isConflictCheckInProgress = true;

        try {
            bool conflict = await DataManager.Instance.CheckForDeviceConflictAsync();
            if (!conflict) {
                await DataManager.Instance.SaveOwnerDataAsync();
                Debug.Log("OnApplicationQuit: Save process initiated and completed.");
            } else {
                Debug.LogWarning("OnApplicationQuit: Device conflict detected. Data will not be saved.");
            }
        } catch (Exception e) {
            Debug.LogError($"OnApplicationQuit: Error during pre-save conflict check: {e.Message}");
        } finally {
            _isConflictCheckInProgress = false;
        }
    }

    private void HandleGameFlowAction(string actionName) {
        if (actionName == "StartFlow") {
            _ = OnTitleScreenPressed();
        }
    }

    private void HandleSystemAction(string actionName) {
        if (actionName == "ReloadScene") {
            ReloadCurrentScene();
        } else if (actionName == "SaveDataToCloud") {
            _ = ExecuteManualSave();
        } else if (actionName == "OpenAccountSettings") {
            DialogManager.Instance.AnimatePopupOpen(accountSettingsDialog);
        }
    }

    public Task OnTitleScreenPressed() {
        return RequestHandler.FromUI(async () => {
            bool ugsSuccess = await UGSInitializationManager.Instance.InitializeUGSIfNeeded();
            if (!ugsSuccess) { throw new Exception("UGS Initialization Failed."); }
            
            if (DataManager.Instance.HasCompletedFirstLaunch()) {
                 await AuthenticationManager.Instance.SignInAnonymouslyIfNeeded();
                 
                bool conflict = await DataManager.Instance.CheckForDeviceConflictAsync();
                if (conflict) {
                    UIActionDispatcher.Instance.DispatchOpenRequest("DeviceConflictError", null);
                    return;
                }

                await RemoteConfigManager.Instance.FetchConfigsAsync();
                bool loadSuccess = await DataManager.Instance.LoadOwnerDataAsync();
                if (loadSuccess) {
                    // ★ UIManagerを呼び出してUIを表示
                    UIManager.Instance.ShowHeader(true);
                    UIManager.Instance.ShowLobbyFooter();

                    titleScreen.SetActive(false);
                    lobbyScreen.SetActive(true);
                    UpdateHeaderUI();
                } else {
                    UIActionDispatcher.Instance.DispatchOpenRequest("DataNotFoundError", null);
                }
            } else {
                DialogManager.Instance.AnimatePopupOpen(firstLaunchDialog);
            }
        });
    }
 
    public void NotifyFirstLaunchComplete() {
        // ★ UIManagerを呼び出してUIを表示
        UIManager.Instance.ShowHeader(true);
        UIManager.Instance.ShowLobbyFooter();

        titleScreen.SetActive(false);
        lobbyScreen.SetActive(true);
        UpdateHeaderUI();
    }

    private void ReloadCurrentScene() {
        Debug.Log("Reloading current scene!!!!!!!!!!!!!!!!!!!!!!!!!!");
        AuthenticationManager.Instance.SignOut();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void SetLoadingScreenActive(bool isActive) {
        loadingScreen.SetActive(isActive);
    }

    private void UpdateHeaderUI() {
        if (DataManager.Instance.OwnerData == null) { return; }
        moneyText.text = DataManager.Instance.Money.ToString();
        gemText.text = DataManager.Instance.Gem.ToString();
    }

    private Task ExecuteManualSave() {
        return RequestHandler.FromUI(async () => {
            bool conflict = await DataManager.Instance.CheckForDeviceConflictAsync();
            if (conflict) {
                UIActionDispatcher.Instance.DispatchOpenRequest("DeviceConflictError", null);
                return;
            }

            await DataManager.Instance.SaveOwnerDataAsync();
            string okFooterPath = "UI/Dialogs/Contents/Content_Footer_OK";
            DialogManager.Instance.ShowErrorDialog("セーブ完了", "ゲームデータをクラウドに保存しました。", okFooterPath);
        });
    }
}