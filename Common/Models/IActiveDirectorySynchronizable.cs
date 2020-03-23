using System;
using System.Collections.Generic;
using System.Text;

namespace JinCreek.Server.Common.Models
{
    public interface IActiveDirectorySynchronizable
    {
        public Guid AdObjectId { get; set; }
    }
}
