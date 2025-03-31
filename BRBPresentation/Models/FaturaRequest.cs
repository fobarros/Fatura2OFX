using System.Text.Json.Serialization;

namespace BRBPresentation.Models
{
    public class FaturaRequest
    {
        [JsonPropertyName("trnType")]
        public string TrnType { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("datePosted")]
        public DateTime DatePosted { get; set; }

        [JsonPropertyName("dataOriginal")]
        public string DataOriginal { get; set; }

        [JsonPropertyName("usuario")]
        public string Usuario { get; set; }

        [JsonPropertyName("cartaoCredito")]
        public string CartaoCredito { get; set; }

        [JsonPropertyName("fitid")]
        public string Fitid { get; set; }
    }
} 