using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject firstLaunchDialog;

    void Awake()
    {
        Instance = this;
        titleScreen?.SetActive(true);
        loadingScreen?.SetActive(false);
    }

    void Start()
    {
        UIActionDispatcher.Instance.OnRequestGameFlowAction += HandleGameFlowAction;
        UIActionDispatcher.Instance.OnRequestSystemAction += HandleSystemAction;
    }

    void OnDestroy()
    {
        UIActionDispatcher.Instance.OnRequestGameFlowAction -= HandleGameFlowAction;
        UIActionDispatcher.Instance.OnRequestSystemAction -= HandleSystemAction;
    }

    private void HandleGameFlowAction(string actionName)
    {
        if (actionName == "StartFlow")
        {
            OnTitleScreenPressed();
        }
    }

    private void HandleSystemAction(string actionName)
    {
        if (actionName == "ReloadScene")
        {
            ReloadCurrentScene();
        }
    }

    public void OnTitleScreenPressed()
    {
        if (!NetworkChecker.IsOnline()) { return; }
        // ★★★ ここの判定メソッドを変更 ★★★
        if (LocalSaveManager.Instance.HasCompletedFirstLaunch())
        {
            LocalSaveManager.Instance.LoadData();
            titleScreen?.SetActive(false);
            // TODO: UIManager.Instance.DisplayHeader(...);
        }
        else
        {
            DialogManager.Instance.AnimatePopupOpen(firstLaunchDialog);
        }
    }

    /// <summary>
    /// FirstLaunchFlowControllerからの依頼を受け、データ作成とゲーム状態の遷移を行う。
    /// </summary>
    /// <param name="username">作成するユーザー名</param>
    /// <param name="onComplete">全ての処理が完了した後に実行されるコールバック</param>
    public void CreateNewUserAndStartGame(string username, Action onComplete)
    {
        // 1. データを新規作成してローカルに保存する
        LocalSaveManager.Instance.CreateNewData(username);

        // 2. ゲームの状態を遷移させる（タイトル画面を非表示に）
        titleScreen?.SetActive(false);

        // 3. 呼び出し元に処理の完了を通知する
        onComplete?.Invoke();
    }

    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
      public void SetLoadingScreenActive(bool isActive) {
        loadingScreen?.SetActive(isActive);
    }
}