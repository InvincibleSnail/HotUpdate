using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 最简单的资源热更新 Demo：
/// 1. 启动时先从本地 persistentDataPath 读取热更后的图片（如果有的话）
/// 2. 再去服务器（本地文件目录的 file:// 路径）拉取 version.json
/// 3. 如果发现远端版本号更大，则下载最新的 png，保存到 persistentDataPath，并立即显示
/// 
/// 使用方法：
/// - 将脚本挂在任意场景物体上（例如 Launcher 场景中的一个空物体）
/// - 在 Inspector 里：
///   - 把要显示图片的 RawImage 拖到 targetImage
///   - 把 serverRootUrl 设为形如：
///     file:///Users/user/Documents/HotUpdate/HotUpdateLocalServer/Simple/
///   - 确保最后有一个斜杠 /
/// - 在 HotUpdateLocalServer/Simple 下放置：
///   - version.json
///   - ui_bg.png
/// </summary>
public class SimpleHotUpdateDemo : MonoBehaviour
{
    [Header("服务器根 URL（以 / 结尾）")]
    [Tooltip("例如：file:///Users/user/Documents/HotUpdate/HotUpdateLocalServer/Simple/")]
    public string serverRootUrl;

    [Header("远端版本文件名")]
    public string remoteVersionFileName = "version.json";

    [Header("热更图片文件名")]
    public string hotImageFileName = "ui_bg.png";

    [Header("显示图片的 RawImage")]
    public RawImage targetImage;

    private const string LocalVersionKey = "SimpleHotUpdate_Version";

    private string LocalImagePath
    {
        get { return Path.Combine(Application.persistentDataPath, hotImageFileName); }
    }

    [System.Serializable]
    private class SimpleVersionFile
    {
        public int version = 1;
        public string[] files;
    }

    private void Start()
    {
        // 先尝试加载本地已热更的图片（如果有）
        TryLoadLocalImage();

        // 然后去检查服务器版本并拉取更新
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

                // 如果下载成功，则更新本地版本号
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
        // 可选：让 RawImage 按原图大小显示
        targetImage.SetNativeSize();
    }

    private string CombineUrl(string root, string fileName)
    {
        if (string.IsNullOrEmpty(root)) return fileName;
        if (!root.EndsWith("/")) root += "/";
        return root + fileName;
    }
}

