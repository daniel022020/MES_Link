using MES_Link.Interfaces.MES_Link.Interfaces;
using MES_Link.MainUI.ViewModels;
using MES_Link.MesSimulator;
using Moq;
using System.Collections.ObjectModel;
using Xunit;

namespace MES_Link.Tests
{
    public class MES_LinkViewModelTests_Moq
    {
        // 按下切換鈕後,驗證 IMesSimulatorService.Start() 這個方法真的有被呼叫過一次
        [Fact]
        public void ToggleServerCommand_WhenStopped_CallsStart()
        {
            // Arrange:建立一個假的 IMesSimulatorService
            var mockService = new Mock<IMesSimulatorService>();
            mockService.Setup(s => s.Routes).Returns(new ObservableCollection<MesRouteBlock>());
            mockService.Setup(s => s.RoutesLock).Returns(new object());
            mockService.Setup(s => s.IsRunning).Returns(false);
            mockService.Setup(s => s.Start()).Returns(true);

            var viewModel = new MES_LinkViewModel(mockService.Object, new FakeLoggerService(), new FakeDialogService());


            // Act
            viewModel.ToggleServerCommand.Execute(null);

            // Assert:驗證「Start() 真的被呼叫過一次」
            mockService.Verify(s => s.Start(), Times.Once);
        }
    }
}