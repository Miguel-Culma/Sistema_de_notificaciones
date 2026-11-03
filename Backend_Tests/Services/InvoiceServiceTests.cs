using Moq;
using MongoDB.Bson;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;
using PruebaTecnica_MonoLegal.Services;
using PruebaTecnica_MonoLegal.Services.Interfaces;
using Xunit;

namespace PruebaTecnica_MonoLegal.Tests.Services
{
    public class InvoiceServiceTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepository;
        private readonly Mock<IClientRepository> _clientRepository;
        private readonly Mock<INotificationRepository> _notificationRepository;
        private readonly Mock<IEmailService> _emailService;

        private readonly InvoiceService _invoiceService;

        public InvoiceServiceTests()
        {
            // MockBehavior.Strict: cualquier llamada sin Setup lanza una excepción
            // en lugar de devolver null en silencio (evita falsos positivos).
            _invoiceRepository = new Mock<IInvoiceRepository>(MockBehavior.Strict);
            _clientRepository = new Mock<IClientRepository>(MockBehavior.Strict);
            _notificationRepository = new Mock<INotificationRepository>(MockBehavior.Strict);
            _emailService = new Mock<IEmailService>(MockBehavior.Strict);

            _invoiceService = new InvoiceService(
                _invoiceRepository.Object,
                _clientRepository.Object,
                _notificationRepository.Object,
                _emailService.Object
            );
        }

        // ---------------------------------------------------------------
        // Métodos auxiliares
        // ---------------------------------------------------------------

        private static Invoice CreateInvoice(
            string invoiceId,
            ObjectId clientId,
            string status,
            string invoiceNumber = "FAC-001")
        {
            return new Invoice
            {
                _id = ObjectId.Parse(invoiceId),
                invoiceNumber = invoiceNumber,
                clientId = clientId,
                amount = 250000,
                currency = "COP",
                createAt = DateTime.UtcNow,
                dueDate = DateTime.UtcNow.AddDays(10),
                status = status
            };
        }

        private static Client CreateClient(ObjectId clientId, bool active = true)
        {
            return new Client
            {
                _id = clientId,
                name = "Cliente de prueba",
                mail = "cliente@prueba.com",
                active = active
            };
        }

