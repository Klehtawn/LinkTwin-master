using System.IO;
using UnityEngine;

public class ContentManager
{
    public static string DownloadPath
    {
        get { return Application.persistentDataPath; }
    }

    public static byte[] LoadBinaryFile(string relativePath)
    {
        string path = Path.Combine(DownloadPath, relativePath + ".bytes");
        if (File.Exists(path))
        {
            Debug.Log("[ContentManager] found requested path \"" + relativePath + "\" in the download folder");
            return File.ReadAllBytes(path);
        }

        TextAsset asset = Resources.Load(relativePath) as TextAsset;
        if (asset != null)
        {
            Debug.Log("[ContentManager] found requested path \"" + relativePath + "\" in the resources folder");
            return asset.bytes;
        }

        Debug.Log("[ContentManager] did not find requested path \"" + relativePath + "\" in either the download or resources folder");
        return null;
    }

    public static string LoadTextFile(string relativePath)
    {
        string path = Path.Combine(DownloadPath, relativePath + ".txt");
        if (File.Exists(path))
        {
            Debug.Log("[ContentManager] found requested path \"" + relativePath + "\" in the download folder");
            return File.ReadAllText(path);
        }

        TextAsset asset = Resources.Load(relativePath) as TextAsset;
        if (asset != null)
        {
            Debug.Log("[ContentManager] found requested path \"" + relativePath + "\" in the resources folder");
            return asset.text;
        }

        Debug.Log("[ContentManager] did not find requested path \"" + relativePath + "\" in either the download or resources folder");
        return null;
    }
}
