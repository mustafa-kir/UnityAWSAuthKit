using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using TMPro;
using UnityEngine;

public class CognitoSDKController1 : MonoBehaviour
{
    private static string idToken;
    public TMP_InputField signUpEmail;
    public TMP_InputField signUpPassword;
    public TMP_InputField signUpNickname;

    public TMP_InputField ConfirmSignUpEmail;
    public TMP_InputField ConfirmSignUpCode;

    public TMP_InputField signInEmail;
    public TMP_InputField signInPasword;

    public TMP_InputField recoverEmail;
    public TMP_InputField confirmRecoverEmail;
    public TMP_InputField confirmRecoverCode;
    public TMP_InputField confirmRecoverNewPasword;
    public TMP_Text nickname;


    private AmazonCognitoIdentityProviderClient _cognitoService;
    private string accessToken;

    private string refreshToken;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _cognitoService =
            new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.EUWest1);
    }


    public async void SignUp()
    {
        var succes = await SignUpAsync();
        print($"signu-up status : {succes}");
    }

    private async Task<bool> SignUpAsync()
    {
        var nicknameAttrs = new AttributeType
        {
            Name = "nickname",
            Value = signUpNickname.text
        };
        var localeAttrs = new AttributeType
        {
            Name = "locale",
            Value = "es_PE"
        };

        var userAttrsList = new List<AttributeType>();

        userAttrsList.Add(nicknameAttrs);
        userAttrsList.Add(localeAttrs);


        var signUpRequest = new SignUpRequest
        {
            UserAttributes = userAttrsList,
            Username = signUpEmail.text,
            ClientId = MyUtils.appClientID,
            Password = signUpPassword.text
        };

        var response = await _cognitoService.SignUpAsync(signUpRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async void ConfirmSignUp()
    {
        var succes = await ConfirmSignupAsync();
        print($"confirm signu-up status : {succes}");
    }

    public async Task<bool> ConfirmSignupAsync()
    {
        var signUpRequest = new ConfirmSignUpRequest
        {
            ClientId = MyUtils.appClientID,
            ConfirmationCode = ConfirmSignUpCode.text,
            Username = ConfirmSignUpEmail.text
        };

        var response = await _cognitoService.ConfirmSignUpAsync(signUpRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async void Login()
    {
        var success = await InitiateAuthAsync();
        print($"Login status : {success}");

        var succes = await GetUserAsync();
        print($"confirm signu-up status : {succes}");

        var CheckEmailSucces = await CheckEmailVerification();
        print($"check email status : {CheckEmailSucces}");
    }

    private async Task<bool> CheckEmailVerification()
    {
        // GetUserRequest ile kullanıcı bilgilerini al
        var getUserRequest = new GetUserRequest
        {
            AccessToken = accessToken // Login sırasında alınan access token kullanılıyor
        };

        var response = await _cognitoService.GetUserAsync(getUserRequest);

        // "email_verified" niteliğini bul
        var emailVerifiedAttr = response.UserAttributes.Find(attribute => attribute.Name == "email_verified");

        if (emailVerifiedAttr != null && emailVerifiedAttr.Value == "true")
        {
            Debug.Log("Email doğrulandı.");
            return true;
        }

        Debug.Log("Email doğrulanmadı. Kullanıcı emailini doğrulamalıdır.");
        return false;
    }

    private async Task<bool> InitiateAuthAsync()
    {
        var authParameters = new Dictionary<string, string>();
        authParameters.Add("USERNAME", signInEmail.text);
        authParameters.Add("PASSWORD", signInPasword.text);

        var authRequest = new InitiateAuthRequest

        {
            ClientId = MyUtils.appClientID,
            AuthParameters = authParameters,
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH
        };

        var response = await _cognitoService.InitiateAuthAsync(authRequest);

        refreshToken = response.AuthenticationResult.RefreshToken;
        accessToken = response.AuthenticationResult.AccessToken;
        idToken = response.AuthenticationResult.IdToken;
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> GetUserAsync()
    {
        var getUserRequest = new GetUserRequest
        {
            AccessToken = accessToken
        };

        var response = await _cognitoService.GetUserAsync(getUserRequest);
        var userNickname = response.UserAttributes.Find(attribute => attribute.Name == "nickname");

        nickname.text = $"Hi {userNickname.Value}";
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async void DeleteUser()
    {
        var succes = await DeleteUserAsync();
        print($"delete status : {succes}");
        if (!succes)
        {
            succes = await DeleteUserAsync();
            print($"delete status : {succes}");
        }
    }

    public async Task<bool> DeleteUserAsync()
    {
        try
        {
            var deleteUserRequest = new DeleteUserRequest
            {
                AccessToken = accessToken
            };

            var response = await _cognitoService.DeleteUserAsync(deleteUserRequest);
            return response.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (NotAuthorizedException e)
        {
            if (e.Message == "Access Token has expired")
            {
                var succes = await RefreshAuthAsync();
                print($"refresh status : {succes}");
            }

            return false;
        }
    }

    private async Task<bool> RefreshAuthAsync()
    {
        var authParameters = new Dictionary<string, string>();
        authParameters.Add("REFRESH_TOKEN", refreshToken);


        var authRequest = new InitiateAuthRequest

        {
            ClientId = MyUtils.appClientID,
            AuthParameters = authParameters,
            AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH
        };

        var response = await _cognitoService.InitiateAuthAsync(authRequest);

        refreshToken = response.AuthenticationResult.RefreshToken;
        accessToken = response.AuthenticationResult.AccessToken;
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async void Recover()
    {
        var succes = await ForgotPasswordAsync();
        print($"Recover status : {succes}");
    }

    private async Task<bool> ForgotPasswordAsync()
    {
        var recoverRequest = new ForgotPasswordRequest
        {
            ClientId = MyUtils.appClientID,
            Username = recoverEmail.text
        };

        var response = await _cognitoService.ForgotPasswordAsync(recoverRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async void ConfirmRecover()
    {
        var succes = await ConfirmForgotPasswordAsync();
        print($"Recover status : {succes}");
    }

    private async Task<bool> ConfirmForgotPasswordAsync()
    {
        var confirmrecoverRequest = new ConfirmForgotPasswordRequest
        {
            Username = confirmRecoverEmail.text,
            ClientId = MyUtils.appClientID,
            ConfirmationCode = confirmRecoverCode.text,
            Password = confirmRecoverNewPasword.text
        };

        var response = await _cognitoService.ConfirmForgotPasswordAsync(confirmrecoverRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }


    public async void GlobalSignOut()
    {
        var succes = await GlobalSignOutAsync();
        print($"Global sign out status : {succes}");
    }

    private async Task<bool> GlobalSignOutAsync()
    {
        var signOutRequest = new GlobalSignOutRequest
        {
            AccessToken = accessToken
        };
        var response = await _cognitoService.GlobalSignOutAsync(signOutRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async void PartialSignOut()
    {
        var succes = await PartialSignOutAsync();
        print($"Partial sign out status : {succes}");
    }

    private async Task<bool> PartialSignOutAsync()
    {
        var revokeTokenRequest = new RevokeTokenRequest
        {
            ClientId = MyUtils.appClientID,
            Token = refreshToken
        };
        var response = await _cognitoService.RevokeTokenAsync(revokeTokenRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    /*****************
     *
     * Cognito as an authorization service for unity
     *
     **************/

    public async void InvokeLambdaFunction()
    {
        var succes = await InvokeFunctionAsync();
        print($"Partial sign out status : {succes}");
    }

    private async Task<bool> InvokeFunctionAsync()
    {
        var request = new InvokeRequest
        {
            FunctionName = "myFunctionFromUnity",
            Payload = "{\"param\": \"Test\"}"
        };
        var credentials =
            new CognitoAWSCredentials(MyUtils.cognitoIdentityID, RegionEndpoint.EUWest1);
        var _lambdaService = new AmazonLambdaClient(credentials, RegionEndpoint.EUWest1);
        var response = await _lambdaService.InvokeAsync(request);

        var returnValue = Encoding.UTF8.GetString(response.Payload.ToArray());
        print(returnValue);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public void PickImage()
    {
        var permission = NativeGallery.GetImageFromGallery(async path =>
        {
            var succes = await UploadFileAsync(path);
            Debug.Log($"upload status : {succes}");
        });
    }

    public static async Task<bool> UploadFileAsync(string filePath)
    {
        var credentials =
            new CognitoAWSCredentials(MyUtils.cognitoIdentityID, RegionEndpoint.EUWest1);

        credentials.AddLogin($"cognito-idp.eu-west-1.amazonaws.com/{MyUtils.userPoolID}", idToken);
        var identityId = new AmazonS3Client(credentials, RegionEndpoint.EUWest1);

        var request = new PutObjectRequest
        {
            BucketName = "my-bucket-id",
            Key = $"{identityId}.jpg",
            FilePath = filePath
        };

        var client = new AmazonS3Client(credentials, RegionEndpoint.EUWest1);

        var response = await client.PutObjectAsync(request);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async void PutItem()
    {
        var email = await GetLoggedInUserEmail();
        if (!string.IsNullOrEmpty(email))
        {
            var succes = await PutItemAsync(email);
            print($"Put item status : {succes}");
        }
        else
        {
            Debug.LogError("Kullanıcı e-posta adresi alınamadı.");
        }
    }

    private static async Task<bool> PutItemAsync(string email)
    {
        var credentials =
            new CognitoAWSCredentials(MyUtils.cognitoIdentityID, RegionEndpoint.EUWest1);
        var item = new Dictionary<string, AttributeValue>
        {
            ["email"] = new() { S = email },
            ["score"] = new() { N = "45" },
            ["itemFound"] = new()
            {
                L =
                {
                    new AttributeValue { N = "1" },
                    new AttributeValue { N = "61" },
                    new AttributeValue { N = "17" }
                }
            }
        };

        var request = new PutItemRequest
        {
            TableName = "MyTable",
            Item = item
        };
        var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.EUWest1);
        var response = await client.PutItemAsync(request);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async void GetItems()
    {
        var success = await GetItemAsync();
        print($"Get item status : {success}");
    }

    private static async Task<bool> GetItemAsync()
    {
        var email = "";
        var credentials =
            new CognitoAWSCredentials(MyUtils.cognitoIdentityID, RegionEndpoint.EUWest1);
        // GetItemRequest oluştur
        var request = new GetItemRequest
        {
            TableName = "MyTable",
            Key = new Dictionary<string, AttributeValue>
            {
                ["email"] = new() { S = email } // Primary key'e göre sorgu yap
            }
        };

        // DynamoDB client'ı oluştur
        var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.EUWest1);
        var response = await client.GetItemAsync(request);
        // Veriyi döndür
        Debug.Log($"{response.Item}");

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    private async Task<string> GetLoggedInUserEmail()
    {
        try
        {
            var getUserRequest = new GetUserRequest
            {
                AccessToken = accessToken
            };

            var response = await _cognitoService.GetUserAsync(getUserRequest);

            var emailAttribute = response.UserAttributes.Find(attr => attr.Name == "email");
            return emailAttribute?.Value;
        }
        catch (Exception ex)
        {
            Debug.LogError($"E-posta adresi alınırken hata oluştu: {ex.Message}");
            return null;
        }
    }
}