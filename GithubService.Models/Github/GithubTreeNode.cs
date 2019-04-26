using Newtonsoft.Json;

namespace GithubService.Models.Github
{
    public class GithubTreeNode
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        // Unique identifier of a file in github, is used to access the file through API
        [JsonProperty("sha")]
        public string Id { get; set; }
    }
}