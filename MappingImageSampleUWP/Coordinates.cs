using System.Text.Json.Serialization;

namespace MappingImageSampleUWP
{
    class Coordinates
    {
        [JsonPropertyName("lat")]
        public string Latitude { get; set; }

        [JsonPropertyName("lon")]
        public string Longitude { get; set; }
    }
}
