//using Newtonsoft.Json;
//using Schedule;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using UnityEngine;
//using static COM3D2.CustomScheduleEvents.Plugin.CustomScheduleEvents;
//using static GUIDropDown;
//using static GUIDropDown.GUIDropDownDynamic;
//using static GUIPages;

//namespace COM3D2.CustomScheduleEvents.Plugin
//{
//    public class UIVIP
//    {
//        //Fields
//        public static ScheduleWorkNightUIVIPNormal2 UIVIPNormal;
//        public static ScheduleWorkNightUIVIPRental2 UIVIPRental;

//        //UI
//        public static IMGuestGUIHelper Main(int parentId)
//        {
//            GUIGroup layout = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, parentId);
//            {
//                //Button
//                GUIButton buttonNormal = new GUIButton(IMGuestGUIHelper.NewId(), "VIP Normal", delegate (int id) { UI.mainPageSwitch(id, "VIP Normal"); }, false, layout.id);
//                layout.AddContent(buttonNormal);

//                //Button
//                GUIButton buttonRental = new GUIButton(IMGuestGUIHelper.NewId(), "VIP Rental", delegate (int id) { UI.mainPageSwitch(id, "VIP Rental"); }, false, layout.id);
//                layout.AddContent(buttonRental);
//            }

//            return layout;
//        }
//        public static IMGuestGUIHelper VIPNormal(int parentId)
//        {
//            GUIGroup layout = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, parentId);
//            {
//                UIVIPNormal = new ScheduleWorkNightUIVIPNormal2(layout.id);
//            }
//            return layout;
//        }
//        public static IMGuestGUIHelper VIPRental(int parentId)
//        {
//            GUIGroup layout = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, parentId);
//            {
//                UIVIPRental = new ScheduleWorkNightUIVIPRental2(layout.id);
//            }
//            return layout;
//        }
//        public static void onPageForward(string pageNameCurr, string pageNameNext)
//        {
//            GUIPagesWizard pages = (GUIPagesWizard)IMGuestGUIHelper.FindByName("Pages");
//            GUIPageItem page = pages.FindByNameLocal(pageNameNext);
//            switch (pageNameNext)
//            {
//                case "VIP":
//                    Console.WriteLine("VIP START");
//                    page.ClearContent();
//                    page.AddContent(UIVIP.Main(page.id));
//                    Console.WriteLine("VIP END");
//                    break;
//                case "VIP Normal":
//                    page.ClearContent();
//                    page.AddContent(UIVIP.VIPNormal(pages.id));
//                    break;
//                case "VIP Rental":
//                    page.ClearContent();
//                    page.AddContent(UIVIP.VIPRental(pages.id));
//                    break;
//            }
//        }
//        public static void onPageBackward(string pageNameCurr, string pageNameNext)
//        {
//            if (pageNameCurr.Equals("VIP") && pageNameNext.Equals("Main"))
//            {
//                //Nothing
//            }
//            else if(pageNameCurr.Equals("VIP Normal") && pageNameNext.Equals("VIP"))
//            {
//                //Reset UI
//                UIVIPNormal = null;
//            }
//            else if(pageNameCurr.Equals("VIP Rental") && pageNameNext.Equals("VIP"))
//            {
//                UIVIPRental = null;
//            }
//        }

//        //Data



        

//        //Classes
//        public abstract class ScheduleWorkNightUI2
//        {
//            #region "Icons"
//            //Only for Data Extraction
//            //private static List<String> _icons = null;
//            //public static List<String> icons 
//            //{ 
//            //    get 
//            //    {
//            //        if (_icons == null)
//            //        {
//            //            string[] allFiles = GameUty.FileSystem.GetList("", AFileSystemBase.ListType.AllFile);
//            //            Array.FindAll<string>(allFiles, (Predicate<string>)(i => new Regex("schedule_icon_*").IsMatch(i) || new Regex("cm3d2_scheduleicon_*").IsMatch(i)));

//            //            //string[] comIcons = GameUty.FileSystem.GetList("schedule_icon_*", AFileSystemBase.ListType.AllFile);
//            //            //string[] cm3d2Icons = GameUty.FileSystem.GetList("cm3d2_scheduleicon_*", AFileSystemBase.ListType.AllFile);

//            //            _icons = new List<string>();
//            //            _icons.AddRange(allFiles);
//            //        }

