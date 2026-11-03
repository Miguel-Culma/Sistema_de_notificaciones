
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;
using PruebaTecnica_MonoLegal.Services.Interfaces;


namespace PruebaTecnica_MonoLegal.Services
{
    public class InvoiceService : IInvoiceService  {

        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IClientRepository _clientRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailService _emailService;


        public InvoiceService(IInvoiceRepository invoiceRepository, IClientRepository clientRepository, INotificationRepository notificationRepository, IEmailService emailService)
        {                                                                                                                                                                                                                                   
            _invoiceRepository = invoiceRepository;
            _clientRepository = clientRepository;
            _notificationRepository = notificationRepository;
            _emailService = emailService;
        }

        // Consultar todas las facturas con el correo del cliente
        public async Task<object> GetAllInvoicesAsync()
        {
            var invoices = await _invoiceRepository.GetAllAsync();
            var clients = await _clientRepository.GetAllAsync();

            var result = invoices.Select(invoice => new
            {
                invoice,
                clientEmail = clients
                    .FirstOrDefault(c => c._id == invoice.clientId)?.mail
            }).ToList();

            return result;
        }

        // Consultar una factura por su ID
        public async Task<object> GetInvoiceByIdAsync(string id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);
            var clients = await _clientRepository.GetAllAsync();

            var result = new
            {
                invoice,
                clientEmail = clients
                    .FirstOrDefault(c => c._id == invoice?.clientId)?.mail
            };

            return result;
        }

        // Consultar facturas por estado
        public async Task<List<Invoice>> GetInvoicesByStateAsync(string state)
        {
            return await _invoiceRepository.GetByStateAsync(state);
        }
        // consultar facturas por estado con el correo del cliente
        public async Task<List<object>> GetInvoicesByStateWithClientAsync(string state)
        {
            var invoices = await _invoiceRepository.GetByStateAsync(state);
            var clients = await _clientRepository.GetAllAsync();

            var result = invoices.Select(invoice => (object)new
            {
                invoice,
                clientEmail = clients
                    .FirstOrDefault(c => c._id == invoice.clientId)?.mail
            }).ToList();

            return result;
        }

        // Procesar el recordatorio de una factura

        public async Task ProcessReminderAsync(string id)
        {
            // 1. Buscar la factura
            var invoice = await _invoiceRepository.GetByIdAsync(id);

            if (invoice == null)
                throw new KeyNotFoundException("La factura no existe.");

            // 2. Determinar el correo y el estado destino
            string subject;
            string body;
            string targetStatus;
            string notificationType;

            switch (invoice.status)
            {
                case "primerrecordatorio":
                    targetStatus = "segundorecordatorio";
                    notificationType = "segundo_recordatorio";
                    subject = "Segundo recordatorio de pago - MonoLegal";
                    body = $"""
                 Estimado cliente:

                Le informamos que la factura {invoice.invoiceNumber}
                ha pasado a la etapa de segundo recordatorio.

                Valor: {invoice.amount} {invoice.currency}
                Fecha de vencimiento: {invoice.dueDate:dd/MM/yyyy}

                Por favor, revise el estado de su pago.
                """;
                    break;

                case "segundorecordatorio":
                    targetStatus = "desactivado";
                    notificationType = "aviso_desactivacion";
                    subject = "Aviso de desactivación - MonoLegal";
                    body = $"""
                Estimado cliente:

                Le informamos que la factura {invoice.invoiceNumber}
                pasará al estado de desactivación.

                Valor: {invoice.amount} {invoice.currency}
                Fecha de vencimiento: {invoice.dueDate:dd/MM/yyyy}

                Por favor, revise el estado de su pago.
                """;
                    break;

                case "desactivado":
                    throw new InvalidOperationException(
                        "La factura ya está desactivada y no admite más recordatorios.");

                default:
                    throw new InvalidOperationException(
                        $"El estado '{invoice.status}' no permite procesar recordatorios.");
            }

            // 3. Buscar al cliente
            var client = await _clientRepository.GetByIdAsync(
                invoice.clientId.ToString());

            if (client == null)
                throw new InvalidOperationException(
                    "No se encontró el cliente asociado a la factura.");

            if (!client.active)
                throw new InvalidOperationException(
                    "El cliente está inactivo y no se puede enviar el correo.");

            // 4. Preparar el registro del intento
            var notification = new Notifications
            {
                _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                invoiceId = invoice._id,
                clientId = client._id,
                type = notificationType,
                recipient = client.mail,
                attemps = 1,
                targetStatus = targetStatus,
                result = "fallido",
                acceptedAt = null,
                error = null,
                createAt = DateTime.UtcNow
            };


            // 5. Intentar enviar el correo
            try
            {
                await _emailService.SendEmailAsync(
                    client.mail,
                    subject,
                    body);
            }
            catch (Exception ex)
            {
                notification.result = "fallido";
                notification.acceptedAt = null;
                notification.error = ex.Message;

                await _notificationRepository.CreateAsync(notification);

                throw new InvalidOperationException(
                    "No se pudo enviar el recordatorio.", ex);
            }

            // 6. Registrar el envío aceptado
            notification.result = "aceptado";
            notification.acceptedAt = DateTime.UtcNow;

            await _notificationRepository.CreateAsync(notification);

            // 7. Actualizar el estado de la factura
            await _invoiceRepository.UpdateStateAsync(id, targetStatus);

        }

        //actualiza el estado de la factura manualmente - solo para la facilidad de probar el programa (Documentacion apartado 8)
        public async Task UpdateInvoiceStateManualAsync(
           string id, string state)
        {
            await _invoiceRepository.UpdateStateManualAsync(id, state);
        }
    }
}
