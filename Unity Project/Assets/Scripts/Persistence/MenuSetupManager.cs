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

    private string editorFolder = "EditorSaves", previewFolder = "PreviewSaves", save1Name = "SAVE_0", save2Name = "SAVE_1", save3Name = "SAVE_2";

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
    }

    /// <summary>
    /// Extracts existing JSON data from the game directory to populate editor and preview structures.
    /// </summary>
    private void ImportJSONInformation()
    {
        if (!AssetDatabase.IsValidFolder("Assets/" + editorFolder))
        {
            Debug.Log("fok");
            AssetDatabase.CreateFolder("Assets/", editorFolder);
        }

        if (AssetDatabase.IsValidFolder("Assets/" + previewFolder))
        {
            AssetDatabase.CreateFolder("Assets/", previewFolder);
        }
    }

    // Getter methods
    public static MenuSetupManager Instance { get { return instance; } }

    public EditorStructure[] EditorStructures { get { return editorStructures; } }

    public List<PreviewStructure> PreviewStructures { get { return previewStructures; } }
}