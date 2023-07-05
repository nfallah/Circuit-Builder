using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;

    private float fps;

    private void Update()
    {
        fps = (int)(Time.timeScale / Time.smoothDeltaTime);
        fpsText.text = "FPS: " + fps;
    }
}