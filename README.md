# Chatbot E-commerce System

Sistema de comercio electronico asistido por chatbot para micro-negocios. El proyecto combina microservicios .NET 8, un gateway Node.js, PostgreSQL, un panel administrativo React y una integracion de pagos con Wompi.

## Tabla de contenido

- [Resumen](#resumen)
- [Arquitectura](#arquitectura)
- [Tecnologias](#tecnologias)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Servicios](#servicios)
- [Modelo de datos](#modelo-de-datos)
- [Endpoints principales](#endpoints-principales)
- [Variables de entorno](#variables-de-entorno)
- [Ejecucion con Docker](#ejecucion-con-docker)
- [Desarrollo local](#desarrollo-local)
- [Migraciones y base de datos](#migraciones-y-base-de-datos)
- [Autenticacion y roles](#autenticacion-y-roles)
- [Notificaciones en tiempo real](#notificaciones-en-tiempo-real)
- [Pagos con Wompi](#pagos-con-wompi)
- [Bot de Telegram](#bot-de-telegram)
- [Panel administrativo](#panel-administrativo)
- [Seguridad](#seguridad)
- [Troubleshooting](#troubleshooting)

## Resumen

La solucion permite administrar catalogos, categorias, clientes, usuarios, pedidos, pagos, conversaciones y mensajes desde un panel web. El cliente final interactua con un bot de Telegram para consultar productos, armar un carrito, crear pedidos y recibir enlaces de pago.

El backend esta dividido por responsabilidades:

- `Shared.Core`: dominio compartido, contexto EF Core, entidades, configuraciones y migraciones.
- `Services.Inventario`: API principal para inventario, usuarios, clientes, pedidos, conversaciones, mensajes, pagos administrativos, autenticacion JWT y notificaciones SignalR.
- `Services.Pagos`: integracion con Wompi, creacion de enlaces de pago, webhook y worker de timeout de pagos pendientes.
- `Services.ChatBot`: servicio .NET para recibir updates de Telegram y ejecutar el flujo conversacional.
- `Gateway.NodeJS`: gateway HTTP para el panel y el bot; proxy hacia Inventario/Pagos y agregador de metricas del dashboard.
- `Admin.Web`: panel administrativo en React, Vite y Tailwind CSS.

## Arquitectura

```text
Telegram
   |
   v
Gateway.NodeJS ---------------> Services.ChatBot
   |                                  |
   |                                  v
   |                           Shared.Core / PostgreSQL
   |
   +---- /api/admin/* -------> Services.Inventario
   |                              |       |
   |                              |       +---- SignalR /notificationHub
   |                              v
   |                         PostgreSQL
   |
   +---- /api/pagos/* ------> Services.Pagos ----> Wompi
                                  |
                                  v
                            Services.Inventario

Admin.Web <---- HTTP/API + JWT ---- Gateway.NodeJS
Admin.Web <---- SignalR ---------- Services.Inventario
```

### Flujo principal

1. El usuario administra el sistema desde `Admin.Web`.
2. `Admin.Web` consume `/api/*` del `Gateway.NodeJS`.
3. El gateway valida JWT y reenvia las operaciones administrativas a `Services.Inventario` o `Services.Pagos`.
4. `Services.Inventario` persiste datos en PostgreSQL mediante `Shared.Core`.
5. El bot de Telegram entra por el gateway y se reenvia a `Services.ChatBot`.
6. El chatbot consulta inventario, registra clientes/conversaciones/pedidos y solicita pagos.
7. `Services.Pagos` crea enlaces de Wompi y recibe webhooks de confirmacion.
8. `Services.Inventario` emite notificaciones por SignalR al panel administrativo.

## Tecnologias

### Backend .NET

- .NET 8, configurado en `global.json`.
- ASP.NET Core Web API.
- Entity Framework Core 8.
- Npgsql para PostgreSQL.
- Swagger / OpenAPI.
- JWT Bearer Authentication.
- FluentValidation.
- SignalR para notificaciones en tiempo real.
- Hosted Services para procesos en segundo plano.
- BCrypt para hashing de contrasenas.

### Gateway

- Node.js con modulos ES.
- Express.
- Axios.
- CORS.
- JSON Web Token.
- `node-telegram-bot-api`.
- Nodemon para desarrollo.

### Frontend

- React 18.
- Vite 5.
- React Router DOM.
- Tailwind CSS.
- Axios.
- Recharts.
- SignalR client.
- QRCode.

### Infraestructura

- PostgreSQL 15 Alpine.
- Docker y Docker Compose.
- Volumen persistente para datos de PostgreSQL.

## Estructura del repositorio

```text
.
|-- Backend.sln
|-- docker-compose.yml
|-- docker-compose.dev.yml
|-- docker-compose.dev2.yml
|-- .env.example
|-- global.json
|-- src
|   |-- Admin.Web
|   |-- Gateway.NodeJS
|   |-- Services.ChatBot
|   |-- Services.Inventario
|   |-- Services.Pagos
|   `-- Shared.Core
`-- test
```

## Servicios

### Shared.Core

Proyecto compartido por los microservicios .NET. Contiene:

- `ApplicationDbContext`.
- Entidades del dominio.
- Configuraciones de mapeo EF Core.
- Migraciones.
- Extension `AddSharedInfrastructure` para registrar PostgreSQL con Npgsql.

### Services.Inventario

API principal del dominio administrativo. Responsabilidades:

- CRUD de productos, categorias, clientes, usuarios, pedidos, conversaciones y mensajes.
- CRUD y actualizacion interna de pagos.
- Autenticacion por JWT.
- Validacion de token en APIs y SignalR.
- Reserva, confirmacion y cancelacion de stock.
- Health check en `/health`.
- Swagger en desarrollo.
- Hub de notificaciones en `/notificationHub`.

Controladores principales:

- `AuthController`
- `InventarioController`
- `CategoriaController`
- `ClienteController`
- `UsuarioController`
- `PedidoController`
- `PagosController`
- `ConversacionController`
- `MensajeController`
- `NotificationController`

### Services.Pagos

Microservicio de pagos. Responsabilidades:

- Crear enlaces automaticos de pago con Wompi.
- Recibir webhooks de Wompi.
- Redireccionar despues del pago.
- Consultar/actualizar el estado del pago en Inventario.
- Ejecutar `PagoTimeoutWorker`, que marca pagos pendientes como rechazados cuando exceden el tiempo configurado.

Configuracion relevante:

- `Services:InventarioBaseUrl`
- `Payments:PendingTimeoutMinutes`
- `Payments:PendingPollingSeconds`
- `Payments:TimeoutRejectReason`
- `Wompi:*`

### Services.ChatBot

Servicio de bot conversacional en .NET. Responsabilidades:

- Recibir updates de Telegram.
- Registrar y persistir conversaciones.
- Renderizar menus, catalogo y carrito.
- Consultar APIs internas para productos/categorias.
- Gestionar pagos desde el flujo del bot.
- Liberar stock reservado por carritos abandonados mediante `StockReleaseWorker`.

Modulos relevantes:

- `MenuModule`
- `CatalogoModule`
- `CarritoModule`
- `UpdateHandler`
- `BotRenderer`
- `BotInteractionHandler`
- `BotOnMsgInteractionHandler`
- `PaymentService`
- `SqlBotPersistence`

### Gateway.NodeJS

Gateway HTTP y capa de entrada para el panel. Responsabilidades:

- Exponer `/health`.
- Reenviar login a `Services.Inventario`.
- Validar JWT en rutas administrativas.
- Proteger rutas por rol.
- Proxy de `/api/admin/inventario/*` hacia Inventario.
- Proxy de `/api/admin/pagos/*` hacia Inventario o Pagos segun la ruta.
- Agregar metricas del dashboard en `/api/admin/dashboard/stats`.
- Reenviar webhooks/updates hacia el servicio del chatbot.
- Proxy interno para consultas de inventario desde el bot.

### Admin.Web

Panel administrativo. Responsabilidades:

- Login y almacenamiento de JWT en `localStorage`.
- Rutas protegidas por rol.
- Gestion de dashboard, inventario, categorias, pedidos, pagos, clientes, usuarios, conversaciones y mensajes.
- Conexion SignalR para notificaciones.
- Modo oscuro con `useDarkMode`.

Rutas visibles:

- `/`
- `/inventario`
- `/categorias`
- `/pedidos`
- `/pagos`
- `/clientes`
- `/usuarios`
- `/conversaciones`
- `/mensajes`

## Modelo de datos

Entidades principales en `Shared.Core`:

- `Categoria`: agrupacion de productos.
- `Producto`: catalogo, precio, imagen, estado activo, stock total, reservado y disponible.
- `Cliente`: datos de contacto, Telegram ID, WhatsApp ID, historial y relaciones con pedidos/conversaciones.
- `Usuario`: usuario administrativo con rol `Administrador` o `Vendedor`.
- `Pedido`: orden de compra, estado, total, direccion, referencia Wompi y productos asociados.
- `PedidoProducto`: relacion entre pedido y producto, con cantidad y precio unitario.
- `Pago`: pago asociado a pedido, monto, metodo, estado y referencia externa.
- `Conversacion`: conversacion asociada a un cliente.
- `Mensaje`: mensaje dentro de una conversacion, con remitente cliente/soporte/sistema.
- `Notificacion`: payload usado para notificaciones internas.

Estados relevantes:

- `EstadoPedido`: `Pendiente`, `Confirmado`, `Pagado`, `Enviado`, `Cancelado`.
- `EstadoPago`: `Pendiente`, `Completado`, `Rechazado`, `Cancelado`.
- `Roles`: `Administrador`, `Vendedor`.

## Endpoints principales

### Gateway

- `GET /health`: estado del gateway.
- `POST /api/auth/login`: login; reenvia a Inventario.
- `GET /api/debug/sessions`: estadisticas de sesiones en desarrollo.
- `POST /api/webhook`: recibe update y lo reenvia al servicio del chatbot.
- `ALL /api/pagos/*`: proxy hacia Pagos.
- `GET /api/proxy/inventario/*`: proxy interno de lectura hacia Inventario.
- `ALL /api/admin/inventario/*`: proxy administrativo hacia Inventario.
- `ALL /api/admin/pagos/*`: proxy administrativo hacia Pagos/Inventario.
- `GET /api/admin/dashboard/stats`: metricas agregadas para el dashboard.

### Inventario

Base administrativa: `/api/inventario`

- Productos:
  - `GET /productos`
  - `GET /productos/paged`
  - `GET /productos/{id}`
  - `POST /productos`
  - `PUT /productos/{id}`
  - `DELETE /productos/soft-delete/{id}`
  - `DELETE /productos/{id}`
  - `GET /productos/list-4/{categoriaId}`
  - `POST /reservar`
  - `POST /confirmar-reserva`
  - `POST /cancelar-reserva`
- Categorias:
  - `GET /categorias`
  - `GET /categorias/paged`
  - `GET /categorias/{id}`
  - `POST /categorias`
  - `PUT /categorias/{id}`
  - `DELETE /categorias/{id}`
  - `GET /categorias/list-6`
- Clientes:
  - `GET /clientes`
  - `GET /clientes/paged`
  - `GET /clientes/{id}`
  - `POST /clientes`
  - `PUT /clientes/{id}`
  - `DELETE /clientes/{id}`
- Usuarios:
  - `GET /usuarios`
  - `GET /usuarios/paged`
  - `GET /usuarios/{id}`
  - `POST /usuarios`
  - `PUT /usuarios/{id}`
  - `DELETE /usuarios/{id}`
- Pedidos:
  - `GET /pedidos`
  - `GET /pedidos/paged`
  - `GET /pedidos/{id}`
  - `POST /pedidos`
  - `PUT /pedidos/{id}`
  - `DELETE /pedidos/{id}`
- Conversaciones:
  - `GET /conversaciones`
  - `GET /conversaciones/paged`
  - `GET /conversaciones/{id}`
  - `POST /conversaciones`
  - `PUT /conversaciones/{id}`
  - `DELETE /conversaciones/{id}`
- Mensajes:
  - `GET /mensajes`
  - `GET /mensajes/paged`
  - `GET /mensajes/{id}`
  - `POST /mensajes`
  - `PUT /mensajes/{id}`
  - `DELETE /mensajes/{id}`

Otros endpoints:

- `POST /api/auth/login`
- `POST /api/Notification/send`
- `GET /health`
- `GET /notificationHub`

### Pagos

Base: `/api/pagos`

- `GET /redirect`: endpoint de redireccion despues del pago.
- `POST /crear-enlace-automatico/{pedidoId}`: crea enlace de pago Wompi para un pedido.
- `POST /webhook/wompi`: recibe eventos de Wompi.
- `POST /`: endpoint auxiliar de pagos.

### Pagos en Inventario

Base: `/api/pagos`

- `GET /`
- `GET /paged`
- `GET /{id}`
- `GET /pedido/{pedidoId}`
- `POST /`
- `PUT /{id}`
- `DELETE /{id}`
- `POST /actualizar-por-referencia/{referencia?}`
- `POST /marcar-rechazado/{referencia}`

### ChatBot

Base: `/api/Bot`

- `GET /setWebhook`
- `POST /`
- `POST /pago-procesando`
- `POST /pagos-completado`

## Variables de entorno

Crear un archivo `.env` a partir de `.env.example` antes de ejecutar con Docker Compose.

### Base de datos

```env
DB_HOST=postgres
DB_PORT=5432
DB_NAME=chatbot_ecommerce
DB_USER=postgres
DB_PASSWORD=change_me
```

### JWT

```env
JWT_SECRET=change_me_to_a_long_random_secret
JWT_EXPIRATION=10h
```

### Usuario administrador inicial

```env
CONTRASENA_ADMIN_HASH=change_me_bcrypt_hash
```

El proyecto usa hashes BCrypt para contrasenas. No guardes contrasenas planas en variables ni en codigo.

### Wompi

```env
WOMPI_CLIENT_ID=your_wompi_client_id
WOMPI_CLIENT_SECRET=your_wompi_client_secret
WOMPI_WEBHOOK_URL=https://your-public-url/api/pagos/webhook/wompi
WOMPI_WEBHOOK_SECRET=your_webhook_secret
WOMPI_REDIRECT_URL=https://your-public-url/api/pagos/redirect
WOMPI_NOTIFICATION_EMAILS=email@example.com
```

### Telegram

```env
TELEGRAM_BOT_TOKEN=your_telegram_bot_token_from_botfather
TELEGRAM_WEBHOOK_URL=https://your-public-url/api/webhook
TELEGRAM_SECRET_TOKEN=your_optional_telegram_secret_token
```

### URLs internas

```env
INVENTARIO_SERVICE_URL=http://inventario-service:8080
PAGOS_SERVICE_URL=http://pagos-service:8080
CHATBOT_SERVICE_URL=http://chatbot-service:8080
IDENTITY_SERVICE_URL=http://inventario-service:8080
ViteUrl=http://localhost:5173
```

### Gateway y frontend

```env
GATEWAY_PORT=3000
NODE_ENV=development
ADMIN_PANEL_URL=http://localhost:5173
VITE_API_URL=http://localhost:3000
VITE_SIGNALR_URL=http://localhost:5001/notificationHub
VITE_NOTIFICATIONS_URL=http://inventario-service:8080
VITE_ALLOWED_HOST=localhost
```

## Ejecucion con Docker

### Produccion/local simple

```bash
docker compose up --build
```

Servicios publicados por `docker-compose.yml`:

| Servicio | URL local | Puerto interno |
| --- | --- | --- |
| Admin Web | `http://localhost` | `80` |
| Gateway | `http://localhost:3000` | variable `GATEWAY_PORT` |
| Inventario | `http://localhost:5001` | `8080` |
| Pagos | `http://localhost:5002` | `8080` |
| PostgreSQL | `localhost:5432` | `5432` |

### Desarrollo con volumenes

```bash
docker compose -f docker-compose.dev.yml up --build
```

Este compose monta carpetas locales para facilitar desarrollo. PostgreSQL guarda datos en `./postgres_data`.

Nota: revisar los nombres de rutas en `docker-compose.dev.yml` si se usa el servicio `chatbot-service`; el proyecto real en el repositorio se llama `Services.ChatBot`.

## Desarrollo local

### Requisitos

- .NET SDK 8.
- Node.js 20 o superior.
- PostgreSQL 15.
- Docker y Docker Compose, opcional pero recomendado.

### Restaurar y compilar backend

```bash
dotnet restore Backend.sln
dotnet build Backend.sln
```

### Ejecutar Inventario

```bash
cd src/Services.Inventario
dotnet run
```

### Ejecutar Pagos

```bash
cd src/Services.Pagos
dotnet run
```

### Ejecutar ChatBot

```bash
cd src/Services.ChatBot
dotnet run
```

### Ejecutar Gateway

```bash
cd src/Gateway.NodeJS
npm install
npm run dev
```

### Ejecutar Admin Web

```bash
cd src/Admin.Web
npm install
npm run dev
```

Vite queda normalmente en:

```text
http://localhost:5173
```

## Migraciones y base de datos

Las migraciones viven en `src/Shared.Core/Migrations` y el contexto es `ApplicationDbContext`.

Aplicar migraciones:

```bash
dotnet ef database update --project src/Shared.Core --startup-project src/Services.Inventario
```

Crear una nueva migracion:

```bash
dotnet ef migrations add NombreMigracion --project src/Shared.Core --startup-project src/Services.Inventario
```

La cadena de conexion se lee desde:

```text
ConnectionStrings:DefaultConnection
```

En Docker se inyecta como:

```text
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=...;Username=...;Password=...
```

## Autenticacion y roles

La autenticacion usa JWT.

- `Admin.Web` guarda el token en `localStorage`.
- `Gateway.NodeJS` valida el token en rutas `/api/admin/*`.
- `Services.Inventario` valida JWT para endpoints protegidos y SignalR.
- SignalR recibe el token por query string `access_token`, porque WebSocket en navegador no permite headers personalizados de forma estandar.

Roles:

- `Administrador`: acceso completo en el panel.
- `Vendedor`: acceso limitado a dashboard, inventario, pedidos, clientes y pagos.

Restricciones aplicadas en gateway:

- Usuarios, conversaciones y mensajes requieren Administrador.
- Categorias permiten `GET` para todos los roles autenticados, pero escritura requiere Administrador.

## Notificaciones en tiempo real

`Services.Inventario` expone:

```text
/notificationHub
```

El cliente React se conecta con `@microsoft/signalr` y escucha:

```text
ReceiveNotification
```

Tambien existe:

```text
POST /api/Notification/send
```

para enviar notificaciones hacia clientes conectados.

## Pagos con Wompi

El flujo de pagos esta dividido en dos partes:

1. `Services.Pagos` crea el enlace de pago para Wompi.
2. Wompi llama al webhook `/api/pagos/webhook/wompi`.
3. `Services.Pagos` actualiza el pago/pedido en `Services.Inventario`.
4. El `PagoTimeoutWorker` consulta pagos pendientes y marca como rechazados aquellos que exceden el timeout configurado.

Variables importantes:

- `WOMPI_CLIENT_ID`
- `WOMPI_CLIENT_SECRET`
- `WOMPI_WEBHOOK_URL`
- `WOMPI_WEBHOOK_SECRET`
- `WOMPI_REDIRECT_URL`
- `Services__InventarioBaseUrl`
- `Payments__PendingTimeoutMinutes`
- `Payments__PendingPollingSeconds`

Para probar webhooks localmente se necesita una URL publica, por ejemplo con ngrok, localtunnel o un reverse proxy.

## Bot de Telegram

Configuracion basica:

1. Crear el bot con `@BotFather`.
2. Copiar el token en `TELEGRAM_BOT_TOKEN`.
3. Configurar `TELEGRAM_WEBHOOK_URL` apuntando al gateway.
4. Ejecutar gateway y chatbot.
5. Llamar el endpoint de configuracion de webhook si aplica:

```text
GET /api/Bot/setWebhook
```

El gateway recibe updates en:

```text
POST /api/webhook
```

y los reenvia al servicio del chatbot.

## Panel administrativo

El panel usa React Router y protege las vistas segun rol.

Paginas principales:

- Dashboard.
- Inventario.
- Categorias.
- Pedidos.
- Pagos.
- Clientes.
- Usuarios.
- Conversaciones.
- Mensajes.

El cliente HTTP esta en:

```text
src/Admin.Web/src/api/client.js
```

Reglas importantes:

- `VITE_API_URL` define la base de la API.
- Si `VITE_API_URL` no termina en `/api`, el cliente lo agrega.
- Ante `401` o `403`, el cliente borra token y redirige al login.

## Seguridad

Antes de usar en produccion:

- Cambiar `JWT_SECRET` por una clave larga, aleatoria y privada.
- No versionar `.env` con secretos reales.
- Rotar cualquier credencial que haya estado en archivos de ejemplo.
- Usar HTTPS para gateway, admin y webhooks.
- Configurar CORS con origenes exactos, no comodines.
- Reemplazar credenciales iniciales de administrador.
- Validar firma/checksum de webhooks de Wompi.
- Usar un almacen persistente para sesiones del bot si se necesita escalar horizontalmente.
- Considerar Redis para estado temporal y carritos.
- Configurar logs estructurados y monitoreo.
- Restringir acceso directo a microservicios internos en despliegues publicos.

## Troubleshooting

### El gateway no inicia

- Verificar `TELEGRAM_BOT_TOKEN`.
- Verificar que `PORT`/`GATEWAY_PORT` no este ocupado.
- Revisar que `npm install` se haya ejecutado en `src/Gateway.NodeJS`.

### El panel redirige al login

- El token pudo expirar.
- Revisar que `JWT_SECRET` sea el mismo en Inventario y Gateway.
- Revisar que el rol venga como `role` o claim de rol de Microsoft.

### SignalR no conecta

- Verificar `VITE_SIGNALR_URL` o proxy `/notificationHub`.
- Verificar que el token se este enviando como `access_token`.
- Revisar CORS en `Services.Inventario`.

### No se conecta a PostgreSQL

- Confirmar que el contenedor `chatbot-db` este saludable.
- Verificar `DB_NAME`, `DB_USER`, `DB_PASSWORD`.
- En Docker, usar host `postgres`; en ejecucion local, usar `localhost` o el host real.
- Ejecutar migraciones.

### Wompi no llama el webhook

- Usar una URL publica HTTPS.
- Verificar `WOMPI_WEBHOOK_URL`.
- Revisar logs de `Services.Pagos`.
- Confirmar que el gateway/proxy reenvie headers requeridos como `x-event-checksum`.

### El bot no responde

- Verificar token de Telegram.
- Confirmar que el webhook apunta al gateway correcto.
- Revisar que `Services.ChatBot` este levantado.
- Revisar `CHATBOT_SERVICE_URL`.

## Comandos utiles

```bash
# Ver estado de contenedores
docker compose ps

# Ver logs
docker compose logs -f gateway
docker compose logs -f inventario-service
docker compose logs -f pagos-service

# Compilar solucion .NET
dotnet build Backend.sln

# Construir frontend
cd src/Admin.Web
npm run build
```

## Notas de mantenimiento

- Mantener las migraciones en `Shared.Core`.
- Evitar duplicar entidades entre servicios.
- Mantener el gateway como capa de proxy/agregacion, no como dueno del dominio.
- Agregar pruebas para reglas criticas: reservas de stock, transiciones de pago, autenticacion y roles.
- Documentar nuevas variables de entorno en `.env.example` y en este README.
