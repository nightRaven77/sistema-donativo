/**
 * ====================================================================
 * TRASPASOS.JS - Módulo de Gestión de Traspasos entre Almacenes
 * ====================================================================
 * Gestiona traspasos normales e instantáneos con control de estados
 * Estados: Pendiente → Aprobado | Cancelado
 */

// ===================================================================
// VARIABLES GLOBALES
// ===================================================================
let almacenesTraspasos = [];
let inventarioActualTraspaso = [];
let articulosTraspasoSeleccionados = [];
let timerBusquedaArticuloTraspaso = null;
let traspasoActual = null;
let archivosAdjuntosTraspaso = [];
let archivosExistentesTraspaso = [];
let archivosAdjuntosRecepcion = []; // Archivos para la recepción
let dataTableTraspasos = null; // DataTable para la tabla de traspasos

// ⭐ Cache de selectores jQuery para mejorar rendimiento
let SELECTORS_TRASPASOS = {};

// ===================================================================
// CONFIGURACIÓN
// ===================================================================
const CONFIG_TRASPASOS = {
    TIMEOUTS: {
        BUSQUEDA: 500,
        HIDE_LIST: 200,
        AUTO_HIDE_LIST: 3000  // ⭐ Agregado: tiempo para ocultar listas automáticamente
    },
    TIPOS: {
        NORMAL: 'normal',
        INSTANTANEO: 'instantaneo'
    },
    ESTADOS: {
        PENDIENTE: 'pendiente',
        APROBADO: 'aprobado',
        CANCELADO: 'cancelado'
    },
    FILES: {
        MAX_SIZE: 5 * 1024 * 1024, // 5MB
        ALLOWED_EXTENSIONS: ['jpg', 'jpeg', 'png', 'gif', 'pdf', 'doc', 'docx', 'xls', 'xlsx']
    }
};

// ===================================================================
// INICIALIZACIÓN
// ===================================================================

// NO usar $(document).ready() porque esto es una SPA
// La inicialización se hace cuando se carga la vista

/**
 * Inicializa el módulo de traspasos
 * Esta función se debe llamar cuando la vista de traspasos se carga
 */
function inicializarTraspasos() {
    console.log('✓ Inicializando módulo Traspasos');

    // ⭐ Verificar que la tabla existe en el DOM antes de continuar
    if ($('#tablaTraspasos').length === 0) {
        console.error('❌ Error: La tabla #tablaTraspasos no existe en el DOM');
        console.log('⏳ Reintentando en 200ms...');
        setTimeout(() => inicializarTraspasos(), 200);
        return;
    }

    cargarAlmacenesTraspasos();
    cargarTraspasos();
    configurarEventosBusquedaTraspaso();
    establecerFechaHoyTraspaso();
    configurarEventosArchivosTraspaso();
}

/**
 * Configura los eventos de archivos adjuntos para traspasos
 */
function configurarEventosArchivosTraspaso() {
    $('#fileAdjuntosTraspaso').on('change', function(e) {
        manejarArchivosAdjuntosTraspaso(e);
    });

    // Archivos de recepción
    $('#fileAdjuntosRecepcion').on('change', function(e) {
        manejarArchivosAdjuntosRecepcion(e);
    });
}

/**
 * Establece la fecha actual
 */
function establecerFechaHoyTraspaso() {
    const hoy = new Date().toISOString().split('T')[0];
    $('#fechaTraspaso').val(hoy);
}

/**
 * Configura eventos de búsqueda
 */
function configurarEventosBusquedaTraspaso() {
    // Búsqueda de artículos en inventario
    $('#buscarArticuloInventarioTraspaso').on('input', function() {
        clearTimeout(timerBusquedaArticuloTraspaso);
        const termino = $(this).val().trim();
        
        if (termino.length >= 2) {
            timerBusquedaArticuloTraspaso = setTimeout(() => {
                buscarEnInventarioTraspaso(termino);
            }, CONFIG_TRASPASOS.TIMEOUTS.BUSQUEDA);
        } else {
            $('#listaInventarioTraspaso').hide();
        }
    });

    // Ocultar lista al hacer clic fuera
    $(document).on('click', function(e) {
        if (!$(e.target).closest('#buscarArticuloInventarioTraspaso, #listaInventarioTraspaso').length) {
            $('#listaInventarioTraspaso').hide();
        }
    });
}

// ===================================================================
// CARGA DE CATÁLOGOS
// ===================================================================

/**
 * Carga almacenes
 */
function cargarAlmacenesTraspasos() {
    AjaxService.almacenes.obtenerTodos()
        .then(response => {
            console.log('✓ Almacenes obtenidos:', response);
            const data = parseApiResponse(response);
            almacenesTraspasos = Array.isArray(data) ? data : [];
            llenarSelectsAlmacenes();
        })
        .catch(error => {
            console.error('❌ Error al cargar almacenes:', error);
            mostrarNotificacion('Error al cargar almacenes', 'error');
        });
}

/**
 * Llena los selects de almacenes
 */
function llenarSelectsAlmacenes() {
    const $selectOrigen = $('#selectAlmacenOrigenTraspaso');
    const $selectDestino = $('#selectAlmacenDestinoTraspaso');
    
    $selectOrigen.empty().append('<option value="">-- Seleccione --</option>');
    $selectDestino.empty().append('<option value="">-- Seleccione --</option>');
    
    almacenesTraspasos.forEach(almacen => {
        const id = almacen.id_almacen || almacen.idAlmacen || almacen.id;
        const nombre = almacen.nombre || `Almacén ${id}`;
        const option = `<option value="${id}">${nombre}</option>`;
        $selectOrigen.append(option);
        $selectDestino.append(option);
    });
}

/**
 * Carga inventario del almacén seleccionado
 */
function cargarInventarioAlmacenTraspaso() {
    const idAlmacen = $('#selectAlmacenOrigenTraspaso').val();
    
    if (!idAlmacen) {
        $('#seccionArticulosTraspaso').hide();
        $('#alertSeleccionarAlmacenTraspaso').show();
        inventarioActualTraspaso = [];
        return;
    }

    console.log(`📦 Cargando inventario del almacén: ${idAlmacen}`);
    $('#alertSeleccionarAlmacenTraspaso').hide();
    $('#seccionArticulosTraspaso').show();

    $('#listaInventarioTraspaso').html('<div class="list-group-item text-center"><i class="fas fa-spinner fa-spin"></i> Cargando inventario...</div>').show();

    AjaxService.inventario.obtenerPorAlmacen(idAlmacen)
        .then(response => {
            console.log('✓ Inventario obtenido:', response);
            inventarioActualTraspaso = Array.isArray(response) ? response : [];
            
            if (inventarioActualTraspaso.length === 0) {
                $('#listaInventarioTraspaso').html('<div class="list-group-item text-warning"><i class="fas fa-exclamation-triangle"></i> No hay artículos con stock en este almacén</div>').show();
                setTimeout(() => $('#listaInventarioTraspaso').hide(), 3000);
            } else {
                $('#listaInventarioTraspaso').hide();
                mostrarNotificacion(`${inventarioActualTraspaso.length} artículos disponibles`, 'success');
            }
        })
        .catch(error => {
            console.error('❌ Error al cargar inventario:', error);
            $('#listaInventarioTraspaso').html('<div class="list-group-item text-danger"><i class="fas fa-times"></i> Error al cargar inventario</div>').show();
        });
}

