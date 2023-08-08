using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// IOAssigner is enabled in the editor scene after a custom circuit is validated to assign input/output orders and labels.
/// </summary>
public class IOAssigner : MonoBehaviour
{
    // Singleton state reference
    private static IOAssigner instance;

    /// <summary>
    /// The type of text <seealso cref="hoverText"/> should display.<br/><br/>
    /// <seealso cref="NONE"/>: prompts the user to hover on valid inputs/outputs.<br/>
    /// <seealso cref="INPUT"/>: currently hovered onto an input; display its potential order.<br/>
    /// <seealso cref="OUTPUT"/>: currently hovered onto an output; display its potential order.
    /// </summary>
    private enum TextMode { NONE, INPUT, OUTPUT }

    /// <summary>
    /// Exits the IOAssigner phase; the circuit must be validated again to reach this point.
    /// </summary>
    [SerializeField]
    KeyCode exitKey;

    /// <summary>
    /// The material applied to all empty inputs that are active and not being hovered on.
    /// </summary>
    [SerializeField]
    Material emptyInputMaterial;

    /// <summary>
    /// The material applied to all empty outputs that are active and not being hovered on.
    /// </summary>
    [SerializeField]
    Material emptyOutputMaterial;

    /// <summary>
    /// The material applied to an empty input/output that is currently hovered on.
    /// </summary>
    [SerializeField]
    Material hoveredMaterial;

    /// <summary>
    /// Displays text determined by the current <seealso cref="TextMode"/>.
    /// </summary>
    [SerializeField]
    TextMeshProUGUI hoverText;

    /// <summary>
    /// Whether the user has clicked on an empty input/output to bring about the label composition UI.
    /// </summary>
    private bool labelSelectionMode;
    
    /// <summary>
    /// Bypasses the movement system; prevents movement when composing a label.
    /// </summary>
    private bool movementOverride;

    /// <summary>
    /// The current selected input.
    /// </summary>
    private Circuit.Input currentInput;

    /// <summary>
    /// The list of empty inputs given to IOAssigner by <see cref="PreviewStructureManager"/>.<br/><br/>
    /// These inputs are guaranteed to be valid, and the role of IOAssigner is to have the user label and order them to their liking.
    /// </summary>
    private List<Circuit.Input> emptyInputs;

    /// <summary>
    /// Contains all elements from <seealso cref="emptyInputs"/>, now reordered by the user.<br/><br/>
    /// </summary>
    private List<Circuit.Input> orderedInputs;

    /// <summary>
    /// The current selected output.
    /// </summary>
    private Circuit.Output currentOutput;

    /// <summary>
    /// The list of empty outputs given to IOAssigner by <see cref="PreviewStructureManager"/>.<br/><br/>
    /// These outputs are guaranteed to be valid, and the role of IOAssigner is to have the user label and order them to their liking.
    /// </summary>
    private List<Circuit.Output> emptyOutputs;

    /// <summary>
    /// Contains all elements from <seealso cref="emptyOutputs"/>, now reordered by the user.<br/><br/>
    /// </summary>
    private List<Circuit.Output> orderedOutputs;

    /// <summary>
    /// The current valid GameObject hovered on by the user.<br/><br/>
    /// This GameObject is guaranteed to either contain an <see cref="Circuit.Input"/> in <seealso cref="emptyInputs"/> or an <see cref="Circuit.Output"/> in <seealso cref="emptyOutputs"/>.
    /// </summary>
    private GameObject currentHover;

    /// <summary>
    /// Number of inputs and outputs have been successfully assigned and labeled by the user.
    /// </summary>
    private int inputCount, outputCount;

    /// <summary>
    /// The number of inputs and outputs that should be assigned for IOAssigner to complete its task.<br/><br/>
    /// Both values initially are equal to their respective lengths of <seealso cref="emptyInputs"/> and <seealso cref="emptyOutputs"/>, but both lists have user-assigned elements removed.
    /// </summary>
    private int targetInputCount, targetOutputCount;

    /// <summary>
    /// Contain the respective labels for each user-assigned input and output.
    /// </summary>
    private List<string> inputLabels, outputLabels;

