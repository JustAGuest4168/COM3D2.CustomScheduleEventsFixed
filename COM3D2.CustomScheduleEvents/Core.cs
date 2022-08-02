using BepInEx;
using COM3D2.NeiAppender.Plugin;
using COM3D2API;
using Newtonsoft.Json;
using Schedule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using CM3D2.Toolkit.Guest4168Branch;

//using UnityEngine;
using System.Collections;
//using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
//using System.IO;
using System.Threading;
using System.Diagnostics;
using HarmonyLib;
using System.Reflection;
using CM3D2.Toolkit.Guest4168Branch.Arc;
using CM3D2.Toolkit.Guest4168Branch.Arc.Entry;
using CM3D2.Toolkit.Guest4168Branch.Arc.FilePointer;

namespace COM3D2.CustomScheduleEvents.Plugin
{
    [BepInPlugin("org.guest4168.plugins.customscheduleevents", "CustomScheduleEvents", "1.0.0.0")]
    public class CustomScheduleEvents : BaseUnityPlugin
    {
        private UnityEngine.GameObject managerObject;

        public void Awake()
        {
            //Copied from examples
            UnityEngine.Debug.Log("CustomScheduleEvents: Core Awake");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);

            this.managerObject = new UnityEngine.GameObject("customScheduleEventsManager");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this.managerObject);
            this.managerObject.AddComponent<Manager>().Initialize();

