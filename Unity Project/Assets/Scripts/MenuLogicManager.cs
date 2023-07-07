using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLogicManager : MonoBehaviour
{
    private static MenuLogicManager instance;

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

        if (isCreated) ImportScene(sceneIndex); else MenuInterfaceManager.Instance.BeginSceneNameSubmission(sceneIndex);
    }

    public void CreateScene(int sceneIndex, string name)
    {
        MenuSetupManager.Instance.EditorStructures[sceneIndex] = new EditorStructure(name);
        SceneManager.LoadScene(1);
    }

    private void ImportScene(int sceneIndex)
    {
        SceneManager.LoadScene(1);
    }

    public static MenuLogicManager Instance { get { return instance; } }
}