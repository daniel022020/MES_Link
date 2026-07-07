using System;
using MES_Link.Interfaces;

namespace MES_Link.Log
{
    public class NLogLoggerService : ILoggerService
    {
        public void Info(string message) => LogService.MainLogger.Info(message);
        public void Error(string message) => LogService.MainLogger.Error(message);
        public void Error(Exception ex, string message) => LogService.MainLogger.Error(ex, message);
        public void Fatal(Exception ex, string message) => LogService.MainLogger.Fatal(ex, message);
    }
}