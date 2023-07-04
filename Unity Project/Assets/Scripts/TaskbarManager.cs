using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarManager : MonoBehaviour
{
    [SerializeField] GameObject background, addMenu;

    [SerializeField] KeyCode cancelKey;

    [SerializeField] RectTransform addStartingPanel;

    private GameObject currentMenu;

    private void Update()
    { 
        if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
        {
            CloseMenu();
        }
    }

    public void OpenAdd()
    {
        currentMenu = addMenu;
        OpenMenu();
    }

    public void UpdateBookmark(Toggle toggle, int startingCircuitIndex)
    {
        Debug.Log(toggle.isOn);
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

    private void OpenMenu()
    {
        BehaviorManager.Instance.LockUI = true;
        background.SetActive(true); currentMenu.SetActive(true);
        enabled = true;
    }

    private void CloseMenu()
    {
        BehaviorManager.Instance.LockUI = false;
        background.SetActive(false); currentMenu.SetActive(false);
        currentMenu = null;
        ResetScroll();
        enabled = false;
    }

    private void ResetScroll()
    {
        addStartingPanel.anchoredPosition = Vector2.zero;
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
}