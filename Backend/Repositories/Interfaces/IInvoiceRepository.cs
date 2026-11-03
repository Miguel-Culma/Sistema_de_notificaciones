using PruebaTecnica_MonoLegal.Models;

namespace PruebaTecnica_MonoLegal.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        //Obtiene todas las facturas
        Task<List<Invoice>> GetAllAsync();
        //Obtiene facturas por id
        Task<Invoice?> GetByIdAsync(string id);
        //obtiene factura por estado
        Task<List<Invoice>> GetByStateAsync(string state);
        //actualiza el estado de la factura una vez enviado el email
        Task UpdateStateAsync(string id, string state);

        //actualiza el estado de la factura manualmente - solo para la facilidad de probar el programa (Documentacion apartado 8)
        Task UpdateStateManualAsync(string id, string state);
    }
}