// ===================================================================
// BÚSQUEDA DE ARTÍCULOS
// ===================================================================

/**
 * Busca artículos en el inventario
 */
function buscarEnInventarioTraspaso(termino) {
    const idAlmacen = $('#selectAlmacenOrigenTraspaso').val();
    
    if (!idAlmacen) {
        mostrarNotificacion('Selecciona un almacén primero', 'warning');
        return;
    }
    
    console.log(`🔍 Buscando en inventario: "${termino}"`);

    const $lista = $('#listaInventarioTraspaso');
    $lista.html('<div class="list-group-item text-center"><i class="fas fa-spinner fa-spin"></i> Buscando...</div>').show();

    AjaxService.inventario.obtenerPorAlmacen(idAlmacen, termino)
        .then(response => {
            console.log('✓ Resultados de búsqueda:', response);
            const articulos = Array.isArray(response) ? response : [];
            mostrarListaInventarioTraspaso(articulos);
        })
        .catch(error => {
            console.error('❌ Error al buscar en inventario:', error);
            $lista.html('<div class="list-group-item text-danger">Error al buscar</div>');
        });
}

/**
 * Muestra la lista de artículos del inventario
 */
function mostrarListaInventarioTraspaso(articulos) {
    const $lista = $('#listaInventarioTraspaso');
    $lista.empty();
    
    if (articulos.length === 0) {
        $lista.html('<div class="list-group-item text-warning">No se encontraron artículos</div>');
        setTimeout(() => $lista.hide(), 2000);
        return;
    }
    
    articulos.forEach(art => {
        const id = art.idArticulo || art.id_articulo || art.id;
        const nombre = art.nombre || 'Sin nombre';
        const stock = art.stockActual || art.stock_actual || 0;
        const unidad = art.unidadMedida || art.unidad_medida || 'pzas';
        const categoria = art.nombreCategoria || art.nombre_categoria || '';
        
        const yaAgregado = articulosTraspasoSeleccionados.some(a => a.id === id);
        
        if (yaAgregado) {
            $lista.append(`
                <div class="list-group-item disabled">
                    <div class="d-flex justify-content-between">
                        <div>
                            <strong>${nombre}</strong>
                            ${categoria ? `<span class="badge badge-secondary ml-2">${categoria}</span>` : ''}
                        </div>
                        <span class="text-muted"><i class="fas fa-check"></i> Ya agregado</span>
                    </div>
                </div>
            `);
        } else {
            $lista.append(`
                <button type="button" class="list-group-item list-group-item-action" onclick="agregarArticuloTraspaso(${id}, '${nombre.replace(/'/g, "\\'")}', ${stock}, '${unidad}')">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <strong>${nombre}</strong>
                            ${categoria ? `<span class="badge badge-secondary ml-2">${categoria}</span>` : ''}
                        </div>
                        <span class="badge badge-success">Stock: ${stock} ${unidad}</span>
                    </div>
                </button>
            `);
        }
    });
    
    $lista.show();
}

// ===================================================================
// GESTIÓN DE ARTÍCULOS SELECCIONADOS
// ===================================================================

/**
 * Agrega un artículo al traspaso
 */
function agregarArticuloTraspaso(id, nombre, stockDisponible, unidad) {
    console.log(`➕ Agregando artículo: ${nombre} (Stock: ${stockDisponible})`);
    
    if (articulosTraspasoSeleccionados.some(a => a.id === id)) {
        mostrarNotificacion('Este artículo ya fue agregado', 'warning');
        return;
    }
    
    articulosTraspasoSeleccionados.push({
        id,
        nombre,
        stockDisponible,
        unidad,
        cantidad: 1
    });
    
    renderizarArticulosTraspaso();
    $('#listaInventarioTraspaso').hide();
    $('#buscarArticuloInventarioTraspaso').val('');
    mostrarNotificacion(`${nombre} agregado`, 'success');
}

/**
 * Renderiza la lista de artículos seleccionados
 */
function renderizarArticulosTraspaso() {
    const $container = $('#articulosSeleccionadosTraspaso');
    
    if (articulosTraspasoSeleccionados.length === 0) {
        $container.html('<div class="alert alert-info"><i class="fas fa-info-circle"></i> No hay artículos seleccionados</div>');
        return;
    }
    
    let html = '<div class="table-responsive"><table class="table table-sm table-bordered">';
    html += '<thead class="thead-light"><tr>';
    html += '<th>Artículo</th><th style="width:150px">Cantidad</th><th style="width:100px">Stock</th><th style="width:80px">Acciones</th>';
    html += '</tr></thead><tbody>';
    
    articulosTraspasoSeleccionados.forEach((art, index) => {
        html += '<tr>';
        html += `<td><strong>${art.nombre}</strong></td>`;
        html += `<td>
            <input type="number" 
                   class="form-control form-control-sm" 
                   value="${art.cantidad}" 
                   min="1" 
                   max="${art.stockDisponible}"
                   onchange="actualizarCantidadArticuloTraspaso(${index}, this.value)">
        </td>`;
        html += `<td class="text-center"><span class="badge badge-info">${art.stockDisponible} ${art.unidad}</span></td>`;
        html += `<td class="text-center">
            <button type="button" class="btn btn-sm btn-danger" onclick="eliminarArticuloTraspaso(${index})">
                <i class="fas fa-trash"></i>
            </button>
        </td>`;
        html += '</tr>';
    });
    
    html += '</tbody></table></div>';
    $container.html(html);
}

/**
 * Actualiza la cantidad de un artículo
 */
function actualizarCantidadArticuloTraspaso(index, cantidad) {
    cantidad = parseInt(cantidad) || 1;
    const articulo = articulosTraspasoSeleccionados[index];
    
    if (cantidad > articulo.stockDisponible) {
        mostrarNotificacion(`Cantidad máxima: ${articulo.stockDisponible}`, 'warning');
        cantidad = articulo.stockDisponible;
    }
    
    if (cantidad < 1) {
        cantidad = 1;
    }
    
    articulo.cantidad = cantidad;
    renderizarArticulosTraspaso();
}

