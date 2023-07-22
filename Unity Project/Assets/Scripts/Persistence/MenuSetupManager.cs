using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    private FileAttributes fileAttributes = FileAttributes.Normal;

    private List<int> previewStructureIDs = new List<int>();

    private List<PreviewStructure> previewStructures = new List<PreviewStructure>();

    private string editorFolder = "EditorSaves", previewFolder = "PreviewSaves", previewSubdirectory = "CUSTOM_", save1Name = "SAVE_0.json", save2Name = "SAVE_1.json", save3Name = "SAVE_2.json";

    private string editorPrefab1Name = "PREFABS_0", editorPrefab2Name = "PREFABS_1", editorPrefab3Name = "PREFABS_2";

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
        string prefabPath = "Assets/" + editorFolder + "/";

        if (sceneIndex == 0) prefabPath += editorPrefab1Name; else if (sceneIndex == 1) prefabPath += editorPrefab2Name; else prefabPath += editorPrefab3Name;

        if (sceneIndex == 0) editorPath += save1Name; else if (sceneIndex == 1) editorPath += save2Name; else editorPath += save3Name;

        File.WriteAllText(editorPath, "");
        prefabPath += "/";

        string[] filePaths = Directory.GetFiles(prefabPath);

        foreach (string file in filePaths)
        {
            FileUtil.DeleteFileOrDirectory(file);
        }

        AssetDatabase.Refresh();
    }

    public void UpdateEditorStructure(int sceneIndex, EditorStructure editorStructure)
    {
        string editorPath = Application.dataPath + "/" + editorFolder + "/";

        if (sceneIndex == 0) editorPath += save1Name; else if (sceneIndex == 1) editorPath += save2Name; else editorPath += save3Name;

        File.WriteAllText(editorPath, JsonUtility.ToJson(editorStructure));
    }

    public void UpdatePreviewStructure(PreviewStructure previewStructure)
    {
        string previewPath = Application.dataPath + "/" + previewFolder + "/" + previewSubdirectory + previewStructure.ID + "/SAVE.json";

        File.WriteAllText(previewPath, JsonUtility.ToJson(previewStructure));
    }

    public void GenerateConnections(bool isEditor, int generateIndex, List<CircuitConnector.Connection> connections)
    {
        string path = "Assets/" + (isEditor ? editorFolder : previewFolder) + "/";

        if (isEditor)
        {
            if (generateIndex == 0) path += editorPrefab1Name; else if (generateIndex == 1) path += editorPrefab2Name; else path += editorPrefab3Name;
        }

        else
        {
            path += previewSubdirectory + generateIndex;

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/" + previewFolder, previewSubdirectory + generateIndex);
            }
        }

        path += "/";

        string[] filePaths = Directory.GetFiles(path);
        
        foreach (string file in filePaths)
        {
            FileUtil.DeleteFileOrDirectory(file);
        }

        if (connections.Count == 0) return;

        int index = 0;

        foreach (CircuitConnector.Connection connection in connections)
        {
            int inputCircuitIndex;
            int outputCircuitIndex;
            int inputIndex;
            int outputIndex;

            if (connection.Input.ParentCircuit.customCircuit != null)
            {
                Circuit actualCircuit = connection.Input.ParentCircuit.customCircuit;

                while (actualCircuit.customCircuit != null)
                {
                    actualCircuit = actualCircuit.customCircuit;
                }

                inputCircuitIndex = EditorStructureManager.Instance.Circuits.IndexOf(actualCircuit);
                inputIndex = Array.IndexOf(actualCircuit.Inputs, connection.Input);
            }

            else
            {
                inputCircuitIndex = EditorStructureManager.Instance.Circuits.IndexOf(connection.Input.ParentCircuit);
                inputIndex = Array.IndexOf(connection.Input.ParentCircuit.Inputs, connection.Input);
            }

            if (connection.Output.ParentCircuit.customCircuit != null)
            {
                Circuit actualCircuit = connection.Output.ParentCircuit.customCircuit;

                while (actualCircuit.customCircuit != null)
                {
                    actualCircuit = actualCircuit.customCircuit;
                }

                outputCircuitIndex = EditorStructureManager.Instance.Circuits.IndexOf(actualCircuit);
                outputIndex = Array.IndexOf(actualCircuit.Outputs, connection.Output);
            }

            else
            {
                outputCircuitIndex = EditorStructureManager.Instance.Circuits.IndexOf(connection.Output.ParentCircuit);
                outputIndex = Array.IndexOf(connection.Output.ParentCircuit.Outputs, connection.Output);
            }

            GameObject temp = Instantiate(connection.gameObject);
            ConnectionIdentifier connectionIdentifier = temp.AddComponent<ConnectionIdentifier>();

            DestroyImmediate(temp.GetComponent<CircuitConnector.Connection>());

            // Has 3 or more connections, meaning mesh optimization occurs & a mesh must be created.
            if (temp.GetComponent<MeshFilter>() != null)
            {
                temp.GetComponent<MeshRenderer>().material = CircuitVisualizer.Instance.PowerOffMaterial;
                AssetDatabase.CreateAsset(temp.GetComponent<MeshFilter>().mesh, path + "MESH_" + index + ".mesh");
            }

            if (temp.transform.childCount == 1)
            {
                connectionIdentifier.EndingWire = connectionIdentifier.StartingWire = temp.transform.GetChild(0).gameObject;
            }

            else
            {
                connectionIdentifier.EndingWire = temp.transform.GetChild(1).gameObject;
                connectionIdentifier.StartingWire = temp.transform.GetChild(0).gameObject;
            }

            connectionIdentifier.EndingWire.GetComponentInChildren<MeshRenderer>().material = connectionIdentifier.StartingWire.GetComponentInChildren<MeshRenderer>().material = CircuitVisualizer.Instance.PowerOffMaterial;
            connectionIdentifier.CircuitConnectorIdentifier = new CircuitConnectorIdentifier(inputCircuitIndex, outputCircuitIndex, inputIndex, outputIndex);
            PrefabUtility.SaveAsPrefabAsset(temp, path + "CONNECTION_" + index + ".prefab");
            DestroyImmediate(temp);
            index++;
        }
    }

    public void RestoreConnections(int sceneIndex)
    {
        string prefabPath = "Assets/" + editorFolder + "/";

        if (sceneIndex == 0) prefabPath += editorPrefab1Name; else if (sceneIndex == 1) prefabPath += editorPrefab2Name; else prefabPath += editorPrefab3Name;

        prefabPath += "/";
        RestoreConnections(prefabPath, true);
    }

    public void RestoreConnections(PreviewStructure previewStructure)
    {
        RestoreConnections("Assets/" + previewFolder + "/" + previewSubdirectory + previewStructure.ID + "/", false);
    }

    public void DeletePreviewStructure(PreviewStructure previewStructure)
    {
        int index = previewStructures.IndexOf(previewStructure);
        string folderPath = "Assets/" + previewFolder + "/" + previewSubdirectory + previewStructure.ID;

        previewStructures.Remove(previewStructure); previewStructureIDs.Remove(previewStructureIDs[index]);

        string[] filePaths = Directory.GetFiles(folderPath + "/");

        foreach (string file in filePaths)
        {
            FileUtil.DeleteFileOrDirectory(file);
        }

        FileUtil.DeleteFileOrDirectory(folderPath + ".meta");
        FileUtil.DeleteFileOrDirectory(folderPath);
        AssetDatabase.Refresh();
    }

    private void RestoreConnections(string prefabPath, bool isEditor)
    {
        string[] filePaths = Directory.GetFiles(prefabPath);
        List<string> prefabFilePaths = filePaths.Where(filePath => filePath.EndsWith(".prefab")).ToList();

        foreach (string prefabFilePath in prefabFilePaths)
        {
            GameObject prefab = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabFilePath));
            ConnectionIdentifier connectionIdentifier = prefab.GetComponent<ConnectionIdentifier>();
            Circuit.Input input = isEditor ?
                EditorStructureManager.Instance.Circuits[connectionIdentifier.CircuitConnectorIdentifier.InputCircuitIndex].Inputs[connectionIdentifier.CircuitConnectorIdentifier.InputIndex] :
                PreviewManager.Instance.Circuits[connectionIdentifier.CircuitConnectorIdentifier.InputCircuitIndex].Inputs[connectionIdentifier.CircuitConnectorIdentifier.InputIndex];
            Circuit.Output output = isEditor ?
                EditorStructureManager.Instance.Circuits[connectionIdentifier.CircuitConnectorIdentifier.OutputCircuitIndex].Outputs[connectionIdentifier.CircuitConnectorIdentifier.OutputIndex] :
                PreviewManager.Instance.Circuits[connectionIdentifier.CircuitConnectorIdentifier.OutputCircuitIndex].Outputs[connectionIdentifier.CircuitConnectorIdentifier.OutputIndex];

            prefab.name = "Connection";
            CircuitConnector.ConnectRestoration(prefab, input, output, connectionIdentifier.EndingWire, connectionIdentifier.StartingWire, isEditor);
            Destroy(connectionIdentifier);
        }
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

        if (!AssetDatabase.IsValidFolder("Assets/" + editorFolder + "/" + editorPrefab1Name))
        {
            AssetDatabase.CreateFolder("Assets/" + editorFolder, editorPrefab1Name);
        }

        if (!AssetDatabase.IsValidFolder("Assets/" + editorFolder + "/" + editorPrefab2Name))
        {
            AssetDatabase.CreateFolder("Assets/" + editorFolder, editorPrefab2Name);
        }

        if (!AssetDatabase.IsValidFolder("Assets/" + editorFolder + "/" + editorPrefab3Name))
        {
            AssetDatabase.CreateFolder("Assets/" + editorFolder, editorPrefab3Name);
        }

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

        string[] previewFilePaths = AssetDatabase.GetSubFolders("Assets/" + previewFolder);

        foreach (string filePath in previewFilePaths)
        {
            string[] previewFiles = Directory.GetFiles(filePath);
            string jsonFile = previewFiles.FirstOrDefault(s => s.EndsWith(".json"));

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