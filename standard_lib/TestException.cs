using System;

namespace standard_lib
{
    public class TestException : Exception
    {
        public TestException(string message) : base(message)
        {
        }

        public TestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}