/**
 * Servicio centralizado para llamadas AJAX
 * Maneja todas las peticiones HTTP al backend
 * Implementa el patrón Service Layer para separar lógica de comunicación
 */
const AjaxService = (function() {
    'use strict';

    /**
     * Detecta el ambiente y retorna la ruta base apropiada
     * - Producción: /donaciones/
     * - Desarrollo: /
     * @returns {string} Ruta base para las URLs
     */
    function _getBasePath() {
        const hostname = window.location.hostname;
        const host = window.location.host;
        // Detectar si es localhost o desarrollo
        const isDevelopment = 
                            hostname === '127.0.0.1' || 
                            hostname.startsWith('192.168.') ||
                            hostname.startsWith('172.16.');

        //hostname === 'localhost' ||
        // Retornar ruta base según el ambiente
        if (host.includes(':'))
        {
            return isDevelopment ? '/' : '/'
        }
        else { return isDevelopment ? '/' : '/donaciones/'; }
        
    }

    // Ruta base para todas las peticiones
    const BASE_PATH = _getBasePath();

    // Log para debugging (remover en producción si se desea)
    console.log(`🌐 Ambiente detectado: ${BASE_PATH === '/' ? 'Desarrollo' : 'Producción'}`);
    console.log(`📍 Ruta base: ${BASE_PATH}`);

    // Configuración base para todas las peticiones AJAX
    const BASE_CONFIG = {
        timeout: 10000, // Se puede referenciar desde CONFIG.AJAX.TIMEOUT si existe
        dataType: 'json',
        contentType: 'application/x-www-form-urlencoded; charset=utf-8'
    };

    /**
     * Construye la URL completa con la ruta base
     * @private
     * @param {string} relativeUrl - URL relativa del endpoint
     * @returns {string} URL completa con ruta base
     */
    function _buildUrl(relativeUrl) {
        // Si la URL ya empieza con /, quitarla para evitar doble barra
        const cleanUrl = relativeUrl.startsWith('/') ? relativeUrl.substring(1) : relativeUrl;
        return `${BASE_PATH}${cleanUrl}`;
    }

    /**
     * Ejecuta una petición AJAX genérica con configuración base
     * @private
     * @param {Object} config - Configuración específica de la petición
     * @returns {jqXHR} Promesa jQuery
     */
    function _ejecutarAjax(config) {
        // Construir la URL completa con ruta base
        const fullUrl = _buildUrl(config.url);

        return $.ajax({
            ...BASE_CONFIG,
            ...config,
            url: fullUrl
        });
    }
    
    // ============ MÓDULO: DONATIVOS ============
    
    const donativos = {
        /**
         * Obtiene todos los donativos del usuario actual
         * @returns {Promise} Lista de donativos
         */
        obtenerTodos() {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerDonativos'
            });
        },
        
        /**
         * Obtiene un donativo específico por ID
         * @param {number} idDonativo - ID del donativo
         * @returns {Promise} Datos del donativo
         */
        obtenerPorId(idDonativo) {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/obtenerDonativo',
                data: { id_donativo: idDonativo }
            });
        },
        
        /**
         * Obtiene los detalles completos de un donativo (incluyendo artículos)
         * @param {number} idDonativo - ID del donativo
         * @returns {Promise} Detalles del donativo
         */
        obtenerDetalles(idDonativo) {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/obtenerDonativoDetalle',
                data: { id_donativo: idDonativo }
            });
        },
        
        /**
         * Inserta un nuevo donativo
         * @param {Object} datos - Datos del donativo (incluye donador, detalles, archivos)
         * @returns {Promise} Respuesta con ID del donativo creado
         */
        insertar(datos) {
            return _ejecutarAjax({
                type: 'POST',
                url: '/Registros/insertaDonativo',
                data: { datos: JSON.stringify(datos) }
            });
        },
        
        /**
         * Actualiza un donativo existente
         * @param {number} idDonativo - ID del donativo
         * @param {Object} datos - Datos actualizados
         * @returns {Promise} Respuesta de actualización
         */
        actualizar(idDonativo, datos) {
            return _ejecutarAjax({
                type: 'POST',
                url: '/Registros/actualizaDonativo',
                data: { 
                    id_donativo: idDonativo,
                    datos: JSON.stringify(datos)
                }
            });
        },
        
        /**
         * Elimina un donativo
         * @param {number} idDonativo - ID del donativo a eliminar
         * @returns {Promise} Confirmación de eliminación
         */
        eliminar(idDonativo) {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/EliminarDonativo',
                data: { id_donativo: idDonativo }
            });
        }
    };
    
    // ============ MÓDULO: DONADORES ============
    
    const donadores = {
        /**
         * Busca donadores por término de búsqueda
         * @param {string} termino - Término a buscar (nombre, RFC, etc.)
         * @returns {Promise} Lista de donadores que coinciden
         */
        buscar(termino) {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerDonadores',
                data: { search: termino }
            });
        },
        
        /**
         * Inserta un nuevo donador
         * @param {Object} datos - Datos del donador
         * @returns {Promise} Respuesta con ID del donador creado
         */
        insertar(datos) {
            return _ejecutarAjax({
                type: 'POST',
                url: '/Registros/insertaDonador',
                data: { datos: JSON.stringify(datos) }
            });
        }
    };
    
    // ============ MÓDULO: CONTACTOS ============
    
    const contactos = {
        /**
         * Obtiene los contactos de un donador específico
         * @param {number} idDonador - ID del donador
         * @returns {Promise} Lista de contactos del donador
         */
        obtenerPorDonador(idDonador) {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerContactos',
                data: { id_donador: idDonador }
            });
        },
        
        /**
         * Inserta un nuevo contacto para un donador
         * @param {Object} datos - Datos del contacto (debe incluir id_donador)
         * @returns {Promise} Respuesta con ID del contacto creado
         */
        insertar(datos) {
            return _ejecutarAjax({
                type: 'POST',
                url: '/Registros/insertaContactoDonador',
                data: { datos: JSON.stringify(datos) }
            });
        }
    };
    
    // ============ MÓDULO: ARTÍCULOS ============
    
    const articulos = {
        /**
         * Busca artículos por término de búsqueda
         * @param {string} termino - Término a buscar (nombre del artículo)
         * @returns {Promise} Lista de artículos que coinciden
         */
        buscar(termino) {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerArticulos',
                data: { search: termino }
            });
        },
        
        /**
         * Inserta múltiples artículos nuevos
         * @param {Array} datos - Array de objetos con datos de artículos
         * @returns {Promise} Respuesta con IDs de artículos creados
         */
        insertar(datos) {
            return _ejecutarAjax({
                type: 'POST',
                url: '/Registros/insertaArticulos',
                data: { datos: JSON.stringify(datos) }
            });
        }
    };
    
    // ============ MÓDULO: ALMACENES ============
    
    const almacenes = {
        /**
         * Obtiene todos los almacenes disponibles
         * @returns {Promise} Lista de almacenes
         */
        obtenerTodos() {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerAlmacenes'
            });
        }
    };
    
    // ============ MÓDULO: CATEGORÍAS ============
    
    const categorias = {
        /**
         * Obtiene todas las categorías de artículos
         * @returns {Promise} Lista de categorías
         */
        obtenerTodas() {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerCategorias'
            });
        }
    };
    
    // ============ MÓDULO: ARCHIVOS ============
    
    const archivos = {
        /**
         * Sube archivos adjuntos al servidor
         * @param {FormData} formData - FormData con archivos y nombres originales
         * @returns {Promise} URLs de archivos guardados
         */
        subir(formData) {
            return $.ajax({
                type: 'POST',
                url: 'GuardarArchivos',
                data: formData,
                processData: false,
                contentType: false,
                timeout: 30000 // 30 segundos para uploads de archivos
            });
        }
    };
    
    // ============ API PÚBLICA ============

    return {
        donativos,
        donadores,
        contactos,
        articulos,
        almacenes,
        categorias,
        archivos,

        // ============ UTILIDADES PÚBLICAS ============

        /**
         * Construye una URL completa con la ruta base según el ambiente
         * Útil para enlaces directos, descargas, etc.
         * @public
         * @param {string} relativeUrl - URL relativa (ej: 'ObtenerArchivo?archivo=...')
         * @returns {string} URL completa con ruta base
         */
        buildUrl(relativeUrl) {
            return _buildUrl(relativeUrl);
        },

        /**
         * Obtiene la ruta base actual según el ambiente
         * @public
         * @returns {string} Ruta base ('/' o '/donaciones/')
         */
        getBasePath() {
            return BASE_PATH;
        },

        // Métodos de conveniencia para acceso directo
        get: function(url, data) {
            return _ejecutarAjax({
                type: 'GET',
                url: url,
                data: data
            });
        },
        post: function(url, data) {
            return _ejecutarAjax({
                type: 'POST',
                url: url,
                data: data
            });
        }
    };
})();

// Crear alias en minúscula para compatibilidad
const ajaxService = AjaxService;

// Log de inicialización
console.log('✓ AjaxService cargado correctamente');
console.log(`📍 Métodos disponibles:`, Object.keys(AjaxService));
