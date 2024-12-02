using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private List<GameObject> Pages;

    private void Start()
    {
        OnPageOpen += OpenPage;

        foreach (var page in Pages)
        {
            page.SetActive(false);
            if (page.name == "LoginPage") page.SetActive(true);
        }
    }

    public void OpenPage(string pageName)
    {
        foreach (var page in Pages)
        {
            page.SetActive(false);
            if (page.name == pageName) page.SetActive(true);
        }
    }

    public event Action<string> OnPageOpen;

    public void TriggerOpenPage(string pageName)
    {
        OnPageOpen?.Invoke(pageName);
    }
}