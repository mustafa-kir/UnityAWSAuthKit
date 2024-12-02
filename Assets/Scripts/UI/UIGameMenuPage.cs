using UnityEngine;
using UnityEngine.SceneManagement;

public class UIGameMenuPage : BasePage
{
    [SerializeField] private AWSSaveAndLoadSystem awsSaveAndLoadSystem;
    [SerializeField] private UIController uiController;
    public override string PageName => "GameMenuPage";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        awsSaveAndLoadSystem.Load();
    }

    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene(1);
    }

    public async void GlobalSignOutBtn()
    {
        var success = await CognitoSDKController.Instance.GlobalSignOutAsync();
        if (success) uiController.TriggerOpenPage("LoginPage");
    }

    public async void PartialSignOutBtn()
    {
        await CognitoSDKController.Instance.PartialSignOutAsync();
    }
}