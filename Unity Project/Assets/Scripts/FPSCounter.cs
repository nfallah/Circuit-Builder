using TMPro;
using UnityEngine;

/// <summary>
/// FPSCounter displays the current frames-per-second to an assigned text UI.
/// </summary>
public class FPSCounter : MonoBehaviour
{
    /// <summary>
    /// The text that displays the FPS.
    /// </summary>
    [SerializeField]
    TextMeshProUGUI fpsText;

    /// <summary>
    /// The frames-per-second.
    /// </summary>
    private float fps;
    
    // Calculates the FPS and writes it to fpsText.
    private void Update()
    {
        fps = (int)(Time.timeScale / Time.smoothDeltaTime);
        fpsText.text = "FPS: " + fps;
    }
}