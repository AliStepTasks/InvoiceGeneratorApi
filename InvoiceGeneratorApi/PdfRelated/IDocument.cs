using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace InvoiceGeneratorApi.PdfRelated;

public interface IDocument
{
    DocumentMetadata GetMetadata();
    void Compose(IDocumentContainer container);
}