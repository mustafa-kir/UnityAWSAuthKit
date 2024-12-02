using System.Collections.Generic;

namespace DefaultNamespace
{
    [System.Serializable]
    public class CognitoAttribute
    {
        public string Name;
        public string Value;
    }

    public class CognitoSignUpRequest
    {
        public string Username;
        public string Password;
        public string ClientId;
        public List<CognitoAttribute> UserAttributes;
    }

    public class CognitoHostedUIUser
    {
        public string nickname;
        public string picture;
    }
}