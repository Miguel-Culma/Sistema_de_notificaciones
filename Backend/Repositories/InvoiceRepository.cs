
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using PruebaTecnica_MonoLegal.Context;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;

namespace PruebaTecnica_MonoLegal.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDBContext _context;

        public InvoiceRepository(AppDBContext context)
        {
            _context = context;
        }

        // Obtener todas las facturas
        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _context.invoices.ToListAsync();
        }

        // Buscar una factura por su ID
        public async Task<Invoice?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return null;
            }

            return await _context.invoices
                .FirstOrDefaultAsync(invoice => invoice._id == objectId);
        }

        // Buscar facturas por estado
        public async Task<List<Invoice>> GetByStateAsync(string state)
        {
            return await _context.invoices
                .Where(invoice => invoice.status == state)
                .ToListAsync();
        }

        // Actualizar el estado de una factura
        public async Task UpdateStateAsync(string id, string state)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))  
            {
                throw new ArgumentException(
                    "El identificador de la factura no es válido.",
                    nameof(id));
            }

            var invoice = await _context.invoices
                .FirstOrDefaultAsync(invoice => invoice._id == objectId);

            if (invoice == null)
            {
                throw new KeyNotFoundException(
                    $"No se encontró la factura con ID {id}.");
            }

            invoice.status = state;
            invoice.updatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // Actualizar el estado de una factura de manera manual
        public async Task UpdateStateManualAsync(string id, string state)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                throw new ArgumentException(
                    "El identificador de la factura no es válido.");
            }

            var invoice = await _context.invoices
                .FirstOrDefaultAsync(invoice => invoice._id == objectId);

            if (invoice == null)
            {
                throw new KeyNotFoundException(
                    $"No se encontró la factura con ID {id}.");
            }

            var allowedStates = new[]
            {
                "primerrecordatorio",
                "segundorecordatorio",
                "desactivado"
            };

            if (!allowedStates.Contains(state))
            {
                throw new ArgumentException(
                    $"El estado '{state}' no es válido.");
            }

            invoice.status = state;
            invoice.updatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
