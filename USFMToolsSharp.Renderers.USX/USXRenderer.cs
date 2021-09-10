using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using USFMToolsSharp.Models.Markers;

namespace USFMToolsSharp.Renderers.USX
{
    public class USXRenderer
    {
        public List<string> UnrenderableTags;
        public USXConfig ConfigurationUSX;
        private string CurrentBookCode;
        private int CurrentChapter;
        private string CurrentVerse;
        
        public USXRenderer()
        {
            UnrenderableTags = new List<string>();
            ConfigurationUSX = new USXConfig();
            CurrentChapter = 1;
            CurrentVerse = "1";
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
                output.AppendLine($"<?xml version=\"1.0\" encoding=\"{encoding}\"?>");
                output.AppendLine($"<usx version=\"{ConfigurationUSX.USXVersion}\">");
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

                    CurrentBookCode = bookCode;
                    
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
                
                // HAVEN'T IMPLEMENTED VID IDENTIFIER
                case PMarker pMarker:

                    // USX 3.0
                    // VID IDENTIFIER IMPLEMENTATION GOES HERE
                    if (ConfigurationUSX.USXVersion.Equals(3.0))
                    {
                        output.AppendLine($"<para style=\"{pMarker.Identifier}\">");
                    }

                    // USX 2.5
                    else
                    {
                        output.AppendLine($"<para style=\"{pMarker.Identifier}\">");
                    }

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</para>");
                    break;
                
                case CMarker cMarker:
                    CurrentChapter = cMarker.Number;

                    // USX 3.0
                    if (ConfigurationUSX.USXVersion.Equals(3.0))
                    {
                        output.AppendLine($"<chapter style=\"{cMarker.Identifier}\" " +
                                          $"number=\"{cMarker.Number}\" " +
                                          $"sid=\"{CurrentBookCode} {cMarker.Number}\" />");
                        foreach (Marker marker in input.Contents)
                        {
                            output.Append(RenderMarker(marker));
                        }
                        output.AppendLine($"<chapter eid=\"{CurrentBookCode} {CurrentChapter}\" />");
                    }
                    
                    // USX 2.5
                    else
                    {
                        output.AppendLine($"<chapter style=\"{cMarker.Identifier}\" " +
                                          $"number=\"{cMarker.Number}\" />");
                        foreach (Marker marker in input.Contents)
                        {
                            output.Append(RenderMarker(marker));
                        }
                    }
                    break;
                 
                case VMarker vMarker:
                    CurrentVerse = vMarker.VerseNumber;

                    // USX 3.0
                    if (ConfigurationUSX.USXVersion.Equals(3.0))
                    {
                        output.AppendLine($"<verse style=\"{vMarker.Identifier}\" " +
                                          $"number=\"{vMarker.VerseNumber}\" " +
                                          $"sid=\"{CurrentBookCode} {CurrentChapter}:{vMarker.VerseNumber}\" />");
                        foreach (Marker marker in input.Contents)
                        {
                            output.Append(RenderMarker(marker));
                        }
                        output.AppendLine($"<verse eid=\"{CurrentBookCode} {CurrentChapter}:{vMarker.VerseNumber}\" />");
                    }

                    // USX 2.5
                    else
                    {
                        output.AppendLine($"<verse style=\"{vMarker.Identifier}\" " +
                                          $"number=\"{vMarker.VerseNumber}\" />");
                        foreach (Marker marker in input.Contents)
                        {
                            output.Append(RenderMarker(marker));
                        }
                    }
                    break;
                
                // HAVEN'T IMPLEMENTED VID IDENTIFIER                
                case QMarker qMarker:
                    output.AppendLine($"<para style=\"{qMarker.Identifier}\">");
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</para>");
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
        
        // Debugging purposes
        public static void Main()
        {
            List<string> UnrenderableTags = new List<string>();
            
            UnrenderableTags.Add("s5");
            
            var parser = new USFMParser(UnrenderableTags);
            var renderer = new USXRenderer();

            // var text = System.IO.File.ReadAllText("../../../../../USFM_Files/en_ulb/31-OBA.usfm");
            // var text = System.IO.File.ReadAllText("../../../../../USFM_Files/en_ulb/67-REV.usfm");
            var text = System.IO.File.ReadAllText("../../../../../USFM_Files/en_ulb/42-MRK.usfm");

            var parsed = parser.ParseFromString(text);
            var rendered = renderer.Render(parsed);

            // foreach (var marker in parsed.Contents)
            // {
            //     Console.WriteLine(marker);
            // }

            // Console.WriteLine(rendered);

            XmlDocument xmlDoc = new XmlDocument();
            StringWriter sw = new StringWriter();
            xmlDoc.LoadXml(rendered);
            xmlDoc.Save(sw);
            var formatted_rendered = sw.ToString();

            Console.WriteLine(formatted_rendered);
        }   
    }
}