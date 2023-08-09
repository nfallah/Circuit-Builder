using System;
using UnityEngine;

/// <summary>
/// BackgroundParallax captures any mouse movement and proportionally moves an assigned background to emulate a parallax effect.
/// </summary>
public class BackgroundParallax : MonoBehaviour
{
    // Singleton state reference
    private static BackgroundParallax instance;

    /// <summary>
    /// Controls how much the captured mouse movement alters the background.
    /// </summary>
    [SerializeField]
    float parallaxStrength;

    /// <summary>
    /// The background that the parallax is applied on.
    /// </summary>
    [SerializeField]
    RectTransform backgroundTransform;

    /// <summary>
    /// Stores prior mouse positions to calculate the delta movement between frames.
    /// </summary>
    private Vector2 mousePos;

    // Enforces a singleton state pattern
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("BackgroundParallax instance already established; terminating.");
        }

        instance = this;
    }

    private void Start() { mousePos = Input.mousePosition; }

    private void Update()
    {
        // Captures the difference in mouse position between frames, proportionally moving the background.
        Vector2 prevMousePos = mousePos, mouseDelta;

        mousePos = Input.mousePosition;
        mouseDelta = (prevMousePos - mousePos) * parallaxStrength;
        backgroundTransform.offsetMin += mouseDelta;
    }

    // Getter method
    public static BackgroundParallax Instance { get { return instance; } }
}