//            //        return _icons;
//            //    } 
//            //}
//            public static string[] scheduleIconsNames = new string[] {
//            "schedule_icon_talk",
//            "schedule_icon_bar",
//            "schedule_icon_cafe",
//            "schedule_icon_casino",
//            "schedule_icon_choukyou",
//            "schedule_icon_clothing_check",
//            "schedule_icon_cookin_school",
//            "schedule_icon_dance_lesson",
//            "schedule_icon_gardening",
//            "schedule_icon_gravure",
//            "schedule_icon_hotel",
//            "schedule_icon_kadou",
//            "schedule_icon_lodgingroom",
//            "schedule_icon_love",
//            "schedule_icon_ntr",
//            "schedule_icon_opera",
//            "schedule_icon_piano",
//            "schedule_icon_pooldanceroom",
//            "schedule_icon_refle",
//            "schedule_icon_restaurant",
//            "schedule_icon_samigurumi",
//            "schedule_icon_shisetsu",
//            "schedule_icon_sm",
//            "schedule_icon_soap",
//            "schedule_icon_sommelier",
//            "schedule_icon_voice_leson",
//            "schedule_icon_yotogi_s",
//            "schedule_icon_collabocafe",
//            "schedule_icon_fantasyinn",
//            "schedule_icon_gelaende",
//            "schedule_icon_pool",
//            "schedule_icon_seacafe",
//            "schedule_icon_shrine",
//            "scheduleicon_gp01yotogi_h",
//            "scheduleicon_gp01yotogi_s",
//            "scheduleicon_gp02yotogi_y",
//            "schedule_icon_springgarden",
//            "cm3d2_scheduleicon_esthe",
//            "cm3d2_scheduleicon_kadou",
//            "cm3d2_scheduleicon_kesyou",
//            "cm3d2_scheduleicon_kyouyou",
//            "cm3d2_scheduleicon_kyusoku",
//            "cm3d2_scheduleicon_maidhaken",
//            "cm3d2_scheduleicon_maidkensyuu",
//            "cm3d2_scheduleicon_moyougae",
//            "cm3d2_scheduleicon_ryouri",
//            "cm3d2_scheduleicon_saihou",
//            "cm3d2_scheduleicon_sekkyaku",
//            "cm3d2_scheduleicon_sekkyakurensyuu",
//            "cm3d2_scheduleicon_souji",
//            "cm3d2_scheduleicon_vip",
//            "cm3d2_scheduleicon_yotogi",
//            "cm3d2_scheduleicon_help",
//            "cm3d2_scheduleicon_kyujitsu",
//            "cm3d2_scheduleicon_sinkonryokou",
//            "cm3d2_scheduleicon_sinkonseikatu",
//            "cm3d2_scheduleicon_jyunaivip",
//            "cm3d2_scheduleicon_rentalmaidvip"
//        };
//            private static List<Texture2D> _scheduleIcons;
//            public static List<Texture2D> ScheduleIcons
//            {
//                get
//                {
//                    if(_scheduleIcons == null)
//                    {
//                        _scheduleIcons = new List<Texture2D>();
//                        for(int i=0; i< scheduleIconsNames.Length; i++)
//                        {
//                            Texture2D icon = new Texture2D(80, 80);
//                            icon.LoadImage(GameUty.FileSystem.FileOpen(scheduleIconsNames[i] + ".tex").ReadAll());
//                            _scheduleIcons.Add(icon);
//                        }
//                    }
//                    return _scheduleIcons;
//                }
//            }
//            #endregion

//            protected GUIPagesTabs tabs { get; set; }

//            #region Basic
//            //protected GUIContainer containerBasic { get; set; }
//            protected GUIGroup containerBasic { get; set; }
//            protected GUIInput inputId { get; set; }
//            protected GUIInput inputName { get; set; }
//            protected GUIDropDownDynamic dropdownCategory { get; set; }
//            protected GUIDropDownDynamic dropdownIcon { get; set; }
//            protected GUIInput inputDescription { get; set; }
//            protected GUIInput inputIncome { get; set; }
//            #endregion

//            #region Requirements
//            //protected GUIContainer containerRequirements { get; set; }
//            protected GUIGroup containerRequirements { get; set; }
//            protected GUIDropDownDynamic dropdownSalonGrade { get; set; }

//            protected GUIContainer containerRequirementsHints { get; set; }
//            protected List<GUIInput> inputRequirementHints { get; set; }
//            #endregion

//            #region Advanced Requirements
//            //protected GUIContainer containerRequirementsAdvanced { get; set; }
//            protected GUIGroup containerRequirementsAdvanced { get; set; }
//            #endregion

