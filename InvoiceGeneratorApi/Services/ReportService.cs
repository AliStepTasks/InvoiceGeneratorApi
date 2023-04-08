using ClosedXML.Excel;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Interfaces;

namespace InvoiceGeneratorApi.Services;

public class ReportService : IReportService
{
    public async Task<byte[]> GenerateCustomerReport(List<CustomerDTO> customers)
    {
        // Create a new Excel workbook
        using var workbook = new XLWorkbook();

        // Add a worksheet to the workbook
        var worksheet = workbook.Worksheets.Add("Customers");

        // Add headers to the worksheet
        worksheet.Cell("A1").Value = "Customer ID";
        worksheet.Cell("B1").Value = "Name";
        worksheet.Cell("C1").Value = "Email";
        worksheet.Cell("D1").Value = "Phone Number";

        // Add data to the worksheet
        for (int i = 0; i < customers.Count; i++)
        {
            var customer = customers[i];
            worksheet.Cell($"A{i + 2}").Value = customer.Id;
            worksheet.Cell($"B{i + 2}").Value = customer.Name;
            worksheet.Cell($"C{i + 2}").Value = customer.Email;
            worksheet.Cell($"D{i + 2}").Value = customer.PhoneNumber;
        }

        // Set column widths
        worksheet.Column("A").Width = 10;
        worksheet.Column("B").Width = 25;
        worksheet.Column("C").Width = 25;
        worksheet.Column("D").Width = 15;

        // Auto-fit column widths for long data
        worksheet.Columns().AdjustToContents();

        // Convert the workbook to a byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}