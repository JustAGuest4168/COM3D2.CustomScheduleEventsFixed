
using COM3D2.NeiAppender.Plugin;
using Newtonsoft.Json;

#region ScheduleNightCategory
[NeiTable("schedule_work_night_category_list.nei")]
public class ScheduleNightCategory : NeiJsonObject
{
    [NeiColumn(0, "ID", typeof(int))]
    [JsonProperty("ID")]
    public int ID { get; set; }

    [NeiColumn(1, "表示名", typeof(string))]
    [JsonProperty("表示名")]
    public string name { get; set; }

    public ScheduleNightCategory()
    {
        ID = 0;
        name = "";
    }

    public ScheduleNightCategory(int _id, string _name)
    {
        ID = _id;
        name = _name;
    }
}

public class ScheduleNightCategoryJSONTable : NeiJSONTable<ScheduleNightCategoryJSONTable, ScheduleNightCategory>
{
    //Nothing really to do here, mostly just a shortcut
    //You read from a file or collection of file to populate this, and then yeah not sure what to do...
}
#endregion

#region ScheduleWorkNight
[NeiTable("schedule_work_night.nei")]
public class ScheduleWorkNight : NeiJsonObject
{
    [NeiColumn(0, "ID", typeof(int))]
    [JsonProperty("ID")]
    public int ID { get; set; }

    [NeiColumn(1, "名前", typeof(string))]
    [JsonProperty("名前")]
    public string name { get; set; }

    [NeiColumn(2, "フリーコメント", typeof(string))]
    [JsonProperty("フリーコメント")]
    public string comment { get; set; }

    [NeiColumn(3, "アイコン名", typeof(string))]
    [JsonProperty("アイコン名")]
    public string icon { get; set; }

    [NeiColumn(4, "カテゴリーID", typeof(int))]
    [JsonProperty("カテゴリーID")]
    public int categoryId { get; set; }

    [NeiColumn(5, "寝取られ要素(廃止)", typeof(bool))]
    [JsonProperty("寝取られ要素(廃止)")]
    public bool ntrFlag_obsolete { get; set; }

    [NeiColumn(6, "特殊処理フラグ", typeof(string))]
    [JsonProperty("特殊処理フラグ")]
    public string yotogiType { get; set; }

    [NeiColumn(7, "説明", typeof(string))]
    [JsonProperty("説明")]
    public string descriptionText { get; set; }

    [NeiColumn(8, "取得金額", typeof(int))]
    [JsonProperty("取得金額")]
    public int incomeBase { get; set; }

    [NeiColumn(9, "評価", typeof(int))]
    [JsonProperty("評価")]
    public int evaluationBase { get; set; }


    [NeiColumn(10, "VIP時の加算経験人数", typeof(int))]
    [JsonProperty("VIP時の加算経験人数")]
    public int countAsMasterEntertained { get; set; }
    [NeiColumn(11, "VIP時の加算接待人数", typeof(int))]
    [JsonProperty("VIP時の加算接待人数")]
    public int countAsGuestsEntertained { get; set; }


    [NeiColumn(12, "条件説明文1", typeof(string))]
    [JsonProperty("条件説明文1")]
    public string requirementText1 { get; set; }

    [NeiColumn(13, "条件説明文2", typeof(string))]
    [JsonProperty("条件説明文2")]
    public string requirementText2 { get; set; }

    [NeiColumn(14, "条件説明文3", typeof(string))]
    [JsonProperty("条件説明文3")]
    public string requirementText3 { get; set; }

    [NeiColumn(15, "条件説明文4", typeof(string))]
    [JsonProperty("条件説明文4")]
    public string requirementText4 { get; set; }

    [NeiColumn(16, "条件説明文5", typeof(string))]
    [JsonProperty("条件説明文5")]
    public string requirementText5 { get; set; }

    [NeiColumn(17, "条件説明文6", typeof(string))]
    [JsonProperty("条件説明文6")]
    public string requirementText6 { get; set; }

    [NeiColumn(18, "条件説明文7", typeof(string))]
    [JsonProperty("条件説明文7")]
    public string requirementText7 { get; set; }

    [NeiColumn(19, "条件説明文8", typeof(string))]
    [JsonProperty("条件説明文8")]
    public string requirementText8 { get; set; }

    [NeiColumn(20, "条件説明文9", typeof(string))]
    [JsonProperty("条件説明文9")]
    public string requirementText9 { get; set; }



