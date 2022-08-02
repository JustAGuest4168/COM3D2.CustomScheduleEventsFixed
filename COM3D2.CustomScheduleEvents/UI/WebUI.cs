using HarmonyLib;
using Newtonsoft.Json;
using Schedule;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core;
using static PluginWebUI.Plugin.SimpleHTTPServer;

namespace COM3D2.CustomScheduleEvents.Plugin
{
    public class WebUI
    {
        #region Translation
        class AsyncTranslationResult
        {
            public bool Succeeded { get; set; }
            public string TranslatedText { get; set; }

            public void Resolve(TranslationResult r)
            {
                this.Succeeded = r.Succeeded;
                this.TranslatedText = r.TranslatedText;
            }
        }

        public static ITranslator _translator;
        public static ITranslator translator
        {
            get
            {
                if (_translator == null)
                {
                    _translator = Resources.FindObjectsOfTypeAll<AutoTranslationPlugin>().FirstOrDefault();
                }

                return _translator;
            }
        }
        public static string Translate(string text)
        {
            try
            {
                var result = new AsyncTranslationResult();
                if (translator != null)
                {
                    translator.TranslateAsync(text, result.Resolve);
                    return (result.Succeeded) ? result.TranslatedText : text;
                }
                else
                {
                    return text;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("CustomScheduleEvent Translate:\n" + ex.ToString());
                return text;
            }


            //string translatedText = "";
            //translationReady = false;
            //translator.TranslateAsync(text, out translatedText);
            //while(!translationReady)
            //return (translated) ? translatedText : text;
        }
        #endregion

        #region Categories
        public static SimpleHTTPServerPOSTResponse getScheduleCategoriesTableData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();

            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            //Fetch data
            List<List<string>> data = new List<List<string>>();
            foreach (KeyValuePair<int, string> taskCategoryName in ScheduleCSVData.TaskCategoryNameMap)
            {
                data.Add(new List<string>() { taskCategoryName.Key.ToString().PadLeft(6, '0'),
                                              Translate(taskCategoryName.Value)
                });
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse saveNewCategory(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            NewCategoryJson data = JsonConvert.DeserializeObject<NewCategoryJson>(json);

            //Validations
            if (data.Id == 0)
            {
                resp.errorText = ("ERROR: Invalid Category ID");
                return resp;
            }
            if (data.Name.Trim().Equals(""))
            {
                resp.errorText = ("ERROR: Invalid Category Name");
                return resp;
            }
            if (ScheduleCSVData.TaskCategoryNameMap.ContainsKey(data.Id))
            {
                resp.errorText = ("ERROR: Category ID Already Taken");
                return resp;
            }

            //Write the file
            Dictionary<string, string> neiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(CustomScheduleEvents.configPath, "neiAppendConfig.json")));
            if (!neiConfig.ContainsKey("schedule_work_night_category_list.nei"))
            {
                neiConfig["schedule_work_night_category_list.nei"] = Path.Combine(CustomScheduleEvents.modPath, "NIGHT_CAT");
            }
            string path = neiConfig["schedule_work_night_category_list.nei"];
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            ScheduleNightCategory newCategory = new ScheduleNightCategory();
            newCategory.ID = data.Id;
            newCategory.name = data.Name.Trim();
            File.WriteAllText(Path.Combine(path, newCategory.ID + ".json"), JsonConvert.SerializeObject(newCategory), System.Text.Encoding.UTF8);

            //Update Data
            ScheduleCSVData.TaskCategoryNameMap[newCategory.ID] = newCategory.name;

            resp.data = ("SUCCESS: Category Created!");
            resp.dataFormat = "string";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse saveEditCategory(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            NewCategoryJson data = JsonConvert.DeserializeObject<NewCategoryJson>(json);

            //Validations
            if (data.Id == 0)
            {
                resp.errorText = ("ERROR: Invalid Category ID");
                return resp;
            }
            if (data.Name.Trim().Equals(""))
            {
                resp.errorText = ("ERROR: Invalid Category Name");
                return resp;
            }
            if (!ScheduleCSVData.TaskCategoryNameMap.ContainsKey(data.Id))
            {
                resp.errorText = ("ERROR: Category ID does not exist");
                return resp;
            }

            //Write the file
            Dictionary<string, string> neiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(CustomScheduleEvents.configPath, "neiAppendConfig.json")));
            if (!neiConfig.ContainsKey("schedule_work_night_category_list.nei"))
            {
                neiConfig["schedule_work_night_category_list.nei"] = Path.Combine(CustomScheduleEvents.modPath, "NIGHT_CAT");
            }
            string path = neiConfig["schedule_work_night_category_list.nei"];
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            ScheduleNightCategory newCategory = new ScheduleNightCategory();
            newCategory.ID = data.Id;
            newCategory.name = data.Name.Trim();
            if (File.Exists(Path.Combine(path, newCategory.ID + ".json")))
            {
                File.Delete(Path.Combine(path, newCategory.ID + ".json"));
            }
            File.WriteAllText(Path.Combine(path, newCategory.ID + ".json"), JsonConvert.SerializeObject(newCategory), System.Text.Encoding.UTF8);

            //Update Data
            ScheduleCSVData.TaskCategoryNameMap[newCategory.ID] = newCategory.name;

