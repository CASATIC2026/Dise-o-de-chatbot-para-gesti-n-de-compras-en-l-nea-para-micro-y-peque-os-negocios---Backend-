# Chatbot E-commerce System

Sistema de chatbot para micro-negocios con arquitectura híbrida de microservicios.

## 🏗️ Arquitectura

- **Shared.Core**: Librería compartida con entidades (.NET 8)
- **Services.Inventario**: Microservicio de gestión de inventario (.NET 8)
- **Services.Pagos**: Microservicio de pagos con integración Wompi (.NET 8)
- **Gateway.NodeJS**: Orquestador con bot de Telegram (Node.js)
- **Admin.Web**: Panel administrativo (React + Vite + Tailwind)

## 📦 Requisitos

- .NET 8 SDK
- Node.js 20+
- PostgreSQL 15
- Docker & Docker Compose (para deployment)

## 🚀 Inicio Rápido

### 1. Configurar Variables de Entorno

Copia `.env.example` a `.env` y configura:
- `DB_PASSWORD`: Contraseña de PostgreSQL
- `JWT_SECRET`: Clave secreta para JWT
- `TELEGRAM_BOT_TOKEN`: Token del bot de Telegram
- `WOMPI_CLIENT_ID` y `WOMPI_CLIENT_SECRET`: Credenciales de Wompi SV

### 2. Iniciar con Docker Compose

```bash
docker-compose up --build
```

Servicios disponibles:
- **Gateway**: http://localhost:3000
- **Inventario**: http://localhost:5001
- **Pagos**: http://localhost:5002
- **Admin Web**: http://localhost:80
- **PostgreSQL**: localhost:5432

### 3. Desarrollo Local

#### Backend (.NET)

```bash
# Restaurar dependencias
dotnet restore

# Crear migraciones
cd src/Shared.Core
dotnet ef migrations add InitialCreate

# Aplicar migraciones
dotnet ef database update

# Ejecutar servicios
cd src/Services.Inventario
dotnet run

cd src/Services.Pagos
dotnet run
```

#### Gateway (Node.js)

```bash
cd src/Gateway.NodeJS
npm install
npm run dev
```

#### Admin Panel (React)

```bash
cd src/Admin.Web
npm install
npm run dev
```

## 🤖 Configurar Bot de Telegram

1. Habla con [@BotFather](https://t.me/botfather) en Telegram
2. Crea un nuevo bot con `/newbot`
3. Copia el token y agrégalo a `.env` como `TELEGRAM_BOT_TOKEN`
4. Inicia el bot con `/start`

## 📊 Panel Administrativo

Accede a http://localhost:5173

**Credenciales por defecto:**
- Usuario: `admin`
- Contraseña: `admin123`

## 🗄️ Base de Datos

El sistema usa PostgreSQL con las siguientes tablas:
- `Productos`: Catálogo de productos
- `Usuarios`: Usuarios del chatbot
- `Pedidos`: Órdenes de compra

## 💳 Pagos con Wompi

1. Crea una cuenta en [Wompi](https://wompi.com/)
2. Obtén tus claves de prueba
3. Configura las claves en `.env`

## 📝 Notas de Desarrollo

- El StateManager usa memoria en RAM (para producción, migrar a Redis)
- Las imágenes de productos se guardan como URLs (para producción, usar S3/CloudStorage)
- El JWT no usa refresh tokens en esta versión

## 🔒 Seguridad

⚠️ **IMPORTANTE**: Cambia las credenciales por defecto antes de producción:
- Administrator password en el panel web
- JWT_SECRET en variables de entorno
- Credenciales de base de datos

## 📚 Documentación Adicional

- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Node-Telegram-Bot-API](https://github.com/yagop/node-telegram-bot-api)
- [Wompi API Docs](https://docs.wompi.co/)
- [React Router](https://reactrouter.com/)
- [Tailwind CSS](https://tailwindcss.com/)

## 🐛 Troubleshooting

**El bot no responde:**
- Verifica que el token de Telegram sea correcto
- Asegúrate de que el Gateway esté corriendo

**Error de conexión a la base de datos:**
- Verifica que PostgreSQL esté corriendo
- Revisa las credenciales en `.env`

**Los microservicios no se comunican:**
- Verifica que todos los servicios estén corriendo
- Revisa los puertos en `docker-compose.yml`
