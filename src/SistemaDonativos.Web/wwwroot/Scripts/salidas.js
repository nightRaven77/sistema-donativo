/**
 * ====================================================================
 * SALIDAS.JS - Módulo de Registro de Salidas de Inventario
 * ====================================================================
 * Gestiona el registro y consulta de salidas (donaciones y mermas)
 * Integrado con el nuevo endpoint de stock: GET /catalogos/articulo/stock/{idAlmacen}
 */

// ===================================================================
// VARIABLES GLOBALES
// ===================================================================
let almacenesSalidas = [];
let beneficiarioSeleccionado = null;
let inventarioActual = [];
let articulosSalidaSeleccionados = [];
let timerBusquedaBeneficiario = null;
let timerBusquedaArticulo = null;
let archivosAdjuntosSalida = [];
let archivosExistentesSalida = [];
let dataTableSalidas = null; // DataTable para la tabla de salidas
let salidasCache = []; // Cache local de salidas para filtrado instantáneo

// ⭐ Cache de selectores jQuery para mejorar rendimiento
let SELECTORS_SALIDAS = {};

// ===================================================================
// CONFIGURACIÓN
// ===================================================================
const CONFIG_SALIDAS = {
    TIMEOUTS: {
        BUSQUEDA: 500,
        HIDE_LIST: 200,
        AUTO_HIDE_LIST: 3000  // ⭐ Agregado: tiempo para ocultar listas automáticamente
    },
    TIPOS_SALIDA: {
        DONACION: 'donacion',
        MERMA: 'merma'
    },
    FILES: {
        MAX_SIZE: 5 * 1024 * 1024, // 5MB
        ALLOWED_EXTENSIONS: ['jpg', 'jpeg', 'png', 'gif', 'pdf', 'doc', 'docx', 'xls', 'xlsx']
    }
};

// ===================================================================
// INICIALIZACIÓN
// ===================================================================

/**
 * Inicializa el módulo de salidas
 * Esta función se debe llamar cuando la vista de salidas se carga
 */
function inicializarSalidas() {
    console.log('✓ Inicializando módulo Salidas');

    // ⭐ Verificar que la tabla existe en el DOM antes de continuar
    if ($('#tablaSalidas').length === 0) {
        console.error('❌ Error: La tabla #tablaSalidas no existe en el DOM');
        console.log('⏳ Reintentando en 200ms...');
        setTimeout(() => inicializarSalidas(), 200);
        return;
    }

    // ⭐ Cachear selectores jQuery frecuentes
    SELECTORS_SALIDAS = {
        selectAlmacenOrigen: $('#selectAlmacenOrigen'),
        seccionArticulos: $('#seccionArticulos'),
        alertSeleccionarAlmacen: $('#alertSeleccionarAlmacen'),
        listaInventario: $('#listaInventario'),
        textBuscarBeneficiario: $('#textBuscarBeneficiario'),
        listaBeneficiarios: $('#listaBeneficiarios'),
        buscarArticuloInventario: $('#buscarArticuloInventario'),
        tablaArticulosSalida: $('#tablaArticulosSalida'),
        fechaSalida: $('#fechaSalida'),
        selectTipoSalida: $('#selectTipoSalida'),
        textObservacionesSalida: $('#textObservacionesSalida'),
        fileAdjuntosSalida: $('#fileAdjuntosSalida')
    };

    cargarAlmacenesSalidas();
    cargarSalidas();
    configurarEventosBusqueda();
    establecerFechaHoy();
    configurarEventosArchivos();

    // Filtrado reactivo al cambiar criterios
    $('#filtroTipoSalida, #filtroFechaDesde, #filtroFechaHasta').off('change.salidasFiltro').on('change.salidasFiltro', function () {
        filtrarSalidas();
    });
}

/**
 * Configura los eventos de archivos adjuntos
 */
function configurarEventosArchivos() {
    SELECTORS_SALIDAS.fileAdjuntosSalida.on('change', function(e) {
        manejarArchivosAdjuntosSalida(e);
    });
}

/**
 * Establece la fecha actual en el campo de fecha
 */
function establecerFechaHoy() {
    const hoy = new Date();
    const año = hoy.getFullYear();
    const mes = String(hoy.getMonth() + 1).padStart(2, '0');
    const dia = String(hoy.getDate()).padStart(2, '0');
    const fechaLocal = `${año}-${mes}-${dia}`;

    SELECTORS_SALIDAS.fechaSalida.val(fechaLocal);
}

/**
 * Configura los eventos de búsqueda (autocomplete)
 */
function configurarEventosBusqueda() {
    // Búsqueda de beneficiarios
    SELECTORS_SALIDAS.textBuscarBeneficiario.on('input', function() {
        clearTimeout(timerBusquedaBeneficiario);
        const termino = $(this).val().trim();

        if (termino.length >= 2) {
            timerBusquedaBeneficiario = setTimeout(() => {
                buscarBeneficiarios(termino);
            }, CONFIG_SALIDAS.TIMEOUTS.BUSQUEDA);
        } else {
            SELECTORS_SALIDAS.listaBeneficiarios.hide();
        }
    });

    // Búsqueda de artículos en inventario
    SELECTORS_SALIDAS.buscarArticuloInventario.on('input', function() {
        clearTimeout(timerBusquedaArticulo);
        const termino = $(this).val().trim();

        if (termino.length >= 2) {
            timerBusquedaArticulo = setTimeout(() => {
                buscarEnInventario(termino);
            }, CONFIG_SALIDAS.TIMEOUTS.BUSQUEDA);
        } else {
            SELECTORS_SALIDAS.listaInventario.hide();
        }
    });

    // Ocultar listas al hacer clic fuera
    $(document).on('click', function(e) {
        if (!$(e.target).closest('#textBuscarBeneficiario, #listaBeneficiarios').length) {
            SELECTORS_SALIDAS.listaBeneficiarios.hide();
        }
        if (!$(e.target).closest('#buscarArticuloInventario, #listaInventario').length) {
            SELECTORS_SALIDAS.listaInventario.hide();
        }
    });
}

