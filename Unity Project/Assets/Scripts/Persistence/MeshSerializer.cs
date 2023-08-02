using System;
using UnityEngine;

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

    public MeshSerializer(Mesh mesh, Transform parentTransform)
    {
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        position = parentTransform.position;
        rotation = parentTransform.eulerAngles;
        scale = parentTransform.localScale;
        uv = mesh.uv;
        normals = mesh.normals;
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