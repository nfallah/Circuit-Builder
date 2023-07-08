using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorStructureManager : MonoBehaviour
{
    private static EditorStructureManager instance;

    [HideInInspector] bool inGridMode;

    [HideInInspector] List<GameObject> circuits = new List<GameObject>(); 
    
    [HideInInspector] List<GameObject> connections = new List<GameObject>();

    [HideInInspector] Vector3 cameraLocation;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("EditorStructureManager instance already established; terminating.");
        }

        instance = this;
    }

    private void Start()
    {
        Deserialize();
    }

    public void Serialize() //todo: grab grid and cameralocation
    {
        EditorStructure editorStructure = MenuSetupManager.Instance.EditorStructures[MenuLogicManager.Instance.CurrentSceneIndex];

        editorStructure.InGridMode= inGridMode;
        editorStructure.Circuits = circuits;
        editorStructure.Connections = connections;

        string json = JsonUtility.ToJson(editorStructure);

        Debug.Log(json);
    }

    public void Deserialize()
    {
        //EditorStructure editorStructure = MenuSetupManager.Instance.EditorStructures[MenuLogicManager.Instance.CurrentSceneIndex]; // change

        //inGridMode = editorStructure
    }

    // Singleton state reference
    public static EditorStructureManager Instance { get { return instance; } }

    // Getter and setter methods
    public bool InGridMode { get { return inGridMode; } set { inGridMode = value; } }

    Vector3 CameraLocation { get { return cameraLocation; } set { cameraLocation = value; } }

    // Getter methods
    public List<GameObject> Circuits { get { return circuits; } }

    public List<GameObject> Connections { get { return connections; } }
}