    [NeiColumn(21, "表示条件：サロングレード", typeof(int))]
    [JsonProperty("表示条件：サロングレード")]
    public int requiredSalonGrade { get; set; }

    [NeiColumn(22, "実行条件：スキル", typeof(string))]
    [JsonProperty("実行条件：スキル")]
    public string requiredSkillLevels { get; set; }

    [NeiColumn(23, "実行条件：契約", typeof(string))]
    [JsonProperty("実行条件：契約")]
    public string requiredContracts { get; set; }

    [NeiColumn(24, "実行条件：メイドクラス（取得している）", typeof(string))]
    [JsonProperty("実行条件：メイドクラス（取得している）")]
    public string requiredMaidClass { get; set; }

    [NeiColumn(25, "実行条件：夜伽タイプ（取得している）", typeof(string))]
    [JsonProperty("実行条件：夜伽タイプ（取得している）")]
    public string requiredYotogiClass { get; set; }

    [NeiColumn(26, "実行条件：所持性癖", typeof(string))]
    [JsonProperty("実行条件：所持性癖")]
    public string requiredPropensity { get; set; }

    [NeiColumn(27, "実行条件：性経験", typeof(string))]
    [JsonProperty("実行条件：性経験")]
    public string requiredHoleExperience { get; set; }

    [NeiColumn(28, "実行条件：メイドフラグが1以上", typeof(string))]
    [JsonProperty("実行条件：メイドフラグが1以上")]
    public string maidHasFlag { get; set; }

    [NeiColumn(29, "実行条件：メイドフラグが0以下", typeof(string))]
    [JsonProperty("実行条件：メイドフラグが0以下")]
    public string maidLacksFlag { get; set; }

    [NeiColumn(30, "実行条件：メイドの状態", typeof(string))]
    [JsonProperty("実行条件：メイドの状態")]
    public string maidRelationship { get; set; }



    [NeiColumn(31, "廃止　表示条件：レンタルメイドの時強制で処理", typeof(string))]
    [JsonProperty("廃止　表示条件：レンタルメイドの時強制で処理")]
    public string isRentalMaid { get; set; }


    [NeiColumn(32, "表示条件：対応サブメイド　ユニーク名", typeof(string))]
    [JsonProperty("表示条件：対応サブメイド　ユニーク名")]
    public string rentalMaidName { get; set; }

    [NeiColumn(33, "表示条件：主人公フラグが1以上", typeof(string))]
    [JsonProperty("表示条件：主人公フラグが1以上")]
    public string manHasFlagDisplay { get; set; }

    [NeiColumn(34, "実行条件：主人公フラグが1以上", typeof(string))]
    [JsonProperty("実行条件：主人公フラグが1以上")]
    public string manHasFlagExecute { get; set; }

    [NeiColumn(35, "実行条件：主人公フラグが0以下", typeof(string))]
    [JsonProperty("実行条件：主人公フラグが0以下")]
    public string manLacksFlagExecute { get; set; }

    [NeiColumn(36, "表示条件：メイドの性格", typeof(string))]
    [JsonProperty("表示条件：メイドの性格")]
    public string maidPersonality { get; set; }

    [NeiColumn(37, "表示条件:メインキャラ", typeof(int))]
    [JsonProperty("表示条件:メインキャラ")]
    public int maidIsMainTrio { get; set; }

    [NeiColumn(38, "実行条件:対応施設ID", typeof(string))]
    [JsonProperty("実行条件:対応施設ID")]
    public string facilitiesInUse { get; set; }

    [NeiColumn(39, "実行条件 ペアとして指定する必要がある性格", typeof(string))]
    [JsonProperty("実行条件 ペアとして指定する必要がある性格")]
    public string additionalMaidPersonalityRequirement { get; set; }



    [NeiColumn(40, "特殊処理 GP002性格プラグインの導入を確認", typeof(string))]
    [JsonProperty("特殊処理 GP002性格プラグインの導入を確認")]
    public string isCheckGP002Personal { get; set; }

    [NeiColumn(41, "ボディ一致チェック", typeof(string))]
    [JsonProperty("ボディ一致チェック")]
    public string bodyMatchCheck { get; set; }

    [NeiColumn(42, "新ボディブロック", typeof(string))]
    [JsonProperty("新ボディブロック")]
    public string bodyMatchBlock { get; set; }

    [NeiColumn(43, "メインキャラとのボディ一致チェック", typeof(string))]
    [JsonProperty("メインキャラとのボディ一致チェック")]
    public string bodyMatchCheckWithMainCharacter { get; set; }

