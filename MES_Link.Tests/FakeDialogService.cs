using MES_Link.Interfaces;

namespace MES_Link.Tests
{
    public class FakeDialogService : IDialogService
    {
        public void ShowError(string message, string title) { }
        public bool Confirm(string message, string title) => true;
    }
}