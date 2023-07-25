using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLogicManager : MonoBehaviour
{
    private static MenuLogicManager instance;

    private bool firstOpen;

    private int currentSceneIndex;

    private PreviewStructure currentPreviewStructure;

    private List<PreviewStructure> invalidCircuits, validCircuits;

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

    public static List<string> CanDeleteCustomCircuit(PreviewStructure _previewStructure)
    {
        int id = _previewStructure.ID;
        List<string> errorMessages = new List<string>();
        instance.invalidCircuits = new List<PreviewStructure>(); instance.validCircuits = new List<PreviewStructure>();

        foreach (PreviewStructure previewStructure in MenuSetupManager.Instance.PreviewStructures) instance.TraversalTest(previewStructure, _previewStructure);

        foreach (EditorStructure editorStructure in MenuSetupManager.Instance.EditorStructures)
        {
            if (editorStructure == null) continue;

            foreach (CircuitIdentifier circuitIdentifier in editorStructure.Circuits)
            {
                if (circuitIdentifier.previewStructureID == -1) continue;

                PreviewStructure previewStructure = MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(circuitIdentifier.previewStructureID)];

                if (instance.invalidCircuits.Contains(previewStructure)) { errorMessages.Add("- Circuit is directly and/or indirectly referenced by a placed circuit in an editor scene"); break; }
            }

            foreach (int customCircuitID in editorStructure.BookmarkIDs)
            {
                if (customCircuitID == -1) continue;

                PreviewStructure previewStructure = MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(customCircuitID)];

                if (instance.invalidCircuits.Contains(previewStructure)) { errorMessages.Add("- Circuit is directly and/or indirectly referenced by a bookmark in an editor scene"); break; }
            }
        }

        foreach (PreviewStructure previewStructure in MenuSetupManager.Instance.PreviewStructures)
        {
            if (previewStructure == _previewStructure) continue;

            if (instance.invalidCircuits.Contains(previewStructure)) { errorMessages.Add("- Circuit is directly and/or indirectly referenced by another custom circuit"); break; }
        }

        return errorMessages;
    }

    private void TraversalTest(PreviewStructure current, PreviewStructure target)
    {
        if (current == target) { if (!invalidCircuits.Contains(current)) { invalidCircuits.Add(current); } return; }

        else if (invalidCircuits.Contains(current) || validCircuits.Contains(current)) return;

        foreach (CircuitIdentifier circuitIdentifier in current.Circuits)
        {
            if (circuitIdentifier.previewStructureID != -1)
            {
                PreviewStructure previewStructure = MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(circuitIdentifier.previewStructureID)];

                TraversalTest(previewStructure, target);

                if (invalidCircuits.Contains(previewStructure)) { invalidCircuits.Add(current); return; }
            }
        }

        validCircuits.Add(current);
    }

    // Singleton state reference
    public static MenuLogicManager Instance { get { return instance; } }

    // Getter methods
    public bool FirstOpen { get { return firstOpen; } }

    public int CurrentSceneIndex { get { return currentSceneIndex; } }

    public PreviewStructure CurrentPreviewStructure { get { return currentPreviewStructure; } }
}