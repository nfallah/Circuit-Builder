using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// MenuInterfaceManager handles all UI interactions and transitions within the menu scene.
/// </summary>
public class MenuInterfaceManager : MonoBehaviour
{
    // Singleton state reference
    private static MenuInterfaceManager instance;

    /// <summary>
    /// The colors of save slots when uncreated and created. respectively.
    /// </summary>
    [SerializeField]
    Color defaultColor,
        saveColor;

    /// <summary>
    /// How much the height of <seealso cref="deleteErrorTransform"/> should increase for each additional error message.
    /// </summary>
    [SerializeField] float deleteErrorMessageSize;

    /// <summary>
    /// Attempts to exit the <seealso cref="currentInterface"/> in use.<br/><br/>
    /// For some interfaces, this will not instantly occur (such as going back to the options menu from the guide).
    /// </summary>
    [SerializeField] KeyCode cancelKey;

    /// <summary>
    /// Utilized to add and reference custom circuits from the directory. 
    /// </summary>
    [SerializeField]
    GameObject customCircuitPanel,
        customCircuitPrefab;

    /// <summary>
    /// Set to visible when an interface is opened.<br/><br/>
    /// Within the scene, this should be a semi-transparent background overlayed onto everything except the currently opened interface.
    /// </summary>
    [SerializeField]
    GameObject transparentBackground,
        transparentBackgroundUpper; // Utilized for multi-level interfaces such as the options interface.

    [Space(10)]
    /// <summary>
    /// List of all interfaces within the scene.
    /// </summary>
    [SerializeField]
    GameObject customCircuitsInterface, // Displays all currently open custom circuits and allows the user to view and/or delete them; opened from the options menu.
        deleteErrorInterface, // The error log(s) displayed when a custom circuit cannot be deleted for any reason(s).
        guideInterface, // Displays the guide prefab; opened from the options menu.
        optionSelectionInterface, // Allows the user to activate customCircuitsInterface as well as guideInterface.
        optionsInterface, // The parent of optionSelectionInterface indicating that the options menu is open.
        sceneDeletionInterface, // Prompts the user to confirm deleting an editor structure (save slot).
        sceneNameInterface; // Prompts the user to compose a name for an editor structure (save slot).

    [Space(10)]
    /// <summary>
    /// Utilized to expand the vertical height of the error interface by factors of <seealso cref="deleteErrorMessageSize"/>.
    /// </summary>
    [SerializeField]
    RectTransform deleteErrorTransform;

    /// <summary>
    /// The text components for the delete and scene creation interfaces utilized to display the cause(s) of error.
    /// </summary>
    [SerializeField]
    TextMeshProUGUI deleteErrorText,
    sceneNameError;

    /// <summary>
    /// The input field within <seealso cref="sceneNameInterface"/> that the user uses to compose a scene name.
    /// </summary>
    [SerializeField]
    TMP_InputField sceneNameInputField;

    /// <summary>
    /// Text components of all 3 save slots within the menu used for setting and deleting their names.
    /// </summary>
    [SerializeField]
    TextMeshProUGUI save1,
        save2,
        save3;

    /// <summary>
    /// The current UI interface in use.
    /// </summary>
    private GameObject currentInterface;

    /// <summary>
    /// Index of the current <see cref="EditorStructure"/> loaded into the scene.
    /// </summary>
    private int currentSceneIndex = -1;

