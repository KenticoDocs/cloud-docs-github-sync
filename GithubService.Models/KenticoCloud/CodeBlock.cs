// This code was generated by a cloud-generators-net tool 
// (see https://github.com/Kentico/cloud-generators-net).
// 
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated. 
// For further modifications of the class, create a separate file with the partial class.

using Newtonsoft.Json;

namespace GithubService.Models.KenticoCloud
{
    public class CodeBlock
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; }
        [JsonProperty("js")]
        public string Js { get; set; }
        [JsonProperty("javarx")]
        public string Javarx { get; set; }
        [JsonProperty("c_")]
        public string C { get; set; }
        [JsonProperty("java")]
        public string Java { get; set; }
        [JsonProperty("python")]
        public string Python { get; set; }
        [JsonProperty("ts")]
        public string Ts { get; set; }
        [JsonProperty("ruby")]
        public string Ruby { get; set; }
        [JsonProperty("swift")]
        public string Swift { get; set; }
        [JsonProperty("curl")]
        public string Curl { get; set; }
    }
}