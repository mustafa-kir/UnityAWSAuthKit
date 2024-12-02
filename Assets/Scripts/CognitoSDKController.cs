using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;

public class CognitoSDKController : MonoSingleton<CognitoSDKController>
{
    private static string idToken;
    private AmazonCognitoIdentityProviderClient _cognitoService;

    private string accessToken;
    private string refreshToken;

    private void Start()
    {
        _cognitoService =
            new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.EUWest1);
    }

    public async Task<bool> InitiateAuthAsync(string signInEmail, string signInPasword)
    {
        var authParameters = new Dictionary<string, string>();
        authParameters.Add("USERNAME", signInEmail);
        authParameters.Add("PASSWORD", signInPasword);

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

    public async Task<string> GetUserAsync()
    {
        var getUserRequest = new GetUserRequest
        {
            AccessToken = accessToken
        };

        var response = await _cognitoService.GetUserAsync(getUserRequest);
        var email = response.UserAttributes.Find(attribute => attribute.Name == "email");

        return email.Value == null ? "" : email.Value;
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

    public async Task<bool> ForgotPasswordAsync(string recoverEmail)
    {
        var recoverRequest = new ForgotPasswordRequest
        {
            ClientId = MyUtils.appClientID,
            Username = recoverEmail
        };

        var response = await _cognitoService.ForgotPasswordAsync(recoverRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
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

    public async Task<bool> ConfirmForgotPasswordAsync(string confirmRecoverEmail, string confirmRecoverCode,
        string confirmRecoverNewPasword)
    {
        var confirmrecoverRequest = new ConfirmForgotPasswordRequest
        {
            Username = confirmRecoverEmail,
            ClientId = MyUtils.appClientID,
            ConfirmationCode = confirmRecoverCode,
            Password = confirmRecoverNewPasword
        };

        var response = await _cognitoService.ConfirmForgotPasswordAsync(confirmrecoverRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> SignUpAsync(string signUpNickname, string signUpEmail, string signUpPassword)
    {
        var nicknameAttrs = new AttributeType
        {
            Name = "nickname",
            Value = signUpNickname
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
            Username = signUpEmail,
            ClientId = MyUtils.appClientID,
            Password = signUpPassword
        };

        var response = await _cognitoService.SignUpAsync(signUpRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }
}