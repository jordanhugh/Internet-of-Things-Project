using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Events 
{
    // General Values
    private static string platform = "UnityEditor";
    private static string country = "UNITED STATES";

    // General 
    public static void Call_App_Load()
    {
        Debug.Log("App Loaded");
        EventsManager.Create_App_Load_Event(platform);
    }

    public static void Call_Login()
    {
        Debug.Log("User logged in");
        EventsManager.Create_Login_Event(platform);
    }

    public static void Call_Signup()
    {
        Debug.Log("User signed-up in");
        EventsManager.Create_Signup_Event(country, platform);
    }
}