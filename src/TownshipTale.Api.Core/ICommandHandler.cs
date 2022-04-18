using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TownshipTale.Api.Core
{
    public interface ICommandHandler<in TCommand>
        where TCommand : Command
    {
        void Handle(TCommand command);
    }
}
