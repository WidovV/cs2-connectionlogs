using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionLogs
{
    internal class User
    {
        public int Id { get; set; }
        public string SteamId { get; set; }
        public string ClientName { get; set; }
        public DateTime ConnectedAt { get; set; }
    }
}
