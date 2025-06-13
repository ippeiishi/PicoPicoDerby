using UnityEngine;
public static class NetworkChecker {
    public static bool IsOnline() {
        if (Application.internetReachability == NetworkReachability.NotReachable) {
            GameFlowManager.Instance.SetLoadingScreenActive(true);         
            UIActionDispatcher.Instance.DispatchOpenRequest("NetworkError", null);
            GameFlowManager.Instance.SetLoadingScreenActive(false); // 実際にはダイアログ表示とほぼ同時
            return false;
        }
        return true;
    }
}