using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuInterfaceManager : MonoBehaviour
{
    private static MenuInterfaceManager instance;

    [SerializeField] Color defaultColor, saveColor;

    [SerializeField] KeyCode cancelKey;

    [SerializeField] GameObject customCircuitPanel, customCircuitPrefab, optionSelectionInterface, guideInterface, customCircuitsInterface, optionsInterface, sceneDeletionInterface, sceneNameInterface, transparentBackground;

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
        AddCustomBookmarks();
        CursorManager.SetMouseTexture(true);
        enabled = false;
    }

    private void Update()
    {
        if (currentInterface != optionsInterface)
        {
            if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1)) CancelCurrentSubmission();
        }

        else
        {
            if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
            {
                if (optionSelectionInterface.activeSelf)
                {
                    CancelCurrentSubmission();
                }

                else if (customCircuitsInterface.activeSelf)
                {
                    customCircuitsInterface.SetActive(false);
                    optionSelectionInterface.SetActive(true);
                }

                else
                {
                    guideInterface.SetActive(false);
                    optionSelectionInterface.SetActive(true);
                }
            }
        }
    }

    private void AddCustomBookmarks()
    {
        List<PreviewStructure> previewStructures = MenuSetupManager.Instance.PreviewStructures;

        foreach (PreviewStructure previewStructure in previewStructures)
        {
            GameObject current = Instantiate(customCircuitPrefab, customCircuitPanel.transform);

            current.GetComponentInChildren<TextMeshProUGUI>().text = previewStructure.Name;
            current.GetComponent<CustomCircuitButtons>().DeleteButton.onClick.AddListener(delegate { DeletePreview(previewStructure); });
            current.GetComponent<CustomCircuitButtons>().ViewButton.onClick.AddListener(delegate { PreviewScene(previewStructure); });
        }
    }

    // Called by pressing one of the scene buttons
    public void OpenScene(int sceneIndex)
    {
        MenuLogicManager.Instance.OpenScene(sceneIndex);
    }

    public void DeletePreview(PreviewStructure previewStructure)
    {
        Debug.Log(previewStructure.Name + ": DELETION ATTEMPT!");
    }

    public void PreviewScene(PreviewStructure previewStructure)
    {
        MenuLogicManager.Instance.OpenPreview(previewStructure);
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

    public void OpenOptionsInterface()
    {
        BeginInterface(optionsInterface);
        optionSelectionInterface.SetActive(true); customCircuitsInterface.SetActive(false); guideInterface.SetActive(false);
    }

    public void OpenGuide()
    {
        optionSelectionInterface.SetActive(false);
        guideInterface.SetActive(true);
    }

    public void OpenCustomCircuits()
    {
        optionSelectionInterface.SetActive(false);
        customCircuitsInterface.SetActive(true);
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
        string submission = sceneNameInputField.text.ToLower().Trim();

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

        BackgroundParallax.Instance.enabled = true;
        ToggleCurrentInterface(false);
        currentSceneIndex = -1;
        currentInterface = null;
        enabled = false;
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void BeginInterface(GameObject newInterface)
    {
        if (currentInterface != null) return;

        BackgroundParallax.Instance.enabled = false;
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