//            public ScheduleWorkNightUI2(int layoutId)
//            {
//                GUIGroup layout = (GUIGroup)IMGuestGUIHelper.widgets[layoutId];

//                //Tabs
//                tabs = new GUIPagesTabs(IMGuestGUIHelper.NewId(), null, new string[] { "Basic", "Requirements", "Advanced" }, layout.id);
//                layout.AddContent(tabs);

                
//                //Basic
//                //containerBasic = new GUIContainer(IMGuestGUIHelper.NewId(), "Basic Info", true, true, GUIContainer.GUIContainerMode.vertical, layout.id);
//                //layout.AddContent(containerBasic);
//                containerBasic = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, 0);
//                tabs.AddPageContent("Basic", containerBasic);
//                {
//                    //ID
//                    inputId = new GUIInput(IMGuestGUIHelper.NewId(), "NewVIPID", "ID", "", GUIInput.GUIInputCharRules.Numeric, IMGuestGUIHelperContained.HelperContainedMode.enabled, inputIdChanged, containerBasic.id);
//                    containerBasic.AddContent(inputId);

//                    //Name
//                    inputName = new GUIInput(IMGuestGUIHelper.NewId(), "NewVIPName", "Name", "", GUIInput.GUIInputCharRules.AlphaEng | GUIInput.GUIInputCharRules.Numeric, IMGuestGUIHelperContained.HelperContainedMode.enabled, inputNameChanged, containerBasic.id);
//                    containerBasic.AddContent(inputName);

//                    //Category
//                    dropdownCategory = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), "Category", "", false, GetCategoriesData, 5, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerBasic.id);
//                    containerBasic.AddContent(dropdownCategory);

//                    //Icon
//                    dropdownIcon = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), "Icon", "", false, scheduleIconsNames, ScheduleIcons.ToArray(), 80, 80, 5, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerBasic.id);
//                    containerBasic.AddContent(dropdownIcon);

//                    //Description
//                    inputDescription = new GUIInput(IMGuestGUIHelper.NewId(), "Description", "", GUIInput.GUIInputCharRules.None, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerBasic.id);
//                    containerBasic.AddContent(inputDescription);

//                    //Income???
//                    inputIncome = new GUIInput(IMGuestGUIHelper.NewId(), "Base Income (Usually 0...)", "0", GUIInput.GUIInputCharRules.Numeric, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerBasic.id);
//                    containerBasic.AddContent(inputIncome);
//                }

//                //Requirements
//                //containerRequirements = new GUIContainer(IMGuestGUIHelper.NewId(), "Requirements", true, false, GUIContainer.GUIContainerMode.vertical, layout.id);
//                //layout.AddContent(containerRequirements);
//                containerRequirements = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, 0);
//                tabs.AddPageContent("Requirements", containerRequirements);
//                {
//                    //Hints
//                    containerRequirementsHints = new GUIContainer(IMGuestGUIHelper.NewId(), "Hints", true, false, GUIContainer.GUIContainerMode.vertical, containerRequirements.id);
//                    containerRequirements.AddContent(containerRequirementsHints);
//                    {
//                        inputRequirementHints = new List<GUIInput>();

//                        //Hint
//                        for (int i = 0; i < 9; i++)
//                        {
//                            inputRequirementHints.Add(new GUIInput(IMGuestGUIHelper.NewId(), "Hint " + (i + 1), "", GUIInput.GUIInputCharRules.None, IMGuestGUIHelperContained.HelperContainedMode.disabled, containerRequirementsHints.id));
//                            containerRequirementsHints.AddContent(inputRequirementHints[i]);
//                        }
//                    }

//                    //Salon Grade
//                    dropdownSalonGrade = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), "Salon Grade", "0", false, new string[] { "0", "1", "2", "3", "4", "5" }, 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerRequirements.id);
//                    containerRequirements.AddContent(dropdownSalonGrade);
//                }

//                //Advanced Requirements
//                //containerRequirementsAdvanced = new GUIContainer(IMGuestGUIHelper.NewId(), "Advanced Requirements", true, false, GUIContainer.GUIContainerMode.vertical, layout.id);
//                //layout.AddContent(containerRequirementsAdvanced);
//                containerRequirementsAdvanced = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, 0);
//                tabs.AddPageContent("Advanced", containerRequirementsAdvanced);
//                {

//                }

//                //Save
//                GUIButton buttonSave = new GUIButton(IMGuestGUIHelper.NewId(), "Save", Save, false, layout.id);
//                layout.AddContent(buttonSave);
//            }

