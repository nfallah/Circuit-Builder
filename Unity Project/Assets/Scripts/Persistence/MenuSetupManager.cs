using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MenuSetupManager : MonoBehaviour
{
    private static MenuSetupManager instance;

    private Type[] componentsToAdd = new Type[]
    {
        typeof(MenuLogicManager)
    };

    private EditorStructure[] editorStructures = new EditorStructure[3];

    private List<PreviewStructure> previewStructures = new List<PreviewStructure>();

    private string editorFolder = "EditorSaves", previewFolder = "PreviewSaves", save1Name = "SAVE_0.json", save2Name = "SAVE_1.json", save3Name = "SAVE_2.json";

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
        ImportJSONInformation();

        foreach (Type type in componentsToAdd) gameObject.AddComponent(type);
    }

    public void DeleteEditorStructure(int sceneIndex)
    {
        editorStructures[sceneIndex] = null;
        
        string editorPath = Application.dataPath + "/" + editorFolder + "/";

        if (sceneIndex == 0) editorPath += save1Name; else if (sceneIndex == 1) editorPath += save2Name; else editorPath += save3Name;

        File.WriteAllText(editorPath, "");
    }

    public void UpdateEditorStructure(int sceneIndex, EditorStructure editorStructure)
    {
        string editorPath = Application.dataPath + "/" + editorFolder + "/";

        if (sceneIndex == 0) editorPath += save1Name; else if (sceneIndex == 1) editorPath += save2Name; else editorPath += save3Name;

        File.WriteAllText(editorPath, JsonUtility.ToJson(editorStructure));
    }

    /// <summary>
    /// Extracts existing JSON data from the game directory to populate editor and preview structures.
    /// </summary>
    private void ImportJSONInformation()
    {
        // Ensures the relevant save folders are created if they were removed
        if (!AssetDatabase.IsValidFolder("Assets/" + editorFolder))
        {
            AssetDatabase.CreateFolder("Assets", editorFolder);
        }

        if (!AssetDatabase.IsValidFolder("Assets/" + previewFolder))
        {
            AssetDatabase.CreateFolder("Assets", previewFolder);
        }

        FileAttributes fileAttributes = FileAttributes.Normal;
        string editorPath = Application.dataPath + "/" + editorFolder + "/";

        // Ensures the relevant save files are created if they were removed
        if (!File.Exists(editorPath + save1Name))
        {
            File.Create(editorPath + save1Name);
            File.SetAttributes(editorPath + save1Name, fileAttributes);
        }

        else
        {
            editorStructures[0] = JsonUtility.FromJson<EditorStructure>(File.ReadAllText(editorPath + save1Name));
        }

        if (!File.Exists(editorPath + save2Name))
        {
            File.Create(editorPath + save2Name);
            File.SetAttributes(editorPath + save2Name, fileAttributes);
        }

        else
        {
            editorStructures[1] = JsonUtility.FromJson<EditorStructure>(File.ReadAllText(editorPath + save2Name));
        }

        if (!File.Exists(editorPath + save3Name))
        {
            File.Create(editorPath + save3Name);
            File.SetAttributes(editorPath + save3Name, fileAttributes);
        }

        else
        {
            editorStructures[2] = JsonUtility.FromJson<EditorStructure>(File.ReadAllText(editorPath + save3Name));
        }
    }

    // Getter methods
    public static MenuSetupManager Instance { get { return instance; } }

    public EditorStructure[] EditorStructures { get { return editorStructures; } }

    public List<PreviewStructure> PreviewStructures { get { return previewStructures; } }
}