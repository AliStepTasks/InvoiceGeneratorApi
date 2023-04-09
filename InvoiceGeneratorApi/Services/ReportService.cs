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

    public async Task<byte[]> GenerateDoneWorkReport(List<InvoiceDTO> invoices, List<Customer> customers)
    {
        // Create a new workbook
        var workbook = new XLWorkbook();

        // Add a worksheet to the workbook
        var worksheet = workbook.Worksheets.Add("Invoice Report");

        // Add headers to the worksheet
        worksheet.Cell(1, 1).Value = "Invoice ID";
        worksheet.Cell(1, 2).Value = "Customer ID";
        worksheet.Cell(1, 3).Value = "Customer Name";
        worksheet.Cell(1, 4).Value = "Start Date";
        worksheet.Cell(1, 5).Value = "End Date";
        worksheet.Cell(1, 6).Value = "Total Sum";

        // Set the style of the header cells
        var headerStyle = workbook.Style;
        headerStyle.Font.Bold = true;
        headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerStyle.Fill.BackgroundColor = XLColor.LightGray;
        headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;

        worksheet.Range("A1:F1").Style = headerStyle;

        // Add data to the worksheet
        for (int i = 0; i < invoices.Count; i++)
        {
            var invoice = invoices[i];
            var customer = customers.SingleOrDefault(c => c.Id == invoice.CustomerId);

            worksheet.Cell(i + 2, 1).Value = invoice.Id;
            worksheet.Cell(i + 2, 2).Value = customer?.Id;
            worksheet.Cell(i + 2, 3).Value = customer?.Name;
            worksheet.Cell(i + 2, 4).Value = invoice.StartDate.ToString("yyyy-MM-dd");
            worksheet.Cell(i + 2, 5).Value = invoice.EndDate.ToString("yyyy-MM-dd");
            worksheet.Cell(i + 2, 6).Value = invoice.TotalSum;

            // Set the style of the data cells
            var dataStyle = workbook.Style;
            dataStyle.Border.BottomBorder = XLBorderStyleValues.Thin;

            worksheet.Range($"A{i + 2}:F{i + 2}").Style = dataStyle;
        }

        // Auto-fit the columns in the worksheet
        worksheet.Columns().AdjustToContents();

        // Convert the workbook to a byte array
        using (var stream = new MemoryStream())
        {
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public async Task<byte[]> GenerateInvoiceReport(List<InvoiceDTO> invoices, List<Customer> customers)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Invoices");

        // Set headers
        worksheet.Cell(1, 1).Value = "Invoice ID";
        worksheet.Cell(1, 2).Value = "Customer Name";
        worksheet.Cell(1, 3).Value = "Total Sum";
        worksheet.Cell(1, 4).Value = "Status";
        worksheet.Cell(1, 5).Value = "Comment";
        worksheet.Cell(1, 6).Value = "Created At";
        worksheet.Cell(1, 7).Value = "Start Date";
        worksheet.Cell(1, 8).Value = "End Date";

        // Set column widths
        worksheet.Column(1).Width = 10;
        worksheet.Column(2).Width = 20;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 15;
        worksheet.Column(5).Width = 30;
        worksheet.Column(6).Width = 20;
        worksheet.Column(7).Width = 20;
        worksheet.Column(8).Width = 20;
        worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Set cell styles
        var headerStyle = worksheet.Cell(1, 1).Style;
        headerStyle.Font.Bold = true;
        headerStyle.Fill.BackgroundColor = XLColor.LightGray;

        var dateStyle = workbook.Style;
        dateStyle.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";

        // Fill data
        for (int i = 0; i < invoices.Count; i++)
        {
            var invoice = invoices[i];
            var customer = customers.FirstOrDefault(c => c.Id == invoice.CustomerId);

            if (customer == null)
            {
                continue;
            }

            worksheet.Cell(i + 2, 1).Value = invoice.Id;
            worksheet.Cell(i + 2, 2).Value = customer.Name;
            worksheet.Cell(i + 2, 3).Value = invoice.TotalSum;
            worksheet.Cell(i + 2, 4).Value = invoice.Status.ToString();
            worksheet.Cell(i + 2, 5).Value = invoice.Comment;
            worksheet.Cell(i + 2, 6).Value = invoice.CreatedAt.ToString();
            worksheet.Cell(i + 2, 6).Style = dateStyle;
            worksheet.Cell(i + 2, 7).Value = invoice.StartDate.ToString();
            worksheet.Cell(i + 2, 7).Style = dateStyle;
            worksheet.Cell(i + 2, 8).Value = invoice.EndDate.ToString();
            worksheet.Cell(i + 2, 8).Style = dateStyle;
        }

        // Convert the workbook to a byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}