using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private static CursorManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("CircuitManager instance already established; terminating.");
        }

        instance = this;
    }

    [SerializeField ] Texture2D cursorTexture, linkTexture;

    public static void SetMouseTexture(bool useCursorTexture)
    {
        if (useCursorTexture)
        {
            Cursor.SetCursor(instance.cursorTexture, Vector2.zero, CursorMode.Auto);
        }

        else
        {
            Cursor.SetCursor(instance.linkTexture, new Vector2(26, 0), CursorMode.Auto);
        }
    }
}