//            //Data
//            public static string[] GetCategoriesData()
//            {
//                List<string> datas = new List<string>();
//                foreach (KeyValuePair<int, string> data in ScheduleCSVData.TaskCategoryNameMap)
//                {
//                    Console.WriteLine(data.Key.ToString().PadLeft(6, '0') + " | " + data.Value);
//                    datas.Add(data.Key.ToString().PadLeft(6, '0') + " | " + data.Value);
//                }

//                int maxLength = datas.OrderByDescending(s => s.Length).First().Length;
//                for(int i=0; i<datas.Count; i++)
//                {
//                    datas[i] = datas[i].PadRight(maxLength);
//                }

//                return datas.ToArray();
//            }

//            //Inputs
//            public void inputIdChanged(string value, string valueText)
//            {
//                if (!int.TryParse(valueText.Trim(), out _))
//                {
//                    UI.DisplayMessage("ERROR: Invalid VIP ID");
//                    valueText = value;
//                    return;
//                }
//            }
//            public void inputNameChanged(string value, string valueText)
//            {
//                if (valueText.Trim().Equals(""))
//                {
//                    UI.DisplayMessage("ERROR: Invalid VIP Name");
//                    valueText = value;
//                    return;
//                }
//            }

//            public void Save(int buttonSaveId)
//            {
//                //Build
//                ScheduleWorkNight data = neiFile();

//                //Actual File
//                Dictionary<string, string> neiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(CustomScheduleEvents.configPath, "neiAppendConfig.json")));
//                if (!neiConfig.ContainsKey("schedule_work_night"))
//                {
//                    neiConfig["schedule_work_night"] = Path.Combine(CustomScheduleEvents.modPath, "NIGHT");
//                }
//                string path = neiConfig["schedule_work_night"];
//                if (!Directory.Exists(path))
//                {
//                    Directory.CreateDirectory(path);
//                }
//                File.WriteAllText(Path.Combine(path, data.ID + "_schedule_work_night.json"), JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8);

//                //Sample Script

//                //Success
//                UI.DisplayMessage("SUCCESS: VIP Event Created!" +
//                                  "\nStarting sample script " + data.ID + ".ks has been added to your Mod folder." +
//                                  "\nEdit this file in a text editor capable of saving with Shift JIS encoding, but do not rename it.");
//            }
//            public ScheduleWorkNight neiFile()
//            {
//                ScheduleWorkNight data = new ScheduleWorkNight();

//                //Basic
//                data.ID = Int32.Parse(this.inputId.GetValue());
//                data.name = this.inputName.GetValue();
//                data.comment = "";//this.comment;
//                data.icon = this.dropdownIcon.GetValue();
//                data.categoryId = Int32.Parse(this.dropdownCategory.GetValue());
//                data.ntrFlag_obsolete = false;
//                data.yotogiType = "Vip";
//                data.descriptionText = this.inputDescription.GetValue();
//                data.incomeBase = Int32.Parse(this.inputIncome.GetValue());
//                data.isRentalMaid = "";

//                this.neiFile_1(data);

//                //Requirements
//                data.requirementText1 = this.inputRequirementHints[0].GetValue();
//                data.requirementText2 = this.inputRequirementHints[1].GetValue();
//                data.requirementText3 = this.inputRequirementHints[2].GetValue();
//                data.requirementText4 = this.inputRequirementHints[3].GetValue();
//                data.requirementText5 = this.inputRequirementHints[4].GetValue();
//                data.requirementText6 = this.inputRequirementHints[5].GetValue();
//                data.requirementText7 = this.inputRequirementHints[6].GetValue();
//                data.requirementText8 = this.inputRequirementHints[7].GetValue();
//                data.requirementText9 = this.inputRequirementHints[8].GetValue();

//                data.requiredSalonGrade = Int32.Parse(this.dropdownSalonGrade.GetValue());

//                this.neiFile_2(data);

//                //Advanced -- flags are dynamic, no predefined list
//                //data.manHasFlagDisplay = this.manHasFlagDisplay; //Condition to Display???
//                //data.manHasFlagExecute = this.manHasFlagExecute;
//                //data.manLacksFlagExecute = this.manLacksFlagExecute;

//                ////Advanced -- this means a maid (other than event maid) must be working at facility for correct day/night
//                //data.facilitiesInUse = string.Join("&", this.facilitiesInUse.Select(lst => string.Join("|", lst.ToArray())).ToArray()); //really ints of the IDs

//                ////Only 1 additional maid???
//                //data.additionalMaidPersonalityRequirement = string.Join("|", this.additionalMaidPersonalityRequirement.ToArray());

