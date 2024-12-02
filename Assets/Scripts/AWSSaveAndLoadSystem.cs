using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AWSSaveAndLoadSystem : MonoBehaviour, IAWSSaveAndLoadService
{
    [SerializeField] private CognitoSDKController cognitoSDKController;

    [SerializeField] private TMP_Text nickNameText;
    [SerializeField] private Image profilePicture;
    private string access_token;

    public async Task<bool> SaveDataAsync(string email, string score, List<string> itemFoundValues)
    {
        var credentials =
            new CognitoAWSCredentials(MyUtils.cognitoIdentityID, RegionEndpoint.EUWest1);
        var item = new Dictionary<string, AttributeValue>
        {
            ["email"] = new() { S = email },
            ["score"] = new() { N = score },
            ["itemFound"] = new()
            {
                L = itemFoundValues.ConvertAll(value => new AttributeValue { N = value })
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

    public async Task<Dictionary<string, AttributeValue>> LoadDataAsync(string email)
    {
        var credentials =
            new CognitoAWSCredentials(MyUtils.cognitoIdentityID, RegionEndpoint.EUWest1);

        var result = new Dictionary<string, AttributeValue>();
        var request = new GetItemRequest
        {
            TableName = "MyTable",
            Key = new Dictionary<string, AttributeValue>
            {
                ["email"] = new() { S = email }
            }
        };


        var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.EUWest1);
        var response = await client.GetItemAsync(request);

        if (response.HttpStatusCode == HttpStatusCode.OK && response.Item.Count > 0)
            result = response.Item.ToDictionary(kvp => kvp.Key, kvp => kvp.Value switch
            {
                { S: string s } => new AttributeValue { S = s },
                { N: string n } => new AttributeValue { N = n },
                { L: var l } => new AttributeValue { L = l }
            });
        // print(result["itemFound"].L[0].N);
        return result;
    }

    // Load System
    public async void Load()
    {
        var email = await cognitoSDKController.GetUserAsync();
        await LoadDataAsync(email);
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

                nickNameText.text = $"Hi {user.nickname}";
                if (!string.IsNullOrEmpty(user.picture)) StartCoroutine(GetPicture(user.picture));
            }
        }
    }


    private IEnumerator GetPicture(string pictureUrl)
    {
        Debug.Log("getpicture: geldi");
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
}