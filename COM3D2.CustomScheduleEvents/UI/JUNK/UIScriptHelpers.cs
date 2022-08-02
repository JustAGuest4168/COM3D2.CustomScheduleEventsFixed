//using COM3D2.CustomScheduleEvents.Plugin;
//using COM3D2.CustomScheduleEvents.SAL;
//using Newtonsoft.Json;
//using Schedule;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using static COM3D2.CustomScheduleEvents.Plugin.CustomScheduleEvents;
//using static GUIDropDown;
//using static GUIPages;

//public class UIScriptHelpers
//{
//    //UI
//    public static IMGuestGUIHelper Main(int parentId)
//    {
//        Console.WriteLine("UI SCRIPT HELPERS");

//        GUIGroup layout = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, parentId);
//        {
//            //Tabs
//            GUIPagesTabs tabs = new GUIPagesTabs(IMGuestGUIHelper.NewId(), null, new string[] { "Camera", "Animation", "Face", "Dialog" }, layout.id);
//            layout.AddContent(tabs);
//            {
//                //Camera
//                GUIGroup groupCamera = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, tabs.id);
//                tabs.AddPageContent("Camera", groupCamera);
//                {

//                }

//                //Animation
//                GUIGroup groupAnimation = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, tabs.id);
//                tabs.AddPageContent("Animation", groupAnimation);
//                {
//                    //Target
//                    GUIDropDownDynamic dropdownMaid = new GUIDropDownDynamic("DropdownMaid", "Target", "", false, getAnimationTargetData, getAnimationTargetImagesData, 90, 90, 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, groupAnimation.id );
//                    groupAnimation.AddContent(dropdownMaid);

//                    //Animations
//                    GUITable tableAnimation = new GUITable("TableAnimation", "Animations", new List<string>() { "ID", "Name" }, getAnimationsData, IMGuestGUIHelperContained.HelperContainedMode.enabled, selectAnimation, groupAnimation.id);
//                    groupAnimation.AddContent(tableAnimation);
//                }

//                //Face
//                GUIGroup groupFace = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, tabs.id);
//                tabs.AddPageContent("Face", groupFace);
//                {

//                }

//                //Dialouge
//                GUIGroup groupDialog = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, tabs.id);
//                tabs.AddPageContent("Dialog", groupDialog);
//                {

//                }
//            }
//        }

//        Console.WriteLine("UI SCRIPT HELPERS 2");
//        return layout;
//    }

//    //Data
//    public static string[] getAnimationTargetData()
//    {
//        List<string> data = new List<string>();

//        //Loop available maids
//        for (int i = 0; i < GameMain.Instance.CharacterMgr.GetMaidCount(); i++)
//        {
//            Maid nextMaid = GameMain.Instance.CharacterMgr.GetMaid(i);
//            if (nextMaid != null)
//            {
//                data.Add(nextMaid.name + "||" + nextMaid.boMAN + "||" + nextMaid.status.guid);
//            }
//        }
//        for (int i = 0; i < GameMain.Instance.CharacterMgr.GetManCount(); i++)
//        {
//            Maid nextMaid = GameMain.Instance.CharacterMgr.GetMan(i);
//            if (nextMaid != null && nextMaid.isActiveAndEnabled)
//            {
//                data.Add(nextMaid.name + "||" + nextMaid.boMAN + "||" + nextMaid.status.guid);
//            }
//        }

//        return data.ToArray();
//    }
//    public static Texture2D[] getAnimationTargetImagesData()
//    {
//        List<Texture2D> data = new List<Texture2D>();
//        //Loop available maids
//        for (int i = 0; i < GameMain.Instance.CharacterMgr.GetMaidCount(); i++)
//        {
//            Maid nextMaid = GameMain.Instance.CharacterMgr.GetMaid(i);
//            if (nextMaid != null)
//            {
//                data.Add(nextMaid.GetThumIcon());
//            }
//        }
//        for (int i = 0; i < GameMain.Instance.CharacterMgr.GetManCount(); i++)
//        {
//            Maid nextMaid = GameMain.Instance.CharacterMgr.GetMan(i);
//            if (nextMaid != null && nextMaid.isActiveAndEnabled)
//            {
//                data.Add(nextMaid.GetThumIcon());
//            }
//        }

//        return data.ToArray();
//    }

//    public static bool firstCall = true;
//    public static List<List<string>> getAnimationsData()
//    {
//        List<List<string>> data = new List<List<string>>();
//        if(firstCall)
//        {
//            firstCall = false;
//            return data;
//        }
//        //PhotoMotion
//        foreach (PhotoMotionData photoData in PhotoMotionData.data)
//        {
//            try
//            {
//                string id = photoData.id.ToString().PadLeft(10, '0');
//                if (photoData.direct_file != null && !photoData.direct_file.Trim().Equals(""))
//                {
//                    string name = photoData.direct_file.Split(new string[] { ".anm" }, System.StringSplitOptions.RemoveEmptyEntries)[0];
//                    data.Add(new List<string>() { id, name });
                
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.ToString());
//            }
//        }

