using System;
using UnityEngine;

/// <summary>
/// MeshSerializer serves as a wrapper class for containing all relevant mesh and transform values for serialization and deserialization of wires.
/// </summary>
[Serializable]
public class MeshSerializer
{
    [SerializeField]
    int[] triangles;

    [SerializeField]
    Vector2[] uv;

    [SerializeField]
    Vector3 position, rotation, scale;

    [SerializeField]
    Vector3[] normals, vertices;

    /// <summary>
    /// Extracts mesh and transform values pertaining to a wire.
    /// </summary>
    /// <param name="mesh">The mesh to extract relevant values from.</param>
    /// <param name="parentTransform">The parent transform of the GameObject containing the mesh (could be itself).</param>
    public MeshSerializer(Mesh mesh, Transform parentTransform)
    {
        triangles = mesh.triangles;
        uv = mesh.uv;
        position = parentTransform.position;
        rotation = parentTransform.eulerAngles;
        scale = parentTransform.localScale;
        normals = mesh.normals;
        vertices = mesh.vertices;
    }

    // Getter methods
    public int[] Triangles { get { return triangles; } }

    public Vector2[] UV { get { return uv; } }

    public Vector3 Position { get { return position; } }

    public Vector3 Rotation { get { return rotation; } }

    public Vector3 Scale { get { return scale; } }
    
    public Vector3[] Normals { get { return normals; } }

    public Vector3[] Vertices { get { return vertices; } }
}