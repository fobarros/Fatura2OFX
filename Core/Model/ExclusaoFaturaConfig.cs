namespace Core;

public class LoteConfig
{
    public string LabelInicial { get; set; } = string.Empty;
    public string LabelFinal { get; set; } = string.Empty;
}

public class IndividualConfig
{
    public string Data { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Valor { get; set; } = 0;
}

public class ExclusoesFaturaConfig
{
    public List<LoteConfig> Lote { get; set; } = new List<LoteConfig>();
    public List<IndividualConfig> Individuais { get; set; } = new List<IndividualConfig>();
}

public class AppSettings
{
    public ExclusoesFaturaConfig ExclusoesFatura { get; set; } = new ExclusoesFaturaConfig();
}
