using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Sample.ConsoleApp.Helpers
{
    class ProcessedUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}
