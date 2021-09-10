namespace USFMToolsSharp.Renderers.USX
{
    public class USXConfig
    {
        public bool PartialUSX;

        public USXConfig()
        {
            PartialUSX = false;
        }

        public USXConfig(bool partialUSX)
        {
            PartialUSX = partialUSX;
        }
    }
}