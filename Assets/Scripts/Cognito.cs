//This project was built upon the ServerlessAnalytics.unity package provided by Amazon for Unity
//Github: https://github.com/aws-samples/serverless-games-on-aws/tree/master/Serverless%20Data%20Analytics%20Lab

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using Michsky.UI.ModernUIPack;

public class Cognito : MonoBehaviour
{
    // UI Buttons & Input Field
    public Button LoginButton;
    public Button SignupButton;
    public TMP_InputField EmailField;
    public TMP_InputField SignupPasswordField;
    public TMP_InputField SignupUsernameField;
    public TMP_InputField LoginPasswordField;
    public TMP_InputField LoginUsernameField;
    public TMP_InputField consoleInputField;
    public Button doorButton;
    public GameObject doorSlider;
    public Button windowButton;
    public GameObject windowSlider;
    public SceneManager sceneManager;
    public TextMeshProUGUI connectionStatus;
    public TMP_InputField ultrasonicSensor;
    public TMP_InputField PIRSensor;
    public TMP_InputField waterSensor;
    public TMP_InputField LDRSensor;
    public TextMeshProUGUI micSensor;

    // Tokens
    private bool waitForToken = false;
    private string identityToken = string.Empty;
    private string jwt = string.Empty;
    private string refreshToken = string.Empty;
    private int frames = 0;

    bool loginSuccessful;

    // Create an Identity Provider
    AmazonCognitoIdentityProviderClient provider = new AmazonCognitoIdentityProviderClient
        ( new Amazon.Runtime.AnonymousAWSCredentials(), CredentialsManager.region );

    // Start is called before the first frame update
    private void Start()
    {
        LoadPreviousSession();

        LoginButton.onClick.AddListener(Login);
        SignupButton.onClick.AddListener(Signup);

        loginSuccessful = false;
    }

    
    private void Update()
    {
        if (frames < 1000)
        {
            frames++;
            return;
        }
        frames = 0;

        if(identityToken.Length != 0)
        {
            CallLambdaFunc();
        }
    }

    private void LoadPreviousSession()
    {
        refreshToken = PlayerPrefs.GetString("RefreshToken");
        Debug.Log("Saved Refresh Token: " + refreshToken);

        StartCoroutine(PostIndentityTokenRequest());
    }

