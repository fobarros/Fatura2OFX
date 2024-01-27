using Core;

namespace BRBPresentation.Services
{
    public class ExclusaoFaturaService : IExclusaoFaturaService
    {
        private readonly List<ExclusaoFaturaConfig> _exclusoesFatura;

        public ExclusaoFaturaService(IConfiguration configuration)
        {
            _exclusoesFatura = configuration
                .GetSection("ExclusoesFatura")
                .Get<List<ExclusaoFaturaConfig>>() ?? new List<ExclusaoFaturaConfig>();
        }

        public List<ExclusaoFaturaConfig> ObterExclusoesFatura()
        {
            return _exclusoesFatura;
        }
    }
}
