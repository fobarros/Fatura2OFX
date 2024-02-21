using System;
using System.Globalization;
using System.Text;

namespace Core{
    public class OfxGenerator
    {
        public string GerarOfx(List<Fatura> faturas, DateTime dtStart, DateTime dtEnd, decimal faturaTotal)
        {
            var dtDateNow = DateTime.Now.ToString("yyyyMMddHHmmss");
            var sb = new StringBuilder();
            sb.AppendLine("OFXHEADER:100");
            sb.AppendLine("DATA:OFXSGML");
            sb.AppendLine("VERSION:102");
            sb.AppendLine("SECURITY:NONE");
            sb.AppendLine("ENCODING:USASCII");
            sb.AppendLine("CHARSET:1252");
            sb.AppendLine("COMPRESSION:NONE");
            sb.AppendLine("OLDFILEUID:NONE");
            sb.AppendLine("NEWFILEUID:NONE");
            sb.AppendLine();
            sb.AppendLine("<OFX>");
            sb.AppendLine("    <SIGNONMSGSRSV1>");
            sb.AppendLine("        <SONRS>");
            sb.AppendLine("            <STATUS>");
            sb.AppendLine("                <CODE>0</CODE>");
            sb.AppendLine("                <SEVERITY>INFO</SEVERITY>");
            sb.AppendLine("            </STATUS>");
            sb.AppendLine("            <DTSERVER>" + dtDateNow + "</DTSERVER>");
            sb.AppendLine("            <LANGUAGE>POR</LANGUAGE>");
            sb.AppendLine("            <FI>");
            sb.AppendLine("                <ORG>HAN</ORG>");
            sb.AppendLine("                <FID>6805</FID>");
            sb.AppendLine("            </FI>");
            sb.AppendLine("        </SONRS>");
            sb.AppendLine("    </SIGNONMSGSRSV1>");
            sb.AppendLine("    <BANKMSGSRSV1>");
            sb.AppendLine("        <STMTTRNRS>");
            sb.AppendLine("            <TRNUID>1</TRNUID>");
            sb.AppendLine("            <STATUS>");
            sb.AppendLine("                <CODE>0</CODE>");
            sb.AppendLine("                <SEVERITY>INFO</SEVERITY>");
            sb.AppendLine("            </STATUS>");
            sb.AppendLine("            <STMTRS>");
            sb.AppendLine("                <CURDEF>BRL</CURDEF>");
            sb.AppendLine("                <BANKACCTFROM>");
            sb.AppendLine("                    <BANKID>BRB</BANKID>");
            sb.AppendLine("                    <ACCTID>1234</ACCTID>");
            sb.AppendLine("                    <ACCTTYPE>CHECKING</ACCTTYPE>");
            sb.AppendLine("                </BANKACCTFROM>");
            sb.AppendLine("                <BANKTRANLIST>");
            sb.AppendLine($"                       <DTSTART>{dtStart.ToString("yyyyMMddHHmmss")}</DTSTART>");
            sb.AppendLine($"                       <DTEND>{dtEnd.ToString("yyyyMMddHHmmss")}</DTEND>");
            foreach (var fatura in faturas.OrderBy(fatura => fatura.DTPOSTED).ToList())
            {
                sb.AppendLine("                    <STMTTRN>");
                sb.AppendLine($"                        <TRNTYPE>{fatura.TRNTYPE}</TRNTYPE>");
                sb.AppendLine($"                        <DTPOSTED>{fatura.DTPOSTED}</DTPOSTED>");
                sb.AppendLine($"                        <TRNAMT>{fatura.TRNAMT}</TRNAMT>");
                sb.AppendLine($"                        <FITID>{fatura.FITID}</FITID>");
                sb.AppendLine($"                        <NAME>{fatura.NAME}</NAME>");
                sb.AppendLine("                    </STMTTRN>");
            }
            sb.AppendLine("                </BANKTRANLIST>");
            sb.AppendLine("                <LEDGERBAL>");
            sb.AppendLine($"                       <BALAMT>{faturaTotal.ToString("F2", CultureInfo.InvariantCulture)}</BALAMT>");
            sb.AppendLine($"                       <DTASOF>{dtDateNow}</DTASOF>");
            sb.AppendLine("                </LEDGERBAL>");
            sb.AppendLine("            </STMTRS>");
            sb.AppendLine("        </STMTTRNRS>");
            sb.AppendLine("    </BANKMSGSRSV1>");
            sb.AppendLine("</OFX>");

            return sb.ToString();
        }
    }
}