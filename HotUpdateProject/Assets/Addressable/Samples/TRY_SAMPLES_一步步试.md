# Addressables Samples 一步步试

从第一个 sample 开始，按顺序在 Unity 里试一遍。每个 sample 下面是要做的步骤和怎么算“试过了”。

---

## 前置：确保已启用 Addressables

1. **Window → Asset Management → Addressables → Groups**
2. 若提示创建 Settings，点 **Create Addressables Settings**（会生成 `Assets/AddressableAssetsData` 等）。
3. 确认有一个 Default 或任意 Group 存在。

---

## Sample 1：Addressables Utility

**作用**：根据一个 `AssetReference` 查出它对应的 **address 字符串**。

### 步骤

1. 在场景里新建一个空物体，命名为 `Sample1_Utility`。
2. 新建脚本（可放在 `Assets/Addressable/` 下），内容如下，挂到该物体上：

```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;

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
```

3. 在 Project 里随便选一个资源（如一张图或一个 Prefab），在 Inspector 里勾选 **Addressable**，并设一个 Address（如 `TestAsset`）。
4. 把该资源拖到 `Sample1_Utility` 上脚本的 **Any Addressable Ref** 栏。
5. 运行场景，看 Console 是否打出：`[Sample1] 该 AssetReference 的 address: TestAsset`（或你设的 address）。

**试过了**：Console 里能看到正确的 address。

---

## Sample 2：ComponentReference

**作用**：做一个“只能引用带某组件 Prefab”的 `AssetReference`，加载后直接得到该组件。

### 步骤

1. 新建空物体，命名为 `Sample2_ComponentRef`。
2. 新建脚本（需继承 `ComponentReference<T>` 的具体类型，例如）：

```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class MyTransformRef : ComponentReference<Transform> { }

public class Sample2_ComponentRefTest : MonoBehaviour
{
    public MyTransformRef prefabWithTransform;

    async void Start()
    {
        if (prefabWithTransform == null || !prefabWithTransform.RuntimeKeyIsValid())
        {
            Debug.Log("[Sample2] 请指定一个带 Transform 的 Prefab（且已设为 Addressable）。");
            return;
        }
        var handle = prefabWithTransform.LoadAssetAsync();
        await handle.Task;
        Debug.Log("[Sample2] 加载到的组件: " + (handle.Result != null ? handle.Result.name : "null"));
        handle.Release();
    }
}
```

3. 创建一个简单 Prefab（例如 Cube），勾选 Addressable，设好 Address。
4. 把该 Prefab 拖到 `Sample2_ComponentRefTest` 的 **Prefab With Transform**。
5. 运行，Console 应看到加载到的 Transform（或组件名）。

**试过了**：能加载并打印出组件，且 Inspector 里若拖入没有 Transform 的资产会报错/不通过校验。

---

## Sample 3：Prefab Spawner

**作用**：用 `AssetReference` 定时实例化、过一段时间再释放（销毁）。

### 步骤

1. 在场景里新建空物体，命名为 `Sample3_Spawner`。
2. 将 **PrefabSpawnerSample** 脚本挂上去（在 `Samples/Addressables/1.22.3/Prefab Spawner/PrefabSpawnerSample.cs`）。
3. 在 Project 里创建一个 Prefab（例如 Cube），设为 Addressable，Address 可填 `Cube`（或任意）。
4. 把该 Prefab 拖到 **Spawnable Prefab**。
5. 运行场景：应每隔约 2 秒生成 Prefab，约 1 秒后消失；可调 **Delay Between Spawns**、**Dealy Before Destroying**、**Number Of Prefabs To Spawn** 观察。

**试过了**：能看到周期性地生成和消失。

---

## Sample 4：Custom Analyze Rules

**作用**：在 Addressables 的 Analyze 窗口里跑自定义检查/修复规则。

### 步骤

1. **Window → Asset Management → Addressables → Groups** 打开 Groups 窗口。
2. 点 **Analyze** 下拉，里面应出现三条自定义规则：
   - **Ensure all addresses have a 'C'**
   - **Addresses that are paths, match path**
   - **Check Duplicate Bundle Dependencies Multi-Isolated Groups**