/**
 * Elimina un artículo del traspaso
 */
function eliminarArticuloTraspaso(index) {
    const articulo = articulosTraspasoSeleccionados[index];
    console.log(`🗑️ Eliminando: ${articulo.nombre}`);
    
    articulosTraspasoSeleccionados.splice(index, 1);
    renderizarArticulosTraspaso();
    mostrarNotificacion('Artículo eliminado', 'info');
}

// ===================================================================
// MODAL DE NUEVO TRASPASO
// ===================================================================

/**
 * Abre el modal para crear un nuevo traspaso
 */
function abrirModalNuevoTraspaso() {
    console.log('📝 Abriendo modal Nuevo Traspaso');
    
    // Resetear formulario
    $('#formNuevoTraspaso')[0].reset();
    articulosTraspasoSeleccionados = [];
    inventarioActualTraspaso = [];
    
    $('#seccionArticulosTraspaso').hide();
    $('#alertSeleccionarAlmacenTraspaso').show();
    $('#articulosSeleccionadosTraspaso').empty();
    
    establecerFechaHoyTraspaso();
    
    // Mostrar modal
    $('#modalNuevoTraspaso').modal('show');
}

// ===================================================================
// GUARDAR TRASPASO
// ===================================================================

/**
 * Guarda el traspaso en el sistema
 */
function guardarTraspaso() {
    console.log('💾 Guardando traspaso...');

    // Validar formulario
    const form = $('#formNuevoTraspaso')[0];
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        mostrarNotificacion('Completa todos los campos requeridos', 'warning');
        return;
    }

    const almacenOrigen = parseInt($('#selectAlmacenOrigenTraspaso').val());
    const almacenDestino = parseInt($('#selectAlmacenDestinoTraspaso').val());

    // Validar que los almacenes sean diferentes
    if (almacenOrigen === almacenDestino) {
        mostrarNotificacion('Los almacenes origen y destino deben ser diferentes', 'warning');
        return;
    }

    // Validar artículos
    if (articulosTraspasoSeleccionados.length === 0) {
        mostrarNotificacion('Agrega al menos un artículo', 'warning');
        return;
    }

    // Deshabilitar botón
    const $btn = $('button[onclick="guardarTraspaso()"]');
    $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Guardando...');

    // Si hay archivos, subirlos primero
    if (archivosAdjuntosTraspaso.length > 0) {
        subirArchivosTraspaso()
            .then(archivosGuardados => {
                guardarTraspasoCompleto(almacenOrigen, almacenDestino, archivosGuardados, $btn);
            })
            .catch(error => {
                console.error('❌ Error al subir archivos:', error);
                notificaciones.error('Error al subir los archivos', 'Error');
                $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Crear Traspaso');
            });
    } else {
        // Sin archivos, guardar directamente
        guardarTraspasoCompleto(almacenOrigen, almacenDestino, [], $btn);
    }
}

/**
 * Guarda el traspaso completo con archivos opcionales
 */
function guardarTraspasoCompleto(almacenOrigen, almacenDestino, archivosGuardados, $btn) {
    const tipoTraspaso = $('#tipoTraspaso').val();

    // Construir objeto de traspaso
    const traspaso = {
        fechaTraspaso: $('#fechaTraspaso').val(),
        almacenOrigen: almacenOrigen,
        almacenDestino: almacenDestino,
        motivo: $('#motivoTraspaso').val(),
        observaciones: $('#observacionesTraspaso').val() || '',
        isInstantaneo: tipoTraspaso === CONFIG_TRASPASOS.TIPOS.INSTANTANEO,
        detalles: articulosTraspasoSeleccionados.map(art => ({
            idArticulo: art.id,
            cantidad: art.cantidad
        }))
    };

    // Agregar archivos si los hay
    if (archivosGuardados && archivosGuardados.length > 0) {
        // Agregar tipo de archivo a cada archivo individual
        traspaso.archivos = archivosGuardados.map(archivo => ({
            ...archivo,
            tipoArchivo: 'Envio'
        }));
        console.log(`📎 Incluyendo ${archivosGuardados.length} archivo(s) de tipo Envio`);
    }

    console.log('📤 Datos a enviar:', traspaso);

    // Enviar al servidor
    AjaxService.traspasos.insertar(traspaso)
        .then(response => {
            console.log('✓ Respuesta del servidor:', response);

            if (response.exito || response.Exito) {
                notificaciones.success('Traspaso registrado correctamente', 'success');
                $('#modalNuevoTraspaso').modal('hide');
                cargarTraspasos();

                // Limpiar archivos
                archivosAdjuntosTraspaso = [];
                $('#fileAdjuntosTraspaso').val('');
                const existingAlert = document.getElementById('fileAdjuntosTraspaso').parentElement.querySelector('.alert-info');
                if (existingAlert && existingAlert.innerHTML.includes('Archivos seleccionados')) {
                    existingAlert.remove();
                }
            } else {
                const mensaje = response.mensaje || response.Mensaje || 'Error al registrar el traspaso';
                notificaciones.error(mensaje, 'error');
            }
        })
        .catch(error => {
            console.error('❌ Error al guardar traspaso:', error);
            notificaciones.error('Error al guardar el traspaso', 'error');
        })
        .always(() => {
            $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Crear Traspaso');
        });
}

// ===================================================================
// CONSULTA DE TRASPASOS
// ===================================================================

/**
 * Inicializa DataTables para traspasos
 */
