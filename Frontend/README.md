# Recordatorios de facturas — Frontend

React + Vite + Tailwind 3 + DaisyUI 4. Consume la API REST en ASP.NET Core con JWT.

## Arranque

```bash
npm install
cp .env.example .env   # ajusta VITE_API_URL
npm run dev
```

## Estructura

```
src/
  assets/       logo
  components/   piezas reutilizables (tabla, botón de recordatorio, rutas protegidas…)
  context/      AuthProvider (JWT, rol admin) y ToastProvider (avisos)
  hooks/        useAuth, useToast, useAsync
  layout/       MainLayout (menú lateral + barra superior)
  pages/        Login, Dashboard, Invoices, InvoiceDetail, ReminderSettings, Notifications
  services/     api.js (fetch + JWT) y un servicio por recurso
  utils/        jwt, formatos y estados de factura
```

## Ajustes que dependen de tu backend

- Nombres de campos: `services/*Service.js` normaliza varias variantes comunes (`invoiceNumber`/`number`, `customerName`/`customer`, etc.). Ajusta ahí si tu API usa otros.
- Body de login: `{ username, password }` (`services/authService.js`).
- Body de configuración: `{ enabled }` y `{ intervalMinutes }` (`services/reminderSettingsService.js`).
- Rol admin: se lee del claim `role`/`roles` (o el claim largo de .NET) del JWT (`utils/jwt.js`).
- CORS: el backend debe permitir el origen de Vite (`http://localhost:5173`).
