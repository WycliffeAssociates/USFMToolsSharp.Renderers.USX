using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using USFMToolsSharp.Models;
using USFMToolsSharp.Models.Markers;

namespace USFMToolsSharp.Renderers.USX
{
    public class USXRenderer
    {
        private const int DefaultHierarchyIndex = 0;

        public List<string> UnrenderableTags;
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

            if (input.Hierarchies.Count > 0)
            {
                foreach (var marker in input.Hierarchies[DefaultHierarchyIndex].Contents)
                {
                    output.Append(RenderMarker(marker));
                }
            }

            if (!ConfigurationUSX.PartialUSX)
            {
                output.AppendLine("</usx>");
            }

            return output.ToString();
        }

        private string GetEncoding(USFMDocument input)
        {
            if (input.Hierarchies.Count == 0)
            {
                return null;
            }

            var encodingSearch = input.Hierarchies[DefaultHierarchyIndex].GetChildMarkers<IDEMarker>();
            if (encodingSearch.Count > 0)
            {
                return encodingSearch[0].As<IDEMarker>().Encoding;
            }

            return null;
        }

        private string RenderMarker(HierarchyNode input)
        {
            var output = new StringBuilder();
            var footnote = new StringBuilder();
            if (ConfigurationUSX.USXVersion != "3.0" && ConfigurationUSX.USXVersion != "2.5")
            {
                throw new NotSupportedException("Only USX 3.0 and 2.5 are supported");
            }

            switch (input.Marker)
            {
                case BMarker bMarker:
                    output.AppendLine($"<para style=\"{bMarker.Identifier}\"/>");
                    break;
                
                case BDMarker bdMarker:
                    output.AppendLine($"<char style=\"{bdMarker.Identifier}\">");
                    
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</char>");
                    break;
                
                case BDITMarker bditMarker:
                    output.AppendLine($"<char style=\"{bditMarker.Identifier}\">");
                    
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</char>");
                    break;
                
                case CMarker cMarker:
                    CurrentChapter = cMarker.Number;

                    if (ConfigurationUSX.USXVersion.Equals("3.0"))
                    {
                        output.AppendLine($"<chapter style=\"{cMarker.Identifier}\" " +
                                          $"number=\"{cMarker.Number}\" " +
                                          $"sid=\"{CurrentBookCode} {cMarker.Number}\" />");
                    }
                    else
                    {
                        output.AppendLine($"<chapter style=\"{cMarker.Identifier}\" " +
                                          $"number=\"{cMarker.Number}\" />");
                    }

                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    if (ConfigurationUSX.USXVersion.Equals("3.0"))
                    {
                        output.AppendLine($"<chapter eid=\"{CurrentBookCode} {CurrentChapter}\" />");
                    }
                    
                    break;
                
                case DMarker dMarker:
                    output.AppendLine($"<para style=\"{dMarker.Identifier}\">{dMarker.Description}</para>");
                    
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    break;
                
                case EMMarker emMarker:
                    output.AppendLine($"<char style=\"{emMarker.Identifier}\">");

                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</char>");
                    break;
                
                case FMarker fMarker:
                    // For USX 3.0, optional "Category" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/notes.html#footnote-note
                    
                    output.AppendLine($"<note style=\"{fMarker.Identifier}\" " +
                                       $"caller=\"{fMarker.FootNoteCaller}\">");
                    
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</note>");
                    break;
                
                case FKMarker fkMarker:
                    output.AppendLine($"<char style=\"{fkMarker.Identifier}\">{fkMarker.FootNoteKeyword}</char>");
                    break;
                
                case FPMarker fpMarker:
                    output.AppendLine($"<char style=\"{fpMarker.Identifier}\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</char>");
                    break;
                
                case FQMarker fqMarker:
                    output.AppendLine($"<char style=\"{fqMarker.Identifier}\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</char>");
                    break;
                
                case FQAMarker fqaMarker:
                    output.AppendLine($"<char style=\"{fqaMarker.Identifier}\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</char>");
                    break;
                
                case FRMarker frMarker:
                    output.AppendLine($"<char style=\"{frMarker.Identifier}\">{frMarker.VerseReference}</char>");
                    break;
                
                case FTMarker ftMarker:
                    output.AppendLine($"<char style=\"{ftMarker.Identifier}\">");
                    foreach (var marker in input.Contents)
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
                    
                    output.AppendLine($"<book style=\"{input.Marker.Identifier}\" code=\"{bookCode}\">{bibleVersion}</book>");
                    break;
                
                case IMTMarker imtMarker:
                    output.AppendLine($"<para style=\"{imtMarker.Identifier}{imtMarker.Weight}\">" +
                                      $"{imtMarker.IntroTitle}" +
                                      $"</para>");
                    break;
                
                case ITMarker itMarker:
                    output.AppendLine($"<char style=\"{itMarker.Identifier}\">");
                    
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</char>");
                    break;
                
                case LIMarker liMarker:
                    output.AppendLine($"<para style=\"{liMarker.Identifier}{liMarker.Depth}\">");
                    
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</para>");
                    break;
                
                case MSMarker msMarker:
                    output.AppendLine($"<para style=\"{msMarker.Identifier}{msMarker.Weight}\">{msMarker.Heading}</para>");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;

                case MTMarker mtMarker:
                    output.AppendLine($"<para style=\"{mtMarker.Identifier}{mtMarker.Weight}\">{mtMarker.Title}</para>");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                
                case NDMarker ndMarker:
                    output.AppendLine($"<char style=\"{ndMarker.Identifier}\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</char>");
                    break;
                
                case NOMarker noMarker:
                    output.AppendLine($"<char style=\"{noMarker.Identifier}\">");

                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</char>");
                    break;
                
                case PMarker pMarker:
                    // For USX 3.0, optional "vid" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#para

                    output.AppendLine($"<para style=\"{pMarker.Identifier}\">");

                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</para>");
                    break;
                
                case PIMarker piMarker:
                    output.AppendLine($"<para style=\"{piMarker.Identifier}{piMarker.Depth}\">");
                    
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</para>");
                    break;
                
                case QMarker qMarker:
                    // For USX 3.0, optional "vid" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#para

                    output.AppendLine($"<para style=\"{qMarker.Identifier}{qMarker.Depth}\">");
                    
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</para>");
                    break;
                
                case QCMarker qcMarker:
                    output.AppendLine($"<para style=\"{qcMarker.Identifier}\">");
                    
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</para>");
                    break;
                
                case QMMarker qmMarker:
                    output.AppendLine($"<para style=\"{qmMarker.Identifier}{qmMarker.Depth}\">");
                    
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</para>");
                    break;
                
                case QRMarker qrMarker:
                    output.AppendLine($"<para style=\"{qrMarker.Identifier}\">");

                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</para>");
                    break;
                
                case QSMarker qsMarker:
                    output.AppendLine($"<char style=\"{qsMarker.Identifier}\">");

                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</char>");
                    break;
                
                case SMarker sMarker:
                    output.AppendLine($"<para style=\"{sMarker.Identifier}{sMarker.Weight}\">");
                    output.AppendLine($"{sMarker.Text}");
                    output.AppendLine("</para>");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                
                case SCMarker scMarker:
                    output.AppendLine($"<char style=\"{scMarker.Identifier}\">");

                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</char>");
                    break;
                
                case TableBlock _:
                    // For USX 3.0, optional "vid" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#table

                    output.AppendLine("<Table>");

                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    output.AppendLine("</Table>");
                    break;
                
                case TCMarker tcMarker:
                    // For USX 3.0, optional "colspan" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#cell
                    
                    output.AppendLine($"<cell style=\"{tcMarker.Identifier}{tcMarker.ColumnPosition}\" " +
                                        $"align=\"start\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</cell>");
                    break;
                
                case TCRMarker tcrMarker:
                    // For USX 3.0, optional "colspan" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#cell
                    
                    output.AppendLine($"<cell style=\"{tcrMarker.Identifier}{tcrMarker.ColumnPosition}\" " +
                                        $"align=\"end\">");
                    foreach (var marker in input.Contents)
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
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</cell>");
                    break;
                
                case THRMarker thrMarker:
                    // For USX 3.0, optional "colspan" attribute can be added: https://ubsicap.github.io/usx/v3.0.0/elements.html#cell
                    
                    output.AppendLine($"<cell style=\"{thrMarker.Identifier}{thrMarker.ColumnPosition}\" " +
                                      $"align=\"end\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    
                    output.AppendLine("</cell>");
                    break;
                
                case TOC1Marker toc1Marker:
                    output.AppendLine($"<para style=\"{toc1Marker.Identifier}\">{toc1Marker.LongTableOfContentsText}</para>");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;

                case TOC2Marker toc2Marker:
                    output.AppendLine($"<para style=\"{toc2Marker.Identifier}\">{toc2Marker.ShortTableOfContentsText}</para>");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                
                case TOC3Marker toc3Marker:
                    output.AppendLine($"<para style=\"{toc3Marker.Identifier}\">{toc3Marker.BookAbbreviation}</para>");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                
                case TRMarker trMarker:
                    output.AppendLine($"<row style=\"{trMarker.Identifier}\">");
                    foreach (var marker in input.Contents)
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
                        output.AppendLine($"<verse " +
                                          $"number=\"{vMarker.VerseNumber}\" " +
                                          $"style=\"{vMarker.Identifier}\" " +
                                          $"sid=\"{CurrentBookCode} {CurrentChapter}:{vMarker.VerseNumber}\" />");
                    }
                    else
                    {

                        output.AppendLine($"<verse number=\"{vMarker.VerseNumber}\" style=\"{vMarker.Identifier}\" />");
                    }

                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }

                    if (ConfigurationUSX.USXVersion.Equals("3.0"))
                    {
                        output.AppendLine($"<verse eid=\"{CurrentBookCode} {CurrentChapter}:{vMarker.VerseNumber}\" />");
                    }

                    break;
                
                case XMarker xMarker:
                    // output.AppendLine($"<note style=\"{xMarker.Identifier}\" " +
                    //                   $"caller=\"{xMarker.CrossRefCaller}\">");
                    //
                    // foreach (var marker in input.Contents)
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
                case WMarker wMarker:
                    output.AppendLine($"<char style=\"w\">{wMarker.Term}</char>");
                    break;
                case PNMarker pnMarker:
                    output.AppendLine($"<char style=\"{pnMarker.Identifier}\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</char>");
                    break;
                case ADDMarker addMarker:
                    output.AppendLine($"<char style=\"{addMarker.Identifier}\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</char>");
                    break;
                case TLMarker tlMarker:
                    output.AppendLine($"<char style=\"{tlMarker.Identifier}\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</char>");
                    break;
                
                case SPMarker spMarker:
                    output.AppendLine($"<para style=\"{spMarker.Identifier}\">{spMarker.Speaker}</para>");
                    break;
                case RMarker rMarker:
                    output.AppendLine($"<para style=\"{rMarker.Identifier}\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</para>");
                    break;
                case PMMarker:
                case PMOMarker:
                case PMCMarker:
                case MMarker:
                case MIMarker:
                case REMMarker:
                    output.AppendLine($"<para style=\"{input.Marker.Identifier}\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</para>");
                    break;
                case FLMarker:
                    output.AppendLine("<char style=\"fl\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</char>");
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
                case NBMarker _:
                case VPEndMarker _:
                case PNEndMarker _:
                    break;
                    
                default:
                    UnrenderableTags.Add(input.Marker.Identifier);
                    break;
            }
            
            return output.ToString();
        }
    }

}