3. 选 **Ensure all addresses have a 'C'** → **Refresh Analysis**：若有 address 里没有大写字母 C，会列在下面。
4. 选 **Addresses that are paths, match path** → **Refresh Analysis**，若有“像路径但和真实路径不一致”的会列出；可点 **Fix Selected** 修复。
5. 第三个规则是检查重复依赖并可按组隔离，有重复时会列出。

**试过了**：Analyze 里能跑这三条规则并看到结果（有无问题都算试过）。

---

## Sample 5：Custom Build and Playmode Scripts

**作用**：自定义“只打当前场景”的 Build，以及自定义 Play Mode 使用已有构建。

### 步骤

1. **把 Custom 脚本加入 Settings**  
   - 打开 **Addressables → Groups**，在窗口里点 **Addressable Asset Settings**（或从 Group 的齿轮进）。  
   - 在 Inspector 里找到 **Build and Play Mode Scripts** 列表。  
   - 把 `CustomBuild.asset` 和 `CustomPlayMode.asset` 拖进去（在 `Samples/.../Custom Build and Playmode Scripts/Editor/` 下）。若没有，可在 Project 里搜 `CustomBuild` / `CustomPlayMode`。

2. **试 Custom Play Mode**  
   - 先 **Build → Build Player Content** 用默认 Build Script 打一次（确保有 content）。  
   - 在 **Play Mode Script** 里选 **Use Custom Build (requires built groups)**。  
   - 再点 Play：应能进 Play 且使用已构建的 data（若之前没 build 会报错，按提示先 build 再试）。

3. **试 Custom Build（只打当前场景）**  
   - 在 **Build → New Build** 里选 **Custom Build Script**。  
   - 打开你要打的场景为当前场景，执行该 Build。  
   - 会生成“只含当前场景”的构建，并有一个 bootstrap 场景逻辑（见 README）。

**试过了**：Play Mode 能选 Custom 并正常进 Play；Build 菜单里能选 Custom Build 并成功打一包。

---

## Sample 6：Disable AssetImport on Build

**作用**：打 Player 时暂时不触发资源导入，加快构建。

### 步骤

1. 菜单里点 **Build → Disabled Importer Build**（脚本里用 `[MenuItem("Build/Disabled Importer Build")]` 注册的）。
2. 会按当前平台打一份 Player，输出在 `DisabledImporterBuildPath/[Platform]/` 下。
3. 构建过程中不会触发 AssetDatabase 的导入（适合已有 Addressables 等已准备好、只想快速打包时用）。

**试过了**：能点菜单并成功打出一版即可（不一定要跑 exe）。

---

## Sample 7：Import Groups Tool

**作用**：从别的路径把现成的 Group/Schema 导入到当前项目的 Addressables 里。

### 步骤

1. 打开 **Window → Asset Management → Addressables → Import Groups**。
2. **Import Groups**：在 **Group Path** 填一个已有的 `.asset` 路径（例如别的项目里的 `Assets/AddressableAssetsData/Group/xxx.asset`），点 **Import Groups**，会把该 Group 拷贝到当前工程的默认 Group 目录。
3. **Import Schemas**：在 **Group Name** 填要加 schema 的 Group 名，**Schema Folder** 填放 schema `.asset` 的文件夹，点 **Import Schemas**。
4. 若当前项目还没有从别处拷来的 group，可先随便创建一个 group 导出/拷贝一份再试导入；或只试 UI 能打开、不报错即可。

**试过了**：能打开 Import Groups 窗口，并至少试过一次 Import Groups 或 Import Schemas（有合法路径会多一个 group 或 schema）。

---

## 建议顺序与时间

| 顺序 | Sample              | 类型   | 建议 |
|------|---------------------|--------|------|
| 1    | Addressables Utility | 运行时 | 先做 1 个 Addressable 资源 + 小测试脚本 |
| 2    | ComponentReference   | 运行时 | 用带组件的 Prefab 试 |
| 3    | Prefab Spawner      | 运行时 | 直接挂 PrefabSpawnerSample，配一个 Prefab |
| 4    | Custom Analyze Rules| 编辑器 | 只跑 Analyze 即可 |
| 5    | Custom Build/Playmode| 编辑器 | 先 Build 再切 Play Mode 试 |
| 6    | Disable AssetImport  | 编辑器 | 跑一次菜单 Build |
| 7    | Import Groups Tool   | 编辑器 | 打开窗口，有资源就试导入 |

按上面从 Sample 1 做到 7，每个都“试过了”即算完成一轮。
