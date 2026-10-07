using Microsoft.UI.Xaml;

namespace FluentAllocation.Views
{
    // shows a help step only while it is the current one; a function binding, so the view model hands over
    // an index and never a WinUI type
    public static class HelpSteps
    {
        public static Visibility Show(int currentStep, int step)
        {
            return currentStep == step ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
