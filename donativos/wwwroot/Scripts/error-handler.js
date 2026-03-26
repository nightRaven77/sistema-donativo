/**
 * ====================================================================
 * ERROR-HANDLER.JS - Manejador Centralizado de Errores AJAX
 * ====================================================================
 * Maneja errores HTTP de forma consistente en toda la aplicación
 * Especializado en conflictos 409 (recursos duplicados)
 * 
 * @version 1.0.0
 * @author Sistema de Donativos - DIF Hidalgo
 */

// ===================================================================
// FUNCIÓN PRINCIPAL DE MANEJO DE ERRORES
// ===================================================================

/**
 * Maneja errores AJAX de forma centralizada
 * @param {Object} xhr - Objeto XMLHttpRequest
 * @param {string} status - Estado de la petición ('timeout', 'error', 'abort', 'parsererror')
 * @param {string} error - Descripción del error
 * @param {Object} opciones - Configuración del manejador
 * @param {string} opciones.recurso - Tipo de recurso ('Beneficiario', 'Donador', 'Artículo', etc.)
 * @param {Function} opciones.onConflictConfirm - Callback cuando se confirma usar recurso existente
 * @param {Function} opciones.onConflictCancel - Callback cuando se cancela
 * @param {boolean} opciones.showGenericErrors - Si debe mostrar errores genéricos (default: true)
 * @returns {boolean} True si el error fue manejado como conflicto 409
 */
