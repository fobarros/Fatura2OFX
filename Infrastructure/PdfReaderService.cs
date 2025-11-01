using System.Text;
using Core;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text.RegularExpressions;

namespace Infrastructure
{
    public class PdfReaderService
    {
        private readonly ProcessadorFatura _processadorFatura;
        private HashSet<string> _linhasProcessadas;

        public PdfReaderService(ProcessadorFatura processadorFatura)
        {
            _processadorFatura = processadorFatura;
            _linhasProcessadas = new HashSet<string>();
        }

        public (List<Fatura>, string) ProcessarFaturasDoPdf(string caminhoPdf, DateTime dataInicioFatura)
        {
            _linhasProcessadas.Clear(); // Limpa o cache de linhas a cada novo processamento
            string textoExtraido = ExtrairTextoDoPdf(caminhoPdf);
            var (faturas, log) = _processadorFatura.ProcessarTextoPdf(textoExtraido, dataInicioFatura);
            return (faturas, log);
        }

        private bool PrecisaCorrigirLinha(string linha)
        {
            return linha.Contains("..") || // Pontos duplicados
                   Regex.IsMatch(linha, @"([A-Z])\1") || // Letras maiúsculas duplicadas
                   Regex.IsMatch(linha, @"(\d)\1") || // Números duplicados
                   linha.Contains("  "); // Espaços duplicados
        }

        private string CorrigirLinha(string linha)
        {
            if (!PrecisaCorrigirLinha(linha))
                return linha;

            // Primeiro passo: corrigir números do cartão mantendo 4 dígitos no início e fim
            var match = Regex.Match(linha, @"(\d+)\.+\*+\.+\*+\.+(\d+)");
            if (match.Success)
            {
                string inicio = match.Groups[1].Value;
                string fim = match.Groups[2].Value;
                
                // Garante exatamente 4 dígitos no início
                if (inicio.Length > 4)
                    inicio = string.Concat(inicio.Where((c, i) => i % 2 == 0).Take(4));
                else if (inicio.Length < 4)
                    inicio = inicio.PadLeft(4, '0');

                // Garante exatamente 4 dígitos no fim
                if (fim.Length > 4)
                    fim = string.Concat(fim.Where((c, i) => i % 2 == 0).Take(4));
                else if (fim.Length < 4)
                    fim = fim.PadLeft(4, '0');

                linha = linha.Replace(match.Value, $"{inicio}.****.****.{fim}");
            }

            // Segundo passo: corrigir letras duplicadas
            linha = Regex.Replace(linha, @"([A-Z])\1+", "$1");

            // Terceiro passo: corrigir palavras específicas que podem ter sido afetadas
            linha = linha.Replace("BAROS", "BARROS");

            // Quarto passo: corrigir símbolos e espaços
            linha = Regex.Replace(linha, @"\s+", " ");           // espaços múltiplos
            linha = Regex.Replace(linha, @"R\$\s+", "R$ ");     // espaço após R$
            linha = Regex.Replace(linha, @"(\d+),(\d+)", "$1,$2"); // números com vírgula

            return linha.Trim();
        }

        private string ExtrairTextoDoPdf(string caminhoPdf)
        {
            using (PdfReader reader = new PdfReader(caminhoPdf))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                var texto = new StringBuilder();
                var strategy = new SimpleTextExtractionStrategy();

                for (int i = 2; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var pagina = pdfDoc.GetPage(i);
                    string conteudoPagina = PdfTextExtractor.GetTextFromPage(pagina, strategy);
                    
                    var linhas = conteudoPagina.Split('\n')
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .Select(l => l.Trim())
                        .Select(l => CorrigirLinha(l))
                        .Where(l => !_linhasProcessadas.Contains(l)); // Remove linhas duplicadas

                    foreach (var linha in linhas)
                    {
                        _linhasProcessadas.Add(linha); // Adiciona a linha ao cache
                        texto.AppendLine(linha);
                    }
                }

                return texto.ToString();
            }
        }
    }
}