using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using static UnityEditor.PlayerSettings;

/// <summary>
/// BehaviorManager primarily handles the bulk of all dynamic and scripted non-UI events triggerable by the user.
/// </summary>
public class BehaviorManager : MonoBehaviour
{
    // Singleton state reference
    private static BehaviorManager instance;

    /// <summary>
    /// The current state that the editor scene is in.
    /// </summary>
    public enum GameState { GRID_HOVER, CIRCUIT_HOVER, CIRCUIT_MOVEMENT, CIRCUIT_PLACEMENT, IO_HOVER, IO_PRESS, USER_INTERFACE, WIRE_HOVER, WIRE_PRESS }

    /// <summary>
    /// Utilized alongside a <seealso cref="GameState"/> to determine the consequences of each game state.<br/><br/>
    /// <seealso cref="UNRESTRICTED"/>: nothing occurs.<br/>
    /// <seealso cref="LOCKED"/>: most/all other non-UI elements are locked; UI is still enabled.<br/>
    /// <seealso cref="PAUSED"/>: all non-UI elements are locked.
    /// </summary>
    public enum StateType { UNRESTRICTED, LOCKED, PAUSED }

    /// <summary>
    /// Cancels most non-UI and UI related events when pressed.<br/><br/>
    /// More often than not, an alternate option to cancel such events also occur with the right mouse button.
    /// </summary>
    [SerializeField]
    KeyCode cancelKey;

    /// <summary>
    /// Displays the name of any hovered inputs/outputs belonging to a custom circuit, if any.
    /// </summary>
    [SerializeField]
    TextMeshProUGUI ioText;

    /// <summary>
    /// Reserved for some UI events that should only occur once. 
    /// </summary>
    private bool doOnce;

    /// <summary>
    /// Whether the left mouse button was pressed when hovered on an input or output node.
    /// </summary>
    private bool ioLMB;

    /// <summary>
    /// Whether all non-UI elements should remain cancelled no matter what.<br/><br/>.
    /// If this value is enabled, then control must be restored to the user through external means.
    /// </summary>
    private bool lockUI;

    /// <summary>
    /// Utilized internally for placing, moving, and deleting circuits and their physical GameObjects.
    /// </summary>
    private Circuit currentCircuit;

    /// <summary>
    /// The current input that the user is attempting to connect.
    /// </summary>
    private Circuit.Input currentInput;

    /// <summary>
    /// The current output that the user is attempting to connect.
    /// </summary>
    private Circuit.Output currentOutput;

    /// <summary>
    /// The current preview pin corresponding to an input node that the user is hovered on.
    /// </summary>
    private GameObject currentPreviewPin;

    /// <summary>
    /// Keeps track of the current game state as well as the previous game state if the game is currently paused.
    /// </summary>
    private GameState gameState, unpausedGameState;

    /// <summary>
    /// The opposite layer of the first input or output the user has pressed in a new connection attempt.<br/><br/>
    /// This helps determine whether the next element to press should be an input or output.<br/>
    /// If an input was first pressed, then the second valid press must be for an output, and vice-versa.
    /// </summary>
    private int ioLayerCheck;

    /// <summary>
    /// Utilized for raycasting all in-scene GameObjects to determine the current game state and state type.
    /// </summary>
    private Ray ray;

    /// <summary>
    /// Keeps track of the current state type as well as the previous state type if the game is currently paused.
    /// </summary>
    private StateType stateType, unpausedStateType;

    /// <summary>
    /// Utilized for moving circuits around the editor scene.
    /// </summary>
    private Vector3 deltaPos, endingOffset, prevDeltaPos, startingOffset, startingPos;

