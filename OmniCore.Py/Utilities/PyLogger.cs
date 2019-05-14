using System;
using System.Diagnostics;

namespace OmniCore.Py
{
    public class PyLogger
    {
        public void Log(string text)
        {
            Debug.WriteLine(text);
        }

        public void Error(string text, Exception e)
        {
            Debug.WriteLine(text);
            Debug.WriteLine($"Exception: {e}");
        }
    }
}
