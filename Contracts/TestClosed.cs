using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public record TestClosed(Guid Id, Guid CorrelationId);
    public record TestIntegrationRequestSent(Guid id) { };

}
