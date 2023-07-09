using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLogicManager : MonoBehaviour
{
    private static MenuLogicManager instance;

    private bool firstOpen;

    private int currentSceneIndex;

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
        bool isCreated = MenuSetupManager.Instance.EditorStructures[sceneIndex] != null;

        firstOpen = !isCreated;

        if (isCreated) ImportScene(sceneIndex); else MenuInterfaceManager.Instance.BeginSceneNameSubmission(sceneIndex);
    }

    public void CreateScene(int sceneIndex, string name)
    {
        MenuSetupManager.Instance.EditorStructures[sceneIndex] = new EditorStructure(name);
        ImportScene(sceneIndex);
    }

    private void ImportScene(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;
        SceneManager.LoadScene(1);
    }

    // Singleton state reference
    public static MenuLogicManager Instance { get { return instance; } }

    // Getter methods
    public bool FirstOpen { get { return firstOpen; } }

    public int CurrentSceneIndex { get { return currentSceneIndex; } }
}