using System;
using TMPro;
using UnityEngine;

public class CircuitVisualizer : MonoBehaviour
{
    private static CircuitVisualizer instance; // Ensures a singleton state pattern is maintained

    public class InputReference : MonoBehaviour
    {
        private Circuit.Input input;

        public Circuit.Input Input { get { return input; } set { input = value; } }
    }

    public class OutputReference : MonoBehaviour
    {
        private Circuit.Output output;

        public Circuit.Output Output { get { return output; } set { output = value; } }
    }

    // The color associated with starting circuits (non-custom)
    [SerializeField] Color startingCircuitColor;

    [SerializeField] float
    borderThickness, // Border surrounding the base of the circuit
    inputSize, // Square dimensions of an input node
    outputSize, // Square dimensions of an output node
    powerSize, // Square dimensions of the power indicator on input and output nodes
    heightMargins, // The distance between each input and output node
    width; // Constant referring to the width of the circuit

    // Reference to the "DISPLAY" prefab, to be utilized as needed when a display circuit is created
    [SerializeField] GameObject displayRef;

    // Represents custom materials for the circuit base & I/O
    [SerializeField] Material baseMaterial, borderMaterial, inputMaterial, outputMaterial, powerOffMaterial, powerOnMaterial;

    [SerializeField] Vector2 textPadding;

    [SerializeField] TMP_FontAsset font;

    private readonly int[] triangles = new int[] { 0, 1, 3, 3, 1, 2 }; // Constant referring to the triangles of any quad

