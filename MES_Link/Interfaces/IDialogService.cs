namespace MES_Link.Interfaces
{
    public interface IDialogService
    {
        void ShowError(string message, string title);
        bool Confirm(string message, string title);
    }
}