using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;                    // Needed to work with R/W data on JSON files

public static class SaveManager
{
    // Folder Path were the game files will be saved
    private static readonly string saveFolder = Application.dataPath + "/Saves/";
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
        File.WriteAllText(saveFolder + "PlayerPath" + "." + saveExtension, saveString);
    }

    public static string Load()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(saveFolder);

        // Get the *.txt files found on the directory and access to the last saving file
        FileInfo[] saveFiles = directoryInfo.GetFiles("*." + saveExtension);
        FileInfo mostRecentFile = null;
        foreach (FileInfo fileInfo in saveFiles)
        {
            if(mostRecentFile == null)
                mostRecentFile = fileInfo;
            else
            {
                if(fileInfo.LastWriteTime > mostRecentFile.LastWriteTime)
                    mostRecentFile = fileInfo;
            }
        }

        // File Loading Data
        if (mostRecentFile != null)
        {
            string saveString = File.ReadAllText(mostRecentFile.FullName);
            return saveString;
        }
        else
            return null;    // No available data to load
    }
}
