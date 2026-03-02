# 🚩 Reporte de Pruebas de Sistema - QA

## 1. Resumen Ejecutivo
* [cite_start]**Estado del Sistema:** 🔴 **CRÍTICO / BLOQUEADO**.
* [cite_start]**Resumen:** El sistema es inoperante debido a fallos en la comunicación con Telegram y errores de configuración básica[cite: 3, 4].

## 2. Detalle de Hallazgos

### ❌ TC-001: Conexión con Gateway (Telegram)
* [cite_start]**Resultado:** FALLIDO[cite: 8].
* [cite_start]**Hallazgo:** El contenedor `gateway` falla por falta de `TELEGRAM_BOT_TOKEN`[cite: 9, 10].
* [cite_start]**Impacto:** Error bloqueante; el chatbot no funciona[cite: 11].

### ⚠️ TC-002: Interfaz Administrativa (Frontend)
* [cite_start]**Resultado:** PARCIAL[cite: 14].
* [cite_start]**Hallazgo:** El servidor Vite inicia en el puerto 5173, pero es inaccesible desde el host por errores en `docker-compose.yml`[cite: 15, 29].

### ❌ TC-003: Microservicios de Backend
* [cite_start]**Resultado:** FALLIDO[cite: 19].
* [cite_start]**Hallazgo:** Excepción `System.IO.IOException` al intentar acceder a `/src/Shared.Core`[cite: 20].
* [cite_start]**Impacto:** Los servicios se reinician constantemente[cite: 21].

---

## 📊 Matriz de Estado de Contenedores

| Contenedor | Acción | ¿Pasó? | Razón |
| :--- | :--- | :--- | :--- |
| **admin-web** | Run | SÍ | [cite_start]Listo en puerto 5173[cite: 27]. |
| **gateway** | Run | NO | [cite_start]Falta Token de Telegram[cite: 27]. |
| **inventario/pagos** | Run | NO | [cite_start]Error en Shared.Core[cite: 27]. |