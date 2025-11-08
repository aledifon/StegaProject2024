using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        // File Loading Data
        if (File.Exists(filepath))        
            return File.ReadAllText(filepath);                    
        else
            return null;    // No available data to load
    }
}
