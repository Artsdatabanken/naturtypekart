using System;

namespace Nin
{
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message)
        {
        }
    }
}