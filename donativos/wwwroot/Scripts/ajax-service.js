/**
 * Servicio centralizado para llamadas AJAX
 * Maneja todas las peticiones HTTP al backend
 * Implementa el patrón Service Layer para separar lógica de comunicación
 */
const AjaxService = (function() {
    'use strict';
    
    // Configuración base para todas las peticiones AJAX
    const BASE_CONFIG = {
        timeout: 10000, // Se puede referenciar desde CONFIG.AJAX.TIMEOUT si existe
        dataType: 'json',
        contentType: 'application/x-www-form-urlencoded; charset=utf-8'
    };
    
    /**
     * Ejecuta una petición AJAX genérica con configuración base
     * @private
     * @param {Object} config - Configuración específica de la petición
     * @returns {jqXHR} Promesa jQuery
     */
    function _ejecutarAjax(config) {
        return $.ajax({
            ...BASE_CONFIG,
            ...config
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
                url: 'ObtenerDonativos'
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
                url: 'obtenerDonativo',
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
                url: 'obtenerDonativoDetalle',
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
                url: 'insertaDonativo',
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
                url: 'actualizaDonativo',
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
                url: 'EliminarDonativo',
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
                url: 'ObtenerDonadores',
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
                url: 'insertaDonador',
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
                url: 'ObtenerContactos',
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
                url: 'insertaContactoDonaodor',
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
                url: 'ObtenerArticulos',
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
                url: 'insertaArticulos',
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
                url: 'ObtenerAlmacenes'
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
                url: 'ObtenerCategorias'
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
        archivos
    };
})();
