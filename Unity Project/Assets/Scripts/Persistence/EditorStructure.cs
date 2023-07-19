using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EditorStructure
{
    [SerializeField] bool inGridMode;

    [SerializeField] List<bool> isPoweredInput = new List<bool>();

    [SerializeField] List<int> bookmarks = new List<int>();

    [SerializeField] List<CircuitIdentifier> circuits = new List<CircuitIdentifier>();

    [SerializeField] string name;

    [SerializeField] Vector3 cameraLocation;

    public EditorStructure(string name)
    {
        this.name = name;
    }

    // Getter and setter methods
    public bool InGridMode { get { return inGridMode; } set { inGridMode = value; } }

    public List<bool> IsPoweredInput { get { return isPoweredInput; } set { isPoweredInput = value; } }

    public List<int> Bookmarks { get { return bookmarks; } set { bookmarks = value; } }

    public List<CircuitIdentifier> Circuits { get { return circuits; } set { circuits = value; } }

    public Vector3 CameraLocation { get { return cameraLocation; } set { cameraLocation = value; } }

    // Getter method
    public string Name { get { return name; } }
}