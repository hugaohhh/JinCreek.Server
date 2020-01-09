using System;
using System.Collections.Generic;

namespace JinCreek.Server.Common.Models
{
    public partial class Radusergroup
    {
        public uint Id { get; set; }
        public string Username { get; set; }
        public string Groupname { get; set; }
        public int Priority { get; set; }
    }
}
