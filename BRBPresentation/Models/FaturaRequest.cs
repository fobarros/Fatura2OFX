using System.Text.Json.Serialization;

namespace BRBPresentation.Models
{
    public class FaturaRequest
    {
        [JsonPropertyName("trnType")]
        public required string TrnType { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("datePosted")]
        public DateTime DatePosted { get; set; }

        [JsonPropertyName("dataOriginal")]
        public required string DataOriginal { get; set; }

        [JsonPropertyName("usuario")]
        public required string Usuario { get; set; }

        [JsonPropertyName("cartaoCredito")]
        public required string CartaoCredito { get; set; }

        [JsonPropertyName("fitid")]
        public required string Fitid { get; set; }
    }
} 