//                neiFile_3(data);

//                //No clue on these, something to do with CRE body
//                data.isCheckGP002Personal = "";
//                data.bodyMatchCheck = "";
//                data.bodyMatchBlock = "";
//                data.bodyMatchCheckWithMainCharacter = "";

//                return data;
//            }
//            public abstract void neiFile_1(ScheduleWorkNight data);
//            public abstract void neiFile_2(ScheduleWorkNight data);
//            public abstract void neiFile_3(ScheduleWorkNight data);
//        }
//        public class ScheduleWorkNightUIVIPNormal2 : ScheduleWorkNightUI2
//        {
//            #region Basic

//            //Entertained
//            protected GUIToggle toggleEval { get; set; }
//            protected GUIToggle toggleEntertainedMaster;
//            protected GUIToggle toggleEntertainedGuest;
//            #endregion

//            #region Requirements
//            //Personality
//            #region PersonalityNames
//            private static List<string> _personalityNames;
//            public static List<string> PersonalityNames
//            {
//                get
//                {
//                    if (_personalityNames == null)
//                    {
//                        _personalityNames = new List<string>();

//                        List<MaidStatus.Personal.Data> datas = MaidStatus.Personal.GetAllDatas(false);
//                        foreach (MaidStatus.Personal.Data data in datas)
//                        {
//                            _personalityNames.Add(data.uniqueName);
//                        }
//                    }
//                    return _personalityNames;
//                }
//            }
//            #endregion
//            protected GUIContainer containerPersonality { get; set; }
//            protected GUIToggle togglePersonalityTrio { get; set; }
//            protected GUIDropDownDynamic dropdownPersonality { get; set; }

//            //Contract
//            protected GUIContainer containerContract { get; set; }
//            protected GUIToggle toggleContractTrainee { get; set; }
//            protected GUIToggle toggleContractExclusive { get; set; }
//            protected GUIToggle toggleContractFree { get; set; }

//            //Hole Experience
//            protected GUIContainer containerHoleExp { get; set; }
//            protected GUIToggle toggleHoleExpVirgin { get; set; }
//            protected GUIToggle toggleHoleExpVag { get; set; }
//            protected GUIToggle toggleHoleExpAnal { get; set; }
//            protected GUIToggle toggleHoleExpBoth { get; set; }

//            //Relationship
//            //protected GUIContainer containerRelationship { get; set; }
//            protected  GUIDropDownDynamic dropdownRelationship { get; set; }

//            //Propensity
//            #region PropensityNames

//            private static List<string> _propensityNames;
//            public static List<string> PropensityNames
//            {
//                get
//                {
//                    if (_propensityNames == null)
//                    {
//                        _propensityNames = new List<string>();

//                        List<MaidStatus.Propensity.Data> datas = MaidStatus.Propensity.GetAllDatas(false);
//                        foreach (MaidStatus.Propensity.Data data in datas)
//                        {
//                            _propensityNames.Add(data.uniqueName);
//                        }
//                    }
//                    return _propensityNames;
//                }
//            }
//            #endregion
//            //protected GUIContainer containerPropensity { get; set; }
//            protected GUIDropDownDynamic dropdownPropensity { get; set; }

//            //Maid Class
//            #region JobClassNames
//            private static List<string> _jobClassNames;
//            public static List<string> JobClassNames
//            {
//                get
//                {
//                    if (_jobClassNames == null)
//                    {
//                        _jobClassNames = new List<string>();

//                        List<MaidStatus.JobClass.Data> datas = MaidStatus.JobClass.GetAllDatas(false);
//                        foreach (MaidStatus.JobClass.Data data in datas)
//                        {
//                            _jobClassNames.Add(data.uniqueName);
//                        }
//                    }

//                    return _jobClassNames;
//                }
//            }
//            #endregion
//            //protected GUIContainer containerClassJob { get; set; }
//            protected GUIDropDownDynamic dropdownClassJob { get; set; }

//            //Yotogi Class
//            #region YotogiClassNames

//            private static List<string> _yotogiClassNames;
//            public static List<string> YotogiClassNames
//            {
//                get
//                {
//                    if (_yotogiClassNames == null)
//                    {
//                        _yotogiClassNames = new List<string>();

