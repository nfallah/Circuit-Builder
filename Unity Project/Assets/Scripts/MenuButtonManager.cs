using UnityEngine;

public class MenuButtonManager : MonoBehaviour
{
    public void OpenScene(int sceneIndex)
    {
        MenuManager.Instance.OpenScene(sceneIndex);
    }
}