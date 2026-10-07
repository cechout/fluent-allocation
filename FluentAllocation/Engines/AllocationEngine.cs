using System;
using System.Collections.Generic;
using System.Linq;
using FluentAllocation.Models;

namespace FluentAllocation.Engines
{
    // the allocation:
    // walks the priorities from the top; per priority every option in ascending id order takes the participants
    // who still need an option and put it on that priority, and a lottery decides wherever they do not all fit
    //
    // there is no fallback into free seats of options a participant did not choose; whoever still lacks options
    // after the last priority is unallocated
    public static class AllocationEngine
    {
        // random is handed in, so a test can seed it; the app passes an unseeded one
        public static AllocationResult Allocate(AllocationInput input, int priorityCount, int optionsPerParticipant, Random random)
        {
            var assignments = input.Participants.ToDictionary(participant => participant, _ => new List<Option>());
            var seatsLeft = input.Options.ToDictionary(option => option, option => option.Capacity);
            List<Option> optionsById = input.Options.OrderBy(option => option.Id).ToList();

            bool StillNeeds(Participant participant) => assignments[participant].Count < optionsPerParticipant;

            for (int rank = 0; rank < priorityCount; rank++)
            {
                foreach (Option option in optionsById)
                {
                    if (!input.Participants.Any(StillNeeds)) break;
                    if (seatsLeft[option] == 0) continue;

                    List<Participant> candidates = input.Participants
                        .Where(participant => StillNeeds(participant)
                            && rank < participant.Priorities.Count
                            && participant.Priorities[rank] == option.Id
                            && !assignments[participant].Contains(option))
                        .ToList();

                    if (candidates.Count > seatsLeft[option]) candidates = Draw(candidates, seatsLeft[option], random);

                    foreach (Participant participant in candidates) assignments[participant].Add(option);
                    seatsLeft[option] -= candidates.Count;
                }
            }

            List<Participant> unallocated = input.Participants.Where(StillNeeds).ToList();

            return new AllocationResult(
                assignments.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<Option>)pair.Value),
                unallocated);
        }

        // count of them, each equally likely; a partial shuffle, which leaves the source list alone
        private static List<Participant> Draw(List<Participant> candidates, int count, Random random)
        {
            var pool = new List<Participant>(candidates);

            for (int index = 0; index < count; index++)
            {
                int pick = random.Next(index, pool.Count);
                (pool[index], pool[pick]) = (pool[pick], pool[index]);
            }

            return pool.GetRange(0, count);
        }
    }
}