//                        List<MaidStatus.YotogiClass.Data> datas = MaidStatus.YotogiClass.GetAllDatas(false);
//                        foreach (MaidStatus.YotogiClass.Data data in datas)
//                        {
//                            _yotogiClassNames.Add(data.uniqueName);
//                        }
//                    }
//                    return _yotogiClassNames;
//                }
//            }
//            #endregion
//            //protected GUIContainer containerClassYotogi { get; set; }
//            protected GUIDropDownDynamic dropdownClassYotogi { get; set; }

//            //Skills

//            #endregion

//            #region Advanced Requirements
//            #endregion

//            public ScheduleWorkNightUIVIPNormal2(int layoutId) : base(layoutId)
//            {
//                GUIGroup layout = (GUIGroup)IMGuestGUIHelper.widgets[layoutId];

//                //Basic
//                {
//                    GUIContainer groupEntertained = new GUIContainer(IMGuestGUIHelper.NewId(), "Maid Stats", false, true, GUIContainer.GUIContainerMode.vertical, containerBasic.id);
//                    containerBasic.AddContent(groupEntertained);
//                    {
//                        toggleEval = new GUIToggle(IMGuestGUIHelper.NewId(), "Improve Maid Customer Evaluation?", false, groupEntertained.id);
//                        groupEntertained.AddContent(toggleEval);

//                        toggleEntertainedMaster = new GUIToggle(IMGuestGUIHelper.NewId(), "Count as Master Entertained?", false, groupEntertained.id);
//                        groupEntertained.AddContent(toggleEntertainedMaster);

//                        toggleEntertainedGuest = new GUIToggle(IMGuestGUIHelper.NewId(), "Count as Guest Entertained?", false, groupEntertained.id);
//                        groupEntertained.AddContent(toggleEntertainedGuest);

//                        groupEntertained.AddContent(new GUISpace(5, groupEntertained.id));
//                    }
//                }

//                //Requirements
//                {
//                    //Personality
//                    containerPersonality = new GUIContainer(IMGuestGUIHelper.NewId(), "Personality", false, true, GUIContainer.GUIContainerMode.vertical, containerRequirements.id);
//                    containerRequirements.AddContent(containerPersonality);
//                    {
//                        //Main Trio
//                        togglePersonalityTrio = new GUIToggle(IMGuestGUIHelper.NewId(), "Main Trio?", false, containerPersonality.id);
//                        containerPersonality.AddContent(togglePersonalityTrio);

//                        //Personality
//                        dropdownPersonality = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), null, null, true, PersonalityNames.ToArray(), 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.disabled, containerPersonality.id);
//                        containerPersonality.AddContent(dropdownPersonality);
//                    }

//                    //Contract
//                    GUIGroup contractGroup = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, containerRequirements.id);
//                    containerRequirements.AddContent(contractGroup);
//                    {
//                        containerContract = new GUIContainer(IMGuestGUIHelper.NewId(), "Contract", false, true, GUIContainer.GUIContainerMode.horizontal, contractGroup.id);
//                        contractGroup.AddContent(containerContract);
//                        {
//                            //Trainee
//                            toggleContractTrainee = new GUIToggle(IMGuestGUIHelper.NewId(), "Trainee", false, containerContract.id);
//                            containerContract.AddContent(toggleContractTrainee);

//                            //Exclusive
//                            toggleContractExclusive = new GUIToggle(IMGuestGUIHelper.NewId(), "Exclusive", false, containerContract.id);
//                            containerContract.AddContent(toggleContractExclusive);

//                            //Free
//                            toggleContractFree = new GUIToggle(IMGuestGUIHelper.NewId(), "Free", false, containerContract.id);
//                            containerContract.AddContent(toggleContractFree);
//                        }

//                        contractGroup.AddContent(new GUISpace(15, contractGroup.id));
//                    }

//                    //Hole Exp
//                    GUIGroup holeGroup = new GUIGroup(IMGuestGUIHelper.NewId(), GUIGroup.GUIGroupMode.vertical, containerRequirements.id);
//                    containerRequirements.AddContent(holeGroup);
//                    {
//                        containerHoleExp = new GUIContainer(IMGuestGUIHelper.NewId(), "Hole Experience", false, true, GUIContainer.GUIContainerMode.horizontal, holeGroup.id);
//                        holeGroup.AddContent(containerHoleExp);
//                        {
//                            //Virgin
//                            toggleHoleExpVirgin = new GUIToggle(IMGuestGUIHelper.NewId(), "Virgin", false, containerHoleExp.id);
//                            containerHoleExp.AddContent(toggleHoleExpVirgin);

//                            //Vag
//                            toggleHoleExpVag = new GUIToggle(IMGuestGUIHelper.NewId(), "Vaginal Only", false, containerHoleExp.id);
//                            containerHoleExp.AddContent(toggleHoleExpVag);

