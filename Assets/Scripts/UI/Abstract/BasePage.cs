using UnityEngine;

public abstract class BasePage : MonoBehaviour, IPage
{
    public abstract string PageName { get; }

    public virtual void ShowPage()
    {
        gameObject.SetActive(true);
    }

    public virtual void HidePage()
    {
        gameObject.SetActive(false);
    }
}