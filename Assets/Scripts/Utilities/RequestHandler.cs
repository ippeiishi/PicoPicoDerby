// RequestHandler.cs
using System;
using System.Threading.Tasks;

public static class RequestHandler {
    /// <summary>
    /// UIからのネットワークリクエストを安全に実行するためのラッパー。
    /// ネットワークチェックとグローバルローディング制御を自動的に行う。
    /// </summary>
    /// <param name="requestFunc">実行する非同期の通信処理</param>
    public static async Task FromUI(Func<Task> requestFunc) {
        // NetworkCheckerがダイアログ表示まで行うので、その動作を尊重する
        if (!NetworkChecker.IsOnline()) { return; }
        
        GameFlowManager.Instance.SetLoadingScreenActive(true);
        
        try {
            await requestFunc();
        }
        catch (Exception e) {
            UIActionDispatcher.Instance.DispatchOpenRequest("ServerError", null);
            UnityEngine.Debug.LogError($"RequestHandler caught an exception: {e}");
        }
        finally {
            GameFlowManager.Instance.SetLoadingScreenActive(false);
        }
    }
}