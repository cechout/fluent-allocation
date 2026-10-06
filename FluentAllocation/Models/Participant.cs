using System.Collections.Generic;

namespace FluentAllocation.Models
{
    // one row of sheet 1
    public sealed class Participant
    {
        public Participant(int id, SheetValue name, IReadOnlyDictionary<string, SheetValue> attributes, IReadOnlyList<int> priorities)
        {
            Id = id;
            Name = name;
            Attributes = attributes;
            Priorities = priorities;
        }

        public int Id { get; }
        public SheetValue Name { get; }

        // keyed by the attribute name from row 1, case-insensitive
        public IReadOnlyDictionary<string, SheetValue> Attributes { get; }

        // option ids, priority 1 first, with the skipped cells already taken out
        public IReadOnlyList<int> Priorities { get; }
    }
}