// ===================================================================
// CARGA DE CATÁLOGOS
// ===================================================================

/**
 * Carga la lista de almacenes para el módulo de Salidas
 */
function cargarAlmacenesSalidas() {
    AjaxService.almacenes.obtenerTodos()
        .then(response => {
            console.log('✓ Almacenes obtenidos (Salidas):', response);
            const data = parseApiResponse(response);
            almacenesSalidas = Array.isArray(data) ? data : [];
            llenarSelectAlmacenesSalida();
        })
        .catch(error => {
            console.error('❌ Error al cargar almacenes (Salidas):', error);
            mostrarNotificacion('Error al cargar almacenes', 'error');
        });
}

/**
 * Llena el select de almacenes en el formulario de Salidas
 */
function llenarSelectAlmacenesSalida() {
    const $select = $('#selectAlmacenOrigen');
    $select.empty().append('<option value="">-- Seleccione --</option>');

    almacenesSalidas.forEach(almacen => {
        const id = almacen.id_almacen || almacen.idAlmacen || almacen.id;
        const nombre = almacen.nombre || `Almacén ${id}`;
        $select.append(`<option value="${id}">${nombre}</option>`);
    });

    const centroIDUsuario = $('#centroIDUsuario').val();

    // Seleccionar el almacén por defecto si existe
    if (centroIDUsuario && centroIDUsuario !== '' && centroIDUsuario !== '0') {
        $select.val(centroIDUsuario);
        // console.log(`✓ Almacén seleccionado por defecto: ${centroIDUsuario}`);
    }
    console.log(`✓ Select de almacenes llenado (Salidas) con ${almacenesSalidas.length} opciones`);
}

/**
 * Carga el inventario cuando se selecciona un almacén
 */
function cargarInventarioAlmacen() {
    const idAlmacen = $('#selectAlmacenOrigen').val();
    
    if (!idAlmacen) {
        $('#seccionArticulos').hide();
        $('#alertSeleccionarAlmacen').show();
        inventarioActual = [];
        return;
    }

    console.log(`📦 Cargando inventario del almacén: ${idAlmacen}`);
    $('#alertSeleccionarAlmacen').hide();
    $('#seccionArticulos').show();

    // Mostrar loading
    $('#listaInventario').html('<div class="list-group-item text-center"><i class="fas fa-spinner fa-spin"></i> Cargando inventario...</div>').show();

    AjaxService.inventario.obtenerPorAlmacen(idAlmacen)
        .then(response => {
            console.log('✓ Inventario obtenido:', response);
            inventarioActual = Array.isArray(response) ? response : [];
            
            if (inventarioActual.length === 0) {
                $('#listaInventario').html('<div class="list-group-item text-warning"><i class="fas fa-exclamation-triangle"></i> No hay artículos con stock en este almacén</div>').show();
                setTimeout(() => $('#listaInventario').hide(), 3000);
            } else {
                $('#listaInventario').hide();
                mostrarNotificacion(`${inventarioActual.length} artículos disponibles`, 'success');
            }
        })
        .catch(error => {
            console.error('❌ Error al cargar inventario:', error);
            $('#listaInventario').html('<div class="list-group-item text-danger"><i class="fas fa-times"></i> Error al cargar inventario</div>').show();
        });
}

// ===================================================================
// BÚSQUEDA DE BENEFICIARIOS
// ===================================================================

/**
 * Busca beneficiarios por nombre
 */
function buscarBeneficiarios(termino) {
    console.log(`🔍 Buscando beneficiarios: "${termino}"`);

    const $lista = $('#listaBeneficiarios');
    $lista.html('<div class="list-group-item text-center"><i class="fas fa-spinner fa-spin"></i> Buscando...</div>').show();

    AjaxService.beneficiarios.buscar(termino)
        .then(response => {
            console.log('✓ Beneficiarios encontrados:', response);
            const beneficiarios = Array.isArray(response) ? response : [];
            mostrarListaBeneficiarios(beneficiarios, termino);
        })
        .catch(error => {
            console.error('❌ Error al buscar beneficiarios:', error);
            $lista.html('<div class="list-group-item text-danger">Error al buscar</div>');
        });
}

/**
 * Muestra la lista de beneficiarios encontrados
 */
function mostrarListaBeneficiarios(beneficiarios, termino) {
    const $lista = $('#listaBeneficiarios');
    $lista.empty();
    
    if (beneficiarios.length === 0) {
        $lista.append(`
            <div class="list-group-item">
                <div class="text-muted">No se encontraron beneficiarios</div>
                <button type="button" class="btn btn-sm btn-success mt-2" onclick="abrirModalNuevoBeneficiario()">
                    <i class="fas fa-plus"></i> Crear "${termino}"
                </button>
            </div>
        `);
    } else {
        beneficiarios.forEach(b => {
            const id = b.id_beneficiario || b.idBeneficiario || b.id;
            const nombre = b.nombre || 'Sin nombre';
            const telefono = b.telefono ? `<small class="text-muted">Tel: ${b.telefono}</small>` : '';
            
            $lista.append(`
                <button type="button" class="list-group-item list-group-item-action" onclick="seleccionarBeneficiario(${id}, '${nombre.replace(/'/g, "\\'")}')">
                    <div class="font-weight-bold">${nombre}</div>
                    ${telefono}
                </button>
            `);
        });
    }
    
    $lista.show();
}

/**
 * Selecciona un beneficiario de la lista
 */
function seleccionarBeneficiario(id, nombre) {
    console.log(`✓ Beneficiario seleccionado: ${nombre} (ID: ${id})`);
    beneficiarioSeleccionado = { id, nombre };
    $('#id_beneficiario').val(id);
    $('#textBuscarBeneficiario').val(nombre);
    $('#listaBeneficiarios').hide();
}

// ===================================================================
// BÚSQUEDA DE ARTÍCULOS EN INVENTARIO
// ===================================================================

