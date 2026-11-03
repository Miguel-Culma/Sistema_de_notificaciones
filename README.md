# Sistema de Notificaciones de Facturas

Sistema web desarrollado como prueba técnica para la gestión de facturas y el envío automático de recordatorios a clientes, mediante una API REST y una interfaz gráfica.

## 1. Descripción del proyecto

El sistema permite gestionar facturas y automatizar el proceso de notificaciones según el estado de cada una.

Su funcionamiento contempla las siguientes transiciones:

- `primerrecordatorio`: se envía un correo informando al cliente sobre el segundo recordatorio y se actualiza el estado de la factura.
- `segundorecordatorio`: se envía un correo informando sobre la próxima desactivación y se actualiza el estado de la factura a `desactivado`.
- `desactivado`: representa el estado final del proceso de recordatorios.

El sistema también permite realizar cambios manuales de estado mediante la interfaz y consultar la información de las facturas.

## 2. Tecnologías utilizadas

| Componente | Tecnologías |
|---|---|
| Backend | C#, ASP.NET Core, .NET 10 |
| Frontend | React, JavaScript, Vite |
| Base de datos | MongoDB Atlas |
| Autenticación | JWT |
| Envío de correos | Gmail SMTP |
| Pruebas | xUnit |
| Control de versiones | Git y GitHub |

## 3. Estructura del proyecto

```text
Sistema_de_notificaciones/
│
├── Backend/
│   ├── BackgroundServices/
│   ├── Configurations/
│   ├── Context/
│   ├── Controllers/
│   ├── Models/
│   ├── Properties/
│   ├── Repositories/
│   ├── Services/
│   ├── Program.cs
│   └── PruebaTecnica_MonoLegal.csproj
│
├── Backend_Tests/
│   ├── BackgroundServices/
│   ├── Services/
│   └── PruebaTecnica_MonoLegal_Test.csproj
│
├── Frontend/
│   ├── public/
│   ├── src/
│   ├── package.json
│   ├── package-lock.json
│   
│
└── .gitignore
```

### Componentes principales

- **Backend:** contiene la API REST, la lógica de negocio, los repositorios, la autenticación y el servicio de recordatorios automáticos.
- **Backend_Tests:** contiene las pruebas automatizadas del backend.
- **Frontend:** contiene la interfaz web desarrollada con React.

## 4. Requisitos previos

Para ejecutar el proyecto se necesitan las siguientes herramientas:

| Herramienta | Versión utilizada |
|---|---|
| Git | Instalado |
| .NET SDK | 10.0.403 |
| Node.js | 26.13.0 |
| npm | 11.20.0 |
| MongoDB Atlas | Acceso a la base de datos |
| Visual studio | Entorno de Desarrollo Integrado |

También se requiere acceso a una cuenta de Gmail con contraseña de aplicación para el envío de correos.

## 5. Clonación del repositorio

Desde una terminal, ejecute:

```bash
git clone https://github.com/Miguel-Culma/Sistema_de_notificaciones.git
```

Ingrese a la carpeta del proyecto:

```bash
cd Sistema_de_notificaciones
```

## 6. Configuración de credenciales

El sistema utiliza credenciales para conectarse a MongoDB Atlas, gestionar la autenticación JWT y enviar correos mediante Gmail SMTP.

Por seguridad, estos valores no se encuentran publicados en el repositorio.

Las credenciales e instrucciones de acceso necesarias para la revisión serán proporcionadas de manera privada en el documento de entrega.

### 6.1. Configuración de User Secrets

Desde la carpeta `Backend`, configure las siguientes claves mediante .NET User Secrets:

```powershell
cd Backend
```

Si el proyecto todavía no tiene inicializado User Secrets, ejecute:

```powershell
dotnet user-secrets init
```

Configure los valores proporcionados:

```powershell
dotnet user-secrets set "ConnectionStrings:MongoDbUri" "CADENA_DE_CONEXION"

dotnet user-secrets set "JwtSettings:SecretKey" "CLAVE_JWT"

dotnet user-secrets set "EmailSettings:SenderEmail" "CORREO_GMAIL"

dotnet user-secrets set "EmailSettings:Password" "CONTRASEÑA_DE_APLICACION"

dotnet user-secrets set "EmailSettings:Username" "USUARIO_GMAIL"

dotnet user-secrets set "FrontendUrl" "URL FRONTEND"

```

Los valores de ejemplo deben reemplazarse por las credenciales correspondientes.

## 7. Ejecución del backend

Desde la carpeta `Backend`, restaure las dependencias:

```powershell
dotnet restore
```

Compile el proyecto:

```powershell
dotnet build
```

Ejecute la API utilizando el perfil HTTP:

```powershell
dotnet run --launch-profile http
```

La API estará disponible en:

```text
http://localhost:5072
```

El backend también cuenta con un perfil HTTPS configurado en el puerto `7234`.

## 8. Ejecución del frontend

Abra una segunda terminal desde la carpeta raíz del proyecto.

Ingrese a `Frontend`:

```powershell
cd Frontend
```

### 8.1. Configurar variables de entorno

Cree el archivo `.env` 

Verifique que el archivo `.env` contenga la dirección del backend:

```env
VITE_API_URL=http://localhost:5072
```

### 8.2. Instalar dependencias

```powershell
npm install
```

### 8.3. Compilar el frontend

```powershell
npm run build
```

### 8.4. Iniciar la aplicación

```powershell
npm run dev
```

La interfaz estará disponible en:

```text
http://localhost:5173
```

**Importante:** mantenga ejecutándose tanto el backend como el frontend para utilizar el sistema correctamente.

## 9. Ejecución de pruebas automatizadas

El proyecto cuenta con pruebas automatizadas desarrolladas con xUnit.

Desde una tercera terminal, ingrese a la carpeta de pruebas:

```powershell
cd Backend_Tests
```

Restaure las dependencias:

```powershell
dotnet restore
```

Ejecute las pruebas:

```powershell
dotnet test
```

Al finalizar, la terminal mostrará el resumen de pruebas aprobadas, fallidas u omitidas.

## 10. Consideraciones importantes

- Se requiere conexión a internet para acceder a MongoDB Atlas y utilizar el servicio de correo.
- Las credenciales deben configurarse localmente mediante .NET User Secrets.
- El archivo `.env` no debe publicarse en GitHub.
- El frontend utiliza el puerto `5173` y el backend utiliza el puerto `5072` en sus perfiles HTTP.
- El servicio de recordatorios automáticos se ejecuta en segundo plano al iniciar el backend.
