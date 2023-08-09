using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// MenuSetupManager serves as the primary script for persistence and communication between the menu and editor/preview scenes.
/// </summary>
public class MenuSetupManager : MonoBehaviour
{
    // Singleton state reference
    private static MenuSetupManager instance;

    /// <summary>
    /// The list of persistent scripts that should be loaded after MenuSetupManager.
    /// </summary>
    private Type[] componentsToAdd = new Type[]
    {
        typeof(MenuLogicManager)
    };

    /// <summary>
    /// The list of editor scenes that exist within the project.
    /// </summary>
    private EditorStructure[] editorStructures = new EditorStructure[3];

    private FileAttributes fileAttributes = FileAttributes.Normal;

    /// <summary>
    /// List of custom circuit IDs corresponding to each element within <seealso cref="previewStructures"/>.<br/><br/>
    /// This list is primarily utilized to find a <see cref="PreviewStructure"/> within <seealso cref="previewStructures"/> through the IndexOf() method.
    /// </summary>
    private List<int> previewStructureIDs = new List<int>();

    /// <summary>
    /// The list of custom circuits created by the user.<br/><br/>
    /// Note: a <see cref="PreviewStructure"/> is synonymous with a custom circuit; however a preview structure tends to refer to the actual scene where the custom circuit can be internally viewed.
    /// </summary>
    private List<PreviewStructure> previewStructures = new List<PreviewStructure>();

    /// <summary>
    /// Static constants representing folder names and file names used for serialization.
    /// </summary>
    private readonly string editorFolder = "EditorSaves",
        editorPrefab1Name = "PREFABS_0",
        editorPrefab2Name = "PREFABS_1",
        editorPrefab3Name = "PREFABS_2",
        previewFolder = "PreviewSaves",
        previewSubdirectory = "CUSTOM_",
        save1Name = "SAVE_0.json",
        save2Name = "SAVE_1.json",
        save3Name = "SAVE_2.json";

    // Enforces a singleton state pattern and imports all serialized information.
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
    /// Deletes a requested editor scene within the running game as well as in the save directory.
    /// </summary>
    /// <param name="sceneIndex">The editor scene to delete (0-2).</param>
    public void DeleteEditorStructure(int sceneIndex)
    {
        editorStructures[sceneIndex] = null;
        
        // Obtains the save path pertaining to the requested editor scene.
        string editorPath = Application.persistentDataPath + "/" + editorFolder + "/";
        string prefabPath = Application.persistentDataPath + "/" + editorFolder + "/";

        if (sceneIndex == 0) prefabPath += editorPrefab1Name; else if (sceneIndex == 1) prefabPath += editorPrefab2Name; else prefabPath += editorPrefab3Name;

        if (sceneIndex == 0) editorPath += save1Name; else if (sceneIndex == 1) editorPath += save2Name; else editorPath += save3Name;

        File.WriteAllText(editorPath, ""); // Deletes the editor structure JSON file
        prefabPath += "/";

        // Deletes all connection JSON files, if any
        string[] filePaths = Directory.GetFiles(prefabPath);

        foreach (string file in filePaths) File.Delete(file);
    }

    /// <summary>
    /// Overrides a requested editor scene as a consequence of a new save.
    /// </summary>
    /// <param name="sceneIndex">The editor scene to update (0-2).</param>
    /// <param name="editorStructure">The editor scene object to update.</param>
    public void UpdateEditorStructure(int sceneIndex, EditorStructure editorStructure)
    {
        string editorPath = Application.persistentDataPath + "/" + editorFolder + "/";

        if (sceneIndex == 0) editorPath += save1Name; else if (sceneIndex == 1) editorPath += save2Name; else editorPath += save3Name;

        File.WriteAllText(editorPath, JsonUtility.ToJson(editorStructure));
    }

    /// <summary>
    /// Overrides a requested preview structure as a consequence of a new save.
    /// </summary>
    /// <param name="previewStructure">The preview structure object to update.</param>
    public void UpdatePreviewStructure(PreviewStructure previewStructure)
    {
        string previewPath = Application.persistentDataPath + "/" + previewFolder + "/" + previewSubdirectory + previewStructure.ID + "/SAVE.json";

        File.WriteAllText(previewPath, JsonUtility.ToJson(previewStructure));
    }

    /// <summary>
    /// Serializes all connections pertaining to either a preview or editor structure.
    /// </summary>
    /// <param name="isEditor">Whether the referenced connections belong to an editor scene.</param>
    /// <param name="generateIndex">The editor scene to update (0-2) if the referenced connections belong to an editor scene.</param>
    /// <param name="connections">The connections to serialize.</param>
    public void GenerateConnections(bool isEditor, int generateIndex, List<CircuitConnector.Connection> connections)
    {
        // Obtains the path to save all connections to.
        string path = Application.persistentDataPath + "/" + (isEditor ? editorFolder : previewFolder) + "/";

        if (isEditor)
        {
            if (generateIndex == 0) path += editorPrefab1Name; else if (generateIndex == 1) path += editorPrefab2Name; else path += editorPrefab3Name;
        }

        else
        {
            path += previewSubdirectory + generateIndex;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/" + previewFolder + "/" + previewSubdirectory + generateIndex);
            }
        }

