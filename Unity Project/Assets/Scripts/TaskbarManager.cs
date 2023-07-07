using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarManager : MonoBehaviour
{
    public static List<Type> bookmarks = new List<Type>();

    private static TaskbarManager instance;

    [SerializeField] Color startingCircuitColor;

    [SerializeField] float bookmarkScrollThickness;

    [SerializeField] GameObject bookmarkScrollbar, background, addMenu, bookmarksMenu, bookmarksScroll, bookmarksPanel;

    [SerializeField] GameObject bookmarkRef;

    [SerializeField] KeyCode cancelKey;

    [SerializeField] RectTransform addStartingPanel, bookmarksBorder;

    [SerializeField] Vector2 bookmarkSize, bookmarkMaskSize;

    private GameObject currentMenu;

    private bool reopenBookmarks = true, bookmarksDown;

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
        // Depending on the current opened menu, the escape controls may alter and thus they are differentiated
        if (currentMenu == bookmarksMenu)
        {
            if (Input.GetMouseButtonDown(0) && !bookmarksDown) { bookmarksDown = true; }

            else if (Input.GetMouseButtonUp(0) && bookmarksDown) { CloseMenu(); }

            else if (Input.GetMouseButtonDown(1)) UpdateBookmarkPosition();

            else if (Input.GetKeyDown(cancelKey)) CloseMenu();
        }

        else if (currentMenu == addMenu)
        {
            if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
            {
                CloseMenu();
            }
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

    public void OpenBookmarks(bool showBackground)
    {
        if (bookmarks.Count == 0) return;

        bookmarksDown = false;
        OpenMenu(showBackground, bookmarksMenu);
    }

    public void UpdateBookmark(GameObject obj)
    {
        bool newStatus = obj.GetComponent<Toggle>().isOn;
        Type type = CircuitType(obj.transform.parent.GetSiblingIndex());

        if (newStatus && !bookmarks.Contains(type))
        {
            bookmarks.Add(type);
            GameObject bookmark = Instantiate(bookmarkRef, bookmarksPanel.transform);
            Button button = bookmark.GetComponentInChildren<Button>();
            TextMeshProUGUI text = bookmark.GetComponentInChildren<TextMeshProUGUI>();
            bookmark.name = text.text = obj.transform.parent.name;
            text.color = startingCircuitColor;

            button.onClick.AddListener(delegate { AddBookmarkCircuit(type); });
        }

        else if (!newStatus && bookmarks.Contains(type))
        {
            int index = bookmarks.IndexOf(type);
            bookmarks.Remove(type);
            Destroy(bookmarksPanel.transform.GetChild(index).gameObject);
        }
    }

    private void AddBookmarkCircuit(Type type)
    {
        ConstructorInfo[] constructorInfo = type.GetConstructors();

        AddCircuit((Circuit)constructorInfo[0].Invoke(new object[] { }));
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

    private void CloseMenu()
    {
        BehaviorManager.Instance.LockUI = false;
        reopenBookmarks = false;
        Invoke("UnlockUI", 0.1f);
        background.SetActive(false); currentMenu.SetActive(false);
        
        if (currentMenu == addMenu) addStartingPanel.anchoredPosition = Vector2.zero;

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

    public static TaskbarManager Instance { get { return instance; } }

    public GameObject CurrentMenu { get { return currentMenu; } }

    public bool ReopenBookmarks { get { return reopenBookmarks; } }
}