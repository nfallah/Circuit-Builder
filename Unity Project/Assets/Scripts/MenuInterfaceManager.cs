using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuInterfaceManager : MonoBehaviour
{
    private static MenuInterfaceManager instance;

    [SerializeField] Color defaultColor, saveColor;

    [SerializeField] KeyCode cancelKey;

    [SerializeField] GameObject sceneDeletionInterface, sceneNameInterface, transparentBackground;

    [SerializeField] TMP_InputField sceneNameInputField;

    [SerializeField] TextMeshProUGUI save1, save2, save3, sceneNameError;

    private GameObject currentInterface;

    private int currentSceneIndex = -1;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("MenuInterfaceManager instance already established; terminating.");
        }

        instance = this;
    }

    private void Start()
    {
        UpdateInterface();
        enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1)) CancelCurrentSubmission();
    }

    // Called by pressing one of the scene buttons
    public void OpenScene(int sceneIndex)
    {
        MenuLogicManager.Instance.OpenScene(sceneIndex);
    }

    public void UpdateInterface()
    {
        EditorStructure[] editorStructures = MenuSetupManager.Instance.EditorStructures;

        if (editorStructures[0] != null) { save1.text = editorStructures[0].Name; save1.color = saveColor; }

        if (editorStructures[1] != null) { save2.text = editorStructures[1].Name; save2.color = saveColor; }

        if (editorStructures[2] != null) { save3.text = editorStructures[2].Name; save3.color = saveColor; }
    }

    public void BeginSceneDeletion(int sceneIndex)
    {
        if (MenuSetupManager.Instance.EditorStructures[sceneIndex] == null) return;

        currentSceneIndex = sceneIndex;
        BeginInterface(sceneDeletionInterface);
    }

    public void BeginSceneNameSubmission(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;
        BeginInterface(sceneNameInterface);
    }

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

    public void SceneNameSubmission()
    {
        string submission = sceneNameInputField.text.ToLower();

        if (submission == string.Empty) sceneNameError.text = "scene name must be non-empty";

        else if (CurrentSceneNames.Contains(submission)) sceneNameError.text = "scene name must be unique";

        else
        {
            MenuLogicManager.Instance.CreateScene(currentSceneIndex, submission);
        }
    }

    public void CancelCurrentSubmission()
    {
        if (currentInterface == sceneNameInterface) sceneNameInputField.text = sceneNameError.text = "";

        ToggleCurrentInterface(false);
        currentSceneIndex = -1;
        currentInterface = null;
        enabled = false;
    }

    private void BeginInterface(GameObject newInterface)
    {
        if (currentInterface != null) return;

        currentInterface = newInterface;
        ToggleCurrentInterface(true);
        enabled = true;
    }

    // Helper method
    private void ToggleCurrentInterface(bool visible)
    {
        currentInterface.SetActive(visible); transparentBackground.SetActive(visible);
    }

    // Getter methods
    public static MenuInterfaceManager Instance { get { return instance; } }

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