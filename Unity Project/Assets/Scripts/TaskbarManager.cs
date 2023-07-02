using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskbarManager : MonoBehaviour
{
    [SerializeField] GameObject background, addMenu;

    [SerializeField] KeyCode cancelKey;

    [SerializeField] RectTransform addStartingPanel;

    private GameObject currentMenu;

    private void Update()
    { 
        if (Input.GetKeyDown(cancelKey))
        {
            CloseMenu();
        }
    }

    public void OpenAdd()
    {
        currentMenu = addMenu;
        OpenMenu();
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

    public void AddInput()
    {
        AddCircuit(new InputGate());
    }

    public void AddDisplay()
    {

    }

    public void AddAnd()
    {
        AddCircuit(new AndGate());
    }

    public void AddNAnd()
    {
        AddCircuit(new NAndGate());
    }

    public void AddNOr()
    {
        AddCircuit(new NOrGate());
    }

    public void AddNot()
    {
        AddCircuit(new NotGate());
    }

    public void AddOr()
    {
        AddCircuit(new OrGate());
    }

    public void AddXor()
    {
        AddCircuit(new XOrGate());
    }

}
