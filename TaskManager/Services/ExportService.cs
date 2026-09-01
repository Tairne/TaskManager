using CsvHelper;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using TaskManager.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskManager.Services
{
    public class ExportService : IExportService
    {
        public async Task<byte[]> ExportToCsvAsync<TModel>(IEnumerable<TModel> model, CancellationToken cancellationToken) where TModel : class
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(
                streamWriter,
                CultureInfo.InvariantCulture);

            await csvWriter.WriteRecordsAsync(model, cancellationToken);

            await streamWriter.FlushAsync(cancellationToken);

            return memoryStream.ToArray();
        }
    }
}
