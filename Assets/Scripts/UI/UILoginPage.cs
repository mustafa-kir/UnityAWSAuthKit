using TMPro;
using UnityEngine;

public class UILoginPage : BasePage
{
    [SerializeField] private TMP_InputField signUpEmail;
    [SerializeField] private TMP_InputField signUpPassword;
    [SerializeField] private TMP_Text LoginWrongText;

    [SerializeField] private UIController uiController;
    private BasePage _basePageImplementation;
    public override string PageName => "LoginPage";

    public async void LoginButton()
    {
        if (signUpEmail.text != "" || signUpPassword.text != "")
        {
            var success = await CognitoSDKController.Instance.InitiateAuthAsync(
                signUpEmail.text,
                signUpPassword.text
            );

            if (success)
                // load  game menu
                uiController.TriggerOpenPage("GameMenuPage");
            else
                LoginWrongText.text = "Email or password is incorrect";
        }
        else
        {
            LoginWrongText.text = "Email or password is incorrect";
        }
    }


    public void forgetPasswordButton()
    {
        uiController.TriggerOpenPage("RevertPasswordPage");
    }

    public void LoginWithGoogle()
    {
        CognitoHostedUIController.Instance.LoginWithGoogle();
    }

    public void LoginWithFacebook()
    {
        CognitoHostedUIController.Instance.LoginWithFacebook();
    }

    public void LoginWithApple()
    {
        CognitoHostedUIController.Instance.LoginWithApple();
    }

    public void SignUp()
    {
        uiController.TriggerOpenPage("SignUpPage");
    }
}