using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EditorStructure
{
    [SerializeField] bool inGridMode;

    [SerializeField] List<CircuitConnector.Connection> connections = new List<CircuitConnector.Connection>();

    [SerializeField] List<int> bookmarks = new List<int>();

    [SerializeField] List<StartingCircuitIdentifier> circuits = new List<StartingCircuitIdentifier>();

    [SerializeField] string name;

    [SerializeField] Vector3 cameraLocation;

    public EditorStructure(string name)
    {
        this.name = name;
    }

    // Getter and setter methods
    public bool InGridMode { get { return inGridMode; } set { inGridMode = value; } }

    public List<CircuitConnector.Connection> Connections { get { return connections; } set { connections = value; } }

    public List<int> Bookmarks { get { return bookmarks; } set { bookmarks = value; } }

    public List<StartingCircuitIdentifier> Circuits { get { return circuits; } set { circuits = value; } }

    public Vector3 CameraLocation { get { return cameraLocation; } set { cameraLocation = value; } }

    // Getter method
    public string Name { get { return name; } }
}