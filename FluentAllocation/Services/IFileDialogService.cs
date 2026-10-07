using System.Threading.Tasks;

namespace FluentAllocation.Services
{
    // the file pickers, behind an interface so the view models stay free of WinUI;
    // every method returns the chosen path, or null when the dialog was cancelled
    public interface IFileDialogService
    {
        Task<string?> PickWorkbookToOpenAsync();
        Task<string?> PickWorkbookToSaveAsync();
    }
}
