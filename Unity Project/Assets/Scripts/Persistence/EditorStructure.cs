using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EditorStructure
{
    [SerializeField] bool inGridMode;

    [SerializeField] List<GameObject> circuits = new List<GameObject>(), connections = new List<GameObject>();

    [SerializeField] string name;

    [SerializeField] Vector3 cameraLocation;

    public EditorStructure(string name)
    {
        this.name = name;
    }

    // Getter and setter methods
    public bool InGridMode { get { return inGridMode; } set { inGridMode = value; } }

    public List<GameObject> Circuits { get { return circuits; } set { circuits = value; } }

    public List<GameObject> Connections { get { return connections; } set { connections = value; } }

    public Vector3 CameraLocation { get { return cameraLocation; } set { cameraLocation = value; } }

    // Getter method
    public string Name { get { return name; } }
}