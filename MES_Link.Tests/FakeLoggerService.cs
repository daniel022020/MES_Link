using System;
using MES_Link.Interfaces;

namespace MES_Link.Tests
{
    public class FakeLoggerService : ILoggerService
    {
        public void Info(string message) { }
        public void Error(string message) { }
        public void Error(Exception ex, string message) { }
        public void Fatal(Exception ex, string message) { }
    }
}