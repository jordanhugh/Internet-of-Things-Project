using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    private void ToggleActive(GameObject go)
    {
        go.SetActive(!go.activeSelf);
    }

    public void GotIt(GameObject window)
    {
        ToggleActive(window);
    }

    public void Cancel(GameObject window)
    {
        ToggleActive(window);
    }

    public void Settings(GameObject window)
    {
        window.transform.SetAsLastSibling();
        ToggleActive(window);
    }
}