    public Schedule.ScheduleCSVData.Yotogi convertToGameData()
    {
        char[] separator1 = new char[1] { '&' };
        char[] separator2 = new char[1] { '|' };
        char[] chArray = new char[1] { ',' };

        Schedule.ScheduleCSVData.Yotogi yotogi = new Schedule.ScheduleCSVData.Yotogi();
        yotogi.mode = Schedule.ScheduleCSVData.ScheduleBase.Mode.COM3D;
        yotogi.type = ScheduleTaskCtrl.TaskType.Yotogi;
        yotogi.id = this.ID;
        yotogi.name = this.name;
        yotogi.icon = this.icon;
        yotogi.isCommu = false;
        yotogi.categoryID = this.categoryId;
        yotogi.netorareFlag = this.ntrFlag_obsolete;
        yotogi.yotogiType = (Schedule.ScheduleCSVData.YotogiType)System.Enum.Parse(typeof(Schedule.ScheduleCSVData.YotogiType), this.yotogiType, false);
        yotogi.information = this.descriptionText;
        yotogi.income = this.incomeBase;
        yotogi.evaluation = this.evaluationBase;
        yotogi.add_play_number = this.countAsMasterEntertained;
        yotogi.add_other_play_number = this.countAsGuestsEntertained;

        yotogi.condInfo = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrEmpty(requirementText1))
        {
            yotogi.condInfo.Add(this.requirementText1);
        }
        if (!string.IsNullOrEmpty(requirementText2))
        {
            yotogi.condInfo.Add(this.requirementText2);
        }
        if (!string.IsNullOrEmpty(requirementText3))
        {
            yotogi.condInfo.Add(this.requirementText3);
        }
        if (!string.IsNullOrEmpty(requirementText4))
        {
            yotogi.condInfo.Add(this.requirementText4);
        }
        if (!string.IsNullOrEmpty(requirementText5))
        {
            yotogi.condInfo.Add(this.requirementText5);
        }
        if (!string.IsNullOrEmpty(requirementText6))
        {
            yotogi.condInfo.Add(this.requirementText6);
        }
        if (!string.IsNullOrEmpty(requirementText7))
        {
            yotogi.condInfo.Add(this.requirementText7);
        }
        if (!string.IsNullOrEmpty(requirementText8))
        {
            yotogi.condInfo.Add(this.requirementText8);
        }
        if (!string.IsNullOrEmpty(requirementText9))
        {
            yotogi.condInfo.Add(this.requirementText9);
        }

        yotogi.condSalonGrade = this.requiredSalonGrade;

