namespace USFMToolsSharp.Renderers.USX
{
    public class USXConfig
    {
        public bool separateChapters;
        public bool separateVerses;
        public bool blankColumn;
        public bool partialUSX;
        public bool renderTableOfContents;

        public USXConfig()
        {
            separateChapters = false;
            separateVerses = false;
            blankColumn = false;
            partialUSX = false;
            renderTableOfContents = false;
        }

        public USXConfig(
            bool separateChapters, bool separateVerses, 
            bool blankColumn, bool partialUSX,
            bool renderTableOfContents)
        {
            this.separateChapters = separateChapters;
            this.separateVerses = separateVerses;
            this.blankColumn = blankColumn;
            this.partialUSX = partialUSX;
            this.renderTableOfContents = renderTableOfContents;
        }
    }
}