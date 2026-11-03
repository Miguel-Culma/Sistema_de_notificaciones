
using Microsoft.AspNetCore.Mvc;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace PruebaTecnica_MonoLegal.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // GET: api/Invoices
        [HttpGet]
        public async Task<ActionResult<List<Invoice>>> GetAll()
        {
            Console.Write("entrea a api/invoices");
            var invoices = await _invoiceService.GetAllInvoicesAsync();

            return Ok(invoices);
        }

        // GET: api/Invoices/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetById(string id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);

            if (invoice == null)
            {
                return NotFound(new
                {
                    message = "No se encontró la factura solicitada."
                });
            }

            return Ok(invoice);
        }

        // GET: api/Invoices/state/{state}
        [HttpGet("state/{state}")]
        public async Task<ActionResult<List<Invoice>>> GetByState(
            string state)
        {
            var invoices = await _invoiceService.GetInvoicesByStateWithClientAsync(state);

            return Ok(invoices);
        }

        // POST: api/Invoices/{id}/reminder
        [HttpPost("{id}/reminder")]
        public async Task<IActionResult> ProcessReminder(string id)
        {
            try
            {
                await _invoiceService.ProcessReminderAsync(id);

                return Ok(new
                {
                    message = "Recordatorio procesado correctamente."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotImplementedException ex)
            {
                return StatusCode(501, new { message = ex.Message });
            }
        }

        // PUT: api/Invoices/{id}/state
        [HttpPut("{id}/state")]
        public async Task<IActionResult> UpdateStateManual(
            string id, [FromBody] string state)
        {
            try
            {
                await _invoiceService.UpdateInvoiceStateManualAsync(id, state);

                return Ok(new
                {
                    message = "Estado de la factura actualizado correctamente."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
