using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using LitJson;
using System.Text;

namespace BuildTools
{
	class Builder : ScriptableObject
    {
        /// <summary>
        /// This contains the scenes to be included in the build, the first one is the startup scene
        /// </summary>
        static string[] scenes =
		    {
			    "Assets/Scenes/index.unity",
            };

        static int ReadRevisionNumber()
        {
            string path = Path.Combine(Application.dataPath, "Resources/svnrev.txt");
            if (File.Exists(path))
            {
                string ver = PlayerSettings.bundleVersion;
                return int.Parse(File.ReadAllText(path));
            }
            return 1;
        }

        static void BuildAndroidPlayer()
        {
            //ExportLevelLists();

            // --- ensure path exists
            string finalPath = "builds/AndroidGooglePlay";
            Directory.CreateDirectory(finalPath);

            // --- set build target to Android
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);

            // --- set preprocessing symbols
            //const string GOOGLE_PLAY_DEFINE = "USE_DMO_ANALYTICS;USE_REFERRAL_STORE;enableGPGServices";
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Android, GOOGLE_PLAY_DEFINE);

            // --- set package name
            //PlayerSettings.bundleIdentifier = "com.amberstudio.linktwin";

            // --- set API Level
            //PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel9;

            // --- set bundle version code
            PlayerSettings.Android.bundleVersionCode = ReadRevisionNumber();

            // --- set product name (different from iOS for some weird spacing issue)
            //PlayerSettings.productName = "Link Twin";

            // --- force OpenGL ES 2.0 graphics due to an autodetection bug on Unity
            //PlayerSettings.targetGlesGraphics = TargetGlesGraphics.OpenGLES_2_0;

            // --- set keystore name and password
            //PlayerSettings.Android.keystoreName = "Assets/LinkTwin.keystore";
            PlayerSettings.Android.keystorePass = "AmberStudi0";
            //PlayerSettings.Android.keyaliasName = "linktwin";
            PlayerSettings.Android.keyaliasPass = "AmberStudi0";

            // --- use obb file for Google Play builds
            //PlayerSettings.Android.useAPKExpansionFiles = true;
            PlayerSettings.Android.useAPKExpansionFiles = false;

            // --- build the package
            BuildPipeline.BuildPlayer(scenes, Path.Combine(finalPath, "LinkTwin.apk"), BuildTarget.Android, BuildOptions.None);

            // --- rename the obb to its final form
            //string obbFile = Path.Combine(finalPath, "Cinderella.main.obb");
            //string finalObbFile = Path.Combine(finalPath, "main." + PlayerSettings.Android.bundleVersionCode + "." + PlayerSettings.bundleIdentifier + ".obb");
            //FileUtil.ReplaceFile(obbFile, finalObbFile);
            //FileUtil.DeleteFileOrDirectory(obbFile);

