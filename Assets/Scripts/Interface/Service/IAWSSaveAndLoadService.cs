using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

public interface IAWSSaveAndLoadService
{
    Task<bool> SaveDataAsync(string email, string score, List<string> itemFoundValues);
    Task<Dictionary<string, AttributeValue>> LoadDataAsync(string email);
}