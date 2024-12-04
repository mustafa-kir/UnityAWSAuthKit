using System.Collections;
using System.Web;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CognitoHostedUIController : MonoBehaviour
{
    public TMP_Text signinText;
    public Image profilePicture;
    [SerializeField] private UIController uiController;
    private string access_token;
    public static CognitoHostedUIController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += onDeepLinkActivated;

            if (!string.IsNullOrEmpty(Application.absoluteURL))
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void onDeepLinkActivated(string url)
    {
        var splittedUrl = url.Split("#");

        if (splittedUrl.Length == 2)
        {
            var urlParams = splittedUrl[1];
            if (!string.IsNullOrEmpty(urlParams))
            {
                access_token = HttpUtility.ParseQueryString(urlParams).Get("access_token");
                Debug.Log($"access toke :  {access_token}");
                if (!string.IsNullOrEmpty(access_token)) Login();
            }
        }
    }

    private void Login()
    {
        uiController.TriggerOpenPage("GameMenuPage");
        StartCoroutine(GetUserInfo());
    }

    private IEnumerator GetUserInfo()
    {
        using (var webRequest = UnityWebRequest.Get($"{MyUtils.hostedUIDomain}/oauth2/userInfo"))
        {
            webRequest.SetRequestHeader("Content-Type", "application/x-amz-json-1.1; charset=UTF-8 ");
            webRequest.SetRequestHeader("Authorization", "Bearer " + access_token);
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var user = JsonUtility.FromJson<CognitoHostedUIUser>(webRequest.downloadHandler.text);

                signinText.text = $"Hi {user.nickname}";
                if (!string.IsNullOrEmpty(user.picture)) StartCoroutine(GetPicture(user.picture));
            }
        }
    }


    private IEnumerator GetPicture(string pictureUrl)
    {
        using (var uwr = UnityWebRequestTexture.GetTexture(pictureUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                var texture = DownloadHandlerTexture.GetContent(uwr);
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                profilePicture.sprite = sprite;
            }
        }
    }

    public void OpenHostedUI()
    {
        string callbackURL;
#if UNITY_ANDROID
        callbackURL = "my-demo-app://login";
#elif UNITY_WEBGL
        callbackURL = Application.absoluteURL;
#elif UNITY_IOS
    callbackURL = "my-demo-app://login";
#endif

        Application.OpenURL(
            $"{MyUtils.hostedUIDomain}/oauth2/authorize?client_id=76f0fh64mu164ih9psbd8art9v&response_type=token&scope=aws.cognito.signin.user.admin+openid&redirect_uri={callbackURL}");
    }

    public void LoginWithGoogle()
    {
        string callbackURL;
#if UNITY_ANDROID
        callbackURL = "my-demo-app://login";
#elif UNITY_WEBGL
        callbackURL = Application.absoluteURL;
#elif UNITY_IOS
    callbackURL = "my-demo-app://login";
#endif
        Application.OpenURL(
            $"{MyUtils.hostedUIDomain}/oauth2/authorize?identity_provider=Google&client_id={MyUtils.appClientID}&response_type=token&scope=aws.cognito.signin.user.admin+openid&redirect_uri={callbackURL}");
    }

    public void LoginWithFacebook()
    {
        string callbackURL;
#if UNITY_ANDROID
        callbackURL = "my-demo-app://login";
#elif UNITY_WEBGL
        callbackURL = Application.absoluteURL;
#elif UNITY_IOS
    callbackURL = "my-demo-app://login";
#endif

        Application.OpenURL(
            $"{MyUtils.hostedUIDomain}/oauth2/authorize?identity_provider=Facebook&client_id={MyUtils.appClientID}&response_type=token&scope=aws.cognito.signin.user.admin+openid&redirect_uri={callbackURL}");
    }

    public void LoginWithApple()
    {
        string callbackURL;
#if UNITY_ANDROID
        callbackURL = "my-demo-app://login";
#elif UNITY_WEBGL
        callbackURL = Application.absoluteURL;
#elif UNITY_IOS
    callbackURL = "my-demo-app://login";
#endif

        Application.OpenURL(
            $"{MyUtils.hostedUIDomain}/oauth2/authorize?identity_provider=SignInWithApple&client_id={MyUtils.appClientID}&response_type=token&scope=aws.cognito.signin.user.admin+openid&redirect_uri={callbackURL}");
    }
    
}