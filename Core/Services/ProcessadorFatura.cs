using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Core
{
    public class ProcessadorFatura
    {
        private readonly IExclusaoFaturaService _exclusaoFaturaService;
        private readonly DateTime _dataInicioFatura;
        private readonly Boolean _pulaLinha;
        private string _usuarioAtual = "";
        private string _cartaoAtual = "";

        public ProcessadorFatura(IExclusaoFaturaService exclusaoFaturaService, DateTime dataInicioFatura, Boolean pulaLinha)
        {
            _exclusaoFaturaService = exclusaoFaturaService;
            _dataInicioFatura = dataInicioFatura;
            _pulaLinha = pulaLinha;
        }

        private (List<Fatura>, string) LimpaFaturadeOutros(List<Fatura> faturas)
        {
            var exclusoes = _exclusaoFaturaService.ObterExclusoesFatura();
            var novaFatura  = new List<Fatura>();
            var log = new StringBuilder();

            for (int i = 0; i < faturas.Count; i++)
            {
                var faturaWasRemoved = false;
                //exclusao individual
                foreach (var individual in exclusoes.Individuais)
                {
                    if (faturas[i].NAME.Contains(individual.Label) && faturas[i].AMOUNT_VALUE == individual.Valor && faturas[i].DTPOSTED_ORIGINAL == individual.Data)
                    {
                        log.AppendLine($"Unit Removed: {faturas[i].NAME}, vl: {individual.Valor}, data: {individual.Data}");
                        faturaWasRemoved = true;
                        break;
                    }
                }
                if (!faturaWasRemoved)
                    novaFatura.Add(faturas[i]);
            }

            faturas = new List<Fatura>(novaFatura);

            // Aplicar a lógica de exclusão baseada em labels
            foreach (var exclusao in exclusoes.Lote)
            {
                for (int i = 0; i < faturas.Count; i++)
                {
                    //exclusao em lote
                    if (faturas[i].NAME.Contains(exclusao.LabelInicial))
                    {
                        log.AppendLine($"Bulk Removed: {faturas[i].NAME}");
                        // Remover a fatura atual (com LabelInicial)
                        faturas.RemoveAt(i);

                        // Pular a próxima fatura (manter uma, remover a seguinte)
                        if(_pulaLinha)
                            i++;

                        // Continuar removendo até encontrar LabelFinal
                        bool removerProxima = true;
                        while (i < faturas.Count && !faturas[i].NAME.Contains(exclusao.LabelFinal))
                        {
                            if (removerProxima)
                            {
                                log.AppendLine($"Bulk Removed: {faturas[i].NAME}");
                                faturas.RemoveAt(i);
                            }
                            else if(_pulaLinha)
                            {
                                i++;
                            }
                            removerProxima = !removerProxima;
                        }

                        // Remover a fatura com LabelFinal e sair do loop interno
                        if (i < faturas.Count)
                        {
                            log.AppendLine($"Bulk Removed: {faturas[i].NAME}. Terminou looping");
                            faturas.RemoveAt(i);
                        }
                        break;
                    }
                }
            }

            return (faturas, log.ToString());
        }

        public (List<Fatura>, string) ProcessarTextoPdf(string texto)
        {
            var faturas = new List<Fatura>();
            var linhas = texto.Split('\n'); // Divide o texto em linhas
            var log = new StringBuilder();

            //log.AppendLine($"Linha: {texto}");
            log.AppendLine("=== Início do Processamento ===");
            log.AppendLine($"Total de linhas para processar: {linhas.Length}");

            foreach (var linha in linhas)
            {
                // Primeiro tenta identificar o cartão
                log.AppendLine($"Linha: {linha}");
                var cartaoMatch = Regex.Match(linha, @"\d{4}\.\*{4}\.\*{4}\.\d{4}");
                if (cartaoMatch.Success)
                {
                    _cartaoAtual = cartaoMatch.Value;
                    log.AppendLine($"Cartão identificado: {_cartaoAtual}");
                    log.AppendLine($"Linha completa do cartão: {linha}");

                    // Se encontrou cartão, procura pelo usuário na mesma linha
                    if (linha.Contains("FERNANDO O BARROS"))
                    {
                        _usuarioAtual = "FERNANDO O BARROS";
                        log.AppendLine($"Usuário identificado: {_usuarioAtual}");
                    }
                    else if (linha.Contains("ROBERTA BARROS"))
                    {
                        _usuarioAtual = "ROBERTA BARROS";
                        log.AppendLine($"Usuário identificado: {_usuarioAtual}");
                    }
                    else
                    {
                        log.AppendLine("Aviso: Cartão encontrado mas usuário não identificado na linha");
                    }
                    
                    continue;
                }

                // Se não tem usuário definido ainda, tenta identificar
                if (string.IsNullOrEmpty(_usuarioAtual))
                {
                    if (linha.Contains("FERNANDO O BARROS", StringComparison.OrdinalIgnoreCase))
                    {
                        _usuarioAtual = "FERNANDO O BARROS";
                        log.AppendLine($"Usuário identificado em linha separada: {_usuarioAtual}");
                        log.AppendLine($"Linha completa do usuário: {linha}");
                    }
                    else if (linha.Contains("ROBERTA BARROS", StringComparison.OrdinalIgnoreCase))
                    {
                        _usuarioAtual = "ROBERTA BARROS";
                        log.AppendLine($"Usuário identificado em linha separada: {_usuarioAtual}");
                        log.AppendLine($"Linha completa do usuário: {linha}");
                    }
                }

                if (LinhaValidaParaFatura(linha))
                {
                    log.AppendLine($"Processando linha de fatura: {linha}");
                    var faturasExtraidas = ExtrairFaturaDaLinha(linha);
                    faturas.AddRange(faturasExtraidas);
                }
            }

            // Se não encontrou usuário, define o padrão
            if (string.IsNullOrEmpty(_usuarioAtual))
            {
                _usuarioAtual = "FERNANDO O BARROS";
                log.AppendLine("Usuário não identificado, usando padrão: FERNANDO O BARROS");
            }

            log.AppendLine($"=== Fim do Processamento ===");
            log.AppendLine($"Total de faturas processadas: {faturas.Count}");
            log.AppendLine($"Usuário final: {_usuarioAtual}");
            log.AppendLine($"Cartão final: {_cartaoAtual}");

            var (_faturas, limpezaLog) = LimpaFaturadeOutros(faturas);
            log.Append(limpezaLog);

            return (_faturas, log.ToString());
        }

        private bool LinhaValidaParaFatura(string linha)
        {
            // Ignora linhas que contêm "Obrigado pelo pagamento"
            if (linha.Contains("Obrigado pelo pagamento"))
                return false;

            // Regex que corresponde a linhas com data, texto opcional, e valor monetário (com ou sem separador de milhar)
            var linhaRegex = Regex.IsMatch(linha, @"\b\d{2}/\d{2}\b.*\bR\$ \d{1,3}(?:\.\d{3})*(?:,\d{2})?\b[\+\-]?");

            //if (linhaRegex)
            //Debug.WriteLine($"Linha: '{linha}' é {(linhaRegex ? "válida" : "inválida")}");

            return linhaRegex;
        }

        private List<Fatura> ExtrairFaturaDaLinha(string linha)
        {
            var faturas = new List<Fatura>();

            try
            {
                // Encontrar todas as datas na linha
                var datasRegex = new Regex(@"\d{2}/\d{2}");
                var datasMatches = datasRegex.Matches(linha);

                foreach (Match dataMatch in datasMatches)
                {
                    var indiceData = dataMatch.Index;
                    var dataString = dataMatch.Value;
                    var data = DateTime.ParseExact(dataString, "dd/MM", CultureInfo.InvariantCulture);

                    //Se o mes da data (ex: 10) for maior q da fatura + 1 mes (ex: 19/01 fica mes 02)
                    if (data.Month > _dataInicioFatura.AddMonths(1).Month)
                        data = _dataInicioFatura;
                    //Se mesmo mes. Se dia da fatura (ex: 19) > dia lido.
                    else if (data.Month == _dataInicioFatura.Month && _dataInicioFatura.Day > data.Day)
                        data = _dataInicioFatura;
                    //Meses anteriores assumem data da fatura
                    else if (data.Month <= _dataInicioFatura.Month)
                        data = _dataInicioFatura;

                    // Encontrar o índice do próximo valor monetário após a data
                    var valorRegex = new Regex(@"R\$ \d{1,3}(?:\.\d{3})*,\d{2}[\+\-]");
                    var valorMatch = valorRegex.Match(linha, indiceData);

                    if (!valorMatch.Success) continue;

                    var indiceValor = valorMatch.Index;
                    var valorString = valorMatch.Value;
                    var descricaoLength = indiceValor - indiceData - 5;
                    var descricao = linha.Substring(indiceData + 5, descricaoLength > 0 ? descricaoLength : 0).Trim();
                    var valorLimpo = valorString.Replace("R$ ", "").Replace(".", "").Replace(",", ".").Replace("+", "").Replace("-", "");
                    var valor = decimal.Parse(valorLimpo, CultureInfo.InvariantCulture);
                    var trnType = valorString.EndsWith("+") ? "DEBIT" : "CREDIT";

                    var fatura = new Fatura(trnType, valor, descricao, data, dataString, _usuarioAtual, _cartaoAtual);
                    faturas.Add(fatura);

                    Debug.WriteLine($"Fatura processada: Data: {data.ToShortDateString()}, Tipo: {trnType}, Valor: {valor}, Descrição: {descricao}, Usuário: {_usuarioAtual}, Cartão: {_cartaoAtual}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao processar a linha '{linha}': {ex.Message} - linha do código: {ex.StackTrace}");
            }

            return faturas;
        }
    }
}