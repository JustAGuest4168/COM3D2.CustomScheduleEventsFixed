//using COM3D2.CustomScheduleEvents.Plugin;
//using Newtonsoft.Json;
//using Schedule;
//using System.Collections.Generic;
//using System.IO;
//using static COM3D2.CustomScheduleEvents.Plugin.CustomScheduleEvents;

//public class UICategory
//{
//    //UI
//    public static IMGuestGUIHelper Main(int parentId)
//    {
//        GUIGroup layout = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, parentId);
//        {
//            //Table
//            GUITable categories = new GUITable("NewCatTable", "Categories", new List<string>() { "ID", "Name" }, getCategoriesData, IMGuestGUIHelperContained.HelperContainedMode.enabled, layout.id);
//            layout.AddContent(categories);

//            //New Form
//            GUIContainer containerNew = new GUIContainer(IMGuestGUIHelper.NewId(), "New Category", false, true, GUIContainer.GUIContainerMode.vertical, layout.id);
//            {
//                //Fields
//                GUIInput inputId = new GUIInput(IMGuestGUIHelper.NewId(), "NewCatID", "ID", "", GUIInput.GUIInputCharRules.Numeric, IMGuestGUIHelperContained.HelperContainedMode.enabled, inputIdChanged, containerNew.id);
//                containerNew.AddContent(inputId);

//                GUIInput inputName = new GUIInput(IMGuestGUIHelper.NewId(), "NewCatName", "Name", "", GUIInput.GUIInputCharRules.AlphaEng | GUIInput.GUIInputCharRules.Numeric, IMGuestGUIHelperContained.HelperContainedMode.enabled, inputNameChanged, containerNew.id);
//                containerNew.AddContent(inputName);

//                //Save
//                GUIButton buttonSave = new GUIButton(IMGuestGUIHelper.NewId(), "Save", SaveCategory, false, containerNew.id);
//                containerNew.AddContent(buttonSave);
//            }
//            layout.AddContent(containerNew);
//        }

//        return layout;
//    }
//    public static void onPageBackward(string pageNameCurr, string pageNameNext)
//    {
//        if (pageNameCurr.Equals("Category") && pageNameNext.Equals("Main"))
//        {
//            GUIInput inputId = ((GUIInput)IMGuestGUIHelper.FindByName("NewCatID"));
//            GUIInput inputName = ((GUIInput)IMGuestGUIHelper.FindByName("NewCatName"));
//            inputId.SetValue("");
//            inputName.SetValue("");
//        }
//    }

//    //Data
//    public static List<List<string>> getCategoriesData()
//    {
//        List<List<string>> data = new List<List<string>>();
//        foreach (KeyValuePair<int, string> taskCategoryName in ScheduleCSVData.TaskCategoryNameMap)
//        {
//            data.Add(new List<string>() { taskCategoryName.Key.ToString().PadLeft(6, '0'), taskCategoryName.Value });
//        }
//        return data;
//    }

//    //Inputs
//    public static void inputIdChanged(string value, string valueText)
//    {
//        if (!int.TryParse(valueText.Trim(), out _))
//        {
//            UI.DisplayMessage("ERROR: Invalid Category ID");
//            valueText = value;
//            return;
//        }
//    }
//    public static void inputNameChanged(string value, string valueText)
//    {
//        if (valueText.Trim().Equals(""))
//        {
//            UI.DisplayMessage("ERROR: Invalid Category Name");
//            valueText = value;
//            return;
//        }
//    }

//    //Save
//    public static void SaveCategory(int id)
//    {
//        GUITable table = ((GUITable)IMGuestGUIHelper.FindByName("NewCatTable"));
//        GUIInput inputId = ((GUIInput)IMGuestGUIHelper.FindByName("NewCatID"));
//        GUIInput inputName = ((GUIInput)IMGuestGUIHelper.FindByName("NewCatName"));

//        string newId = inputId.GetValue();
//        string newName = inputName.GetValue();

//        Dictionary<string, string> neiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(CustomScheduleEvents.configPath, "neiAppendConfig.json")));

//        //Validations
//        if (!int.TryParse(newId.Trim(), out _))
//        {
//            UI.DisplayMessage("ERROR: Invalid Category ID");
//            return;
//        }
//        if (newName.Trim().Equals(""))
//        {
//            UI.DisplayMessage("ERROR: Invalid Category Name");
//            return;
//        }
//        foreach (KeyValuePair<int, string> taskCategoryName in ScheduleCSVData.TaskCategoryNameMap)
//        {
//            if (taskCategoryName.Key == int.Parse(newId.Trim()))
//            {
//                UI.DisplayMessage("ERROR: Category ID Already Taken");
//                return;
//            }
//        }

//        //Write the file
//        if (!neiConfig.ContainsKey("schedule_work_night_category_list"))
//        {
//            neiConfig["schedule_work_night_category_list"] = Path.Combine(CustomScheduleEvents.modPath, "NIGHT_CAT");
//        }
//        string path = neiConfig["schedule_work_night_category_list"];
//        if (!Directory.Exists(path))
//        {
//            Directory.CreateDirectory(path);
//        }
//        ScheduleNightCategory newCategory = new ScheduleNightCategory();
//        newCategory.ID = int.Parse(newId.Trim());
//        newCategory.name = newName.Trim();
//        File.WriteAllText(Path.Combine(path, newCategory.ID + ".json"), JsonConvert.SerializeObject(newCategory), System.Text.Encoding.UTF8);

//        //Update Data
//        ScheduleCSVData.TaskCategoryNameMap[newCategory.ID] = newCategory.name;
//        table.Refresh();
//        inputId.SetValue("");
//        inputName.SetValue("");

//        UI.DisplayMessage("SUCCESS: Category Created!\nCategory available on next restart.");
//        return;
//    }
//}