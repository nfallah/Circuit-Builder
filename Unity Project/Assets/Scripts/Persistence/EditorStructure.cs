using UnityEngine;

public class EditorStructure
{
    private string name;

    public EditorStructure(string name)
    {
        this.name = name;
    }

    public string Name { get { return name; } }
}