    // Enforces a singleton state pattern
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("BehaviorManager instance already established; terminating.");
        }

        instance = this;
    }

    // Listens to and acts on additional UI-based events.
    private void Update()
    {
        // If the scene is currently listening to UI, return and disable set values
        if (EventSystem.current.IsPointerOverGameObject() || lockUI)
        {
            if (currentPreviewPin != null)
            {
                currentPreviewPin.SetActive(false);
                currentPreviewPin = null;
            }

            // Disables the currently hovered display pin, if any
            // This specifically occurs once as ioText is externally used and can have non-empty text.
            if (doOnce && ioText.text != "") ioText.text = "";

            doOnce = false;
            return;
        }

        // Otherwise, checks for all relevant events by beginning to raycast.
        doOnce = true;
        ray = CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition);

        // If nothing is raycasted, also return and disable set values.
        if (!Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (currentPreviewPin != null)
            {
                currentPreviewPin.SetActive(false);
                currentPreviewPin = null;
            }

            if (ioText.text != "") ioText.text = "";

            return;
        }

        GameObject hitObj = hitInfo.transform.gameObject;

        // If hovered on an input or output belonging to a custom circuit, obtain its label.
        if ((hitObj.layer == 9 || hitObj.layer == 10) && hitObj.GetComponentInParent<CircuitReference>().Circuit.GetType() == typeof(CustomCircuit))
        {
            CustomCircuit customCircuit = (CustomCircuit)hitObj.GetComponentInParent<CircuitReference>().Circuit;

            int index;
            string label;

            // Is an input, therefore looks in relevant input variables.
            if (hitObj.layer == 9)
            {
                index = Array.IndexOf(customCircuit.Inputs, hitObj.GetComponent<CircuitVisualizer.InputReference>().Input);
                label = customCircuit.PreviewStructure.InputLabels[index];
            }

            // Is an output, therefore looks in relevant output variables.
            else
            {
                index = Array.IndexOf(customCircuit.Outputs, hitObj.GetComponent<CircuitVisualizer.OutputReference>().Output);
                label = customCircuit.PreviewStructure.OutputLabels[index];
            }

            ioText.text = label;
        }

        // Otherwise, therre is no label to display
        else if (ioText.text != "") ioText.text = "";

        // If hovered on an input belonging to a display, enable its corresponding preview pin
        if (hitObj.layer == 9 && hitObj.GetComponentInParent<CircuitReference>().Circuit.GetType() == typeof(Display))
        {
            // Occurs if still hovered on the same input node
            if (currentPreviewPin == hitObj.transform) return;

            Display display = (Display)hitObj.GetComponentInParent<CircuitReference>().Circuit;
            int index = -1;

            // Determines which preview pin should be enabled
            for (int i = 0; i < 8; i++)
            {
                if (display.Inputs[i].Transform.gameObject == hitObj)
                {
                    index = i;
                    break;
                }
            }

            // If the last frame focused on a separate preview pin, disable that first
            if (currentPreviewPin != null) currentPreviewPin.SetActive(false);

            // Enable the current preview pin
            currentPreviewPin = display.PreviewPins[index];
            currentPreviewPin.SetActive(true);
        }

        // Otherwise, disable the current preview pin, if any
        else if (currentPreviewPin != null)
        {
            currentPreviewPin.SetActive(false);
            currentPreviewPin = null;
        }
    }

    // Obtains a new game state/state type, and if applicable, listens to input/events corresponding to the game state.
    private void LateUpdate()
    {
        gameState = UpdateGameState();
        GameStateListener();
    }

    /// <summary>
    /// Obtains a new GameState by performing a raycast in combination with the current game state.
    /// </summary>
    /// <returns>The new game state to switch to</returns>
    private GameState UpdateGameState()
    {
        // Current state is UI
        if (EventSystem.current.IsPointerOverGameObject() || lockUI)
        {
            if (gameState == GameState.USER_INTERFACE) return gameState; // Last state was UI, return.

            // The UI state pauses the previous game state/state type, storing it in separate paused values.
            unpausedGameState = gameState;
            unpausedStateType = stateType;
            stateType = StateType.PAUSED;
            Cursor.visible = true;
            CursorManager.SetMouseTexture(true);
            return GameState.USER_INTERFACE;
        }

        // Current game state is not UI but the previous game state was.
        // Therefore, restore the game state/state type present before the user hovered onto UI.
        if (gameState == GameState.USER_INTERFACE)
        {
            gameState = unpausedGameState;
            stateType = unpausedStateType;

            // Conditions for a visible cursor
            Cursor.visible = unpausedGameState != GameState.CIRCUIT_MOVEMENT && unpausedGameState != GameState.CIRCUIT_PLACEMENT;
        }

        // Locked states must change manually, not automatically.
        if (stateType == StateType.LOCKED) return gameState;

        // The raycast reached nothing -- defaults to the grid hover state.
        if (!Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            stateType = StateType.UNRESTRICTED;
            CursorManager.SetMouseTexture(true);
            return GameState.GRID_HOVER;
        }

        GameObject hitObject = hitInfo.transform.gameObject;

        // Mouse is on top of a circuit & LMB and/or RMB have been pressed
        if (gameState == GameState.CIRCUIT_HOVER && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            currentCircuit = hitObject.GetComponentInParent<CircuitReference>().Circuit;

            // Left click (or both): circuit movement begins.
            if (Input.GetMouseButtonDown(0))
            {
                CircuitPress();
                stateType = StateType.LOCKED;
                CursorManager.SetMouseTexture(false);
                Cursor.visible = false;
            }

            // Right click: destroy current circuit.
            else
            {
                CircuitCaller.Destroy(currentCircuit);
                stateType = StateType.UNRESTRICTED;
            }

            return GameState.CIRCUIT_MOVEMENT;
        }

        // Mouse is on top of a circuit
        if (hitObject.layer == 8) // 8 --> circuit base layer
        {
            stateType = StateType.UNRESTRICTED;
            CursorManager.SetMouseTexture(false);
            return GameState.CIRCUIT_HOVER;
        }

        // Mouse is on top of an input/output & LMB and/or RMB and/or MMB have been pressed
        if (gameState == GameState.IO_HOVER && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
        {
            ioLMB = Input.GetMouseButtonDown(0);
            stateType = StateType.LOCKED;
                
            // Left click (perahsp with other inputs, but LMB has the highest preference): begins the connection process
            if (ioLMB)
            {
                IOLMBPress(hitObject);
                CursorManager.SetMouseTexture(true);
            }

            // Right click or middle mouse button: alternate press
            // RMB: deletes all connections attached to the input/output in question
            // MMB (only applicable if hovered onto an input gate's output): switch power states
            else IOAlternatePress(hitObject);

            return GameState.IO_PRESS;
        }

        // Mouse is on top of any input or output
        if (hitObject.layer == 9 || hitObject.layer == 10)
        {
            stateType = StateType.UNRESTRICTED;
            CursorManager.SetMouseTexture(false);
            return GameState.IO_HOVER;
        }

        // Mouse is on top of a wire & RMB has been pressed
        if (gameState == GameState.WIRE_HOVER && Input.GetMouseButtonDown(1))
        {
            stateType = StateType.LOCKED;
            CursorManager.SetMouseTexture(true);
            WirePress(hitObject); // Deletes the wire and its corresponding connection
            return GameState.WIRE_PRESS;
        }

        // Mouse is on top of a wire
        if (hitObject.layer == 11)
        {
            stateType = StateType.UNRESTRICTED;
            CursorManager.SetMouseTexture(false);
            return GameState.WIRE_HOVER;
        }

        // If none of the other conditions were met, default to the grid hover state instead.
        stateType = StateType.UNRESTRICTED;
        CursorManager.SetMouseTexture(true);
        return GameState.GRID_HOVER;
    }

    /// <summary>
    /// Begins the connection process.
    /// </summary>
    /// <param name="hitObject">The GameObject that was raycasted.</param>
    private void IOLMBPress(GameObject hitObject)
    {
        Vector3 startingPos;

        // Input layer was pressed; next press should be on an output layer
        if (hitObject.layer == 9)
        {
            currentInput = hitObject.GetComponent<CircuitVisualizer.InputReference>().Input;
            ioLayerCheck = 10;
            startingPos = currentInput.Transform.position;
        }

        // Output layer was pressed; next press should be on an input layer
        else
        {
            currentOutput = hitObject.GetComponent<CircuitVisualizer.OutputReference>().Output;
            ioLayerCheck = 9;
            startingPos = currentOutput.Transform.position;
        }

        CircuitConnector.Instance.BeginConnectionProcess(startingPos);
    }

    /// <summary>
    /// Based on context, deletes all connections belonging to an input or output node or alternates the power status of an input gate.
    /// </summary>
    /// <param name="hitObject"></param>
    private void IOAlternatePress(GameObject hitObject)
    {
        // If the raycasted object was an input and the MMB is pressed, alternate its input
        if (Input.GetMouseButtonDown(2))
        {
            if (hitObject.layer == 10 && hitObject.GetComponentInParent<CircuitReference>().Circuit.GetType() == typeof(InputGate))
            {
                InputGate gate = (InputGate)hitObject.GetComponentInParent<CircuitReference>().Circuit;

                gate.Powered = !gate.Powered;
                EditorStructureManager.Instance.DisplaySavePrompt = true; // Important enough to trigger the save prompt
            }

        }

        // RMB on an input -- begin disconnection process
        else if (hitObject.layer == 9)
        {
            Circuit.Input input = hitObject.GetComponent<CircuitVisualizer.InputReference>().Input;

            // If there is a connection, disconnect it.
            if (input.Connection != null) CircuitConnector.Disconnect(input.Connection);
        }

        // RMB on an output -- begin disconnection process
        else
        {
            Circuit.Output output = hitObject.GetComponent<CircuitVisualizer.OutputReference>().Output;
            List<CircuitConnector.Connection> connections = new List<CircuitConnector.Connection>(output.Connections);

            // Disconnects each connection associated with this output, if any.
            foreach (CircuitConnector.Connection connection in connections) CircuitConnector.Disconnect(connection);
        }

        stateType = StateType.UNRESTRICTED;
    }

    /// <summary>
    /// Called after a new circuit has been instantiated; sets initial values.
    /// </summary>
    /// <param name="currentCircuit">The circuit that has just been created.</param>
    public void CircuitPlacement(Circuit currentCircuit)
    {
        // If there is a already a circuit in the process of being placed, destroy it.
        if (this.currentCircuit != null) { CircuitCaller.Destroy(this.currentCircuit); }

        this.currentCircuit = currentCircuit;
        currentCircuit.PhysicalObject.transform.position = Coordinates.Instance.MousePos;
    }

    /// <summary>
    /// Deletes a wire GameObject and its associated connection
    /// </summary>
    /// <param name="hitObject">The wire to delete.</param>
    private void WirePress(GameObject hitObject)
    {
        CircuitConnector.Connection connection;

        // Determines if the raycasted object is the parent mesh (aka not the starting/ending wire mesh).
        if (hitObject.transform.parent == null)
        {
            connection = hitObject.GetComponent<CircuitConnector.Connection>();
            Destroy(hitObject.transform.gameObject);
        }

        // Otherwise, is a starting or ending wire.
        else
        {
            connection = hitObject.GetComponentInParent<CircuitConnector.Connection>();
            Destroy(hitObject.transform.parent.parent.gameObject);
        }

        CircuitConnector.Disconnect(connection); // Disconnects the logic associated with the connection
        stateType = StateType.UNRESTRICTED;
    }

    /// <summary>
    /// Called after a circuit has been pressed; sets initial values.
    /// </summary>
    private void CircuitPress()
    {
        Vector3 mousePos = Coordinates.Instance.MousePos;

        startingPos = currentCircuit.PhysicalObject.transform.position;
        endingOffset = startingOffset = mousePos;
    }

    /// <summary>
    /// Cancels the connection process.
    /// </summary>
    public void CancelWirePlacement()
    {
        CircuitConnector.Instance.CancelConnectionProcess();
        currentInput = null; currentOutput = null;
    }

    /// <summary>
    /// Cancels the circuit movement process.
    /// </summary>
    public void CancelCircuitMovement()
    {
        Cursor.visible = true;
        currentCircuit = null;
    }

    /// <summary>
    /// Called after obtaining a new game state; listens to input/events corresponding to the game state.
    /// </summary>
    private void GameStateListener()
    {
        switch (gameState)
        {
            case GameState.GRID_HOVER:
                // Opens the bookmarked circuits menu.
                if (Input.GetMouseButtonDown(1) && TaskbarManager.Instance.CurrentMenu == null && TaskbarManager.Instance.ReopenBookmarks) TaskbarManager.Instance.OpenBookmarks();

                return;
            case GameState.IO_PRESS:
                if (!ioLMB) return; // The left mouse button was not pressed, therefore the corresponding connection code should be skipped.

                // Checks to see if the user is hovered on a valid GameObject to complete the connection process.
                if (Physics.Raycast(CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo) && hitInfo.transform.gameObject.layer == ioLayerCheck)
                {
                    // Output layer was initially pressed, therefore this is an input node
                    if (ioLayerCheck == 9) currentInput = hitInfo.transform.GetComponent<CircuitVisualizer.InputReference>().Input;

                    // Input layer was initially pressed, therefore this is an output node
                    else currentOutput = hitInfo.transform.GetComponent<CircuitVisualizer.OutputReference>().Output;

                    CursorManager.SetMouseTexture(false);

                    // The user completes the connection process by hovering on a valid input AND pressing the left mouse button.
                    if (Input.GetMouseButtonDown(0))
                    {
                        EditorStructureManager.Instance.DisplaySavePrompt = true; // Important enough to trigger the save prompt

                        // Disconnects the current connection to the input, if there is one
                        if (currentInput.ParentOutput != null) CircuitConnector.Disconnect(currentInput.Connection);

                        CircuitConnector.Connection connection = CircuitConnector.Instance.CurrentConnection;

                        CircuitConnector.Connect(currentInput, currentOutput); // Ensures the connection is logically accounted for

                        // If the order of selection was not output -> input, the starting and ending wires are swapped with one another.
                        // This occurs because the starting wire is always associated with the input node, hence the GameObjects are swapped to maintain this rule.
                        if (ioLayerCheck == 10)
                        {
                            GameObject temp = connection.StartingWire;

                            // Swaps the starting and ending wires within the connection
                            connection.StartingWire = connection.EndingWire;
                            connection.EndingWire = temp;

                            // Ensures the serialization process works as intended by keeping the hierarchy order of the wires the same, regardless of connection order.
                            if (connection.StartingWire != connection.EndingWire)
                            {
                                connection.StartingWire.name = "Starting Wire";
                                connection.EndingWire.name = "Ending Wire";
                                connection.StartingWire.transform.SetAsFirstSibling();
                            }
                        }
                        
                        stateType = StateType.UNRESTRICTED;
                        currentInput = null; currentOutput = null;
                        return;
                    }
                }

                else CursorManager.SetMouseTexture(true);

                // Cancels the connection process.
                if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
                {
                    CancelWirePlacement();
                    stateType = StateType.UNRESTRICTED;
                }

                break;
            case GameState.CIRCUIT_MOVEMENT:
                // Cancels the circuit movement process if the left mouse button is not held.
                if (!Input.GetMouseButton(0))
                {
                    CancelCircuitMovement();
                    stateType = StateType.UNRESTRICTED;
                    return;
                }

                // Calculates the delta mouse movement from the last frame
                endingOffset = Coordinates.Instance.MousePos;
                prevDeltaPos = deltaPos;
                deltaPos = endingOffset - startingOffset + startingPos;

                // Snaps the obtained position to the grid if grid snapping is enabled.
                if (Coordinates.Instance.CurrentSnappingMode == Coordinates.SnappingMode.GRID) deltaPos = Coordinates.NormalToGridPos(deltaPos);

                currentCircuit.PhysicalObject.transform.position = deltaPos;

                if (prevDeltaPos != deltaPos) // Ensures the circuit has moved from its previous position before updating the transforms of both wire GameObjects.
                {
                    EditorStructureManager.Instance.DisplaySavePrompt = true; // Important enough to trigger the save prompt

                    // Updates the position/scale each valid connection associated with the inputs of the moved circuit.
                    // This occurs so that each physical wire continues to stretch/shrink and follow each circuit within the scene.
                    foreach (Circuit.Input input in currentCircuit.Inputs)
                    {
                        if (input.Connection != null)
                        {
                            bool isCentered = input.Connection.EndingWire == input.Connection.StartingWire;
                            Vector3 fromPos = isCentered ? input.Connection.Output.Transform.position : input.Connection.EndingWire.transform.position;

                            CircuitConnector.UpdatePosition(input.Connection.EndingWire, fromPos, input.Transform.position, isCentered);
                        }
                    }

                    // Updates the position/scale each valid connection associated with the outputs of the moved circuit.
                    // This occurs so that each physical wire continues to stretch/shrink and follow each circuit within the scene.
                    foreach (Circuit.Output output in currentCircuit.Outputs)
                    {
                        foreach (CircuitConnector.Connection connection in output.Connections)
                        {
                            bool isCentered = connection.EndingWire == connection.StartingWire;
                            Vector3 fromPos = isCentered ? connection.Input.Transform.position : connection.StartingWire.transform.position;

                            CircuitConnector.UpdatePosition(connection.StartingWire, fromPos, output.Transform.position, isCentered);
                        }
                    }
                }

                break;
            case GameState.CIRCUIT_PLACEMENT:
                // Until its placement is confirmed, the circuit follows the mouse cursor.
                currentCircuit.PhysicalObject.transform.position = Coordinates.Instance.ModePos;

                // Placement is confirmed
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.visible = true;
                    EditorStructureManager.Instance.Circuits.Add(currentCircuit); // Adds circuit for potential serialization
                    EditorStructureManager.Instance.DisplaySavePrompt = true;
                    currentCircuit = null;
                    stateType = StateType.UNRESTRICTED;
                    LateUpdate();
                    return;
                }

                // Placement is cancelled; delete the circuit.
                if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
                {
                    Cursor.visible = true;
                    CircuitCaller.Destroy(currentCircuit);
                    currentCircuit = null;
                    stateType = StateType.UNRESTRICTED;
                }

                break;
        }
    }

    // Getter and setter methods
    public GameState UnpausedGameState { get { return unpausedGameState; } set { unpausedGameState = value; } }

    public StateType UnpausedStateType { get { return unpausedStateType; } set { unpausedStateType = value; } }

    // Getter methods
    public static BehaviorManager Instance { get { return instance; } }

    public bool LockUI { get { return lockUI; } set { lockUI = value; } }

    public GameState CurrentGameState { get { return gameState; } }

    public int IOLayerCheck { get { return ioLayerCheck; } }

    public StateType CurrentStateType { get { return stateType; } }
}