function manejarErrorAjax(xhr, status, error, opciones = {}) {
    const {
        recurso = 'Recurso',
        onConflictConfirm = null,
        onConflictCancel = null,
        showGenericErrors = true
    } = opciones;

    console.group('🔍 Manejador de Errores AJAX');
    console.log('Estado:', status);
    console.log('Error:', error);
    console.log('Status Code:', xhr.status);
    console.log('Response Text:', xhr.responseText);
    console.groupEnd();

    // ===================================================================
    // MANEJO DE CONFLICTO 409 (Recurso Duplicado)
    // ===================================================================
    if (xhr.status === 409) {
        try {
            const conflictData = JSON.parse(xhr.responseText);
            console.log('📦 Datos de conflicto:', conflictData);
            console.log('   - existingResource:', conflictData.existingResource);
            console.log('   - existingResources:', conflictData.existingResources);
            console.log('   - resourceType:', conflictData.resourceType);
            console.log('   - conflictingField:', conflictData.conflictingField);

            // Construir HTML del modal
            const resourceInfo = construirInfoRecursoExistente(conflictData);

            Swal.fire({
                icon: 'warning',
                title: '⚠️ Registro Duplicado',
                html: `
                    <div class="text-left">
                        <p class="mb-3"><strong>${conflictData.message || 'Ya existe un registro con estos datos'}</strong></p>
                        ${resourceInfo}
                        <hr class="my-3">
                        <p class="text-muted mb-2">
                            <i class="fas fa-question-circle"></i> 
                            ¿Deseas utilizar el registro existente en lugar de crear uno nuevo?
                        </p>
                    </div>
                `,
                showCancelButton: true,
                confirmButtonText: '<i class="fas fa-check"></i> Usar Existente',
                cancelButtonText: '<i class="fas fa-times"></i> Cancelar',
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                reverseButtons: true,
                customClass: {
                    popup: 'animated fadeInDown faster'
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    console.log('✅ Usuario confirmó usar recurso existente');
                    if (onConflictConfirm && typeof onConflictConfirm === 'function') {
                        // Pasar existingResource o existingResources según corresponda
                        // Verificar que existingResources tenga elementos antes de usarlo
                        const recursoExistente = (conflictData.existingResources && Array.isArray(conflictData.existingResources) && conflictData.existingResources.length > 0)
                            ? conflictData.existingResources
                            : conflictData.existingResource;

                        console.log('🔍 Recurso existente a pasar:', recursoExistente);
                        onConflictConfirm(recursoExistente, conflictData);
                    } else {
                        console.warn('⚠️ No se proporcionó callback onConflictConfirm');
                        notificaciones.info(`${recurso} existente seleccionado`, 'Información');
                    }
                } else if (result.isDismissed) {
                    console.log('❌ Usuario canceló la acción');
                    if (onConflictCancel && typeof onConflictCancel === 'function') {
                        onConflictCancel(conflictData);
                    }
                }
            });

            return true; // Indica que se manejó el conflicto
        } catch (e) {
            console.error('❌ Error al parsear respuesta de conflicto 409:', e);
            console.error('Response Text:', xhr.responseText);
            
            // Fallback: mostrar error genérico
            if (showGenericErrors) {
                notificaciones.error(
                    'Registro duplicado detectado pero no se pudo procesar la respuesta', 
                    'Error de Conflicto'
                );
            }
            return false;
        }
    }

    // ===================================================================
    // MANEJO DE OTROS ERRORES HTTP
    // ===================================================================
    if (!showGenericErrors) {
        return false;
    }

    // Timeout
    if (status === 'timeout') {
        console.error('⏱️ Timeout en petición AJAX');
        notificaciones.error(
            'La petición tardó demasiado tiempo. Verifica tu conexión a Internet.', 
            'Timeout'
        );
        return false;
    }

    // 404 Not Found
    if (xhr.status === 404) {
        console.error('🔍 Endpoint no encontrado (404)');
        notificaciones.error(
            'El servicio solicitado no fue encontrado. Contacta al administrador.', 
            'Error 404'
        );
        return false;
    }

    // 500 Internal Server Error
    if (xhr.status === 500) {
        console.error('💥 Error interno del servidor (500)');
        console.error('Response:', xhr.responseText);
        
        // Intentar extraer mensaje del servidor
        let mensajeServidor = 'Error en el servidor. Revisa los logs.';
        try {
            const errorData = JSON.parse(xhr.responseText);
            if (errorData.message || errorData.Message) {
                mensajeServidor = errorData.message || errorData.Message;
            }
        } catch (e) {
            // No se pudo parsear, usar mensaje genérico
        }

        notificaciones.error(mensajeServidor, 'Error del Servidor');
        return false;
    }

    // 400 Bad Request
    if (xhr.status === 400) {
        console.error('📝 Petición mal formada (400)');
        
        let mensajeValidacion = 'Datos de entrada inválidos. Verifica el formulario.';
        try {
            const errorData = JSON.parse(xhr.responseText);
            if (errorData.message || errorData.Message) {
                mensajeValidacion = errorData.message || errorData.Message;
            }
        } catch (e) {
            // No se pudo parsear
        }

        notificaciones.warning(mensajeValidacion, 'Validación');
        return false;
    }

    // 401 Unauthorized
    if (xhr.status === 401) {
        console.error('🔒 No autorizado (401)');
        notificaciones.error(
            'Tu sesión ha expirado. Por favor inicia sesión nuevamente.', 
            'No Autorizado'
        );
        
        // Opcional: Redirigir al login después de 2 segundos
        setTimeout(() => {
            window.location.href = '/Login';
        }, 2000);
        return false;
    }

    // 403 Forbidden
    if (xhr.status === 403) {
        console.error('🚫 Acceso prohibido (403)');
        notificaciones.error(
            'No tienes permisos para realizar esta acción.', 
            'Acceso Denegado'
        );
        return false;
    }

    // Error genérico
    console.error('❌ Error genérico en petición AJAX');
    notificaciones.error(
        `Error al guardar ${recurso}. Intenta de nuevo.`, 
        'Error'
    );
    return false;
}

// ===================================================================
// FUNCIONES AUXILIARES
// ===================================================================

/**
 * Construye HTML con información del recurso existente
 * @param {Object} conflictData - Datos del conflicto desde la API
 * @returns {string} HTML formateado
 */
