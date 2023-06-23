using System;
using TMPro;
using UnityEngine;

public class CircuitVisualizer : MonoBehaviour
{
    private static CircuitVisualizer instance; // Ensures a singleton state pattern is maintained

    [SerializeField] float
    borderThickness, // Border surrounding the base of the circuit
    inputSize, // Square dimensions of an input node
    outputSize, // Square dimensions of an output node
    heightMargins, // The distance between each input and output node
    width; // Constant referring to the width of the circuit

    // Represents custom materials for the circuit base & I/O
    [SerializeField] Material baseMaterial, borderMaterial, inputMaterial, outputMaterial;

    [SerializeField] Vector2 textPadding;

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

    private void Start()
    {
        VisualizeCircuit(new AndGate());    
    }

    // Generates a mesh corresponding to the name & inputs/outputs of a given circuit
    public void VisualizeCircuit(Circuit circuit)
    {
        // Setting dimensions
        int numInputMargins = circuit.Inputs.Length + 1, numOutputMargins = circuit.Outputs.Length + 1;
        float inputHeight = numInputMargins * heightMargins + circuit.Inputs.Length * inputSize;
        float outputHeight = numOutputMargins * heightMargins + circuit.Outputs.Length * outputSize;
        Vector2 dimensions = new Vector2(width, Mathf.Max(inputHeight, outputHeight));

        // Creating circuit base
        GameObject physicalObject = new GameObject("\"" + circuit.CircuitName + "\" Gate");
        GameObject baseQuad = new GameObject("Base");

        baseQuad.transform.parent = physicalObject.transform;
        baseQuad.transform.localPosition = Vector3.zero;

        Mesh mesh = new Mesh();
        MeshFilter meshFilter = baseQuad.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = baseQuad.AddComponent<MeshRenderer>();

        mesh.vertices = new Vector3[]
        {
            new Vector3(-dimensions.x / 2, 0, -dimensions.y / 2),
            new Vector3(-dimensions.x / 2, 0, dimensions.y / 2),
            new Vector3(dimensions.x / 2, 0, dimensions.y / 2),
            new Vector3(dimensions.x / 2, 0, -dimensions.y / 2)
        };
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals;
        meshFilter.mesh = mesh;
        meshRenderer.material = baseMaterial;
        baseQuad.AddComponent<MeshCollider>();

        // Creating circuit border
        GameObject borderQuad = new GameObject("Border");

        borderQuad.transform.parent = physicalObject.transform;
        borderQuad.transform.localPosition = Vector3.down * 0.01f;

        mesh = new Mesh();
        meshFilter = borderQuad.AddComponent<MeshFilter>();
        meshRenderer = borderQuad.AddComponent<MeshRenderer>();

        mesh.vertices = new Vector3[]
        {
            new Vector3(-dimensions.x / 2 - borderThickness, 0, -dimensions.y / 2 - borderThickness),
            new Vector3(-dimensions.x / 2 - borderThickness, 0, dimensions.y / 2 + borderThickness),
            new Vector3(dimensions.x / 2 + borderThickness, 0, dimensions.y / 2 + borderThickness),
            new Vector3(dimensions.x / 2 + borderThickness, 0, -dimensions.y / 2 - borderThickness)
        };
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals;
        meshFilter.mesh = mesh;
        meshRenderer.material = borderMaterial;
        borderQuad.AddComponent<MeshCollider>();

        // Creating input nodes
        float inputStepSize = (dimensions.y - circuit.Inputs.Length * inputSize) / numInputMargins;
        int index = 0;

        for (float currentHeight = inputStepSize + inputSize / 2; index < circuit.Inputs.Length; currentHeight += inputStepSize + inputSize)
        {
            GameObject inputQuad = new GameObject("Input " + (index + 1));

            inputQuad.transform.parent = physicalObject.transform;
            inputQuad.transform.localPosition = new Vector3(-dimensions.x / 2, 0.01f, currentHeight - dimensions.y / 2);
            mesh = new Mesh();
            meshFilter = inputQuad.AddComponent<MeshFilter>();
            meshRenderer = inputQuad.AddComponent<MeshRenderer>();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-inputSize / 2, 0, -inputSize / 2),
                new Vector3(-inputSize / 2, 0, inputSize / 2),
                new Vector3(inputSize / 2, 0, inputSize / 2),
                new Vector3(inputSize / 2, 0, -inputSize / 2)
            };
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.normals = normals;
            meshFilter.mesh = mesh;
            meshRenderer.material = inputMaterial;
            inputQuad.AddComponent<MeshCollider>();
            index++;
        }

        // Creating output nodes
        float outputStepSize = (dimensions.y - circuit.Outputs.Length * outputSize) / numOutputMargins;
        index = 0;

        for (float currentHeight = outputStepSize + outputSize / 2; index < circuit.Outputs.Length; currentHeight += outputStepSize + outputSize)
        {
            GameObject outputQuad = new GameObject("Output " + (index + 1));

            outputQuad.transform.parent = physicalObject.transform;
            outputQuad.transform.localPosition = new Vector3(dimensions.x / 2, 0.01f, currentHeight - dimensions.y / 2);
            mesh = new Mesh();
            meshFilter = outputQuad.AddComponent<MeshFilter>();
            meshRenderer = outputQuad.AddComponent<MeshRenderer>();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-outputSize / 2, 0, -outputSize / 2),
                new Vector3(-outputSize / 2, 0, outputSize / 2),
                new Vector3(outputSize / 2, 0, outputSize / 2),
                new Vector3(outputSize / 2, 0, -outputSize / 2)
            };
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.normals = normals;
            meshFilter.mesh = mesh;
            meshRenderer.material = outputMaterial;
            outputQuad.AddComponent<MeshCollider>();
            index++;
        }

        // Adding text component
        GameObject name = new GameObject("Name");

        name.transform.parent = physicalObject.transform;
        name.transform.localPosition = Vector3.right * (inputSize - outputSize) / 4 + Vector3.up * 0.01f;
        name.transform.eulerAngles = Vector3.right * 90;

        Vector2 nameDimensions = new Vector2(dimensions.x - (inputSize + outputSize) / 2 - 2 * textPadding.x, dimensions.y - 2 * textPadding.y);
        TextMeshPro text = name.AddComponent<TextMeshPro>();

        text.text = circuit.CircuitName;
        text.rectTransform.sizeDelta = nameDimensions;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 0;

        circuit.PhysicalObject = physicalObject; // Connects new game object to its circuit for future access
    }

    // Getter method
    public static CircuitVisualizer Instance { get { return instance; } }
}