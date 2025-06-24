using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }

    [Header("Global UI Elements")]
    [SerializeField] private GameObject _header;

    [Header("Footers")]
    [SerializeField] private GameObject _footerLobby;
    [SerializeField] private GameObject _footerProduction; // 将来用
    [SerializeField] private GameObject _footerTraining;   // 将来用
    // 他のフッターも必要に応じて追加

    private GameObject _currentFooter;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void ShowHeader(bool show) {
        if (_header != null) {
            _header.SetActive(show);
        }
    }

    public void ShowFooter(GameObject footerToShow) {
        // 現在表示中のフッターがあれば非表示にする
        if (_currentFooter != null && _currentFooter != footerToShow) {
            _currentFooter.SetActive(false);
        }

        // 新しいフッターを表示
        if (footerToShow != null) {
            footerToShow.SetActive(true);
            _currentFooter = footerToShow;
        } else {
            // nullが渡された場合は、すべてのフッターを非表示にする
            HideAllFooters();
        }
    }

    public void ShowLobbyFooter() {
        ShowFooter(_footerLobby);
    }

    public void ShowProductionFooter() {
        ShowFooter(_footerProduction);
    }

    public void ShowTrainingFooter() {
        ShowFooter(_footerTraining);
    }

    public void HideAllFooters() {
        if (_currentFooter != null) {
            _currentFooter.SetActive(false);
        }
        // 念のため、管理外のフッターも非表示にする場合
        if (_footerLobby != null) _footerLobby.SetActive(false);
        if (_footerProduction != null) _footerProduction.SetActive(false);
        if (_footerTraining != null) _footerTraining.SetActive(false);
        
        _currentFooter = null;
    }
}