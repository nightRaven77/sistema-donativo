/**
 * ====================================================================
 * LOGGER.JS - Sistema de Logging Inteligente
 * ====================================================================
 * Sistema centralizado de logging que detecta el ambiente automáticamente
 * En producción, solo muestra errores. En desarrollo, muestra todo.
 * 
 * @author Sistema Donativos
 * @version 1.0.0
 */

// ===================================================================
// DETECCIÓN DE AMBIENTE
// ===================================================================

/**
 * Detecta si la aplicación está en modo desarrollo
 * @returns {boolean} True si está en desarrollo
 */
function isDevEnvironment() {
    // Métodos de detección:
    // 1. Hostname localhost
    if (window.location.hostname === 'localhost' || 
        window.location.hostname === '127.0.0.1' ||
        window.location.hostname.includes('local')) {
        return true;
    }
    
    // 2. Puerto de desarrollo (5000-5999, 3000-3999)
    const port = parseInt(window.location.port);
    if ((port >= 3000 && port <= 3999) || (port >= 5000 && port <= 5999)) {
        return true;
    }
    
    // 3. Variable global de configuración
    if (typeof window.APP_ENV !== 'undefined' && window.APP_ENV === 'development') {
        return true;
    }
    
    return false;
}

// ===================================================================
// MÓDULO LOGGER
// ===================================================================

/**
 * Logger centralizado con detección automática de ambiente
 */
const Logger = (function() {
    const isDev = isDevEnvironment();
    const APP_NAME = '🎁 Sistema Donativos';
    
    /**
     * Formatea un mensaje con timestamp y contexto
     * @param {string} level - Nivel de log (INFO, WARN, ERROR)
     * @param {string} message - Mensaje principal
     * @param {*} data - Datos adicionales (opcional)
     * @returns {string} Mensaje formateado
     */
    function formatMessage(level, message, data) {
        const timestamp = new Date().toISOString().split('T')[1].split('.')[0];
        let formatted = `[${timestamp}] [${level}] ${APP_NAME} → ${message}`;
        return formatted;
    }
    
    /**
     * Envía logs críticos al servidor (solo en producción)
     * @param {string} level - Nivel de error
     * @param {string} message - Mensaje de error
     * @param {*} error - Objeto de error
     */
    function sendToServer(level, message, error) {
        if (isDev) return; // No enviar en desarrollo
        
        try {
            // Aquí se puede implementar envío al servidor
            // Por ejemplo, usando fetch o AjaxService
            const errorData = {
                level: level,
                message: message,
                error: error ? error.toString() : null,
                stack: error ? error.stack : null,
                url: window.location.href,
                userAgent: navigator.userAgent,
                timestamp: new Date().toISOString()
            };
            
            // TODO: Implementar endpoint de logging en el servidor
            // fetch('/api/logs', {
            //     method: 'POST',
            //     headers: { 'Content-Type': 'application/json' },
            //     body: JSON.stringify(errorData)
            // });
            
        } catch (e) {
            // Falló el envío, no hacer nada para evitar loops
        }
    }
    
    // ===================================================================
    // API PÚBLICA
    // ===================================================================
    
    return {
        /**
         * Log informativo (solo en desarrollo)
         * @param {string} message - Mensaje a mostrar
         * @param {*} data - Datos adicionales (opcional)
         */
        log: function(message, data) {
            if (isDev) {
                const formatted = formatMessage('INFO', message, data);
                if (data !== undefined) {
                    console.log(formatted, data);
                } else {
                    console.log(formatted);
                }
            }
        },
        
        /**
         * Log de información (alias de log)
         * @param {string} message - Mensaje a mostrar
         * @param {*} data - Datos adicionales (opcional)
         */
        info: function(message, data) {
            this.log(message, data);
        },
        
        /**
         * Log de advertencia
         * @param {string} message - Mensaje de advertencia
         * @param {*} data - Datos adicionales (opcional)
         */
        warn: function(message, data) {
            const formatted = formatMessage('WARN', message, data);
            if (data !== undefined) {
                console.warn(formatted, data);
            } else {
                console.warn(formatted);
            }
        },
        
        /**
         * Log de error (siempre se muestra y se envía al servidor en producción)
         * @param {string} message - Mensaje de error
         * @param {Error|*} error - Objeto de error
         */
        error: function(message, error) {
            const formatted = formatMessage('ERROR', message, error);
            
            if (error !== undefined) {
                console.error(formatted, error);
            } else {
                console.error(formatted);
            }
            
            // Enviar al servidor en producción
            sendToServer('ERROR', message, error);
        },
        
        /**
         * Log de depuración (solo en desarrollo)
         * @param {string} message - Mensaje de debug
         * @param {*} data - Datos adicionales (opcional)
         */
        debug: function(message, data) {
            if (isDev) {
                const formatted = formatMessage('DEBUG', message, data);
                if (data !== undefined) {
                    console.debug(formatted, data);
                } else {
                    console.debug(formatted);
                }
            }
        },
        
        /**
         * Agrupa múltiples logs (solo en desarrollo)
         * @param {string} groupName - Nombre del grupo
         * @param {Function} callback - Función con los logs a agrupar
         */
        group: function(groupName, callback) {
            if (isDev) {
                console.group(`${APP_NAME} → ${groupName}`);
                callback();
                console.groupEnd();
            }
        },
        
        /**
         * Mide el tiempo de ejecución de una función
         * @param {string} label - Etiqueta del timer
         */
        time: function(label) {
            if (isDev) {
                console.time(`⏱️ ${label}`);
            }
        },
        
        /**
         * Finaliza la medición de tiempo
         * @param {string} label - Etiqueta del timer
         */
        timeEnd: function(label) {
            if (isDev) {
                console.timeEnd(`⏱️ ${label}`);
            }
        },
        
        /**
         * Muestra una tabla (solo en desarrollo)
         * @param {Array|Object} data - Datos a mostrar en tabla
         */
        table: function(data) {
            if (isDev) {
                console.table(data);
            }
        },
        
        /**
         * Verifica si está en modo desarrollo
         * @returns {boolean} True si está en desarrollo
         */
        isDevelopment: function() {
            return isDev;
        },
        
        /**
         * Información del ambiente
         * @returns {Object} Información del ambiente
         */
        getEnvironmentInfo: function() {
            return {
                isDevelopment: isDev,
                hostname: window.location.hostname,
                port: window.location.port,
                protocol: window.location.protocol,
                url: window.location.href
            };
        }
    };
})();

// ===================================================================
// EXPORTAR PARA USO GLOBAL
// ===================================================================

// Hacer Logger disponible globalmente
window.Logger = Logger;

// Mostrar información de inicio solo en desarrollo
if (Logger.isDevelopment()) {
    Logger.group('🚀 Sistema de Logging Inicializado', function() {
        Logger.info('Ambiente detectado: DESARROLLO');
        Logger.info('Logs completos habilitados');
        Logger.table(Logger.getEnvironmentInfo());
    });
} else {
    // En producción, solo mostrar que el sistema está activo
    console.log('🎁 Sistema Donativos - Logger activo (modo producción)');
}
