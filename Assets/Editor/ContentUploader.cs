using UnityEditor;
using UnityEngine;
using GameSparks.Editor;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Collections.Generic;
using System.Net;
using System;

public class ContentUploader : EditorWindow
{
    private const string gsRestUrl = "https://portal.gamesparks.net/rest/games/";
    private const string gsStage = "preview";
    private const string gsCollection = "script.contentUpdates";

    string userName = "";
    string password = "";

    private int localContentVersion;
    string localContentDetails = "";
    int serverContentVersion = 0;
    int serverLevelFormat = 0;
    string serverContentDetails = "";
    string restResult = "";
    bool gotServerInfo = false;
    bool serverEmpty = false;

    bool CanAuthenticate
    {
        get { return (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password)); }
    }

    [MenuItem("LinkTwin/Content Uploader")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(ContentUploader));
    }

    void OnEnable()
    {
        ReadLocalContentVersion();
        if (string.IsNullOrEmpty(userName))
            userName = EditorPrefs.GetString("ContentUploader.UserName", "");
    }

    void OnDisable()
    {
        EditorPrefs.SetString("ContentUploader.UserName", userName);
    }

    private string GetArchiveFolder()
    {
        return Path.Combine(Application.dataPath, "ContentPacks");
    }

    private string GetArchivePath()
    {
        return Path.Combine(GetArchiveFolder(), GetShortCode() + ".zip");
    }

    int GetUploadedContentVersion()
    {
        if (localContentVersion <= serverContentVersion)
            return serverContentVersion + 1;
        else
            return localContentVersion + 1;
    }

    string GetShortCode()
    {
        return string.Format("LTContent{0:D3}", GetUploadedContentVersion());
    }

    void OnGUI()
    {
        ShowCredentialsArea();

        ShowServerContentInfo();
        ShowLocalContentInfo();

        ShowRestOutput();
    }

    private void ShowRestOutput()
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("REST Output (what the server said)", EditorStyles.boldLabel);
        GUILayout.TextArea(restResult, EditorStyles.wordWrappedLabel);
        GUILayout.EndVertical();
    }

    private void ShowLocalContentInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Local data pack", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Content version will be automatically updated");
        EditorGUILayout.LabelField("Content version: ", GetUploadedContentVersion().ToString());
        EditorGUILayout.LabelField("Level format:", TableDescription.CURRENT_VERSION.ToString());
        localContentDetails = EditorGUILayout.TextField("Pack info: ", localContentDetails);
        EditorGUILayout.LabelField("ShortCode: ", GetShortCode());
        if (gotServerInfo)
        {
            if (GUILayout.Button("Upload pack", GUILayout.Width(100)))
            {
                string message = "Warning!\n\n" +
                    "This will upload the current asset pack to the GameSparks server and make it immediately available to installed apps.\n\n" +
                    "This can not be undone once it is downloaded.";
                if (EditorUtility.DisplayDialog("Confirm", message, "Upload", "Cancel"))
                {
                    restResult = "";
                    UploadArchive();
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void ShowServerContentInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Server data pack", EditorStyles.boldLabel);
        if (gotServerInfo)
        {
            if (serverEmpty)
            {
                EditorGUILayout.LabelField("No content pack metadata found on server");
            }
            else
            {
                EditorGUILayout.LabelField("Content version:", serverContentVersion.ToString());
                EditorGUILayout.LabelField("Level format:", serverLevelFormat.ToString());
                EditorGUILayout.LabelField("Pack info:", serverContentDetails);
            }
        }
        else
        {
            EditorGUILayout.LabelField("Server data not available. Fill the credential fields and hit the Get Data button");
        }
        EditorGUILayout.EndVertical();
    }

    private void UploadArchive()
    {
        string progressTitle = "Uploading content";
        EditorUtility.DisplayProgressBar(progressTitle, "Exporting level list", 0.0f);
        BuildTools.Builder.ExportLevelLists(GetUploadedContentVersion());
        EditorUtility.DisplayProgressBar(progressTitle, "Building archive", 0.2f);
        BuildArchive();
        EditorUtility.DisplayProgressBar(progressTitle, "Uploading archive", 0.5f);
        restResult += "\n" + GameSparksRestApi.setDownloadable(GameSparksSettings.ApiKey, userName, password, GetShortCode(), GetArchivePath());
        EditorUtility.DisplayProgressBar(progressTitle, "Creating archive metadata", 0.7f);
        UpdateServerContentVersion();
        EditorUtility.DisplayProgressBar(progressTitle, "Updating server info", 1.0f);
        ReadServerContentVersion();
        ReadLocalContentVersion();
        EditorUtility.ClearProgressBar();
    }

    private void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
    {
        string[] files = Directory.GetFiles(path);

        foreach (string filename in files)
        {
            if (filename.EndsWith(".meta"))
                continue;

            FileInfo fi = new FileInfo(filename);

            string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
            entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
            ZipEntry newEntry = new ZipEntry(entryName);
            newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity
            newEntry.Size = fi.Length;

            zipStream.PutNextEntry(newEntry);

            // Zip the file in buffered chunks
            // the "using" will close the stream even if an exception occurs
            byte[] buffer = new byte[4096];
            using (FileStream streamReader = File.OpenRead(filename))
            {
                StreamUtils.Copy(streamReader, zipStream, buffer);
            }
            zipStream.CloseEntry();
        }
        string[] folders = Directory.GetDirectories(path);
        foreach (string folder in folders)
        {
            CompressFolder(folder, zipStream, folderOffset);
        }
    }

    private void BuildArchive()
    {
        MemoryStream memStream = new MemoryStream();
        ZipOutputStream zipStream = new ZipOutputStream(memStream);

        zipStream.SetLevel(9);

        string rootpath = Path.Combine(Application.dataPath, "Resources");
        int folderOffset = rootpath.Length + 1;

        AddFolderToArchive(zipStream, rootpath, folderOffset, "Levels");
        AddFolderToArchive(zipStream, rootpath, folderOffset, "Texts");
        AddFolderToArchive(zipStream, rootpath, folderOffset, "Shop");

        zipStream.IsStreamOwner = false;
        zipStream.Close();

        memStream.Close();

        byte[] data = memStream.ToArray();

        Directory.CreateDirectory(GetArchiveFolder());

        File.WriteAllBytes(GetArchivePath(), data);
    }

    private void AddFolderToArchive(ZipOutputStream zipStream, string rootpath, int folderOffset, string folder)
    {
        string path = Path.Combine(rootpath, folder);

        ZipEntry entry = new ZipEntry(ZipEntry.CleanName(folder + "/"));
        zipStream.PutNextEntry(entry);
        zipStream.CloseEntry();

        CompressFolder(path, zipStream, folderOffset);
    }

    private void ShowCredentialsArea()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("GameSparks credentials", EditorStyles.boldLabel);
        userName = EditorGUILayout.TextField("User Name", userName);
        password = EditorGUILayout.PasswordField("Password", password);
        GUI.enabled = CanAuthenticate;
        if (GUILayout.Button("Get server data", GUILayout.Width(120)))
        {
            restResult = "";
            ReadServerContentVersion();
        }
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }

    void ReadLocalContentVersion()
    {
        string path = Path.Combine(Application.dataPath, "Resources/Levels/chapters.txt");
        string text = File.ReadAllText(path);
        string[] lines = text.Split('\n');
        localContentVersion = int.Parse(lines[0]);
    }

    void ReadServerContentVersion()
    {
        string postUrl = gsRestUrl + GameSparksSettings.ApiKey + "/mongo/" + gsStage + "/" + gsCollection + "/find";
        HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

        if (request == null)
        {
            Debug.LogError("request is not a http request");
            return;
        }

        IDictionary<string, object> postParams = new Dictionary<string, object>();
        postParams.Add("query", "{}");
        postParams.Add("sort", "{\"version\":-1}");
        postParams.Add("limit", "1");
        string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
        string contentType = "multipart/form-data; boundary=" + formDataBoundary;
        byte[] formData = GameSparksEditorFormUpload.GetMultipartFormData(postParams, formDataBoundary);


        // Set up the request properties.
        request.Method = "POST";
        request.ContentType = contentType;
        request.UserAgent = "Unity Editor";
        request.CookieContainer = new CookieContainer();
        request.ContentLength = formData.Length;

        // You could add authentication here as well if needed:
        request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(userName + ":" + password)));

        Stream requestStream = request.GetRequestStream();
        requestStream.Write(formData, 0, formData.Length);
        requestStream.Close();

        File.WriteAllBytes(Application.dataPath + "/request.txt", formData);

        try
        {
            WebResponse webResponse = request.GetResponse();
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());

            string responseString = responseReader.ReadToEnd();
            SimpleJSON.JSONNode json = SimpleJSON.JSON.Parse(responseString);

            if (json.Count == 1)
            {
                serverContentVersion = json[0]["version"].AsInt;
                serverContentDetails = json[0]["details"].ToString();
                serverLevelFormat = json[0]["levelformat"].AsInt;
                gotServerInfo = true;
                serverEmpty = false;
            }
            else if (json.Count == 0)
            {
                serverContentVersion = 0;
                serverContentDetails = "";
                serverLevelFormat = 0;
                gotServerInfo = true;
                serverEmpty = true;
            }
            else
            {
                serverContentVersion = 0;
                serverContentDetails = "";
                serverLevelFormat = 0;
                gotServerInfo = false;
                Debug.LogWarning("some error occured while trying to get server data");
                Debug.Log(responseString);
            }

            restResult += "\n" + responseString;
        }
        catch (WebException e)
        {
            //Debug.LogError(e.Message);
            restResult += "\n" + e.Message;
        }

        //Debug.Log(json[0]);

        //Debug.Log(responseString);
    }

    private void UpdateServerContentVersion()
    {
        string postUrl = gsRestUrl + GameSparksSettings.ApiKey + "/mongo/" + gsStage + "/" + gsCollection + "/insert";
        HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

        if (request == null)
        {
            Debug.LogError("request is not a http request");
            return;
        }

        IDictionary<string, object> postParams = new Dictionary<string, object>();
        localContentDetails = localContentDetails.Replace("\"", "");
        string document = string.Format("{{\"levelformat\":{0},\"version\":{1},\"downloadId\":\"{2}\",\"details\":\"{3}\"}}", TableDescription.CURRENT_VERSION, GetUploadedContentVersion(), GetShortCode(), localContentDetails);

        //Debug.Log(document);
        postParams.Add("document", document);
        string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
        string contentType = "multipart/form-data; boundary=" + formDataBoundary;
        byte[] formData = GameSparksEditorFormUpload.GetMultipartFormData(postParams, formDataBoundary);


        // Set up the request properties.
        request.Method = "POST";
        request.ContentType = contentType;
        request.UserAgent = "Unity Editor";
        request.CookieContainer = new CookieContainer();
        request.ContentLength = formData.Length;

        // You could add authentication here as well if needed:
        request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(userName + ":" + password)));

        Stream requestStream = request.GetRequestStream();
        requestStream.Write(formData, 0, formData.Length);
        requestStream.Close();

        //File.WriteAllBytes(Application.dataPath + "/request.txt", formData);

        try
        {
            WebResponse webResponse = request.GetResponse();
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());

            string responseString = responseReader.ReadToEnd();
            restResult += "\n" + responseString;
        }
        catch (WebException e)
        {
            //Debug.LogError(e.Message);
            restResult += "\n" + e.Message;
        }

        //Debug.Log(json[0]);

        //Debug.Log(responseString);
    }
}
