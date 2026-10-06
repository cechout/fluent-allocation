using System;
using System.IO;
using FluentAllocation.Models;

namespace FluentAllocation.Engines
{
    // one run, start to end: check the request, read the source, allocate, write the result
    // everything a user can fix is caught before the result file is touched
    public static class AllocationPipeline
    {
        public static AllocationResult Run(AllocationRequest request, Random random)
        {
            Validate(request);

            AllocationInput input = SourceWorkbookReader.Read(request.SourcePath, request);
            if (request.GroupAttribute != null) ResultWorkbookWriter.ResolveAttribute(input, request.GroupAttribute);

            AllocationResult result = AllocationEngine.Allocate(input, request.PriorityCount, request.OptionsPerParticipant, random);
            ResultWorkbookWriter.Write(request.ResultPath, input, result, request);

            return result;
        }

        private static void Validate(AllocationRequest request)
        {
            if (request.GroupAttribute != null && string.IsNullOrWhiteSpace(request.GroupAttribute))
            {
                throw new AllocationException("Enter the name of the attribute to group by.");
            }

            if (string.Equals(Path.GetFullPath(request.SourcePath), Path.GetFullPath(request.ResultPath), StringComparison.OrdinalIgnoreCase))
            {
                throw new AllocationException("The result workbook cannot be the source workbook, it would overwrite the source.");
            }
        }
    }
}
