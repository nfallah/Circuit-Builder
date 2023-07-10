using System;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class EditorStructureManager : MonoBehaviour
{
    private static EditorStructureManager instance;

    [HideInInspector] List<Circuit> circuits = new List<Circuit>();

    [HideInInspector] List<int> bookmarks = new List<int>();
    
    [HideInInspector] List<CircuitConnector.Connection> connections = new List<CircuitConnector.Connection>();

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

    public void Serialize()
    {
        int sceneIndex = MenuLogicManager.Instance.CurrentSceneIndex;
        EditorStructure editorStructure = MenuSetupManager.Instance.EditorStructures[sceneIndex];

        editorStructure.InGridMode = Coordinates.Instance.CurrentSnappingMode == Coordinates.SnappingMode.GRID;
        
        List<StartingCircuitIdentifier> startingCircuitIdentifiers = new List<StartingCircuitIdentifier>();

        foreach (Circuit circuit in circuits)
        {
            startingCircuitIdentifiers.Add(new StartingCircuitIdentifier(circuit));
        }

        editorStructure.Circuits = startingCircuitIdentifiers;
        editorStructure.Bookmarks = bookmarks;
        editorStructure.Connections = connections;
        editorStructure.CameraLocation = CameraMovement.Instance.PlayerCamera.transform.position;
        MenuSetupManager.Instance.UpdateEditorStructure(sceneIndex, editorStructure);
        MenuSetupManager.Instance.GenerateConnectionPrefabs(sceneIndex, connections);
    }

    public void Deserialize()
    {
        int sceneIndex = MenuLogicManager.Instance.CurrentSceneIndex;
        EditorStructure editorStructure = MenuSetupManager.Instance.EditorStructures[sceneIndex];

        if (MenuLogicManager.Instance.FirstOpen)
        {
            bool inGridMode = Coordinates.Instance.CurrentSnappingMode == Coordinates.SnappingMode.GRID;
            Vector3 cameraLocation = CameraMovement.Instance.PlayerCamera.transform.position;

            editorStructure.InGridMode = inGridMode;
            editorStructure.CameraLocation = cameraLocation;
            MenuSetupManager.Instance.UpdateEditorStructure(sceneIndex, editorStructure);
            return;
        }

        Coordinates.Instance.CurrentSnappingMode = editorStructure.InGridMode ? Coordinates.SnappingMode.GRID : Coordinates.SnappingMode.NONE;
        CameraMovement.Instance.PlayerCamera.transform.position = editorStructure.CameraLocation;

        foreach (StartingCircuitIdentifier startingCircuitIdentifier in editorStructure.Circuits)
        {
            Circuits.Add(StartingCircuitIdentifier.RestoreCircuit(startingCircuitIdentifier));
        }

        MenuSetupManager.Instance.RestoreConnections(sceneIndex);
        TaskbarManager.Instance.RestoreBookmarks(editorStructure.Bookmarks);
    }

    // Singleton state reference
    public static EditorStructureManager Instance { get { return instance; } }

    // Getter methods
    public List<Circuit> Circuits { get { return circuits; } }

    public List<int> Bookmarks { get { return bookmarks; } }

    public List<CircuitConnector.Connection> Connections { get { return connections; } }
}