        yotogi.condSkill = new System.Collections.Generic.Dictionary<int, int>();
        if (this.requiredSkillLevels != null)
        {
            string[] strArray1 = this.requiredSkillLevels.Split(new char[1] { '&' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < strArray1.Length; ++index)
            {
                if (strArray1[index].Contains(","))
                {
                    string[] strArray2 = strArray1[index].Split(new char[1] { ',' }, System.StringSplitOptions.None);
                    yotogi.condSkill[int.Parse(strArray2[0])] = int.Parse(strArray2[1]);
                }
            }
        }

        yotogi.condContract = new System.Collections.Generic.List<MaidStatus.Contract>();
        if (this.requiredContracts != null)
        {
            string[] strArray3 = this.requiredContracts.Split(new char[1] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < strArray3.Length; ++index)
            {
                if (System.Enum.IsDefined(typeof(MaidStatus.Contract), (object)strArray3[index]))
                {
                    yotogi.condContract.Add((MaidStatus.Contract)System.Enum.Parse(typeof(MaidStatus.Contract), strArray3[index], false));
                }
            }
        }

        yotogi.condMaidClass = new System.Collections.Generic.List<int>();
        if (this.requiredMaidClass != null)
        {
            foreach (string uniqueName in this.requiredMaidClass.Split(separator1, System.StringSplitOptions.RemoveEmptyEntries))
            {
                int id = MaidStatus.JobClass.GetData(uniqueName).id;
                yotogi.condMaidClass.Add(id);
            }
        }

        yotogi.condYotogiClass = new System.Collections.Generic.List<int>();
        if (this.requiredYotogiClass != null)
        {
            foreach (string uniqueName in this.requiredYotogiClass.Split(separator1, System.StringSplitOptions.RemoveEmptyEntries))
            {
                int id = MaidStatus.YotogiClass.GetData(uniqueName).id;
                yotogi.condYotogiClass.Add(id);
            }
        }

        yotogi.condPropensity = new System.Collections.Generic.List<int>();
        if (this.requiredPropensity != null)
        {
            string[] strArray4 = this.requiredPropensity.Split(new char[1] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < strArray4.Length; ++index)
            {
                if (MaidStatus.Propensity.Contains(strArray4[index]))
                {
                    int id = MaidStatus.Propensity.GetData(strArray4[index]).id;
                    yotogi.condPropensity.Add(id);
                }
            }
        }

        yotogi.condSeikeiken = new System.Collections.Generic.List<MaidStatus.Seikeiken>();
        if (this.requiredHoleExperience != null)
        {
            string[] strArray5 = this.requiredHoleExperience.Split(new char[1] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < strArray5.Length; ++index)
            {
                if (System.Enum.IsDefined(typeof(MaidStatus.Seikeiken), (object)strArray5[index]))
                    yotogi.condSeikeiken.Add((MaidStatus.Seikeiken)System.Enum.Parse(typeof(MaidStatus.Seikeiken), strArray5[index], false));
            }
        }

        yotogi.condFlag1 = new System.Collections.Generic.List<string>();
        if (this.maidHasFlag != null)
        {
            foreach (string str4 in this.maidHasFlag.Split(separator1, System.StringSplitOptions.RemoveEmptyEntries))
            {
                yotogi.condFlag1.Add(str4);
            }
        }

        yotogi.condFlag0 = new System.Collections.Generic.List<string>();
        if (this.maidLacksFlag != null)
        {
            foreach (string str4 in this.maidLacksFlag.Split(separator1, System.StringSplitOptions.RemoveEmptyEntries))
            {
                yotogi.condFlag0.Add(str4);
            }
        }

        
        yotogi.condRelation = new System.Collections.Generic.List<MaidStatus.Relation>();
        yotogi.condAdditionalRelation = new System.Collections.Generic.List<MaidStatus.AdditionalRelation>();
        yotogi.condSpecialRelation = new System.Collections.Generic.List<MaidStatus.SpecialRelation>();
        yotogi.condRelationOld = new System.Collections.Generic.List<MaidStatus.Old.Relation>();
        if (this.maidRelationship != null)
        {
            string[] strArray6 = this.maidRelationship.Split(new char[1] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < strArray6.Length; ++index)
            {
                if (System.Enum.IsDefined(typeof(MaidStatus.Relation), (object)strArray6[index]))
                    yotogi.condRelation.Add((MaidStatus.Relation)System.Enum.Parse(typeof(MaidStatus.Relation), strArray6[index], false));
                else if (System.Enum.IsDefined(typeof(MaidStatus.AdditionalRelation), (object)strArray6[index]))
                    yotogi.condAdditionalRelation.Add((MaidStatus.AdditionalRelation)System.Enum.Parse(typeof(MaidStatus.AdditionalRelation), strArray6[index], false));
                else if (System.Enum.IsDefined(typeof(MaidStatus.SpecialRelation), (object)strArray6[index]))
                    yotogi.condSpecialRelation.Add((MaidStatus.SpecialRelation)System.Enum.Parse(typeof(MaidStatus.SpecialRelation), strArray6[index], false));
            }
        }

        yotogi.subMaidUnipueName = this.rentalMaidName;

        yotogi.condPackage = new System.Collections.Generic.List<string>();

        yotogi.condManVisibleFlag1 = new System.Collections.Generic.List<string>();
        if (this.manHasFlagDisplay != null)
        {
            foreach (string str4 in this.manHasFlagDisplay.Split(separator1, System.StringSplitOptions.RemoveEmptyEntries))
            {
                yotogi.condManVisibleFlag1.Add(str4);
            }
        }

        yotogi.condManFlag1 = new System.Collections.Generic.List<string>();
        if (this.manHasFlagExecute != null)
        {
            foreach (string str4 in this.manHasFlagExecute.Split(separator1, System.StringSplitOptions.RemoveEmptyEntries))
            {
                yotogi.condManFlag1.Add(str4);
            }
        }

        yotogi.condManFlag0 = new System.Collections.Generic.List<string>();
        if (this.manLacksFlagExecute != null)
        {
            foreach (string str4 in this.manLacksFlagExecute.Split(separator1, System.StringSplitOptions.RemoveEmptyEntries))
            {
                yotogi.condManFlag0.Add(str4);
            }
        }

        yotogi.condPersonal = new System.Collections.Generic.List<int>();
        if (this.maidPersonality != null)
        {
            foreach (string uniqueName in this.maidPersonality.Split(separator2, System.StringSplitOptions.RemoveEmptyEntries))
            {
                int id = MaidStatus.Personal.GetData(uniqueName).id;
                yotogi.condPersonal.Add(id);
            }
        }

        yotogi.condMainChara = (this.maidIsMainTrio == 1);

        yotogi.condFacilityID = new System.Collections.Generic.List<System.Collections.Generic.List<int>>();
        if (this.facilitiesInUse != null)
        {
            foreach (string str4 in this.facilitiesInUse.Split(separator1, System.StringSplitOptions.RemoveEmptyEntries))
            {
                string[] strArray2 = str4.Split(separator2, System.StringSplitOptions.RemoveEmptyEntries);
                System.Collections.Generic.List<int> intList = new System.Collections.Generic.List<int>();
                for (int index = 0; index < strArray2.Length; ++index)
                {
                    int result = 0;
                    if (int.TryParse(strArray2[index], out result) && result != 0)
                        intList.Add(result);
                }
                yotogi.condFacilityID.Add(intList);
            }
        }

        yotogi.pairCondPersonal = new System.Collections.Generic.List<int>();
        if (this.additionalMaidPersonalityRequirement != null)
        {
            foreach (string uniqueName in this.additionalMaidPersonalityRequirement.Split(separator2, System.StringSplitOptions.RemoveEmptyEntries))
            {
                int id = MaidStatus.Personal.GetData(uniqueName).id;
                yotogi.pairCondPersonal.Add(id);
            }
        }

        yotogi.isCheckGP002Personal = this.isCheckGP002Personal == "〇" || this.isCheckGP002Personal == "○";
        yotogi.isCheckBodyType = this.bodyMatchCheck == "〇" || this.bodyMatchCheck == "○";
        yotogi.isNewBodyBlock = this.bodyMatchBlock == "〇" || this.bodyMatchBlock == "○";

        yotogi.mainHeroineBodyTypeMatchCheckList = new System.Collections.Generic.HashSet<string>();
        if (this.bodyMatchCheckWithMainCharacter != null)
        {
            foreach (string str4 in this.bodyMatchCheckWithMainCharacter.Split(chArray))
            {
                if (!string.IsNullOrEmpty(str4))
                {
                    switch (str4.Trim())
                    {
                        case "無垢":
                        case "真面目":
                        case "凜デレ":
                            yotogi.mainHeroineBodyTypeMatchCheckList.Add(str4.Trim());
                            continue;
                        default:
                            //Debug.LogError((object)("ID[" + (object)yotogi.id + "]" + yotogi.name + "のメインキャラとのボディ一致チェック項目で不正な文字列が指定されています=>" + str4.Trim()));
                            continue;
                    }
                }
            }
        }

        if (yotogi.yotogiType == Schedule.ScheduleCSVData.YotogiType.HaveSex)
        {
            yotogi.mode = Schedule.ScheduleCSVData.ScheduleBase.Mode.Common;
        }
        if (yotogi.yotogiType == Schedule.ScheduleCSVData.YotogiType.Entertain || yotogi.yotogiType == Schedule.ScheduleCSVData.YotogiType.Rest)
        {
            yotogi.mode = Schedule.ScheduleCSVData.ScheduleBase.Mode.CM3D2;
        }
        
        return yotogi;
    }
}

public class ScheduleWorkNightJSONTable : NeiJSONTable<ScheduleWorkNightJSONTable, ScheduleWorkNight>
{
    //Nothing really to do here, mostly just a shortcut
    //You read from a file or collection of file to populate this, and then yeah not sure what to do...
}
#endregion

#region ScheduleWorkNightEnabled
[NeiTable("schedule_work_night_enabled.nei")]
public class ScheduleWorkNightEnabled : NeiJsonObject
{
    [NeiColumn(0, "ID", typeof(int))]
    [JsonProperty("ID")]
    public int ID { get; set; }

    [NeiColumn(1, "name", typeof(string))]
    [JsonProperty("name")]
    public string name { get; set; }

    public ScheduleWorkNightEnabled()
    {
        ID = 0;
        name = "";
    }

    public ScheduleWorkNightEnabled(int _id, string _name)
    {
        ID = _id;
        name = _name;
    }
}

public class ScheduleWorkNightEnabledJSONTable : NeiJSONTable<ScheduleWorkNightEnabledJSONTable, ScheduleWorkNightEnabled>
{
    //Nothing really to do here, mostly just a shortcut
    //You read from a file or collection of file to populate this, and then yeah not sure what to do...
}
#endregion