    private readonly Vector2[] uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) }; // Constant referring to the uv of any quad

    private readonly Vector3[] normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up }; // Constant referring to normals of any quad

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("CircuitVisualizer instance already established; terminating.");
        }

        instance = this;
    }

    // Generates a mesh corresponding to the name & inputs/outputs of a given circuit
    public void VisualizeCircuit(Circuit circuit)
    {
        VisualizeCircuit(circuit, Vector2.zero);
    }

    public void VisualizeCircuit(Circuit circuit, Vector2 startingPosition)
    {
        // Target circuit is a display; run alternate code
        if (circuit.GetType() == typeof(Display))
        {
            Display display = (Display)circuit;
            GameObject displayObj = Instantiate(displayRef);
            DisplayReference displayVals = displayObj.GetComponent<DisplayReference>();
            Circuit.Input[] inputs = display.Inputs;

            displayObj.name = display.CircuitName;
            displayObj.transform.position = new Vector3(startingPosition.x, GridMaintenance.Instance.GridHeight, startingPosition.y);
            display.PhysicalObject = displayObj;
            display.Pins = displayVals.Pins;
            display.PreviewPins = displayVals.PreviewPins;

            for (int i = 0; i < 8; i++)
            {
                GameObject currentInput = displayVals.Inputs[i];
                InputReference inputReference = currentInput.AddComponent<InputReference>();

                inputReference.Input = inputs[i];
                inputs[i].Transform = currentInput.transform;
                inputs[i].StatusRenderer = displayVals.InputStatuses[i];
            }

            Destroy(displayVals); // Reference script no longer needed after extracing relevant values

            CircuitReference circuitRef = displayObj.AddComponent<CircuitReference>();

            circuitRef.Circuit = circuit;
            return;
        }

        // Setting dimensions
        int numInputMargins = circuit.Inputs.Length + 1, numOutputMargins = circuit.Outputs.Length + 1;
        float inputHeight = numInputMargins * heightMargins + circuit.Inputs.Length * inputSize;
        float outputHeight = numOutputMargins * heightMargins + circuit.Outputs.Length * outputSize;
        Vector2 dimensions = new Vector2(width, Mathf.Max(inputHeight, outputHeight));

        // Creating circuit base
        GameObject physicalObject = new GameObject("\"" + circuit.CircuitName + "\" Gate");
        GameObject baseQuad = new GameObject("Base");

        physicalObject.transform.position = new Vector3(startingPosition.x, GridMaintenance.Instance.GridHeight, startingPosition.y);
        baseQuad.layer = 8;
        baseQuad.transform.parent = physicalObject.transform;
        baseQuad.transform.localPosition = Vector3.up * 0.005f;

        Vector3[] vertices = new Vector3[]
{
            new Vector3(-dimensions.x / 2, 0, -dimensions.y / 2),
            new Vector3(-dimensions.x / 2, 0, dimensions.y / 2),
            new Vector3(dimensions.x / 2, 0, dimensions.y / 2),
            new Vector3(dimensions.x / 2, 0, -dimensions.y / 2)
};

        CreateQuad(baseQuad, vertices, baseMaterial);

        // Creating circuit border
        GameObject borderQuad = new GameObject("Border");

        borderQuad.layer = 13;
        borderQuad.transform.parent = physicalObject.transform;
        //borderQuad.transform.localPosition = Vector3.zero;
        vertices = new Vector3[]
        {
            new Vector3(-dimensions.x / 2 - borderThickness, 0, -dimensions.y / 2 - borderThickness),
            new Vector3(-dimensions.x / 2 - borderThickness, 0, dimensions.y / 2 + borderThickness),
            new Vector3(dimensions.x / 2 + borderThickness, 0, dimensions.y / 2 + borderThickness),
            new Vector3(dimensions.x / 2 + borderThickness, 0, -dimensions.y / 2 - borderThickness)
        };
        CreateQuad(borderQuad, vertices, borderMaterial, false);

        // Power on/off vertices
        Vector3[] powerVertices = new Vector3[]
        {
            new Vector3(-powerSize / 2, 0, -powerSize / 2),
            new Vector3(-powerSize / 2, 0, powerSize / 2),
            new Vector3(powerSize / 2, 0, powerSize / 2),
            new Vector3(powerSize / 2, 0, -powerSize / 2)
        };

        // Creating input nodes
        float inputStepSize = (dimensions.y - circuit.Inputs.Length * inputSize) / numInputMargins;
        int index = 0;

        vertices = new Vector3[]
        {
                new Vector3(-inputSize / 2, 0, -inputSize / 2),
                new Vector3(-inputSize / 2, 0, inputSize / 2),
                new Vector3(inputSize / 2, 0, inputSize / 2),
                new Vector3(inputSize / 2, 0, -inputSize / 2)
        };

        for (float currentHeight = inputStepSize + inputSize / 2; index < circuit.Inputs.Length; currentHeight += inputStepSize + inputSize)
        {
            GameObject inputQuad = new GameObject("Input " + (index + 1));
            GameObject inputQuadPower = new GameObject("Input Status " + (index + 1));
            Vector3 pos = new Vector3(-dimensions.x / 2, 0.01f, currentHeight - dimensions.y / 2);

            inputQuad.layer = 9;
            inputQuad.transform.parent = inputQuadPower.transform.parent = physicalObject.transform;
            inputQuad.transform.localPosition = inputQuadPower.transform.localPosition = pos;
            CreateQuad(inputQuad, vertices, inputMaterial);
            CreateQuad(inputQuadPower, powerVertices, powerOffMaterial, false);
            circuit.Inputs[index].Transform = inputQuad.transform;
            circuit.Inputs[index].StatusRenderer = inputQuadPower.GetComponent<MeshRenderer>();

            InputReference inputReference = inputQuad.AddComponent<InputReference>();

            inputReference.Input = circuit.Inputs[index];
            index++;
        }

        // Creating output nodes
        float outputStepSize = (dimensions.y - circuit.Outputs.Length * outputSize) / numOutputMargins;

        index = 0;
        vertices = new Vector3[]
        {
                new Vector3(-outputSize / 2, 0, -outputSize / 2),
                new Vector3(-outputSize / 2, 0, outputSize / 2),
                new Vector3(outputSize / 2, 0, outputSize / 2),
                new Vector3(outputSize / 2, 0, -outputSize / 2)
        };

        for (float currentHeight = outputStepSize + outputSize / 2; index < circuit.Outputs.Length; currentHeight += outputStepSize + outputSize)
        {
            GameObject outputQuad = new GameObject("Output " + (index + 1));
            GameObject outputQuadPower = new GameObject("Output Status " + (index + 1));
            Vector3 pos = new Vector3(dimensions.x / 2, 0.01f, currentHeight - dimensions.y / 2);

            outputQuad.layer = 10;
            outputQuad.transform.parent = outputQuadPower.transform.parent = physicalObject.transform;
            outputQuad.transform.localPosition = outputQuadPower.transform.localPosition = pos;
            CreateQuad(outputQuad, vertices, outputMaterial);
            CreateQuad(outputQuadPower, powerVertices, powerOffMaterial, false);
            circuit.Outputs[index].Transform = outputQuad.transform;
            circuit.Outputs[index].StatusRenderer = outputQuadPower.GetComponent<MeshRenderer>();

            OutputReference outputReference = outputQuad.AddComponent<OutputReference>();

            outputReference.Output = circuit.Outputs[index];
            index++;
        }

        // Adding text component
        GameObject name = new GameObject("Name");

        name.transform.parent = physicalObject.transform;
        name.transform.localPosition = Vector3.up * 0.01f + Vector3.right * (inputSize - outputSize) / 4;
        name.transform.eulerAngles = Vector3.right * 90;

        Vector2 nameDimensions = new Vector2(dimensions.x - (inputSize + outputSize) / 2 - 2 * textPadding.x, dimensions.y - 2 * textPadding.y);
        TextMeshPro text = name.AddComponent<TextMeshPro>();

        text.text = circuit.CircuitName;
        text.rectTransform.sizeDelta = nameDimensions;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 0;
        text.font = font;
        text.color = startingCircuitColor;

        circuit.PhysicalObject = physicalObject; // Connects new game object to its circuit for future access

        CircuitReference circuitReference = physicalObject.AddComponent<CircuitReference>();

        circuitReference.Circuit = circuit;
        circuit.Update();
    }

    private void CreateQuad(GameObject quad, Vector3[] vertices, Material material)
    {
        CreateQuad(quad, vertices, material, true);
    }

    private void CreateQuad(GameObject quad, Vector3[] vertices, Material material, bool addMeshCollider)
    {
        Mesh mesh = new Mesh();
        MeshFilter meshFilter = quad.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = quad.AddComponent<MeshRenderer>();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals;
        meshFilter.mesh = mesh;
        meshRenderer.material = material;
        if (addMeshCollider) quad.AddComponent<MeshCollider>();
    }

    // Getter methods
    public static CircuitVisualizer Instance { get { return instance; } }

    public Material PowerOffMaterial { get { return powerOffMaterial; } }

    public Material PowerOnMaterial { get { return powerOnMaterial;  } }
}