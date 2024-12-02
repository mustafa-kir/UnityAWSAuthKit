using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIRevertPasswordPage : BasePage
{
    [SerializeField] private List<GameObject> RevertPasswordPages;
    [SerializeField] private TMP_Text wrongCodeText;

    [SerializeField] private TMP_InputField emailRevertPassword;
    [SerializeField] private TMP_InputField codeRevertPassword;
    [SerializeField] private TMP_InputField newPasswordOne;
    [SerializeField] private TMP_InputField newPasswordTwo;
    [SerializeField] private UIController uiController;
    public override string PageName => "RevertPasswordPage";

    private void Start()
    {
        foreach (var page in RevertPasswordPages)
        {
            page.SetActive(false);
            if (page.name == "SendMailPage") page.SetActive(true);
        }
    }

    public async void SendToMail()
    {
        var success = await CognitoSDKController.Instance.ForgotPasswordAsync(emailRevertPassword.text);
        Debug.Log(success);
        if (success)
            foreach (var page in RevertPasswordPages)
            {
                page.SetActive(false);
                if (page.name == "CodeVerifying") page.SetActive(true);
            }
    }

    public async void RevertToCodeVerification()
    {
        if (newPasswordOne.text == newPasswordTwo.text)
        {
            var success = await CognitoSDKController.Instance.ConfirmForgotPasswordAsync(emailRevertPassword.text,
                codeRevertPassword.text, newPasswordOne.text);
            if (success)
            {
                wrongCodeText.text = "";
                uiController.TriggerOpenPage("GameMenuPage");
            }
            else
            {
                wrongCodeText.text = "Sorry, you have to try again.";
            }
        }
        else
        {
            wrongCodeText.text = "New passwords do not match.";
        }
    }
}