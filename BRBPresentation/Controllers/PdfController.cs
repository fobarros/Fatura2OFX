using Microsoft.AspNetCore.Mvc;
using Infrastructure;
using System.Diagnostics;

public class PdfController : Controller
{
    private readonly PdfReaderService _pdfReaderService;

    public PdfController(PdfReaderService pdfReaderService)
    {
        _pdfReaderService = pdfReaderService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult DownloadOfx()
    {
        var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", "brbCard.ofx");
        var bytesArquivo = System.IO.File.ReadAllBytes(caminhoArquivo);
        return File(bytesArquivo, "application/ofx", "brbCard.ofx");
    }

    [HttpPost]
    public IActionResult Upload(IFormFile file)
    {
        var nomeArquivoOfx = String.Empty;
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

            // Calcular o total de créditos (valores positivos)
            decimal totalFaturaCredito = faturas.Where(fatura => fatura.AMOUNT_VALUE > 0).Sum(fatura => fatura.AMOUNT_VALUE);

            // Calcular o total de débitos (valores negativos)
            decimal totalFaturaDebito = faturas.Where(fatura => fatura.AMOUNT_VALUE < 0).Sum(fatura => fatura.AMOUNT_VALUE);

            Debug.WriteLine($"Total Fatura: {totalFatura}");
            Debug.WriteLine($"Total Fatura Crédito: {totalFaturaCredito}");
            Debug.WriteLine($"Total Fatura Débito: {totalFaturaDebito}");

            // Armazenando valores para exibição
            ViewBag.TotalFatura = totalFatura;
            ViewBag.TotalFaturaCredito = totalFaturaCredito;
            ViewBag.TotalFaturaDebito = totalFaturaDebito;
            ViewBag.log = log;

            //Gera o OFX baseado nas faturas
            var ofxGenerator = new Core.OfxGenerator();
            string conteudoOfx = ofxGenerator.GerarOfx(faturas, menorData, maiorData, totalFatura);

            //Faz o processamento no arquivo
            nomeArquivoOfx = "brbCard.ofx";
            var caminhoArquivoOfx = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeArquivoOfx);
            System.IO.File.WriteAllText(caminhoArquivoOfx, conteudoOfx);
        }

        return View("DownloadOfx", nomeArquivoOfx);
    }
}
