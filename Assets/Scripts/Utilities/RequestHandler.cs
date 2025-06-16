// RequestHandler.cs (新規作成)
using System;
using System.Threading.Tasks;

public static class RequestHandler {
    /// <summary>
    /// UIからのネットワークリクエストを安全に実行するためのラッパー。
    /// ネットワークチェックとグローバルローディング制御を自動的に行う。
    /// </summary>
    /// <param name="requestFunc">実行する非同期の通信処理</param>
    public static async Task FromUI(Func<Task> requestFunc) {
        if (!NetworkChecker.IsOnline()) { return; }
        
        GameFlowManager.Instance.SetLoadingScreenActive(true);
        
        try {
            await requestFunc();
        }
        catch (Exception e) {
            // ここで共通のエラー処理を行うことも可能
            // 例: UIActionDispatcher.Instance.DispatchOpenRequest("ServerError", null);
            UnityEngine.Debug.LogError($"RequestHandler caught an exception: {e}");
        }
        finally {
            GameFlowManager.Instance.SetLoadingScreenActive(false);
        }
    }
}