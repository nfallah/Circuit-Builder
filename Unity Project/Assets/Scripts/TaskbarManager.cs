using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TaskbarManager : MonoBehaviour
{
    private static TaskbarManager instance;

    [SerializeField] TextMeshProUGUI notifierText, circuitErrorText, labelText;

    [SerializeField] TMP_InputField circuitNameField;

    [SerializeField] Color startingCircuitColor, customCircuitColor;

    [SerializeField] float bookmarkScrollThickness;

    [SerializeField] GameObject saveWarning, labelMenu, nullState, circuitSaveErrorMenu, notifierPanel, sceneSaveMenu, bookmarkScrollbar, background, addMenu, bookmarksMenu, bookmarksScroll, bookmarksPanel;

    [SerializeField] GameObject bookmarkRef, customBookmarkRef;

    [SerializeField] KeyCode cancelKey;

    [SerializeField] RectTransform addCustomPanel, addStartingPanel, bookmarksBorder;

    [SerializeField] Toggle circuitToggle;

    [SerializeField] Vector2 bookmarkSize, bookmarkMaskSize;

    private bool currentlyRestoring, reopenBookmarks = true, bookmarksDown;

    private GameObject currentMenu;

    private List<Type> bookmarks = new List<Type>();

    private List<int> bookmarkIDs = new List<int>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("TaskbarManager instance already established; terminating.");
        }

        instance = this;
        enabled = false;
    }

    private void Update()
    {
        if (currentMenu == nullState) return;

        // Depending on the current opened menu, the escape controls may alter and thus they are differentiated
        if (currentMenu == bookmarksMenu)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) { bookmarksDown = true; }

            else if (Input.GetMouseButtonUp(0) && bookmarksDown)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    bookmarksDown = false;
                }

                else CloseMenu();
            }

            else if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject()) UpdateBookmarkPosition();

            else if (Input.GetKeyDown(cancelKey)) CloseMenu();
        }

        else if (currentMenu == addMenu || currentMenu == sceneSaveMenu || currentMenu == circuitSaveErrorMenu || currentMenu == saveWarning)
        {
            if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
            {
                if (currentMenu == circuitSaveErrorMenu) ConfirmError(); else CloseMenu();
            }
        }
    }

    // Essentially disables the taskbar from functioning; a locked state that must be manually closed via script.
    public void NullState()
    {
        OpenMenu(false, nullState);
    }

    public void UpdateSaveToggle()
    {
        bool isOn = circuitToggle.isOn;

        circuitNameField.interactable = isOn;

        if (!isOn)
        {
            circuitNameField.text = "";
        }
    }

    public void OpenOptions()
    {
        if (EditorStructureManager.Instance.DisplaySavePrompt)
        {
            OpenMenu(true, saveWarning);
        }

        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void OpenMenuScene() { SceneManager.LoadScene(0); }

    public void OpenLabelMenu(bool isInput)
    {
        OpenMenu(true, labelMenu);
        labelText.text = "compose a label for the selected " + (isInput ? "input" : "output");
    }

    public void OpenSave()
    {
        OpenMenu(true, sceneSaveMenu);
    }

    public void CircuitSaveError(string errorMessage)
    {
        CloseMenu();
        circuitErrorText.text = errorMessage;
        OpenMenu(true, circuitSaveErrorMenu);
    }

    public void ConfirmError()
    {
        CloseMenu();
        OpenSave();
    }

    public void SaveConfirm()
    {
        CloseMenu();

        if (circuitToggle.isOn)
        {
            notifierText.text = "verifying...";
            OpenMenu(true, notifierPanel);
            PreviewStructureManager.Instance.VerifyPreviewStructure(circuitNameField.text.ToLower().Trim());
        }

        else
        {
            notifierText.text = "saving scene...";
            OpenMenu(true, notifierPanel);
            EditorStructureManager.Instance.Serialize();
        }
    }

    public void OpenAdd()
    {
        OpenMenu(true, addMenu);
    }

    public void OpenBookmarks()
    {
        OpenBookmarks(false);
    }

    public void OnSuccessfulPreviewStructure()
    {
        circuitNameField.text = "";
        CloseMenu();
    }

    public void OnSuccessfulPreviewVerification()
    {
        CloseMenu();
        OpenMenu(true, notifierPanel);
        notifierText.text = "creating...";
    }

    public void OpenBookmarks(bool showBackground)
    {
        if (bookmarks.Count == 0) return;

        bookmarksDown = false;
        OpenMenu(showBackground, bookmarksMenu);
    }

    public void RestoreBookmarks(List<int> circuitIndeces, List<int> circuitIDs)
    {
        int index = 0;

        currentlyRestoring = true;
        
        foreach (int circuitIndex in new List<int>(circuitIndeces))
        {
            if (circuitIndex != -1)
            {
                Toggle toggle = addStartingPanel.GetChild(circuitIndex).GetComponentInChildren<Toggle>();

                toggle.isOn = true;
                UpdateBookmarkAll(toggle.gameObject);
            }

            else
            {
                AddCustomCircuitPanel(circuitIDs[index], true);
            }

            index++;
        }

        currentlyRestoring = false;
    }

    public void RestoreCustomCircuits()
    {
        foreach (PreviewStructure previewStructure in MenuSetupManager.Instance.PreviewStructures)
        {
            if (bookmarkIDs.Contains(previewStructure.ID)) continue;

            AddCustomCircuitPanel(previewStructure.ID, false);
        }
    }

    public void AddCustomCircuitPanel(int circuitID, bool bookmarked)
    {
        GameObject current = Instantiate(customBookmarkRef, addCustomPanel.transform);
        Toggle toggle = current.GetComponentInChildren<Toggle>();
        PreviewStructure.PreviewStructureReference reference = current.AddComponent<PreviewStructure.PreviewStructureReference>();
        current.GetComponentInChildren<TextMeshProUGUI>().text = MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(circuitID)].Name;
        reference.ID = circuitID;
        current.GetComponentInChildren<Button>().onClick.AddListener(delegate { AddBookmarkCircuit(-1, reference.ID); });
        toggle.onValueChanged.AddListener(delegate { UpdateBookmark(reference); });

        if (bookmarked)
        {
            toggle.isOn = true;
            UpdateBookmarkCustom(reference);
        }
    }

    // This method is specifically for the toggles due to them being called whenever their value is changed, including within code
    public void UpdateBookmark(GameObject obj)
    {
        if (currentlyRestoring) return;
        UpdateBookmarkAll(obj);
    }

    public void UpdateBookmark(PreviewStructure.PreviewStructureReference previewStructureReference)
    {
        if (currentlyRestoring) return;
        UpdateBookmarkCustom(previewStructureReference);
    }

    public void UpdateBookmarkAll(GameObject obj)
    {
        bool newStatus = obj.GetComponent<Toggle>().isOn;
        Type type = CircuitType(obj.transform.parent.GetSiblingIndex());

        if (newStatus && !bookmarks.Contains(type))
        {
            if (!currentlyRestoring) EditorStructureManager.Instance.DisplaySavePrompt = true;

            EditorStructureManager.Instance.Bookmarks.Add(StartingCircuitIndex(type));
            bookmarks.Add(type);
            bookmarkIDs.Add(-1);
            GameObject bookmark = Instantiate(bookmarkRef, bookmarksPanel.transform);
            Button button = bookmark.GetComponentInChildren<Button>();
            TextMeshProUGUI text = bookmark.GetComponentInChildren<TextMeshProUGUI>();
            bookmark.name = text.text = obj.transform.parent.name;
            text.color = startingCircuitColor;

            button.onClick.AddListener(delegate { AddBookmarkCircuit(StartingCircuitIndex(type), -1); });
        }

        else if (!newStatus && bookmarks.Contains(type))
        {
            if (!currentlyRestoring) EditorStructureManager.Instance.DisplaySavePrompt = true;

            int index = bookmarks.IndexOf(type);
            EditorStructureManager.Instance.Bookmarks.Remove(StartingCircuitIndex(type));
            bookmarks.Remove(type);
            bookmarkIDs.RemoveAt(index);
            Destroy(bookmarksPanel.transform.GetChild(index).gameObject);
        }
    }

    public void UpdateBookmarkCustom(PreviewStructure.PreviewStructureReference reference)
    {
        bool newStatus = reference.GetComponentInChildren<Toggle>().isOn;
        int id = reference.GetComponentInChildren<PreviewStructure.PreviewStructureReference>().ID;

        if (newStatus && !bookmarkIDs.Contains(id))
        {
            if (!currentlyRestoring) EditorStructureManager.Instance.DisplaySavePrompt = true;

            EditorStructureManager.Instance.Bookmarks.Add(-1);
            bookmarks.Add(typeof(CustomCircuit));
            bookmarkIDs.Add(id);
            GameObject bookmark = Instantiate(bookmarkRef, bookmarksPanel.transform);
            Button button = bookmark.GetComponentInChildren<Button>();
            TextMeshProUGUI text = bookmark.GetComponentInChildren<TextMeshProUGUI>();
            bookmark.name = text.text = MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(id)].Name;
            text.color = customCircuitColor;

            button.onClick.AddListener(delegate { AddBookmarkCircuit(-1, id); });
        }

        else if (!newStatus && bookmarkIDs.Contains(id))
        {
            if (!currentlyRestoring) EditorStructureManager.Instance.DisplaySavePrompt = true;

            int index = bookmarkIDs.IndexOf(id);
            EditorStructureManager.Instance.Bookmarks.RemoveAt(index);
            bookmarks.RemoveAt(index);
            bookmarkIDs.Remove(id);
            Destroy(bookmarksPanel.transform.GetChild(index).gameObject);
        }
    }

    private void AddBookmarkCircuit(int circuitType, int circuitID)
    {
        switch (circuitType)
        {
            case -1:
                AddCircuit(new CustomCircuit(MenuSetupManager.Instance.PreviewStructures[MenuSetupManager.Instance.PreviewStructureIDs.IndexOf(circuitID)]));
                return;
            case 0:
                AddCircuit(new InputGate());
                return;
            case 1:
                AddCircuit(new Display());
                return;
            case 2:
                AddCircuit(new Buffer());
                return;
            case 3:
                AddCircuit(new AndGate());
                return;
            case 4:
                AddCircuit(new NAndGate());
                return;
            case 5:
                AddCircuit(new NOrGate());
                return;
            case 6:
                AddCircuit(new NotGate());
                return;
            case 7:
                AddCircuit(new OrGate());
                return;
            case 8:
                AddCircuit(new XOrGate());
                return;
        }
    }

    public void AddStartingCircuit(int startingCircuitIndex)
    {
        AddCircuit(GetStartingCircuit(startingCircuitIndex));
    }

    private void AddCircuit(Circuit newCircuit)
    {
        switch (BehaviorManager.Instance.UnpausedGameState)
        {
            case BehaviorManager.GameState.CIRCUIT_MOVEMENT:
                BehaviorManager.Instance.CancelCircuitMovement();
                break;

            case BehaviorManager.GameState.IO_PRESS:
                BehaviorManager.Instance.CancelWirePlacement();
                break;
        }

        BehaviorManager.Instance.UnpausedGameState = BehaviorManager.GameState.CIRCUIT_PLACEMENT;
        BehaviorManager.Instance.UnpausedStateType = BehaviorManager.StateType.LOCKED;
        BehaviorManager.Instance.CircuitPlacement(newCircuit);
        CloseMenu();
    }

    private Type CircuitType(int startingCircuitIndex)
    {
        switch (startingCircuitIndex)
        {
            case -1:
                return typeof(CustomCircuit);
            case 0:
                return typeof(InputGate);
            case 1:
                return typeof(Display);
            case 2:
                return typeof(Buffer);
            case 3:
                return typeof(AndGate);
            case 4:
                return typeof(NAndGate);
            case 5:
                return typeof(NOrGate);
            case 6:
                return typeof(NotGate);
            case 7:
                return typeof(OrGate);
            case 8:
                return typeof(XOrGate);
            default:
                throw new Exception("Invalid starting circuit index.");
        }
    }

    private int StartingCircuitIndex(Type circuitType)
    {
        if (circuitType == typeof(InputGate)) return 0;

        else if (circuitType == typeof(Display)) return 1;

        else if (circuitType == typeof(Buffer)) return 2;

        else if (circuitType == typeof(AndGate)) return 3;

        else if (circuitType == typeof(NAndGate)) return 4;

        else if (circuitType == typeof(NOrGate)) return 5;

        else if (circuitType == typeof(NotGate)) return 6;

        else if (circuitType == typeof(OrGate)) return 7;

        else if (circuitType == typeof(XOrGate)) return 8;

        else throw new Exception("Invalid starting circuit type.");
    }

    private void OpenMenu(bool showBackground, GameObject newMenu)
    {
        if (currentMenu != null && currentMenu != bookmarksMenu) return;

        if (currentMenu == bookmarksMenu) CloseMenu();

        currentMenu = newMenu;

        if (newMenu == bookmarksMenu)
        {
            UpdateBookmarkPosition();
            UpdateBookmarkScroll();
        }

        BehaviorManager.Instance.LockUI = true;
        background.SetActive(showBackground); currentMenu.SetActive(true);
        enabled = true;
    }

    private void UpdateBookmarkScroll()
    {
        // If the bookmarks menu does not show all bookmarked circuts, the vertical scroll bar should appear
        bool exceededViewport = bookmarkSize.y * bookmarks.Count > bookmarkMaskSize.y;
        
        if (exceededViewport)
        {
            // If large enough to scroll, always starts at top of options list
            bookmarksPanel.GetComponent<RectTransform>().anchoredPosition *= Vector2.right;
            bookmarksPanel.GetComponent<RectTransform>().sizeDelta = Vector2.right * (bookmarkSize.x + bookmarkScrollThickness);
            bookmarksBorder.sizeDelta = new Vector2(bookmarkSize.x + bookmarkScrollThickness, Mathf.Clamp(bookmarks.Count * bookmarkSize.y, 0, bookmarkMaskSize.y));
        }

        else
        {
            bookmarksPanel.GetComponent<RectTransform>().sizeDelta = Vector2.right * bookmarkSize.x;
            bookmarksBorder.sizeDelta = new Vector2(bookmarkSize.x, Mathf.Clamp(bookmarks.Count * bookmarkSize.y, 0, bookmarkMaskSize.y));
        }

        bookmarkScrollbar.SetActive(exceededViewport);
    }

    private void UpdateBookmarkPosition()
    {
        RectTransform bottomLeftPos = bookmarksScroll.GetComponent<RectTransform>();
        Vector2 currentPosition = Input.mousePosition;
        currentPosition.x -= bookmarkSize.x / 2;
        float downVal = bookmarkMaskSize.y - (bookmarkSize.y / 2 * bookmarks.Count);
        currentPosition.y -= Mathf.Clamp(downVal, bookmarkMaskSize.y / 2, bookmarkMaskSize.y);
        bottomLeftPos.anchoredPosition = currentPosition;
        Vector2 borderPosition = currentPosition;
        borderPosition.y += Mathf.Clamp(0, downVal - bookmarks.Count * bookmarkSize.y / 2, bookmarkMaskSize.y);
        bookmarksBorder.anchoredPosition = borderPosition;
    }

    public void CloseMenu()
    {
        BehaviorManager.Instance.LockUI = false;
        reopenBookmarks = false;
        Invoke("UnlockUI", 0.1f);
        background.SetActive(false); currentMenu.SetActive(false);

        if (currentMenu == addMenu) addStartingPanel.anchoredPosition = addCustomPanel.anchoredPosition = Vector2.zero;

        currentMenu = null;
        enabled = false;
    }

    private void UnlockUI()
    {
        reopenBookmarks = true;
    }

    private Circuit GetStartingCircuit(int startingCircuitIndex)
    {
        switch (startingCircuitIndex)
        {
            case 0:
                return new InputGate();
            case 1:
                return new Display();
            case 2:
                return new Buffer();
            case 3:
                return new AndGate();
            case 4:
                return new NAndGate();
            case 5:
                return new NOrGate();
            case 6:
                return new NotGate();
            case 7:
                return new OrGate();
            case 8:
                return new XOrGate();
            default:
                throw new Exception("Invalid starting circuit index.");
        }
    }

    public void Serialize()
    {
        EditorStructureManager.Instance.Serialize();
    }

    public static TaskbarManager Instance { get { return instance; } }

    public List<int> BookmarkIDs { get { return bookmarkIDs; } }

    public GameObject CurrentMenu { get { return currentMenu; } }

    public bool ReopenBookmarks { get { return reopenBookmarks; } }
}