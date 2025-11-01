using Infrastructure;
using System.Collections.Generic;

namespace BRBPresentation.Models
{
    public class ReportViewModel
    {
        public required List<Core.Fatura> Faturas { get; set; }
        public decimal TotalFatura { get; set; }
        public decimal TotalFaturaCredito { get; set; }
        public decimal TotalFaturaDebito { get; set; }
    }
} 