using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EditorStructure contains all serializable values to restore an editor scene.
/// </summary>
[Serializable]
public class EditorStructure
{
    /// <summary>
    /// Whether grid snapping is enabled.
    /// </summary>
    [SerializeField]
    bool inGridMode;

    /// <summary>
    /// The list of circuits determined to be powered inputs.
    /// </summary>
    [SerializeField]
    List<bool> isPoweredInput = new List<bool>();

    /// <summary>
    /// The list of circuit identifiers pertaining to each circuit within the scene.
    /// </summary>
    [SerializeField]
    List<CircuitIdentifier> circuits = new List<CircuitIdentifier>();

    /// <summary>
    /// The circuit index representation of all bookmarked circuits.<br/><br/>
    /// All indeces except that of custom circuits are unique.
    /// </summary>
    [SerializeField]
    List<int> bookmarks = new List<int>();

    /// <summary>
    /// The custom circuit ID representation of all bookmarked circuits.<br/><br/>
    /// If a bookmarked circuit is not a custom circuit, -1 is utilized as a corresponding identifier.
    /// </summary>
    [SerializeField]
    List<int> bookmarkIDs = new List<int>();

    /// <summary>
    /// Name of the editor scene assigned when a save slot is first used.
    /// </summary>
    [SerializeField]
    string name;

    /// <summary>
    /// Location of the camera within the editor scene.
    /// </summary>
    [SerializeField]
    Vector3 cameraLocation;

    public EditorStructure(string name) { this.name = name; }

    // Getter and setter methods
    public bool InGridMode { get { return inGridMode; } set { inGridMode = value; } }

    public List<bool> IsPoweredInput { get { return isPoweredInput; } set { isPoweredInput = value; } }

    public List<CircuitIdentifier> Circuits { get { return circuits; } set { circuits = value; } }

    public List<int> Bookmarks { get { return bookmarks; } set { bookmarks = value; } }

    public List<int> BookmarkIDs { get { return bookmarkIDs; } set { bookmarkIDs = value; } }

    public Vector3 CameraLocation { get { return cameraLocation; } set { cameraLocation = value; } }

    // Getter method
    public string Name { get { return name; } }
}