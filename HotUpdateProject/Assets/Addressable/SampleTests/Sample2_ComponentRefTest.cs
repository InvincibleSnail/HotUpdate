using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// ComponentReference 需要具体类型才能序列化，这里用 Transform。
/// </summary>
[System.Serializable]
public class MyTransformRef : ComponentReference<Transform> { }

/// <summary>
/// Sample 2 试玩：ComponentReference — 加载后直接得到指定组件（如 Transform）。
/// 指定一个已设为 Addressable 的 Prefab（带 Transform 即可）。
/// </summary>
public class Sample2_ComponentRefTest : MonoBehaviour
{
    public MyTransformRef prefabWithTransform;

    void Start()
    {
        if (prefabWithTransform == null || !prefabWithTransform.RuntimeKeyIsValid())
        {
            Debug.Log("[Sample2] 请指定一个带 Transform 的 Prefab（且已设为 Addressable）。");
            return;
        }

        var handle = prefabWithTransform.LoadAssetAsync();
        handle.Completed += OnLoaded;
    }

    void OnLoaded(AsyncOperationHandle<Transform> op)
    {
        if (op.Status == AsyncOperationStatus.Succeeded)
            Debug.Log("[Sample2] 加载到的组件: " + op.Result.name);
        else
            Debug.LogWarning("[Sample2] 加载失败: " + op.OperationException);
        op.Release();
    }
}
