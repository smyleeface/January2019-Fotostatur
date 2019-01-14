using Newtonsoft.Json;

namespace Fotostatur.Fotostatur.ImageAnalyzer {
    public class FoundCriterias {
        
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("points")]
        public float Points;
    }
}