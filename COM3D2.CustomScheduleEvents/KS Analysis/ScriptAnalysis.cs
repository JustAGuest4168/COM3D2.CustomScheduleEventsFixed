using CM3D2.Toolkit.Guest4168Branch.Arc.Entry;
using CM3D2.Toolkit.Guest4168Branch.MultiArcLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.CustomScheduleEvents.Plugin
{
    public class ScriptAnalysis
    {
        //ARCs
        private static MultiArcLoader _mal;
        public static MultiArcLoader mal 
        { 
            get 
            {
                if(_mal == null)
                {
                    try
                    {
                        _mal = new MultiArcLoader(new string[] {    System.IO.Path.Combine(GameMain.Instance.CMSystem.CM3D2Path, "GameData"),
                                                                    System.IO.Path.Combine(UTY.gameProjectPath, "GameData"),
                                                                    System.IO.Path.Combine(UTY.gameProjectPath, "GameData_20"),
                                                                    System.IO.Path.Combine(UTY.gameProjectPath, "Mod")}, 
                                                        3, MultiArcLoader.LoadMethod.Single, false, null, true, 
                                                        MultiArcLoader.Exclude.None | MultiArcLoader.Exclude.BG | MultiArcLoader.Exclude.CSV |
                                                        MultiArcLoader.Exclude.Motion | MultiArcLoader.Exclude.Parts | MultiArcLoader.Exclude.PriorityMaterial |
                                                        MultiArcLoader.Exclude.Sound | MultiArcLoader.Exclude.System | MultiArcLoader.Exclude.Voice);
                        _mal.LoadArcs();
                        AnalyzeScripts();
                    }
                    catch(Exception ex)
                    {
                        _mal = null;
                        throw;
                    }
                }
                return _mal;
            } 
        }

        //Special Tags
        private static Dictionary<string, List<string>> _MotionScripts = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<string>> MotionScripts
        {
            get
            {
                return _MotionScripts;
            }
        }

        private static Dictionary<string, List<ScriptTag.VoiceData>> _TalkVoice = new Dictionary<string, List<ScriptTag.VoiceData>>();
        public static Dictionary<string, List<ScriptTag.VoiceData>> TalkVoice
        {
            get
            {
                return _TalkVoice;
            }
        }
        private static Dictionary<string, List<ScriptTag.VoiceData>> _TalkRepeatVoice = new Dictionary<string, List<ScriptTag.VoiceData>>();
        public static Dictionary<string, List<ScriptTag.VoiceData>> TalkRepeatVoice
        {
            get
            {
                return _TalkRepeatVoice;
            }
        }
        private static Dictionary<string, List<ScriptTag.VoiceData>> _PlayVoice = new Dictionary<string, List<ScriptTag.VoiceData>>();
        public static Dictionary<string, List<ScriptTag.VoiceData>> PlayVoice
        {
            get
            {
                return _PlayVoice;
            }
        }

        //private static ScriptTag _Motions;
        //public static ScriptTag Motions
        //{
        //    get
        //    {
        //        //if(_Motions == null)
        //        //{
        //        //    ScriptTag.Motion.analyzeScripts(mal);
        //        //    _Motions = ScriptTag.Motion;
        //        //}

        //        //return _Motions;
        //    }
        //}
        //public static void AnalyzeTag(ScriptTag st)
        //{
        //    switch(st.tag)
        //    {
        //        case "@motionscript ":

        //            break;
        //        default:
        //            st.analyzeScripts(mal);
        //            break;
        //    }
        //}

        private static void AnalyzeScripts()
        {
            List<string> unrecognizedCommands = new List<string>();

            //Loop files
            foreach (ArcFileEntry arcFile in mal.arc.Files.Values)
            {
                //Must be a script
                if (arcFile.Name.EndsWith("." + "ks"))
                {
                    if (!(mal.GetContentsArcFilePath(arcFile) == null || mal.GetContentsArcFilePath(arcFile).Trim().Equals("")))
                    {
                        string path = arcFile.FullName;
                        ArcDirectoryEntry parent = arcFile.Parent;
                        string arcPath = parent.ArcPath;
                        while (arcPath == null && parent.Parent != null)
                        {
                            parent = parent.Parent;
                            arcPath = parent.ArcPath;
                        }
                        if (!path.Contains("cbl"))
                        {
                            arcFile.Pointer = arcFile.Pointer.Decompress();
                            string script = NUty.SjisToUnicode(arcFile.Pointer.Data); //Encoding.UTF8.GetString(arcFile.Pointer.Data);
                            string[] lines = script.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
                            
                            for (int i = 0; i < lines.Length; i++)
                            {
                                string line = lines[i].TrimStart().ToLower();

                                //Command
                                if (line.StartsWith(@"@", StringComparison.OrdinalIgnoreCase))
                                {
                                    //Special Cases
                                    string commandText = line.Contains(" ")?line.Split(' ')[0].Trim() : line.Trim();
                                    switch(commandText)
                                    {
                                        case ScriptTag.ScriptCommand.talkrepeat:
                                        {
                                            ScriptTag tag = analyzeCommandStandard(ScriptTag.TalkRepeat, line, path, arcPath);
                                            if (tag.results.ContainsKey("voice") && tag.results["voice"].Keys.Count > 0)
                                            {
                                                string voiceFile = tag.results["voice"].Keys.ToList()[0];

                                                string nextLine = ((i + 1) < lines.Length) ? lines[i + 1] : "";
                                                if (!_TalkRepeatVoice.ContainsKey(voiceFile))
                                                {
                                                    _TalkRepeatVoice[voiceFile] = new List<ScriptTag.VoiceData>();
                                                }
                                                _TalkRepeatVoice[voiceFile].Add(new ScriptTag.VoiceData(nextLine, path, line, arcPath));
                                            }
                                            break;
                                        }
                                        case ScriptTag.ScriptCommand.talk:
                                        {
                                            ScriptTag tag = analyzeCommandStandard(ScriptTag.Talk, line, path, arcPath);
                                            if(tag.results.ContainsKey("voice") && tag.results["voice"].Keys.Count > 0)
                                            {
                                                string voiceFile = tag.results["voice"].Keys.ToList()[0];

                                                string nextLine = ((i + 1) < lines.Length) ? lines[i+1] : "";
                                                if(!_TalkVoice.ContainsKey(voiceFile))
                                                {
                                                    _TalkVoice[voiceFile] = new List<ScriptTag.VoiceData>();
                                                }
                                                _TalkVoice[voiceFile].Add(new ScriptTag.VoiceData(nextLine, path, line, arcPath));
                                            }
                                            break;
                                        }
                                        case ScriptTag.ScriptCommand.playvoice:
                                        {
                                            ScriptTag tag = analyzeCommandStandard(ScriptTag.PlayVoice, line, path, arcPath);
                                            if (tag.results.ContainsKey("voice") && tag.results["voice"].Keys.Count > 0)
                                            {
                                                string voiceFile = tag.results["voice"].Keys.ToList()[0];

                                                string nextLine = ((i + 1) < lines.Length) ? lines[i + 1] : "";
                                                if (!_PlayVoice.ContainsKey(voiceFile))
                                                {
                                                    _PlayVoice[voiceFile] = new List<ScriptTag.VoiceData>();
                                                }
                                                _PlayVoice[voiceFile].Add(new ScriptTag.VoiceData(nextLine, path, line, arcPath));
                                            }
                                            break;
                                        }
                                        case ScriptTag.ScriptCommand.playse:
                                        {
                                            analyzeCommandStandard(ScriptTag.PlaySE, line, path, arcPath);
                                            break;
                                        }
                                        case ScriptTag.ScriptCommand.motion:
                                        {
                                            analyzeCommandStandard(ScriptTag.Motion, line, path, arcPath);
                                            break;
                                        }
                                        default:
                                        {
                                            if(!unrecognizedCommands.Contains(commandText))
                                            {
                                                unrecognizedCommands.Add(commandText);
                                            }
                                            break;
                                        }
                                    }
                                }

                                //Label
                                else if(line.StartsWith(@"*", StringComparison.OrdinalIgnoreCase))
                                {
                                    //MotionScript
                                    if (path.Contains(@"\motion\"))
                                    {
                                        string key = arcFile.Name;

                                        if (!_MotionScripts.ContainsKey(key))
                                        {
                                            _MotionScripts[key] = new List<string>();
                                        }

                                        if (!_MotionScripts[key].Contains(line))
                                        {
                                            _MotionScripts[key].Add(line);
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                    else
                    {
                        Console.WriteLine("LOST ARC: " + arcFile.FullName);
                    }
                }
            }
        }

        private static ScriptTag analyzeCommandStandard(ScriptTag tag, string line, string path, string arcPath)
        {
            ScriptTag tagReturn = new ScriptTag(tag.tag, tag.paramNames.ToArray());

            //Remove initial tag
            line = line.Substring(tag.tag.Length - 1).Trim();
            string lineOriginal = line;

            //Replace all param names with alternative to fix missing spaces
            for (int j = 0; j < tag.paramNames.Count; j++)
            {
                line = line.Replace(tag.paramNames[j], "|" + tag.paramNames[j]);
            }

            List<string> paramsSplit = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int j = 0; j < paramsSplit.Count; j++)
            {
                //Cleanup whitespace
                paramsSplit[j] = paramsSplit[j].Trim();

                //Collect
                if (paramsSplit[j].Contains("="))
                {
                    string[] paramDetails = paramsSplit[j].Split('=');

                    if (paramDetails.Length == 2)
                    {
                        string paramName = paramDetails[0].Trim().ToLower();
                        string paramValue = paramDetails[1].Trim().ToLower();

                        if (tag.paramNames.Contains(paramName + "="))
                        {
                            //If param needs dictionary of value->source
                            if (!tag.results.ContainsKey(paramName))
                            {
                                tag.results[paramName] = new Dictionary<string, string[]>();   
                            }
                            if(!tagReturn.results.ContainsKey(paramName))
                            {
                                tagReturn.results[paramName] = new Dictionary<string, string[]>();
                            }

                            //If param does not have value yet
                            if (tag.results.ContainsKey(paramName) && !tag.results[paramName].ContainsKey(paramValue))
                            {
                                tag.results[paramName][paramValue] = new string[] { lineOriginal, path, arcPath };
                                tagReturn.results[paramName][paramValue] = new string[] { lineOriginal, path, arcPath };
                            }
                        }
                        else
                        {
                            tag.unknownParams.Add(new List<string>() { paramName, lineOriginal, path, arcPath });
                            tagReturn.unknownParams.Add(new List<string>() { paramName, lineOriginal, path, arcPath });
                        }
                    }
                    else
                    {
                        tag.badScripts.Add(new List<string>() { lineOriginal, path, arcPath });
                        tagReturn.badScripts.Add(new List<string>() { lineOriginal, path, arcPath });
                    }
                }
                else
                {
                    string paramName = paramsSplit[j];
                    string paramValue = paramName;

                    //If param needs dictionary of value->source
                    if (!tag.results.ContainsKey(paramName))
                    {
                        if (tag.paramNames.Contains(paramName))
                        {
                            tag.results[paramName] = new Dictionary<string, string[]>();
                            tagReturn.results[paramName] = new Dictionary<string, string[]>();
                        }
                        else
                        {
                            tag.unknownParams.Add(new List<string>() { paramName, lineOriginal, path, arcPath });
                            tagReturn.unknownParams.Add(new List<string>() { paramName, lineOriginal, path, arcPath });
                        }
                    }

                    //If param does not have value yet
                    if (tag.results.ContainsKey(paramName) && !tag.results[paramName].ContainsKey(paramValue))
                    {
                        tag.results[paramName][paramValue] = new string[] { lineOriginal, path, arcPath };
                        tagReturn.results[paramName][paramValue] = new string[] { lineOriginal, path, arcPath };
                    }
                }
            }

            return tagReturn;
        }
    }

    
    public class ScriptTag
    {
        public string tag { get; set; }
        public bool includeCBL { get; set; }
        public List<string> paramNames { get; set; }
        public Dictionary<string, Dictionary<string, string[]>> results { get; set; }
        public List<List<string>> unknownParams { get; set; }
        public List<List<string>> badScripts { get; set; }
        public ScriptTag(string tag, string[] paramNames)
        {
            this.tag = tag;
            this.paramNames = paramNames.ToList<String>();
            results = new Dictionary<string, Dictionary<string, string[]>>();
            unknownParams = new List<List<string>>();
            badScripts = new List<List<string>>();
        }
        public ScriptTag(string tag, string[] paramNames, bool includeCBL)
        {
            this.tag = tag;
            this.paramNames = paramNames.ToList<String>();
            this.includeCBL = includeCBL;

            results = new Dictionary<string, Dictionary<string, string[]>>();
            unknownParams = new List<List<string>>();
            badScripts = new List<List<string>>();
        }

        public void analyzeScripts(MultiArcLoader mal)
        {
            //Loop files
            foreach (ArcFileEntry arcFile in mal.arc.Files.Values)
            {
                //Must be a script
                if (arcFile.Name.EndsWith("." + "ks"))
                {
                    if (!(mal.GetContentsArcFilePath(arcFile) == null || mal.GetContentsArcFilePath(arcFile).Trim().Equals("")))
                    {
                        string path = arcFile.FullName;
                        if (!path.Contains("cbl") || this.includeCBL)
                        {
                            arcFile.Pointer = arcFile.Pointer.Decompress();
                            string script = NUty.SjisToUnicode(arcFile.Pointer.Data); //Encoding.UTF8.GetString(arcFile.Pointer.Data);

                            string[] lines = script.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
                            for (int i = 0; i < lines.Length; i++)
                            {
                                string line = lines[i].TrimStart().ToLower();
                                if (line.StartsWith(this.tag.TrimStart().ToLower(), StringComparison.OrdinalIgnoreCase))
                                {
                                    //Remove initial tag
                                    line = line.Substring(this.tag.Length).Trim();
                                    string lineOriginal = line;

                                    //Replace all param names with alternative to fix missing spaces
                                    for (int j = 0; j < this.paramNames.Count; j++)
                                    {
                                        line = line.Replace(this.paramNames[j], "|" + this.paramNames[j]);
                                    }

                                    List<string> paramsSplit = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                    for (int j = 0; j < paramsSplit.Count; j++)
                                    {
                                        //Cleanup whitespace
                                        paramsSplit[j] = paramsSplit[j].Trim();

                                        //Collect
                                        if (paramsSplit[j].Contains("="))
                                        {
                                            string[] paramDetails = paramsSplit[j].Split('=');

                                            if (paramDetails.Length == 2)
                                            {
                                                string paramName = paramDetails[0].Trim().ToLower();
                                                string paramValue = paramDetails[1].Trim().ToLower();

                                                if (this.paramNames.Contains(paramName + "="))
                                                {
                                                    //If param needs dictionary of value->source
                                                    if (!this.results.ContainsKey(paramName))
                                                    {
                                                        this.results[paramName] = new Dictionary<string, string[]>();
                                                    }

                                                    //Special Combo Cases
                                                    //if (this.tag.Equals(ScriptTag.MotionScript.tag) && paramName.Equals("file"))
                                                    //{
                                                    //    string extraParamName = "label";
                                                    //    for (int k = 0; k < paramsSplit.Count; k++)
                                                    //    {
                                                    //        string temp = paramsSplit[k].Trim();
                                                    //        if (temp.Contains("="))
                                                    //        {
                                                    //            string[] paramDetails2 = temp.Split('=');

                                                    //            if (paramDetails2.Length == 2)
                                                    //            {
                                                    //                string paramName2 = paramDetails2[0].Trim().ToLower();
                                                    //                string paramValue2 = paramDetails2[1].Trim().ToLower();

                                                    //                if (this.paramNames.Contains(paramName2 + "=") && paramName2.Equals(extraParamName))
                                                    //                {
                                                    //                    paramValue += "~" + paramValue2;
                                                    //                }
                                                    //            }
                                                    //        }
                                                    //    }

                                                    //}

                                                    //If param does not have value yet
                                                    if (this.results.ContainsKey(paramName) && !this.results[paramName].ContainsKey(paramValue))
                                                    {
                                                        this.results[paramName][paramValue] = new string[] { lineOriginal, path };
                                                    }
                                                }
                                                else
                                                {
                                                    this.unknownParams.Add(new List<string>() { paramName, lineOriginal, path });
                                                }
                                            }
                                            else
                                            {
                                                this.badScripts.Add(new List<string>() { lineOriginal, path });
                                            }
                                        }
                                        else
                                        {
                                            string paramName = paramsSplit[j];
                                            string paramValue = paramName;

                                            //If param needs dictionary of value->source
                                            if (!this.results.ContainsKey(paramName))
                                            {
                                                if (this.paramNames.Contains(paramName))
                                                {
                                                    this.results[paramName] = new Dictionary<string, string[]>();
                                                }
                                                else
                                                {
                                                    this.unknownParams.Add(new List<string>() { paramName, lineOriginal, path });
                                                }
                                            }

                                            //If param does not have value yet
                                            if (this.results.ContainsKey(paramName) && !this.results[paramName].ContainsKey(paramValue))
                                            {
                                                this.results[paramName][paramValue] = new string[] { lineOriginal, path };
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("LOST ARC: " + arcFile.FullName);
                    }
                }
            }
        }

        public static ScriptTag Talk = new ScriptTag(@"@talk ", new string[] { "name=", "real=", "voice=", "maid=", "man=", "np" });
        public static ScriptTag TalkRepeat = new ScriptTag(@"@talkrepeat ", new string[] { "name=", "real=", "voice=", "maid=", "man=", "np" });
        public static ScriptTag Face = new ScriptTag(@"@face ", new string[] { "maid=", "name=", "wait=" });
        public static ScriptTag FaceBlend = new ScriptTag(@"@faceblend ", new string[] { "maid=", "man=", "name=" });
        public static ScriptTag FaceBlend2 = new ScriptTag(@"@faceblend2 ", new string[] { "maid=", "man=", "name=" });
        public static ScriptTag Motion = new ScriptTag(@"@motion ", new string[] { "maid=", "man=", "mot=", "blend=", "loop=", "wait=" });
       // public static ScriptTag MotionScript = new ScriptTag(@"@motionscript ", new string[] { "maid=", "man=", "file=", "label=", "wait=", "sloat=", "npos", "bodymix" });
        public static ScriptTag PhysicsHit = new ScriptTag(@"@phisicshit ", new string[] { "maid=", "man=", "height=" });
        public static ScriptTag TexMulAdd = new ScriptTag(@"@texmuladd ", new string[] { "layer=", "x=", "y=", "z=", "r=", "s=", "slot=", "matno=", "propname=", "file=", "res=", "part=", "delay=" });
        public static ScriptTag EyeToPosition = new ScriptTag(@"@eyetoposition ", new string[] { "maid=", "man=", "x=", "y=", "z=", "blend=" });
        public static ScriptTag PlayVoice = new ScriptTag(@"@playvoice ", new string[] { "maid=", "voice=", "name=", "wait=", "wait" });
        public static ScriptTag PlaySE = new ScriptTag(@"@playse ", new string[] { "file=", "loop", "wait=", "wait", "fade=" });

        public class ScriptCommand
        {
            public const string talk = @"@talk";
            public const string talkrepeat = @"@talkrepeat";
            public const string face = @"@face";
            public const string faceblend = @"@faceblend";
            public const string faceblend2 = @"@faceblend2";
            public const string motion = @"@motion";
            public const string phisicshit = @"@phisicshit";
            public const string texmuladd = @"@texmuladd";
            public const string eyetoposition = @"@eyetoposition";
            public const string playvoice = @"@playvoice";
            public const string playse = @"@playse";
        }

        public class VoiceData
        {
            public string nextLine { get; set; }
            public string path { get; set; }
            public string line { get; set; }
            public string arcPath { get; set; }
            public VoiceData(string _nextLine, string _path, string _line, string _arcPath)
            {
                nextLine = _nextLine;
                path = _path;
                line = _line;
                arcPath = _arcPath;
            }
        }
    }
}