function construirInfoRecursoExistente(conflictData) {
    const { resourceType, conflictingField, existingResource, existingResources } = conflictData;

    // Manejar múltiples recursos (caso de artículos duplicados)
    if (existingResources && Array.isArray(existingResources) && existingResources.length > 0) {
        let html = `
            <div class="alert alert-info mb-3">
                <h6 class="mb-2">
                    <i class="fas fa-info-circle"></i> 
                    <strong>${existingResources.length} ${resourceType || 'Registros'} Existentes</strong>
                </h6>
        `;

        existingResources.forEach((recurso, index) => {
            html += `
                <ul class="mb-2 text-left" style="padding-left: 20px;">
                    <li class="font-weight-bold">${resourceType || 'Recurso'} #${index + 1}:</li>
            `;

            for (const [key, value] of Object.entries(recurso)) {
                const label = formatearNombreCampo(key);
                const valorFormateado = value || '<em class="text-muted">No especificado</em>';
                html += `<li style="margin-left: 20px;"><strong>${label}:</strong> ${valorFormateado}</li>`;
            }

            html += `</ul>`;
        });

        html += `</div>`;

        if (conflictingField) {
            html += `
                <p class="text-muted small mb-0">
                    <i class="fas fa-exclamation-triangle text-warning"></i> 
                    Campo en conflicto: <strong>${formatearNombreCampo(conflictingField)}</strong>
                </p>
            `;
        }

        return html;
    }

    // Manejar recurso único
    if (!existingResource) {
        return '<p class="text-muted"><em>No se pudo obtener información del registro existente</em></p>';
    }

    let html = `
        <div class="alert alert-info mb-3">
            <h6 class="mb-2">
                <i class="fas fa-info-circle"></i> 
                <strong>${resourceType || 'Registro'} Existente</strong>
            </h6>
            <ul class="mb-0 text-left" style="padding-left: 20px;">
    `;

    // Mostrar campos del recurso existente
    for (const [key, value] of Object.entries(existingResource)) {
        const label = formatearNombreCampo(key);
        const valorFormateado = value || '<em class="text-muted">No especificado</em>';
        html += `<li><strong>${label}:</strong> ${valorFormateado}</li>`;
    }

    html += `
            </ul>
        </div>
    `;

    // Mostrar campo en conflicto
    if (conflictingField) {
        html += `
            <p class="text-muted small mb-0">
                <i class="fas fa-exclamation-triangle text-warning"></i> 
                Campo en conflicto: <strong>${formatearNombreCampo(conflictingField)}</strong>
            </p>
        `;
    }

    return html;
}

/**
 * Formatea nombres de campos de snake_case o camelCase a texto legible
 * @param {string} campo - Nombre del campo
 * @returns {string} Nombre formateado
 */
function formatearNombreCampo(campo) {
    const mapeo = {
        'id_beneficiario': 'ID',
        'idBeneficiario': 'ID',
        'id_donador': 'ID',
        'idDonador': 'ID',
        'id_articulo': 'ID',
        'idArticulo': 'ID',
        'id_contacto': 'ID',
        'idContacto': 'ID',
        'codigo': 'Código',
        'nombre': 'Nombre',
        'rfc': 'RFC',
        'telefono': 'Teléfono',
        'email': 'Email',
        'tipo': 'Tipo'
    };

    return mapeo[campo] || campo
        .replace(/_/g, ' ')
        .replace(/([A-Z])/g, ' $1')
        .replace(/^./, str => str.toUpperCase())
        .trim();
}

/**
 * Extrae el ID del recurso existente de forma genérica
 * @param {Object} existingResource - Recurso existente desde la API
 * @param {string} tipoRecurso - Tipo de recurso ('beneficiario', 'donador', 'articulo', etc.)
 * @returns {number|null} ID del recurso o null
 */
function extraerIdRecurso(existingResource, tipoRecurso = '') {
    if (!existingResource) return null;

    // Intentar con diferentes formatos de ID
    const posiblesIds = [
        existingResource[`id_${tipoRecurso}`],
        existingResource[`id${tipoRecurso.charAt(0).toUpperCase() + tipoRecurso.slice(1)}`],
        existingResource.id,
        existingResource.ID,
        existingResource.Id
    ];

    for (const id of posiblesIds) {
        if (id !== undefined && id !== null) {
            return parseInt(id);
        }
    }

    // Fallback: primer valor numérico encontrado
    for (const value of Object.values(existingResource)) {
        if (typeof value === 'number') {
            return value;
        }
        if (typeof value === 'string' && !isNaN(value)) {
            return parseInt(value);
        }
    }

    return null;
}

// ===================================================================
// EXPORTAR FUNCIONES GLOBALES
// ===================================================================

window.manejarErrorAjax = manejarErrorAjax;
window.extraerIdRecurso = extraerIdRecurso;
window.construirInfoRecursoExistente = construirInfoRecursoExistente;

// ===================================================================
// LOG DE CARGA
// ===================================================================
console.log('✓ error-handler.js cargado correctamente');
console.log('✓ Funciones disponibles: manejarErrorAjax(), extraerIdRecurso()');
