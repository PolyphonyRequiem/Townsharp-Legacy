using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Townsharp.Consoles
{
    public record class ConsoleAccessResult(bool IsOnline, Uri? Endpoint, string Token);
}
