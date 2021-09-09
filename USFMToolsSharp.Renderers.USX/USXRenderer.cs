using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using USFMToolsSharp.Models.Markers;

namespace USFMToolsSharp.Renderers.USX
{
    public class USXRenderer
    {
        public List<string> UnrenderableTags;
        public USXConfig ConfigurationUSX;
        private IList<string> TOCEntries;
        
        public USXRenderer()
        {
            UnrenderableTags = new List<string>();
            ConfigurationUSX = new USXConfig();
            TOCEntries = new List<string>();
        }

        public USXRenderer(USXConfig config) : this()
        {
            ConfigurationUSX = config;
        }
        public string Render(USFMDocument input)
        {
            var encoding = GetEncoding(input);
            var output = new StringBuilder();
            var bodyContent = new StringBuilder();

            if (!ConfigurationUSX.partialUSX)
            {
                bodyContent.AppendLine($"<?xml version=\"1.0\" encoding=\"{encoding}\">");
                bodyContent.AppendLine("<usx version=\"3.0\">");
            }

            foreach (Marker marker in input.Contents)
            {
                bodyContent.Append(RenderMarker(marker));
            }

            if (!ConfigurationUSX.partialUSX)
            {
                bodyContent.AppendLine("</usx>");
            }
            
            // Insert TOC
            if (ConfigurationUSX.renderTableOfContents && TOCEntries.Count > 0)
            {
                output.AppendLine(RenderTOC());
            }

            output.Append(bodyContent);
            
            return output.ToString();
        }

        private string GetEncoding(USFMDocument input)
        {
            var encodingSearch = input.GetChildMarkers<IDEMarker>();
            if (encodingSearch.Count > 0)
            {
                return encodingSearch[0].Encoding;
            }

            return null;
        }

        private string RenderMarker(Marker input)
        {
            var output = new StringBuilder();

            switch (input)
            {
                
                default:
                    UnrenderableTags.Add(input.Identifier);
                    break;
            }
            
            return output.ToString();
        }

        private string RenderTOC()
        {
            var output = new StringBuilder();

            return output.ToString();
        }
    }
}