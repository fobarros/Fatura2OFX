using Microsoft.AspNetCore.Mvc;
using Infrastructure;
using System.Diagnostics;
using System.Text;
using BRBPresentation.Models;
using Core;

public enum TipoSaida
{
    OFX,
    GoogleSpreadsheet,
    Tela
}

public class PdfController : Controller
{
    private readonly PdfReaderService _pdfReaderService;

    public PdfController(PdfReaderService pdfReaderService)
    {
        _pdfReaderService = pdfReaderService;
    }

    public IActionResult Index()
    {
        ViewBag.TiposSaida = Enum.GetValues(typeof(TipoSaida));
        return View();
    }

    [HttpGet]
    public IActionResult DownloadOfx()
    {
        var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", "brbCard.ofx");
        var bytesArquivo = System.IO.File.ReadAllBytes(caminhoArquivo);
        return File(bytesArquivo, "application/ofx", "brbCard.ofx");
    }

    [HttpGet]
    public IActionResult DownloadGoogleSpreadsheet()
    {
        var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", "brbCard.csv");
        var bytesArquivo = System.IO.File.ReadAllBytes(caminhoArquivo);
        return File(bytesArquivo, "text/csv", "brbCard.csv");
    }

    private (string nomeArquivo, string conteudoOfx) GerarArquivoOFX(List<Core.Fatura> faturas)
    {
        var menorData = faturas.Min(f => f.DATA);
        var maiorData = faturas.Max(f => f.DATA);
        var totalFatura = faturas.Sum(f => f.AMOUNT_VALUE);

        var ofxGenerator = new Core.OfxGenerator();
        string conteudoOfx = ofxGenerator.GerarOfx(faturas, menorData, maiorData, totalFatura);
        
        return ("brbCard.ofx", conteudoOfx);
    }

    [HttpPost]
    public IActionResult Upload(IFormFile file, string dataInicioFatura, TipoSaida tipoSaida, bool debugMode = false)
    {
        var nomeArquivoSaida = String.Empty;
        if (file != null && file.Length > 0)
        {
            // Parse da data recebida do formulário (formato YYYY-MM-DD)
            if (string.IsNullOrEmpty(dataInicioFatura) || !DateTime.TryParse(dataInicioFatura, out DateTime dataInicio))
            {
                ViewBag.ErrorMessage = "Data inicial inválida. Por favor, informe uma data válida.";
                ViewBag.TiposSaida = Enum.GetValues(typeof(TipoSaida));
                return View("Index");
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            //realiza o processo de gerar faturas para cada linha do PDF
            var (faturas, log) = _pdfReaderService.ProcessarFaturasDoPdf(path, dataInicio);

            // Calcular o total das faturas
            decimal totalFatura = faturas.Sum(fatura => fatura.AMOUNT_VALUE);
            decimal totalFaturaCredito = faturas.Where(fatura => fatura.AMOUNT_VALUE > 0).Sum(fatura => fatura.AMOUNT_VALUE);
            decimal totalFaturaDebito = faturas.Where(fatura => fatura.AMOUNT_VALUE < 0).Sum(fatura => fatura.AMOUNT_VALUE);

            Debug.WriteLine($"Total Fatura: {totalFatura}");
            Debug.WriteLine($"Total Fatura Crédito: {totalFaturaCredito}");
            Debug.WriteLine($"Total Fatura Débito: {totalFaturaDebito}");

            ViewBag.TotalFatura = totalFatura;
            ViewBag.TotalFaturaCredito = totalFaturaCredito;
            ViewBag.TotalFaturaDebito = totalFaturaDebito;
            
            // Só passa o log para a view se estiver no modo debug e o tipo de saída for Tela
            if (tipoSaida == TipoSaida.Tela && debugMode)
            {
                ViewBag.log = log;
            }

            if (tipoSaida == TipoSaida.OFX)
            {
                var (nomeArquivo, conteudoOfx) = GerarArquivoOFX(faturas);
                var caminhoArquivoSaida = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeArquivo);
                System.IO.File.WriteAllText(caminhoArquivoSaida, conteudoOfx);
                return View("DownloadOfx", nomeArquivo);
            }
            else if (tipoSaida == TipoSaida.GoogleSpreadsheet)
            {
                var csvBuilder = new StringBuilder();
                // Cabeçalho do CSV
                csvBuilder.AppendLine("TRNTYPE,DTPOSTED,DTPOSTED_ORIGINAL,TRNAMT,FITID,NAME,AMOUNT_VALUE,DATA");
                
                // Dados
                foreach (var fatura in faturas)
                {
                    csvBuilder.AppendLine($"{fatura.TRNTYPE},{fatura.DTPOSTED},{fatura.DTPOSTED_ORIGINAL},{fatura.TRNAMT},{fatura.FITID},{fatura.NAME},{fatura.AMOUNT_VALUE.ToString(System.Globalization.CultureInfo.InvariantCulture)},{fatura.DATA:yyyy-MM-dd}");
                }

                nomeArquivoSaida = "brbCard.csv";
                var caminhoArquivoSaida = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeArquivoSaida);
                System.IO.File.WriteAllText(caminhoArquivoSaida, csvBuilder.ToString());
                return View("DownloadGoogleSpreadsheet", nomeArquivoSaida);
            }
            else // Tela
            {
                var viewModel = new ReportViewModel
                {
                    Faturas = faturas,
                    TotalFatura = totalFatura,
                    TotalFaturaCredito = totalFaturaCredito,
                    TotalFaturaDebito = totalFaturaDebito
                };
                ViewBag.NomeArquivo = file.FileName;
                return View("Report", viewModel);
            }
        }

        return View("Index");
    }

    [HttpPost]
    public IActionResult GerarOFXParaFernando([FromBody] List<FaturaRequest> faturasData)
    {
        try
        {
            if (faturasData == null || !faturasData.Any())
            {
                return Json(new { success = false, message = "Nenhuma fatura selecionada para o Fernando" });
            }

            var faturas = faturasData.Select(f => new Core.Fatura(
                f.TrnType,
                f.Amount,
                f.Name,
                f.DatePosted,
                f.DataOriginal,
                f.Usuario,
                f.CartaoCredito
            )).ToList();

            // Atualizar o FITID das faturas com os valores originais
            for (int i = 0; i < faturas.Count; i++)
            {
                faturas[i].FITID = faturasData[i].Fitid;
            }

            var (nomeArquivo, conteudoOfx) = GerarArquivoOFX(faturas);
            nomeArquivo = "brbCard_fernando.ofx";
            
            var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeArquivo);
            System.IO.File.WriteAllText(caminhoArquivo, conteudoOfx);

            // Verificar se o arquivo foi criado
            if (!System.IO.File.Exists(caminhoArquivo))
            {
                return Json(new { success = false, message = "Erro ao criar arquivo OFX" });
            }

            // Retornar o arquivo diretamente
            var bytes = System.IO.File.ReadAllBytes(caminhoArquivo);
            return File(bytes, "application/x-ofx", nomeArquivo);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
