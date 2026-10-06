using System.Collections.Generic;

namespace FluentAllocation.Models
{
    // who got what
    public sealed class AllocationResult
    {
        public AllocationResult(IReadOnlyDictionary<Participant, IReadOnlyList<Option>> assignments, IReadOnlyList<Participant> unallocated)
        {
            Assignments = assignments;
            Unallocated = unallocated;
        }

        // every participant, with the options in the order they were given
        public IReadOnlyDictionary<Participant, IReadOnlyList<Option>> Assignments { get; }

        // the participants who still lack options after the last priority, in source order
        public IReadOnlyList<Participant> Unallocated { get; }
    }
}
