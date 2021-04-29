using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollView : MonoBehaviour
{
    public Scrollbar scrollbar;
    public Transform panels;

    public void Update_Value()
    {
        if(scrollbar.size < 1.0f)
        {
            scrollbar.value = 1.0f;
        }
    }
}