using Microsoft.Xrm.Sdk;
using System;

namespace TestHarness.MockServices
{
    public class MockTracingService : ITracingService
    {
        public void Trace(string format, params object[] args)
        {
            Console.WriteLine($"[TRACE] {DateTime.Now:HH:mm:ss.fff} - {string.Format(format, args)}");
        }
    }
}