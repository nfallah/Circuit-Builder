using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private static MenuManager instance;

    private bool[] isCreated = new bool[3];

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("MenuManager instance already established; terminating.");
        }

        instance = this;
    }

    public void OpenScene(int sceneIndex)
    {
        bool isCreated = this.isCreated[sceneIndex];

        if (isCreated) ImportScene(sceneIndex); else CreateScene(sceneIndex);
    }

    private void CreateScene(int sceneIndex)
    {
        isCreated[sceneIndex] = true;
        SceneManager.LoadScene(1);
    }

    private void ImportScene(int sceneIndex)
    {
        SceneManager.LoadScene(1);
    }

    public static MenuManager Instance { get { return instance; } }
}