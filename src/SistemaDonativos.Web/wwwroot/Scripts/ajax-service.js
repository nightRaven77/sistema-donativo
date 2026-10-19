/**
 * Servicio centralizado para llamadas AJAX
 * Maneja todas las peticiones HTTP al backend
 * Implementa el patrón Service Layer para separar lógica de comunicación
 */
const AjaxService = (function() {
    'use strict';

    /**
     * Detecta el ambiente y retorna la ruta base apropiada
     * 
     * Ambientes soportados:
     * 1. Debug local: localhost:puerto → /
     * 2. Publicación local: localhost → /
     * 3. Staging: 172.16.x.x → /
     * 4. Producción: dominio → /donaciones/
     * 
     * @returns {string} Ruta base para las URLs
     */
    function _getBasePath() {
        const pathname = window.location.pathname || '/';
        if (pathname.toLowerCase().startsWith('/donaciones/') || pathname.toLowerCase() === '/donaciones') {
            return '/donaciones/';
        }
        return '/';
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
         * Trae todos los donadores
         * @returns {Promise} Lista de donadores completa
         */
        obtenerTodos() {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerTodosDonadores'
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
                url: _buildUrl('Registros/GuardarArchivos'),
                data: formData,
                processData: false,
                contentType: false,
                timeout: 30000 // 30 segundos para uploads de archivos
            });
        }
    };

    // ============ MÓDULO: SALIDAS ============

    const salidas = {
        /**
         * Obtiene todas las salidas
         * @returns {Promise} Lista de salidas
         */
        obtenerTodas() {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerSalidas'
            });
        },

        /**
         * Obtiene una salida específica por ID
         * @param {number} idSalida - ID de la salida
         * @returns {Promise} Datos de la salida
         */
        obtenerPorId(idSalida) {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/obtenerSalida',
                data: { id_salida: idSalida }
            });
        },

        /**
         * Inserta una nueva salida
         * @param {Object} datos - Datos de la salida
         * @returns {Promise} Respuesta con ID de la salida creada
         */
        insertar(datos) {
            return _ejecutarAjax({
                type: 'POST',
                url: '/Registros/insertaSalida',
                data: { datos: JSON.stringify(datos) }
            });
        },

        /**
         * Elimina una salida
         * @param {number} idSalida - ID de la salida a eliminar
         * @returns {Promise} Confirmación de eliminación
         */
        eliminar(idSalida) {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/EliminarSalida',
                data: { id_salida: idSalida }
            });
        }
    };

    // ============ MÓDULO: BENEFICIARIOS ============

    const beneficiarios = {
        /**
         * Busca beneficiarios por término de búsqueda
         * @param {string} termino - Término a buscar
         * @returns {Promise} Lista de beneficiarios que coinciden
         */
        buscar(termino) {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerBeneficiarios',
                data: { search: termino }
            });
        },

        /**
         * Obtiene todos los beneficiarios
         * @returns {Promise} Lista de beneficiarios completa
         */
        obtenerTodos() {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerTodosBeneficiarios'
            });
        },

        /**
         * Inserta un nuevo beneficiario
         * @param {Object} datos - Datos del beneficiario
         * @returns {Promise} Respuesta con ID del beneficiario creado
         */
        insertar(datos) {
            return _ejecutarAjax({
                type: 'POST',
                url: '/Registros/insertaBeneficiario',
                data: { datos: JSON.stringify(datos) }
            });
        }
    };

    // ============ MÓDULO: INVENTARIO ============

    const inventario = {
        /**
         * Obtiene el inventario de un almacén específico
         * @param {number} idAlmacen - ID del almacén
         * @param {string} buscar - Término de búsqueda opcional
         * @returns {Promise} Lista de artículos en inventario
         */
        obtenerPorAlmacen(idAlmacen, buscar = '') {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerInventario',
                data: { id_almacen: idAlmacen, buscar: buscar }
            });
        }
    };

    // ============ MÓDULO: TRASPASOS ============

    const traspasos = {
        /**
         * Obtiene todos los traspasos
         * @returns {Promise} Lista de traspasos
         */
        obtenerTodos() {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/ObtenerTraspasos'
            });
        },

        /**
         * Obtiene un traspaso específico por ID
         * @param {number} idTraspaso - ID del traspaso
         * @returns {Promise} Datos del traspaso
         */
        obtenerPorId(idTraspaso) {
            return _ejecutarAjax({
                type: 'GET',
                url: '/Registros/obtenerTraspaso',
                data: { id_traspaso: idTraspaso }
            });
        },

        /**
         * Inserta un nuevo traspaso
         * @param {Object} datos - Datos del traspaso
         * @returns {Promise} Respuesta con ID del traspaso creado
         */
        insertar(datos) {
            return _ejecutarAjax({
                type: 'POST',
                url: '/Registros/insertaTraspaso',
                data: { datos: JSON.stringify(datos) }
            });
        },

        /**
         * Recibe un traspaso (cambia estado a aprobado)
         * @param {Object} datos - Datos de recepción
         * @returns {Promise} Confirmación de recepción
         */
        recibir(datos) {
            return _ejecutarAjax({
                type: 'POST',
                url: '/Registros/RecibirTraspaso',
                data: { datos: JSON.stringify(datos) }
            });
        },

        /**
         * Cancela un traspaso
         * @param {number} idTraspaso - ID del traspaso
         * @param {string} motivo - Motivo de cancelación
         * @returns {Promise} Confirmación de cancelación
         */
        cancelar(idTraspaso, motivo) {
            return _ejecutarAjax({
                type: 'POST',
                url: '/Registros/CancelarTraspaso',
                data: { 
                    id_traspaso: idTraspaso,
                    motivo_cancelacion: motivo
                }
            });
        }
    };

    // ============ MÓDULO: REPORTES ============

    const reportes = {
        /**
         * Genera el reporte de entradas en formato Excel
         * @param {Object} parametros - Parámetros del reporte: { fechaInicio, fechaFin, idAlmacen?, idCategoria? }
         * @returns {Promise} Promesa con blob del archivo Excel
         */
        generarEntradas(parametros) {
            return $.ajax({
                type: 'POST',
                url: _buildUrl('Reportes/GenerarReporteEntradas'),
                data: { datos: JSON.stringify(parametros) },
                xhrFields: {
                    responseType: 'blob'
                },
                timeout: 30000
            });
        },

        /**
         * Genera el reporte de salidas en formato Excel
         * @param {Object} parametros - Parámetros del reporte: { fechaInicio, fechaFin, idAlmacen?, tipoSalida? }
         * @returns {Promise} Promesa con blob del archivo Excel
         */
        generarSalidas(parametros) {
            return $.ajax({
                type: 'POST',
                url: _buildUrl('Reportes/GenerarReporteSalidas'),
                data: { datos: JSON.stringify(parametros) },
                xhrFields: {
                    responseType: 'blob'
                },
                timeout: 30000
            });
        },

        /**
         * Genera el reporte de traspasos en formato Excel
         * @param {Object} parametros - Parámetros del reporte: { fechaInicio, fechaFin, idAlmacenOrigen?, idAlmacenDestino?, estatus? }
         * @returns {Promise} Promesa con blob del archivo Excel
         */
        generarTraspasos(parametros) {
            return $.ajax({
                type: 'POST',
                url: _buildUrl('Reportes/GenerarReporteTraspasos'),
                data: { datos: JSON.stringify(parametros) },
                xhrFields: {
                    responseType: 'blob'
                },
                timeout: 30000
            });
        },

        /**
         * Genera el reporte de existencias en formato Excel
         * @param {Object} parametros - Parámetros del reporte: { idAlmacen?, idCategoria?, stockMinimo? }
         * @returns {Promise} Promesa con blob del archivo Excel
         */
        generarExistencias(parametros) {
            return $.ajax({
                type: 'POST',
                url: _buildUrl('Reportes/GenerarReporteExistencias'),
                data: { datos: JSON.stringify(parametros) },
                xhrFields: {
                    responseType: 'blob'
                },
                timeout: 30000
            });
        },

        /**
         * Genera el reporte Kardex de inventario en formato Excel
         * @param {Object} parametros - Parámetros del reporte: { fechaInicio, fechaFin, idArticulo, idAlmacen? }
         * @returns {Promise} Promesa con blob del archivo Excel
         */
        generarKardex(parametros) {
            return $.ajax({
                type: 'POST',
                url: _buildUrl('Reportes/GenerarReporteKardex'),
                data: { datos: JSON.stringify(parametros) },
                xhrFields: {
                    responseType: 'blob'
                },
                timeout: 30000
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
        salidas,
        beneficiarios,
        inventario,
        traspasos,
        reportes,

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
