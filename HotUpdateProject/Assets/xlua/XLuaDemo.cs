using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XLua;

[Hotfix]
public class XLuaDemo : MonoBehaviour
{
    [SerializeField] private Button hotUpdateButton;
    [SerializeField] private Button hotFixButton;
    [SerializeField] private Button genCubeButton;
    [SerializeField] private TextMeshProUGUI text;

    [Header("服务器根 URL（以 / 结尾）")]
    public string serverRootUrl = "http://localhost:8081/";

    [Header("远端版本文件名")]
    public string remoteVersionFileName = "version.json";

    [Header("热更 Lua 相对路径（在 xlua 根下）")]
    public string hotLuaFileName = "Res/demo.lua.txt";

    [Header("编辑器回退路径（Assets 下）")]
    public string editorLocalLuaPath = "xlua/Res/demo.lua.txt";

    [Header("热修 Lua 相对路径（从服务器拉取）")]
    public string hotfixLuaFileName = "Res/hotfix_gencube.lua.txt";

    private LuaEnv luaEnv;

    private string LocalCacheDir => Path.Combine(Application.persistentDataPath, "XLua");
    private string LocalVersionPath => Path.Combine(LocalCacheDir, remoteVersionFileName);
    private string LocalLuaPath => Path.Combine(LocalCacheDir, Path.GetFileName(hotLuaFileName));

    [System.Serializable]
    private class XluaVersionFile
    {
        public int version;
        public string[] files;
    }

    private int GetLocalVersion()
    {
        try
        {
            if (!File.Exists(LocalVersionPath)) return 0;
            string json = File.ReadAllText(LocalVersionPath);
            var info = JsonUtility.FromJson<XluaVersionFile>(json);
            return info?.version ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private void SaveLocalVersion(string json)
    {
        try
        {
            if (!Directory.Exists(LocalCacheDir))
                Directory.CreateDirectory(LocalCacheDir);
            File.WriteAllText(LocalVersionPath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[XLuaDemo] 写入本地 version.json 失败: " + e);
        }
    }

    private void Start()
    {
        luaEnv = new LuaEnv();
        TryLoadAndExecuteLua();
        if (hotUpdateButton != null)
            hotUpdateButton.onClick.AddListener(OnHotUpdateButtonClick);
        if (hotFixButton != null)
            hotFixButton.onClick.AddListener(OnHotFixButtonClick);
        if (genCubeButton != null)
            genCubeButton.onClick.AddListener(GenShape);
    }

    /// <summary>
    /// 未热修时生成球体，热修后由 Lua 替换为生成立方体。
    /// </summary>
    public void GenShape()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = new Vector3(0, 2f, 0);
        go.name = "GenShape_Sphere";
        Debug.Log("[XLuaDemo] GenShape: 生成球体（未热修）");
    }

    private void OnHotFixButtonClick()
    {
        if (string.IsNullOrEmpty(serverRootUrl))
        {
            Debug.LogWarning("[XLuaDemo] serverRootUrl 未设置，无法从服务器拉取热修。");
            return;
        }
        StartCoroutine(DownloadAndExecuteHotfixLuaCoroutine());
    }

    private IEnumerator DownloadAndExecuteHotfixLuaCoroutine()
    {
        if (luaEnv == null) yield break;

        string url = CombineUrl(serverRootUrl, hotfixLuaFileName);
        Debug.Log("[XLuaDemo] 从服务器拉取热修脚本: " + url);

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError("[XLuaDemo] 下载热修脚本失败: " + req.error + "\n请确认 xlua 服务器已启动且 URL 可访问: " + url);
                if (text != null) text.text = "热修拉取失败: " + req.error;
                yield break;
            }

            string luaCode = req.downloadHandler?.text;
            if (string.IsNullOrEmpty(luaCode))
            {
                Debug.LogError("[XLuaDemo] 热修脚本内容为空。");
                yield break;
            }

            try
            {
                luaEnv.DoString(luaCode);
                if (text != null) text.text = "已执行热修（来自服务器），再点「生成」将生成 Cube";
                Debug.Log("[XLuaDemo] 热修脚本已执行（服务器）。");

                string cachePath = Path.Combine(LocalCacheDir, Path.GetFileName(hotfixLuaFileName));
                if (!Directory.Exists(LocalCacheDir)) Directory.CreateDirectory(LocalCacheDir);
                File.WriteAllText(cachePath, luaCode, Encoding.UTF8);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[XLuaDemo] 执行热修脚本失败: " + e);
                if (text != null) text.text = "热修执行异常: " + e.Message;
            }
        }
    }

