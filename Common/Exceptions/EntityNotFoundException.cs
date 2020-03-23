using System;

namespace JinCreek.Server.Common.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException() : base()
        {
        }

        public EntityNotFoundException(string name) : base(name)
        {
        }
    }
}
