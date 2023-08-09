using System;
using UnityEngine;

/// <summary>
/// CursorManager handles the switching of mouse textures within each scene.
/// </summary>
public class CursorManager : MonoBehaviour
{
    // Singleton state reference
    private static CursorManager instance;

    /// <summary>
    /// Texture that is utilized when the cursor is in use.
    /// </summary>
    [SerializeField]
    Texture2D cursorTexture;
    
    /// <summary>
    /// Texture that is utilized when the link is in use.
    /// </summary>
    [SerializeField]
    Texture2D linkTexture;

    /// <summary>
    /// The pixel position the cursor texture is centered around.
    /// </summary>
    private static Vector2 cursorPosition = Vector2.zero;

    /// <summary>
    /// The pixel position the link texture is centered around.
    /// </summary>
    private static Vector2 linkPosition = new Vector2(12, 0);

    // Enforces a singleton state pattern
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("CircuitManager instance already established; terminating.");
        }

        instance = this;
    }

    /// <summary>
    /// Sets a new mouse texture based on the specified value.
    /// </summary>
    /// <param name="useCursorTexture">Whether the cursor texture should be used.</param>
    public static void SetMouseTexture(bool useCursorTexture)
    {
        if (useCursorTexture) Cursor.SetCursor(instance.cursorTexture, cursorPosition, CursorMode.ForceSoftware);

        else Cursor.SetCursor(instance.linkTexture, linkPosition, CursorMode.ForceSoftware);
    }
}