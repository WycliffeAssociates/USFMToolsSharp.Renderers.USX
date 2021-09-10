namespace USFMToolsSharp.Renderers.USX
{
    public class USXConfig
    {
        public bool PartialUSX;
        public double USXVersion;

        public USXConfig()
        {
            PartialUSX = false;
            USXVersion = 3.0;
        }

        public USXConfig(bool partialUSX, double USXVersion)
        {
            PartialUSX = partialUSX;
            this.USXVersion = USXVersion;
        }
    }
}