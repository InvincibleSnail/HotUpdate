using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// Addressable 最简单热更 Demo：
/// - 服务器：HotUpdateLocalServer/Addressable/version.json + catalog_vX.json + bundles
/// - 客户端：启动时对比版本，发现新版本就 LoadContentCatalogAsync(远端 catalog)，实现 Addressables 资源热更。
/// </summary>
public class SimpleAddressablesHotUpdateDemo : MonoBehaviour
{
    [SerializeField] private Button _button;
    public string versionJsonUrl = "http://localhost:8080/Addressable/version.json";

    [System.Serializable]
    private class AddrVersionInfo
    {
        public int version;
        public string catalogUrl;
    }

    private string LocalDir => Path.Combine(Application.persistentDataPath, "Addressable");
    private string LocalVersionPath => Path.Combine(LocalDir, "version.json");

    private void Start()
    {
        _button.onClick.AddListener(() => { StartCoroutine(CheckAndUpdateAddressables()); });
    }

    private IEnumerator CheckAndUpdateAddressables()
    {
        int localVersion = 0;
        try
        {
            if (File.Exists(LocalVersionPath))
            {
                var json = File.ReadAllText(LocalVersionPath);
                var info = JsonUtility.FromJson<AddrVersionInfo>(json);
                if (info != null) localVersion = info.version;
            }
        }
        catch
        {
        }

        using (UnityWebRequest req = UnityWebRequest.Get(versionJsonUrl))
        {
            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError("[AddrHotUpdate] 请求版本文件失败: " + req.error);
                yield break;
            }

            var remoteJson = req.downloadHandler.text;
            AddrVersionInfo remoteInfo = null;
            try
            {
                remoteInfo = JsonUtility.FromJson<AddrVersionInfo>(remoteJson);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[AddrHotUpdate] 解析远端版本文件失败: " + e);
                yield break;
            }

            if (remoteInfo == null || string.IsNullOrEmpty(remoteInfo.catalogUrl))
            {
                Debug.LogError("[AddrHotUpdate] 远端版本信息不完整。");
                yield break;
            }

            Debug.Log($"[AddrHotUpdate] 本地版本: {localVersion}, 远端版本: {remoteInfo.version}");

            if (remoteInfo.version <= localVersion)
            {
                Debug.Log("[AddrHotUpdate] 已是最新 Addressables 版本。");
                yield break;
            }

            Debug.Log("[AddrHotUpdate] 发现新版本，加载远端 catalog: " + remoteInfo.catalogUrl);
            AsyncOperationHandle<IResourceLocator> handle =
                Addressables.LoadContentCatalogAsync(remoteInfo.catalogUrl, true);

            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[AddrHotUpdate] 加载远端 catalog 失败: " + handle.OperationException);
                yield break;
            }

            Debug.Log("[AddrHotUpdate] 远端 catalog 加载成功，新 Addressables 资源已加入系统。");

            try
            {
                if (!Directory.Exists(LocalDir))
                    Directory.CreateDirectory(LocalDir);
                File.WriteAllText(LocalVersionPath, remoteJson);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[AddrHotUpdate] 写本地版本失败: " + e);
            }
        }
    }
}