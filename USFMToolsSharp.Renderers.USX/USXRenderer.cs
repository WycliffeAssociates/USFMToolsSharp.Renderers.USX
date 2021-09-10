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
                    var bookCode = idMarker.TextIdentifier.Substring(0, 3);
                    var bibleVersion = idMarker.TextIdentifier.Substring(4);
                    
                    output.Append($"<book style=\"{input.Identifier}\" code=\"{bookCode}\">");
                    output.Append(bibleVersion);
                    output.AppendLine("</book>");
                    break;
                
                case USFMMarker usfmMarker:
                    output.AppendLine($"<para style=\"{usfmMarker.Identifier}\">{usfmMarker.Version}</para>");
                    break;
                
                case IDEMarker ideMarker:
                    output.AppendLine($"<para style=\"{ideMarker.Identifier}\">{ideMarker.Encoding}</para>");
                    break;
                    
                case HMarker hMarker:
                    output.AppendLine($"<para style=\"{hMarker.Identifier}\">{hMarker.HeaderText}</para>");
                    break;
                
                case TOC1Marker toc1Marker:
                    output.AppendLine($"<para style=\"{toc1Marker.Identifier}\">{toc1Marker.LongTableOfContentsText}</para>");
                    break;

                case TOC2Marker toc2Marker:
                    output.AppendLine($"<para style=\"{toc2Marker.Identifier}\">{toc2Marker.ShortTableOfContentsText}</para>");
                    break;
                
                case TOC3Marker toc3Marker:
                    output.AppendLine($"<para style=\"{toc3Marker.Identifier}\">{toc3Marker.BookAbbreviation}</para>");
                    break;
                
                case MTMarker mtMarker:
                    output.AppendLine($"<para style=\"{mtMarker.Identifier}\">{mtMarker.Title}</para>");
                    break;
                
                case SMarker sMarker:
                    output.AppendLine($"<para style=\"{sMarker.Identifier}{sMarker.Weight}\">{sMarker.Text}</para>");
                    break;
                
                case PMarker pMarker:
                    output.AppendLine($"<para style=\"{pMarker.Identifier}\">");

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</para>");
                    break;
                
                case CMarker cMarker:
                    break;
                
                case VMarker vMarker:
                    output.AppendLine($"<verse style=\"{vMarker.Identifier}\" " +
                                      $"number=\"{vMarker.VerseNumber}\" " +
                                      $"sid=\":{vMarker.VerseNumber}\" />");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("<verse/>");
                    break;
                
                case TextBlock textBlock:
                    output.AppendLine(textBlock.Text.Trim());
                    break;

                default:
                    UnrenderableTags.Add(input.Identifier);
                    break;
            }

            return output.ToString();
        }
    }
}