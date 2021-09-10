namespace USFMToolsSharp.Renderers.USX
{
    public class USXConfig
    {
        public bool PartialUSX;
        public string USXVersion;

        public USXConfig()
        {
            PartialUSX = false;
            USXVersion = "3.0";
        }

        public USXConfig(bool partialUSX, string USXVersion)
        {
            PartialUSX = partialUSX;
            this.USXVersion = USXVersion;
        }
    }
}