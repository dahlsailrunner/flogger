using System;

namespace Flogging.Data
{
    public class SprocException : Exception
    {
        public string StoredProcName { get; private set; }
        public string ParameterString { get; private set; }

        public SprocException(string message, string storedProc, string paramString) : base(message)
        {
            StoredProcName = storedProc;
            ParameterString = paramString;
        }
        public SprocException(string message, string storedProc, string paramString, Exception innerException)
            : base(message, innerException)
        {
            StoredProcName = storedProc;
            ParameterString = paramString;
        }
    }
}
