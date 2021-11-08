namespace USFMToolsSharp.Renderers.USX
{
    public class USXConfig
    {
        public bool PartialUSX;
        public string USXVersion;


        public USXConfig()
        {
            PartialUSX = false;
            USXVersion = "2.5";
        }

        public USXConfig(bool partialUSX, string USXVersion)
        {
            PartialUSX = partialUSX;
            this.USXVersion = USXVersion;
        }
    }
}