            string commands = "adb -d uninstall " + PlayerSettings.bundleIdentifier + "\n" +
                "adb -d install -r LinkTwin.apk" + "\n" +
                "adb -d shell am start -n " + PlayerSettings.bundleIdentifier + "/com.amberstudio.amberlib.AmberActivity" + "\n";
            File.WriteAllText(Path.Combine(finalPath, "installnrun.bat"), commands);
        }

		static void BuildIOSPlayer()
        {
            //ExportLevelLists();

            // --- set bundle id
            PlayerSettings.bundleIdentifier = "com.amberstudio.linktwin";

            // --- set build number
            PlayerSettings.iOS.buildNumber = ReadRevisionNumber().ToString();

            // --- set build target to iOS
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);

            // --- build the package
            BuildPipeline.BuildPlayer(scenes, "BuildIOS", BuildTarget.iOS, BuildOptions.None);
        }

        static void BuildWin32Player()
        {
            //ExportLevelLists();

            // --- ensure path exists
            string finalPath = "builds/Win32/LinkTwin";
            Directory.CreateDirectory(finalPath);

            // --- set build target to Android
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);

            // --- build the package
            BuildPipeline.BuildPlayer(scenes, Path.Combine(finalPath, "LinkTwin.exe"), BuildTarget.StandaloneWindows, BuildOptions.None);

            // --- remove pdb files for now
            foreach (string file in Directory.GetFiles(finalPath, "*.pdb"))
            {
                Debug.Log("deleting " + file);
                File.Delete(file);
            }
        }

        [MenuItem("LinkTwin/Build/Android")]
        static void BuildAndroidMenuItem()
        {
            BuildAndroidPlayer();
        }

        [MenuItem("LinkTwin/Build/iOS")]
        static void BuildIOSMenuItem()
        {
            BuildIOSPlayer();
        }

        [MenuItem("LinkTwin/Build/Win32")]
        static void BuildWin32MenuItem()
        {
            BuildWin32Player();
        }

        private static void ReportError(string message)
        {
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
                Debug.LogError(message);
            else
                EditorUtility.DisplayDialog("Error", message, "Ok");
        }

        [MenuItem("LinkTwin/Export levels")]
        public static void ExportLevelLists()
        {
            ExportLevelLists(1);
        }

        private static bool ExportLevel(string srcPath, string destPath, float timelimit)
        {
            TableDescription td = null;
            LevelRoot root = LevelRoot.CreateRoot("LevelRootTemp");

            try
            {
                string path = Path.Combine(Path.GetDirectoryName(srcPath), Path.GetFileNameWithoutExtension(srcPath));
                TableLoadSave.LoadAbsolutePath(path, ref td, root);
                TableLoadSave.ConvertMapDescriptionToScene(td, root);

                LevelSimulation.Solver levelSim = new LevelSimulation.Solver();
                levelSim.Init(root);

                levelSim.RunSolver(timelimit, 1);

                List<LevelSimulation.Solution> solutions = levelSim.Solutions;
                if (solutions != null && solutions.Count > 0)
                {
                    td.numSolutionSteps = (ushort)solutions[0].NumSteps;
                    td.solutionSteps = new byte[td.numSolutionSteps];
                    for (int i = 0; i < td.numSolutionSteps; i++)
                        td.solutionSteps[i] = solutions[0].steplist[i + 1].id;
                }
                else
                {
                    Debug.LogWarning(srcPath + ": Unable to solve level in " + timelimit + " seconds, level will be exported without a solution!");
                    td.numSolutionSteps = 0;
                }

                TableLoadSave.SaveAbsolutePath(destPath, td, true);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            finally
            {
                LevelRoot.DestroyRoot(ref root);
            }
            return true;
        }

        public static void ExportLevelLists(int contentVersion)
        {
            string sourcePath = Application.dataPath + "/Levels/MainFlow";
            string destPath = Application.dataPath + "/Resources/Levels";

            if (!Directory.Exists(sourcePath))
            {
                ReportError("Failed to export level lists. Folder \"" + sourcePath + "\" does not exist. This is where the main flow chapters should be.");
                return;
            }

            if (!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);

            string[] files = Directory.GetFiles(destPath, "*.bytes");
            foreach (string filename in files)
                File.Delete(filename);

            List<string> chapterFiles = new List<string>(Directory.GetFiles(sourcePath, "*.levellist"));

            if (chapterFiles.Count == 0)
            {
                ReportError("Failed to export level lists. Folder \"" + sourcePath + "\" is empty. This is where the main flow chapters should be.");
                return;
            }

            chapterFiles.Sort();

            // --- try to read existing chapter file and keep the exported version
            if (contentVersion == 1)
            {
                string path = Path.Combine(Application.dataPath, "Resources/Levels/chapters.txt");
                if (File.Exists(path))
                {
                    string text = File.ReadAllText(path);
                    try
                    {
                        JsonReader reader = new JsonReader(text);
                        JsonData data = JsonMapper.ToObject(reader);
                        contentVersion = (int)data["version"];
                    }
                    catch (System.Exception)
                    {
                        contentVersion = 1;
                    }
                }
            }

            //string chapterInfo = "" + contentVersion + "\n" + chapterFiles.Count + "\n";
            StringBuilder sb = new StringBuilder(1024);
            JsonWriter writer = new JsonWriter(sb);
            writer.WriteObjectStart();
            writer.WritePropertyName("version");
            writer.Write(contentVersion);
            writer.WritePropertyName("chapters");
            writer.WriteArrayStart();
            int numLevels = 0;
            for (int ci = 0; ci < chapterFiles.Count; ci++)
            {
                string chapterFile = chapterFiles[ci];
                string text = File.ReadAllText(chapterFile);
                string[] lines = text.Split('\n');
                int version = int.Parse(lines[0]);
                int count = int.Parse(lines[1]);
                int levelCount = count;
                int bonusCount = 0;// int.Parse(lines[2]);
                int baseIdx = 2;
                if (version > 1)
                {
                    levelCount = int.Parse(lines[2]);
                    bonusCount = count - levelCount;
                    baseIdx = 3;
                }
                //chapterInfo += "" + count + "\n";
                numLevels += count;
                for (int i = 0; i < count; i++)
                {
                    string relativePath = "/Levels/" + lines[2 * i + baseIdx] + ".bytes";
                    string levelSourcePath = Application.dataPath + relativePath;
                    EditorUtility.DisplayProgressBar("Exporting Chapter " + (ci + 1), relativePath, (float)i / count);
                    string levelDestPath = destPath + "/" +
                        string.Format("c{0:D2}l{1:D2}.bytes", ci + 1, i + 1);
                    //FileUtil.CopyFileOrDirectory(levelSourcePath, levelDestPath);
                    if (!ExportLevel(levelSourcePath, levelDestPath, 30.0f))
                    //if (!ExportLevel(levelSourcePath, levelDestPath, 3600.0f))
                    {
                        Debug.LogError("Export failed for " + levelSourcePath);
                        return;
                    }
                }
                writer.WriteObjectStart();
                writer.WritePropertyName("levels");
                writer.Write(levelCount);
                writer.WritePropertyName("bonus");
                writer.Write(bonusCount);
                writer.WriteObjectEnd();
            }
            writer.WriteArrayEnd();
            writer.WriteObjectEnd();

            EditorUtility.ClearProgressBar();

            //File.WriteAllText(destPath + "/chapters.txt", chapterInfo);
            File.WriteAllText(destPath + "/chapters.txt", sb.ToString());

            Debug.Log("Exported " + numLevels + " levels in " + chapterFiles.Count + " chapters.");
        }

        [MenuItem("LinkTwin/Run solver on all levels")]
        public static void RunSolverAll()
        {
            const float timelimit = 60.0f;

            string basePath = Application.dataPath + "/Levels";
            List<string> files = new List<string>(Directory.GetFiles(basePath, "*.bytes", SearchOption.AllDirectories));

            // --- cut out the replay files
            List<string> levelfiles = files.FindAll(s => !s.EndsWith(".replay.bytes"));

            Stream fs = File.OpenWrite(Application.dataPath + "/SolverReport.txt");
            StreamWriter writer = new StreamWriter(fs);

            writer.WriteLine("Starting automatic solver run, individual time limit " + timelimit + " seconds");
            writer.WriteLine("Processing " + levelfiles.Count + " files");
            writer.WriteLine();

            int timeouts = 0;
            List<string> timeoutlevels = new List<string>();

            foreach (string levelPath in levelfiles)
            {
                LevelLink ll = new LevelLink();
                ll.SetPath(levelPath);
                writer.WriteLine(levelPath);
                ll.LoadLevelInfo();
                float t = Time.realtimeSinceStartup;
                ll.RunLevelSimulation(timelimit, 1);
                if (ll.levelSim.Solutions == null || ll.levelSim.Solutions.Count == 0)
                {
                    writer.WriteLine("\tSolver timed out at " + timelimit + " seconds");
                    timeoutlevels.Add(levelPath);
                    timeouts++;
                }
                else
                {
                    writer.WriteLine("\t" + string.Format("(in {0:N2} seconds): ", (Time.realtimeSinceStartup - t))+ ll.levelSim.Solutions[0].description);
                }
                writer.WriteLine();
            }

            writer.WriteLine("Solver timed out at " + timelimit + " on " + timeouts + " levels");

            if (timeoutlevels.Count > 0)
            {
                writer.WriteLine("Timeout on following levels: ");
                foreach (string path in timeoutlevels)
                    writer.WriteLine(path);
            }

            writer.Close();
            fs.Close();

            Debug.Log("solver run complete, " + timeouts + " timeouts");
        }
    }
}