function inicializarDataTablesTraspasos() {
    // ⭐ Verificar que el elemento existe antes de inicializar
    if ($('#tablaTraspasos').length === 0) {
        console.error('❌ No se puede inicializar DataTables: #tablaTraspasos no existe en el DOM');
        return;
    }

    // ⭐ Destruir instancia anterior si existe
    if (dataTableTraspasos) {
        console.log('🧹 Destruyendo instancia anterior de DataTable Traspasos');
        dataTableTraspasos.destroy();
        dataTableTraspasos = null;
        // Limpiar el HTML de la tabla
        $('#tablaTraspasos').empty();
        $('#tablaTraspasos').html(`
            <thead class="table-light">
                <tr style="background-color: var(--guinda); color: white;">
                    <th>ID</th>
                    <th>Folio</th>
                    <th>Fecha</th>
                    <th>Almacén Origen</th>
                    <th>Almacén Destino</th>
                    <th>Artículos</th>
                    <th>Estado</th>
                    <th>Tipo</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody></tbody>
        `);
    }

    console.log('🔄 Inicializando DataTables Traspasos...');

    dataTableTraspasos = $('#tablaTraspasos').DataTable({
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
        },
        pageLength: 10,
        lengthChange: true,
        searching: true,
        ordering: true,
        info: true,
        paging: true,
        responsive: true,
        autoWidth: false, // Deshabilitar ancho automático
        order: [[2, 'desc']], // Ordenar por fecha descendente por defecto (cambió índice por nueva columna)
        columnDefs: [
            {
                targets: 0, // ID
                width: '50px',
                className: 'text-center'
            },
            {
                targets: 1, // Folio
                width: '100px',
                className: 'small-text'
            },
            {
                targets: 2, // Fecha
                width: '90px',
                className: 'text-center'
            },
            {
                targets: 3, // Almacén Origen
                width: '150px',
                render: function(data, type, row) {
                    if (type === 'display' && data.length > 25) {
                        return '<span title="' + data + '">' + data.substr(0, 25) + '...</span>';
                    }
                    return data;
                }
            },
            {
                targets: 4, // Almacén Destino
                width: '150px',
                render: function(data, type, row) {
                    if (type === 'display' && data.length > 25) {
                        return '<span title="' + data + '">' + data.substr(0, 25) + '...</span>';
                    }
                    return data;
                }
            },
            {
                targets: 5, // Artículos
                width: '70px',
                className: 'text-center',
                orderable: false
            },
            {
                targets: 6, // Estado
                width: '110px',
                orderable: true,
                searchable: true
            },
            {
                targets: 7, // Tipo
                width: '100px',
                orderable: true,
                searchable: true
            },
            {
                targets: -1, // Última columna (Acciones)
                width: '120px',
                orderable: false,
                searchable: false,
                className: 'text-center'
            }
        ],
        initComplete: function () {
            console.log('✓ DataTables Traspasos inicializado correctamente');
        }
    });
}

/**
 * Carga la tabla de traspasos
 */
function cargarTraspasos() {
    console.log('📋 Cargando traspasos...');

    AjaxService.traspasos.obtenerTodos()
        .then(response => {
            console.log('✓ Traspasos obtenidos:', response);
            const traspasos = Array.isArray(response) ? response : [];
            renderizarTablaTraspasos(traspasos);
        })
        .catch(error => {
            console.error('❌ Error al cargar traspasos:', error);
            mostrarNotificacion('Error al cargar los traspasos', 'error');
        });
}

/**
 * Renderiza la tabla de traspasos usando DataTables
 */
function renderizarTablaTraspasos(traspasos) {
    // Inicializar DataTables si no existe
    if (!dataTableTraspasos) {
        inicializarDataTablesTraspasos();
    }

    // Limpiar tabla
    dataTableTraspasos.clear();

    if (traspasos.length === 0) {
        dataTableTraspasos.draw();
        console.log('⚠ No hay traspasos para mostrar');
        return;
    }

    // Agregar filas usando DataTables
    traspasos.forEach(traspaso => {
        const id = traspaso.id_traspaso || traspaso.idTraspaso || traspaso.id;
        const folio = traspaso.folio || '-';
        const fecha = formatearFecha(traspaso.fecha_traspaso || traspaso.fechaTraspaso);
        const almacenOrigen = traspaso.almacenOrigenNombre || '-';
        const almacenDestino = traspaso.almacenDestinoNombre || '-';
        const numArticulos = traspaso.cantidadArticulos || 0;
        const estado = traspaso.estatus || 'pendiente';
        const isInstantaneo = traspaso.is_instantaneo || traspaso.isInstantaneo || false;

        const estadoBadge = obtenerBadgeEstado(estado);
        const tipoBadge = isInstantaneo 
            ? '<span class="badge badge-warning"><i class="fas fa-bolt"></i> Instantáneo</span>'
            : '<span class="badge badge-secondary">Normal</span>';

        const acciones = generarBotonesAccion(id, estado);

        // Agregar fila a DataTables
        dataTableTraspasos.row.add([
            id,
            folio,
            fecha,
            almacenOrigen,
            almacenDestino,
            `<span class="badge badge-info">${numArticulos}</span>`,
            estadoBadge,
            tipoBadge,
            acciones
        ]);
    });

    // Redibujar tabla
    dataTableTraspasos.draw();
    console.log(`✓ DataTables Traspasos actualizado con ${traspasos.length} registros`);

    // ⭐ Forzar actualización visual después de un breve delay
    setTimeout(() => {
        if (dataTableTraspasos) {
            dataTableTraspasos.columns.adjust().responsive.recalc();
            console.log('✓ Columnas ajustadas y responsive recalculado');
        }
    }, 100);
}

/**
 * Obtiene el badge según el estado
 */
function obtenerBadgeEstado(estado) {
    switch (estado.toLowerCase()) {
        case 'pendiente':
            return '<span class="badge badge-warning"><i class="fas fa-clock"></i> Pendiente</span>';
        case 'aprobado':
            return '<span class="badge badge-success"><i class="fas fa-check-circle"></i> Aprobado</span>';
        case 'cancelado':
            return '<span class="badge badge-danger"><i class="fas fa-times-circle"></i> Cancelado</span>';
        default:
            return `<span class="badge badge-secondary">${estado}</span>`;
    }
}

/**
 * Genera botones de acción según el estado
 */
function generarBotonesAccion(id, estado) {
    let botones = `
        <button class="btn btn-sm btn-info" onclick="verDetalleTraspaso(${id})" title="Ver detalle">
            <i class="fas fa-eye"></i>
        </button>
    `;
    
    if (estado.toLowerCase() === 'pendiente') {
        botones += `
            <button class="btn btn-sm btn-success ml-1" onclick="abrirModalRecibirTraspaso(${id})" title="Recibir">
                <i class="fas fa-check"></i>
            </button>
            <button class="btn btn-sm btn-danger ml-1" onclick="abrirModalCancelarTraspaso(${id})" title="Cancelar">
                <i class="fas fa-ban"></i>
            </button>
        `;
    }
    
    return botones;
}

// ===================================================================
// DETALLE DE TRASPASO
// ===================================================================

/**
 * Muestra el detalle de un traspaso
 */
