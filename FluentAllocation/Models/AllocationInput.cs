using System.Collections.Generic;

namespace FluentAllocation.Models
{
    // everything the source workbook holds, in source order
    public sealed class AllocationInput
    {
        public AllocationInput(IReadOnlyList<Participant> participants, IReadOnlyList<Option> options, IReadOnlyList<string> attributeNames)
        {
            Participants = participants;
            Options = options;
            AttributeNames = attributeNames;
        }

        public IReadOnlyList<Participant> Participants { get; }
        public IReadOnlyList<Option> Options { get; }
        public IReadOnlyList<string> AttributeNames { get; } // in column order
    }
}
