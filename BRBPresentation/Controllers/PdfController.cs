using Microsoft.AspNetCore.Mvc;
using Infrastructure;
using System.Diagnostics;
using System.Text;
using BRBPresentation.Models;

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

    [HttpPost]
    public IActionResult Upload(IFormFile file, TipoSaida tipoSaida)
    {
        var nomeArquivoSaida = String.Empty;
        if (file != null && file.Length > 0)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            //realiza o processo de gerar faturas para cada linha do PDF
            var (faturas, log) = _pdfReaderService.ProcessarFaturasDoPdf(path);

            var menorData = faturas.Min(fatura => fatura.DATA);
            var maiorData = faturas.Max(fatura => fatura.DATA);

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
            ViewBag.log = log;

            if (tipoSaida == TipoSaida.OFX)
            {
                var ofxGenerator = new Core.OfxGenerator();
                string conteudoOfx = ofxGenerator.GerarOfx(faturas, menorData, maiorData, totalFatura);
                nomeArquivoSaida = "brbCard.ofx";
                var caminhoArquivoSaida = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeArquivoSaida);
                System.IO.File.WriteAllText(caminhoArquivoSaida, conteudoOfx);
                return View("DownloadOfx", nomeArquivoSaida);
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
                return View("Report", viewModel);
            }
        }

        return View("Index");
    }
}
