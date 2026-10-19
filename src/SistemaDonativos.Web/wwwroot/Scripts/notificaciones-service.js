/**
 * ====================================================================
 * NOTIFICACIONES-SERVICE.JS - Servicio Centralizado de Notificaciones
 * ====================================================================
 * Sistema unificado de notificaciones usando SweetAlert2
 * Reemplaza Toastr para consistencia visual en toda la aplicación
 */

const NotificacionService = (function() {
    'use strict';

    // Configuración por defecto para todas las notificaciones toast
    const DEFAULT_TOAST_CONFIG = {
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    };

    /**
     * Muestra una notificación de éxito
     * @param {string} mensaje - Mensaje principal a mostrar
     * @param {string} titulo - Título opcional (si no se proporciona, se usa el mensaje como título)
     * @param {number} timer - Tiempo en ms que se muestra (default: 3000)
     */
    function success(mensaje, titulo = '', timer = 3000) {
        Swal.fire({
            ...DEFAULT_TOAST_CONFIG,
            icon: 'success',
            title: titulo || mensaje,
            text: titulo ? mensaje : '',
            timer: timer,
            background: '#d4edda',
            iconColor: '#28a745'
        });
    }

    /**
     * Muestra una notificación de error
     * @param {string} mensaje - Mensaje de error a mostrar
     * @param {string} titulo - Título opcional
     * @param {number} timer - Tiempo en ms (default: 5000 para errores)
     */
    function error(mensaje, titulo = '', timer = 5000) {
        Swal.fire({
            ...DEFAULT_TOAST_CONFIG,
            icon: 'error',
            title: titulo || mensaje,
            text: titulo ? mensaje : '',
            timer: timer,
            background: '#f8d7da',
            iconColor: '#dc3545'
        });
    }

    /**
     * Muestra una notificación de advertencia
     * @param {string} mensaje - Mensaje de advertencia
     * @param {string} titulo - Título opcional
     * @param {number} timer - Tiempo en ms (default: 4000)
     */
    function warning(mensaje, titulo = '', timer = 4000) {
        Swal.fire({
            ...DEFAULT_TOAST_CONFIG,
            icon: 'warning',
            title: titulo || mensaje,
            text: titulo ? mensaje : '',
            timer: timer,
            background: '#fff3cd',
            iconColor: '#ffc107'
        });
    }

    /**
     * Muestra una notificación informativa
     * @param {string} mensaje - Mensaje informativo
     * @param {string} titulo - Título opcional
     * @param {number} timer - Tiempo en ms (default: 3000)
     */
    function info(mensaje, titulo = '', timer = 3000) {
        Swal.fire({
            ...DEFAULT_TOAST_CONFIG,
            icon: 'info',
            title: titulo || mensaje,
            text: titulo ? mensaje : '',
            timer: timer,
            background: '#d1ecf1',
            iconColor: '#17a2b8'
        });
    }

    /**
     * Muestra un modal de confirmación
     * @param {string} mensaje - Mensaje de confirmación
     * @param {string} titulo - Título del modal (default: "¿Estás seguro?")
     * @param {function} callback - Función a ejecutar si confirma
     * @param {object} opciones - Opciones adicionales
     */
    function confirmar(mensaje, titulo = '¿Estás seguro?', callback, opciones = {}) {
        Swal.fire({
            title: titulo,
            text: mensaje,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: opciones.confirmButtonText || 'Sí, continuar',
            cancelButtonText: opciones.cancelButtonText || 'Cancelar',
            reverseButtons: true,
            ...opciones
        }).then((result) => {
            if (result.isConfirmed && callback) {
                callback();
            }
        });
    }

    /**
     * Muestra un modal personalizado (no toast)
     * @param {object} config - Configuración completa de SweetAlert2
     */
    function modal(config) {
        return Swal.fire(config);
    }

    /**
     * Muestra una notificación con loading (spinner)
     * Útil para procesos que tardan
     */
    function loading(mensaje = 'Procesando...') {
        Swal.fire({
            title: mensaje,
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    }

    /**
     * Cierra cualquier notificación o modal activo
     */
    function cerrar() {
        Swal.close();
    }

    // ============ API PÚBLICA ============
    return {
        success,
        error,
        warning,
        info,
        confirmar,
        modal,
        loading,
        cerrar,
        
        // Alias compatibles con nombres comunes
        exito: success,
        fallo: error,
        advertencia: warning,
        informacion: info,
        pregunta: confirmar,
        cargando: loading
    };
})();

// Crear alias global para fácil acceso
window.notificaciones = NotificacionService;

// Crear alias global "toastr-like" para compatibilidad durante migración
window.notif = NotificacionService;

// Log de inicialización
console.log('✓ NotificacionService cargado correctamente');
console.log('✓ Uso: notificaciones.success("mensaje", "título")');
console.log('✓ Métodos disponibles:', Object.keys(NotificacionService));
