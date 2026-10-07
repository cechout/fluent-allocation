using System;

namespace FluentAllocation.Models
{
    // a failure the user can fix; the message is shown as it is
    public sealed class AllocationException : Exception
    {
        public AllocationException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
