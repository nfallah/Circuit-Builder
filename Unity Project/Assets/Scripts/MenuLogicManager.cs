using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLogicManager : MonoBehaviour
{
    private static MenuLogicManager instance;

    private bool firstOpen;

    private int currentSceneIndex;

    private PreviewStructure currentPreviewStructure;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("MenuManager instance already established; terminating.");
        }

        instance = this;
    }

    public void OpenScene(int sceneIndex)
    {
        bool isCreated = MenuSetupManager.Instance.EditorStructures[sceneIndex] != null;

        firstOpen = !isCreated;

        if (isCreated) ImportScene(sceneIndex); else MenuInterfaceManager.Instance.BeginSceneNameSubmission(sceneIndex);
    }

    public void CreateScene(int sceneIndex, string name)
    {
        MenuSetupManager.Instance.EditorStructures[sceneIndex] = new EditorStructure(name);
        ImportScene(sceneIndex);
    }

    public void OpenPreview(PreviewStructure previewStructure)
    {
        currentPreviewStructure = previewStructure;
        SceneManager.LoadScene(2);
    }

    private void ImportScene(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;
        SceneManager.LoadScene(1);
    }

    public static bool CanDeleteCustomCircuit(PreviewStructure _previewStructure)
    {
        int id = _previewStructure.ID;

        foreach (EditorStructure editorStructure in MenuSetupManager.Instance.EditorStructures)
        {
            if (editorStructure == null) continue;

            foreach (CircuitIdentifier circuitIdentifier in editorStructure.Circuits)
            {
                if (circuitIdentifier.previewStructureID == id) return false;
            }

            foreach (int customCircuitID in editorStructure.BookmarkIDs)
            {
                if (customCircuitID == _previewStructure.ID) return false;
            }
        }

        foreach (PreviewStructure previewStructure in MenuSetupManager.Instance.PreviewStructures)
        {
            if (previewStructure.ID == id && previewStructure != _previewStructure) return false;
        }

        return true;
    }

    // Singleton state reference
    public static MenuLogicManager Instance { get { return instance; } }

    // Getter methods
    public bool FirstOpen { get { return firstOpen; } }

    public int CurrentSceneIndex { get { return currentSceneIndex; } }

    public PreviewStructure CurrentPreviewStructure { get { return currentPreviewStructure; } }
}