function verDetalleTraspaso(id) {
    console.log(`👁️ Ver detalle de traspaso: ${id}`);
    
    const $modal = $('#modalDetalleTraspaso');
    const $contenido = $('#contenidoDetalleTraspaso');
    const $acciones = $('#accionesDetalleTraspaso');
    
    $contenido.html('<div class="text-center"><i class="fas fa-spinner fa-spin fa-2x"></i><p>Cargando...</p></div>');
    $acciones.empty();
    $modal.modal('show');

    AjaxService.traspasos.obtenerPorId(id)
        .then(response => {
            console.log('✓ Detalle obtenido:', response);
            const traspaso = parseApiResponse(response.data || response.Data || response);
            traspasoActual = traspaso;
            renderizarDetalleTraspaso(traspaso);
            agregarBotonesAccionModal(traspaso);
        })
        .catch(error => {
            console.error('❌ Error al obtener detalle:', error);
            $contenido.html('<div class="alert alert-danger">Error al cargar el detalle</div>');
        });
}

/**
 * Renderiza el detalle de un traspaso
 */
function renderizarDetalleTraspaso(traspaso) {
    const $contenido = $('#contenidoDetalleTraspaso');
    
    const id = traspaso.id_traspaso || traspaso.idTraspaso || traspaso.id;
    const fecha = formatearFecha(traspaso.fecha_traspaso || traspaso.fechaTraspaso);
    const almacenOrigen = traspaso.almacenOrigenNombre || '-';
    const almacenDestino = traspaso.almacenDestinoNombre || '-';
    const motivo = traspaso.motivo || '-';
    const observaciones = traspaso.observaciones || '-';
    const estado = traspaso.estatus || 'pendiente';
    const isInstantaneo = traspaso.is_instantaneo || traspaso.isInstantaneo || false;
    const estadoBadge = obtenerBadgeEstado(estado);
    const tipoBadge = isInstantaneo ? '<span class="badge badge-warning">Instantáneo</span>' : '<span class="badge badge-secondary">Normal</span>';
    
    let html = `
        <div class="row">
            <div class="col-md-6"><strong>ID:</strong> ${id}</div>
            <div class="col-md-6"><strong>Fecha:</strong> ${fecha}</div>
        </div>
        <div class="row mt-2">
            <div class="col-md-6"><strong>Almacén Origen:</strong> ${almacenOrigen}</div>
            <div class="col-md-6"><strong>Almacén Destino:</strong> ${almacenDestino}</div>
        </div>
        <div class="row mt-2">
            <div class="col-md-6"><strong>Estado:</strong> ${estadoBadge}</div>
            <div class="col-md-6"><strong>Tipo:</strong> ${tipoBadge}</div>
        </div>
        <div class="row mt-2">
            <div class="col-md-12"><strong>Motivo:</strong> ${motivo}</div>
        </div>
        <div class="row mt-2">
            <div class="col-md-12"><strong>Observaciones:</strong> ${observaciones}</div>
        </div>
    `;
    
    if (traspaso.motivo_cancelacion) {
        html += `
            <div class="row mt-2">
                <div class="col-md-12">
                    <div class="alert alert-danger">
                        <strong>Motivo de Cancelación:</strong> ${traspaso.motivo_cancelacion}
                    </div>
                </div>
            </div>
        `;
    }
    
    html += `
        <hr>
        <h6 class="font-weight-bold">Artículos:</h6>
        <table class="table table-sm table-bordered">
            <thead><tr><th>Artículo</th><th>Cantidad</th></tr></thead>
            <tbody>
    `;
    
    if (traspaso.detalles && traspaso.detalles.length > 0) {
        traspaso.detalles.forEach(d => {
            const nombre = d.nombreArticulo || '-';
            const cantidad = d.cantidad || 0;
            const unidad = d.unidad_medida || d.unidadMedida || '';
            html += `<tr><td>${nombre}</td><td>${cantidad} ${unidad}</td></tr>`;
        });
    } else {
        html += '<tr><td colspan="2" class="text-center text-muted">Sin artículos</td></tr>';
    }
    
    html += '</tbody></table>';

    // ⭐ Sección de archivos adjuntos (si los hay)
    if (traspaso.archivos && traspaso.archivos.length > 0) {
        html += `
            <hr class="my-4">
            <h6 class="font-weight-bold"><i class="fas fa-paperclip"></i> Archivos Adjuntos (${traspaso.archivos.length})</h6>
            <div class="row">`;

        traspaso.archivos.forEach((archivo) => {
            const extension = (archivo.nombreArchivo || '').split('.').pop().toLowerCase();
            const iconoArchivo = obtenerIconoArchivoTraspaso(extension);
            // ⭐ Usar AjaxService.buildUrl() para construir la URL correcta según el ambiente
            const urlCompleta = AjaxService.buildUrl(`/Registros/ObtenerArchivo?archivo=${encodeURIComponent(archivo.urlArchivo || archivo.url || '')}`);
            const tipoArchivo = archivo.tipoArchivo || 'General';

            // Badge según el tipo de archivo
            let badgeTipo = '';
            if (tipoArchivo === 'Recepcion') {
                badgeTipo = '<span class="badge badge-success">Recepción</span>';
            } else if (tipoArchivo === 'Envio') {
                badgeTipo = '<span class="badge badge-primary">Envío</span>';
            } else {
                badgeTipo = '<span class="badge badge-secondary">General</span>';
            }

            html += `
                <div class="col-md-6 col-lg-4 mb-3">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body text-center">
                            <i class="${iconoArchivo} fa-3x mb-3" style="color: var(--dorado);"></i>
                            <h6 class="card-title text-truncate" title="${archivo.nombreArchivo || 'Archivo'}">
                                ${archivo.nombreArchivo || 'Archivo'}
                            </h6>
                            <small class="text-muted">${badgeTipo}</small>
                            <div class="mt-3">
                                ${extension === 'jpg' || extension === 'jpeg' || extension === 'png' || extension === 'gif'
                    ? `<button type="button" class="btn btn-sm btn-info mb-2" onclick="visualizarArchivoTraspaso('${urlCompleta}', '${archivo.nombreArchivo}', 'imagen')">
                                        <i class="fas fa-eye"></i> Ver
                                       </button>`
                    : extension === 'pdf'
                        ? `<button type="button" class="btn btn-sm btn-info mb-2" onclick="visualizarArchivoTraspaso('${urlCompleta}', '${archivo.nombreArchivo}', 'pdf')">
                                            <i class="fas fa-eye"></i> Ver
                                           </button>`
                        : ''}
                                <a href="${urlCompleta}" download="${archivo.nombreArchivo}" class="btn btn-sm btn-success mb-2">
                                    <i class="fas fa-download"></i> Descargar
                                </a>
                            </div>
                        </div>
                    </div>
                </div>`;
        });

        html += `
            </div>`;
    }

    $contenido.html(html);
}

/**
 * Obtiene el icono de Font Awesome según la extensión del archivo
 */
