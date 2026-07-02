using System;
using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// Ponto ÚNICO para normalizar descrições de marketplaces.
    ///
    /// Algumas descrições vêm no formato "PREFIXO*IDENTIFICADOR"
    /// (ex.: "MERCADO*MERCADOLIVRE", "SHOPE *BYKGF"). Nesses casos o nome do
    /// recebedor (NAME) deve virar um nome amigável e o que vem depois do "*"
    /// deve ir para o MEMO.
    ///
    /// ATENÇÃO: isso NÃO é regra geral. Só se aplica aos prefixos listados abaixo.
    /// Ex.: "PAGAMENTO*FREGNI" NÃO casa (PAGAMENTO não é o nome do recebedor) e
    /// portanto é ignorado, mantendo o comportamento original.
    ///
    /// Para adicionar um novo marketplace, basta incluir uma linha no dicionário.
    /// </summary>
    public static class MarketplaceNormalizer
    {
        // Prefixo (o que vem antes do '*', já com Trim) -> nome amigável do recebedor.
        // A comparação é case-insensitive.
        private static readonly Dictionary<string, string> Marketplaces =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["MERCADO"] = "MERCADO LIVRE",
                ["SHOPE"] = "SHOPEE",
            };

        /// <summary>
        /// Tenta normalizar a descrição de um marketplace conhecido.
        /// Retorna true e preenche <paramref name="nome"/> (nome amigável) e
        /// <paramref name="memo"/> (texto após o "*") quando o prefixo é conhecido.
        /// Caso contrário retorna false e não altera a descrição.
        /// </summary>
        public static bool TryNormalizar(string descricao, out string nome, out string memo)
        {
            nome = descricao;
            memo = string.Empty;

            if (string.IsNullOrWhiteSpace(descricao))
                return false;

            var idx = descricao.IndexOf('*');
            if (idx < 0)
                return false;

            var prefixo = descricao.Substring(0, idx).Trim();

            if (Marketplaces.TryGetValue(prefixo, out var nomeAmigavel))
            {
                nome = nomeAmigavel;
                memo = descricao.Substring(idx + 1).Trim();
                return true;
            }

            return false;
        }
    }
}
