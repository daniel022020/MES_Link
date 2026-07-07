using System;

namespace MES_Link.Interfaces
{
    public interface ILoggerService
    {
        void Info(string message);
        void Error(string message);
        void Error(Exception ex, string message);
        void Fatal(Exception ex, string message);
    }
}