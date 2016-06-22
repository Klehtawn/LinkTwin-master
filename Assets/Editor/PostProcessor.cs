using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BuildTools
{
    [InitializeOnLoad]
    public class PostProcessor : AssetPostprocessor
    {
        private static string libConfig = "debug";

        override public int GetPostprocessOrder()
        {
            return 1;
        }

        /// <summary>
        /// Called by Unity when all assets have been updated. This
        /// is used to kick off resolving the dependendencies declared.
        /// </summary>
        /// <param name="importedAssets">Imported assets. (unused)</param>
        /// <param name="deletedAssets">Deleted assets. (unused)</param>
        /// <param name="movedAssets">Moved assets. (unused)</param>
        /// <param name="movedFromAssetPaths">Moved from asset paths. (unused)</param>
        static void OnPostprocessAllAssets(string[] importedAssets,
                                           string[] deletedAssets,
                                           string[] movedAssets,
                                           string[] movedFromAssetPaths)
        {
            UpdateAmberLibs();
        }

        public static void UpdateAmberLibs()
        {
            string dataPath = Application.dataPath.Replace("\\", "/");
            dataPath = dataPath.Substring(0, dataPath.LastIndexOf("/"));
            string basePath = Path.Combine(dataPath, "BuildUtils/Android/NativeUtils").Replace("\\", "/");
            CheckForUpdate(basePath, "amberlib", true);
            CheckForUpdate(basePath, "samsungiap", false);
        }

        static void CheckForUpdate(string basePath, string libname, bool exploded)
        {
            string srcPath = basePath + "/" + libname + "/build/outputs/aar/" + libname + "-" + libConfig + ".aar";
            if (!File.Exists(srcPath))
            {
                Debug.LogError("File not found: " + srcPath);
                return;
            }

            string destAarPath = Path.Combine(Application.dataPath, "Plugins/Android/" + libname + ".aar");
            string destFolderPath = Path.Combine(Application.dataPath, "Plugins/Android/" + libname);

            if (exploded)
            {
                string dstPath = Path.Combine(Application.dataPath, "Plugins/Android/" + libname);
                string manifestPath = dstPath + "/AndroidManifest.xml";
                if (!File.Exists(manifestPath))
                {
                    Debug.Log("can't find folder " + libname + " in plugins folder, exploding");
                    ExplodeAar(srcPath, dstPath);
                }
                FileInfo srcInfo = new FileInfo(srcPath);
                FileInfo dstInfo = new FileInfo(manifestPath);

                if (srcInfo.LastWriteTime > dstInfo.LastWriteTime)
                {
                    Debug.Log("aar " + libname + " needs update, copying");
                    ExplodeAar(srcPath, dstPath);
                }

                // --- file needs to be exploded, delete if found as .aar
                if (File.Exists(destAarPath))
                    File.Delete(destAarPath);
            }
            else
            {
                if (!File.Exists(destAarPath))
                {
                    Debug.Log("can't find " + libname + " in plugins folder, copying");
                    File.Copy(srcPath, destAarPath);
                    return;
                }

                FileInfo srcInfo = new FileInfo(srcPath);
                FileInfo dstInfo = new FileInfo(destAarPath);

                if (srcInfo.LastWriteTime > dstInfo.LastWriteTime)
                {
                    Debug.Log("aar " + libname + " needs update, copying");
                    File.Delete(destAarPath);
                    File.Copy(srcPath, destAarPath);
                }

                // --- delete exploded folder if found, file is to be copied as .aar
                if (Directory.Exists(destFolderPath))
                    Directory.Delete(destFolderPath, true);
            }
        }

        static void ExplodeAar(string aarPath, string destFolder)
        {
            if (Directory.Exists(destFolder))
                Directory.Delete(destFolder, true);
            Directory.CreateDirectory(destFolder);
            try
            {
                string exe = "jar";
                if (RuntimePlatform.WindowsEditor == Application.platform)
                {
                    string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
                    if (javaHome == null)
                    {
                        EditorUtility.DisplayDialog("Error",
                            "JAVA_HOME environment variable must be set.", "OK");
                        throw new Exception("JAVA_HOME not set");
                    }
                    exe = Path.Combine(javaHome, Path.Combine("bin", "jar.exe"));
                }

                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.Arguments = "xvf " +
                    "\"" + Path.GetFullPath(aarPath) + "\"";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = false;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.FileName = exe;
                p.StartInfo.WorkingDirectory = destFolder;
                p.Start();

                // To avoid deadlocks, always read the output stream first and then wait.
                string stderr = p.StandardError.ReadToEnd();

                p.WaitForExit();

                if (p.ExitCode == 0)
                {

                    // move the classes.jar file to libs.
                    string libDir = Path.Combine(destFolder, "libs");
                    if (!Directory.Exists(libDir))
                    {
                        Directory.CreateDirectory(libDir);
                    }
                    if (File.Exists(Path.Combine(libDir, "classes.jar")))
                    {
                        File.Delete(Path.Combine(libDir, "classes.jar"));
                    }
                    if (File.Exists(Path.Combine(destFolder, "classes.jar")))
                    {
                        File.Move(Path.Combine(destFolder, "classes.jar"),
                            Path.Combine(libDir, "classes.jar"));
                    }

                    // Create the project.properties file which indicates to
                    // Unity that this directory is a plugin.
                    if (!File.Exists(Path.Combine(destFolder, "project.properties")))
                    {
                        // write out project.properties
                        string[] props =
                            {
                                "# Project target.",
                                "target=android-9",
                                "android.library=true"
                            };

                        File.WriteAllLines(Path.Combine(destFolder, "project.properties"),
                            props);
                    }

                    ReplaceVariables(Path.Combine(destFolder, "AndroidManifest.xml"));

                    Debug.Log(aarPath + " expanded successfully");
                }
                else
                {
                    Debug.LogError("Error expanding " +
                        Path.GetFullPath(aarPath) +
                        " err: " + p.ExitCode + ": " + stderr);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw e;
            }
        }

        static void ReplaceVariables(string manifestPath)
        {
            if (File.Exists(manifestPath))
            {
                StreamReader sr = new StreamReader(manifestPath);
                string body = sr.ReadToEnd();
                sr.Close();

                body = body.Replace("${applicationId}", PlayerSettings.bundleIdentifier);

                using (var wr = new StreamWriter(manifestPath, false))
                {
                    wr.Write(body);
                }
            }
        }
    }
}
