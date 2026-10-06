using System;
using System.IO;
using System.Linq;
using FluentAllocation.Engines;
using FluentAllocation.Models;
using Xunit;

namespace FluentAllocation.Tests
{
    // the two workbooks in Samples/, run through with many seeds against what has to hold for any lottery
    public class SampleWorkbookTests
    {
        public static TheoryData<string, int, int, int, int, int> Samples => new()
        {
            // file, participants, options, attributes, priorities, options per participant
            { "employees-and-vacation-weeks.xlsx", 40, 8, 1, 4, 2 },
            { "students-and-courses.xlsx", 60, 8, 1, 4, 2 }
        };

        [Theory]
        [MemberData(nameof(Samples))]
        public void EveryRunKeepsTheRules(string file, int participants, int options, int attributes, int priorities, int perParticipant)
        {
            var request = new AllocationRequest
            {
                SourcePath = Path.Combine(AppContext.BaseDirectory, "Samples", file),
                ResultPath = "unused.xlsx",
                ParticipantCount = participants,
                OptionCount = options,
                AttributeCount = attributes,
                PriorityCount = priorities,
                OptionsPerParticipant = perParticipant
            };
            AllocationInput input = SourceWorkbookReader.Read(request.SourcePath, request);

            for (int seed = 0; seed < 50; seed++)
            {
                AllocationResult result = AllocationEngine.Allocate(input, priorities, perParticipant, new Random(seed));

                // no option over its capacity
                foreach (Option option in input.Options)
                {
                    Assert.True(result.Assignments.Values.Count(given => given.Contains(option)) <= option.Capacity);
                }

                foreach (Participant participant in input.Participants)
                {
                    int[] given = result.Assignments[participant].Select(option => option.Id).ToArray();

                    // only what was chosen, never twice, in priority order, never more than needed
                    Assert.All(given, id => Assert.Contains(id, participant.Priorities));
                    Assert.Equal(given.Distinct(), given);
                    Assert.Equal(participant.Priorities.Where(given.Contains), given);
                    Assert.True(given.Length <= perParticipant);
                    Assert.Equal(given.Length < perParticipant, result.Unallocated.Contains(participant));
                }
            }
        }
    }
}
