using MES_Link.MainUI.ViewModels;
using Xunit;

namespace MES_Link.Tests
{
    public class MES_LinkViewModelTests_Fake
    {
        // 按下切換鈕、原本停止 → 執行後 IsServerRunning 要變 true
        [Fact]
        public void ToggleServerCommand_WhenStopped_StartsServer()
        {
            var fakeService = new FakeMesSimulatorService();
            var viewModel = new MES_LinkViewModel(fakeService, new FakeLoggerService(), new FakeDialogService());

            viewModel.ToggleServerCommand.Execute(null);

            Assert.True(viewModel.IsServerRunning);
        }

        // 連續按兩次(啟動再關閉)→ 最後 IsServerRunning 要變回 false
        [Fact]
        public void ToggleServerCommand_WhenRunning_StopsServer()
        {
            var fakeService = new FakeMesSimulatorService();
            var viewModel = new MES_LinkViewModel(fakeService, new FakeLoggerService(), new FakeDialogService());

            viewModel.ToggleServerCommand.Execute(null);
            viewModel.ToggleServerCommand.Execute(null);

            Assert.False(viewModel.IsServerRunning);
        }
    }
}