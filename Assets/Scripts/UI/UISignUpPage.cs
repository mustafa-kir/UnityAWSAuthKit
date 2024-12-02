using TMPro;
using UnityEngine;

public class UISignUpPage : BasePage
{
    [SerializeField] private TMP_InputField signUpEmail;
    [SerializeField] private TMP_InputField signUpPassword;
    [SerializeField] private TMP_InputField signUpNickName;
    [SerializeField] private TMP_Text signUpWrong;
    [SerializeField] private UIController uiController;
    public override string PageName => "SignUpPage";

    public async void SignUp()
    {
        if (signUpNickName.text != "" && signUpPassword.text != "" && signUpEmail.text != "")
        {
            var success =
                await CognitoSDKController.Instance.SignUpAsync(signUpNickName.text, signUpEmail.text,
                    signUpPassword.text);
            if (success) uiController.TriggerOpenPage("GameMenuPage");
            else signUpWrong.text = "Please entry email and password again!";
        }
        else
        {
            signUpWrong.text = "Please enter the correct nickname, email and password!";
        }
    }
}