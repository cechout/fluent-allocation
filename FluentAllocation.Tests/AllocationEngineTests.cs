using System;
using System.Collections.Generic;
using System.Linq;
using FluentAllocation.Engines;
using FluentAllocation.Models;
using Xunit;
using static FluentAllocation.Tests.Workbooks;

namespace FluentAllocation.Tests
{
    public class AllocationEngineTests
    {
        private static (AllocationInput Input, AllocationResult Result) Allocate(object?[][] participants, object?[][] options, int optionsPerParticipant = 1, int seed = 1)
        {
            using var workbook = Source(participants, options);
            AllocationRequest request = Request(participants, options, optionsPerParticipant: optionsPerParticipant);
            AllocationInput input = SourceWorkbookReader.Read(workbook, request);

            return (input, AllocationEngine.Allocate(input, request.PriorityCount, optionsPerParticipant, new Random(seed)));
        }

        private static string[] OptionsOf(AllocationInput input, AllocationResult result, int participantId)
        {
            Participant participant = input.Participants.Single(candidate => candidate.Id == participantId);
            return result.Assignments[participant].Select(option => option.Name.Text).ToArray();
        }


        [Fact]
        public void EveryoneGetsTheirFirstChoiceWhenItFits()
        {
            var (input, result) = Allocate(
                new[] { Row(1, "Ada", 1, 2), Row(2, "Ben", 2, 1), Row(3, "Cy", 1, 2) },
                new[] { Row(1, "Art", 2), Row(2, "Music", 1) });

            Assert.Equal(new[] { "Art" }, OptionsOf(input, result, 1));
            Assert.Equal(new[] { "Music" }, OptionsOf(input, result, 2));
            Assert.Equal(new[] { "Art" }, OptionsOf(input, result, 3));
            Assert.Empty(result.Unallocated);
        }

        [Fact]
        public void AnOversubscribedOptionTakesExactlyItsCapacityAndTheRestMoveOn()
        {
            var (input, result) = Allocate(
                new[] { Row(1, "Ada", 1, 2), Row(2, "Ben", 1, 2), Row(3, "Cy", 1, 2), Row(4, "Di", 1, 2) },
                new[] { Row(1, "Art", 2), Row(2, "Music", 2) });

            Assert.Equal(2, result.Assignments.Values.Count(options => options.Single().Name.Text == "Art"));
            Assert.Equal(2, result.Assignments.Values.Count(options => options.Single().Name.Text == "Music"));
            Assert.Empty(result.Unallocated);
        }

        [Fact]
        public void TheLotteryFollowsTheSeed()
        {
            object?[][] participants = Enumerable.Range(1, 10).Select(id => Row(id, $"P{id}", 1)).ToArray();
            object?[][] options = { Row(1, "Art", 3) };

            var first = Allocate(participants, options, seed: 42);
            var second = Allocate(participants, options, seed: 42);

            int[] Winners((AllocationInput Input, AllocationResult Result) run) =>
                run.Input.Participants.Where(participant => run.Result.Assignments[participant].Count == 1).Select(participant => participant.Id).ToArray();

            Assert.Equal(Winners(first), Winners(second));
            Assert.Equal(3, Winners(first).Length);
        }

        // over enough draws every candidate has to win sometimes and lose sometimes
        [Fact]
        public void TheLotteryCanPickAnyCandidate()
        {
            object?[][] participants = Enumerable.Range(1, 5).Select(id => Row(id, $"P{id}", 1)).ToArray();
            object?[][] options = { Row(1, "Art", 2) };
            var wins = new Dictionary<int, int>();

            for (int seed = 0; seed < 500; seed++)
            {
                var (input, result) = Allocate(participants, options, seed: seed);
                foreach (Participant participant in input.Participants.Where(participant => result.Assignments[participant].Count == 1))
                {
                    wins[participant.Id] = wins.GetValueOrDefault(participant.Id) + 1;
                }
            }

            Assert.Equal(5, wins.Count);
            Assert.All(wins.Values, count => Assert.InRange(count, 120, 280));
        }

        [Fact]
        public void AFullOptionSendsTheParticipantToTheNextPriority()
        {
            var (input, result) = Allocate(
                new[] { Row(1, "Ada", 1, 3), Row(2, "Ben", 3, 2) },
                new[] { Row(1, "Art", 0), Row(2, "Music", 1), Row(3, "Sport", 1) });

            // Ben takes the only Sport seat on priority 1, so Ada finds both of hers full
            Assert.Equal(new[] { "Sport" }, OptionsOf(input, result, 2));
            Assert.Empty(OptionsOf(input, result, 1));
            Assert.Equal(new[] { 1 }, result.Unallocated.Select(participant => participant.Id));
        }

        [Fact]
        public void ThereIsNoFallbackIntoOptionsNobodyChose()
        {
            var (_, result) = Allocate(
                new[] { Row(1, "Ada", 1), Row(2, "Ben", 1) },
                new[] { Row(1, "Art", 1), Row(2, "Music", 10) });

            Assert.Single(result.Unallocated);
        }

        [Fact]
        public void SeveralOptionsPerParticipantComeInPriorityOrder()
        {
            var (input, result) = Allocate(
                new[] { Row(1, "Ada", 3, 1, 2) },
                new[] { Row(1, "Art", 1), Row(2, "Music", 1), Row(3, "Sport", 1) },
                optionsPerParticipant: 2);

            Assert.Equal(new[] { "Sport", "Art" }, OptionsOf(input, result, 1));
        }

        [Fact]
        public void TooFewPrioritiesLeaveAParticipantUnallocatedWithWhatTheyGot()
        {
            var (input, result) = Allocate(
                new[] { Row(1, "Ada", 1) },
                new[] { Row(1, "Art", 1), Row(2, "Music", 1) },
                optionsPerParticipant: 2);

            Assert.Equal(new[] { "Art" }, OptionsOf(input, result, 1));
            Assert.Single(result.Unallocated);
        }

        // version 1 stopped at the id of the last options row, so an unsorted sheet lost every option above it
        [Fact]
        public void AnUnsortedOptionsSheetIsAllocatedCompletely()
        {
            var (input, result) = Allocate(
                new[] { Row(1, "Ada", 9), Row(2, "Ben", 4) },
                new[] { Row(9, "Art", 1), Row(4, "Music", 1) });

            Assert.Equal(new[] { "Art" }, OptionsOf(input, result, 1));
            Assert.Equal(new[] { "Music" }, OptionsOf(input, result, 2));
        }

        [Fact]
        public void NoOptionsPerParticipantMeansNobodyNeedsAnything()
        {
            var (_, result) = Allocate(new[] { Row(1, "Ada", 1) }, new[] { Row(1, "Art", 1) }, optionsPerParticipant: 0);

            Assert.Empty(result.Assignments.Values.SelectMany(options => options));
            Assert.Empty(result.Unallocated);
        }
    }
}
