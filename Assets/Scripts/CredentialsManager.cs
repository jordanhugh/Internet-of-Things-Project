using System;
using UnityEngine;
using Amazon;
using Amazon.CognitoIdentity;

public class CredentialsManager
{

    // Region - A game may need multiple region endpoints if services
    // are in multiple regions or different per service
    public static RegionEndpoint region = RegionEndpoint.USEast2;

    // Cognito Credentials Variables
    public const string identityPool = "us-east-2:0624d115-c7a3-432f-a148-68b22ea8d9c3";
    public static string userPoolId = "us-east-2_Ka7jwyuym";
    public static string appClientId = "4jkes0gi9qu26vr2c7k0hc2sgr";

    //Personal Cloud for Debugging
    //public static RegionEndpoint region = RegionEndpoint.EUWest1;
    //public const string identityPool = "eu-west-1:302dfcbd-5b5c-4c89-ba13-f41916a3c39a";
    //public static string userPoolId = "eu-west-1_sWqGfMbzV";
    //public static string appClientId = "3cj5s021s5ks8v8dj542aueg2q";

    // Initialize the Amazon Cognito credentials provider
    public static CognitoAWSCredentials credentials = new CognitoAWSCredentials(
        identityPool, region
    );

    // User's Cognito ID once logged in becomes set here
    public static string userid = "";
}