function obtenerIconoArchivoTraspaso(extension) {
    switch(extension.toLowerCase()) {
        case 'pdf': return 'fas fa-file-pdf text-danger';
        case 'jpg':
        case 'jpeg':
        case 'png':
        case 'gif': return 'fas fa-file-image text-primary';
        case 'doc':
        case 'docx': return 'fas fa-file-word text-primary';
        case 'xls':
        case 'xlsx': return 'fas fa-file-excel text-success';
        default: return 'fas fa-file text-secondary';
    }
}

/**
 * Visualiza un archivo en un modal (imágenes o PDFs)
 */
function visualizarArchivoTraspaso(url, nombreArchivo, tipo) {
    let contenidoModal = '';

    if (tipo === 'imagen') {
        contenidoModal = `<img src="${url}" alt="${nombreArchivo}" style="max-width: 100%; height: auto;">`;
    } else if (tipo === 'pdf') {
        contenidoModal = `<iframe src="${url}" style="width: 100%; height: 70vh;" frameborder="0"></iframe>`;
    }

    Swal.fire({
        title: nombreArchivo,
        html: contenidoModal,
        width: '80%',
        showCloseButton: true,
        showConfirmButton: false,
        customClass: {
            popup: 'swal-wide'
        }
    });
}

/**
 * Agrega botones de acción al modal
 */
function agregarBotonesAccionModal(traspaso) {
    const $acciones = $('#accionesDetalleTraspaso');
    const estado = traspaso.estatus || 'pendiente';
    const id = traspaso.id_traspaso || traspaso.idTraspaso || traspaso.id;
    
    let botones = '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>';
    
    if (estado.toLowerCase() === 'pendiente') {
        botones += `
            <button type="button" class="btn btn-success" onclick="abrirModalRecibirTraspaso(${id})">
                <i class="fas fa-check"></i> Recibir
            </button>
            <button type="button" class="btn btn-danger" onclick="abrirModalCancelarTraspaso(${id})">
                <i class="fas fa-ban"></i> Cancelar
            </button>
        `;
    }
    
    $acciones.html(botones);
}

// ===================================================================
// RECIBIR TRASPASO
// ===================================================================

/**
 * Abre el modal para recibir un traspaso
 */
function abrirModalRecibirTraspaso(id) {
    console.log(`📥 Abrir modal recibir traspaso: ${id}`);
    $('#modalDetalleTraspaso').modal('hide');
    $('#idTraspasoRecibir').val(id);
    $('#observacionesRecepcion').val('');

    // Limpiar archivos de recepción
    archivosAdjuntosRecepcion = [];
    $('#fileAdjuntosRecepcion').val('');
    const existingAlert = document.getElementById('fileAdjuntosRecepcion')?.parentElement?.querySelector('.alert-info');
    if (existingAlert && existingAlert.innerHTML.includes('Archivos seleccionados')) {
        existingAlert.remove();
    }

    $('#modalRecibirTraspaso').modal('show');
}

/**
 * Confirma la recepción del traspaso
 */
function confirmarRecepcionTraspaso() {
    const id = $('#idTraspasoRecibir').val();
    const observaciones = $('#observacionesRecepcion').val() || '';

    console.log(`✅ Recibiendo traspaso ${id}`);

    const $btn = $('button[onclick="confirmarRecepcionTraspaso()"]');
    $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Procesando...');

    // Si hay archivos, subirlos primero
    if (archivosAdjuntosRecepcion.length > 0) {
        subirArchivosRecepcion()
            .then(archivosGuardados => {
                confirmarRecepcionConArchivos(id, observaciones, archivosGuardados, $btn);
            })
            .catch(error => {
                console.error('❌ Error al subir archivos:', error);
                mostrarNotificacion('Error al subir los archivos de recepción', 'error');
                $btn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirmar Recepción');
            });
    } else {
        // Sin archivos, confirmar directamente
        confirmarRecepcionConArchivos(id, observaciones, [], $btn);
    }
}

/**
 * Confirma la recepción con archivos opcionales
 */
function confirmarRecepcionConArchivos(id, observaciones, archivosGuardados, $btn) {
    // Construir objeto de recepción completo
    const datosRecepcion = {
        idTraspaso: id,
        observaciones: observaciones
    };

    // Agregar archivos si los hay
    if (archivosGuardados && archivosGuardados.length > 0) {
        datosRecepcion.archivos = archivosGuardados.map(archivo => ({
            ...archivo,
            tipoArchivo: 'Recepcion'
        }));
        console.log(`📎 Incluyendo ${archivosGuardados.length} archivo(s) de tipo Recepcion`);
    }

    console.log('📤 Datos de recepción a enviar:', datosRecepcion);

    // Enviar al servidor
    AjaxService.traspasos.recibir(datosRecepcion)
        .then(response => {
            console.log('✓ Respuesta:', response);

            if (response.exito || response.Exito) {
                mostrarNotificacion('Traspaso recibido correctamente', 'success');
                $('#modalRecibirTraspaso').modal('hide');
                cargarTraspasos();

                // Limpiar archivos
                archivosAdjuntosRecepcion = [];
                $('#fileAdjuntosRecepcion').val('');
            } else {
                const mensaje = response.mensaje || response.Mensaje || 'Error al recibir el traspaso';
                mostrarNotificacion(mensaje, 'error');
            }
        })
        .catch(error => {
            console.error('❌ Error:', error);
            mostrarNotificacion('Error al procesar la recepción', 'error');
        })
        .always(() => {
            $btn.prop('disabled', false).html('<i class="fas fa-check"></i> Confirmar Recepción');
        });
}

// ===================================================================
// CANCELAR TRASPASO
// ===================================================================

/**
 * Abre el modal para cancelar un traspaso
 */
function abrirModalCancelarTraspaso(id) {
    console.log(`❌ Abrir modal cancelar traspaso: ${id}`);
    $('#modalDetalleTraspaso').modal('hide');
    $('#idTraspasoCancelar').val(id);
    $('#motivoCancelacion').val('');
    $('#modalCancelarTraspaso').modal('show');
}

/**
 * Confirma la cancelación del traspaso
 */