/**
 * Busca artículos en el inventario actual
 */
function buscarEnInventario(termino) {
    const idAlmacen = $('#selectAlmacenOrigen').val();
    
    if (!idAlmacen) {
        mostrarNotificacion('Selecciona un almacén primero', 'warning');
        return;
    }
    
    console.log(`🔍 Buscando en inventario: "${termino}"`);

    const $lista = $('#listaInventario');
    $lista.html('<div class="list-group-item text-center"><i class="fas fa-spinner fa-spin"></i> Buscando...</div>').show();

    // Llamar al endpoint con búsqueda
    AjaxService.inventario.obtenerPorAlmacen(idAlmacen, termino)
        .then(response => {
            console.log('✓ Resultados de búsqueda:', response);
            const articulos = Array.isArray(response) ? response : [];
            mostrarListaInventario(articulos);
        })
        .catch(error => {
            console.error('❌ Error al buscar en inventario:', error);
            $lista.html('<div class="list-group-item text-danger">Error al buscar</div>');
        });
}

/**
 * Muestra la lista de artículos del inventario
 */
function mostrarListaInventario(articulos) {
    const $lista = $('#listaInventario');
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
        
        // Verificar si ya está agregado
        const yaAgregado = articulosSalidaSeleccionados.some(a => a.id === id);
        
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
                <button type="button" class="list-group-item list-group-item-action" onclick="agregarArticuloSalida(${id}, '${nombre.replace(/'/g, "\\'")}', ${stock}, '${unidad}')">
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
 * Agrega un artículo a la salida
 */
function agregarArticuloSalida(id, nombre, stockDisponible, unidad) {
    console.log(`➕ Agregando artículo: ${nombre} (Stock: ${stockDisponible})`);
    
    // Verificar si ya existe
    if (articulosSalidaSeleccionados.some(a => a.id === id)) {
        mostrarNotificacion('Este artículo ya fue agregado', 'warning');
        return;
    }
    
    articulosSalidaSeleccionados.push({
        id,
        nombre,
        stockDisponible,
        unidad,
        cantidad: 1
    });
    
    renderizarArticulosSalida();
    $('#listaInventario').hide();
    $('#buscarArticuloInventario').val('');
    mostrarNotificacion(`${nombre} agregado`, 'success');
}

/**
 * Renderiza la lista de artículos seleccionados
 */
