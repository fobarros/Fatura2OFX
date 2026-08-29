using System;
using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// Ponto ÚNICO para normalizar descrições de recebedores conhecidos.
    ///
    /// Dois tipos de regra (ambas explícitas — NÃO é regra geral):
    ///
    /// 1) Prefixo*identificador  → NAME = nome amigável, MEMO = texto após o '*'
    ///    Ex.: "UPS*29CDF905747" → NAME "UPS", MEMO "29CDF905747"
    ///    Ex.: "PAGAMENTO*FREGNI" NÃO casa (prefixo não listado) e fica inalterado.
    ///
    /// 2) Começa com texto conhecido → NAME = nome amigável, MEMO = resto da descrição
    ///    Ex.: "DROGARIASP-74" → NAME "DROGARIA_SP", MEMO "74"
    ///    Ex.: "10 CARTOES 289EKF" → NAME "Zona Azul", MEMO "289EKF"
    ///
    /// Para adicionar: uma linha no dicionário correspondente.
    /// </summary>
    public static class MarketplaceNormalizer
    {
        // Prefixo (antes do '*', já com Trim) -> nome amigável. Case-insensitive.
        private static readonly Dictionary<string, string> PrefixosComAsterisco =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["MERCADO"] = "MERCADO LIVRE",
                ["MERCADOLIVRE"] = "MERCADO LIVRE",
                ["MP"] = "MERCADO LIVRE",
                ["SHOPE"] = "SHOPEE",
                ["UPS"] = "UPS",
                ["99FOD"] = "iFood",
                ["IFD"] = "iFood",
            };

        // Descrição começa com (case-insensitive) -> nome amigável.
        // Ordem importa: entradas mais específicas primeiro.
        private static readonly (string Prefixo, string Nome)[] PrefixosSimples =
        {
            ("DROGARIA_SP", "DROGARIA_SP"),
            ("DROGARIASP", "DROGARIA_SP"),
            ("10 CARTOES", "Zona Azul"),
            ("PAGUE MENOS", "PAGUE MENOS"),
        };

        /// <summary>
        /// Tenta normalizar a descrição. Retorna true e preenche
        /// <paramref name="nome"/> / <paramref name="memo"/> quando casa com
        /// alguma regra conhecida. Caso contrário retorna false.
        /// </summary>
        public static bool TryNormalizar(string descricao, out string nome, out string memo)
        {
            nome = descricao;
            memo = string.Empty;

            if (string.IsNullOrWhiteSpace(descricao))
                return false;

            // 1) Prefixo*identificador
            var idx = descricao.IndexOf('*');
            if (idx >= 0)
            {
                var prefixo = descricao.Substring(0, idx).Trim();
                if (PrefixosComAsterisco.TryGetValue(prefixo, out var nomeAmigavel))
                {
                    nome = nomeAmigavel;
                    memo = descricao.Substring(idx + 1).Trim();
                    return true;
                }
            }

            // 2) Começa com texto conhecido
            foreach (var (prefixo, nomeAmigavel) in PrefixosSimples)
            {
                if (descricao.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase))
                {
                    nome = nomeAmigavel;
                    memo = descricao.Substring(prefixo.Length).Trim().TrimStart('-', ' ').Trim();
                    return true;
                }
            }

            return false;
        }
    }
}
