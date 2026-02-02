using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SimpleHotUpdateDemo : MonoBehaviour
{
    [Header("服务器根 URL（以 / 结尾）")] public string serverRootUrl;

    [Header("远端版本文件名")] public string remoteVersionFileName = "version.json";

    [Header("热更图片相对路径（在 Simple 根下）")] public string hotImageFileName = "Res/ui_bg.png";

    [Header("显示图片的 RawImage")] public RawImage targetImage;

    private const string LocalVersionKey = "SimpleHotUpdate_Version";

    private string LocalImagePath
    {
        get { return Path.Combine(Application.dataPath, "Simple/Res", hotImageFileName); }
    }

    [System.Serializable]
    private class SimpleVersionFile
    {
        public int version = 1;
        public string[] files;
    }

    private void Start()
    {
        // 编辑器默认：工程外层 HotUpdateLocalServer/Simple/；用 HTTP 时在 Inspector 填 http://localhost:8080/
        if (string.IsNullOrEmpty(serverRootUrl))
            serverRootUrl = Path.Combine(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName, "HotUpdateLocalServer", "Simple") + Path.DirectorySeparatorChar;
        TryLoadLocalImage();
        if (!string.IsNullOrEmpty(serverRootUrl))
        {
            StartCoroutine(CheckAndUpdateCoroutine());
        }
        else
        {
            Debug.LogWarning("[SimpleHotUpdateDemo] serverRootUrl 未设置，跳过远端检查。");
        }
    }

    private void TryLoadLocalImage()
    {
        try
        {
            if (File.Exists(LocalImagePath))
            {
                byte[] bytes = File.ReadAllBytes(LocalImagePath);
                if (bytes != null && bytes.Length > 0)
                {
                    var tex = new Texture2D(2, 2);
                    if (tex.LoadImage(bytes))
                    {
                        ApplyTexture(tex);
                        Debug.Log("[SimpleHotUpdateDemo] 已从本地缓存加载图片: " + LocalImagePath);
                    }
                    else
                    {
                        Debug.LogWarning("[SimpleHotUpdateDemo] 本地图片 LoadImage 失败。");
                    }
                }
            }
            else
            {
                Debug.Log("[SimpleHotUpdateDemo] 本地还没有缓存图片，首次运行或未更新过。");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SimpleHotUpdateDemo] 加载本地图片异常: " + e);
        }
    }

    private IEnumerator CheckAndUpdateCoroutine()
    {
        string versionUrl = CombineUrl(serverRootUrl, remoteVersionFileName);
        Debug.Log("[SimpleHotUpdateDemo] 开始请求版本文件: " + versionUrl);

        using (UnityWebRequest req = UnityWebRequest.Get(versionUrl))
        {
            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError("[SimpleHotUpdateDemo] 请求版本文件失败: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            SimpleVersionFile remoteInfo = null;
            try
            {
                remoteInfo = JsonUtility.FromJson<SimpleVersionFile>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[SimpleHotUpdateDemo] 解析版本文件失败: " + e + "\n内容: " + json);
                yield break;
            }

            if (remoteInfo == null)
            {
                Debug.LogError("[SimpleHotUpdateDemo] 远端版本文件为空或格式不对。");
                yield break;
            }

            int localVersion = PlayerPrefs.GetInt(LocalVersionKey, 0);
            int remoteVersion = remoteInfo.version;

            Debug.Log($"[SimpleHotUpdateDemo] 本地版本: {localVersion}, 远端版本: {remoteVersion}");

            if (remoteVersion > localVersion)
            {
                Debug.Log("[SimpleHotUpdateDemo] 发现新版本，开始下载图片。");
                yield return StartCoroutine(DownloadAndApplyImage());
                
                PlayerPrefs.SetInt(LocalVersionKey, remoteVersion);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log("[SimpleHotUpdateDemo] 当前已是最新版本，无需更新。");
            }
        }
    }

    private IEnumerator DownloadAndApplyImage()
    {
        string fileUrl = CombineUrl(serverRootUrl, hotImageFileName);
        Debug.Log("[SimpleHotUpdateDemo] 下载图片: " + fileUrl);

        using (UnityWebRequest req = UnityWebRequest.Get(fileUrl))
        {
            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError("[SimpleHotUpdateDemo] 下载图片失败: " + req.error);
                yield break;
            }

            byte[] bytes = req.downloadHandler.data;
            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("[SimpleHotUpdateDemo] 下载到的图片数据为空。");
                yield break;
            }

            // 保存到本地
            try
            {
                string dir = Path.GetDirectoryName(LocalImagePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllBytes(LocalImagePath, bytes);
                Debug.Log("[SimpleHotUpdateDemo] 已将图片保存到本地: " + LocalImagePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[SimpleHotUpdateDemo] 保存图片到本地失败: " + e);
            }

            // 立即加载并应用
            var tex = new Texture2D(2, 2);
            if (tex.LoadImage(bytes))
            {
                ApplyTexture(tex);
                Debug.Log("[SimpleHotUpdateDemo] 已应用最新图片。");
            }
            else
            {
                Debug.LogError("[SimpleHotUpdateDemo] LoadImage 失败。");
            }
        }
    }

    private void ApplyTexture(Texture2D tex)
    {
        if (targetImage == null)
        {
            Debug.LogWarning("[SimpleHotUpdateDemo] targetImage 未设置，无法显示图片。");
            return;
        }

        targetImage.texture = tex;
        targetImage.SetNativeSize();
    }

    private string CombineUrl(string root, string fileName)
    {
        if (string.IsNullOrEmpty(root)) return fileName;
        if (!root.EndsWith("/")) root += "/";
        return root + fileName;
    }
}