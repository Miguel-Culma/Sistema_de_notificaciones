using PruebaTecnica_MonoLegal.Models;

namespace PruebaTecnica_MonoLegal.Services.Interfaces
{
    public interface IInvoiceService
    {
        // Consultar todas las facturas
        Task<object> GetAllInvoicesAsync();
        // Consultar una factura por su identificador
        Task<object> GetInvoiceByIdAsync(string id);

        // Consultar facturas por estado
        Task<List<Invoice>> GetInvoicesByStateAsync(string state);
        // Consultar facturas por estado con el correo del cliente
        Task<List<object>> GetInvoicesByStateWithClientAsync(string state);

        // Procesar los recordatorios de una factura
        Task ProcessReminderAsync(string id);

        // Procesar los recordatorios de una factura
        Task UpdateInvoiceStateManualAsync(string id, string state);
    }
}