        // In case the folder is already populated, all files are cleared from the obtained directory.
        string[] filePaths = Directory.GetFiles(path);
        
        foreach (string file in filePaths) File.Delete(file);

        // No point in continuing if there are no connections.
        if (connections.Count == 0) return;

        int index = 0;

        // Traverses through each connection and generates a corresponding JSON file.
        foreach (CircuitConnector.Connection connection in connections)
        {
            int inputCircuitIndex;
            int outputCircuitIndex;
            int inputIndex;
            int outputIndex;

            // Runs if the input belongs to a custom circuit
            // customCircuit == null --> a non-custom circuit
            if (connection.Input.ParentCircuit.customCircuit != null)
            {
                /* A custom circuit can be put inside of another custom circuit recursively.
                 * Therefore, to obtain the top-most (actual) custom circuit located in the scene, some calculations must occur.
                 * The primary condition for this is to keep accessing the custom circuit of the parent until it is null.
                 * If it is null, that means the current custom circuit is at the top-most level.
                 * This essentially emulates a linked-list property, where the head is the node with no parent.
                 */
                Circuit actualCircuit = connection.Input.ParentCircuit.customCircuit;

                // Obtains the top-most custom circuit
                while (actualCircuit.customCircuit != null) actualCircuit = actualCircuit.customCircuit;

                inputCircuitIndex = EditorStructureManager.Instance.Circuits.IndexOf(actualCircuit);
                inputIndex = Array.IndexOf(actualCircuit.Inputs, connection.Input);
            }

            // Runs if the input belongs to a non-custom circuit
            else
            {
                inputCircuitIndex = EditorStructureManager.Instance.Circuits.IndexOf(connection.Input.ParentCircuit);
                inputIndex = Array.IndexOf(connection.Input.ParentCircuit.Inputs, connection.Input);
            }

            // Runs if the output belongs to a custom circuit
            if (connection.Output.ParentCircuit.customCircuit != null)
            {
                Circuit actualCircuit = connection.Output.ParentCircuit.customCircuit;

                while (actualCircuit.customCircuit != null) actualCircuit = actualCircuit.customCircuit;

                outputCircuitIndex = EditorStructureManager.Instance.Circuits.IndexOf(actualCircuit);
                outputIndex = Array.IndexOf(actualCircuit.Outputs, connection.Output);
            }

            // Runs if the output belongs to a non-custom circuit
            else
            {
                outputCircuitIndex = EditorStructureManager.Instance.Circuits.IndexOf(connection.Output.ParentCircuit);
                outputIndex = Array.IndexOf(connection.Output.ParentCircuit.Outputs, connection.Output);
            }

            // Creates a corresponding connection identifier from the obtained indeces and saves to the obtained directory.
            CircuitConnectorIdentifier circuitConnectionIdentifier = new CircuitConnectorIdentifier(inputCircuitIndex, outputCircuitIndex, inputIndex, outputIndex);
            ConnectionSerializer.SerializeConnection(connection, circuitConnectionIdentifier, path + "/CONNECTION_" + index + ".json");
            index++;
        }
    }

    /// <summary>
    /// Editor structure variation of the RestoreConnections() method utilized to restore serialized connections.
    /// </summary>
    /// <param name="sceneIndex">The editor scene to delete (0-2).</param>
    public void RestoreConnections(int sceneIndex)
    {
        // Obtains the path to access the connection JSON files from.
        string prefabPath = Application.persistentDataPath + "/" + editorFolder + "/";

        if (sceneIndex == 0) prefabPath += editorPrefab1Name; else if (sceneIndex == 1) prefabPath += editorPrefab2Name; else prefabPath += editorPrefab3Name;

        prefabPath += "/";

        // Calls the primary method with the obtained directory.
        RestoreConnections(prefabPath, true);
    }

    /// <summary>
    /// Preview structure variation of the RestoreConnections() method utilized to restore serialized connections.
    /// </summary>
    /// <param name="previewStructure">The preview structure object to access.</param>
    public void RestoreConnections(PreviewStructure previewStructure) { RestoreConnections(Application.persistentDataPath + "/" + previewFolder + "/" + previewSubdirectory + previewStructure.ID + "/", false); }

    /// <summary>
    /// Deserializes saved connections and restores them to the relevant scene.
    /// </summary>
    /// <param name="prefabPath">The path to access the serialized connection files from.</param>
    /// <param name="isEditor">Whether the path points to an editor structure.</param>
    private void RestoreConnections(string prefabPath, bool isEditor)
    {
        string[] filePaths = Directory.GetFiles(prefabPath);

        // Ensures only JSON files are being accessed.
        List<string> prefabFilePaths = filePaths.Where(filePath => filePath.EndsWith(".json")).ToList();

        // Iterates through each connection file and restores it to the scene.
        foreach (string prefabFilePath in prefabFilePaths)
        {
            // Implies the current JSON is a save file rather than a connection file, and should therefore be skipped.
            if (prefabFilePath.EndsWith("SAVE.json")) continue;

            // Simultaneously creates the connection mesh as well as all relevant values in the form of a ConnectionSerializerRestorer object
            ConnectionSerializerRestorer connectionParent = CircuitVisualizer.Instance.VisualizeConnection(JsonUtility.FromJson<ConnectionSerializer>(File.ReadAllText(prefabFilePath)));

            // Depending on whether the scene is in the editor or not, the method of obtaining the connection's input and output will differ.
            Circuit.Input input = isEditor ?
                EditorStructureManager.Instance.Circuits[connectionParent.circuitConnectorIdentifier.InputCircuitIndex].Inputs[connectionParent.circuitConnectorIdentifier.InputIndex] :
                PreviewManager.Instance.Circuits[connectionParent.circuitConnectorIdentifier.InputCircuitIndex].Inputs[connectionParent.circuitConnectorIdentifier.InputIndex];
            Circuit.Output output = isEditor ?
                EditorStructureManager.Instance.Circuits[connectionParent.circuitConnectorIdentifier.OutputCircuitIndex].Outputs[connectionParent.circuitConnectorIdentifier.OutputIndex] :
                PreviewManager.Instance.Circuits[connectionParent.circuitConnectorIdentifier.OutputCircuitIndex].Outputs[connectionParent.circuitConnectorIdentifier.OutputIndex];

            // Names and restores the connection within the scene by setting the parent circuits of the input and output.
            connectionParent.parentObject.name = "Connection";
            CircuitConnector.ConnectRestoration(connectionParent.parentObject, input, output, connectionParent.endingWire, connectionParent.startingWire, isEditor);
        }
    }

    /// <summary>
    /// Deletes a requested preview structure within the running game as well as in the save directory.
    /// </summary>
    /// <param name="previewStructure">The preview structure object to delete.</param>
    public void DeletePreviewStructure(PreviewStructure previewStructure)
    {
        int index = previewStructures.IndexOf(previewStructure);
        string folderPath = Application.persistentDataPath + "/" + previewFolder + "/" + previewSubdirectory + previewStructure.ID;

        previewStructures.Remove(previewStructure);
        previewStructureIDs.Remove(previewStructureIDs[index]);

        string[] filePaths = Directory.GetFiles(folderPath + "/");

        foreach (string file in filePaths) File.Delete(file);

        Directory.Delete(folderPath);
    }

    /// <summary>
    /// Extracts existing JSON data from the game directory to populate all editor and preview structures.<br/><br/>
    /// This method is called on startup before anything else.
    /// </summary>
    private void ImportJSONInformation()
    {
        // Ensures the base editor and preview save folders are created if they were removed
        if (!Directory.Exists(Application.persistentDataPath + "/" + editorFolder))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + editorFolder);
        }

        if (!Directory.Exists(Application.persistentDataPath + "/" + previewFolder))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + previewFolder);
        }

        //  Ensures the editor subdirectory save folders are created if they were removed
        if (!Directory.Exists(Application.persistentDataPath + "/" + editorFolder + "/" + editorPrefab1Name))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + editorFolder + "/" + editorPrefab1Name);
        }

        if (!Directory.Exists(Application.persistentDataPath + "/" + editorFolder + "/" + editorPrefab2Name))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + editorFolder + "/" + editorPrefab2Name);
        }

        if (!Directory.Exists(Application.persistentDataPath + "/" + editorFolder + "/" + editorPrefab3Name))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + editorFolder + "/" + editorPrefab3Name);
        }

        string editorPath = Application.persistentDataPath + "/" + editorFolder + "/";

        // Ensures the relevant editor JSON save files are created if they were removed, otherwise they are loaded into the game.
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

        string[] previewFilePaths = Directory.GetDirectories(Application.persistentDataPath + "/" + previewFolder);

        // Traverses through all valid preview save files and loads them into the game.
        foreach (string filePath in previewFilePaths)
        {
            string[] previewFiles = Directory.GetFiles(filePath);
            string jsonFile = previewFiles.FirstOrDefault(s => s.EndsWith("SAVE.json"));

            if (jsonFile == null) throw new Exception("Preview structure JSON modified outside the script; terminating.");

            try
            {
                PreviewStructure previewStructure = JsonUtility.FromJson<PreviewStructure>(File.ReadAllText(jsonFile));

                previewStructures.Add(previewStructure);
                PreviewStructureIDs.Add(previewStructure.ID);
            }

            catch
            {
                throw new Exception("Preview structure JSON modified outside the script; terminating.");
            }
        }
    }

    // Getter methods
    public static MenuSetupManager Instance { get { return instance; } }

    public EditorStructure[] EditorStructures { get { return editorStructures; } }

    public List<int> PreviewStructureIDs { get { return previewStructureIDs; } }

    public List<PreviewStructure> PreviewStructures { get { return previewStructures; } }
}