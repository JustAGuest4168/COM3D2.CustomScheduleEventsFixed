//using COM3D2.CustomScheduleEvents.Plugin;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using static GUIPages;
//using static IMGuestGUIHelperContained;

//public class UI
//{
//    public static int mainWindowId = 416809834;
//    public static IMGuestGUIMainWindow mainWindow { get; set; }

//    public static void Update()
//    {
//        //Create UI Once
//        if (mainWindow == null)
//        {
//            mainWindow = new IMGuestGUIMainWindow(mainWindowId, "Custom Schedule Event", 40, 40);

//            //HiddenPages
//            GUIPagesWizard pages = new GUIPagesWizard("Pages", null, new string[] { "Main", 
//                                                                                    "Category", 
//                                                                                    "VIP", "VIP Normal", "VIP Rental", 
//                                                                                    "Story Yotogi",
//                                                                                    "Script Helpers" }, onPageBackward, onPageForward, mainWindowId);
//            mainWindow.AddContent(pages);
//            {
//                //Main
//                GUIGroup main = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, pages.id);
//                pages.FindByNameLocal("Main").AddContent(main);
//                {
//                    //Category
//                    GUIButton categoryButton = new GUIButton(IMGuestGUIHelper.NewId(), "Create Category", delegate (int id) { mainPageSwitch(id, "Category"); }, false, main.id);
//                    main.AddContent(categoryButton);

//                    //VIP
//                    GUIButton vipButton = new GUIButton(IMGuestGUIHelper.NewId(), "Create VIP", delegate (int id) { mainPageSwitch(id, "VIP"); }, false, main.id);
//                    main.AddContent(vipButton);

//                    //Story Yotogi
//                    GUIButton storyYotogiButton = new GUIButton(IMGuestGUIHelper.NewId(), "Create Story Yotogi", delegate (int id) { mainPageSwitch(id, "Story Yotogi"); }, false, main.id);
//                    main.AddContent(storyYotogiButton);

//                    //Script Helpers
//                    GUIButton scriptHelpersButton = new GUIButton(IMGuestGUIHelper.NewId(), "Script Helpers", delegate (int id) { mainPageSwitch(id, "Script Helpers"); }, false, main.id);
//                    main.AddContent(scriptHelpersButton);
//                }

//                //Category
//                pages.FindByNameLocal("Category").AddContent(UICategory.Main(pages.id));

//                //VIP

//                //StoryYotogi

//                //Script Helpers
//                pages.FindByNameLocal("Script Helpers").AddContent(UIScriptHelpers.Main(pages.id));
//            }
            
//        }
//    }

//    //
//    public static void mainPageSwitch(int id, string nextPage)
//    {
//        GUIPagesWizard pages = (GUIPagesWizard)IMGuestGUIHelper.FindByName("Pages");
//        pages.MoveForwardToPage(nextPage);
//    }
//    public static void mainPageBack(int id)
//    {
//        GUIPagesWizard pages = (GUIPagesWizard)IMGuestGUIHelper.FindByName("Pages");
//        pages.MoveBackwards();
//    }

//    //Events
//    public static void onPageForward(string pageNameCurr, string pageNameNext)
//    {
//        if (Array.IndexOf(new string[] { "VIP", "VIP Normal", "VIP Rental" }, pageNameNext) != -1)
//        {
//            UIVIP.onPageForward(pageNameCurr, pageNameNext);
//        }
//    }
//    public static void onPageBackward(string pageNameCurr, string pageNameNext)
//    {
//        if(pageNameCurr.Equals("Category"))
//        {
//            UICategory.onPageBackward(pageNameCurr, pageNameNext);
//        }
//        if (Array.IndexOf(new string[]{ "VIP", "VIP Normal", "VIP Rental" }, pageNameCurr) != -1)
//        {
//            UIVIP.onPageBackward(pageNameCurr, pageNameNext);
//        }
//    }

//    //Helpers
//    public static void DisplayMessage(string message)
//    {
//        while (GameMain.Instance != null && GameMain.Instance.SysDlg != null && GameMain.Instance.SysDlg.isActiveAndEnabled)
//        {
//            GameMain.Instance.SysDlg.Close();
//        }

//        if (GameMain.Instance != null && GameMain.Instance.SysDlg != null)
//        {
//            GameMain.Instance.SysDlg.Show(message, SystemDialog.TYPE.OK, new SystemDialog.OnClick(GameMain.Instance.SysDlg.Close));
//        }
//    }
//}