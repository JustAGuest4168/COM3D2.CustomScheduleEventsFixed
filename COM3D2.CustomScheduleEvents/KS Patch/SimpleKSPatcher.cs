using CM3D2.Toolkit.Guest4168Branch.Arc;
using CM3D2.Toolkit.Guest4168Branch.Arc.Entry;
using CM3D2.Toolkit.Guest4168Branch.Arc.FilePointer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace COM3D2.CustomScheduleEvents.Plugin
{
    public partial class SimpleKSPatcher
    {
        private static string[] standardKsFiles = { "newyotogimain_init_end", "newyotogimain_夜伽キャラクター選択終了", "newyotogimain_夜伽ハーレムペア選択終了",
                                                    "vip_main_0001_custom", "vipmain_VIPキャラクター選択", "vipmain_VIPキャラクター選択終了_custom"
        };
        public static void CreateCustomScheduleEventKS()
        {
            System.Resources.ResourceSet resourceSet = Properties.Resources.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
            foreach (System.Collections.DictionaryEntry entry in resourceSet)
            {
                if (standardKsFiles.Contains(entry.Key.ToString()))
                {
                    string fileName = entry.Key.ToString() + ".ks";
                    byte[] file = (byte[])entry.Value;

                    string path = Path.Combine(CustomScheduleEvents.configPath, fileName);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    File.WriteAllBytes(path, file);
                }
            }
        }
        public static void ApplyAllPatches(string arcName, Dictionary<string, SimpleKSPatch> patches, List<string> additionalFiles)
        {
            //Collection
            Dictionary<string, byte[]> newFiles = new Dictionary<string, byte[]>();

            //Process each entry
            foreach (KeyValuePair<string, SimpleKSPatch> kvp in patches)
            {
                //Get the file data
                SimpleKSFile fileData = parseFileSimpleKS(kvp.Key);

                //Write the new file to byte[]
                newFiles[kvp.Key] = patchFile(fileData, kvp.Value);
            }

            //Files to add to the ARC
            foreach (string file_name in additionalFiles)
            {
                string modPath = System.IO.Path.Combine(CustomScheduleEvents.configPath, file_name);
                newFiles[file_name] = System.IO.File.ReadAllBytes(modPath);
            }

            //Create the arc
            createArc(arcName, newFiles);
        }

        private static SimpleKSFile parseFileSimpleKS(string file_name)
        {
            //Get the file from filesystem
            string script = "";
            using (AFileBase file = GameUty.FileOpen(file_name))
            {
                byte[] data = file.ReadAll();
                script = NUty.SjisToUnicode(data);
            }

            bool subroutineEncountered = false;
            List<string> mainBody = new List<string>();
            List<List<string>> subroutines = new List<List<string>>();

            string[] lines = script.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string nextLine = lines[i];

                //Check if line is a subroutine label
                if (nextLine.StartsWith(@"*") && !nextLine.Contains(@"|"))
                {
                    subroutineEncountered = true;
                    subroutines.Add(new List<string>());
                }

                if (subroutineEncountered == false)
                {
                    //Main body
                    mainBody.Add(nextLine);
                }
                else
                {
                    //Break into subroutines
                    subroutines[subroutines.Count - 1].Add(nextLine);
                }
            }

            return new SimpleKSFile { name = file_name, mainBody = mainBody, subroutines = subroutines };
        }

        private static byte[] patchFile(SimpleKSFile ksFile, SimpleKSPatch patch)
        {
            byte[] data;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ms, System.Text.Encoding.UTF8))//Encoding.GetEncoding(932)))
                {
                    //Main body
                    for (int i = 0; i < ksFile.mainBody.Count; i++)
                    {
                        sw.WriteLine(ksFile.mainBody[i]);
                    }

                    //Subroutines
                    Dictionary<string, int> labelCount = new Dictionary<string, int>();
                    for (int i = 0; i < ksFile.subroutines.Count; i++)
                    {
                        string label = ksFile.subroutines[i][0].Trim();

                        //See if the file has the same label multiple times
                        if (!labelCount.ContainsKey(label))
                        {
                            labelCount[label] = 0;
                        }
                        labelCount[label] = labelCount[label] + 1;

                        //Check for prefixes
                        if (patch.insertBeforeSubroutine.ContainsKey(label) && labelCount[label] == patch.insertBeforeSubroutine[label].targetOccurance)
                        {
                            string modPath = System.IO.Path.Combine(CustomScheduleEvents.configPath, patch.insertBeforeSubroutine[label].modFile);
                            if (File.Exists(modPath))
                            {
                                byte[] contents = System.IO.File.ReadAllBytes(modPath);
                                string script = NUty.SjisToUnicode(contents); //ToUnicode(contents);

                                sw.Write(script);
                                sw.Write("\r\n");
                            }
                        }

                        //Check if this subroutine should be replaced
                        bool replaced = false;
                        if (patch.replacements.ContainsKey(label) && labelCount[label] == patch.replacements[label].targetOccurance)
                        {
                            string modPath = System.IO.Path.Combine(CustomScheduleEvents.configPath, patch.replacements[label].modFile);
                            if (File.Exists(modPath))
                            {
                                replaced = true;
                                byte[] contents = System.IO.File.ReadAllBytes(modPath);
                                string script = NUty.SjisToUnicode(contents); //ToUnicode(contents);

                                sw.Write(script);
                                sw.Write("\r\n");
                            }
                        }
                        if (!replaced)
                        {
                            //Write the original subroutine
                            for (int j = 0; j < ksFile.subroutines[i].Count; j++)
                            {
                                sw.WriteLine(ksFile.subroutines[i][j]);
                            }

                            //Check if any additional subroutines should be written after
                            if (patch.insertAfterSubroutine.ContainsKey(label) && labelCount[label] == patch.insertAfterSubroutine[label].targetOccurance)
                            {
                                string modPath = System.IO.Path.Combine(CustomScheduleEvents.configPath, patch.insertAfterSubroutine[label].modFile);
                                if (File.Exists(modPath))
                                {
                                    byte[] contents = System.IO.File.ReadAllBytes(modPath);
                                    string script = NUty.SjisToUnicode(contents); //ToUnicode(contents);

                                    sw.Write(script);
                                    sw.Write("\r\n");
                                }
                            }
                        }
                    }

                    //Get the byte[]
                    sw.Flush();
                    ms.Seek(0, System.IO.SeekOrigin.Begin);
                    data = ms.ToArray();
                    //data = ToSJIS(data);

                    //using (System.IO.FileStream fs = new System.IO.FileStream(System.IO.Path.Combine(CustomScheduleEvents.modPath, ksFile.name), System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    //{
                    //    ms.WriteTo(fs);
                    //    fs.Flush();
                    //}
                }
            }

            return data;
        }

        //Modified code from https://github.com/Neerhom/COM3D2.ModLoader
        private static void createArc(string arcName, Dictionary<string, byte[]> newFiles)
        {
            //Create a temp Arc file
            // get gamepath and Mod folder
            string gamepath = System.IO.Path.GetFullPath(".\\");

            ArcFileSystem fs = new ArcFileSystem();

            foreach (KeyValuePair<string, byte[]> newFile in newFiles)
            {
                ArcFileEntry arcFile = fs.CreateFile(newFile.Key);
                arcFile.Pointer = new MemoryFilePointer(newFile.Value);
            }

            // Save the ARC file on disk because the game does not support loading ARC from meory easily
            // the temp ARC is saved in ML_temp folder in game's folder, so as to not get in the way of general mods
            if (!System.IO.Directory.Exists(System.IO.Path.Combine(gamepath, arcName)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(gamepath, arcName));
            }

            //Delete any existing
            string arcPath = System.IO.Path.Combine(gamepath, arcName + "\\" + arcName + ".arc");
            if (File.Exists(arcPath))
            {
                File.Delete(arcPath);
            }

            //Create the new arc
            using (System.IO.FileStream fStream = System.IO.File.Create(arcPath))
            {
                fs.Save(fStream);
            }

            // Get the game's file system and add our custom ARC file
            FileSystemArchive gameFileSystem = GameUty.FileSystem as FileSystemArchive;

            gameFileSystem.AddArchive(arcPath);
        }
    }
    public class SimpleKSFile
    {
        public string name { get; set; }
        public List<string> mainBody { get; set; }
        public List<List<string>> subroutines { get; set; }

        public SimpleKSFile()
        {
            name = "";
            mainBody = new List<string>();
            subroutines = new List<List<string>>();
        }

    }
    public class SimpleKSPatch
    {
        public Dictionary<string, SimpleKSPatchMod> insertBeforeSubroutine { get; set; }
        public Dictionary<string, SimpleKSPatchMod> replacements { get; set; }
        public Dictionary<string, SimpleKSPatchMod> insertAfterSubroutine { get; set; }

        public SimpleKSPatch()
        {
            insertBeforeSubroutine = new Dictionary<string, SimpleKSPatchMod>();
            replacements = new Dictionary<string, SimpleKSPatchMod>();
            insertAfterSubroutine = new Dictionary<string, SimpleKSPatchMod>();
        }
    }
    public class SimpleKSPatchMod
    {
        public int targetOccurance { get; set; }
        public string modFile { get; set; }

        public SimpleKSPatchMod()
        {
            targetOccurance = 1;
            modFile = "";
        }
    }
}
