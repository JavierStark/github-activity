using System.Collections;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();


var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/users/javierstark/events");
request.Headers.Add("Accept", "application/vnd.github+json");
request.Headers.Add("Authorization", $"token {config["GITHUB_API_TOKEN"]}");
request.Headers.Add("User-Agent", "github-activity");
request.Headers.Add("X-GitHub-Api-Version","2022-11-28");
var response = await client.SendAsync(request);

var content = await response.Content.ReadAsStringAsync();
JsonReader reader = new JsonTextReader(new StringReader(content));
var serializer = new JsonSerializer();
var elements = serializer.Deserialize<List<Dictionary<string, object>>>(reader);

elements?.ForEach(x=>Console.WriteLine(x["type"]));