    // Enforces a singleton state pattern
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("MenuInterfaceManager instance already established; terminating.");
        }

        instance = this;
    }

    // Loads in all serialized editor scenes and preview structures.
    private void Start()
    {
        UpdateInterface();
        AddCustomBookmarks();
        CursorManager.SetMouseTexture(true);
        enabled = false;
    }

    private void Update()
    {
        // Default exit controls for all interfaces except the options interface.
        if (currentInterface != optionsInterface)
        {
            if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1)) CancelCurrentSubmission();
        }

        // Otherwise, the options interface is opened and should utilize a different transition scheme.
        else if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
        {
            // If at the root, exit the interface.
            if (optionSelectionInterface.activeSelf) CancelCurrentSubmission();

            // If within the custom circuits interface whilst the delete error interface is not open, return to the root.
            else if (customCircuitsInterface.activeSelf && !deleteErrorInterface.activeSelf)
            {
                customCircuitsInterface.SetActive(false);
                optionSelectionInterface.SetActive(true);
            }

            // If within the guide interface, return to the root.
            else if (guideInterface.activeSelf)
            {
                guideInterface.SetActive(false);
                optionSelectionInterface.SetActive(true);
            }

            // If within the guide delete error interface, re-adjust background layers to make the custom circuit interface accessible.
            else if (deleteErrorInterface.activeSelf)
            {
                transparentBackgroundUpper.SetActive(false);
                transparentBackground.SetActive(true);
                deleteErrorInterface.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Opens an editor scene; called by pressing one of the valid save buttons.
    /// </summary>
    /// <param name="sceneIndex"></param>
    public void OpenScene(int sceneIndex) { MenuLogicManager.Instance.OpenScene(sceneIndex); }

    /// <summary>
    /// Instantiates a <seealso cref="customCircuitPrefab"/> button for each preview structure.
    /// </summary>
    private void AddCustomBookmarks()
    {
        List<PreviewStructure> previewStructures = MenuSetupManager.Instance.PreviewStructures;

        foreach (PreviewStructure previewStructure in previewStructures)
        {
            GameObject current = Instantiate(customCircuitPrefab, customCircuitPanel.transform);

            current.GetComponentInChildren<TextMeshProUGUI>().text = previewStructure.Name;

            // Adds the relevant listeners to ensure the custom circuit can be deleted and viewed by their respective buttons.
            current.GetComponent<CustomCircuitButtons>().DeleteButton.onClick.AddListener(delegate { DeletePreview(current, previewStructure); });
            current.GetComponent<CustomCircuitButtons>().ViewButton.onClick.AddListener(delegate { PreviewScene(previewStructure); });
        }
    }

    /// <summary>
    /// Deletes a preview structure from the game and directory given it is not in use.
    /// </summary>
    /// <param name="button">The button assigned this method by a delegate.</param>
    /// <param name="previewStructure">The preview structure to attempt deletion on.</param>
    public void DeletePreview(GameObject button, PreviewStructure previewStructure)
    {
        // Obtains a list of all error messages in an attempt to delete the preview structure
        List<string> errorMessages = MenuLogicManager.CanDeleteCustomCircuit(previewStructure);

        // If there are no errors, proceed with deletion.
        if (errorMessages.Count == 0)
        {
            MenuSetupManager.Instance.DeletePreviewStructure(previewStructure);
            Destroy(button);
        }

        // Otherwise, open the delete error interface and display errors.
        else
        {
            transparentBackgroundUpper.SetActive(true);
            transparentBackground.SetActive(false);
            deleteErrorInterface.SetActive(true);
            deleteErrorTransform.sizeDelta = new Vector2(deleteErrorTransform.sizeDelta.x, deleteErrorMessageSize * errorMessages.Count);
            deleteErrorText.text = "";

            int index = 0;

            // Adds each error
            foreach (string errorMessage in errorMessages)
            {
                index++;
                deleteErrorText.text += "- " + errorMessage + (index != errorMessages.Count ? "\n\n" : "");
            }
        }
    }

    /// <summary>
    /// Referenced when the user acknowledges and closes the delete error interface; called by pressing an in-scene button.
    /// </summary>
    public void OnCircuitDeleteErrorConfirm()
    {
        transparentBackgroundUpper.SetActive(false);
        transparentBackground.SetActive(true);
        deleteErrorInterface.SetActive(false);
    }

    /// <summary>
    /// Opens a preview structure whose internal components the user wishes to inspect.<br/><br/>
    /// This method is assigned as a listener to the delete button within each instantiated <seealso cref="customCircuitPrefab"/>.
    /// </summary>
    /// <param name="previewStructure">The preview structure to open.</param>
    public void PreviewScene(PreviewStructure previewStructure) { MenuLogicManager.Instance.OpenPreview(previewStructure); }

    /// <summary>
    /// Sets the names of all save slots corresponding to serialized editor structures.
    /// </summary>
    public void UpdateInterface()
    {
        EditorStructure[] editorStructures = MenuSetupManager.Instance.EditorStructures;

        if (editorStructures[0] != null) { save1.text = editorStructures[0].Name; save1.color = saveColor; }

        if (editorStructures[1] != null) { save2.text = editorStructures[1].Name; save2.color = saveColor; }

        if (editorStructures[2] != null) { save3.text = editorStructures[2].Name; save3.color = saveColor; }
    }

    /// <summary>
    /// Opens <seealso cref="sceneDeletionInterface"/> prompting a user to acknowledge their action to delete a editor structure (save slot).
    /// </summary>
    /// <param name="sceneIndex">The index of the prospective editor structure to delete</param>
    public void BeginSceneDeletion(int sceneIndex)
    {
        if (MenuSetupManager.Instance.EditorStructures[sceneIndex] == null) return;

        currentSceneIndex = sceneIndex;
        BeginInterface(sceneDeletionInterface);
    }

    /// <summary>
    /// Opens <seealso cref="sceneNameInterface"/> prompting a user to compose a name to create an editor structure.
    /// </summary>
    /// <param name="sceneIndex">The index of the prospective editor structure to create.</param>
    public void BeginSceneNameSubmission(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;
        BeginInterface(sceneNameInterface);
    }

    /// <summary>
    /// Opens the options interface; called by pressing an in-scene button.
    /// </summary>
    public void OpenOptionsInterface()
    {
        BeginInterface(optionsInterface);
        optionSelectionInterface.SetActive(true); customCircuitsInterface.SetActive(false); guideInterface.SetActive(false);
    }

    /// <summary>
    /// Opens the guide interface; called by pressing an in-scene button.
    /// </summary>
    public void OpenGuide()
    {
        optionSelectionInterface.SetActive(false);
        guideInterface.SetActive(true);
    }

    /// <summary>
    /// Opens the custom circuit interface; called by pressing an in-scene button.
    /// </summary>
    public void OpenCustomCircuits()
    {
        optionSelectionInterface.SetActive(false);
        customCircuitsInterface.SetActive(true);
    }

    /// <summary>
    /// Restores the text of a save slot to its default values after a success editor scene deletion.
    /// </summary>
    public void SceneDeleteSubmission()
    {
        MenuSetupManager.Instance.DeleteEditorStructure(currentSceneIndex);
        TextMeshProUGUI currentText;

        if (currentSceneIndex == 0)
        {
            currentText = save1;
        }

        else if (currentSceneIndex == 1)
        {
            currentText = save2;
        }

        else
        {
            currentText = save3;
        }

        CancelCurrentSubmission();
        currentText.text = "new save";
        currentText.color = defaultColor;

    }

    /// <summary>
    /// Checks the name in <seealso cref="sceneNameInputField"/> and creates an editor scene if it is valid.
    /// </summary>
    public void SceneNameSubmission()
    {
        string submission = sceneNameInputField.text.ToLower().Trim();

        // Cannot be empty
        if (submission == string.Empty) sceneNameError.text = "scene name must be non-empty";

        // Must be unique
        else if (CurrentSceneNames.Contains(submission)) sceneNameError.text = "scene name must be unique";

        // Creates and opens the editor structure.
        else MenuLogicManager.Instance.CreateScene(currentSceneIndex, submission);
    }

    /// <summary>
    /// Closes the current interface in use.
    /// </summary>
    public void CancelCurrentSubmission()
    {
        if (currentInterface == sceneNameInterface) sceneNameInputField.text = sceneNameError.text = "";

        BackgroundParallax.Instance.enabled = true;
        ToggleCurrentInterface(false);
        currentSceneIndex = -1;
        currentInterface = null;
        enabled = false;
    }

    // Exits the game; called by pressing an in-scene button.
    public void Quit() { Application.Quit(); }

    /// <summary>
    /// Opens an interface.
    /// </summary>
    /// <param name="newInterface">The interface to open.</param>
    private void BeginInterface(GameObject newInterface)
    {
        if (currentInterface != null) return;

        BackgroundParallax.Instance.enabled = false;
        currentInterface = newInterface;
        ToggleCurrentInterface(true);
        enabled = true;
    }

    /// <summary>
    /// Sets the visibility of the current interface.
    /// </summary>
    /// <param name="visible">Whether the current interface should be visible.</param>
    private void ToggleCurrentInterface(bool visible)
    {
        currentInterface.SetActive(visible); transparentBackground.SetActive(visible);
    }

    // Getter methods
    public static MenuInterfaceManager Instance { get { return instance; } }

    /// <summary>
    /// Returns the named list of all editor structures.
    /// </summary>
    private List<string> CurrentSceneNames
    {
        get
        {
            EditorStructure[] editorStructures = MenuSetupManager.Instance.EditorStructures;
            List<string> currentSceneNames = new List<string>();

            foreach (EditorStructure editorStructure in editorStructures) if (editorStructure != null) currentSceneNames.Add(editorStructure.Name);

            return currentSceneNames;
        }
    }
}