using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;                    // Needed to work with R/W data on JSON files

using static GhostPathsEnum;

public static class SaveManager
{
    // Folder Path were the game files will be saved
    //private static readonly string saveFolder = Application.dataPath + "/Saves/";
    private static readonly string saveFolder = Application.streamingAssetsPath;
    private const string saveExtension = "txt";

    public static void Init()
    {
        // Check if the saving directory exist or not.
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
    }

    public static void Save(string saveString)
    {
        // File saving data
        string filePath = Path.Combine(saveFolder, "HookPath." + saveExtension);
        File.WriteAllText(filePath, saveString);        
    }

    public static string Load(GhostPaths ghostPath)
    {        
        string fileName = (ghostPath == GhostPaths.WallJumpingPath) ? 
                "WallJumpingPath" : 
                "HookPath";

        string filepath = Path.Combine(saveFolder, fileName + "." + saveExtension);

#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.LogWarning("SaveManager.Load() not available on WebGL. Used LoadAsync() instead");
        return null;
#else
        // File Loading Data
        if (File.Exists(filepath))
            return File.ReadAllText(filepath);
        else
            return null;    // No available data to load
#endif
    }

    public static IEnumerator LoadAsync(GhostPaths ghostPath, System.Action<string> onLoaded)
    {
        string fileName = (ghostPath == GhostPaths.WallJumpingPath) ?
                "WallJumpingPath" :
                "HookPath";

        string filepath = Path.Combine(saveFolder, fileName + "." + saveExtension);

#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log($"[SaveManager] Trying to load JSON from: {filepath}");
        UnityWebRequest request = UnityWebRequest.Get(filepath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[SaveManager] JSON loaded successfully from: {request.url}");
            onLoaded?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"[SaveManager] Error loading JSON on WebGL: {request.error}\nURL: {request.url}");
            onLoaded?.Invoke(null);
        }   
#else
        // File Loading Data
        if (File.Exists(filepath)) 
        {
            string json = File.ReadAllText(filepath);
            onLoaded?.Invoke(json);
        }
        else
        {
            Debug.LogError($"File not found {filepath}");
            onLoaded?.Invoke(null);
        }
        yield break;
#endif
    }
}