        private void VerifyNoEmailSent()
        {
            _emailService.Verify(
                e => e.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private void VerifyNoNotificationRegistered()
        {
            _notificationRepository.Verify(
                r => r.CreateAsync(It.IsAny<Notifications>()),
                Times.Never);
        }

        private void VerifyInvoiceNotUpdated()
        {
            _invoiceRepository.Verify(
                r => r.UpdateStateAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        private void VerifyClientNeverQueried()
        {
            _clientRepository.Verify(
                r => r.GetByIdAsync(It.IsAny<string>()),
                Times.Never);
        }

        // ---------------------------------------------------------------
        // Prueba 1: flujo exitoso (cambio de estado + notificación)
        // ---------------------------------------------------------------

        [Theory]
        [InlineData(
            "primerrecordatorio",
            "segundorecordatorio",
            "segundo_recordatorio",
            "Segundo recordatorio de pago - MonoLegal")]
        [InlineData(
            "segundorecordatorio",
            "desactivado",
            "aviso_desactivacion",
            "Aviso de desactivación - MonoLegal")]
        public async Task ProcessReminderAsync_ChangesStatusAndRegistersNotification(
            string initialStatus,
            string expectedStatus,
            string expectedNotificationType,
            string expectedSubject)
        {
            // Arrange
            var invoiceId = ObjectId.GenerateNewId().ToString();
            var clientId = ObjectId.GenerateNewId();

            var invoice = CreateInvoice(invoiceId, clientId, initialStatus);
            var client = CreateClient(clientId);

            Notifications? savedNotification = null;

            _invoiceRepository
                .Setup(r => r.GetByIdAsync(invoiceId))
                .ReturnsAsync(invoice);

            _clientRepository
                .Setup(r => r.GetByIdAsync(clientId.ToString()))
                .ReturnsAsync(client);

            _emailService
                .Setup(e => e.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _notificationRepository
                .Setup(r => r.CreateAsync(It.IsAny<Notifications>()))
                .Callback<Notifications>(n => savedNotification = n)
                .Returns(Task.CompletedTask);

            _invoiceRepository
                .Setup(r => r.UpdateStateAsync(invoiceId, expectedStatus))
                .Returns(Task.CompletedTask);

            // Act
            await _invoiceService.ProcessReminderAsync(invoiceId);

            // Assert: correo enviado una sola vez, al destinatario y con el asunto correcto
            _emailService.Verify(
                e => e.SendEmailAsync(
                    client.mail,
                    expectedSubject,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Assert: notificación registrada correctamente
            Assert.NotNull(savedNotification);
            Assert.Equal(expectedNotificationType, savedNotification!.type);
            Assert.Equal(expectedStatus, savedNotification.targetStatus);
            Assert.Equal("aceptado", savedNotification.result);
            Assert.Equal(client.mail, savedNotification.recipient);
            Assert.NotNull(savedNotification.acceptedAt);
            Assert.Null(savedNotification.error);

            _notificationRepository.Verify(
                r => r.CreateAsync(It.IsAny<Notifications>()),
                Times.Once);

            // Assert: factura actualizada al estado esperado
            _invoiceRepository.Verify(
                r => r.UpdateStateAsync(invoiceId, expectedStatus),
                Times.Once);
        }

        // ---------------------------------------------------------------
        // Prueba 2: factura no encontrada
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessReminderAsync_InvoiceNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var invoiceId = ObjectId.GenerateNewId().ToString();

            _invoiceRepository
                .Setup(r => r.GetByIdAsync(invoiceId))
                .ReturnsAsync((Invoice?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _invoiceService.ProcessReminderAsync(invoiceId));

            Assert.Contains("La factura no existe", ex.Message);

            // Debe cortar desde el inicio
            VerifyClientNeverQueried();
            VerifyNoEmailSent();
            VerifyNoNotificationRegistered();
            VerifyInvoiceNotUpdated();
        }

        // ---------------------------------------------------------------
        // Prueba 3: cliente no encontrado
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessReminderAsync_ClientNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var invoiceId = ObjectId.GenerateNewId().ToString();
            var clientId = ObjectId.GenerateNewId();

            var invoice = CreateInvoice(invoiceId, clientId, "primerrecordatorio", "FAC-002");

            _invoiceRepository
                .Setup(r => r.GetByIdAsync(invoiceId))
                .ReturnsAsync(invoice);

            _clientRepository
                .Setup(r => r.GetByIdAsync(clientId.ToString()))
                .ReturnsAsync((Client?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _invoiceService.ProcessReminderAsync(invoiceId));

            Assert.Contains("No se encontró el cliente", ex.Message);

            // Prueba que realmente llegó a buscar al cliente
            _clientRepository.Verify(
                r => r.GetByIdAsync(clientId.ToString()),
                Times.Once);

            VerifyNoEmailSent();
            VerifyNoNotificationRegistered();
            VerifyInvoiceNotUpdated();
        }

        // ---------------------------------------------------------------
        // Prueba 4: factura ya desactivada
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessReminderAsync_InvoiceIsDisabled_ThrowsInvalidOperationException()
        {
            // Arrange
            var invoiceId = ObjectId.GenerateNewId().ToString();

            var invoice = CreateInvoice(
                invoiceId, ObjectId.GenerateNewId(), "desactivado", "FAC-003");

            _invoiceRepository
                .Setup(r => r.GetByIdAsync(invoiceId))
                .ReturnsAsync(invoice);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _invoiceService.ProcessReminderAsync(invoiceId));

            Assert.Contains("ya está desactivada", ex.Message);

            // La excepción sale del estado, antes de buscar al cliente
            VerifyClientNeverQueried();
            VerifyNoEmailSent();
            VerifyNoNotificationRegistered();
            VerifyInvoiceNotUpdated();
        }

        // ---------------------------------------------------------------
        // Prueba 5: estado desconocido
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessReminderAsync_UnknownStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var invoiceId = ObjectId.GenerateNewId().ToString();

            var invoice = CreateInvoice(
                invoiceId, ObjectId.GenerateNewId(), "estado_inventado", "FAC-004");

            _invoiceRepository
                .Setup(r => r.GetByIdAsync(invoiceId))
                .ReturnsAsync(invoice);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _invoiceService.ProcessReminderAsync(invoiceId));

            Assert.Contains("no permite procesar recordatorios", ex.Message);

            VerifyClientNeverQueried();
            VerifyNoEmailSent();
            VerifyNoNotificationRegistered();
            VerifyInvoiceNotUpdated();
        }


        // ---------------------------------------------------------------
        // Prueba 6: falla el envío del correo 
        // ---------------------------------------------------------------

        [Fact]
        public async Task ProcessReminderAsync_EmailFails_RegistersFailedNotificationAndThrows()
        {
            // Arrange
            var invoiceId = ObjectId.GenerateNewId().ToString();
            var clientId = ObjectId.GenerateNewId();

            var invoice = CreateInvoice(invoiceId, clientId, "primerrecordatorio", "FAC-003");
            var client = CreateClient(clientId);

            Notifications? savedNotification = null;

            _invoiceRepository
                .Setup(r => r.GetByIdAsync(invoiceId))
                .ReturnsAsync(invoice);

            _clientRepository
                .Setup(r => r.GetByIdAsync(clientId.ToString()))
                .ReturnsAsync(client);

            _emailService
                .Setup(e => e.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SMTP caído"));

            _notificationRepository
                .Setup(r => r.CreateAsync(It.IsAny<Notifications>()))
                .Callback<Notifications>(n => savedNotification = n)
                .Returns(Task.CompletedTask);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _invoiceService.ProcessReminderAsync(invoiceId));

            Assert.Contains("No se pudo enviar", ex.Message);

            // Se registró la notificación fallida con el detalle del error
            Assert.NotNull(savedNotification);
            Assert.Equal("fallido", savedNotification!.result);
            Assert.Equal("SMTP caído", savedNotification.error);
            Assert.Null(savedNotification.acceptedAt);

            _notificationRepository.Verify(
                r => r.CreateAsync(It.IsAny<Notifications>()),
                Times.Once);

            // La factura NO debe cambiar de estado si el correo falló
            VerifyInvoiceNotUpdated();
        }
    }
}