    private IEnumerator PostIndentityTokenRequest()
    {
        WWWForm form = new WWWForm();
        form.AddField("grant_type", "refresh_token");
        form.AddField("client_id", CredentialsManager.appClientId);
        form.AddField("refresh_token", refreshToken);

        UnityWebRequest www = UnityWebRequest.Post("https://esp-test.auth.us-east-2.amazoncognito.com/oauth2/token", form);
        www.SetRequestHeader("content-type", "application/x-www-form-urlencoded");

        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            JSONObject data = new JSONObject(www.downloadHandler.text);
            ParseIdentityToken(data);
            CallLambdaFunc();
        }
    }

    private void ParseIdentityToken(JSONObject jsonObject)
    {
        Dictionary<string, string> dict = jsonObject.ToDictionary();
        foreach (KeyValuePair<string, string> kvp in dict)
        {
            if (kvp.Key == "id_token")
            {
                identityToken = kvp.Value;
                Debug.Log("Identity Token: " + identityToken);
            }
        }
    }

    public void Login()
    {
        //Reset tokens
        identityToken = "";
        jwt = "";
        refreshToken = "";
        PlayerPrefs.SetString("RefreshToken", refreshToken);

        _ = Login_User();

        while (!waitForToken)
        {
            StartCoroutine(WaitForTokens());
        }

        PlayerPrefs.SetString("RefreshToken", refreshToken);
        CallLambdaFunc();
    }

    public void Signup()
    {
        _ = Signup_Method_Async();
    }

    public void Logout()
    {
        //Reset Tokens
        identityToken = "";
        jwt = "";
        refreshToken = "";
        PlayerPrefs.SetString("RefreshToken", refreshToken);

        //Display Status
        connectionStatus.SetText("Connection: <color=white>Inactive</color>");
    }

    private IEnumerator WaitForTokens()
    {
        while (!waitForToken)
        {
            if (identityToken.Length == 0 || jwt.Length == 0 || refreshToken.Length == 0)
            {
                yield return new WaitForSeconds(1);
            }
            else
            {
                waitForToken = true;
            }
        }
    }

    //Method that creates a new Cognito user
    private async Task Signup_Method_Async()
    {
        string userName = SignupUsernameField.text;
        string passWord = SignupPasswordField.text;
        string email = EmailField.text;

        SignUpRequest signUpRequest = new SignUpRequest()
        {
            ClientId = CredentialsManager.appClientId,
            Username = userName,
            Password = passWord
        };

        List<AttributeType> attributes = new List<AttributeType>()
        {
            new AttributeType(){Name = "email", Value = email}
        };

        signUpRequest.UserAttributes = attributes;

        try
        {
            SignUpResponse request = await provider.SignUpAsync(signUpRequest);
            Debug.Log("Sign up worked");

            // Send Login Event
            Events.Call_Signup();
        }
        catch (Exception e)
        {
            Debug.Log("Exception: " + e);
            return;
        }
    }

    //Method that signs in Cognito user 
    private async Task Login_User()
    {
        string userName = LoginUsernameField.text;
        string passWord = LoginPasswordField.text;

        CognitoUserPool userPool = new CognitoUserPool(CredentialsManager.userPoolId, CredentialsManager.appClientId, provider);
        CognitoUser user = new CognitoUser(userName, CredentialsManager.appClientId, userPool, provider);

        InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
        {
            Password = passWord
        };

        try
        {
            AuthFlowResponse authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

            GetUserRequest getUserRequest = new GetUserRequest();
            getUserRequest.AccessToken = authResponse.AuthenticationResult.AccessToken;

            identityToken = authResponse.AuthenticationResult.IdToken;
            Debug.Log("Identity Token: " + identityToken);

            jwt = getUserRequest.AccessToken;
            Debug.Log("Access Token: " + jwt);
             
            refreshToken = authResponse.AuthenticationResult.RefreshToken;
            Debug.Log("Refresh Token: " + refreshToken);

            // User is logged in
            loginSuccessful = true;
        }
        catch (Exception e)
        {
            Debug.Log("Exception: " + e);
            return;
        }

        if (loginSuccessful == true)
        {
            string subId = await Get_User_Id();
            CredentialsManager.userid = subId;

            // Send Login Events
            Events.Call_Login();

            // Print UserID
            Debug.Log("Response - User's Sub ID from Cognito: " + CredentialsManager.userid);
        }
    }

    private void CallLambdaFunc()
    {
        StartCoroutine(GetRequest());
    }

    private IEnumerator GetRequest()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://igo7oiugh7.execute-api.us-east-2.amazonaws.com/fetch_data/get-data");
        www.SetRequestHeader("content-type", "application/json");
        www.SetRequestHeader("Authorization", identityToken);
        byte[] bodyRaw = Encoding.UTF8.GetBytes("{\"value\":0}");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);

        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            JSONObject data = new JSONObject(www.downloadHandler.text);
            ParseSensorData(data["body"]);
            connectionStatus.SetText("Connection: <color=orange>Active</color>");
        }
    }

    private void ParseSensorData(JSONObject jsonObject)
    {
        string ultrasonic = string.Empty;
        int PIR = -1;
        string water = string.Empty;
        string LDR = string.Empty;
        int mic = -1;

        foreach(var data in jsonObject.list)
        {
            Dictionary<string, string> dict = data.ToDictionary();
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                if (kvp.Key == "ultrasonic")
                {
                    if(kvp.Value.Length == 0)
                    {
                        continue;
                    }

                    ultrasonic = kvp.Value;
                }
                else if (kvp.Key == "PIR")
                {
                    if (kvp.Value.Length == 0)
                    {
                        continue;
                    }

                    PIR = GetInt(kvp.Value, 0);
                }
                else if (kvp.Key == "water")
                {
                    if (kvp.Value.Length == 0)
                    {
                        continue;
                    }

                    water = kvp.Value;
                }
                else if (kvp.Key == "ldr")
                {
                    if (kvp.Value.Length == 0)
                    {
                        continue;
                    }

                    LDR = kvp.Value;
                }
                else if (kvp.Key == "mic")
                {
                    if (kvp.Value.Length == 0)
                    {
                        continue;
                    }

                    mic = GetInt(kvp.Value, 0);
                }
            }
        }

        if (ultrasonic != string.Empty)
        {
            ultrasonicSensor.SetTextWithoutNotify(ultrasonic.ToString());
        }

        if (PIR != -1)
        {
            PIRSensor.SetTextWithoutNotify(PIR.ToString());
        }

        if (water != string.Empty)
        {
            waterSensor.SetTextWithoutNotify(water.ToString());
        }

        if (LDR != string.Empty)
        {
            LDRSensor.SetTextWithoutNotify(LDR.ToString());
        }

        if (mic == 0)
        {
            micSensor.SetText("Noise Level Threshold\n" +
                "Status: <color=orange>Not Triggered</color>");
        }
        else if (mic == 1)
        {
            micSensor.SetText("Noise Level Threshold\n" +
                "Status: <color=orange>Triggered</color>");
        }
    }

    private int GetInt(string stringValue, int defaultValue)
    {
        int result = defaultValue;
        int.TryParse(stringValue, out result);
        return result;
    }

    // Gets a User's sub UUID from Cognito
    private async Task<string> Get_User_Id()
    {
        Debug.Log("Getting user's id...");

        string subId = "";

        Task<GetUserResponse> responseTask =
            provider.GetUserAsync(new GetUserRequest
            {
                AccessToken = jwt
            });

        GetUserResponse responseObject = await responseTask;

        // Set User ID
        foreach (var attribute in responseObject.UserAttributes)
        {
            if (attribute.Name == "sub")
            {
                subId = attribute.Value;
                break;
            }
        }

        return subId;
    }

    public void ToggleLight()
    {
        ToggleValue("LED", ref sceneManager.light, null);
    }

    public void ToggleDoor()
    {
        RadialSlider slider = doorSlider.GetComponent<RadialSlider>();
        ToggleValue("door", ref sceneManager.door, slider);
    }

    public void SetDoorValue(float value)
    {
        RadialSlider slider = doorSlider.GetComponent<RadialSlider>();
        sceneManager.door = value / slider.maxValue;

        SwitchManager manager = doorButton.gameObject.GetComponent<SwitchManager>();
        if (sceneManager.door == 0)
        {
            if (manager.isOn)
            {
                manager.AnimateSwitch();
            }
        }
        else
        {
            if (!manager.isOn)
            {
                manager.AnimateSwitch();
            }
        }

        SetValue("door", sceneManager.door);
    }

    public void ToggleWindow()
    {
        RadialSlider slider = windowSlider.GetComponent<RadialSlider>();
        ToggleValue("window", ref sceneManager.window, slider);
    }

    public void SetWindowValue(float value)
    {
        RadialSlider slider = windowSlider.GetComponent<RadialSlider>();
        sceneManager.window = value / slider.maxValue;

        SwitchManager manager = windowButton.gameObject.GetComponent<SwitchManager>();
        if (sceneManager.window == 0)
        {
            if (manager.isOn)
            {
                manager.AnimateSwitch();
            }
        }
        else
        {
            if (!manager.isOn)
            {
                manager.AnimateSwitch();
            }
        }
        SetValue("window", sceneManager.window);
    }

    private void ToggleValue(string name, ref float value, RadialSlider slider)
    {
        if (value > 0.0f)
        {
            value = 0.0f;
        }
        else
        {
            value = 1.0f;
        }

        if (slider != null)
        {
            slider.SliderValue = slider.maxValue * value;
            slider.UpdateUI();
        }

        SendFloat(name, value);
    }

    private void SetValue(string name, float value)
    {
        SendFloat(name, value);
    }

    private void SendFloat(string name, float value)
    {
        SendString(name, value.ToString());
    }

    private void SendString(string name, string data)
    {
        DateTime timestamp = DateTime.Now;
        string text = "{\"sensor_name\":\"" + name + "\",\"value\":\"" + data + "\",\"timestamp\":\"" + timestamp.ToShortDateString() + " " + timestamp.ToLongTimeString() + "\"}";
        Debug.Log(text);
        StartCoroutine(PostRequest(text));
    }

    private IEnumerator PostRequest(string text)
    {
        UnityWebRequest www = UnityWebRequest.Post("https://igo7oiugh7.execute-api.us-east-2.amazonaws.com/test/helloworld", "");
        www.SetRequestHeader("content-type", "application/json");
        www.SetRequestHeader("Authorization", identityToken);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(text);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);

        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            Debug.Log("Message Sent");
        }
    }
}