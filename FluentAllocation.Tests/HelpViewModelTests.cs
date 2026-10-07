using FluentAllocation.ViewModels;
using Xunit;

namespace FluentAllocation.Tests
{
    public class HelpViewModelTests
    {
        [Fact]
        public void ItStartsOnTheFirstStepWithOnlyNextOpen()
        {
            var help = new HelpViewModel(3);

            Assert.Equal(0, help.CurrentStep);
            Assert.False(help.BackCommand.CanExecute(null));
            Assert.True(help.NextCommand.CanExecute(null));
        }

        [Fact]
        public void NextStopsAtTheLastStep()
        {
            var help = new HelpViewModel(3);

            help.NextCommand.Execute(null);
            help.NextCommand.Execute(null);

            Assert.Equal(2, help.CurrentStep);
            Assert.False(help.NextCommand.CanExecute(null));
            Assert.True(help.BackCommand.CanExecute(null));
        }

        // the pips pager writes the step directly, and the buttons have to follow
        [Fact]
        public void AJumpUpdatesBothButtons()
        {
            var help = new HelpViewModel(5);

            help.CurrentStep = 4;

            Assert.False(help.NextCommand.CanExecute(null));
            Assert.True(help.BackCommand.CanExecute(null));

            help.BackCommand.Execute(null);

            Assert.Equal(3, help.CurrentStep);
        }
    }
}
