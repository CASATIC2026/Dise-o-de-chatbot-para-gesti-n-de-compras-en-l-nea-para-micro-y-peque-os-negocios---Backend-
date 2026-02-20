/**
 * ChatbotEngine - Main chatbot logic with Telegram integration
 */
import TelegramBot from 'node-telegram-bot-api';
import axios from 'axios';
import StateManager from './StateManager.js';

class ChatbotEngine {
    constructor(token, inventarioUrl, pagosUrl) {
        this.bot = new TelegramBot(token, { polling: true });
        this.inventarioUrl = inventarioUrl;
        this.pagosUrl = pagosUrl;

        this.setupHandlers();
    }

    setupHandlers() {
        // Start command
        this.bot.onText(/\/start/, async (msg) => {
            const chatId = msg.chat.id;
            const userId = msg.from.id;

            StateManager.clearState(userId);
            StateManager.setCurrentState(userId, 'MENU');

            await this.sendMenu(chatId, userId);
        });

        // Handle all text messages
        this.bot.on('message', async (msg) => {
            if (msg.text && !msg.text.startsWith('/')) {
                await this.handleMessage(msg);
            }
        });
    }

    async handleMessage(msg) {
        const chatId = msg.chat.id;
        const userId = msg.from.id;
        const text = msg.text.trim();

        const state = StateManager.getState(userId);

        console.log(`[ChatBot] User ${userId} in state ${state.currentState}: ${text}`);

        try {
            switch (state.currentState) {
                case 'MENU':
                    await this.handleMenuSelection(chatId, userId, text);
                    break;
                case 'BROWSING_PRODUCTS':
                    await this.handleProductSelection(chatId, userId, text);
                    break;
                case 'SELECTING_QUANTITY':
                    await this.handleQuantitySelection(chatId, userId, text);
                    break;
                case 'CONTINUE_SHOPPING':
                    await this.handleContinueShopping(chatId, userId, text);
                    break;
                case 'ENTERING_ADDRESS':
                    await this.handleAddressEntry(chatId, userId, text);
                    break;
                case 'CONFIRMING_ORDER':
                    await this.handleOrderConfirmation(chatId, userId, text);
                    break;
                default:
                    await this.sendMenu(chatId, userId);
            }
        } catch (error) {
            console.error('[ChatBot] Error handling message:', error);
            await this.bot.sendMessage(chatId, '❌ Ocurrió un error. Por favor intenta de nuevo.');
            await this.sendMenu(chatId, userId);
        }
    }

    async sendMenu(chatId, userId) {
        const menuText = `
🛍️ *Bienvenido a nuestra tienda*

¿Qué deseas hacer?

1️⃣ Ver catálogo de productos
2️⃣ Ver mi carrito
3️⃣ Finalizar compra
4️⃣ Ayuda

Escribe el número de la opción que deseas.
        `;

        await this.bot.sendMessage(chatId, menuText, { parse_mode: 'Markdown' });
        StateManager.setCurrentState(userId, 'MENU');
    }

    async handleMenuSelection(chatId, userId, text) {
        switch (text) {
            case '1':
                await this.showProducts(chatId, userId);
                break;
            case '2':
                await this.showCart(chatId, userId);
                break;
            case '3':
                await this.startCheckout(chatId, userId);
                break;
            case '4':
                await this.showHelp(chatId);
                break;
            default:
                await this.bot.sendMessage(chatId, '❌ Opción inválida. Por favor selecciona 1, 2, 3 o 4.');
        }
    }

    async showProducts(chatId, userId) {
        try {
            const response = await axios.get(`${this.inventarioUrl}/api/inventario/productos?soloActivos=true`);
            const productos = response.data;

            if (!productos || productos.length === 0) {
                await this.bot.sendMessage(chatId, '😔 No hay productos disponibles en este momento.');
                await this.sendMenu(chatId, userId);
                return;
            }

            let catalogText = '📦 *Catálogo de Productos*\n\n';

            productos.forEach((producto, index) => {
                catalogText += `${index + 1}. *${producto.nombre}*\n`;
                catalogText += `   ${producto.descripcion}\n`;
                catalogText += `   💰 $${producto.precio.toLocaleString('es-CO')}\n`;
                catalogText += `   📦 Stock: ${producto.stock}\n\n`;
            });

            catalogText += '\n¿Qué producto deseas agregar al carrito? Escribe el número.';

            // Store products in state for reference
            StateManager.setData(userId, 'productos', productos);
            StateManager.setCurrentState(userId, 'BROWSING_PRODUCTS');

            await this.bot.sendMessage(chatId, catalogText, { parse_mode: 'Markdown' });
        } catch (error) {
            console.error('[ChatBot] Error fetching products:', error);
            await this.bot.sendMessage(chatId, '❌ Error al cargar productos. Por favor intenta más tarde.');
            await this.sendMenu(chatId, userId);
        }
    }

    async handleProductSelection(chatId, userId, text) {
        const productos = StateManager.getData(userId, 'productos');
        const selection = parseInt(text);

        if (isNaN(selection) || selection < 1 || selection > productos.length) {
            await this.bot.sendMessage(chatId, '❌ Selección inválida. Por favor escribe el número del producto.');
            return;
        }

        const producto = productos[selection - 1];
        StateManager.setData(userId, 'selectedProduct', producto);
        StateManager.setCurrentState(userId, 'SELECTING_QUANTITY');

        await this.bot.sendMessage(
            chatId,
            `Seleccionaste: *${producto.nombre}*\n\n¿Cuántas unidades deseas? (Máximo: ${producto.stock})`,
            { parse_mode: 'Markdown' }
        );
    }

