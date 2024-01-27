using System.Text;
using Core;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace Infrastructure
{
    public class PdfReaderService
    {
        private readonly ProcessadorFatura _processadorFatura;

        public PdfReaderService(ProcessadorFatura processadorFatura)
        {
            _processadorFatura = processadorFatura;
        }

        public List<Fatura> ProcessarFaturasDoPdf(string caminhoPdf)
        {
            string textoExtraido = ExtrairTextoDoPdf(caminhoPdf);
            return _processadorFatura.ProcessarTextoPdf(textoExtraido);
        }

        private string ExtrairTextoDoPdf(string caminhoPdf)
        {
            using (PdfReader reader = new PdfReader(caminhoPdf))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                StringBuilder texto = new StringBuilder();

                for (int i = 2; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    texto.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
                }

                return texto.ToString();
            }
        }
    }
}