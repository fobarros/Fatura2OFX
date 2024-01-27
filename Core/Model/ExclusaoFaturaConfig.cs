namespace Core;
public class ExclusaoFaturaConfig
{
    public string LabelInicial { get; set; } = string.Empty;
    public string LabelFinal { get; set; } = string.Empty;
}

public class AppSettings
{
    public List<ExclusaoFaturaConfig> ExclusoesFatura { get; set; } = new List<ExclusaoFaturaConfig>();
}