function confirmarCancelacionTraspaso() {
    const id = $('#idTraspasoCancelar').val();
    const motivo = $('#motivoCancelacion').val().trim();
    
    if (!motivo) {
        mostrarNotificacion('El motivo de cancelación es requerido', 'warning');
        return;
    }
    
    console.log(`🚫 Cancelando traspaso ${id}`);

    const $btn = $('button[onclick="confirmarCancelacionTraspaso()"]');
    $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Procesando...');

    AjaxService.traspasos.cancelar(id, motivo)
        .then(response => {
            console.log('✓ Respuesta:', response);

            if (response.exito || response.Exito) {
                mostrarNotificacion('Traspaso cancelado correctamente', 'success');
                $('#modalCancelarTraspaso').modal('hide');
                cargarTraspasos();
            } else {
                const mensaje = response.mensaje || response.Mensaje || 'Error al cancelar el traspaso';
                mostrarNotificacion(mensaje, 'error');
            }
        })
        .catch(error => {
            console.error('❌ Error:', error);
            mostrarNotificacion('Error al cancelar el traspaso', 'error');
        })
        .always(() => {
            $btn.prop('disabled', false).html('<i class="fas fa-ban"></i> Cancelar Traspaso');
        });
}

// ===================================================================
// FILTRADO
// ===================================================================

/**
 * Filtra los traspasos según los criterios seleccionados
 */
function filtrarTraspasos() {
    // Implementar filtrado si es necesario
    cargarTraspasos();
}

// ===================================================================
// GESTIÓN DE ARCHIVOS ADJUNTOS
// ===================================================================

/**
 * Maneja los archivos adjuntos seleccionados para traspasos
 * Valida tipo y tamaño de archivos
 */
function manejarArchivosAdjuntosTraspaso(e) {
    const files = e.target.files;

    if (!files || files.length === 0) {
        return;
    }

    archivosAdjuntosTraspaso = [];
    const archivosValidos = [];
    const errores = [];

    // Validar todos los archivos
    for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const validacion = validarArchivoTraspaso(file);

        if (validacion.valido) {
            archivosValidos.push({
                file: file,
                nombre: file.name,
                tamaño: (file.size / 1024).toFixed(2) + ' KB',
                tipo: file.type
            });
        } else {
            errores.push(validacion.error);
        }
    }

    // Mostrar errores agrupados
    if (errores.length > 0) {
        errores.forEach(error => notificaciones.warning(error, 'Validación'));
    }

    // Guardar archivos válidos
    archivosAdjuntosTraspaso = archivosValidos;

    if (archivosAdjuntosTraspaso.length > 0) {
        mostrarResumenArchivosTraspaso();
        notificaciones.success(`${archivosAdjuntosTraspaso.length} archivo(s) seleccionado(s) correctamente`, 'Éxito');
    } else {
        console.warn('⚠ No hay archivos válidos para procesar');
        notificaciones.error('No hay archivos válidos para adjuntar', 'Error');
    }
}

/**
 * Valida un archivo individual para traspasos
 * @param {File} file - Archivo a validar
 * @returns {Object} Resultado de validación
 */
function validarArchivoTraspaso(file) {
    const fileExtension = file.name.split('.').pop().toLowerCase();

    // Validar extensión
    if (!CONFIG_TRASPASOS.FILES.ALLOWED_EXTENSIONS.includes(fileExtension)) {
        return {
            valido: false,
            error: `"${file.name}" - Tipo no permitido: ${fileExtension}`
        };
    }

    // Validar tamaño
    if (file.size > CONFIG_TRASPASOS.FILES.MAX_SIZE) {
        const sizeMB = (file.size / (1024 * 1024)).toFixed(2);
        return {
            valido: false,
            error: `"${file.name}" excede 5MB (${sizeMB}MB)`
        };
    }

    return { valido: true };
}

/**
 * Muestra un resumen de los archivos adjuntos seleccionados para traspasos
 */
function mostrarResumenArchivosTraspaso() {
    if (archivosAdjuntosTraspaso.length === 0) {
        return;
    }

    let resumenHTML = '<div class="alert alert-info mt-3" role="alert">';
    resumenHTML += '<i class="fas fa-info-circle"></i> <strong>Archivos seleccionados:</strong><br>';
    resumenHTML += '<ul class="mb-0 mt-2">';

    archivosAdjuntosTraspaso.forEach((archivo, index) => {
        resumenHTML += `<li>${index + 1}. ${archivo.nombre} (${archivo.tamaño})</li>`;
    });

    resumenHTML += '</ul></div>';

    // Insertar después del input de archivo
    const fileInput = document.getElementById('fileAdjuntosTraspaso');
    const existingAlert = fileInput.parentElement.querySelector('.alert-info');

    if (existingAlert && existingAlert.innerHTML.includes('Archivos seleccionados')) {
        existingAlert.remove();
    }

    fileInput.insertAdjacentHTML('afterend', resumenHTML);
}

/**
 * Sube los archivos adjuntos al servidor antes de guardar el traspaso
 */
function subirArchivosTraspaso() {
    return new Promise((resolve, reject) => {
        if (archivosAdjuntosTraspaso.length === 0) {
            resolve([]);
            return;
        }

        const formData = new FormData();

        // Agregar cada archivo al FormData
        archivosAdjuntosTraspaso.forEach((archivoObj) => {
            formData.append('archivos', archivoObj.file);
            formData.append('nombres_originales', archivoObj.nombre);
        });

        notificaciones.info(`Subiendo ${archivosAdjuntosTraspaso.length} archivo(s)...`, 'Procesando');

        AjaxService.archivos.subir(formData)
            .done(function(response) {
                console.log('✓ Archivos subidos:', response);

                // Procesar respuesta para obtener URLs
                let archivosGuardados = [];

                try {
                    if (response.exito && response.archivos && Array.isArray(response.archivos)) {
                        archivosGuardados = response.archivos;
                    } else if (Array.isArray(response)) {
                        archivosGuardados = response;
                    } else if (response.archivos && Array.isArray(response.archivos)) {
                        archivosGuardados = response.archivos;
                    } else if (response.data && Array.isArray(response.data)) {
                        archivosGuardados = response.data;
                    }

                    console.log('✓ Archivos procesados:', archivosGuardados);
                    resolve(archivosGuardados);
                } catch (e) {
                    console.error('❌ Error al procesar archivos:', e);
                    reject(e);
                }
            })
            .fail(function(xhr, status, error) {
                console.error('❌ Error al subir archivos:', error);
                reject(error);
            });
    });
}

// ===================================================================
// GESTIÓN DE ARCHIVOS ADJUNTOS DE RECEPCIÓN
// ===================================================================

/**
 * Maneja los archivos adjuntos seleccionados para la recepción
 */
