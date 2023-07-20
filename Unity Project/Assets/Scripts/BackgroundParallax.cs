using System;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    private static BackgroundParallax instance;

    [SerializeField] float parallaxStrength;

    [SerializeField] RectTransform backgroundTransform;

    private Vector2 mousePos;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("BackgroundParallax instance already established; terminating.");
        }

        instance = this;
    }

    private void Start()
    {
        mousePos = Input.mousePosition;
    }

    private void Update()
    {
        Vector2 prevMousePos = mousePos, mouseDelta;

        mousePos = Input.mousePosition;
        mouseDelta = (prevMousePos - mousePos) * parallaxStrength;
        backgroundTransform.offsetMin += mouseDelta;
    }

    // Single state reference
    public static BackgroundParallax Instance { get { return instance; } }
}