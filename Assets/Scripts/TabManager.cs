using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabManager : MonoBehaviour
{
    private List<GameObject> panels;
    private static GameObject loginPanel;
    private static GameObject signUpPanel;

    void Start()
    {
        panels = new List<GameObject>(GameObject.FindGameObjectsWithTag("Panel"));
        foreach(GameObject panel in panels)
        {
            if(panel.name == "LoginPanel")
            {
                loginPanel = panel;
            }
            else if (panel.name == "SignUpPanel")
            {
                signUpPanel = panel;
            }
        }
    }

    public void SwitchPanel(GameObject currPanel)
    {
        GameObject currContent = currPanel.transform.Find("Content").gameObject;
        if (currContent.activeSelf)
        {
            return;
        }

        for (int i = 0; i < panels.Capacity; i++)
        {
            GameObject content = panels[i].transform.Find("Content").gameObject;
            if (content.activeSelf)
            {
                content.SetActive(false);
                currContent.SetActive(true);
            }
        }
    }

    public static void ShowSignUpPanel()
    {
        GameObject content = loginPanel.transform.Find("Content").gameObject;
        content.SetActive(false);
        content = signUpPanel.transform.Find("Content").gameObject;
        content.SetActive(true);
    }

    public static void ShowLoginPanel()
    {
        GameObject content = signUpPanel.transform.Find("Content").gameObject;
        content.SetActive(false);
        content = loginPanel.transform.Find("Content").gameObject;
        content.SetActive(true);  
    }
}