    async handleQuantitySelection(chatId, userId, text) {
        const cantidad = parseInt(text);
        const producto = StateManager.getData(userId, 'selectedProduct');

        if (isNaN(cantidad) || cantidad < 1) {
            await this.bot.sendMessage(chatId, '❌ Por favor ingresa una cantidad válida (número mayor a 0).');
            return;
        }

        if (cantidad > producto.stock) {
            await this.bot.sendMessage(chatId, `❌ Solo hay ${producto.stock} unidades disponibles.`);
            return;
        }

        // Add to cart
        StateManager.addToCart(userId, {
            productoId: producto.id,
            nombre: producto.nombre,
            precio: producto.precio,
            cantidad: cantidad
        });

        await this.bot.sendMessage(
            chatId,
            `✅ Agregado: ${cantidad}x ${producto.nombre}\n\n¿Deseas agregar más productos?\n\n1️⃣ Sí, seguir comprando\n2️⃣ No, ir al carrito`,
            { parse_mode: 'Markdown' }
        );

        StateManager.setCurrentState(userId, 'CONTINUE_SHOPPING');
    }

    async handleContinueShopping(chatId, userId, text) {
        if (text === '1') {
            await this.showProducts(chatId, userId);
        } else if (text === '2') {
            await this.showCart(chatId, userId);
        } else {
            await this.bot.sendMessage(chatId, '❌ Por favor selecciona 1 o 2.');
        }
    }

    async showCart(chatId, userId) {
        const cart = StateManager.getCart(userId);

        if (cart.length === 0) {
            await this.bot.sendMessage(chatId, '🛒 Tu carrito está vacío.');
            await this.sendMenu(chatId, userId);
            return;
        }

        let cartText = '🛒 *Tu Carrito*\n\n';
        let total = 0;

        cart.forEach((item, index) => {
            const subtotal = item.precio * item.cantidad;
            total += subtotal;
            cartText += `${index + 1}. ${item.nombre}\n`;
            cartText += `   ${item.cantidad}x $${item.precio.toLocaleString('es-CO')} = $${subtotal.toLocaleString('es-CO')}\n\n`;
        });

        cartText += `💰 *Total: $${total.toLocaleString('es-CO')}*\n\n`;
        cartText += '1️⃣ Finalizar compra\n2️⃣ Volver al menú';

        StateManager.setData(userId, 'total', total);
        await this.bot.sendMessage(chatId, cartText, { parse_mode: 'Markdown' });
    }

    async startCheckout(chatId, userId) {
        const cart = StateManager.getCart(userId);

        if (cart.length === 0) {
            await this.bot.sendMessage(chatId, '❌ Tu carrito está vacío. Agrega productos primero.');
            await this.sendMenu(chatId, userId);
            return;
        }

        await this.bot.sendMessage(
            chatId,
            '📍 Por favor ingresa tu dirección de entrega:'
        );

        StateManager.setCurrentState(userId, 'ENTERING_ADDRESS');
    }

    async handleAddressEntry(chatId, userId, text) {
        if (text.length < 10) {
            await this.bot.sendMessage(chatId, '❌ Por favor ingresa una dirección válida (mínimo 10 caracteres).');
            return;
        }

        StateManager.setData(userId, 'direccion', text);

        const cart = StateManager.getCart(userId);
        const total = StateManager.getData(userId, 'total');

        let confirmText = '✅ *Confirmar Pedido*\n\n';
        confirmText += `📍 Dirección: ${text}\n\n`;
        confirmText += '*Productos:*\n';

        cart.forEach(item => {
            confirmText += `• ${item.cantidad}x ${item.nombre}\n`;
        });

        confirmText += `\n💰 Total: $${total.toLocaleString('es-CO')}\n\n`;
        confirmText += '¿Confirmar pedido?\n\n1️⃣ Sí, confirmar\n2️⃣ No, cancelar';

        await this.bot.sendMessage(chatId, confirmText, { parse_mode: 'Markdown' });
        StateManager.setCurrentState(userId, 'CONFIRMING_ORDER');
    }

    async handleOrderConfirmation(chatId, userId, text) {
        if (text === '2') {
            await this.bot.sendMessage(chatId, '❌ Pedido cancelado.');
            StateManager.clearCart(userId);
            await this.sendMenu(chatId, userId);
            return;
        }

        if (text !== '1') {
            await this.bot.sendMessage(chatId, '❌ Por favor selecciona 1 para confirmar o 2 para cancelar.');
            return;
        }

        // Create order here - this will be implemented when we integrate with backend
        await this.bot.sendMessage(
            chatId,
            '⏳ Procesando tu pedido...'
        );

        // TODO: Call backend to create order
        // For now, just clear cart and show success

        await this.bot.sendMessage(
            chatId,
            '✅ ¡Pedido creado exitosamente!\n\nPronto recibirás el enlace de pago.'
        );

        StateManager.clearCart(userId);
        StateManager.setCurrentState(userId, 'MENU');
    }

    async showHelp(chatId) {
        const helpText = `
ℹ️ *Ayuda*

Este bot te permite:
• Ver el catálogo de productos
• Agregar productos al carrito
• Finalizar tu compra
• Pagar de forma segura

Para empezar, usa /start

Si tienes problemas, contacta a soporte.
        `;

        await this.bot.sendMessage(chatId, helpText, { parse_mode: 'Markdown' });
    }
}

export default ChatbotEngine;
