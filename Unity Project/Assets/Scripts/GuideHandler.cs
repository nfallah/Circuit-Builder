using System;
using UnityEngine;
using UnityEngine.UI;

public class GuideHandler : MonoBehaviour
{
    [SerializeField] Color selectedTabColor, unselectedTabColor;

    [SerializeField] Image welcomeWindow, logicGatesWindow, controlsWindow;

    [SerializeField] GameObject welcomeTabsParent, logicGatesTabsParent, controlsTabsParent;

    [SerializeField] Image[] welcomeTabs, logicGatesTabs, controlsTabs;

    [SerializeField] GameObject[] welcomeTabsViews, logicGatesTabsViews, controlsTabsViews;

    private Image currentWindow;

    private int currentWelcomeTab = 0, currentLogicGatesTab = 0, currentControlsTab = 0;

    private void Start()
    {
        currentWindow = welcomeWindow;
    }

    public void UpdateTab(int newTab)
    {
        Image currentButton, newButton;
        GameObject currentView, newView;

        if (currentWindow == welcomeWindow)
        {
            if (newTab == currentWelcomeTab) return;

            currentButton = welcomeTabs[currentWelcomeTab];
            newButton = welcomeTabs[newTab];
            currentView = welcomeTabsViews[currentWelcomeTab];
            newView = welcomeTabsViews[newTab];
            currentWelcomeTab = newTab;
        }

        else if (currentWindow == logicGatesWindow)
        {
            if (newTab == currentLogicGatesTab) return;

            currentButton = logicGatesTabs[currentLogicGatesTab];
            newButton = logicGatesTabs[newTab];
            currentView = logicGatesTabsViews[currentLogicGatesTab];
            newView = logicGatesTabsViews[newTab];
            currentLogicGatesTab = newTab;
        }

        else if (currentWindow == controlsWindow)
        {
            if (newTab == currentControlsTab) return;

            currentButton = controlsTabs[currentControlsTab];
            newButton = controlsTabs[newTab];
            currentView = controlsTabsViews[currentControlsTab];
            newView = controlsTabsViews[newTab];
            currentControlsTab = newTab;
        }

        else throw new Exception("Invalid current window.");

        currentButton.color = unselectedTabColor;
        newButton.color = selectedTabColor;
        currentView.SetActive(false);
        newView.SetActive(true);
    }

    public void UpdateWindow(Image newWindow)
    {
        if (currentWindow == newWindow) return;

        currentWindow.color = unselectedTabColor;
        newWindow.color = selectedTabColor;

        if (currentWindow == welcomeWindow) welcomeTabsParent.SetActive(false);

        else if (currentWindow == logicGatesWindow) logicGatesTabsParent.SetActive(false);

        else if (currentWindow == controlsWindow) controlsTabsParent.SetActive(false);

        else throw new Exception("Invalid current window.");

        if (newWindow == welcomeWindow) welcomeTabsParent.SetActive(true);

        else if (newWindow == logicGatesWindow) logicGatesTabsParent.SetActive(true);

        else if (newWindow == controlsWindow) controlsTabsParent.SetActive(true);

        else throw new Exception("Invalid new window.");

        currentWindow = newWindow;
    }
}