    // Enforces a singleton state pattern and disables update calls.
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("IOAssigner instance already established; terminating.");
        }

        instance = this;
        enabled = false; // After completing instance assignment, disable update calls until script is needed.
    }

    private void Update()
    {
        // Currently composing a label for an empty input or output.
        if (labelSelectionMode)
        {
            // Exit conditions for quitting the labeling interface.
            if (Input.GetKeyDown(exitKey) || Input.GetMouseButtonDown(1)) { CancelIOPress(); }

            return;
        }

        // Exit conditions for quitting IOAssigner.
        if (Input.GetKeyDown(exitKey) || Input.GetMouseButtonDown(1)) { Exit(); return; }

        Ray ray = CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition);

        // Checks to see if the raycast hit ANY input or output
        if (Physics.Raycast(ray, out RaycastHit hitInfo) && (hitInfo.transform.gameObject.layer == 9 || hitInfo.transform.gameObject.layer == 10))
        {
            Circuit.Input input = null; Circuit.Output output = null;
            GameObject hitObject = hitInfo.transform.gameObject;

            // Is an input
            if (hitObject.layer == 9)
            {
                input = hitObject.GetComponent<CircuitVisualizer.InputReference>().Input;

                // If not within emptyInputs, not valid.
                if (!emptyInputs.Contains(input)) return;
            }

            // Is an output
            else
            {
                output = hitObject.GetComponent<CircuitVisualizer.OutputReference>().Output;

                // If not within emptyOutputs, not valid.
                if (!emptyOutputs.Contains(output)) return;
            }

            // Updates the interface and input/output material if the current hit object is not the hovered object from the last frame.
            if (currentHover != hitObject)
            {
                // If the last hit object was something, restore its default material.
                if (currentHover != null) currentHover.GetComponent<MeshRenderer>().material = currentHover.layer == 9 ? emptyInputMaterial : emptyOutputMaterial;

                // Update material and set text based on GameObject layer (i.e. input or output)
                hitObject.GetComponent<MeshRenderer>().material = hoveredMaterial;
                SetHoverText(hitObject.layer == 9 ? TextMode.INPUT : TextMode.OUTPUT);

                // Store as current hover object
                currentHover = hitObject;
            }

            // Also begins the labeling process if LMB is pressed
            if (Input.GetMouseButtonDown(0)) OnIOPress(input, output);
        }

        // Restores to default values if the raycast was unsuccessful.
        else if (currentHover != null)
        {
            currentHover.GetComponent<MeshRenderer>().material = currentHover.layer == 9 ? emptyInputMaterial : emptyOutputMaterial;
            currentHover = null;
            SetHoverText(TextMode.NONE);
        }
    }

    /// <summary>
    /// Enables IOAssigner and starts the labeling/ordering process.
    /// </summary>
    /// <param name="emptyInputs">The empty inputs of the prospective custom circuit.</param>
    /// <param name="emptyOutputs">The empty outputs of the prospective custom circuit.</param>
    public void Initialize(List<Circuit.Input> emptyInputs, List<Circuit.Output> emptyOutputs)
    {
        labelSelectionMode = false; movementOverride = true;
        inputCount = outputCount = 0;
        targetInputCount = emptyInputs.Count; targetOutputCount = emptyOutputs.Count;
        this.emptyInputs = emptyInputs; this.emptyOutputs = emptyOutputs;
        orderedInputs = new List<Circuit.Input>(); orderedOutputs = new List<Circuit.Output>();
        inputLabels = new List<string>(); outputLabels = new List<string>();
        hoverText.gameObject.SetActive(true);

        // Switches the materials of all empty inputs and outputs to highlight them.
        foreach (Circuit.Input input in emptyInputs) input.Transform.GetComponent<MeshRenderer>().material = emptyInputMaterial;

        foreach (Circuit.Output output in emptyOutputs) output.Transform.GetComponent<MeshRenderer>().material = emptyOutputMaterial;

        SetHoverText(TextMode.NONE);
        Update();
        enabled = true; // Enables frame-by-frame update calls from Unity.
    }
    
    /// <summary>
    /// Disables IOAssigner and exists the labeling/ordering process.
    /// </summary>
    private void Exit()
    {
        movementOverride = false;

        // If there are any empty inputs and/or outputs left, restore their default material.
        foreach (Circuit.Input input in emptyInputs) input.Transform.GetComponent<MeshRenderer>().material = CircuitVisualizer.Instance.InputMaterial;

        foreach (Circuit.Output output in emptyOutputs) output.Transform.GetComponent<MeshRenderer>().material = CircuitVisualizer.Instance.OutputMaterial;

        hoverText.gameObject.SetActive(false);
        enabled = false; // Disables frame-by-frame update calls from Unity.
        TaskbarManager.Instance.CloseMenu();
    }

    /// <summary>
    /// Cancels the label selection process for the current input or output.
    /// </summary>
    public void CancelIOPress()
    {
        movementOverride = true; labelSelectionMode = false;
        TaskbarManager.Instance.CloseMenu();
        TaskbarManager.Instance.NullState();
    }

    /// <summary>
    /// Begins the label selection process for the current input or output.<br/><br/>
    /// While there is both an input and output parameter, one of them will always be null.
    /// </summary>
    /// <param name="input">The input to label.</param>
    /// <param name="output">The output to label.</param>
    private void OnIOPress(Circuit.Input input, Circuit.Output output)
    {
        movementOverride = false; labelSelectionMode = true;
        currentInput = input; currentOutput = output;
        TaskbarManager.Instance.CloseMenu();
        TaskbarManager.Instance.OpenLabelMenu(input != null);
    }

    /// <summary>
    /// Successfully completes the label selection process for the current input or output.
    /// </summary>
    /// <param name="inputField">The text box to extract the label name from.</param>
    public void ConfirmIOPress(TMP_InputField inputField)
    {
        string labelName = inputField.text;

        // Implies the current labeling was done for an input
        if (currentInput != null)
        {
            // Moves the current input to the ordered list and out of the empty list.
            inputCount++;
            emptyInputs.Remove(currentInput);
            orderedInputs.Add(currentInput);
            inputLabels.Add(labelName);
        }

        // Implies the current labeling was done for an output
        else
        {
            // Moves the current output to the ordered list and out of the empty list.
            outputCount++;
            emptyOutputs.Remove(currentOutput);
            orderedOutputs.Add(currentOutput);
            outputLabels.Add(labelName);
        }

        inputField.text = "";
        currentHover.GetComponent<MeshRenderer>().material = currentHover.layer == 9 ? CircuitVisualizer.Instance.InputMaterial : CircuitVisualizer.Instance.OutputMaterial;
        currentHover = null;
        SetHoverText(TextMode.NONE);
        labelSelectionMode = false;
        TaskbarManager.Instance.CloseMenu();
        TaskbarManager.Instance.NullState();

        // All inputs/outputs have been labeled and/or ordered
        if (inputCount == targetInputCount && outputCount == targetOutputCount)
        {
            hoverText.gameObject.SetActive(false);
            enabled = false;
            PreviewStructureManager.Instance.CreateCustomCircuit(orderedInputs, orderedOutputs, inputLabels, outputLabels); // Finally creates the custom circuit.
        }

        else movementOverride = true;
    }

    /// <summary>
    /// Modifies the value of <seealso cref="hoverText"/> based on the assigned text mode.
    /// </summary>
    /// <param name="textMode">The current text mode.</param>
    private void SetHoverText(TextMode textMode)
    {
        // Default text
        string text = "hover over and select inputs/outputs to determine their order & label";

        // Modifies the text if an input or output is implied.
        switch (textMode)
        {
            case TextMode.INPUT:
                text = "input #" + (inputCount + 1);
                break;
            case TextMode.OUTPUT:
                text = "output #" + (outputCount + 1);
                break;
        }

        hoverText.text = text;
    }

    // Getter methods
    public static IOAssigner Instance { get { return instance; } }

    public bool MovementOverride { get { return movementOverride; } }
}