using System.Text;
using Kmic.BuildingBlocks.Application.Files;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Files;

public sealed class FileTypesTests
{
    private static readonly byte[] _jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46];
    private static readonly byte[] _png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];
    private static readonly byte[] _ole = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, 0x00];

    private static byte[] Zip(string firstEntryName) => [.. new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00 }, .. Encoding.ASCII.GetBytes(firstEntryName)];

    [Theory]
    [InlineData("合同.pdf", "%PDF-1.7\n...", FileType.Pdf)]
    [InlineData("a.GIF", "GIF89a....", FileType.Gif)]
    [InlineData("a.gif", "GIF87a....", FileType.Gif)]
    [InlineData("说明.txt", "你好，这是一份说明\r\n第二行", FileType.Txt)]
    [InlineData("明细.csv", "名称,数量\n样品,2\n", FileType.Csv)]
    public void Identify_ExtensionAndHeaderAgree_GivesThatType(string fileName, string content, string expected) =>
        FileTypes.Identify(fileName, Encoding.UTF8.GetBytes(content)).ShouldNotBeNull().Code.ShouldBe(expected);

    [Fact]
    public void Identify_BinaryImagesAndWebp_ByTheirSignatures()
    {
        FileTypes.Identify("a.jpg", _jpeg)!.Code.ShouldBe(FileType.Jpeg);
        FileTypes.Identify("A.JPEG", _jpeg)!.Code.ShouldBe(FileType.Jpeg);
        FileTypes.Identify("a.png", _png)!.Code.ShouldBe(FileType.Png);
        FileTypes.Identify("a.webp", [.. "RIFF"u8, 0x24, 0x00, 0x00, 0x00, .. "WEBP"u8, .. "VP8 "u8])!.Code.ShouldBe(FileType.Webp);
        FileTypes.Identify("a.webp", [.. "RIFF"u8, 0x24, 0x00, 0x00, 0x00, .. "WAVE"u8, .. "fmt "u8]).ShouldBeNull("RIFF 容器里装的是音频");
        FileTypes.Identify("a.webp", "RIFF"u8).ShouldBeNull("头不够长");
    }

    [Fact]
    public void Identify_OpenXmlFormats_AreZipsWhoseExtensionDecidesWhichOne()
    {
        FileTypes.Identify("a.docx", Zip("word/document.xml"))!.Code.ShouldBe(FileType.Docx);
        FileTypes.Identify("a.xlsx", Zip("xl/workbook.xml"))!.Code.ShouldBe(FileType.Xlsx);
        FileTypes.Identify("a.pptx", Zip("ppt/presentation.xml"))!.Code.ShouldBe(FileType.Pptx);
        FileTypes.Identify("a.docx", Zip("[Content_Types].xml"))!.Code.ShouldBe(FileType.Docx, "文件头里还没出现具体的目录名，由扩展名决定");
        FileTypes.Identify("a.zip", Zip("any.txt"))!.Code.ShouldBe(FileType.Zip);
        FileTypes.Identify("a.docx", Zip("xl/workbook.xml")).ShouldBeNull("头里是 Excel 的目录，扩展名是改成 docx 的");
        FileTypes.Identify("a.xlsx", Zip("word/document.xml")).ShouldBeNull();
        FileTypes.Identify("a.pptx", Zip("word/document.xml")).ShouldBeNull();
    }

    [Fact]
    public void Identify_LegacyOffice_ByTheCompoundDocumentHeader()
    {
        foreach (var name in new[] { "a.doc", "a.xls", "a.ppt" })
        {
            FileTypes.Identify(name, _ole).ShouldNotBeNull();
        }

        FileTypes.Identify("a.doc", Zip("word/document.xml")).ShouldBeNull();
    }

    [Theory]
    [InlineData("a.pdf", "不是 PDF 的内容")]
    [InlineData("a.exe", "MZ......")]
    [InlineData("没有扩展名", "%PDF-1.4")]
    [InlineData("a.pdf.exe", "%PDF-1.4")]
    [InlineData("a.", "%PDF-1.4")]
    public void Identify_ExtensionOrHeaderDoesNotMatch_GivesNothing(string fileName, string content) =>
        FileTypes.Identify(fileName, Encoding.UTF8.GetBytes(content)).ShouldBeNull();

    [Fact]
    public void Identify_AnImageRenamedAsPdf_AndAPdfRenamedAsImage_AreBothRefused()
    {
        FileTypes.Identify("a.pdf", _jpeg).ShouldBeNull();
        FileTypes.Identify("a.jpg", "%PDF-1.4"u8).ShouldBeNull();
    }

    [Fact]
    public void Identify_TextThatContainsBinary_IsRefused_ButGbkBytesAreFine()
    {
        FileTypes.Identify("a.txt", [0x68, 0x69, 0x00, 0x68]).ShouldBeNull("空字节说明是二进制");
        FileTypes.Identify("a.csv", [0x68, 0x69, 0x01]).ShouldBeNull();
        FileTypes.Identify("a.txt", [0xC4, 0xE3, 0xBA, 0xC3, 0x0D, 0x0A]).ShouldNotBeNull("GBK 的“你好”");
        FileTypes.Identify("a.txt", [0xEF, 0xBB, 0xBF, 0x68, 0x69]).ShouldNotBeNull("UTF-8 带 BOM");
        FileTypes.Identify("a.txt", [0xFF, 0xFE, 0x68, 0x00]).ShouldBeNull("UTF-16 里有空字节，这里不当文本收");
    }

    [Fact]
    public void Identify_AnEmptyFileHasNoType() => FileTypes.Identify("a.txt", []).ShouldBeNull();

    [Fact]
    public void Registry_EveryTypeIsConsistent()
    {
        FileTypes.All.Select(t => t.Code).Distinct(StringComparer.Ordinal).Count().ShouldBe(FileTypes.All.Count);
        foreach (var type in FileTypes.All)
        {
            FileTypes.Find(type.Code).ShouldBe(type);
            type.Extensions.ShouldAllBe(e => e.StartsWith('.') && string.Equals(e, e.ToLowerInvariant(), StringComparison.Ordinal));
            type.Extensions.ShouldAllBe(e => ReferenceEquals(FileTypes.ByFileName("x" + e), type), "扩展名只对应一种类型");
            type.ContentType.ShouldNotBeNullOrWhiteSpace();
        }

        FileTypes.Find("exe").ShouldBeNull();
        FileTypes.HeaderLength.ShouldBe(4096);
    }
}
