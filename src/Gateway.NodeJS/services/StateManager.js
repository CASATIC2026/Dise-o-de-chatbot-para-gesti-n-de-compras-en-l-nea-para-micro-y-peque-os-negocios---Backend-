/**
 * StateManager - Manages conversation state for each user
 * Implements in-memory state storage with automatic cleanup
 */

class StateManager {
    constructor() {
        this.states = new Map();
        this.CLEANUP_INTERVAL = 5 * 60 * 1000; // 5 minutes
        this.SESSION_TIMEOUT = 20 * 60 * 1000; // 20 minutes
        
        // Start cleanup job
        this.startCleanupJob();
    }

    /**
     * Get user state or create new one
     * @param {string|number} userId - Telegram or WhatsApp user ID
     * @returns {Object} User state object
     */
    getState(userId) {
        if (!this.states.has(userId)) {
            this.states.set(userId, {
                userId,
                currentState: 'MENU',
                cart: [],
                lastActivity: Date.now(),
                data: {}
            });
        } else {
            // Update last activity
            const state = this.states.get(userId);
            state.lastActivity = Date.now();
        }
        
        return this.states.get(userId);
    }

    /**
     * Update user state
     */
    setState(userId, newState) {
        const currentState = this.getState(userId);
        this.states.set(userId, {
            ...currentState,
            ...newState,
            lastActivity: Date.now()
        });
    }

    /**
     * Update current conversation state
     */
    setCurrentState(userId, stateName) {
        const state = this.getState(userId);
        state.currentState = stateName;
        state.lastActivity = Date.now();
    }

    /**
     * Add product to cart
     */
    addToCart(userId, product) {
        const state = this.getState(userId);
        
        // Check if product already in cart
        const existingIndex = state.cart.findIndex(item => item.productoId === product.productoId);
        
        if (existingIndex >= 0) {
            state.cart[existingIndex].cantidad += product.cantidad;
        } else {
            state.cart.push(product);
        }
        
        state.lastActivity = Date.now();
    }

    /**
     * Get user cart
     */
    getCart(userId) {
        const state = this.getState(userId);
        return state.cart || [];
    }

    /**
     * Clear user cart
     */
    clearCart(userId) {
        const state = this.getState(userId);
        state.cart = [];
        state.lastActivity = Date.now();
    }

    /**
     * Store temporary data
     */
    setData(userId, key, value) {
        const state = this.getState(userId);
        state.data[key] = value;
        state.lastActivity = Date.now();
    }

    /**
     * Get temporary data
     */
    getData(userId, key) {
        const state = this.getState(userId);
        return state.data[key];
    }

    /**
     * Clear user state completely
     */
    clearState(userId) {
        this.states.delete(userId);
    }

    /**
     * Start automatic cleanup job for stale sessions
     */
    startCleanupJob() {
        setInterval(() => {
            const now = Date.now();
            let cleaned = 0;
            
            for (const [userId, state] of this.states.entries()) {
                if (now - state.lastActivity > this.SESSION_TIMEOUT) {
                    this.states.delete(userId);
                    cleaned++;
                }
            }
            
            if (cleaned > 0) {
                console.log(`[StateManager] Cleaned ${cleaned} stale sessions`);
            }
        }, this.CLEANUP_INTERVAL);
    }

    /**
     * Get statistics
     */
    getStats() {
        return {
            activeSessions: this.states.size,
            sessions: Array.from(this.states.entries()).map(([userId, state]) => ({
                userId,
                currentState: state.currentState,
                cartItems: state.cart.length,
                lastActivity: new Date(state.lastActivity).toISOString()
            }))
        };
    }
}

export default new StateManager();
