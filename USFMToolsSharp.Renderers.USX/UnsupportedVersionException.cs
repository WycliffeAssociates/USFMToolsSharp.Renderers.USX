using System;

namespace USFMToolsSharp.Renderers.USX
{
    public class UnsupportedVersionException : Exception
    {
        public UnsupportedVersionException()
        {
        }

        public UnsupportedVersionException(string message)
            : base(message)
        {
        }
    }
}