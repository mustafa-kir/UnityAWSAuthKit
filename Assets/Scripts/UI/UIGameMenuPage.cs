using UnityEngine;
using UnityEngine.SceneManagement;

public class UIGameMenuPage : BasePage
{
    [SerializeField] private AWSSaveAndLoadSystem awsSaveAndLoadSystem;
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
}