function manejarArchivosAdjuntosRecepcion(e) {
    const files = e.target.files;

    if (!files || files.length === 0) {
        return;
    }

    archivosAdjuntosRecepcion = [];
    const archivosValidos = [];
    const errores = [];

    // Validar todos los archivos
    for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const validacion = validarArchivoTraspaso(file);

        if (validacion.valido) {
            archivosValidos.push({
                file: file,
                nombre: file.name,
                tamaño: (file.size / 1024).toFixed(2) + ' KB',
                tipo: file.type
            });
        } else {
            errores.push(validacion.error);
        }
    }

    // Mostrar errores agrupados
    if (errores.length > 0) {
        errores.forEach(error => notificaciones.warning(error, 'Validación'));
    }

    // Guardar archivos válidos
    archivosAdjuntosRecepcion = archivosValidos;

    if (archivosAdjuntosRecepcion.length > 0) {
        mostrarResumenArchivosRecepcion();
        notificaciones.success(`${archivosAdjuntosRecepcion.length} archivo(s) de recepción seleccionado(s)`, 'Éxito');
    } else {
        console.warn('⚠ No hay archivos válidos para procesar');
        notificaciones.error('No hay archivos válidos para adjuntar', 'Error');
    }
}

/**
 * Muestra un resumen de los archivos adjuntos de recepción
 */
function mostrarResumenArchivosRecepcion() {
    if (archivosAdjuntosRecepcion.length === 0) {
        return;
    }

    let resumenHTML = '<div class="alert alert-info mt-3" role="alert">';
    resumenHTML += '<i class="fas fa-info-circle"></i> <strong>Archivos seleccionados:</strong><br>';
    resumenHTML += '<ul class="mb-0 mt-2">';

    archivosAdjuntosRecepcion.forEach((archivo, index) => {
        resumenHTML += `<li>${index + 1}. ${archivo.nombre} (${archivo.tamaño})</li>`;
    });

    resumenHTML += '</ul></div>';

    // Insertar después del input de archivo
    const fileInput = document.getElementById('fileAdjuntosRecepcion');
    const existingAlert = fileInput.parentElement.querySelector('.alert-info');

    if (existingAlert && existingAlert.innerHTML.includes('Archivos seleccionados')) {
        existingAlert.remove();
    }

    fileInput.insertAdjacentHTML('afterend', resumenHTML);
}

/**
 * Sube los archivos adjuntos de recepción al servidor
 */
function subirArchivosRecepcion() {
    return new Promise((resolve, reject) => {
        if (archivosAdjuntosRecepcion.length === 0) {
            resolve([]);
            return;
        }

        const formData = new FormData();

        // Agregar cada archivo al FormData
        archivosAdjuntosRecepcion.forEach((archivoObj) => {
            formData.append('archivos', archivoObj.file);
            formData.append('nombres_originales', archivoObj.nombre);
        });

        notificaciones.info(`Subiendo ${archivosAdjuntosRecepcion.length} archivo(s) de recepción...`, 'Procesando');

        AjaxService.archivos.subir(formData)
            .done(function(response) {
                console.log('✓ Archivos de recepción subidos:', response);

                // Procesar respuesta para obtener URLs
                let archivosGuardados = [];

                try {
                    if (response.exito && response.archivos && Array.isArray(response.archivos)) {
                        archivosGuardados = response.archivos;
                    } else if (Array.isArray(response)) {
                        archivosGuardados = response;
                    } else if (response.archivos && Array.isArray(response.archivos)) {
                        archivosGuardados = response.archivos;
                    } else if (response.data && Array.isArray(response.data)) {
                        archivosGuardados = response.data;
                    }

                    console.log('✓ Archivos de recepción procesados:', archivosGuardados);
                    resolve(archivosGuardados);
                } catch (e) {
                    console.error('❌ Error al procesar archivos de recepción:', e);
                    reject(e);
                }
            })
            .fail(function(xhr, status, error) {
                console.error('❌ Error al subir archivos de recepción:', error);
                reject(error);
            });
    });
}

// ===================================================================
// UTILIDADES
// ===================================================================

/**
 * Parsea la respuesta del API
 */
function parseApiResponse(response) {
    if (typeof response === 'string') {
        try {
            return JSON.parse(response);
        } catch (e) {
            return response;
        }
    }
    return response;
}

/**
 * Formatea una fecha ISO a formato legible
 */
function formatearFecha(fechaISO) {
    if (!fechaISO) return '-';
    const fecha = new Date(fechaISO);
    return fecha.toLocaleDateString('es-MX', { year: 'numeric', month: '2-digit', day: '2-digit' });
}

/**
 * Muestra una notificación al usuario usando el servicio centralizado
 * @param {string} mensaje - Mensaje a mostrar
 * @param {string} tipo - Tipo de notificación: success, error, warning, info
 * @param {string} titulo - Título opcional
 */
function mostrarNotificacion(mensaje, tipo = 'info', titulo = '') {
    console.log(`[${tipo.toUpperCase()}] ${mensaje}`);

    switch(tipo.toLowerCase()) {
        case 'success':
        case 'exito':
            notificaciones.success(mensaje, titulo);
            break;
        case 'error':
        case 'danger':
            notificaciones.error(mensaje, titulo);
            break;
        case 'warning':
        case 'advertencia':
            notificaciones.warning(mensaje, titulo);
            break;
        case 'info':
        case 'informacion':
            notificaciones.info(mensaje, titulo);
            break;
        default:
            notificaciones.info(mensaje, titulo);
    }
}

// Exportar funciones globales necesarias
window.abrirModalNuevoTraspaso = abrirModalNuevoTraspaso;
window.cargarInventarioAlmacenTraspaso = cargarInventarioAlmacenTraspaso;
window.agregarArticuloTraspaso = agregarArticuloTraspaso;
window.actualizarCantidadArticuloTraspaso = actualizarCantidadArticuloTraspaso;
window.eliminarArticuloTraspaso = eliminarArticuloTraspaso;
window.guardarTraspaso = guardarTraspaso;
window.filtrarTraspasos = filtrarTraspasos;
window.verDetalleTraspaso = verDetalleTraspaso;
window.abrirModalRecibirTraspaso = abrirModalRecibirTraspaso;
window.confirmarRecepcionTraspaso = confirmarRecepcionTraspaso;
window.abrirModalCancelarTraspaso = abrirModalCancelarTraspaso;
window.confirmarCancelacionTraspaso = confirmarCancelacionTraspaso;
window.obtenerIconoArchivoTraspaso = obtenerIconoArchivoTraspaso;
window.visualizarArchivoTraspaso = visualizarArchivoTraspaso;

// ===================================================================
// LOG DE CARGA
// ===================================================================
console.log('✓ traspasos.js cargado completamente');
console.log('✓ Funciones exportadas:', {
    abrirModalNuevoTraspaso: typeof abrirModalNuevoTraspaso,
    inicializarTraspasos: typeof inicializarTraspasos,
    guardarTraspaso: typeof guardarTraspaso
});
