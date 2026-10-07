namespace FluentAllocation.Models
{
    public enum ResultTarget
    {
        NewFile,
        ExistingFile
    }

    // what the allocation form hands over; the counts decide how many rows and columns are read
    public sealed record AllocationRequest
    {
        public required string SourcePath { get; init; }
        public required string ResultPath { get; init; }
        public ResultTarget Target { get; init; } = ResultTarget.NewFile;

        public int ParticipantCount { get; init; }
        public int OptionCount { get; init; }
        public int AttributeCount { get; init; }
        public int PriorityCount { get; init; }
        public int OptionsPerParticipant { get; init; }

        // null when the result is not grouped
        public string? GroupAttribute { get; init; }
    }
}
