using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Interfaces;
using InvoiceGeneratorApi.Models;

namespace InvoiceGeneratorApi.Services;

public class ReportService : IReportService
{
    public async Task<byte[]> GenerateCustomerReport(List<CustomerDTO> customers, List<Invoice> invoices)
    {
        // Create a new Excel workbook
        var workbook = new XLWorkbook();

        // Add a worksheet to the workbook
        var worksheet = workbook.Worksheets.Add("Customers");

        // Add headers to the worksheet
        worksheet.Cell("A1").Value = "Customer ID";
        worksheet.Cell("B1").Value = "Name";
        worksheet.Cell("C1").Value = "Email";
        worksheet.Cell("D1").Value = "Phone Number";
        worksheet.Cell("E1").Value = "Count of Invoices";

        // Add data to the worksheet
        for (int i = 0; i < customers.Count; i++)
        {
            var customer = customers[i];
            worksheet.Cell($"A{i + 2}").Value = customer.Id;
            worksheet.Cell($"B{i + 2}").Value = customer.Name;
            worksheet.Cell($"C{i + 2}").Value = customer.Email;
            worksheet.Cell($"D{i + 2}").Value = customer.PhoneNumber;
            worksheet.Cell($"E{i + 2}").Value = invoices
                .Count(i => i.CustomerId == customer.Id);
        }

        // Set column widths
        worksheet.Column("A").Width = 5;
        worksheet.Column("B").Width = 25;
        worksheet.Column("C").Width = 25;
        worksheet.Column("D").Width = 20;
        worksheet.Column("E").Width = 10;
        worksheet.Column("A").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Auto-fit column widths for long data
        worksheet.Columns().AdjustToContents();

        // Convert the workbook to a byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GenerateInvoiceReport(IEnumerable<InvoiceDTO> invoices, IEnumerable<Customer> customers)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Invoices");

        // Add column headers
        worksheet.Cell(1, 1).Value = "Customer ID";
        worksheet.Cell(1, 2).Value = "Customer Name";
        worksheet.Cell(1, 3).Value = "Invoice CreatedAt";
        worksheet.Cell(1, 4).Value = "Total Sum";
        worksheet.Cell(1, 5).Value = "Status";
        worksheet.Cell(1, 6).Value = "Start Date";
        worksheet.Cell(1, 7).Value = "End Date";

        // Add data to cells
        var rowNumber = 2;
        foreach (var invoice in invoices)
        {
            // Get customer name
            var customerName = customers.FirstOrDefault(c => c.Id == invoice.CustomerId)?.Name ?? "Unknown";
            var customerId = customers.FirstOrDefault(c => c.Id == invoice.CustomerId)?.Id;

            // Add invoice data to cells
            worksheet.Cell(rowNumber, 1).Value = customerId;
            worksheet.Cell(rowNumber, 2).Value = customerName;
            worksheet.Cell(rowNumber, 3).Value = invoice.CreatedAt.ToString("yyyy-MM-dd");
            worksheet.Cell(rowNumber, 4).Value = invoice.TotalSum;
            worksheet.Cell(rowNumber, 5).Value = invoice.Status.ToString();
            worksheet.Cell(rowNumber, 6).Value = invoice.StartDate.ToString("yyyy-MM-dd");
            worksheet.Cell(rowNumber, 7).Value = invoice.EndDate.ToString("yyyy-MM-dd");

            rowNumber++;
        }
        // Set column widths
        worksheet.Column("A").Width = 5;
        worksheet.Column("B").Width = 20;
        worksheet.Column("C").Width = 20;
        worksheet.Column("D").Width = 10;
        worksheet.Column("E").Width = 20;
        worksheet.Column("F").Width = 20;
        worksheet.Column("D").Width = 20;
        worksheet.Column("A").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Auto-fit column widths for long data
        worksheet.Columns().AdjustToContents();

        // Convert the workbook to a byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        // Return the MemoryStream as a byte array for download
        return stream.ToArray();
    }
}