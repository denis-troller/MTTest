using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public record TestIntegrationRequest(Guid TestId, Guid CorrelationId)
    {
    }

    public record TestIntegrationStarted(Guid TestId, Guid CorrelationId) { }

    public record TestIntegrated(Guid TestId, Guid CorrelationId) { }
    public record TestRejected(Guid TestId, Guid CorrelationId) { }
}
