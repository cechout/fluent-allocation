using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.Windows.Storage.Pickers;

namespace FluentAllocation.Services
{
    // the Windows App SDK pickers, which take the window id directly and so work in an unpackaged app
    public sealed class FileDialogService : IFileDialogService
    {
        // --- workbook filter ---
        private const string WorkbookExtension = ".xlsx";
        private const string WorkbookTypeName = "Excel Workbook";
        private const string NewWorkbookName = "Allocation";

        private readonly WindowId _windowId;

        public FileDialogService(WindowId windowId)
        {
            _windowId = windowId;
        }

        public async Task<string?> PickWorkbookToOpenAsync()
        {
            var picker = new FileOpenPicker(_windowId)
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(WorkbookExtension);

            PickFileResult? result = await picker.PickSingleFileAsync();
            return result?.Path;
        }

        // starts in Documents, as version 1 did; the picker asks before it lets an existing file be chosen
        public async Task<string?> PickWorkbookToSaveAsync()
        {
            var picker = new FileSavePicker(_windowId)
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = NewWorkbookName,
                DefaultFileExtension = WorkbookExtension
            };
            picker.FileTypeChoices.Add(WorkbookTypeName, new List<string> { WorkbookExtension });

            PickFileResult? result = await picker.PickSaveFileAsync();
            return result?.Path;
        }
    }
}
