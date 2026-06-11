using AIRecruitmentAPI.Core;
using DocumentFormat.OpenXml.Packaging;
using System.Text;
using UglyToad.PdfPig;

namespace AIRecruitmentAPI.Services;

public class FileService : IFileService
{
    public async Task<string> ExtractText(Stream fileStream, string extension)
    {
        extension = extension.ToLower();

        if (extension == ".txt")
        {
            using var reader = new StreamReader(fileStream);
            return await reader.ReadToEndAsync();
        }
        else if (extension == ".pdf")
        {
            var text = new StringBuilder();

            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);
            ms.Position = 0;

            using var pdf = PdfDocument.Open(ms);
            foreach (var page in pdf.GetPages())
                text.AppendLine(page.Text);

            return text.ToString();
        }
        else if (extension == ".docx")
        {            
            using var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);

            memoryStream.Position = 0;

            try
            {
                using var doc = WordprocessingDocument.Open(memoryStream, false);
                var body = doc.MainDocumentPart.Document.Body;

                return body.InnerText;
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid or corrupted DOCX file", ex);
            }
        }

        return string.Empty;
    }
}