function renderizarArticulosSalida() {
    const $container = $('#articulosSeleccionados');
    
    if (articulosSalidaSeleccionados.length === 0) {
        $container.html('<div class="alert alert-info"><i class="fas fa-info-circle"></i> No hay artículos seleccionados</div>');
        return;
    }
    
    let html = '<div class="table-responsive"><table class="table table-sm table-bordered">';
    html += '<thead class="thead-light"><tr>';
    html += '<th>Artículo</th><th style="width:150px">Cantidad</th><th style="width:100px">Stock</th><th style="width:80px">Acciones</th>';
    html += '</tr></thead><tbody>';
    
    articulosSalidaSeleccionados.forEach((art, index) => {
        html += '<tr>';
        html += `<td><strong>${art.nombre}</strong></td>`;
        html += `<td>
            <input type="number" 
                   class="form-control form-control-sm" 
                   value="${art.cantidad}" 
                   min="1" 
                   max="${art.stockDisponible}"
                   onchange="actualizarCantidadArticulo(${index}, this.value)">
        </td>`;
        html += `<td class="text-center"><span class="badge badge-info">${art.stockDisponible} ${art.unidad}</span></td>`;
        html += `<td class="text-center">
            <button type="button" class="btn btn-sm btn-danger" onclick="eliminarArticuloSalida(${index})">
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
function actualizarCantidadArticulo(index, cantidad) {
    cantidad = parseInt(cantidad) || 1;
    const articulo = articulosSalidaSeleccionados[index];
    
    if (cantidad > articulo.stockDisponible) {
        mostrarNotificacion(`Cantidad máxima: ${articulo.stockDisponible}`, 'warning');
        cantidad = articulo.stockDisponible;
    }
    
    if (cantidad < 1) {
        cantidad = 1;
    }
    
    articulo.cantidad = cantidad;
    renderizarArticulosSalida();
}

/**
 * Elimina un artículo de la salida
 */
function eliminarArticuloSalida(index) {
    const articulo = articulosSalidaSeleccionados[index];
    console.log(`🗑️ Eliminando: ${articulo.nombre}`);
    
    articulosSalidaSeleccionados.splice(index, 1);
    renderizarArticulosSalida();
    mostrarNotificacion('Artículo eliminado', 'info');
}

// ===================================================================
// MODAL DE NUEVA SALIDA
// ===================================================================

/**
 * Abre el modal para crear una nueva salida
 */
function abrirModalNuevaSalida() {
    console.log('📝 Abriendo modal Nueva Salida');
    
    // Resetear formulario
    $('#formNuevaSalida')[0].reset();
    $('#id_beneficiario').val('');
    beneficiarioSeleccionado = null;
    articulosSalidaSeleccionados = [];
    inventarioActual = [];
    
    $('#seccionArticulos').hide();
    $('#alertSeleccionarAlmacen').show();
    $('#articulosSeleccionados').empty();
    $('#divBeneficiario').show();
    
    establecerFechaHoy();
    
    // Mostrar modal
    $('#modalNuevaSalida').modal('show');
}

/**
 * Cambia la visibilidad de campos según el tipo de salida
 */
function cambiarTipoSalida() {
    const tipo = $('#tipoSalida').val();
    console.log(`🔄 Tipo de salida cambiado: ${tipo}`);
    
    if (tipo === CONFIG_SALIDAS.TIPOS_SALIDA.MERMA) {
        // Para merma, no se requiere beneficiario
        $('#divBeneficiario').hide();
        $('#textBuscarBeneficiario').prop('required', false);
    } else {
        // Para donación, sí se requiere
        $('#divBeneficiario').show();
        $('#textBuscarBeneficiario').prop('required', true);
    }
}

// ===================================================================
// GUARDAR SALIDA
// ===================================================================

/**
 * Guarda la salida en el sistema
 */
function guardarSalida() {
    console.log('💾 Guardando salida...');

    // Validar formulario
    const form = $('#formNuevaSalida')[0];
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        mostrarNotificacion('Completa todos los campos requeridos', 'warning');
        return;
    }

    // Validar tipo de salida
    const tipoSalida = $('#tipoSalida').val();
    if (!tipoSalida) {
        mostrarNotificacion('Selecciona el tipo de salida', 'warning');
        return;
    }

    console.log(`🔍 Tipo de salida seleccionado: "${tipoSalida}"`);

    // Validar beneficiario para donaciones
    if (tipoSalida === CONFIG_SALIDAS.TIPOS_SALIDA.DONACION && !beneficiarioSeleccionado) {
        mostrarNotificacion('Selecciona un beneficiario', 'warning');
        $('#textBuscarBeneficiario').focus();
        return;
    }

    // Validar artículos
    if (articulosSalidaSeleccionados.length === 0) {
        mostrarNotificacion('Agrega al menos un artículo', 'warning');
        return;
    }

    // Deshabilitar botón
    const $btn = $('button[onclick="guardarSalida()"]');
    $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Guardando...');

    // Si hay archivos, subirlos primero
    if (archivosAdjuntosSalida.length > 0) {
        subirArchivosSalida()
            .then(archivosGuardados => {
                guardarSalidaCompleta(tipoSalida, archivosGuardados, $btn);
            })
            .catch(error => {
                console.error('❌ Error al subir archivos:', error);
                notificaciones.error('Error al subir los archivos', 'Error');
                $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Guardar Salida');
            });
    } else {
        // Sin archivos, guardar directamente
        guardarSalidaCompleta(tipoSalida, [], $btn);
    }
}

/**
 * Guarda la salida completa con archivos opcionales
 */
function guardarSalidaCompleta(tipoSalida, archivosGuardados, $btn) {
    // Obtener valores de los campos
    const motivoTexto = $('#motivoSalida').val() || '';
    const observacionesTexto = $('#observacionesSalida').val() || '';

    console.log(`📝 Motivo: "${motivoTexto}"`);
    console.log(`📝 Observaciones: "${observacionesTexto}"`);

    // Construir objeto de salida
    const salida = {
        fechaSalida: $('#fechaSalida').val(),
        idAlmacenOrigen: parseInt($('#selectAlmacenOrigen').val()),
        idBeneficiario: tipoSalida === CONFIG_SALIDAS.TIPOS_SALIDA.DONACION ? parseInt($('#id_beneficiario').val()) : null,
        motivo: motivoTexto,
        estatus: 'Entregada',
        observaciones: observacionesTexto,
        tipoSalida: tipoSalida,
        detalles: articulosSalidaSeleccionados.map(art => ({
            idArticulo: art.id,
            cantidadEntregada: art.cantidad,
            valorUnitario: 0,
            unidadMedida: art.unidad
        }))
    };

    // Agregar archivos si los hay
    if (archivosGuardados && archivosGuardados.length > 0) {
        salida.archivos = archivosGuardados;
        console.log(`📎 Incluyendo ${archivosGuardados.length} archivo(s)`);
    }

    console.log('📤 Objeto salida construido:');
    console.log('   - Tipo de Salida:', salida.tipoSalida);
    console.log('   - Motivo:', salida.motivo);
    console.log('   - Observaciones:', salida.observaciones);
    console.log('   - Archivos:', salida.archivos?.length || 0);
    console.log('📤 Datos completos a enviar:', salida);

    // Enviar al servidor
    AjaxService.salidas.insertar(salida)
        .then(response => {
            console.log('✓ Respuesta del servidor:', response);

            if (response.exito || response.Exito) {
                notificaciones.success('Salida registrada correctamente', 'success');
                $('#modalNuevaSalida').modal('hide');
                cargarSalidas();

                // Limpiar archivos
                archivosAdjuntosSalida = [];
                $('#fileAdjuntosSalida').val('');
                const existingAlert = document.getElementById('fileAdjuntosSalida').parentElement.querySelector('.alert');
                if (existingAlert) existingAlert.remove();
            } else {
                const mensaje = response.mensaje || response.Mensaje || 'Error al registrar la salida';
                notificaciones.error(mensaje, 'error');
            }
        })
        .catch(error => {
            console.error('❌ Error al guardar salida:', error);
            notificaciones.error('Error al guardar la salida', 'error');
        })
        .always(() => {
            $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Guardar Salida');
        });
}

// ===================================================================
// GESTIÓN DE BENEFICIARIOS
// ===================================================================

/**
 * Abre el modal para crear un nuevo beneficiario
 */
function abrirModalNuevoBeneficiario() {
    console.log('📝 Abriendo modal Nuevo Beneficiario');
    $('#formNuevoBeneficiario')[0].reset();
    $('#modalNuevoBeneficiario').modal('show');
}

/**
 * Guarda un nuevo beneficiario
 */
function guardarNuevoBeneficiario() {
    const nombre = $('#nombreBeneficiario').val().trim();

    if (!nombre) {
        mostrarNotificacion('El nombre es requerido', 'warning');
        return;
    }

    const beneficiario = {
        nombre: nombre,
        telefono: $('#telefonoBeneficiario').val() || '',
        email: $('#emailBeneficiario').val() || '',
        direccion: $('#direccionBeneficiario').val() || '',
        tipo: 'persona'
    };

    console.log('💾 Guardando beneficiario:', beneficiario);

    const $btn = $('button[onclick="guardarNuevoBeneficiario()"]');
    $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Guardando...');

    // ✅ Usar AjaxService para detección automática de ambiente
    AjaxService.beneficiarios.insertar(beneficiario)
        .done(function(response) {
            console.log('✓ Beneficiario guardado - Respuesta completa:', response);

            const data = parseApiResponse(response);
            console.log('✓ Data parseada:', data);

            // ⚠️ VERIFICAR SI ES UN ERROR ENVUELTO (HTTP 200 con exito: false)
            if (data.exito === false) {
                console.warn('⚠️ Respuesta indica error (exito: false)');

                // Intentar parsear el error como JSON (puede ser un 409)
                try {
                    const errorData = JSON.parse(data.error);
                    console.log('📦 Error parseado:', errorData);

                    // Si tiene existingResource, es un conflicto 409
                    if (errorData.existingResource || errorData.existingResources) {
                        // Crear un objeto XHR simulado para el error handler
                        const fakeXhr = {
                            status: 409,
                            responseText: data.error
                        };

                        // Llamar al manejador de errores
                        const manejado = manejarErrorAjax(fakeXhr, 'error', 'Conflict', {
                            recurso: 'Beneficiario',
                            onConflictConfirm: (existingResource, conflictData) => {
                                if (!existingResource) {
                                    console.error('❌ existingResource es null');
                                    mostrarNotificacion('Error al obtener datos del beneficiario existente', 'error');
                                    return;
                                }

                                const id = existingResource.id_beneficiario || 
                                           existingResource.idBeneficiario || 
                                           existingResource.id ||
                                           extraerIdRecurso(existingResource, 'beneficiario');

                                if (!id) {
                                    console.error('❌ No se pudo extraer el ID del beneficiario existente');
                                    console.error('existingResource:', existingResource);
                                    mostrarNotificacion('Error al obtener ID del beneficiario existente', 'error');
                                    return;
                                }

                                const nombre = $('#nombreBeneficiario').val().trim();
                                console.log(`✅ Usando beneficiario existente: ${nombre} (ID: ${id})`);

                                seleccionarBeneficiario(id, nombre);
                                $('#modalNuevoBeneficiario').modal('hide');
                                mostrarNotificacion('Beneficiario existente seleccionado correctamente', 'success');
                            },
                            onConflictCancel: () => {
                                console.log('❌ Usuario canceló usar beneficiario existente');
                            }
                        });

                        if (!manejado) {
                            mostrarNotificacion('Error al guardar el beneficiario', 'error');
                        }

                        $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Guardar');
                        return; // Salir de la función
                    }
                } catch (e) {
                    console.error('No se pudo parsear el error:', e);
                }

                // Si llegamos aquí, es un error genérico
                mostrarNotificacion(data.mensaje || 'Error al guardar el beneficiario', 'error');
                $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Guardar');
                return;
            }

            // ✅ Respuesta exitosa, continuar con el flujo normal
            const beneficiarioData = parseApiResponse(data.data);
            console.log('✓ Beneficiario data parseada:', beneficiarioData);

            if (!beneficiarioData) {
                console.error('❌ beneficiarioData es null');
                mostrarNotificacion('Error al procesar la respuesta del servidor', 'error');
                $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Guardar');
                return;
            }

            const id = beneficiarioData.id_beneficiario || beneficiarioData.idBeneficiario || beneficiarioData.id;
            console.log('✓ ID extraído:', id);

            if (id) {
                seleccionarBeneficiario(id, nombre);
                $('#modalNuevoBeneficiario').modal('hide');
                mostrarNotificacion('Beneficiario registrado correctamente', 'success');
            } else {
                mostrarNotificacion('Beneficiario guardado pero no se pudo obtener el ID', 'warning');
            }

            $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Guardar');
        })
        .fail(function(xhr, status, error) {
            console.error('❌ Error al guardar beneficiario:');
            console.error('   Status:', status);
            console.error('   Error:', error);
            console.error('   XHR:', xhr);
            console.error('   Response Text:', xhr.responseText);
            console.error('   Status Code:', xhr.status);

            // ⭐ NUEVO: Usar manejador centralizado de errores
            const manejado = manejarErrorAjax(xhr, status, error, {
                recurso: 'Beneficiario',
                onConflictConfirm: (existingResource, conflictData) => {
                    // Verificar que existingResource no sea null
                    if (!existingResource) {
                        console.error('❌ existingResource es null');
                        mostrarNotificacion('Error al obtener datos del beneficiario existente', 'error');
                        return;
                    }

                    // Extraer ID del beneficiario existente de forma segura
                    const id = existingResource.id_beneficiario || 
                               existingResource.idBeneficiario || 
                               existingResource.id ||
                               extraerIdRecurso(existingResource, 'beneficiario');

                    if (!id) {
                        console.error('❌ No se pudo extraer el ID del beneficiario existente');
                        console.error('existingResource:', existingResource);
                        mostrarNotificacion('Error al obtener ID del beneficiario existente', 'error');
                        return;
                    }

                    const nombre = $('#nombreBeneficiario').val().trim();

                    console.log(`✅ Usando beneficiario existente: ${nombre} (ID: ${id})`);

                    // Seleccionar el beneficiario existente
                    seleccionarBeneficiario(id, nombre);
                    $('#modalNuevoBeneficiario').modal('hide');
                    mostrarNotificacion('Beneficiario existente seleccionado correctamente', 'success');
                },
                onConflictCancel: () => {
                    console.log('❌ Usuario canceló usar beneficiario existente');
                    // El modal de beneficiario permanece abierto para corrección
                }
            });

            // Si no fue manejado como conflicto 409, mostrar errores genéricos manualmente
            if (!manejado) {
                if (status === 'timeout') {
                    mostrarNotificacion('La petición tardó demasiado tiempo. Verifica tu conexión.', 'error');
                } else if (xhr.status === 404) {
                    mostrarNotificacion('El endpoint no fue encontrado. Verifica la ruta.', 'error');
                } else if (xhr.status === 500) {
                    mostrarNotificacion('Error en el servidor. Revisa los logs.', 'error');
                } else {
                    mostrarNotificacion('Error al guardar el beneficiario', 'error');
                }
            }

            $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Guardar');
        });
}

// ===================================================================
// CONSULTA DE SALIDAS
// ===================================================================

/**
 * Inicializa DataTables para salidas
 */
function inicializarDataTablesSalidas() {
    // ⭐ Verificar que el elemento existe antes de inicializar
    if ($('#tablaSalidas').length === 0) {
        console.error('❌ No se puede inicializar DataTables: #tablaSalidas no existe en el DOM');
        return;
    }

    // ⭐ Destruir instancia anterior si existe
    if (dataTableSalidas) {
        console.log('🧹 Destruyendo instancia anterior de DataTable Salidas');
        dataTableSalidas.destroy();
        dataTableSalidas = null;
        // Limpiar el HTML de la tabla
        $('#tablaSalidas').empty();
        $('#tablaSalidas').html(`
            <thead class="table-light">
                <tr style="background-color: var(--guinda); color: white;">
                    <th>ID</th>
                    <th>Folio</th>
                    <th>Fecha</th>
                    <th>Tipo</th>
                    <th>Beneficiario</th>
                    <th>Almacén Origen</th>
                    <th>Artículos</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody></tbody>
        `);
    }

    console.log('🔄 Inicializando DataTables Salidas...');

    dataTableSalidas = $('#tablaSalidas').DataTable({
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
        order: [[1, 'desc']], // Ordenar por fecha descendente por defecto
        columnDefs: [
            {
                targets: -1, // Última columna (Acciones)
                orderable: false,
                searchable: false
            },
            {
                targets: 2, // Columna Tipo (badges)
                orderable: true,
                searchable: true
            }
        ],
        initComplete: function () {
            console.log('✓ DataTables Salidas inicializado correctamente');
        }
    });
}

/**
 * Carga la tabla de salidas
 */
function cargarSalidas() {
    console.log('📋 Cargando salidas...');

    AjaxService.salidas.obtenerTodas()
        .then(response => {
            console.log('✓ Salidas obtenidas:', response);
            salidasCache = Array.isArray(response) ? response : [];
            filtrarSalidas();
        })
        .catch(error => {
            console.error('❌ Error al cargar salidas:', error);
            mostrarNotificacion('Error al cargar las salidas', 'error');
        });
}

/**
 * Renderiza la tabla de salidas usando DataTables
 */
function renderizarTablaSalidas(salidas) {
    // Inicializar DataTables si no existe
    if (!dataTableSalidas) {
        inicializarDataTablesSalidas();
    }

    // Limpiar tabla
    dataTableSalidas.clear();

    if (salidas.length === 0) {
        dataTableSalidas.draw();
        console.log('⚠ No hay salidas para mostrar');
        return;
    }

    // Agregar filas usando DataTables
    salidas.forEach(salida => {
        const id = salida.id_salida || salida.idSalida || salida.id;
        const folio = salida.folio;
        const fecha = formatearFecha(salida.fecha_salida || salida.fechaSalida);

        // ⭐ IMPORTANTE: Asegurarse de obtener el TIPO correcto (no el motivo)
        const tipo = salida.tipo_salida || salida.tipoSalida || salida.tipo || '';
        const beneficiario = salida.beneficiario?.nombre || salida.nombreBeneficiario || '-';
        const almacen = salida.almacenOrigen?.nombre || salida.nombreAlmacen || '-';
        const numArticulos = salida.cantidadArticulos || 0;

        // Badge del tipo de salida
        let tipoBadge = '';
        if (tipo.toLowerCase() === 'donacion') {
            tipoBadge = '<span class="badge badge-success">Donación</span>';
        } else if (tipo.toLowerCase() === 'merma') {
            tipoBadge = '<span class="badge badge-warning">Merma</span>';
        } else {
            tipoBadge = `<span class="badge badge-secondary">${tipo || 'N/A'}</span>`;
        }

        // Botones de acción
        const botones = `
            <button class="btn btn-sm btn-secondary" onclick="imprimirSalida(${id})" title="Imprimir Comprobante"><i class="fas fa-print"></i></button> <button class="btn btn-sm btn-info" onclick="verDetalleSalida(${id})" title="Ver detalle">
                <i class="fas fa-eye"></i>
            </button>
            <button class="btn btn-sm btn-danger" onclick="eliminarSalida(${id})" title="Eliminar">
                <i class="fas fa-trash"></i>
            </button>
        `;

        // Agregar fila a DataTables
        dataTableSalidas.row.add([
            id,
            folio,
            fecha,
            tipoBadge,
            beneficiario,
            almacen,
            `<span class="badge badge-info">${numArticulos}</span>`,
            botones
        ]);
    });

    // Redibujar tabla
    dataTableSalidas.draw();
    console.log(`✓ DataTables Salidas actualizado con ${salidas.length} registros`);

    // ⭐ Forzar actualización visual después de un breve delay
    setTimeout(() => {
        if (dataTableSalidas) {
            dataTableSalidas.columns.adjust().responsive.recalc();
            console.log('✓ Columnas ajustadas y responsive recalculado');
        }
    }, 100);
}

/**
 * Muestra el detalle de una salida
 */
function verDetalleSalida(id) {
    console.log(`👁️ Ver detalle de salida: ${id}`);
    
    const $modal = $('#modalDetalleSalida');
    const $contenido = $('#contenidoDetalleSalida');

    $contenido.html('<div class="text-center"><i class="fas fa-spinner fa-spin fa-2x"></i><p>Cargando...</p></div>');
    $modal.modal('show');

    AjaxService.salidas.obtenerPorId(id)
        .then(response => {
            console.log('✓ Detalle obtenido:', response);
            const salida = parseApiResponse(response.data || response.Data || response);
            renderizarDetalleSalida(salida);
        })
        .catch(error => {
            console.error('❌ Error al obtener detalle:', error);
            $contenido.html('<div class="alert alert-danger">Error al cargar el detalle</div>');
        });
}

/**
 * Renderiza el detalle de una salida
 */
function renderizarDetalleSalida(salida) {
    const $contenido = $('#contenidoDetalleSalida');

    const fecha = formatearFecha(salida.fecha_salida || salida.fechaSalida);

    // ⭐ Separar claramente TIPO y MOTIVO
    const tipo = salida.tipo_salida || salida.tipoSalida || salida.tipo || 'N/A';
    const motivo = salida.motivo || 'Sin motivo especificado';

    const beneficiario = salida.nombreBeneficiario || 'N/A';
    const almacen = salida.nombreAlmacen || 'N/A';
    const observaciones = salida.observaciones || 'Sin observaciones';

    // Badge del tipo
    let tipoBadge = '';
    if (tipo.toLowerCase() === 'donacion') {
        tipoBadge = '<span class="badge badge-success badge-lg">Donación</span>';
    } else if (tipo.toLowerCase() === 'merma') {
        tipoBadge = '<span class="badge badge-warning badge-lg">Merma</span>';
    } else {
        tipoBadge = `<span class="badge badge-secondary badge-lg">${tipo}</span>`;
    }

    let html = `
        <div class="row">
            <div class="col-md-6">
                <p><strong><i class="fas fa-calendar"></i> Fecha:</strong> ${fecha}</p>
            </div>
            <div class="col-md-6">
                <p><strong><i class="fas fa-tag"></i> Tipo de Salida:</strong> ${tipoBadge}</p>
            </div>
        </div>
        <div class="row mt-2">
            <div class="col-md-6">
                <p><strong><i class="fas fa-user"></i> Beneficiario:</strong> ${beneficiario}</p>
            </div>
            <div class="col-md-6">
                <p><strong><i class="fas fa-warehouse"></i> Almacén Origen:</strong> ${almacen}</p>
            </div>
        </div>
        <div class="row mt-2">
            <div class="col-md-12">
                <p><strong><i class="fas fa-comment"></i> Motivo:</strong></p>
                <div class="alert alert-info mb-2">
                    ${motivo}
                </div>
            </div>
        </div>
        <div class="row mt-2">
            <div class="col-md-12">
                <p><strong><i class="fas fa-clipboard"></i> Observaciones:</strong></p>
                <div class="alert alert-secondary">
                    ${observaciones}
                </div>
            </div>
        </div>
        <hr>
        <h6 class="font-weight-bold"><i class="fas fa-box-open"></i> Artículos Entregados:</h6>
        <table class="table table-sm table-bordered">
            <thead class="thead-light">
                <tr>
                    <th>Artículo</th>
                    <th style="width: 150px;">Cantidad</th>
                    <th style="width: 100px;">Unidad</th>
                </tr>
            </thead>
            <tbody>
    `;

    if (salida.detalles && salida.detalles.length > 0) {
        salida.detalles.forEach(d => {
            const nombre = d.articulo?.nombreArticulo || d.nombreArticulo || 'N/A';
            const cantidad = d.cantidad_entregada || d.cantidadEntregada || 0;
            const unidad = d.unidad_medida || d.unidadMedida || '';
            html += `<tr>
                <td><strong>${nombre}</strong></td>
                <td class="text-center">${cantidad}</td>
                <td>${unidad}</td>
            </tr>`;
        });
    } else {
        html += '<tr><td colspan="3" class="text-center text-muted"><i class="fas fa-inbox"></i> Sin artículos</td></tr>';
    }

    html += '</tbody></table>';

    // ⭐ Sección de archivos adjuntos (si los hay)
    if (salida.archivos && salida.archivos.length > 0) {
        html += `
            <hr class="my-4">
            <h6 class="font-weight-bold"><i class="fas fa-paperclip"></i> Archivos Adjuntos (${salida.archivos.length})</h6>
            <div class="row">`;

        salida.archivos.forEach((archivo) => {
            const extension = (archivo.nombreArchivo || '').split('.').pop().toLowerCase();
            const iconoArchivo = obtenerIconoArchivo(extension);
            // ⭐ Usar AjaxService.buildUrl() para construir la URL correcta según el ambiente
            const urlCompleta = AjaxService.buildUrl(`/Registros/ObtenerArchivo?archivo=${encodeURIComponent(archivo.urlArchivo || archivo.url || '')}`);

            html += `
                <div class="col-md-6 col-lg-4 mb-3">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body text-center">
                            <i class="${iconoArchivo} fa-3x mb-3" style="color: var(--dorado);"></i>
                            <h6 class="card-title text-truncate" title="${archivo.nombreArchivo || 'Archivo'}">
                                ${archivo.nombreArchivo || 'Archivo'}
                            </h6>
                            <small class="text-muted">Tipo: ${archivo.tipoArchivo || 'General'}</small>
                            <div class="mt-3">
                                ${extension === 'jpg' || extension === 'jpeg' || extension === 'png' || extension === 'gif'
                    ? `<button type="button" class="btn btn-sm btn-info mb-2" onclick="visualizarArchivo('${urlCompleta}', '${archivo.nombreArchivo}', 'imagen')">
                                        <i class="fas fa-eye"></i> Ver
                                       </button>`
                    : extension === 'pdf'
                        ? `<button type="button" class="btn btn-sm btn-info mb-2" onclick="visualizarArchivo('${urlCompleta}', '${archivo.nombreArchivo}', 'pdf')">
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
function obtenerIconoArchivo(extension) {
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
function visualizarArchivo(url, nombreArchivo, tipo) {
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
 * Filtra las salidas según los criterios seleccionados
 */
function filtrarSalidas() {
    if (!Array.isArray(salidasCache)) {
        cargarSalidas();
        return;
    }

    const tipoFiltro = ($('#filtroTipoSalida').val() || '').toLowerCase().trim();
    const fechaDesde = $('#filtroFechaDesde').val() || '';
    const fechaHasta = $('#filtroFechaHasta').val() || '';

    const salidasFiltradas = salidasCache.filter(salida => {
        const tipoSalida = (salida.tipo_salida || salida.tipoSalida || salida.tipo || '').toLowerCase().trim();
        if (tipoFiltro && tipoSalida !== tipoFiltro) {
            return false;
        }

        const rawFecha = salida.fecha_salida || salida.fechaSalida || '';
        const fechaISO = rawFecha ? String(rawFecha).substring(0, 10) : '';

        if (fechaDesde && (!fechaISO || fechaISO < fechaDesde)) {
            return false;
        }

        if (fechaHasta && (!fechaISO || fechaISO > fechaHasta)) {
            return false;
        }

        return true;
    });

    renderizarTablaSalidas(salidasFiltradas);
}

// ===================================================================
// ELIMINAR SALIDAS
// ===================================================================

/**
 * Elimina la salida 
 */
function eliminarSalida(id) {
    Swal.fire({
        title: '¿Eliminar salida?',
        text: 'Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            notificaciones.info('Eliminando Salida...', 'Procesando');

            AjaxService.salidas.eliminar(id)
                .then(response => {
                    console.log('Salida eliminada:', response);
                    notificaciones.success('Salida eliminada correctamente', 'Éxito');
                    cargarSalidas();
                })
                .catch(error => {
                    console.error('Error al eliminar salida:', error);
                    notificaciones.error('Error al eliminar la salida', 'Error');
                });
        }
    });
}

// ===================================================================
// GESTIÓN DE ARCHIVOS ADJUNTOS
// ===================================================================

/**
 * Maneja los archivos adjuntos seleccionados para salidas
 * Valida tipo y tamaño de archivos
 */
function manejarArchivosAdjuntosSalida(e) {
    const files = e.target.files;

    if (!files || files.length === 0) {
        return;
    }

    archivosAdjuntosSalida = [];
    const archivosValidos = [];
    const errores = [];

    // Validar todos los archivos
    for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const validacion = validarArchivoSalida(file);

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
    archivosAdjuntosSalida = archivosValidos;

    if (archivosAdjuntosSalida.length > 0) {
        mostrarResumenArchivosSalida();
        notificaciones.success(`${archivosAdjuntosSalida.length} archivo(s) seleccionado(s) correctamente`, 'Éxito');
    } else {
        console.warn('⚠ No hay archivos válidos para procesar');
        notificaciones.error('No hay archivos válidos para adjuntar', 'Error');
    }
}

/**
 * Valida un archivo individual para salidas
 * @param {File} file - Archivo a validar
 * @returns {Object} Resultado de validación
 */
function validarArchivoSalida(file) {
    const fileExtension = file.name.split('.').pop().toLowerCase();

    // Validar extensión
    if (!CONFIG_SALIDAS.FILES.ALLOWED_EXTENSIONS.includes(fileExtension)) {
        return {
            valido: false,
            error: `"${file.name}" - Tipo no permitido: ${fileExtension}`
        };
    }

    // Validar tamaño
    if (file.size > CONFIG_SALIDAS.FILES.MAX_SIZE) {
        const sizeMB = (file.size / (1024 * 1024)).toFixed(2);
        return {
            valido: false,
            error: `"${file.name}" excede 5MB (${sizeMB}MB)`
        };
    }

    return { valido: true };
}

/**
 * Muestra un resumen de los archivos adjuntos seleccionados para salidas
 */
function mostrarResumenArchivosSalida() {
    if (archivosAdjuntosSalida.length === 0) {
        return;
    }

    let resumenHTML = '<div class="alert alert-info mt-3" role="alert">';
    resumenHTML += '<i class="fas fa-info-circle"></i> <strong>Archivos seleccionados:</strong><br>';
    resumenHTML += '<ul class="mb-0 mt-2">';

    archivosAdjuntosSalida.forEach((archivo, index) => {
        resumenHTML += `<li>${index + 1}. ${archivo.nombre} (${archivo.tamaño})</li>`;
    });

    resumenHTML += '</ul></div>';

    // Insertar después del input de archivo
    const fileInput = document.getElementById('fileAdjuntosSalida');
    const existingAlert = fileInput.parentElement.querySelector('.alert');

    if (existingAlert) {
        existingAlert.remove();
    }

    fileInput.insertAdjacentHTML('afterend', resumenHTML);
}

/**
 * Sube los archivos adjuntos al servidor antes de guardar la salida
 */
function subirArchivosSalida() {
    return new Promise((resolve, reject) => {
        if (archivosAdjuntosSalida.length === 0) {
            resolve([]);
            return;
        }

        const formData = new FormData();

        // Agregar cada archivo al FormData
        archivosAdjuntosSalida.forEach((archivoObj) => {
            formData.append('archivos', archivoObj.file);
            formData.append('nombres_originales', archivoObj.nombre);
        });

        notificaciones.info(`Subiendo ${archivosAdjuntosSalida.length} archivo(s)...`, 'Procesando');

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
window.abrirModalNuevaSalida = abrirModalNuevaSalida;
window.cargarInventarioAlmacen = cargarInventarioAlmacen;
window.cambiarTipoSalida = cambiarTipoSalida;
window.seleccionarBeneficiario = seleccionarBeneficiario;
window.agregarArticuloSalida = agregarArticuloSalida;
window.actualizarCantidadArticulo = actualizarCantidadArticulo;
window.eliminarArticuloSalida = eliminarArticuloSalida;
window.guardarSalida = guardarSalida;
window.abrirModalNuevoBeneficiario = abrirModalNuevoBeneficiario;
window.guardarNuevoBeneficiario = guardarNuevoBeneficiario;
window.filtrarSalidas = filtrarSalidas;
window.verDetalleSalida = verDetalleSalida;
window.obtenerIconoArchivo = obtenerIconoArchivo;
window.visualizarArchivo = visualizarArchivo;

// ===================================================================
// LOG DE CARGA
// ===================================================================
console.log('✓ salidas.js cargado completamente');
console.log('✓ Funciones exportadas:', {
    abrirModalNuevaSalida: typeof abrirModalNuevaSalida,
    inicializarSalidas: typeof inicializarSalidas,
    guardarSalida: typeof guardarSalida
});

function imprimirSalida(id) {
    const url = AjaxService.buildUrl('Registros/ImprimirSalida?id=' + id + '&autoPrint=true');
    window.open(url, '_blank');
}
