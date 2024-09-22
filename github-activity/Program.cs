using System.Collections;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var dopplerToken = config["Doppler_Token"];

Console.WriteLine("Enter the username:");
var user = Console.ReadLine();

var client = new HttpClient();

client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dopplerToken);
var dopplerResponse = await client.GetAsync("https://api.doppler.com/v3/configs/config/secrets/download?format=json");
var dopplerContent = await dopplerResponse.Content.ReadAsStringAsync();
var secrets = JObject.Parse(dopplerContent);

var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/users/{user}/events");
request.Headers.Add("Accept", "application/vnd.github+json");
request.Headers.Add("Authorization", $"token {secrets["GITHUB_API_TOKEN"]}");
request.Headers.Add("User-Agent", "github-activity");
request.Headers.Add("X-GitHub-Api-Version","2022-11-28");
var response = await client.SendAsync(request);

try {
    response.EnsureSuccessStatusCode();
} catch (HttpRequestException e) {
    Console.WriteLine(e.Message);
    return;
}

var content = await response.Content.ReadAsStringAsync();
var elements = JArray.Parse(content);

if (elements == null) {
    Console.WriteLine("No events found.");
    return;
}

foreach (var element in elements) {
    var payload = (JObject)element["payload"];
    var repo = (JObject)element["repo"];

    switch (element["type"]?.ToString())
    {
        case "PushEvent":
        {
            var commits = (JArray)payload["commits"];
            Console.WriteLine($"Pushed {commits.Count} commits to {repo["name"]}");
            break;
        }
        case "CreateEvent":
        {
            Console.WriteLine($"Created {repo["name"]} repository");
            break;
        }
        case "DeleteEvent":
        {
            Console.WriteLine($"Deleted {repo["name"]} repository");
            break;
        }
        case "WatchEvent":
        {
            Console.WriteLine($"Starred {repo["name"]} repository");
            break;
        }
        case "IssuesEvent":
        {
            var action = payload["action"].ToString();
            action = char.ToUpper(action[0]) + action[1..];
            Console.WriteLine($"{action} issue in {repo["name"]} repository");
            break;
        }
        case "IssueCommentEvent":
        {
            Console.WriteLine($"Commented on issue in {repo["name"]} repository");
            break;
        }
        case "PullRequestEvent":
        {
            var action = payload["action"].ToString();
            action = char.ToUpper(action[0]) + action[1..];
            Console.WriteLine($"{action} pull request in {repo["name"]} repository");
            break;
        }
        case "PullRequestReviewEvent":
        {
            var action = payload["action"].ToString();
            action = char.ToUpper(action[0]) + action[1..];
            Console.WriteLine($"{action} pull request review in {repo["name"]} repository");
            break;
        }
        case "ForkEvent":
        {
            Console.WriteLine($"Forked {repo["name"]} repository");
            break;
        }
        case "PullRequestReviewCommentEvent":
        {
            Console.WriteLine($"Commented on pull request in {repo["name"]} repository");
            break;
        }
        case "ReleaseEvent":
        {
            Console.WriteLine($"Released {payload["release"]["name"]} in {repo["name"]} repository");
            break;
        }
        case "PublicEvent":
        {
            Console.WriteLine($"Open sourced {repo["name"]} repository");
            break;
        }
        case "MemberEvent":
        {
            Console.WriteLine($"Added {payload["member"]["login"]} as a collaborator to {repo["name"]} repository");
            break;
        }
        case "GollumEvent":
        {
            Console.WriteLine($"Updated the wiki in {repo["name"]} repository");
            break;
        }
        case "CommitCommentEvent":
        {
            Console.WriteLine($"Commented on commit in {repo["name"]} repository");
            break;
        }
    }
}

public class Doppler
{
    
    [JsonPropertyName("DOPPLER_PROJECT")]
    public string DopplerProject { get; set; }

    [JsonPropertyName("DOPPLER_ENVIRONMENT")]
    public string DopplerEnvironment { get; set; }

    [JsonPropertyName("DOPPLER_CONFIG")]
    public string DopplerConfig { get; set; }
    
    [JsonPropertyName("GITHUB_API_TOKEN")]
    public string GithubApiToken { get; set; }
    
}
