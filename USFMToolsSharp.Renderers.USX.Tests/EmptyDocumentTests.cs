using System;
using NUnit.Framework;
using USFMToolsSharp.Models.Markers;

namespace USFMToolsSharp.Renderers.USX.Tests
{
    /// <summary>
    /// A USFMDocument has no hierarchies until a marker is inserted, so the renderer must not
    /// assume Hierarchies[0] exists. Empty documents render as an empty usx wrapper.
    /// </summary>
    public class EmptyDocumentTests
    {
        private static string EmptyWrapper(string usxVersion) =>
            "<?xml version=\"1.0\" encoding=\"\"?>" + Environment.NewLine +
            $"<usx version=\"{usxVersion}\">" + Environment.NewLine +
            "</usx>" + Environment.NewLine;

        [TestCase("2.5")]
        [TestCase("3.0")]
        public void NewDocumentRendersEmptyWrapper(string usxVersion)
        {
            var renderer = new USXRenderer(new USXConfig(false, usxVersion));

            var output = renderer.Render(new USFMDocument());

            Assert.That(output, Is.EqualTo(EmptyWrapper(usxVersion)));
            Assert.That(renderer.UnrenderableTags, Is.Empty);
        }

        [TestCase("2.5")]
        [TestCase("3.0")]
        public void NewDocumentRendersNothingWhenPartial(string usxVersion)
        {
            var renderer = new USXRenderer(new USXConfig(true, usxVersion));

            Assert.That(renderer.Render(new USFMDocument()), Is.Empty);
        }

        [TestCase("")]
        [TestCase("   ")]
        public void ParsedEmptyInputRendersSameAsNewDocument(string usfm)
        {
            var document = new USFMParser().ParseFromString(usfm);

            Assert.That(new USXRenderer().Render(document), Is.EqualTo(new USXRenderer().Render(new USFMDocument())));
        }

        [Test]
        public void EncodingIsReadFromIdeMarker()
        {
            var document = new USFMParser().ParseFromString("\\id MAT EN_ULB\n\\ide UTF-8\n");

            var output = new USXRenderer().Render(document);

            Assert.That(output, Does.StartWith("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"));
        }
    }
}
