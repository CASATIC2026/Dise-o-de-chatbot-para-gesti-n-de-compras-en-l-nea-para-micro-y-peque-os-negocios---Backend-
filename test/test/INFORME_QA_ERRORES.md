# Informe de Control de Calidad (QA) - Proyecto Chatbot

**Fecha:** 
**Analista:** 
**Estado del Entorno:** En Pruebas

---

## 🛑 Incidencia Detectada: QA-001
**Componente:** Infraestructura / Persistencia de Datos
**Fallo:** El contenedor de PostgreSQL aborta el inicio (Exited 1).                                            

### Análisis de la Causa
Al revisar los logs en Docker Desktop, se identificó el mensaje:
> `Error: Database is uninitialized and superuser password is not specified.`

**Origen:** Falta de la variable de entorno `POSTGRES_PASSWORD` en la configuración inicial del contenedor.

### Solución Propuesta
Se creó un archivo `docker-compose.yml` para estandarizar el despliegue e inyectar las credenciales necesarias.

### Resultado de la Prueba de Humo (Smoke Test)
- [x] Contenedor de BD en estado "Running".
- [x] Logs confirman: "database system is ready to accept connections".
- [ ] Conexión con Microservicios (Pendiente).