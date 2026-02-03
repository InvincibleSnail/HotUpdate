using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Sample 1 试玩：Addressables Utility — 根据 AssetReference 打印其 address。
/// 挂到空物体上，在 Inspector 里指定一个已设为 Addressable 的资源即可。
/// </summary>
public class Sample1_AddressablesUtilityTest : MonoBehaviour
{
    public AssetReference anyAddressableRef;

    void Start()
    {
        if (anyAddressableRef == null || !anyAddressableRef.RuntimeKeyIsValid())
        {
            Debug.Log("[Sample1] 请给 anyAddressableRef 指定一个 Addressable 资源。");
            return;
        }

        string address = AddressablesUtility.GetAddressFromAssetReference(anyAddressableRef);
        Debug.Log("[Sample1] 该 AssetReference 的 address: " + address);
    }
}