//        //General
//        //FileSystemWindows crcFileSystem = null;
//        //if (Directory.Exists(UTY.gameProjectPath + "\\" + "SaveData\\_gp03_import"))
//        //{
//        //    crcFileSystem = new FileSystemWindows();
//        //    crcFileSystem.SetBaseDirectory(UTY.gameProjectPath + "\\");
//        //    crcFileSystem.AddFolder("SaveData\\_gp03_import");
//        //    foreach (string path in crcFileSystem.GetList(string.Empty, AFileSystemBase.ListType.AllFolder))
//        //    {
//        //        if (!crcFileSystem.AddAutoPath(path))
//        //            UnityEngine.Debug.Log((object)("m_CrcFileSystemのAddAutoPathには既に " + path + " がありました。"));
//        //    }
//        //}
//        //AFileSystemBase[] afileSystemBaseArray = new AFileSystemBase[3]{ GameUty.FileSystemMod, GameUty.FileSystem, crcFileSystem };
//        //foreach (AFileSystemBase afileSystemBase in afileSystemBaseArray)
//        //{
//        //    if (afileSystemBase != null)
//        //    {
//        //        string[] files = afileSystemBase.GetFileListAtExtension("anm");
//        //        foreach(string file in files)
//        //        {
//        //            string id = "X".PadLeft(10, '0');
//        //            string name = file.Split(new string[] { ".anm" }, System.StringSplitOptions.RemoveEmptyEntries)[0];
//        //            data.Add(new List<string>() { id, name });
//        //        }
//        //    }
//        //}

//        MultiArcLoader sal = new MultiArcLoader(new string[] { @"C:\KISS\CM3D2\GameData", @"C:\KISS\COM3D2\GameData", @"C:\KISS\COM3D2\GameData_20" }, 3, MultiArcLoader.LoadMethod.Single, false, false, MultiArcLoader.Exclude.Voice | MultiArcLoader.Exclude.Sound);
//        sal.LoadArcs();
//        string[] files = sal.GetFileListAtExtension("anm");
//        foreach (string file in files)
//        {
//            string id = "X".PadLeft(10, '0');
//            string name = file.Split(new string[] { ".anm" }, System.StringSplitOptions.RemoveEmptyEntries)[0];
//            data.Add(new List<string>() { id, name });
//        }
//        return data;
//    }

//    public static void selectAnimation(int selectedIndex)
//    {
//        GUIDropDownDynamic dropdownMaid = (GUIDropDownDynamic)IMGuestGUIHelper.FindByName("DropdownMaid");
//        string maidVal = dropdownMaid.GetValue();

//        if(maidVal == null || maidVal.Equals(""))
//        {
//            return;
//        }

//        string[] maidVals = maidVal.Split(new string[] { "||" }, StringSplitOptions.None);
//        bool man = bool.Parse(maidVals[1]);
//        string guid = maidVals[2];
//        Maid maid;
//        if (man)
//        {
//            maid = GameMain.Instance.CharacterMgr.GetStockMan(guid);
//        }
//        else
//        {
//            maid = GameMain.Instance.CharacterMgr.GetMaid(guid);
//        }
//        if (maid == null)
//        {
//            return;
//        }

//        GUITable tableAnimation = (GUITable)IMGuestGUIHelper.FindByName("TableAnimation");
//        List<string> rowData = tableAnimation.GetRowData(selectedIndex);
//        string play_motion_name = rowData[1].Trim() + ".anm";
//        if (maid.body0.IsCrcBody)
//        {
//            if (play_motion_name.IndexOf("crc_") != 0)
//            {
//                string fileName = "crc_" + play_motion_name;
//                if (GameUty.IsExistFile(fileName, (AFileSystemBase)null))
//                    play_motion_name = fileName;
//            }
//            MotionKagManager.MotionSettingReset(maid);
//        }
//        if (maid.Visible)
//            PlayMaidMotion(maid, play_motion_name, false, false, false, GameUty.MillisecondToSecond(500));
//        return;
//    }
//    protected static void PlayMaidMotion(Maid maid, string fn,bool additive = false, bool loop = false, bool boAddQue = false, float val = 0.5f)
//    {
//        float num = val;
//        if (!GameMain.Instance.ScriptMgr.is_motion_blend)
//            val = 0.0f;
//        if ((UnityEngine.Object)maid.body0 != (UnityEngine.Object)null)
//        {
//            maid.body0.motionBlendTime = val;
//            if (!GameMain.Instance.ScriptMgr.is_motion_blend)
//                maid.fullBodyIK.bodyOffsetCtrl.blendTime = num;
//        }
//        maid.CrossFade(fn, GameUty.FileSystem, additive, loop, boAddQue, val, 1f);
//        maid.fullBodyIK.bodyOffsetCtrl.CheckBlendType();
//    }
//}