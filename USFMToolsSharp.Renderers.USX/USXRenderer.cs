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
                case BMarker bMarker:
                    output.AppendLine($"<para style=\"{bMarker.Identifier}\"></para>");
                    break;
                
                case BDMarker bdMarker:
                    output.AppendLine($"<char style=\"{bdMarker.Identifier}\">");
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</char>");
                    break;
                
                case BDITMarker bditMarker:
                    output.AppendLine($"<char style=\"{bditMarker.Identifier}\">");
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</char>");
                    break;
                
                case CMarker cMarker:
                    CurrentChapter = cMarker.Number;

                    // USX 3.0
                    if (ConfigurationUSX.USXVersion.Equals("3.0"))
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
                
                case FMarker fMarker:

                    // USX 3.0
                    // Optional "Category" identifier can be added: https://ubsicap.github.io/usx/v3.0.0/notes.html#footnote-note
                    if (ConfigurationUSX.USXVersion.Equals("3.0"))
                    {
                        output.AppendLine($"<note style=\"{fMarker.Identifier}\" " +
                                          $"caller=\"{fMarker.FootNoteCaller}\">");
                    }

                    // USX 2.5
                    else
                    {
                        output.AppendLine($"<note style=\"{fMarker.Identifier}\" " +
                                          $"caller=\"{fMarker.FootNoteCaller}\">");
                    }
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</note>");
                    break;
                
                case FQAMarker fqaMarker:
                    output.AppendLine($"<char style=\"{fqaMarker.Identifier}\">");

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</char>");
                    break;
                
                case FQMarker fqMarker:
                    output.AppendLine($"<char style=\"{fqMarker.Identifier}\">");

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</char>");
                    break;
                
                case FTMarker ftMarker:
                    output.AppendLine($"<char style=\"{ftMarker.Identifier}\">");
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</char>");
                    break;
                
                case HMarker hMarker:
                    output.AppendLine($"<para style=\"{hMarker.Identifier}\">{hMarker.HeaderText}</para>");
                    break;
                
                case IDEMarker ideMarker:
                    output.AppendLine($"<para style=\"{ideMarker.Identifier}\">{ideMarker.Encoding}</para>");
                    break;
                
                case IDMarker idMarker:
                    var bookCode = idMarker.TextIdentifier.Substring(0, 3);
                    var bibleVersion = idMarker.TextIdentifier.Substring(4);

                    CurrentBookCode = bookCode;
                    
                    output.AppendLine($"<book style=\"{input.Identifier}\" code=\"{bookCode}\">{bibleVersion}</book>");
                    break;
                
                case ITMarker itMarker:
                    output.AppendLine($"<char style=\"{itMarker.Identifier}\">");
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</char>");
                    break;
                
                case LIMarker liMarker:
                    output.AppendLine($"<para style=\"{liMarker.Identifier}{liMarker.Depth}\">");
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</para>");
                    break;
                
                case MSMarker msMarker:
                    output.AppendLine($"<para style=\"{msMarker.Identifier}{msMarker.Weight}\">{msMarker.Heading}</para>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;

                case MTMarker mtMarker:
                    output.AppendLine($"<para style=\"{mtMarker.Identifier}{mtMarker.Weight}\">{mtMarker.Title}</para>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;

                case PMarker pMarker:
                
                    // USX 3.0
                    // NEEDS IMPLEMENTATION
                    // Mandatory "vid" identifier can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#para
                    if (ConfigurationUSX.USXVersion.Equals("3.0"))
                    {
                        throw new Exception("USX 3.0 P Marker Not Implemented.");
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
                
                case PIMarker piMarker:
                    output.AppendLine($"<para style=\"{piMarker.Identifier}{piMarker.Depth}\">");
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</para>");
                    break;
                
                case QMarker qMarker:
                    // USX 3.0
                    // NEEDS IMPLEMENTATION
                    // Mandatory "vid" identifier can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#para
                    if (ConfigurationUSX.USXVersion.Equals("3.0"))
                    {
                        throw new Exception("USX 3.0 Q Marker Not Implemented.");
                    }

                    // USX 2.5
                    else
                    {
                        output.AppendLine($"<para style=\"{qMarker.Identifier}{qMarker.Depth}\">");
                    }
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</para>");
                    break;
                
                case QCMarker qcMarker:
                    output.AppendLine($"<para style=\"{qcMarker.Identifier}\">");
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</para>");
                    break;
                
                case QMMarker qmMarker:
                    output.AppendLine($"<para style=\"{qmMarker.Identifier}{qmMarker.Depth}\">");
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</para>");
                    break;

                case SMarker sMarker:
                    output.AppendLine($"<para style=\"{sMarker.Identifier}{sMarker.Weight}\">");
                    output.AppendLine($"{sMarker.Text}");
                    output.AppendLine("</para>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                
                case TextBlock textBlock:
                    output.AppendLine(textBlock.Text.Trim());
                    break;
                
                case TOC1Marker toc1Marker:
                    output.AppendLine($"<para style=\"{toc1Marker.Identifier}\">{toc1Marker.LongTableOfContentsText}</para>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;

                case TOC2Marker toc2Marker:
                    output.AppendLine($"<para style=\"{toc2Marker.Identifier}\">{toc2Marker.ShortTableOfContentsText}</para>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                
                case TOC3Marker toc3Marker:
                    output.AppendLine($"<para style=\"{toc3Marker.Identifier}\">{toc3Marker.BookAbbreviation}</para>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                
                case USFMMarker usfmMarker:
                    output.AppendLine($"<para style=\"{usfmMarker.Identifier}\">{usfmMarker.Version}</para>");
                    break;
                
                case VMarker vMarker:
                    CurrentVerse = vMarker.VerseNumber;

                    // USX 3.0
                    if (ConfigurationUSX.USXVersion.Equals("3.0"))
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

            var text = System.IO.File.ReadAllText("../../../../../USFM_Files/en_ulb/31-OBA.usfm");
            // var text = System.IO.File.ReadAllText("../../../../../USFM_Files/en_ulb/67-REV.usfm");
            // var text = System.IO.File.ReadAllText("../../../../../USFM_Files/en_ulb/42-MRK.usfm");

            var parsed = parser.ParseFromString(text);
            var rendered = renderer.Render(parsed);

            // foreach (var marker in parsed.Contents)
            // {
            //     Console.WriteLine(marker);
            // }

            Console.WriteLine(rendered);

            XmlDocument xmlDoc = new XmlDocument();
            StringWriter sw = new StringWriter();
            xmlDoc.LoadXml(rendered);
            xmlDoc.Save(sw);
            var formatted_rendered = sw.ToString();

            // Console.WriteLine(formatted_rendered);
        }   
    }
}