//                            //Anal
//                            toggleHoleExpAnal = new GUIToggle(IMGuestGUIHelper.NewId(), "Anal Only", false, containerHoleExp.id);
//                            containerHoleExp.AddContent(toggleHoleExpAnal);

//                            //Both
//                            toggleHoleExpBoth = new GUIToggle(IMGuestGUIHelper.NewId(), "Both", false, containerHoleExp.id);
//                            containerHoleExp.AddContent(toggleHoleExpBoth);
//                        }

//                        holeGroup.AddContent(new GUISpace(15, holeGroup.id));
//                    }

//                    //Relationship
//                    //containerRelationship = new GUIContainer(IMGuestGUIHelper.NewId(), "Relationship", false, true, GUIContainer.GUIContainerMode.vertical, containerRequirements.id);
//                    //containerRequirements.AddContent(containerRelationship);
//                    {
//                        //dropdownRelationship = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), null, "", true, new string[] {"Contact", "Trust", "Lover", "Vigilance", "LoverPlus", "Slave", "Married" }, 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerRelationship.id);
//                        //containerRelationship.AddContent(dropdownRelationship);
//                        dropdownRelationship = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), "Relationship", "", true, new string[] { "Contact", "Trust", "Lover", "Vigilance", "LoverPlus", "Slave", "Married" }, 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerRequirements.id);
//                        containerRequirements.AddContent(dropdownRelationship);
//                    }

//                    //Propensity
//                    //containerPropensity = new GUIContainer(IMGuestGUIHelper.NewId(), "Propensity", false, true, GUIContainer.GUIContainerMode.vertical, containerRequirements.id);
//                    //containerRequirements.AddContent(containerPropensity);
//                    {
//                        //dropdownPropensity = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), null, "", true, PropensityNames.ToArray(), 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerPropensity.id);
//                        //containerPropensity.AddContent(dropdownPropensity);
//                        dropdownPropensity = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), "Propensity", "", true, PropensityNames.ToArray(), 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerRequirements.id);
//                        containerRequirements.AddContent(dropdownPropensity);
//                    }

//                    //Job Class
//                    //containerClassJob = new GUIContainer(IMGuestGUIHelper.NewId(), "Job Class", false, true, GUIContainer.GUIContainerMode.vertical, containerRequirements.id);
//                    //containerRequirements.AddContent(containerClassJob);
//                    {
//                        //dropdownClassJob = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), null, "", true, JobClassNames.ToArray(), 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerClassJob.id);
//                        //containerClassJob.AddContent(dropdownClassJob);
//                        dropdownClassJob = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), "Job Class", "", true, JobClassNames.ToArray(), 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerRequirements.id);
//                        containerRequirements.AddContent(dropdownClassJob);
//                    }

//                    //Yotogi Class
//                    //containerClassYotogi = new GUIContainer(IMGuestGUIHelper.NewId(), "Yotogi Class", false, true, GUIContainer.GUIContainerMode.vertical, containerRequirements.id);
//                    //containerRequirements.AddContent(containerClassYotogi);
//                    {
//                        //dropdownClassYotogi = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), "", "", true, YotogiClassNames.ToArray(), 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerClassYotogi.id);
//                        //containerClassYotogi.AddContent(dropdownClassYotogi);
//                        dropdownClassYotogi = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), "Yotogi Class", "", true, YotogiClassNames.ToArray(), 3, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerRequirements.id);
//                        containerRequirements.AddContent(dropdownClassYotogi);
//                    }

//                    //Skills
//                }

//                //Advanced
//                //Maid Has Flags
//                //Maid Lacks Flags
//            }

            

//            public override void neiFile_1(ScheduleWorkNight data)
//            {
//                data.evaluationBase = (this.toggleEval.GetValue()) ? 10 : 0;
//                data.countAsMasterEntertained = this.toggleEntertainedMaster.GetValue() ? 1 : 0;
//                data.countAsGuestsEntertained = this.toggleEntertainedGuest.GetValue() ? 1 : 0;
//                data.rentalMaidName = "";
//            }
//            public override void neiFile_2(ScheduleWorkNight data)
//            {
//                data.maidIsMainTrio = (this.togglePersonalityTrio.GetValue()) ? 1 : 0;
//                data.maidPersonality = string.Join("|", this.dropdownPersonality.GetValues().ToArray());

