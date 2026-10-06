namespace FluentAllocation.Models
{
    // one of the things participants compete for, as read from sheet 2
    public sealed class Option
    {
        public Option(int id, SheetValue name, int capacity)
        {
            Id = id;
            Name = name;
            Capacity = capacity;
        }

        public int Id { get; }
        public SheetValue Name { get; }
        public int Capacity { get; } // seats as the workbook gives them; the engine counts down a copy
    }
}
