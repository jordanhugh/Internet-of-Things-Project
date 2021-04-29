using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

//Need to usr this class to store the datetime of each actuator

public class SceneManager : MonoBehaviour
{
    public float light = 0.0f;
    public float door = 0.0f;
    public float window = 0.0f;

    public string refreshToken;
}