//                data.requiredContracts = ((this.toggleContractFree.GetValue()) ? "|Free" : "") + ((this.toggleContractExclusive.GetValue()) ? "|Exclusive" : "") + ((this.toggleContractTrainee.GetValue()) ? "|Trainee" : "");
//                data.requiredHoleExperience = ((this.toggleHoleExpVirgin.GetValue()) ? "|No_No" : "") + ((this.toggleHoleExpVag.GetValue()) ? "|Yes_No" : "") + ((this.toggleHoleExpAnal.GetValue()) ? "|No_Yes" : "") + ((this.toggleHoleExpBoth.GetValue()) ? "|Yes_Yes" : "");
//                data.maidRelationship = string.Join("|", this.dropdownRelationship.GetValues().ToArray());

//                data.requiredPropensity = string.Join("|", this.dropdownPropensity.GetValues().ToArray());
//                data.requiredMaidClass = string.Join("&", this.dropdownClassJob.GetValues().ToArray());
//                //foreach (string maidClass in this.requiredMaidClass)
//                //{
//                //    if(!data.requiredMaidClass.Equals(""))
//                //    {
//                //        data.requiredMaidClass += "&";
//                //    }
//                //    data.requiredMaidClass += maidClass;
//                //}
//                data.requiredYotogiClass = string.Join("&", this.dropdownClassYotogi.GetValues().ToArray());
                
//                //data.requiredSkillLevels = string.Join("&", (this.requiredSkillMinLevel.Select(kvp => string.Format("@{0},{1}", kvp.Key, Math.Min(3, Math.Max(0, kvp.Value))))).ToArray());
                
//                //foreach(KeyValuePair<int, int> kvp in this.requiredSkillMinLevel)
//                //{
//                //    if(!data.requiredSkillLevels.Equals(""))
//                //    {
//                //        data.requiredSkillLevels += "&";
//                //    }
//                //    data.requiredSkillLevels +=(kvp.Key + "," + Math.Min(3, Math.Max(0, kvp.Value)));
//                //}
//            }
//            public override void neiFile_3(ScheduleWorkNight data)
//            {
//                //data.maidHasFlag = string.Join("&", this.maidHasFlag.ToArray());
//                //data.maidLacksFlag = string.Join("&", this.maidLacksFlag.ToArray());
//            }
//        }
//        public class ScheduleWorkNightUIVIPRental2 : ScheduleWorkNightUI2
//        {
//            #region SubMaidNames
//            private static List<string> _subMaidNames;
//            public static List<string> SubMaidNames
//            {
//                get
//                {
//                    if (_subMaidNames == null)
//                    {
//                        _subMaidNames = new List<string>();

//                        List<MaidStatus.SubMaid.Data> datas = MaidStatus.SubMaid.GetAllDatas(false);
//                        foreach (MaidStatus.SubMaid.Data data in datas)
//                        {
//                            _subMaidNames.Add(data.uniqueName);
//                        }
//                    }
//                    return _subMaidNames;
//                }
//            }
//            #endregion
//            protected GUIDropDownDynamic dropdownRentalMaidName;

//            public ScheduleWorkNightUIVIPRental2(int layoutId) : base(layoutId)
//            {
//                GUIGroup layout = (GUIGroup)IMGuestGUIHelper.widgets[layoutId];

//                //Basic
//                {
//                    dropdownRentalMaidName = new GUIDropDownDynamic(IMGuestGUIHelper.NewId(), "Rental Maid", "", false, SubMaidNames.ToArray(), 5, UI.mainWindowId, IMGuestGUIHelperContained.HelperContainedMode.enabled, containerBasic.id);
//                    containerBasic.AddContent(dropdownRentalMaidName);
//                }
//            }

//            public override void neiFile_1(ScheduleWorkNight data)
//            {
//                data.evaluationBase = 0;
//                data.countAsMasterEntertained = 0;
//                data.countAsGuestsEntertained = 0;
//                data.rentalMaidName = this.dropdownRentalMaidName.GetValue();
//            }
//            public override void neiFile_2(ScheduleWorkNight data)
//            {
//                data.maidIsMainTrio = 0;
//                data.maidPersonality = "";

//                data.requiredContracts = "";
//                data.requiredHoleExperience = "";
//                data.maidRelationship = "";
//                data.requiredPropensity = "";
//                data.requiredMaidClass = "";
//                data.requiredYotogiClass = "";
//                data.requiredSkillLevels = "";
//            }
//            public override void neiFile_3(ScheduleWorkNight data)
//            {
//                data.maidHasFlag = "";
//                data.maidLacksFlag = "";
//            }
//        }
//    }
//}