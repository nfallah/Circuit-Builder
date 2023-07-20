using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IOAssigner : MonoBehaviour
{
    private static IOAssigner instance;

    private enum TextMode { NONE, INPUT, OUTPUT }

    [SerializeField] KeyCode exitKey;

    [SerializeField] Material hoveredMaterial, emptyInputMaterial, emptyOutputMaterial;

    [SerializeField] TextMeshProUGUI hoverText;

    private bool labelSelectionMode, movementOverride;

    private Circuit.Input currentInput;

    private Circuit.Output currentOutput;

    private GameObject currentHover;

    private int inputCount, outputCount, targetInputCount, targetOutputCount;

    private List<Circuit.Input> emptyInputs, orderedInputs;

    private List<Circuit.Output> emptyOutputs, orderedOutputs;

    private List<string> inputLabels, outputLabels;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("IOAssigner instance already established; terminating.");
        }

        instance = this;
        enabled = false;
    }

    private void Update()
    {
        if (labelSelectionMode)
        {
            if (Input.GetKeyDown(exitKey) || Input.GetMouseButtonDown(1)) { CancelIOPress(); }

            return;
        }

        if (Input.GetKeyDown(exitKey) || Input.GetMouseButtonDown(1)) { Exit(); return; }

        Ray ray = CameraMovement.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo) && (hitInfo.transform.gameObject.layer == 9 || hitInfo.transform.gameObject.layer == 10))
        {
            Circuit.Input input = null; Circuit.Output output = null;
            GameObject hitObject = hitInfo.transform.gameObject;

            if (hitObject.layer == 9)
            {
                input = hitObject.GetComponent<CircuitVisualizer.InputReference>().Input;

                if (!emptyInputs.Contains(input)) return;
            }

            else
            {
                output = hitObject.GetComponent<CircuitVisualizer.OutputReference>().Output;

                if (!emptyOutputs.Contains(output)) return;
            }

            if (currentHover != hitObject)
            {
                if (currentHover != null) currentHover.GetComponent<MeshRenderer>().material = currentHover.layer == 9 ? emptyInputMaterial : emptyOutputMaterial;

                hitObject.GetComponent<MeshRenderer>().material = hoveredMaterial;
                SetHoverText(hitObject.layer == 9 ? TextMode.INPUT : TextMode.OUTPUT);
                currentHover = hitObject;
            }

            if (Input.GetMouseButtonDown(0)) OnIOPress(input, output);
        }

        else if (currentHover != null)
        {
            currentHover.GetComponent<MeshRenderer>().material = currentHover.layer == 9 ? emptyInputMaterial : emptyOutputMaterial;
            currentHover = null;
            SetHoverText(TextMode.NONE);
        }
    }

    public void Initialize(List<Circuit.Input> emptyInputs, List<Circuit.Output> emptyOutputs)
    {
        labelSelectionMode = false; movementOverride = true;
        inputCount = outputCount = 0;
        targetInputCount = emptyInputs.Count; targetOutputCount = emptyOutputs.Count;
        this.emptyInputs = emptyInputs; this.emptyOutputs = emptyOutputs;
        orderedInputs = new List<Circuit.Input>(); orderedOutputs = new List<Circuit.Output>();
        inputLabels = new List<string>(); outputLabels = new List<string>();
        hoverText.gameObject.SetActive(true);

        foreach (Circuit.Input input in emptyInputs) input.Transform.GetComponent<MeshRenderer>().material = emptyInputMaterial;

        foreach (Circuit.Output output in emptyOutputs) output.Transform.GetComponent<MeshRenderer>().material = emptyOutputMaterial;

        SetHoverText(TextMode.NONE);
        Update();
        enabled = true;
    }
    
    private void Exit()
    {
        movementOverride = false;

        foreach (Circuit.Input input in emptyInputs) input.Transform.GetComponent<MeshRenderer>().material = CircuitVisualizer.Instance.InputMaterial;

        foreach (Circuit.Output output in emptyOutputs) output.Transform.GetComponent<MeshRenderer>().material = CircuitVisualizer.Instance.OutputMaterial;

        hoverText.gameObject.SetActive(false);
        enabled = false;
        TaskbarManager.Instance.CloseMenu();
    }

    public void CancelIOPress()
    {
        movementOverride = true; labelSelectionMode = false;
        TaskbarManager.Instance.CloseMenu();
        TaskbarManager.Instance.NullState();
    }

    private void OnIOPress(Circuit.Input input, Circuit.Output output)
    {
        movementOverride = false; labelSelectionMode = true;
        currentInput = input; currentOutput = output;
        TaskbarManager.Instance.CloseMenu();
        TaskbarManager.Instance.OpenLabelMenu(input != null);
    }

    public void ConfirmIOPress(TMP_InputField inputField)
    {
        string labelName = inputField.text;

        if (currentInput != null)
        {
            inputCount++;
            emptyInputs.Remove(currentInput);
            orderedInputs.Add(currentInput);
            inputLabels.Add(labelName);
        }

        else
        {
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
            PreviewStructureManager.Instance.CreateCustomCircuit(orderedInputs, orderedOutputs, inputLabels, outputLabels);
        }

        else
        {
            movementOverride = true;
        }
    }

    private void SetHoverText(TextMode textMode)
    {
        string text = "hover over and select inputs/outputs to determine their order & label";

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

    // Singleton state reference
    public static IOAssigner Instance { get { return instance; } }

    // Getter method
    public bool MovementOverride { get { return movementOverride; } }
}