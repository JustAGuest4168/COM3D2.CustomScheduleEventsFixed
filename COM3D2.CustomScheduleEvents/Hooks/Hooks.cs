using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.CustomScheduleEvents.Plugin
{
    internal static class Hooks
    {
        public static bool initialized { get; set; }
        private static HarmonyLib.Harmony instance;

        public static void Initialize()
        {
            //Copied from examples
            if (Hooks.initialized)
                return;

            Hooks.instance = Harmony.CreateAndPatchAll(typeof(Hooks), "org.guest4168.customscheduleevents.hooks.base");
            Hooks.initialized = true;

            UnityEngine.Debug.Log("CustomScheduleEvents: Hooks Initialize");
        }

        [HarmonyPatch(typeof(GameUty), nameof(GameUty.Init))]
        [HarmonyPostfix()]
        public static void GameUty_Init_Postfix()
        {
            CustomScheduleEvents.InstallKSPatches();
            CustomScheduleEvents.InstallUI();
        }

        [HarmonyPatch(typeof(TJSScript), nameof(TJSScript.Create), new Type[] { typeof(AFileSystemBase) })]
        [HarmonyPostfix()]
        public static void TJSScript_Create_Post(AFileSystemBase file_system, TJSScript __result)
        {
            __result.AddFunction("IsMaidTaskCustom", new TJSScript.FunctionCallBack(CustomScheduleEvents.IsMaidTaskCustom));
            __result.AddFunction("GetMaidTaskCustom", new TJSScript.FunctionCallBack(CustomScheduleEvents.GetMaidTaskCustom));
            __result.AddFunction("ExecMaidTaskCustom", new TJSScript.FunctionCallBack(CustomScheduleEvents.ExecMaidTaskCustom));
            __result.AddFunction("KSLog", new TJSScript.FunctionCallBack(CustomScheduleEvents.KSLog));
        }
    }
}