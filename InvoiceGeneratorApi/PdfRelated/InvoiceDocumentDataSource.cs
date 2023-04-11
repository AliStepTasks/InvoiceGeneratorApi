using InvoiceGeneratorApi.PdfRelated.Models;
using QuestPDF.Helpers;

namespace InvoiceGeneratorApi.PdfRelated;

public static class InvoiceDocumentDataSource
{
    private static Address GenerateRandomAddress()
    {
        return new Address
        {
            CompanyName = Placeholders.Name(),
            Street = Placeholders.Label(),
            City = Placeholders.Label(),
            State = Placeholders.Label(),
            Email = Placeholders.Email(),
            Phone = Placeholders.PhoneNumber()
        };
    }
}