    private void OnHotUpdateButtonClick()
    {
        if (string.IsNullOrEmpty(serverRootUrl))
        {
            Debug.LogWarning("[XLuaDemo] serverRootUrl 未设置，跳过热更。");
            return;
        }
        StartCoroutine(CheckAndUpdateLuaCoroutine());
    }

    /// <summary>
    /// 优先从本地缓存执行 Lua，没有则从编辑器本地路径执行。
    /// </summary>
    private void TryLoadAndExecuteLua()
    {
        if (File.Exists(LocalLuaPath))
        {
            ExecuteLuaFromPath(LocalLuaPath);
            Debug.Log("[XLuaDemo] 已从本地缓存执行 Lua: " + LocalLuaPath);
        }
        else
        {
            ExecuteLuaFromEditorLocal(editorLocalLuaPath);
        }
    }

    private void ExecuteLuaFromEditorLocal(string fileName)
    {
        string luaPath = Path.Combine(Application.dataPath, fileName);
        if (!File.Exists(luaPath))
        {
            return;
        }
        ExecuteLuaFromPath(luaPath);
    }

    private void ExecuteLuaFromPath(string luaPath)
    {
        if (luaEnv == null) return;
        try
        {
            string luaCode = File.ReadAllText(luaPath, Encoding.UTF8);
            object[] ret = luaEnv.DoString(luaCode);
            if (text != null && ret != null && ret.Length > 0 && ret[0] != null)
                text.text = ret[0].ToString();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[XLuaDemo] 执行 Lua 失败: " + e);
        }
    }

    private IEnumerator CheckAndUpdateLuaCoroutine()
    {
        string versionUrl = CombineUrl(serverRootUrl, remoteVersionFileName);
        Debug.Log("[XLuaDemo] 请求版本文件: " + versionUrl);

        using (UnityWebRequest req = UnityWebRequest.Get(versionUrl))
        {
            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError("[XLuaDemo] 请求版本文件失败: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            XluaVersionFile remoteInfo = null;
            try
            {
                remoteInfo = JsonUtility.FromJson<XluaVersionFile>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[XLuaDemo] 解析版本文件失败: " + e);
                yield break;
            }

            if (remoteInfo == null)
            {
                Debug.LogError("[XLuaDemo] 远端版本文件为空或格式错误。");
                yield break;
            }

            int localVersion = GetLocalVersion();
            int remoteVersion = remoteInfo.version;
            Debug.Log($"[XLuaDemo] 本地版本: {localVersion}, 远端版本: {remoteVersion}");

            if (remoteVersion > localVersion)
            {
                yield return StartCoroutine(DownloadAndSaveLuaCoroutine());
                SaveLocalVersion(json);
                ExecuteLuaFromPath(LocalLuaPath);
                Debug.Log("[XLuaDemo] Lua 热更完成并已执行。");
            }
            else
            {
                Debug.Log("[XLuaDemo] 当前已是最新版本。");
            }
        }
    }

    private IEnumerator DownloadAndSaveLuaCoroutine()
    {
        string fileUrl = CombineUrl(serverRootUrl, hotLuaFileName);
        Debug.Log("[XLuaDemo] 下载 Lua: " + fileUrl);

        using (UnityWebRequest req = UnityWebRequest.Get(fileUrl))
        {
            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError("[XLuaDemo] 下载 Lua 失败: " + req.error + "\n请确认 server 从 xlua 目录启动，且 URL 可访问: " + fileUrl);
                yield break;
            }

            byte[] bytes = req.downloadHandler.data;
            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("[XLuaDemo] 下载到的 Lua 数据为空。");
                yield break;
            }

            try
            {
                if (!Directory.Exists(LocalCacheDir))
                    Directory.CreateDirectory(LocalCacheDir);
                File.WriteAllBytes(LocalLuaPath, bytes);
                Debug.Log("[XLuaDemo] 已保存到: " + LocalLuaPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[XLuaDemo] 保存 Lua 失败: " + e);
            }
        }
    }

    private static string CombineUrl(string root, string fileName)
    {
        if (string.IsNullOrEmpty(root)) return fileName;
        if (!root.EndsWith("/")) root += "/";
        string path = (fileName ?? "").Replace('\\', '/').TrimStart('/');
        return root + path;
    }

    private void OnDestroy()
    {
        luaEnv?.Dispose();
    }
}
