using Core;

namespace BRBPresentation.Services
{
    public class ExclusaoFaturaService : IExclusaoFaturaService
    {
        private readonly ExclusoesFaturaConfig _exclusoesFatura;

        public ExclusaoFaturaService(IConfiguration configuration)
        {
            _exclusoesFatura = configuration
                .GetSection("ExclusoesFatura")
                .Get<ExclusoesFaturaConfig>() ?? new ExclusoesFaturaConfig();
        }

        public ExclusoesFaturaConfig ObterExclusoesFatura()
        {
            return _exclusoesFatura;
        }
    }
}