            resp.data = ("SUCCESS: Category Changed!");
            resp.dataFormat = "string";
            return resp;
        }
        public class NewCategoryJson
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public NewCategoryJson()
            {
                Name = "";
            }
        }
        #endregion

        #region VIP
        public static SimpleHTTPServerPOSTResponse getScheduleVIPNormalTableData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            //Config
            List<int> configEnabled = CustomScheduleEvents.config.customEvents["vip"].Keys.ToList();

            //Night Files
            Dictionary<int, ScheduleWorkNightFile> nightFiles = _getNightEventFiles();

            //Night Enabled
            HashSet<int> nightEnabled = new HashSet<int>();
            wf.CsvCommonIdManager.ReadEnabledIdList(wf.CsvCommonIdManager.FileSystemType.Normal, true, "schedule_work_night_enabled", ref nightEnabled);

            List<List<string>> data = new List<List<string>>();
            foreach (KeyValuePair<int, ScheduleCSVData.Yotogi> yotogi in ScheduleCSVData.YotogiData)
            {
                //Not Rental
                if ((yotogi.Value.subMaidUnipueName == null || yotogi.Value.subMaidUnipueName.Trim().Equals("")) && nightEnabled.Contains(yotogi.Key))
                {
                    //In the MOD files
                    if (nightFiles.ContainsKey(yotogi.Key))
                    {
                        //In the config & has a Night_Enabled
                        if (configEnabled.Contains(yotogi.Key))
                        {
                            data.Add(new List<string>() { yotogi.Key.ToString().PadLeft(6, '0'),
                                            Translate(yotogi.Value.name),
                                            Translate((ScheduleCSVData.TaskCategoryNameMap.ContainsKey(yotogi.Value.categoryID))? ScheduleCSVData.TaskCategoryNameMap[yotogi.Value.categoryID] : ""),
                                            Translate(yotogi.Value.information),
                                            Translate("CustomScheduleEvents")
                            });
                        }
                    }
                    //KISS Event
                    else
                    {
                        data.Add(new List<string>() { yotogi.Key.ToString().PadLeft(6, '0'),
                                            Translate(yotogi.Value.name),
                                            Translate((ScheduleCSVData.TaskCategoryNameMap.ContainsKey(yotogi.Value.categoryID))? ScheduleCSVData.TaskCategoryNameMap[yotogi.Value.categoryID] : ""),
                                            Translate(yotogi.Value.information),
                                            Translate("KISS")
                        });
                    }
                }
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getScheduleVIPRentalTableData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            //Config
            List<int> configEnabled = CustomScheduleEvents.config.customEvents["vip"].Keys.ToList();

            //Night Files
            Dictionary<int, ScheduleWorkNightFile> nightFiles = _getNightEventFiles();

            //Night Enabled
            HashSet<int> nightEnabled = new HashSet<int>();
            wf.CsvCommonIdManager.ReadEnabledIdList(wf.CsvCommonIdManager.FileSystemType.Normal, true, "schedule_work_night_enabled", ref nightEnabled);

            List<List<string>> data = new List<List<string>>();
            foreach (KeyValuePair<int, ScheduleCSVData.Yotogi> yotogi in ScheduleCSVData.YotogiData)
            {
                //Not Rental
                if (!(yotogi.Value.subMaidUnipueName == null || yotogi.Value.subMaidUnipueName.Trim().Equals("")) && nightEnabled.Contains(yotogi.Key))
                {
                    //In the MOD files
                    if (nightFiles.ContainsKey(yotogi.Key))
                    {
                        //In the config & has a Night_Enabled
                        if (configEnabled.Contains(yotogi.Key))
                        {
                            data.Add(new List<string>() { yotogi.Key.ToString().PadLeft(6, '0'),
                                            Translate(yotogi.Value.name),
                                            Translate((ScheduleCSVData.TaskCategoryNameMap.ContainsKey(yotogi.Value.categoryID))? ScheduleCSVData.TaskCategoryNameMap[yotogi.Value.categoryID] : ""),
                                            Translate(yotogi.Value.information),
                                            Translate("CustomScheduleEvents")
                            });
                        }
                    }
                    //KISS Event
                    else
                    {
                        data.Add(new List<string>() { yotogi.Key.ToString().PadLeft(6, '0'),
                                            Translate(yotogi.Value.name),
                                            Translate((ScheduleCSVData.TaskCategoryNameMap.ContainsKey(yotogi.Value.categoryID))? ScheduleCSVData.TaskCategoryNameMap[yotogi.Value.categoryID] : ""),
                                            Translate(yotogi.Value.information),
                                            Translate("KISS")
                        });
                    }
                }
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getScheduleVIPImportTableData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            //Config
            List<int> configEnabled = CustomScheduleEvents.config.customEvents["vip"].Keys.ToList();

            //Night Files
            Dictionary<int, ScheduleWorkNightFile> nightFiles = _getNightEventFiles();

            //Night Enabled
            HashSet<int> nightEnabled = new HashSet<int>();
            wf.CsvCommonIdManager.ReadEnabledIdList(wf.CsvCommonIdManager.FileSystemType.Normal, true, "schedule_work_night_enabled", ref nightEnabled);

            List<List<string>> data = new List<List<string>>();
            foreach (KeyValuePair<int, ScheduleWorkNightFile> nightFile in nightFiles)
            {
                bool inYotogi = ScheduleCSVData.YotogiData.ContainsKey(nightFile.Key);
                bool inCSEConfig = configEnabled.Contains(nightFile.Key);
                bool hasNightEnabled = nightEnabled.Contains(nightFile.Key);

                //In the config & has a Night_Enabled
                if (!(inYotogi && hasNightEnabled && inCSEConfig))
                {
                    data.Add(new List<string>() { nightFile.Value.ID.ToString().PadLeft(6, '0'),
                                                  Translate(nightFile.Value.name),
                                                  Path.GetFileName(nightFile.Value.fileName),
                                                  (inYotogi)? "In YotogiData":"Missing from YotogiData",
                                                  (inCSEConfig)? "In CSE config":"Missing from CSE config",
                                                  (hasNightEnabled)? "Has NIGHT_ENABLED file":"Missing NIGHT_ENABLED file"
                    });
                }
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse editVIPGetDetails(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            EditVIPJson inputData = JsonConvert.DeserializeObject<EditVIPJson>(json);

            NewVIPJson data = new NewVIPJson();

            if (ScheduleCSVData.YotogiData.ContainsKey(inputData.Id))
            {
                ScheduleCSVData.Yotogi yotogi = ScheduleCSVData.YotogiData[inputData.Id];
                if (!CustomScheduleEvents.config.customEvents["vip"].ContainsKey(inputData.Id))
                {
                    resp.errorText = ("ERROR: Cannot Edit KISS Events");
                    return resp;
                }
                else
                {
                    //Basic
                    data.Id = yotogi.id;
                    data.Name = yotogi.name;
                    //data.comment = "";//this.comment;
                    data.Icon = yotogi.icon;
                    data.CategoryId = yotogi.categoryID;
                    //data.ntrFlag_obsolete = false;
                    //data.yotogiType = "Vip";
                    data.Description = yotogi.information;
                    data.Income = yotogi.income;
                    data.rental = yotogi.subMaidUnipueName != null && !yotogi.subMaidUnipueName.Trim().Equals("");

                    if (data.rental)
                    {
                        data.RentalMaidName = yotogi.subMaidUnipueName;
                    }
                    else
                    {
                        data.ImproveCustomerRelation = (yotogi.evaluation > 10);
                        data.EntertainedMaster = (yotogi.add_play_number > 0);
                        data.EntertainedGuest = (yotogi.add_other_play_number > 0);
                        data.RentalMaidName = "";
                    }

                    //Requirements
                    for (int i = 0; i < yotogi.condInfo.Count; i++)
                    {
                        if (yotogi.condInfo[i] != null && !yotogi.condInfo[i].Trim().Equals(""))
                        {
                            data.Hints.Add(yotogi.condInfo[i]);
                        }
                    }
                    data.SalonGrade = yotogi.condSalonGrade;

                    if (data.rental)
                    {
                        data.MainTrio = false;
                        data.Personality = new List<string>();

                        data.ContractFree = false;
                        data.ContractExclusive = false;
                        data.ContractTrainee = false;
                        data.HoleExpVirgin = false;
                        data.HoleExpVag = false;
                        data.HoleExpAnal = false;
                        data.HoleExpBoth = false;
                        data.Relationship = new List<string>();
                        data.Propensity = new List<string>();
                        data.JobClass = new List<string>();
                        data.NightClass = new List<string>();
                        //data.requiredSkillLevels = "";
                    }
                    else
                    {
                        data.MainTrio = yotogi.condMainChara;
                        data.Personality = new List<string>();
                        for (int i = 0; i < yotogi.condPersonal.Count; i++)
                        {
                            data.Personality.Add(MaidStatus.Personal.IdToUniqueName(yotogi.condPersonal[i]));
                        }

                        data.ContractFree = yotogi.condContract.Contains(MaidStatus.Contract.Free);
                        data.ContractExclusive = yotogi.condContract.Contains(MaidStatus.Contract.Exclusive);
                        data.ContractTrainee = yotogi.condContract.Contains(MaidStatus.Contract.Trainee);
                        data.HoleExpVirgin = yotogi.condSeikeiken.Contains(MaidStatus.Seikeiken.No_No);
                        data.HoleExpVag = yotogi.condSeikeiken.Contains(MaidStatus.Seikeiken.Yes_No);
                        data.HoleExpAnal = yotogi.condSeikeiken.Contains(MaidStatus.Seikeiken.No_Yes);
                        data.HoleExpBoth = yotogi.condSeikeiken.Contains(MaidStatus.Seikeiken.Yes_Yes);
                        data.Relationship = new List<string>();
                        for (int i = 0; i < yotogi.condRelation.Count; i++)
                        {
                            if (yotogi.condRelation[i] == MaidStatus.Relation.Contact)
                            {
                                data.Relationship.Add("Contract");
                            }
                            else if (yotogi.condRelation[i] == MaidStatus.Relation.Trust)
                            {
                                data.Relationship.Add("Trust");
                            }
                            else if (yotogi.condRelation[i] == MaidStatus.Relation.Lover)
                            {
                                data.Relationship.Add("Lover");
                            }
                        }
                        for (int i = 0; i < yotogi.condAdditionalRelation.Count; i++)
                        {
                            if (yotogi.condAdditionalRelation[i] == MaidStatus.AdditionalRelation.Vigilance)
                            {
                                data.Relationship.Add("Vigilance");
                            }
                            else if (yotogi.condAdditionalRelation[i] == MaidStatus.AdditionalRelation.LoverPlus)
                            {
                                data.Relationship.Add("LoverPlus");
                            }
                            else if (yotogi.condAdditionalRelation[i] == MaidStatus.AdditionalRelation.Slave)
                            {
                                data.Relationship.Add("Slave");
                            }
                        }
                        data.Propensity = new List<string>();
                        for (int i = 0; i < yotogi.condPropensity.Count; i++)
                        {
                            data.Propensity.Add(MaidStatus.Propensity.IdToUniqueName(yotogi.condPropensity[i]));
                        }
                        data.JobClass = new List<string>();
                        for (int i = 0; i < yotogi.condMaidClass.Count; i++)
                        {
                            data.JobClass.Add(MaidStatus.JobClass.IdToUniqueName(yotogi.condMaidClass[i]));
                        }
                        data.NightClass = new List<string>();
                        for (int i = 0; i < yotogi.condYotogiClass.Count; i++)
                        {
                            data.NightClass.Add(MaidStatus.YotogiClass.IdToUniqueName(yotogi.condYotogiClass[i]));
                        }
                    }

                    //Advanced
                    //data.manHasFlagDisplay = this.manHasFlagDisplay; //Condition to Display???
                    //data.manHasFlagExecute = this.manHasFlagExecute;
                    //data.manLacksFlagExecute = this.manLacksFlagExecute;

                    ////Advanced -- this means a maid (other than event maid) must be working at facility for correct day/night
                    //data.facilitiesInUse = string.Join("&", this.facilitiesInUse.Select(lst => string.Join("|", lst.ToArray())).ToArray()); //really ints of the IDs

                    ////Only 1 additional maid???
                    //data.additionalMaidPersonalityRequirement = string.Join("|", this.additionalMaidPersonalityRequirement.ToArray());
                    if (data.rental)
                    {
                        //data.maidHasFlag = "";
                        //data.maidLacksFlag = "";
                    }
                    else
                    {
                        //data.maidHasFlag = "";
                        //data.maidLacksFlag = "";
                    }
                }
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getScheduleCategoriesDropdownData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<GuestUIDropdownOption> data = new List<GuestUIDropdownOption>();
            foreach (KeyValuePair<int, string> taskCategoryName in ScheduleCSVData.TaskCategoryNameMap)
            {
                GuestUIDropdownOption option = new GuestUIDropdownOption();
                option.label = Translate(taskCategoryName.Value);
                option.value = taskCategoryName.Key.ToString();
                data.Add(option);
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getScheduleIconsDropdownData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<GuestUIDropdownOption> data = new List<GuestUIDropdownOption>();

            string[] scheduleIconsNames = new string[] {
            "schedule_icon_talk",
            "schedule_icon_bar",
            "schedule_icon_cafe",
            "schedule_icon_casino",
            "schedule_icon_choukyou",
            "schedule_icon_clothing_check",
            "schedule_icon_cookin_school",
            "schedule_icon_dance_lesson",
            "schedule_icon_gardening",
            "schedule_icon_gravure",
            "schedule_icon_hotel",
            "schedule_icon_kadou",
            "schedule_icon_lodgingroom",
            "schedule_icon_love",
            "schedule_icon_ntr",
            "schedule_icon_opera",
            "schedule_icon_piano",
            "schedule_icon_pooldanceroom",
            "schedule_icon_refle",
            "schedule_icon_restaurant",
            "schedule_icon_samigurumi",
            "schedule_icon_shisetsu",
            "schedule_icon_sm",
            "schedule_icon_soap",
            "schedule_icon_sommelier",
            "schedule_icon_voice_leson",
            "schedule_icon_yotogi_s",
            "schedule_icon_collabocafe",
            "schedule_icon_fantasyinn",
            "schedule_icon_gelaende",
            "schedule_icon_pool",
            "schedule_icon_seacafe",
            "schedule_icon_shrine",
            "scheduleicon_gp01yotogi_h",
            "scheduleicon_gp01yotogi_s",
            "scheduleicon_gp02yotogi_y",
            "schedule_icon_springgarden",
            "cm3d2_scheduleicon_esthe",
            "cm3d2_scheduleicon_kadou",
            "cm3d2_scheduleicon_kesyou",
            "cm3d2_scheduleicon_kyouyou",
            "cm3d2_scheduleicon_kyusoku",
            "cm3d2_scheduleicon_maidhaken",
            "cm3d2_scheduleicon_maidkensyuu",
            "cm3d2_scheduleicon_moyougae",
            "cm3d2_scheduleicon_ryouri",
            "cm3d2_scheduleicon_saihou",
            "cm3d2_scheduleicon_sekkyaku",
            "cm3d2_scheduleicon_sekkyakurensyuu",
            "cm3d2_scheduleicon_souji",
            "cm3d2_scheduleicon_vip",
            "cm3d2_scheduleicon_yotogi",
            "cm3d2_scheduleicon_help",
            "cm3d2_scheduleicon_kyujitsu",
            "cm3d2_scheduleicon_sinkonryokou",
            "cm3d2_scheduleicon_sinkonseikatu",
            "cm3d2_scheduleicon_jyunaivip",
            "cm3d2_scheduleicon_rentalmaidvip"
        };
            for (int i = 0; i < scheduleIconsNames.Length; i++)
            {
                string iconName = scheduleIconsNames[i];

                GuestUIDropdownOption option = new GuestUIDropdownOption();
                option.label = iconName;
                option.value = iconName;

                //Convert tex to png
                byte[] bytes = GameUty.FileSystem.FileOpen(iconName + ".tex").ReadAll();
                using (MemoryStream stream = new MemoryStream())
                {
                    //Write to stream then reset position
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Position = 0;

                    //Convert to png
                    option.img = TexTool.TexToImg(stream, iconName + ".tex");
                }
                option.imgType = "datapng";

                data.Add(option);
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getSchedulePersonalityMultiselectData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<GuestUIDropdownOption> data = new List<GuestUIDropdownOption>();
            List<MaidStatus.Personal.Data> personalities = MaidStatus.Personal.GetAllDatas(false);
            foreach (MaidStatus.Personal.Data personality in personalities)
            {
                GuestUIDropdownOption option = new GuestUIDropdownOption();
                option.label = Translate(personality.drawName);
                option.value = personality.uniqueName;
                data.Add(option);
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getSchedulePropensityMultiselectData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<GuestUIDropdownOption> data = new List<GuestUIDropdownOption>();
            List<MaidStatus.Propensity.Data> propensities = MaidStatus.Propensity.GetAllDatas(false);
            foreach (MaidStatus.Propensity.Data propensity in propensities)
            {
                GuestUIDropdownOption option = new GuestUIDropdownOption();
                option.label = Translate(propensity.drawName);
                option.value = propensity.uniqueName;
                data.Add(option);
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getScheduleJobClassMultiselectData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<GuestUIDropdownOption> data = new List<GuestUIDropdownOption>();
            List<MaidStatus.JobClass.Data> jobClasses = MaidStatus.JobClass.GetAllDatas(false);
            foreach (MaidStatus.JobClass.Data jobClass in jobClasses)
            {
                GuestUIDropdownOption option = new GuestUIDropdownOption();
                option.label = Translate(jobClass.drawName);
                option.value = jobClass.uniqueName;
                data.Add(option);
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getScheduleNightClassMultiselectData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<GuestUIDropdownOption> data = new List<GuestUIDropdownOption>();
            List<MaidStatus.YotogiClass.Data> nightClasses = MaidStatus.YotogiClass.GetAllDatas(false);
            foreach (MaidStatus.YotogiClass.Data nightClass in nightClasses)
            {
                GuestUIDropdownOption option = new GuestUIDropdownOption();
                option.label = Translate(nightClass.drawName);
                option.value = nightClass.uniqueName;
                data.Add(option);
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getScheduleRentalMaidsDropdownData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<GuestUIDropdownOption> data = new List<GuestUIDropdownOption>();
            List<MaidStatus.SubMaid.Data> rentalMaids = MaidStatus.SubMaid.GetAllDatas(false);
            foreach (MaidStatus.SubMaid.Data rentalMaid in rentalMaids)
            {
                GuestUIDropdownOption option = new GuestUIDropdownOption();
                option.label = Translate(rentalMaid.uniqueName);
                option.value = rentalMaid.uniqueName;
                data.Add(option);
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse saveNewVIP(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            NewVIPJson inputData = JsonConvert.DeserializeObject<NewVIPJson>(json);

            //Build
            ScheduleWorkNight data = _createVIPEvent(inputData);
            ScheduleWorkNightEnabled dataEnabled = _createVIPEventEnabled(inputData);

            //Validations
            {
                if (data.ID == 0)
                {
                    resp.errorText = ("ERROR: Invalid VIP ID");
                    return resp;
                }
                if (ScheduleCSVData.YotogiData.ContainsKey(data.ID))
                {
                    resp.errorText = ("ERROR: VIP ID Already Taken");
                    return resp;
                }
                if (data.name.Trim().Equals(""))
                {
                    resp.errorText = ("ERROR: Invalid VIP Name");
                    return resp;
                }
                if (data.icon.Trim().Equals(""))
                {
                    resp.errorText = ("ERROR: Invalid Icon Name");
                    return resp;
                }
                if (!ScheduleCSVData.TaskCategoryNameMap.ContainsKey(data.categoryId))
                {
                    resp.errorText = ("ERROR: Invalid Category ID");
                    return resp;
                }
                if (data.requiredSalonGrade < 0 || data.requiredSalonGrade > 5)
                {
                    resp.errorText = ("ERROR: Invalid Salon Grade (0-5)");
                    return resp;
                }
                if (data.maidIsMainTrio == 1)
                {
                    if (inputData.Personality.Count == 0)
                    {
                        resp.errorText = ("ERROR: Maid cannot be Main Trio with every Personality");
                        return resp;
                    }
                    foreach (string personality in inputData.Personality)
                    {
                        if (!((new string[] { "muku", "majime", "rindere" }).Contains(personality.Trim().ToLower())))
                        {
                            resp.errorText = ("ERROR: Maid cannot be Main Trio with Personality " + personality);
                            return resp;
                        }
                    }
                }
            }

            //NeiAppend
            {
                //Config
                Dictionary<string, string> neiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(CustomScheduleEvents.configPath, "neiAppendConfig.json")));

                //Nei File
                if (!neiConfig.ContainsKey("schedule_work_night.nei"))
                {
                    neiConfig["schedule_work_night.nei"] = Path.Combine(CustomScheduleEvents.modPath, "NIGHT");
                }
                string path = neiConfig["schedule_work_night.nei"];
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                File.WriteAllText(Path.Combine(path, data.ID + "_schedule_work_night.json"), JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8);

                //Enabled File
                if (!neiConfig.ContainsKey("schedule_work_night_enabled"))
                {
                    neiConfig["schedule_work_night_enabled"] = Path.Combine(CustomScheduleEvents.modPath, "NIGHT_ENABLED");
                }
                string pathEnabled = neiConfig["schedule_work_night_enabled"];
                if (!Directory.Exists(pathEnabled))
                {
                    Directory.CreateDirectory(pathEnabled);
                }
                File.WriteAllText(Path.Combine(pathEnabled, dataEnabled.ID + "_schedule_work_night_enabled.json"), JsonConvert.SerializeObject(dataEnabled), System.Text.Encoding.UTF8);
            }

            //Sample Script
            System.Resources.ResourceSet resourceSet = Properties.Resources.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
            foreach (System.Collections.DictionaryEntry entry in resourceSet)
            {
                //Write sample file
                if (new string[] { "CustomScriptSample" }.Contains(entry.Key.ToString()))
                {
                    string fileName = data.ID.ToString() + ".ks";
                    byte[] file = (byte[])entry.Value;

                    if (!Directory.Exists(Path.Combine(CustomScheduleEvents.modPath, "VIP_" + data.ID)))
                    {
                        Directory.CreateDirectory(Path.Combine(CustomScheduleEvents.modPath, "VIP_" + data.ID));
                    }

                    string path2 = Path.Combine(Path.Combine(CustomScheduleEvents.modPath, "VIP_" + data.ID), fileName);
                    if (File.Exists(path2))
                    {
                        File.Delete(path2);
                    }
                    File.WriteAllBytes(path2, file);
                }
            }

            //Update config
            CustomScheduleEvents.config.customEvents["vip"][data.ID] = data.ID.ToString() + ".ks";
            if (File.Exists(Path.Combine(CustomScheduleEvents.configPath, "config.json")))
            {
                File.Delete(Path.Combine(CustomScheduleEvents.configPath, "config.json"));
            }
            File.WriteAllText(Path.Combine(CustomScheduleEvents.configPath, "config.json"), Newtonsoft.Json.JsonConvert.SerializeObject(CustomScheduleEvents.config));
            CustomScheduleEvents.Refresh_custom_vip_events();

            //Success
            FieldInfo yDictInfo = AccessTools.Field(typeof(Schedule.ScheduleCSVData), "YotogiDataDic");
            Dictionary<int, ScheduleCSVData.Yotogi> yDict = (Dictionary<int, ScheduleCSVData.Yotogi>)yDictInfo.GetValue(null);
            yDict[data.ID] = data.convertToGameData();
            yDictInfo.SetValue(null, yDict);
            //MethodInfo cvsReadMI = AccessTools.Method("Schedule.ScheduleCSVData:CVSRead");
            //cvsReadMI.Invoke(null, null);
            FieldInfo yCacheInfo = AccessTools.Field(typeof(Schedule.ScheduleCSVData), "cacheAllDataData");
            yCacheInfo.SetValue(null, null);

            resp.data = ("SUCCESS: VIP Event Created!" +
                            "\nStarting sample script " + data.ID + ".ks has been added to your Mod folder." +
                            "\nEdit this file in a text editor capable of saving with Shift JIS encoding, but do not rename it.");
            resp.dataFormat = "string";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse saveEditVIP(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            NewVIPJson inputData = JsonConvert.DeserializeObject<NewVIPJson>(json);

            //Build
            ScheduleWorkNight data = _createVIPEvent(inputData);

            //Validations
            {
                if (data.ID == 0)
                {
                    resp.errorText = ("ERROR: Invalid VIP ID");
                    return resp;
                }
                if (!CustomScheduleEvents.config.customEvents["vip"].ContainsKey(data.ID))
                {
                    resp.errorText = ("ERROR: Cannot Edit KISS Events");
                    return resp;
                }
                if (!ScheduleCSVData.YotogiData.ContainsKey(data.ID))
                {
                    resp.errorText = ("ERROR: VIP ID does not exist");
                    return resp;
                }
                if (data.name.Trim().Equals(""))
                {
                    resp.errorText = ("ERROR: Invalid VIP Name");
                    return resp;
                }
                if (data.icon.Trim().Equals(""))
                {
                    resp.errorText = ("ERROR: Invalid Icon Name");
                    return resp;
                }
                if (!ScheduleCSVData.TaskCategoryNameMap.ContainsKey(data.categoryId))
                {
                    resp.errorText = ("ERROR: Invalid Category ID");
                    return resp;
                }
                if (data.requiredSalonGrade < 0 || data.requiredSalonGrade > 5)
                {
                    resp.errorText = ("ERROR: Invalid Salon Grade (0-5)");
                    return resp;
                }
                if (data.maidIsMainTrio == 1)
                {
                    if (inputData.Personality.Count == 0)
                    {
                        resp.errorText = ("ERROR: Maid cannot be Main Trio with every Personality");
                        return resp;
                    }
                    foreach (string personality in inputData.Personality)
                    {
                        if (!((new string[] { "muku", "majime", "rindere" }).Contains(personality.Trim().ToLower())))
                        {
                            resp.errorText = ("ERROR: Maid cannot be Main Trio with Personality " + personality);
                            return resp;
                        }
                    }
                }
            }

            //NeiAppend
            {
                //Config
                Dictionary<string, string> neiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(CustomScheduleEvents.configPath, "neiAppendConfig.json")));

                //Nei File
                if (!neiConfig.ContainsKey("schedule_work_night.nei"))
                {
                    neiConfig["schedule_work_night.nei"] = Path.Combine(CustomScheduleEvents.modPath, "NIGHT");
                }
                string path = neiConfig["schedule_work_night.nei"];
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (File.Exists(Path.Combine(path, data.ID + "_schedule_work_night.json")))
                {
                    File.Delete(Path.Combine(path, data.ID + "_schedule_work_night.json"));
                }
                File.WriteAllText(Path.Combine(path, data.ID + "_schedule_work_night.json"), JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8);
            }

            //Success
            FieldInfo yDictInfo = AccessTools.Field(typeof(Schedule.ScheduleCSVData), "YotogiDataDic");
            Dictionary<int, ScheduleCSVData.Yotogi> yDict = (Dictionary<int, ScheduleCSVData.Yotogi>)yDictInfo.GetValue(null);
            if (yDict.ContainsKey(data.ID))
            {
                yDict.Remove(data.ID);// = data.convertToGameData();
                yDictInfo.SetValue(null, yDict);
            }
            yDict[data.ID] = data.convertToGameData();
            yDictInfo.SetValue(null, yDict);
            //MethodInfo cvsReadMI = AccessTools.Method("Schedule.ScheduleCSVData:CVSRead");
            //cvsReadMI.Invoke(null, null);
            FieldInfo yCacheInfo = AccessTools.Field(typeof(Schedule.ScheduleCSVData), "cacheAllDataData");
            yCacheInfo.SetValue(null, null);


            resp.data = ("SUCCESS: VIP Event Edited!");
            resp.dataFormat = "string";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse importVIP(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            //Build
            ImportVIPJson data = JsonConvert.DeserializeObject<ImportVIPJson>(json);

            //Night Enabled
            HashSet<int> nightEnabled = new HashSet<int>();
            wf.CsvCommonIdManager.ReadEnabledIdList(wf.CsvCommonIdManager.FileSystemType.Normal, true, "schedule_work_night_enabled", ref nightEnabled);
            //if (!nightEnabled.Contains(data.Id))
            {
                //Config
                Dictionary<string, string> neiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(CustomScheduleEvents.configPath, "neiAppendConfig.json")));
                string path = neiConfig["schedule_work_night_enabled"];

                //Build
                ScheduleWorkNightEnabled dataEnabled = new ScheduleWorkNightEnabled() { ID = data.Id, name = data.Name };
                if (!File.Exists(Path.Combine(path, dataEnabled.ID + "_schedule_work_night_enabled.json")))
                {
                    File.WriteAllText(Path.Combine(path, dataEnabled.ID + "_schedule_work_night_enabled.json"), JsonConvert.SerializeObject(dataEnabled), System.Text.Encoding.UTF8);
                }
            }

            //Update Config
            List<int> configEnabled = CustomScheduleEvents.config.customEvents["vip"].Keys.ToList();
            //if (!configEnabled.Contains(data.Id))
            {
                CustomScheduleEvents.config.customEvents["vip"][data.Id] = data.Id.ToString() + ".ks";
                if (File.Exists(Path.Combine(CustomScheduleEvents.configPath, "config.json")))
                {
                    File.Delete(Path.Combine(CustomScheduleEvents.configPath, "config.json"));
                }
                File.WriteAllText(Path.Combine(CustomScheduleEvents.configPath, "config.json"), Newtonsoft.Json.JsonConvert.SerializeObject(CustomScheduleEvents.config));
                CustomScheduleEvents.Refresh_custom_vip_events();
            }

            //Update game data
            //if (!ScheduleCSVData.YotogiData.ContainsKey(data.Id))
            {
                //Config
                Dictionary<string, string> neiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(CustomScheduleEvents.configPath, "neiAppendConfig.json")));
                string path = neiConfig["schedule_work_night.nei"];
                ScheduleWorkNight nightEvent = JsonConvert.DeserializeObject<ScheduleWorkNight>(File.ReadAllText(Path.Combine(path, data.Id.ToString() + "_schedule_work_night.json")));

                FieldInfo yDictInfo = AccessTools.Field(typeof(Schedule.ScheduleCSVData), "YotogiDataDic");
                Dictionary<int, ScheduleCSVData.Yotogi> yDict = (Dictionary<int, ScheduleCSVData.Yotogi>)yDictInfo.GetValue(null);
                yDict[nightEvent.ID] = nightEvent.convertToGameData();
                yDictInfo.SetValue(null, yDict);
                //MethodInfo cvsReadMI = AccessTools.Method("Schedule.ScheduleCSVData:CVSRead");
                //cvsReadMI.Invoke(null, null);
                FieldInfo yCacheInfo = AccessTools.Field(typeof(Schedule.ScheduleCSVData), "cacheAllDataData");
                yCacheInfo.SetValue(null, null);
            }

            //Success
            resp.data = ("SUCCESS: VIP Event Imported!");
            resp.dataFormat = "string";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse deleteVIP(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            EditVIPJson data = JsonConvert.DeserializeObject<EditVIPJson>(json);

            if (!CustomScheduleEvents.config.customEvents["vip"].ContainsKey(data.Id))
            {
                resp.errorText = ("ERROR: Cannot Delete KISS Events");
                return resp;
            }

            //Update config
            CustomScheduleEvents.config.customEvents["vip"].Remove(data.Id);
            if (File.Exists(Path.Combine(CustomScheduleEvents.configPath, "config.json")))
            {
                File.Delete(Path.Combine(CustomScheduleEvents.configPath, "config.json"));
            }
            File.WriteAllText(Path.Combine(CustomScheduleEvents.configPath, "config.json"), Newtonsoft.Json.JsonConvert.SerializeObject(CustomScheduleEvents.config));

            //Update data
            if (ScheduleCSVData.YotogiData.ContainsKey(data.Id))
            {
                FieldInfo yDictInfo = AccessTools.Field(typeof(Schedule.ScheduleCSVData), "YotogiDataDic");
                Dictionary<int, ScheduleCSVData.Yotogi> yDict = (Dictionary<int, ScheduleCSVData.Yotogi>)yDictInfo.GetValue(null);
                yDict.Remove(data.Id);
                yDictInfo.SetValue(null, yDict);
                //MethodInfo cvsReadMI = AccessTools.Method("Schedule.ScheduleCSVData:CVSRead");
                //cvsReadMI.Invoke(null, null);
                FieldInfo yCacheInfo = AccessTools.Field(typeof(Schedule.ScheduleCSVData), "cacheAllDataData");
                yCacheInfo.SetValue(null, null);
            }

            //Success
            resp.data = ("SUCCESS: VIP Event Deleted!");
            resp.dataFormat = "string";
            return resp;
        }
        private static Dictionary<int, ScheduleWorkNightFile> _getNightEventFiles()
        {
            Dictionary<int, ScheduleWorkNightFile> data = new Dictionary<int, ScheduleWorkNightFile>();

            //Verify config
            Dictionary<string, string> neiConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(CustomScheduleEvents.configPath, "neiAppendConfig.json")));
            if (!neiConfig.ContainsKey("schedule_work_night.nei"))
            {
                neiConfig["schedule_work_night.nei"] = Path.Combine(CustomScheduleEvents.modPath, "NIGHT");
            }
            string path = neiConfig["schedule_work_night.nei"];
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //Get Files
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = files[i];
                if (fileName.EndsWith("_schedule_work_night.json"))
                {
                    try
                    {
                        ScheduleWorkNightFile nightEvent = JsonConvert.DeserializeObject<ScheduleWorkNightFile>(System.IO.File.ReadAllText(fileName));
                        nightEvent.fileName = fileName;
                        data[nightEvent.ID] = nightEvent;
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.ToString());
                    }
                }
            }

            return data;
        }
        private static ScheduleWorkNight _createVIPEvent(NewVIPJson json)
        {
            ScheduleWorkNight data = new ScheduleWorkNight();

            //Basic
            data.ID = json.Id;
            data.name = json.Name;
            data.comment = "";//this.comment;
            data.icon = json.Icon;
            data.categoryId = json.CategoryId;
            data.ntrFlag_obsolete = false;
            data.yotogiType = "Vip";
            data.descriptionText = json.Description;
            data.incomeBase = json.Income;
            data.isRentalMaid = "";

            if (json.rental)
            {
                _createVIPEvent_basic_rental(json, data);
            }
            else
            {
                _createVIPEvent_basic_normal(json, data);
            }

            //Requirements
            data.requirementText1 = (json.Hints.Count > 0) ? json.Hints[0] : "";
            data.requirementText2 = (json.Hints.Count > 1) ? json.Hints[1] : "";
            data.requirementText3 = (json.Hints.Count > 2) ? json.Hints[2] : "";
            data.requirementText4 = (json.Hints.Count > 3) ? json.Hints[3] : "";
            data.requirementText5 = (json.Hints.Count > 4) ? json.Hints[4] : "";
            data.requirementText6 = (json.Hints.Count > 5) ? json.Hints[5] : "";
            data.requirementText7 = (json.Hints.Count > 6) ? json.Hints[6] : "";
            data.requirementText8 = (json.Hints.Count > 7) ? json.Hints[7] : "";
            data.requirementText9 = (json.Hints.Count > 8) ? json.Hints[8] : "";

            data.requiredSalonGrade = json.SalonGrade;

            if (json.rental)
            {
                _createVIPEvent_requirements_rental(json, data);
            }
            else
            {
                _createVIPEvent_requirements_normal(json, data);
            }

            //Advanced
            data.manHasFlagDisplay = "";
            data.manHasFlagExecute = "";
            data.manLacksFlagExecute = "";
            //data.manHasFlagDisplay = this.manHasFlagDisplay; //Condition to Display???
            //data.manHasFlagExecute = this.manHasFlagExecute;
            //data.manLacksFlagExecute = this.manLacksFlagExecute;

            data.facilitiesInUse = "";
            ////Advanced -- this means a maid (other than event maid) must be working at facility for correct day/night
            //data.facilitiesInUse = string.Join("&", this.facilitiesInUse.Select(lst => string.Join("|", lst.ToArray())).ToArray()); //really ints of the IDs

            data.additionalMaidPersonalityRequirement = "";
            ////Only 1 additional maid???
            //data.additionalMaidPersonalityRequirement = string.Join("|", this.additionalMaidPersonalityRequirement.ToArray());
            if (json.rental)
            {
                _createVIPEvent_advanced_normal(json, data);
            }
            else
            {
                _createVIPEvent_advanced_rental(json, data);
            }

            //No clue on these, something to do with CRE body
            data.isCheckGP002Personal = "";
            data.bodyMatchCheck = "";
            data.bodyMatchBlock = "";
            data.bodyMatchCheckWithMainCharacter = "";

            return data;
        }
        private static void _createVIPEvent_basic_normal(NewVIPJson json, ScheduleWorkNight data)
        {
            data.evaluationBase = (json.ImproveCustomerRelation) ? 10 : 0;
            data.countAsMasterEntertained = json.EntertainedMaster ? 1 : 0;
            data.countAsGuestsEntertained = json.EntertainedGuest ? 1 : 0;
            data.rentalMaidName = "";
        }
        private static void _createVIPEvent_basic_rental(NewVIPJson json, ScheduleWorkNight data)
        {
            data.evaluationBase = 0;
            data.countAsMasterEntertained = 0;
            data.countAsGuestsEntertained = 0;
            data.rentalMaidName = json.RentalMaidName;
        }
        private static void _createVIPEvent_requirements_normal(NewVIPJson json, ScheduleWorkNight data)
        {
            data.maidIsMainTrio = (json.MainTrio) ? 1 : 0;
            data.maidPersonality = string.Join("|", json.Personality.ToArray());

            data.requiredContracts = ((json.ContractFree) ? "|Free" : "") + ((json.ContractExclusive) ? "|Exclusive" : "") + ((json.ContractTrainee) ? "|Trainee" : "");
            data.requiredHoleExperience = ((json.HoleExpVirgin) ? "|No_No" : "") + ((json.HoleExpVag) ? "|Yes_No" : "") + ((json.HoleExpAnal) ? "|No_Yes" : "") + ((json.HoleExpBoth) ? "|Yes_Yes" : "");
            data.maidRelationship = string.Join("|", json.Relationship.ToArray());

            data.requiredPropensity = string.Join("|", json.Propensity.ToArray());
            data.requiredMaidClass = string.Join("&", json.JobClass.ToArray());
            //foreach (string maidClass in this.requiredMaidClass)
            //{
            //    if(!data.requiredMaidClass.Equals(""))
            //    {
            //        data.requiredMaidClass += "&";
            //    }
            //    data.requiredMaidClass += maidClass;
            //}
            data.requiredYotogiClass = string.Join("&", json.NightClass.ToArray());

            data.requiredSkillLevels = "";
            //data.requiredSkillLevels = string.Join("&", (this.requiredSkillMinLevel.Select(kvp => string.Format("@{0},{1}", kvp.Key, Math.Min(3, Math.Max(0, kvp.Value))))).ToArray());

            //foreach(KeyValuePair<int, int> kvp in this.requiredSkillMinLevel)
            //{
            //    if(!data.requiredSkillLevels.Equals(""))
            //    {
            //        data.requiredSkillLevels += "&";
            //    }
            //    data.requiredSkillLevels +=(kvp.Key + "," + Math.Min(3, Math.Max(0, kvp.Value)));
            //}
        }
        private static void _createVIPEvent_requirements_rental(NewVIPJson json, ScheduleWorkNight data)
        {
            data.maidIsMainTrio = 0;
            data.maidPersonality = "";

            data.requiredContracts = "";
            data.requiredHoleExperience = "";
            data.maidRelationship = "";
            data.requiredPropensity = "";
            data.requiredMaidClass = "";
            data.requiredYotogiClass = "";
            data.requiredSkillLevels = "";
        }
        private static void _createVIPEvent_advanced_normal(NewVIPJson json, ScheduleWorkNight data)
        {
            //data.maidHasFlag = string.Join("&", this.maidHasFlag.ToArray());
            //data.maidLacksFlag = string.Join("&", this.maidLacksFlag.ToArray());
        }
        private static void _createVIPEvent_advanced_rental(NewVIPJson json, ScheduleWorkNight data)
        {
            data.maidHasFlag = "";
            data.maidLacksFlag = "";
        }
        private static ScheduleWorkNightEnabled _createVIPEventEnabled(NewVIPJson json)
        {
            ScheduleWorkNightEnabled data = new ScheduleWorkNightEnabled();
            data.ID = json.Id;
            data.name = json.Name;
            return data;
        }
        public class NewVIPJson
        {
            public bool rental { get; set; }

            //Shared
            public int Id { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public int CategoryId { get; set; }
            public string Description { get; set; }
            public int Income { get; set; }
            public List<string> Hints { get; set; }
            public int SalonGrade { get; set; }

            //Basic-Normal
            public bool ImproveCustomerRelation { get; set; }
            public bool EntertainedMaster { get; set; }
            public bool EntertainedGuest { get; set; }

            //Basic-Rental
            public string RentalMaidName { get; set; }

            //Requirements-Normal
            public bool MainTrio { get; set; }
            public List<string> Personality { get; set; }
            public bool ContractFree { get; set; }
            public bool ContractExclusive { get; set; }
            public bool ContractTrainee { get; set; }
            public bool HoleExpVirgin { get; set; }
            public bool HoleExpVag { get; set; }
            public bool HoleExpAnal { get; set; }
            public bool HoleExpBoth { get; set; }
            public List<string> Relationship { get; set; }
            public List<string> Propensity { get; set; }
            public List<string> JobClass { get; set; }
            public List<string> NightClass { get; set; }

            //Requirements-Rental

            //Advanced-Normal

            //Advanced-Rental

            public NewVIPJson()
            {
                Hints = new List<string>();
                Personality = new List<string>();
                Relationship = new List<string>();
                Propensity = new List<string>();
                JobClass = new List<string>();
                NightClass = new List<string>();
            }
        }
        public class EditVIPJson
        {
            public int Id { get; set; }

            public EditVIPJson()
            {
            }
        }
        public class ImportVIPJson
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ImportVIPJson()
            {
            }
        }
        public class ScheduleWorkNightFile : ScheduleWorkNight
        {
            public string fileName { get; set; }
            public ScheduleWorkNightFile() : base()
            {
                fileName = "";
            }
        }
        #endregion

        #region ScriptTest

        public static SimpleHTTPServerPOSTResponse ScriptTestTableFavoriteUpdate(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            TableFavoriteJson data = JsonConvert.DeserializeObject<TableFavoriteJson>(json);

            if(data.status == 0)
            {
                CustomScheduleEvents.config.webData.resetFavorite(data.table, data.rowId);
            }
            else if(data.status == 1)
            {
                CustomScheduleEvents.config.webData.addFavorite(data.table, data.rowId);
            }
            else
            {
                CustomScheduleEvents.config.webData.hideFavorite(data.table, data.rowId);
            }
            if (File.Exists(Path.Combine(CustomScheduleEvents.configPath, "config.json")))
            {
                File.Delete(Path.Combine(CustomScheduleEvents.configPath, "config.json"));
            }
            File.WriteAllText(Path.Combine(CustomScheduleEvents.configPath, "config.json"), Newtonsoft.Json.JsonConvert.SerializeObject(CustomScheduleEvents.config));

            return resp;
        }
        public class TableFavoriteJson
        {
            public string table { get; set; }
            public string rowId { get; set; }
            public int status { get; set; }

            public TableFavoriteJson()
            {
                table = "";
                rowId = "";
                status = 0;
            }
        }

        #region MotionScript
        public static SimpleHTTPServerPOSTResponse getScriptHelpersMotionScriptFileDropdownData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<GuestUIDropdownOption> data = new List<GuestUIDropdownOption>();

            //Load data
            foreach(KeyValuePair<string, List<string>> kvp in ScriptAnalysis.MotionScripts)
            {
                string file = kvp.Key;
                GuestUIDropdownOption option = new GuestUIDropdownOption();
                option.label = file;
                option.value = file;
                data.Add(option);
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getScriptHelpersMotionScriptLabelDropdownData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<GuestUIDropdownOption> data = new List<GuestUIDropdownOption>();

            //Load data
            foreach(KeyValuePair<string, List<string>> kvp in ScriptAnalysis.MotionScripts)
            {
                string file = kvp.Key;
                for(int j=0; j < ScriptAnalysis.MotionScripts[file].Count; j++)
                {
                    string label = ScriptAnalysis.MotionScripts[file][j];
                    GuestUIDropdownOption option = new GuestUIDropdownOption();
                    option.label = label + ":" + Translate(label);
                    option.value = file + "|" + label;
                    data.Add(option);
                }
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse getScriptHelpersMotionScriptTableData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<List<string>> data = new List<List<string>>();
            foreach(KeyValuePair<string, List<string>> kvp in ScriptAnalysis.MotionScripts)
            {
                string fileOriginal = kvp.Key;
                string file = fileOriginal.EndsWith(".ks") ? fileOriginal.Split('.')[0] : fileOriginal;

                for (int j = 0; j < ScriptAnalysis.MotionScripts[fileOriginal].Count; j++)
                {
                    string label = ScriptAnalysis.MotionScripts[fileOriginal][j];
                    string favorite = ((int)CustomScheduleEvents.config.webData.getFavorite("motionscript", file + "_" + label)).ToString().Trim();

                    bool crc = checkForTag("crc", file);
                    bool customcast = checkForTag("cas", file);
                    bool test = checkForTag("test", file);
                    bool ck = checkForTag("ck", file);

                    bool pose = checkForTag("edit_pose", file);
                    bool work = checkForTag("work", file) | checkForTag("workff", file) | checkForTag("workfm", file);
                    bool dance = checkForTag("dance", file);
                    bool h = checkForTag("h", file) | checkForTag("sex", file);

                    bool conversation = checkForTag("kaiwa", file) | checkForTag("kaiwac", file) | checkForTag("kaiwar", file);
                    bool evnt = checkForTag("event", file);
                    bool gp = checkForTag("gp2", file) | checkForTag("gp3", file);

                    bool maid = checkForTag("maid", file);
                    bool man = checkForTag("man", file);

                    string[] fileSplit = file.Split('_');
                    string fileTranslated = "";
                    Dictionary<string, string> fixedSexTranslations = new Dictionary<string, string>() {
                        { "(aibu)", "caress" },
                        { "(kouhaii)", "doggystyle" },
                        { "(taimenkijyoui)", "face-to-face cowgirl" },
                        { "(tati)", "standing" },
                        { "(tekoki)", "handjob" },
                        { "2ana", "double penetration"},
                        { "2vibe", "double vibe"},
                        { "3ana", "triple penetration"},
                        { "3p", "threeway/threesome"},
                        { "6p", "sixway/sixsome"},
                        { "69", "sixty-nine/69"},
                        { "aibu", "caress" },
                        { "aibu2", "caress" },
                        { "aibu1", "caress" },
                        { "analo", "anal onahole ?" },
                        { "arai", "wash/bath"},
                        { "arai2", "wash/bath"},
                        { "asikoki", "footjob" },
                        { "asikoki2", "footjob" },
                        { "asikoki3", "footjob" },
                        { "asiname", "foot-licking"},
                        { "b", "bukkake"},
                        { "ba", "bukkake"},
                        { "baramuti", "rose whip"},
                        { "bg", "bridge"},
                        { "c", "change"},
                        { "dai", "table"},
                        { "daijyou", "tabletop"},
                        { "denma", "electric-wand"},
                        { "dl", "domination-loss?"},
                        { "dla", "domination-loss ? anal"},
                        { "dt", "deepthroat"},
                        { "ekiben", "standing-legs-wrapped"},
                        { "ekibena", "standing-legs-wrapped anal"},
                        { "fera", "oral/fellatio" },
                        { "furo", "wash/bath"},
                        { "ganmenkijyoui", "face-sitting"},
                        { "haimen", "reverse"},
                        { "haimenekiben", "reverse standing-legs-wrapped"},
                        { "haimenekibena", "reverse standing-legs-wrapped anal"},
                        { "haimenkijyoui", "reverse cowgirl"},
                        { "haimenkijyouia", "reverse cowgirl anal"},
                        { "haimenritui", "reverse standing"},
                        { "haimenritui2", "reverse standing"},
                        { "haimenritui2a", "reverse standing anal"},
                        { "haimenrituia", "reverse standing anal"},
                        { "haimenzai", "reverse sitting"},
                        { "haimenzai2", "reverse sitting"},
                        { "haimenzai2a", "reverse sitting anal"},
                        { "haimenzaia", "reverse sitting anal"},
                        { "hanyou", "general"},
                        { "harituke", "crucifixion bondage"},
                        { "hasamikomi", "sandwiched"},
                        { "hekimen", "wall"},
                        { "hizadati", "kneeling"},
                        { "housi", "service"},
                        { "in", "insertion"},
                        { "inu", "dog"},
                        { "ir", "deepthroat" },
                        { "ir2", "deepthroat" },
                        { "ir2v", "deepthroat vibrator" },
                        { "ira", "deepthroat anal" },
                        { "irruma", "deepthroat"},
                        { "irruma2", "deepthroat"},
                        { "isu", "chair"},
                        { "isutaimenzai", "chair face-to-face sitting"},
                        { "isutaimenzaia", "chair face-to-face sitting anal"},
                        { "itya", "flirting"},
                        { "jyouou", "queen"},
                        { "kaiawase", "tribadism"},
                        { "kaikyaku", "open-legs"},
                        { "kakae", "raised ?"},
                        { "kakaekomizai", "raised sitting ?"},
                        { "kakaekomizaia", "raised sitting anal ?"},
                        { "kakaemzi", "raised open-legs ?"},
                        { "kasane", "pile" },
                        { "kijyoui", "cowgirl"},
                        { "kijyoui2", "cowgirl"},
                        { "kijyoui2a", "cowgirl anal"},
                        { "kijyouia", "cowgirl anal"},
                        { "kimesex", "?"},
                        { "kiss", "kiss"},
                        { "kosikake", "sitting"},
                        { "kouhaii", "doggystyle" },
                        { "kouhaii2", "doggystyle" },
                        { "kouhaii2a", "doggystyle anal" },
                        { "kouhaiia", "doggystyle anal" },
                        { "kousoku", "restraint" },
                        { "kousokudai", "restraint standing" },
                        { "kousokui", "straitjacket"},
                        { "kousokuia", "straitjacket anal" },
                        { "kubisime", "choking" },
                        { "kubisimekyou", "choking rough" },
                        { "kunni", "cunnilingus" },
                        { "kunni2", "cunnilingus" },
                        { "kuti", "oral/fellatio"},
                        { "kutia", "oral/fellatio anal"},
                        { "kyousitu", "classroom"},
                        { "m", "masochist ?"},
                        { "manguri", "piledriver"},
                        { "manguri2", "piledriver"},
                        { "manguri2a", "piledriver anal"},
                        { "manguri3", "piledriver"},
                        { "manguri3a", "piledriver anal"},
                        { "manguria", "piledriver anal"},
                        { "matuba", "masturbation"},
                        { "matubaa", "masturbation anal"},
                        { "misetuke", "exhibition"},
                        { "mittyaku", "hold-tight"},
                        { "mode", "mode"},
                        { "mokuba", "wooden horse"},
                        { "mp", "mat play"},
                        { "mp2", "mat play"},
                        { "muri", "hesitant/forced"},
                        { "mzi", "open-legs"},
                        { "name", "licking"},
                        { "naziri", "edging"},
                        { "nefera", "sleeping oral/fellatio"},
                        { "om", "order-made ?" },
                        { "onahokoki", "onahole"},
                        { "onani", "masturbation" },
                        { "onani2", "masturbation"},
                        { "onani2a", "masturbation anal"},
                        { "onani3", "masturbation"},
                        { "onania", "masturbation anal"},
                        { "osae", "holding-tight/pinned"},
                        { "osaetuke", "holding-tight/pinned"},
                        { "ositaosi", "holding-tight/pinned"},
                        { "paizuri", "titjob/boobjob" },
                        { "paizuri2", "titjob/boobjob" },
                        { "pantskoki", "panties handjob"},
                        { "pose", "pose"},
                        { "poseizi", "pose groping"},
                        { "poseizi2", "pose groping"},
                        { "ran3p", "orgy threeway/threesome"},
                        { "ran4p", "orgy fourway/foursome"},
                        { "ritui", "standing"},
                        { "ritui2", "standing"},
                        { "ritui2a", "standing anal"},
                        { "rituia", "standing anal"},
                        { "rosyutu", "exhibition"},
                        { "s2", "s2"},
                        { "seijyoui", "missionary"},
                        { "seijyoui2", "missionary"},
                        { "seijyoui2a", "missionary anal"},
                        { "seijyouia", "missionary anal"},
                        { "self", "self"},
                        { "senboukyou", "underwater"},
                        { "settai", "service"},
                        { "sex", "sex"},
                        { "sexa", "anal"},
                        { "sexsofa", "sofa"},
                        { "siriage", "raised-butt"},
                        { "siriname", "rimjob"},
                        { "siruo", "ejaculation ?"},
                        { "sixnine", "sixty-nine/69"},
                        { "sofa", "sofa"},
                        { "sokui", "sideways"},
                        { "sokui2", "sideways"},
                        { "sokui2a", "sideways anal"},
                        { "sokui3", "sideways"},
                        { "sokui3a", "sideways"},
                        { "sokuia", "sideways anal"},
                        { "soutouvibe", "double-ended dildo"},
                        { "soutouvibe2", "double-ended dildo"},
                        { "sukebeisu", "bath stool"},
                        { "sumata", "bath mat"},
                        { "syumoku", "suplex ?"},
                        { "syumokua", "suplex ? anal"},
                        { "table", "table"},
                        { "taimen", "face-to-face"},
                        { "taimenekiben", "face-to-face standing-legs-wrapped"},
                        { "taimenekibena", "face-to-face standing-legs-wrapped anal"},
                        { "taimenkijyoui", "face-to-face cowgirl"},
                        { "taimenkijyoui2", "face-to-face cowgirl"},
                        { "taimenkijyoui2a", "face-to-face cowgirl anal"},
                        { "taimenkijyoui3", "face-to-face cowgirl"},
                        { "taimenkijyoui3a", "face-to-face cowgirl anal"},
                        { "taimenkijyouia", "face-to-face cowgirl anal"},
                        { "taimenzai", "face-to-face sitting"},
                        { "taimenzai2", "face-to-face sitting"},
                        { "taimenzai2a", "face-to-face sitting anal"},
                        { "taimenzai3", "face-to-face sitting"},
                        { "taimenzai3a", "face-to-face sitting anal"},
                        { "taimenzaia", "face-to-face sitting anal"},
                        { "tati", "standing"},
                        { "tekoki", "handjob"},
                        { "tekoki2", "handjob"},
                        { "tekoki3", "handjob"},
                        { "tekoki4", "handjob"},
                        { "tekokio", "handjob onahole"},
                        { "tekoona", "handjob onahole"},
                        { "tetunagi", "restraint"},
                        { "tikan", "molest"},
                        { "tinguri", "piledriver"},
                        { "tinguria", "piledriver anal"},
                        { "toilet", "toilet"},
                        { "turusi", "hanging"},
                        { "ude", "arm"},
                        { "udemoti", "arm restraint"},
                        { "uma", "straddling"},
                        { "umanori", "straddling"},
                        { "utubuse", "downward"},
                        { "vibe", "vibrator"},
                        { "vibe2", "vibrator"},
                        { "vibe2a", "vibrator anal"},
                        { "vibea", "vibrator anal"},
                        { "vr", "virtual reality"},
                        { "waki", "armpit"},
                        { "wasikoki", "double footjob"},
                        { "wfera", "double oral/fellatio"},
                        { "x", "x"},
                        { "yorisoi", "snuggling"},
                        { "yotunbai", "on-hand-and-feet/on-all-fours"},
                        { "yukadon", "lying-down"},
                        { "yuri", "lesbian"}
                    };
                    for(int k=0; k<fileSplit.Length; k++)
                    {
                        if (k != 0)
                        {
                            fileTranslated += " ";
                        }
                        string str = fileSplit[k];
                        if(fixedSexTranslations.ContainsKey(str))
                        {
                            fileTranslated += fixedSexTranslations[str];
                        }
                        else
                        {
                            fileTranslated += Translate(str);
                        }
                    }

                    //Row Contents
                    List<string> row = new List<string>();
                    row.Add(file);
                    row.Add(fileTranslated);
                    row.Add(label);
                    row.Add(Translate(label));
                    row.Add(favorite);

                    if (!ck && !crc)
                    {
                        data.Add(row);
                    }
                }
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        private static bool checkForTag(string tag, string file)
        {
            return file.StartsWith(tag + "_") || file.EndsWith("_" + tag) || file.Contains("_" + tag + "_");
        }
        public static SimpleHTTPServerPOSTResponse MotionScriptTest(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            MotionScriptTestJson data = JsonConvert.DeserializeObject<MotionScriptTestJson>(json);

            if (data.File != null && !data.File.Trim().Equals("") && data.Label != null && !data.Label.Trim().Equals(""))
            {
                GameMain.Instance.ScriptMgr.adv_kag.LoadScriptFile(data.File + ".ks", data.Label);
                GameMain.Instance.ScriptMgr.adv_kag.Exec();
            }

            return resp;
        }

        public class MotionScriptTestJson
        {
            public string File { get; set; }
            public string Label { get; set; }

            public MotionScriptTestJson()
            {
                File = "";
                Label = "";
            }
        }
        #endregion

        #region Motion
        public static SimpleHTTPServerPOSTResponse getScriptHelpersMotionTableData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if(!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<List<string>> data = new List<List<string>>();
            foreach(KeyValuePair<string, string[]> kvp in ScriptTag.Motion.results["mot"])
            {
                string animation = kvp.Key;
                bool male = animation.EndsWith("_m") || animation.EndsWith("_m1") || animation.EndsWith("_m2") || animation.EndsWith("_m3") || animation.EndsWith("_m4") || animation.EndsWith("_m5")
                         || animation.EndsWith("_m_once_") || animation.EndsWith("_m1_once_") || animation.EndsWith("_m2_once_") || animation.EndsWith("_m3_once_") || animation.EndsWith("_m4_once_") || animation.EndsWith("_m5_once_");
                bool crc = animation.StartsWith("crc_");
                string example = ScriptTag.Motion.results["mot"][animation][0];
                string examplePath = ScriptTag.Motion.results["mot"][animation][1];
                string arcPath = ScriptTag.Motion.results["mot"][animation][2];
                string game = arcPath.StartsWith(GameMain.Instance.CMSystem.CM3D2Path) ? "CM3D2" : "COM3D2";
                string favorite = ((int)CustomScheduleEvents.config.webData.getFavorite("motion", game + "_" + animation)).ToString().Trim();

                //Row Contents
                List<string> row = new List<string>();
                row.Add(animation);
                row.Add(male.ToString().ToUpper());
                row.Add(examplePath);
                row.Add(example);
                row.Add(game);
                row.Add(favorite);

                if (!crc)
                {
                    data.Add(row);
                }
            }
            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse MotionTest(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            MotionTestJson data = JsonConvert.DeserializeObject<MotionTestJson>(json);

            if (data.Maid != -1 && data.Mot != null && !data.Mot.Trim().Equals(""))
            {
                Maid maid = (data.Man) ? GameMain.Instance.CharacterMgr.GetMan(data.Maid) : GameMain.Instance.CharacterMgr.GetMaid(data.Maid);
                
                string play_motion_name = data.Mot + ".anm";
                float blend = GameUty.MillisecondToSecond(data.Blend);
                float weight = GameUty.MillisecondToSecond(data.Weight);

                if (!(maid == null || maid.body0 == null || (maid.body0.m_Bones == null || maid.IsBusy)))
                {
                    if (!GameMain.Instance.ScriptMgr.is_motion_blend)
                    {
                        maid.body0.motionBlendTime = 0.0f;
                        maid.fullBodyIK.bodyOffsetCtrl.blendTime = blend;
                        maid.body0.CrossFade(play_motion_name, GameMain.Instance.ScriptMgr.file_system, data.Additive, data.Loop, data.Queue, 0.0f, weight);
                    }
                    else
                    {
                        maid.body0.motionBlendTime = blend;
                        maid.body0.CrossFade(play_motion_name, GameMain.Instance.ScriptMgr.file_system, data.Additive, data.Loop, data.Queue, blend, weight);
                    }
                    maid.fullBodyIK.bodyOffsetCtrl.CheckBlendType();
                }
            }

            return resp;
        }

        public class MotionTestJson
        {
            public bool Man { get; set; }
            public int Maid { get; set; }
            public string Mot { get; set; }
            public bool Loop { get; set; }
            public int Blend { get; set; }
            public bool Additive { get; set; }
            public bool Queue { get; set; }
            public int Weight { get; set; }

            public MotionTestJson()
            {
                Man = false;
                Maid = -1;
                Mot = "";
                Blend = 500;
                Additive = false;
                Queue = false;
                Weight = 1000;
            }
        }
        #endregion

        #region Talk
        public static SimpleHTTPServerPOSTResponse getScriptHelpersSoundData()
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            List<List<string>> data = new List<List<string>>();

            //Talk
            foreach (KeyValuePair<string, List<ScriptTag.VoiceData>> kvp in ScriptAnalysis.TalkVoice)
            {
                string commandType = "Talk";
                string file = kvp.Key;
                for (int j = 0; j < ScriptAnalysis.TalkVoice[file].Count; j++)
                {
                    string subtitles = ScriptAnalysis.TalkVoice[file][j].nextLine;
                    string translated = subtitles;//Translate(subtitles);
                    string examplePath = ScriptAnalysis.TalkVoice[file][j].path;
                    string example = ScriptAnalysis.TalkVoice[file][j].line;
                    string arcPath = ScriptAnalysis.TalkVoice[file][j].arcPath;
                    string game = arcPath.StartsWith(GameMain.Instance.CMSystem.CM3D2Path) ? "CM3D2" : "COM3D2";
                    string favorite = ((int)CustomScheduleEvents.config.webData.getFavorite("sound", game + "_" + file)).ToString().Trim();

                    //Row Contents
                    List<string> row = new List<string>();
                    row.Add(file);
                    row.Add(commandType);
                    row.Add(subtitles);
                    row.Add(translated);
                    row.Add(examplePath);
                    row.Add(example);
                    row.Add(game);
                    row.Add(favorite);
                    data.Add(row);
                }
            }

            //TalkRepeat
            foreach(KeyValuePair<string, List<ScriptTag.VoiceData>> kvp in ScriptAnalysis.TalkRepeatVoice)
            {
                string commandType = "TalkRepeat";
                string file = kvp.Key;
                for (int j = 0; j < ScriptAnalysis.TalkRepeatVoice[file].Count; j++)
                {
                    string subtitles = ScriptAnalysis.TalkRepeatVoice[file][j].nextLine;
                    string translated = subtitles;//Translate(subtitles);
                    string examplePath = ScriptAnalysis.TalkRepeatVoice[file][j].path;
                    string example = ScriptAnalysis.TalkRepeatVoice[file][j].line;
                    string arcPath = ScriptAnalysis.TalkRepeatVoice[file][j].arcPath;
                    string game = arcPath.StartsWith(GameMain.Instance.CMSystem.CM3D2Path) ? "CM3D2" : "COM3D2";
                    string favorite = ((int)CustomScheduleEvents.config.webData.getFavorite("sound", game + "_" + file)).ToString().Trim();

                    //Row Contents
                    List<string> row = new List<string>();
                    row.Add(file);
                    row.Add(commandType);
                    row.Add(subtitles);
                    row.Add(translated);
                    row.Add(examplePath);
                    row.Add(example);
                    row.Add(game);
                    row.Add(favorite);
                    data.Add(row);
                }
            }

            //PlayVoice
            foreach(KeyValuePair<string, List<ScriptTag.VoiceData>> kvp in ScriptAnalysis.PlayVoice)
            {
                string commandType = "PlayVoice";
                string file = kvp.Key;
                for (int j = 0; j < ScriptAnalysis.PlayVoice[file].Count; j++)
                {
                    string subtitles = ScriptAnalysis.PlayVoice[file][j].nextLine;
                    string translated = subtitles;// Translate(subtitles);
                    string examplePath = ScriptAnalysis.PlayVoice[file][j].path;
                    string example = ScriptAnalysis.PlayVoice[file][j].line;
                    string arcPath = ScriptAnalysis.PlayVoice[file][j].arcPath;
                    string game = arcPath.StartsWith(GameMain.Instance.CMSystem.CM3D2Path) ? "CM3D2" : "COM3D2";
                    string favorite = ((int)CustomScheduleEvents.config.webData.getFavorite("sound", game + "_" + file)).ToString().Trim();

                    //Row Contents
                    List<string> row = new List<string>();
                    row.Add(file);
                    row.Add(commandType);
                    row.Add(subtitles);
                    row.Add(translated);
                    row.Add(examplePath);
                    row.Add(example);
                    row.Add(game);
                    row.Add(favorite);
                    data.Add(row);
                }
            }

            //PlaySE
            foreach(KeyValuePair<string, string[]> kvp in ScriptTag.PlaySE.results["file"])
            {
                string commandType = "PlaySE";
                
                string file = kvp.Key;
                string arcPath = ScriptTag.PlaySE.results["file"][file][2];
                string game = arcPath.StartsWith(GameMain.Instance.CMSystem.CM3D2Path) ? "CM3D2" : "COM3D2";
                string favorite = ((int)CustomScheduleEvents.config.webData.getFavorite("sound", game + "_" + file)).ToString().Trim();

                //Row Contents
                List<string> row = new List<string>();
                row.Add(file);
                row.Add(commandType);
                row.Add("");
                row.Add("");
                row.Add(ScriptTag.PlaySE.results["file"][file][1]);
                row.Add(ScriptTag.PlaySE.results["file"][file][0]);
                row.Add(game);
                row.Add(favorite);
                data.Add(row);
            }

            resp.data = JsonConvert.SerializeObject(data);
            resp.dataFormat = "json";
            return resp;
        }
        public static SimpleHTTPServerPOSTResponse SoundTest(string json)
        {
            SimpleHTTPServerPOSTResponse resp = new SimpleHTTPServerPOSTResponse();
            if (!CustomScheduleEvents.InPhotoMode)
            {
                resp.errorText = "ERROR: Cannot use CustomScheduleEvents UI from this Scene";
                return resp;
            }

            SoundTestJson data = JsonConvert.DeserializeObject<SoundTestJson>(json);

            if (data.File != null && !data.File.Trim().Equals(""))
            {
                string play_sound_name = data.File + ".ogg";

                //Stop all other sounds
                GameMain.Instance.SoundMgr.StopSe();

                //Play provided sound
                GameMain.Instance.SoundMgr.PlaySe(play_sound_name, false);
            }

            return resp;
        }
        public class SoundTestJson
        {
            public string File { get; set; }
            public SoundTestJson()
            {
                File = "";
            }
        }
        #endregion
        #endregion
    }

    //Modified from https://github.com/ghorsington/TexTool
    public class TexTool
    {
        public static byte[] TexToImg(Stream stream, string texFileName)
        {
            using (var br = new BinaryReader(stream))
            {
                var tag = br.ReadString();

                if (tag != "CM3D2_TEX")
                {
                    Console.WriteLine($"File {texFileName} is not a valid TEX file!");
                    return new byte[] { };
                }

                var version = br.ReadInt32();
                br.ReadString();
                var width = 0;
                var height = 0;
                Rect[] rects = null;

                var format = TextureFormat.ARGB32;

                if (version >= 1010)
                {
                    if (version >= 1011)
                    {
                        var rectCount = br.ReadInt32();
                        rects = new Rect[rectCount];

                        for (var i = 0; i < rectCount; i++)
                        {
                            rects[i] = new Rect(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        }
                    }

                    width = br.ReadInt32();
                    height = br.ReadInt32();
                    format = (TextureFormat)br.ReadInt32();
                }

                if (!Enum.IsDefined(typeof(TextureFormat), format))
                {
                    Console.WriteLine($"File {texFileName} has unsupported texture format: {format}");
                    return new byte[] { };
                }

                var size = br.ReadInt32();
                //Doc said these can be broken/corrupted size = (int)Math.Min(size, br.BaseStream.Length);
                var data = new byte[size];
                br.Read(data, 0, size);

                if (version == 1000)
                {
                    width = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
                    height = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
                }

                if (format == TextureFormat.ARGB32 || format == TextureFormat.RGB24)
                {
                    return data;
                }
                else if (format == TextureFormat.DXT1 || format == TextureFormat.DXT5)
                {
                    var squishFlags = (format == TextureFormat.DXT1) ? Squish.SquishFlags.kDxt1 : Squish.SquishFlags.kDxt5;

                    var outData = new byte[width * height * 4];
                    Squish.Squish.DecompressImage(outData, width, height, data, squishFlags);

                    for (var i = 0; i < width * height; i++)
                    {
                        var r = outData[i * 4];
                        outData[i * 4] = outData[i * 4 + 2];
                        outData[i * 4 + 2] = r;
                    }

                    var gch = GCHandle.Alloc(outData, GCHandleType.Pinned);
                    var img = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, gch.AddrOfPinnedObject());
                    img.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    byte[] retData = ImageToByte2(img);

                    img.Dispose();
                    gch.Free();

                    return retData;
                }
                else
                {
                    Console.WriteLine($"File {texFileName} uses format {format} that is not supported!");
                }

                return new byte[] { };
            }
        }

        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}