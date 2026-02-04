#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    
    public class UnityEnginePrimitiveTypeWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(UnityEngine.PrimitiveType), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(UnityEngine.PrimitiveType), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(UnityEngine.PrimitiveType), L, null, 7, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Sphere", UnityEngine.PrimitiveType.Sphere);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Capsule", UnityEngine.PrimitiveType.Capsule);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Cylinder", UnityEngine.PrimitiveType.Cylinder);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Cube", UnityEngine.PrimitiveType.Cube);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Plane", UnityEngine.PrimitiveType.Plane);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Quad", UnityEngine.PrimitiveType.Quad);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(UnityEngine.PrimitiveType), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushUnityEnginePrimitiveType(L, (UnityEngine.PrimitiveType)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Sphere"))
                {
                    translator.PushUnityEnginePrimitiveType(L, UnityEngine.PrimitiveType.Sphere);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Capsule"))
                {
                    translator.PushUnityEnginePrimitiveType(L, UnityEngine.PrimitiveType.Capsule);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Cylinder"))
                {
                    translator.PushUnityEnginePrimitiveType(L, UnityEngine.PrimitiveType.Cylinder);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Cube"))
                {
                    translator.PushUnityEnginePrimitiveType(L, UnityEngine.PrimitiveType.Cube);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Plane"))
                {
                    translator.PushUnityEnginePrimitiveType(L, UnityEngine.PrimitiveType.Plane);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Quad"))
                {
                    translator.PushUnityEnginePrimitiveType(L, UnityEngine.PrimitiveType.Quad);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for UnityEngine.PrimitiveType!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for UnityEngine.PrimitiveType! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
}