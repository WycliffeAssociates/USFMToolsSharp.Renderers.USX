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
        
        public USXRenderer()
        {
            UnrenderableTags = new List<string>();
            ConfigurationUSX = new USXConfig();
        }

        public USXRenderer(USXConfig config) : this()
        {
            ConfigurationUSX = config;
        }
        public string Render(USFMDocument input)
        {
            var encoding = GetEncoding(input);
            var output = new StringBuilder();

            if (!ConfigurationUSX.PartialUSX)
            {
                output.AppendLine($"<?xml version=\"1.0\" encoding=\"{encoding}\">");
                output.AppendLine("<usx version=\"3.0\">");
            }

            foreach (Marker marker in input.Contents)
            {
                output.Append(RenderMarker(marker));
            }

            if (!ConfigurationUSX.PartialUSX)
            {
                output.AppendLine("</usx>");
            }

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
                case IDMarker idMarker:
                    string code = idMarker.TextIdentifier.Substring(0, 3);
                    var bibleName = idMarker.TextIdentifier.Substring(4);
                    
                    output.Append($"<book style=\"{input.Identifier}\" code=\"{code}\">");
                    output.Append(bibleName);
                    output.AppendLine("</book>");
                    break;

                
                default:
                    UnrenderableTags.Add(input.Identifier);
                    break;
            }
            
            return output.ToString();
        }
    }
}