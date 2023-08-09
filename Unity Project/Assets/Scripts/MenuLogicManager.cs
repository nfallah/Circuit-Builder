using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MenuLogicManager handles all scene transitions as well as validation tests for deleting custom circuits.
/// </summary>
public class MenuLogicManager : MonoBehaviour
{
    // Singleton state reference
    private static MenuLogicManager instance;

    /// <summary>
    /// Whether the current editor scene has just been created or not.<br/><br/>
    /// If it is the first time, this value indicates that default values must be initialized in the editor scene.
    /// </summary>
    private bool firstOpen;

    /// <summary>
    /// The index of the current editor scene to open.
    /// </summary>
    private int currentSceneIndex;

    /// <summary>
    /// The current preview structure in the preview scene.
    /// </summary>
    private PreviewStructure currentPreviewStructure;

    /// <summary>
    /// Utilized within <seealso cref="TraversalTest(PreviewStructure, PreviewStructure)"/> to sort all preview structures into these respective lists.<br/>
    /// If a circuit is invalid, it means that it is being utilized somewhere and cannot be deleted. A valid circuit can be deleted.
    /// </summary>
    private List<PreviewStructure> invalidCircuits, validCircuits;

    // Enforces a singleton state pattern
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("MenuLogicManager instance already established; terminating.");
        }

        instance = this;
    }

    /// <summary>
    /// Attempts to open an editor scene by index.
    /// </summary>
    /// <param name="sceneIndex">Index of the editor structure to open.</param>
    public void OpenScene(int sceneIndex)
    {
        bool isCreated = MenuSetupManager.Instance.EditorStructures[sceneIndex] != null;

        firstOpen = !isCreated;

        // Opens the scene only if it has not been just created; otherwise completes the setup process.
        if (isCreated) ImportScene(sceneIndex); else MenuInterfaceManager.Instance.BeginSceneNameSubmission(sceneIndex);
    }

    /// <summary>
    /// Creates an editor scene.
    /// </summary>
    /// <param name="sceneIndex">Index of the editor structure to create.</param>
    /// <param name="name">Name of the editor structure to create.</param>
    public void CreateScene(int sceneIndex, string name)
    {
        MenuSetupManager.Instance.EditorStructures[sceneIndex] = new EditorStructure(name);
        ImportScene(sceneIndex);
    }

    /// <summary>
    /// Opens a preview structure.
    /// </summary>
    /// <param name="previewStructure">The preview structure to open.</param>
    public void OpenPreview(PreviewStructure previewStructure)
    {
        currentPreviewStructure = previewStructure;
        SceneManager.LoadScene(2);
    }

    /// <summary>
    /// Opens a scene that has already been created beforehand.
    /// </summary>
    /// <param name="sceneIndex"></param>
    private void ImportScene(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;
        SceneManager.LoadScene(1);
    }
    
    /// <summary>
    /// Runs several validation tests that determine whether a custom circuit can be deleted.<br/><br/>.
    /// If there are no error messages returned, then the custom circuit can be deleted.
    /// </summary>
    /// <param name="_previewStructure">The prospect custom circuit/preview structure to delete.</param>
    /// <returns></returns>
    public static List<string> CanDeleteCustomCircuit(PreviewStructure _previewStructure)
    {
        List<string> errorMessages = new List<string>();
        instance.invalidCircuits = new List<PreviewStructure>(); instance.validCircuits = new List<PreviewStructure>();

        // Marks each preview structure as either valid or invalid
        foreach (PreviewStructure previewStructure in MenuSetupManager.Instance.PreviewStructures) instance.TraversalTest(previewStructure, _previewStructure);

        // Tests #1/#2: not placed or part of a bookmark directly (top-most custom circuit) or indirectly (inside of another custom circuit) within an editor scene.
        foreach (EditorStructure editorStructure in MenuSetupManager.Instance.EditorStructures)
        {
            if (editorStructure == null) continue;

            // Placed circuit test
            foreach (CircuitIdentifier circuitIdentifier in editorStructure.Circuits)
            {
                if (circuitIdentifier.previewStructureID == -1) continue;

                PreviewStructure previewStructure = MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(circuitIdentifier.previewStructureID)];

                if (instance.invalidCircuits.Contains(previewStructure)) { errorMessages.Add("Circuit is directly and/or indirectly referenced by a placed circuit in an editor scene"); break; }
            }

            // Bookmark test
            foreach (int customCircuitID in editorStructure.BookmarkIDs)
            {
                if (customCircuitID == -1) continue;

                PreviewStructure previewStructure = MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(customCircuitID)];

                if (instance.invalidCircuits.Contains(previewStructure)) { errorMessages.Add("Circuit is directly and/or indirectly referenced by a bookmark in an editor scene"); break; }
            }
        }

        // Test #3: not directly (top-most custom circuit) or indirectly (inside of another custom circuit) within a custom circuit.
        foreach (PreviewStructure previewStructure in MenuSetupManager.Instance.PreviewStructures)
        {
            if (previewStructure == _previewStructure) continue;

            if (instance.invalidCircuits.Contains(previewStructure)) { errorMessages.Add("Circuit is directly and/or indirectly referenced by another custom circuit"); break; }
        }

        // Returns the list of all obtained error messages, if any.
        return errorMessages;
    }

    /// <summary>
    /// Sorts the current preview structure as invalid (contains the target circuit) or valid by running a modified depth-first search.<br/><br/>
    /// Essentially, the user inputs a starting preview structure. Until a starting circuit (e.g. AND) is reached or there is nothing else to explore, this method:<br/>
    /// - Recursively calls this method on all of its circuit components that are custom circuits granted they have not already been sorted into valid or invalid circuits.
    /// - Adds the current circuit as an invalid preview structure if any of its children contained the target custom circuit, and returns.<br/>
    /// - Adds the current circuit as a valid preview structure if none of its child custom circuits contained the target custom circuit.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="target"></param>
    private void TraversalTest(PreviewStructure current, PreviewStructure target)
    {
        // If the target preview structure was recursively called by TraversalTest, then add it.
        if (current == target) { if (!invalidCircuits.Contains(current)) { invalidCircuits.Add(current); } return; }

        // If already designated as an invalid/valid circuit, return.
        else if (invalidCircuits.Contains(current) || validCircuits.Contains(current)) return;

        // Traverse through each custom circuit and recursively call this method.
        // Once done, check to see if the target preview structure was detected. If so, then also add this preview structure.
        foreach (CircuitIdentifier circuitIdentifier in current.Circuits)
        {
            if (circuitIdentifier.previewStructureID != -1)
            {
                PreviewStructure previewStructure = MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(circuitIdentifier.previewStructureID)];

                TraversalTest(previewStructure, target);

                if (invalidCircuits.Contains(previewStructure)) { invalidCircuits.Add(current); return; }
            }
        }

        // If not invalid, then must be valid.
        validCircuits.Add(current);
    }

    // Getter methods
    public static MenuLogicManager Instance { get { return instance; } }

    public bool FirstOpen { get { return firstOpen; } }

    public int CurrentSceneIndex { get { return currentSceneIndex; } }

    public PreviewStructure CurrentPreviewStructure { get { return currentPreviewStructure; } }
}