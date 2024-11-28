using System.Globalization;

namespace Core
{
    public class Fatura
    {
        public string TRNTYPE { get; set; }
        public string DTPOSTED { get; set; }
        public string DTPOSTED_ORIGINAL { get; set; }
        public string TRNAMT { get; set; }
        public string FITID { get; set; }
        public string NAME { get; set; }
        public decimal AMOUNT_VALUE { get; set; }
        public DateTime DATA { get; set; }

        public Fatura(string trnType, decimal amount, string name, DateTime datePosted, string dataOriginal)
        {
            TRNTYPE = trnType;
            NAME = name.Replace(",", ".").Replace("$", "").Replace("/", "").Replace("\\", "").Replace("%", "");
            NAME = string.IsNullOrWhiteSpace(NAME) ? "Anuidade" : (NAME.Length > 30 ? NAME[..30] : NAME);
            AMOUNT_VALUE = trnType == "DEBIT" ? -amount : amount;
            DATA = datePosted;
            DTPOSTED_ORIGINAL = dataOriginal;

            // Formatar a data
            DTPOSTED = datePosted.ToString("yyyyMMdd05mmss");

            // Calcular e formatar o valor
            var signedAmount = trnType == "DEBIT" ? -amount : amount;
            TRNAMT = signedAmount.ToString("F2", CultureInfo.InvariantCulture);

            // Gerar FITID
            FITID = GenerateFITID(datePosted, name, amount);
        }

        private string GenerateFITID(DateTime datePosted, string name, decimal valor)
        {
            // Concatenar os valores de entrada em uma string
            var datePart = datePosted.ToString("yyyyMMdd");
            var namePart = name.Replace(" ", "").Replace("&", "").Replace(",", ".")
                               .Replace("$", "");
            var combinedInput = $"{datePart}{namePart}{valor}";

            // Gerar um hash determin�stico dos par�metros de entrada
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combinedInput));
                return new Guid(hash).ToString();
            }
        }

    }
}