            SceneManager.sceneLoaded += SceneLoaded;
        }

        //Settings

        private static string modPath_;
        public static string modPath
        {
            get
            {
                if (modPath_ == null)
                {
                    modPath_ = UTY.gameProjectPath + "\\Mod\\[CustomScheduleEvents]\\";
                }

                return modPath_;
            }
        }

        private static string _configPath;
        public static string configPath
        {
            get
            {
                //Lazy init
                if (_configPath == null)
                {
                    _configPath = UTY.gameProjectPath + "\\BepinEx\\plugins\\[CustomScheduleEvents]";
                }

                //Create the folder
                if (!Directory.Exists(_configPath))
                {
                    Directory.CreateDirectory(_configPath);
                }

                //Create a .neiConfig
                if (!File.Exists(Path.Combine(_configPath, "neiAppendConfig.json")))
                {
                    File.WriteAllText(Path.Combine(_configPath, "neiAppendConfig.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string>()
                    {
                        //FACILITIES
                        { "schedule_work_facility_enabled", Path.Combine(modPath, "FAC_ENABLED") },

                        //REGULAR WORK
                        { "schedule_work_noon_enabled", Path.Combine(modPath, "NOON_ENABLED") },
                        { "schedule_work_noon.nei", Path.Combine(modPath, "NOON") },

                        //YOTOGI
                        { "schedule_work_night_category_list.nei", Path.Combine(modPath, "NIGHT_CAT") },
                        { "schedule_work_night_enabled", Path.Combine(modPath, "NIGHT_ENABLED") },
                        { "schedule_work_night.nei", Path.Combine(modPath, "NIGHT") },
                        { "schedule_work_netorare.nei", Path.Combine(modPath, "NETORARE") },

                        //YOTOGI BOOKS
                        { "schedule_work_easyyotogi.nei", Path.Combine(modPath, "EASYYOT") }

                        //TODO MORE TYPES LIKE TRAINING
                    }));
                }

                //Create the file
                if (!File.Exists(Path.Combine(_configPath, "config.json")))
                {
                    File.WriteAllText(Path.Combine(_configPath, "config.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new CustomScheduleEventConfig()
                    {
                        patches = new Dictionary<string, SimpleKSPatch>()
                        {
                            {
                                "newyotogimain.ks", new SimpleKSPatch()
                                {
                                    insertBeforeSubroutine = new Dictionary<string, SimpleKSPatchMod>() { { "*init_end", new SimpleKSPatchMod() { targetOccurance = 1, modFile = "newyotogimain_init_end.ks" } } },
                                    replacements = new Dictionary<string, SimpleKSPatchMod>() { { "*夜伽キャラクター選択終了", new SimpleKSPatchMod() { targetOccurance = 2, modFile = "newyotogimain_夜伽キャラクター選択終了.ks" } },
                                                                                                { "*夜伽ハーレムペア選択終了", new SimpleKSPatchMod() { targetOccurance = 1, modFile = "newyotogimain_夜伽ハーレムペア選択終了.ks" } }},
                                    insertAfterSubroutine = new Dictionary<string, SimpleKSPatchMod>() { }
                                }
                            },
                            {
                                "vipmain.ks", new SimpleKSPatch()
                                {
                                    insertBeforeSubroutine = new Dictionary<string, SimpleKSPatchMod>() { { "*VIPキャラクター選択終了", new SimpleKSPatchMod() { targetOccurance = 1, modFile = "vipmain_VIPキャラクター選択終了_custom.ks" } } },
                                    replacements = new Dictionary<string, SimpleKSPatchMod>() { { "*VIPキャラクター選択", new SimpleKSPatchMod() { targetOccurance = 1, modFile= "vipmain_VIPキャラクター選択.ks" } } },
                                    insertAfterSubroutine = new Dictionary<string, SimpleKSPatchMod>() { }
                                }
                            }
                        },
                        additionalFiles = new List<string>() { "vip_main_0001_custom.ks" },
                        customEvents = new Dictionary<string, Dictionary<int, string>>()
                        {
                            {
                                "vip", new Dictionary<int, string>()
                            },
                            {
                                "newyotogi", new Dictionary<int, string>()
                            },
                        }
                    }));
                }

                return _configPath;
            }
        }

        private static CustomScheduleEventConfig config_;
        public static CustomScheduleEventConfig config
        {
            get
            {
                if (config_ == null)
                {
                    config_ = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomScheduleEventConfig>(System.IO.File.ReadAllText(Path.Combine(configPath, "config.json")));

                    //_config = new CustomScheduleEventConfig();

                    //ANY .json is considered a config
                    //foreach(string configFilePlus in Directory.GetFiles(configPath, "config.json", SearchOption.AllDirectories))
                    //{
                    //    CustomScheduleEventConfig nextConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomScheduleEventConfig>(System.IO.File.ReadAllText(configFilePlus));
                    //    foreach(KeyValuePair<string, SimpleKSPatch> kvp in nextConfig.patches)
                    //    {
                    //        _config.patches[kvp.Key] = kvp.Value;
                    //    }
                    //    foreach (KeyValuePair<string, Dictionary<int, string>> kvp in nextConfig.customEvents)
                    //    {
                    //        if(!_config.customEvents.ContainsKey(kvp.Key))
                    //        {
                    //            _config.customEvents[kvp.Key] = new Dictionary<int, string>();
                    //        }
                    //        foreach (KeyValuePair<int, string> kvp2 in kvp.Value)
                    //        {
                    //            _config.customEvents[kvp.Key][kvp2.Key] = kvp2.Value;
                    //        }
                    //    }
                    //}
                }

                return config_;
            }
        }

        //KS Patch
        public static void InstallKSPatches()
        {
            SimpleKSPatcher.CreateCustomScheduleEventKS();
            SimpleKSPatcher.ApplyAllPatches("CSE_temp", config.patches, config.additionalFiles);
        }


        private static Dictionary<int, string> custom_vip_events_;
        public static Dictionary<int, string> custom_vip_events
        {
            get
            {
                if (custom_vip_events_ == null)
                {
                    if (config.customEvents.ContainsKey("vip"))
                    {
                        custom_vip_events_ = config.customEvents["vip"];
                    }
                    else if (config.customEvents.ContainsKey("VIP"))
                    {
                        custom_vip_events_ = config.customEvents["VIP"];
                    }
                    else
                    {
                        custom_vip_events_ = new Dictionary<int, string>();
                    }
                }

                return custom_vip_events_;
            }
        }
        public static void Refresh_custom_vip_events()
        {
            custom_vip_events_ = null;
        }
        private static Dictionary<int, string> custom_newyotogi_events_;
        public static Dictionary<int, string> custom_newyotogi_events
        {
            get
            {
                if (custom_newyotogi_events_ == null)
                {
                    if (config.customEvents.ContainsKey("newyotogi"))
                    {
                        custom_newyotogi_events_ = config.customEvents["newyotogi"];
                    }
                    else if (config.customEvents.ContainsKey("NEWYOTOGI"))
                    {
                        custom_newyotogi_events_ = config.customEvents["NEWYOTOGI"];
                    }
                    else
                    {
                        custom_newyotogi_events_ = new Dictionary<int, string>();
                    }
                }

                return custom_newyotogi_events_;
            }
        }
        public static void Refresh_custom_newyotogi_events()
        {
            custom_newyotogi_events_ = null;
        }

        //Custom KS Functions
        public static void IsMaidTaskCustom(TJSVariantRef[] tjs_param, TJSVariantRef result)
        {
            NDebug.Assert(3 == tjs_param.Length, "CustomScheduleEvents: IsMaidTaskCustom args count error.");

            if (result == null)
            {
                UnityEngine.Debug.Log("CustomScheduleEvents: IsMaidTaskCustom RESULT NULL");
                return;
            }

            int eventId = 0;
            Dictionary<int, string> customCollection = null;

            int maidNum = tjs_param[0].AsInteger();
            string timezoneStatus = tjs_param[1].AsString();
            string customCollectionName = tjs_param[2].AsString();

            //Get the Maid
            Maid maid = GameMain.Instance.CharacterMgr.GetMaid(maidNum);
            if (maid != null)
            {
                //Determine which event property to check
                if (timezoneStatus == "昼仕事ID")
                {
                    eventId = maid.status.noonWorkId;
                }
                else if (timezoneStatus == "夜仕事ID")
                {
                    eventId = maid.status.nightWorkId;
                }

                //Check the collection
                if (customCollectionName.Trim().ToLower() == "VIP".ToLower())
                {
                    customCollection = custom_vip_events;
                }
                else if (customCollectionName.Trim().ToLower() == "NewYotogi".ToLower())
                {
                    customCollection = custom_newyotogi_events;
                }
            }

            //Set result
            result.SetBool(maid != null && eventId != 0 && customCollection != null && customCollection.ContainsKey(eventId));
        }
        public static void GetMaidTaskCustom(TJSVariantRef[] tjs_param, TJSVariantRef result)
        {
            NDebug.Assert(3 == tjs_param.Length, "CustomScheduleEvents: GetMaidTaskCustom args count error.");

            if (result == null)
            {
                UnityEngine.Debug.Log("CustomScheduleEvents: GetMaidTaskCustom RESULT NULL");
                return;
            }

            int eventId = 0;
            Dictionary<int, string> customCollection = null;

            int maidNum = tjs_param[0].AsInteger();
            string timezoneStatus = tjs_param[1].AsString();
            string customCollectionName = tjs_param[2].AsString();

            //Get the Maid
            Maid maid = GameMain.Instance.CharacterMgr.GetMaid(maidNum);
            if (maid != null)
            {
                //Determine which event property to check
                if (timezoneStatus == "昼仕事ID")
                {
                    eventId = maid.status.noonWorkId;
                }
                else if (timezoneStatus == "夜仕事ID")
                {
                    eventId = maid.status.nightWorkId;
                }

                //Check the collection
                if (customCollectionName.Trim().ToLower() == "VIP".ToLower())
                {
                    customCollection = custom_vip_events;
                }
                else if (customCollectionName.Trim().ToLower() == "NewYotogi".ToLower())
                {
                    customCollection = custom_newyotogi_events;
                }
            }

            //Set result
            if (maid != null && eventId != 0 && customCollection != null && customCollection.ContainsKey(eventId))
            {
                result.SetString(customCollection[eventId]);
            }
            else
            {
                UnityEngine.Debug.Log("CustomScheduleEvents: GetMaidTaskCustom Failed to load Event Id:" + eventId);
            }
        }
        public static void ExecMaidTaskCustom(TJSVariantRef[] tjs_param, TJSVariantRef result)
        {
            NDebug.Assert(3 == tjs_param.Length, "CustomScheduleEvents: ExecMaidTaskCustom args count error.");

            //if (result == null)
            //{
            //    UnityEngine.Debug.Log("CustomScheduleEvents: ExecMaidTaskCustom RESULT NULL");
            //    return;
            //}

            int eventId = 0;
            Dictionary<int, string> customCollection = null;

            int maidNum = tjs_param[0].AsInteger();
            string timezoneStatus = tjs_param[1].AsString();
            string customCollectionName = tjs_param[2].AsString();

            //Get the Maid
            Maid maid = GameMain.Instance.CharacterMgr.GetMaid(maidNum);
            if (maid != null)
            {
                //Determine which event property to check
                if (timezoneStatus == "昼仕事ID")
                {
                    eventId = maid.status.noonWorkId;
                }
                else if (timezoneStatus == "夜仕事ID")
                {
                    eventId = maid.status.nightWorkId;
                }

                //Check the collection
                if (customCollectionName.Trim().ToLower() == "VIP".ToLower())
                {
                    customCollection = custom_vip_events;
                }
                else if (customCollectionName.Trim().ToLower() == "NewYotogi".ToLower())
                {
                    customCollection = custom_newyotogi_events;
                }
            }

            //Set result
            if (maid != null && eventId != 0 && customCollection != null && customCollection.ContainsKey(eventId))
            {
                //Execute the non-recollection code
                UnityEngine.Debug.Log("CustomScheduleEvents: ExecMaidTaskCustom Loading:" + eventId + " " + customCollection[eventId]);
                GameMain.Instance.ScriptMgr.LoadAdvScenarioScript(customCollection[eventId]);
                GameMain.Instance.ScriptMgr.adv_kag.Exec();
            }
            else
            {
                UnityEngine.Debug.Log("CustomScheduleEvents: ExecMaidTaskCustom Failed to load Event Id:" + eventId);
            }
        }
        public static void KSLog(TJSVariantRef[] tjs_param, TJSVariantRef result)
        {
            NDebug.Assert(1 == tjs_param.Length, "CustomScheduleEvents: IsMaidTaskCustom args count error.");

            UnityEngine.Debug.Log("CustomScheduleEvents: LOG: " + tjs_param[0].AsString());
        }


        #region Event Creator
        private bool gearAdded = false;
        private static bool _inPhotoMode = false;
        public static bool InPhotoMode
        {
            get { return _inPhotoMode; }
        }

        private static byte[] _GearIcon = null;
        public static byte[] GearIcon
        {
            get
            {
                if (_GearIcon == null)
                {
                    _GearIcon = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwAAADsABataJCQAAABl0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC4xNkRpr/UAAAKxSURBVFhH7ZfNSxtBGMYXGxMoCZ5a8eapBUU8eRAEDyX0UiEIngoiBW+KqRURRRPbUkppTWl7qZgKHgOlUHos2EOgiGA/MKWY/gPWL6iXFJI8fd/JzmZmM0swJunFB37u7vDOPM98KVoANGKWBRPuumqYxmDcddoHY+rEuOuqYRqDcddpH4ypE+Ouq4ZpDMZd51nYLC4CXARoaID7yvuy8q6iBaibCgXg8ND+UHR6ChSL1QPE43ENKa/2W3SlHYo0ztGRaDfW53KIK54VASxuPAMsNUAkV30iqmddAgwVyrPnp5RXAPU8nH8LTk4QzpRX4OZ3Gsfef2P98bHjx5zvEO7vI5VMwufzIZFICPg9tbHhnANNHIwOqOppDGC90pFSZ/Nobg47m5vC0L0t3LaztUWdd4lfNl9Lt4OketYWIBbDg4UF0RaNRisCcJuQMN4rP22pnjUFWF5aEk8WL7s7ALcJWT8IuQKELdVTD5DPA5kM7txO4uO1G/gdvIKfV68Do6PisDkrQCRolllaZq8t2NveJtNvZwywS3vW0QG0ttLdGgJ6eoDubqClBVhZsbsrUg4hLzvD76m1NeDgwC6qlOqpB5ieBvx+vB4fx5OZGTyencWbsTFgdRXo69NnI2dEKxO499nZrst30465umKMlOqpBwiFgHAYb4eH9Y7ZLNDWZg5A8j//i0sv8iJA6Okfu7WWAL29AM08MTXldHo4Pw8sLgIDA54B5OwlUqo5I6V66gH6+8WeP6OteDkxgXeRCD4NDgKBAEB3vvEB1tdLBzAYBDo7ga4uoL0dGBlxfonUQ6qnHoCVTiP+wcL7yRKTX6idryfJa0ZeK8NSxzdRGYAk/7BIpOodgKT/XyDVjAAk/vF/ApCEOWMMUE+p49sejjmjBWg0bnOmaQHcxiVg/QNk9IQG4BHfpQAAAABJRU5ErkJggg==");
                }
                return _GearIcon;
            }
        }

        public void SceneLoaded(Scene s, LoadSceneMode lsm)
        {
            //Reset
            _inPhotoMode = false;

            //Add the button
            if (GameMain.Instance != null && s != null && s.name.Equals("SceneTitle") && GameMain.Instance.SysShortcut != null && !gearAdded)
            {
                SystemShortcutAPI.AddButton("CustomScheduleEvents", OnMenuButtonClickCallback, "Custom Schedule Events", GearIcon);
                CM3D2.Toolkit.Guest4168Branch.MultiArcLoader.MultiArcLoader mal = ScriptAnalysis.mal;
                gearAdded = true;
            }

            //Special boolean for displaying info message in edit mode
            if (GameMain.Instance != null && s.name.Equals("ScenePhotoMode"))
            {
                _inPhotoMode = true;
            }
        }
        private void OnMenuButtonClickCallback()
        {
            if (_inPhotoMode)
            {
                //Launch WebUI
                PluginWebUI.Plugin.PluginWebUI.OpenPage(@"/CustomScheduleEvents/CustomScheduleEvents.html");
            }
        }

        public static void InstallUI()
        {
            System.Resources.ResourceSet resourceSet = Properties.Resources.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
            foreach (System.Collections.DictionaryEntry entry in resourceSet)
            {
                UnityEngine.Debug.Log("Install UI" + entry.Key.ToString());
                if ((new string[]{ "CustomScheduleEvents.html", "CustomScheduleEvents.js" }).Contains(entry.Key.ToString()))
                {
                    string fileName = entry.Key.ToString();
                    byte[] file = (byte[])entry.Value;

                    PluginWebUI.Plugin.PluginWebUI.AddContent("CustomScheduleEvents", fileName, file);
                }
            }

            
        }
        #endregion
    }

    public class CustomScheduleEventConfig
    {
        public string version { get; set; }
        public Dictionary<string, SimpleKSPatch> patches { get; set; }
        public List<string> additionalFiles { get; set; }
        public Dictionary<string, Dictionary<int, string>> customEvents { get; set; }
        public WebData webData { get; set; }

        public CustomScheduleEventConfig()
        {
            version = "1.0.0.0";
            patches = new Dictionary<string, SimpleKSPatch>();
            additionalFiles = new List<string>();
            customEvents = new Dictionary<string, Dictionary<int, string>>();
            webData = new WebData();
        }

        public class WebData
        {
            public Dictionary<string, Dictionary<string, Favorite>> TableFavorites;

            public WebData()
            {
                TableFavorites = new Dictionary<string, Dictionary<string, Favorite>>();
            }

            public Favorite getFavorite(string table, string rowId)
            {
                if (!TableFavorites.ContainsKey(table))
                {
                    return Favorite.Unknown;
                }

                if(!TableFavorites[table].ContainsKey(rowId))
                {
                    return Favorite.Unknown;
                }

                return TableFavorites[table][rowId];
            }
            public void addFavorite(string table, string rowId)
            {
                updateFavorite(table, rowId, Favorite.Favorite);
            }
            public void hideFavorite(string table, string rowId)
            {
                updateFavorite(table, rowId, Favorite.Hidden);
            }
            public void resetFavorite(string table, string rowId)
            {
                updateFavorite(table, rowId, Favorite.Unknown);
                TableFavorites[table].Remove(rowId);                
            }

            private void updateFavorite(string table, string rowId, Favorite favorite)
            {
                if (!TableFavorites.ContainsKey(table))
                {
                    TableFavorites[table] = new Dictionary<string, Favorite>();
                }

                TableFavorites[table][rowId] = favorite;
            }

            public enum Favorite
            {
                Unknown = 0,
                Favorite = 1,
                Hidden = 2
            }
        }
    }
}