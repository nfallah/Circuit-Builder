using System;
using System.Collections.Generic;
using UnityEngine;

public class PersistentComponentsMenu : MonoBehaviour
{
    private static PersistentComponentsMenu instance;

    private Type[] componentsToAdd = new Type[]
    {
        typeof(MenuManager)
    };

    private EditorStructure[] editorStructures = new EditorStructure[3];

    private List<PreviewStructure> previewStructures = new List<PreviewStructure>();

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

    /// <summary>
    /// Extracts existing JSON data from directory to populate editor and preview structures.
    /// </summary>
    private void ImportJSONInformation()
    {

    }

    // Getter methods
    public static PersistentComponentsMenu Instance { get { return instance; } }
}