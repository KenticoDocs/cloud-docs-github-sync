using GithubService.Models;
using Newtonsoft.Json;

namespace GithubService.Repository.Models
{
    public class CodeFragment
    {
        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "codename")]
        public string Codename
            => $"{Identifier}_{Platform.GetPlatformCodenameTag()}";

        public override string ToString() 
            => $"Identifier: {Identifier}, Content: {Content}, Language: {Language}";
    }
}
