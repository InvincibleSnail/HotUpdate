using System.Collections.Generic;
using UnityEngine;
using XLua;

/// <summary>
/// XLua 生成与热修配置。在 Unity 菜单执行「XLua/Generate Code」和「XLua/Hotfix Inject In Editor」后生效。
/// </summary>
public static class XLuaGenConfig
{
    [LuaCallCSharp]
    public static List<System.Type> LuaCallCSharp = new List<System.Type>
    {
        typeof(XLuaDemo),
        typeof(GameObject),
        typeof(Transform),
        typeof(PrimitiveType),
        typeof(Vector3),
    };

    [Hotfix]
    public static List<System.Type> Hotfix = new List<System.Type>
    {
        typeof(XLuaDemo),
    };
}
