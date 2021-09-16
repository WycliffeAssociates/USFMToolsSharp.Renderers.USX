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
        private List<string> UnrenderableTags;
        private readonly USXConfig ConfigurationUSX;
        private string CurrentBookCode;
        private int CurrentChapter;

        
        public USXRenderer()
        {
            UnrenderableTags = new List<string>();
            ConfigurationUSX = new USXConfig();
            CurrentChapter = 1;
        }

        public USXRenderer(USXConfig config) : this()
        {
            UnrenderableTags = new List<string>();
            ConfigurationUSX = config;
            CurrentChapter = 1;
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
            var footnote = new StringBuilder();

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
                    else if (ConfigurationUSX.USXVersion.Equals("2.5"))
                    {
                        output.AppendLine($"<chapter style=\"{cMarker.Identifier}\" " +
                                          $"number=\"{cMarker.Number}\" />");
                        foreach (Marker marker in input.Contents)
                        {
                            output.Append(RenderMarker(marker));
                        }
                    }

                    else
                    {
                        throw new UnsupportedVersionException($"Unsupported USX version: {ConfigurationUSX.USXVersion}");
                    }
                    break;
                
                case DMarker dMarker:
                    output.AppendLine($"<para style=\"{dMarker.Identifier}\">{dMarker.Description}</para>");
                    
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    break;
                
                case EMMarker emMarker:
                    output.AppendLine($"<char style=\"{emMarker.Identifier}\">");

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</char>");
                    break;
                
                case FMarker fMarker:
                    // For USX 3.0, optional "Category" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/notes.html#footnote-note
                    
                    // output.AppendLine($"<note style=\"{fMarker.Identifier}\" " +
                    //                   $"caller=\"{fMarker.FootNoteCaller}\">");
                    //
                    // foreach (Marker marker in input.Contents)
                    // {
                    //     output.Append(RenderMarker(marker));
                    // }
                    //
                    // output.AppendLine("</note>");
                    break;
                
                case FKMarker fkMarker:
                    output.AppendLine($"<char style=\"{fkMarker.Identifier}\">{fkMarker.FootNoteKeyword}</char>");
                    break;
                
                case FPMarker fpMarker:
                    break;
                
                case FQMarker fqMarker:
                    break;
                
                case FQAMarker fqaMarker:
                    break;
                
                case FRMarker frMarker:
                    output.AppendLine($"<char style=\"{frMarker.Identifier}\">{frMarker.VerseReference}</char>");
                    break;
                
                case FTMarker ftMarker:
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
                
                case IMTMarker imtMarker:
                    output.AppendLine($"<para style=\"{imtMarker.Identifier}{imtMarker.Weight}\">" +
                                      $"{imtMarker.IntroTitle}" +
                                      $"</para>");
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
                
                case NDMarker ndMarker:
                    output.AppendLine($"<char style=\"{ndMarker.Identifier}\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</char>");
                    break;
                
                case NOMarker noMarker:
                    output.AppendLine($"<char style=\"{noMarker.Identifier}\">");

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</char>");
                    break;
                
                case PMarker pMarker:
                    // For USX 3.0, optional "vid" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#para

                    output.AppendLine($"<para style=\"{pMarker.Identifier}\">");

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
                    // For USX 3.0, optional "vid" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#para

                    output.AppendLine($"<para style=\"{qMarker.Identifier}{qMarker.Depth}\">");
                    
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
                
                case QRMarker qrMarker:
                    output.AppendLine($"<para style=\"{qrMarker.Identifier}\">");

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</para>");
                    break;
                
                case QSMarker qsMarker:
                    output.AppendLine($"<char style=\"{qsMarker.Identifier}\">");

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</char>");
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
                
                case SCMarker scMarker:
                    output.AppendLine($"<char style=\"{scMarker.Identifier}\">");

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</char>");
                    break;
                
                case TableBlock _:
                    // For USX 3.0, optional "vid" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#table

                    output.AppendLine("<Table>");

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</Table>");
                    break;
                
                case TCMarker tcMarker:
                    // For USX 3.0, optional "colspan" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#cell
                    
                    output.AppendLine($"<cell style=\"{tcMarker.Identifier}{tcMarker.ColumnPosition}\" " +
                                        $"align=\"start\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</cell>");
                    break;
                
                case TCRMarker tcrMarker:
                    // For USX 3.0, optional "colspan" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#cell
                    
                    output.AppendLine($"<cell style=\"{tcrMarker.Identifier}{tcrMarker.ColumnPosition}\" " +
                                        $"align=\"end\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</cell>");
                    break;
                
                case TextBlock textBlock:
                    output.Append(textBlock.Text);
                    break;
                
                case THMarker thMarker:
                    // For USX 3.0, optional "colspan" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#cell
                    
                    output.AppendLine($"<cell style=\"{thMarker.Identifier}{thMarker.ColumnPosition}\" " +
                                      $"align=\"start\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</cell>");
                    break;
                
                case THRMarker thrMarker:
                    // For USX 3.0, optional "colspan" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#cell
                    
                    output.AppendLine($"<cell style=\"{thrMarker.Identifier}{thrMarker.ColumnPosition}\" " +
                                      $"align=\"end\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</cell>");
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
                
                case TRMarker trMarker:
                    output.AppendLine($"<row style=\"{trMarker.Identifier}\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</row>");
                    break;
                
                case USFMMarker usfmMarker:
                    output.AppendLine($"<para style=\"{usfmMarker.Identifier}\">{usfmMarker.Version}</para>");
                    break;
                
                case VMarker vMarker:
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
                    else if (ConfigurationUSX.USXVersion.Equals("2.5"))
                    {
                        output.AppendLine($"<verse style=\"{vMarker.Identifier}\" " +
                                          $"number=\"{vMarker.VerseNumber}\" />");
                        foreach (Marker marker in input.Contents)
                        {
                            output.Append(RenderMarker(marker));
                        }
                    }

                    else
                    {
                        throw new UnsupportedVersionException($"Unsupported USX version: {ConfigurationUSX.USXVersion}");
                    }
                    break;
                
                case XMarker xMarker:
                    // output.AppendLine($"<note style=\"{xMarker.Identifier}\" " +
                    //                   $"caller=\"{xMarker.CrossRefCaller}\">");
                    //
                    // foreach (Marker marker in input.Contents)
                    // {
                    //     output.Append(RenderMarker(marker));
                    // }
                    //
                    // output.AppendLine("</note>");
                    break;
                
                case XOMarker xoMarker:
                    output.AppendLine($"<char style=\"{xoMarker.Identifier}\">{xoMarker.OriginRef}</char>");
                    break;
                
                case XTMarker xtMarker:
                    break;
                
                case IOREndMarker _:
                case SUPEndMarker _:
                case NDEndMarker _:
                case NOEndMarker _:
                case BDITEndMarker _:
                case EMEndMarker _:
                case QACEndMarker _:
                case QSEndMarker _:
                case XEndMarker _:
                case WEndMarker _:
                case RQEndMarker _:
                case FVEndMarker _:
                case TLEndMarker _:
                case SCEndMarker _:
                case ADDEndMarker _:
                case BKEndMarker _:
                case FEndMarker _:
                case VPMarker _:
                case VPEndMarker _:
                    break;
                    
                default:
                    UnrenderableTags.Add(input.Identifier);
                    break;
            }
            
            return output.ToString();
        }
    }
}
