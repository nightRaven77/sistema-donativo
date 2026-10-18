// ============================================================
// CONSULTA.JS - Gestión de Consulta de Movimientos e Inventario
// ============================================================

// Variables globales
let tablaMovimientos;
let tablaInventario;
let centroIDUsuario = null;
let estadisticasCargadas = false; // ⭐ Bandera para controlar carga de estadísticas

// ============================================================
// CONSTRUCCIÓN DE URL CON RUTA BASE (4 NIVELES)
// ============================================================
/**
 * Detecta el ambiente y construye la URL con la ruta base correcta
 * Soporta 4 niveles de servidor: /donaciones/
 * @param {string} relativeUrl - URL relativa del endpoint
 * @returns {string} URL completa con ruta base
 */
function getConsultaUrl(relativeUrl) {
    const pathname = window.location.pathname || '/';
    const basePath = pathname.toLowerCase().startsWith('/donaciones/') || pathname.toLowerCase() === '/donaciones'
        ? '/donaciones/'
        : '/';

    const cleanUrl = relativeUrl.startsWith('/') ? relativeUrl.substring(1) : relativeUrl;
    return `${basePath}${cleanUrl}`;
}

// ============================================================
// FUNCIÓN DE INICIALIZACIÓN (LLAMADA DESDE INDEX.CSHTML)
// ============================================================
function inicializarConsulta() {
    console.log('✓ Módulo de consulta inicializado');

    // Obtener el centro del usuario desde sessionStorage o variable global
    centroIDUsuario = usuarioSession?.centroID || sessionStorage.getItem('centroID');

    //if (!centroIDUsuario) {
    //    console.warn('⚠ No se encontró el ID del centro del usuario');
    //} else {
    //    console.log('✓ Centro ID del usuario:', centroIDUsuario);
    //}

    // Limpiar DataTables previas si existen
    destruirTablasExistentes();

    // Inicializar DataTables
    inicializarTablaMovimientos();
    inicializarTablaInventario();

    // Cargar catálogos para filtros
    cargarCatalogos();

    // Establecer fecha por defecto (último mes)
    establecerFechasPorDefecto();

    // Event listeners
    configurarEventListeners();

    // Cargar inventario automáticamente si el tab está activo
    $('#inventario-tab').on('shown.bs.tab', function () {
        if (centroIDUsuario) {
            // ⭐ Cargar estadísticas solo la primera vez que se muestra la pestaña
            if (!estadisticasCargadas) {
                //console.log('📊 Primera vez en pestaña Inventario - cargando estadísticas...');
                cargarEstadisticasAlmacen();
                estadisticasCargadas = true;
            }
            cargarInventario();
        }
    });
}

// ============================================================
// DESTRUIR TABLAS EXISTENTES
// ============================================================
function destruirTablasExistentes() {
    // Destruir tabla de movimientos si existe
    if (typeof tablaMovimientos !== 'undefined' && tablaMovimientos !== null) {
        try {
            //console.log('🧹 Destruyendo DataTable Movimientos');
            tablaMovimientos.destroy();
            tablaMovimientos = null;
        } catch (e) {
            console.log('⚠ Error al destruir tabla movimientos:', e);
        }
    }

    // Destruir tabla de inventario si existe
    if (typeof tablaInventario !== 'undefined' && tablaInventario !== null) {
        try {
            //console.log('🧹 Destruyendo DataTable Inventario');
            tablaInventario.destroy();
            tablaInventario = null;
        } catch (e) {
            console.log('⚠ Error al destruir tabla inventario:', e);
        }
    }
}

// ============================================================
// CONFIGURACIÓN DE EVENT LISTENERS
// ============================================================
function configurarEventListeners() {
    // Enter en inputs de texto para buscar
    $('#inputFolio, #inputBusquedaInventario').on('keypress', function (e) {
        if (e.which === 13) { // Enter
            if ($(this).attr('id') === 'inputFolio') {
                buscarMovimientos();
            } else {
                cargarInventario();
            }
        }
    });

    // Cambio en filtros de inventario
    $('#selectCategoriaInventario, #selectSoloConStock').on('change', function () {
        // Auto-buscar cuando cambian los filtros (opcional)
        // cargarInventario();
    });
}

// ============================================================
// ESTABLECER FECHAS POR DEFECTO
// ============================================================
function establecerFechasPorDefecto() {
    const hoy = new Date();
    const mesAtras = new Date();
    mesAtras.setMonth(mesAtras.getMonth() - 1);

    // Formato YYYY-MM-DD
    const formatoFecha = (fecha) => {
        const año = fecha.getFullYear();
        const mes = String(fecha.getMonth() + 1).padStart(2, '0');
        const dia = String(fecha.getDate()).padStart(2, '0');
        return `${año}-${mes}-${dia}`;
    };

    $('#inputFechaInicio').val(formatoFecha(mesAtras));
    $('#inputFechaFin').val(formatoFecha(hoy));
}

// ============================================================
// INICIALIZAR DATATABLE DE MOVIMIENTOS
// ============================================================
function inicializarTablaMovimientos() {
    tablaMovimientos = $('#tablaMovimientos').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
        },
        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        order: [[2, 'desc']], // Ordenar por fecha descendente
        columnDefs: [
            { targets: [9], orderable: false }, // Acciones no ordenables
            { targets: [7], className: 'text-end' }, // Cantidad alineada a la derecha
            { targets: [0, 1, 2, 4, 5, 6, 7, 8], className: 'small' }, // Texto pequeño en varias columnas
            {
                targets: 3, // Columna de descripción
                className: 'small',
                render: function(data, type, row) {
                    if (type === 'display' && data && data.length > 50) {
                        return '<span title="' + data + '">' + data.substr(0, 47) + '...</span>';
                    }
                    return data;
                }
            }
        ],
        dom: 'Bfrtip',
        buttons: []
    });
}

// ============================================================
// INICIALIZAR DATATABLE DE INVENTARIO
// ============================================================
function inicializarTablaInventario() {
    tablaInventario = $('#tablaInventario').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
        },
        responsive: true,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        order: [[0, 'asc']], // Ordenar por nombre de artículo
        columnDefs: [
            { targets: [3, 4, 5], className: 'text-end' }, // Stock, entradas, salidas alineadas
        ]
    });
}

// ============================================================
// CARGAR CATÁLOGOS PARA FILTROS
// ============================================================
async function cargarCatalogos() {
    try {
        // Cargar artículos para filtro
        //console.log('📦 Cargando catálogo de artículos...');
        const respArticulos = await fetch(getConsultaUrl('/Consulta/ObtenerArticulos'));
        const dataArticulos = await respArticulos.json();

        //console.log('📦 Respuesta completa de artículos:', dataArticulos);
        //console.log('📦 Tipo de dataArticulos.data:', typeof dataArticulos.data);
        //console.log('📦 Es array dataArticulos.data?:', Array.isArray(dataArticulos.data));

        if (dataArticulos.data) {
            //console.log('📦 Contenido de dataArticulos.data:', dataArticulos.data);
            //console.log('📦 Longitud del array:', dataArticulos.data.length);
            if (dataArticulos.data.length > 0) {
                //console.log('📦 Primer artículo:', dataArticulos.data[0]);
            }
        }

        if (dataArticulos.exito && dataArticulos.data) {
            const selectArticulo = $('#selectArticulo');
            selectArticulo.empty().append('<option value="">-- Todos los artículos --</option>');

            dataArticulos.data.forEach((articulo, index) => {
                //console.log(`  Artículo ${index}:`, articulo);
                const idArticulo = articulo.id_articulo || articulo.IdArticulo || articulo.id;
                const nombre = articulo.nombre || articulo.Nombre || articulo.name || 'Sin nombre';

                //console.log(`    -> ID: ${idArticulo}, Nombre: "${nombre}"`);

                selectArticulo.append(`<option value="${idArticulo}">${nombre}</option>`);
            });
            //console.log('✅ Artículos cargados correctamente');
        } else {
            console.error('❌ Error cargando artículos:', dataArticulos);
        }

        // Cargar categorías para filtro de inventario
        //console.log('📦 Cargando catálogo de categorías...');
        const respCategorias = await fetch(getConsultaUrl('/Consulta/ObtenerCategorias'));
        const dataCategorias = await respCategorias.json();

        //console.log('📦 Respuesta completa de categorías:', dataCategorias);
        //console.log('📦 Tipo de dataCategorias.data:', typeof dataCategorias.data);
        //console.log('📦 Es array dataCategorias.data?:', Array.isArray(dataCategorias.data));

        //if (dataCategorias.data) {
        //    //console.log('📦 Contenido de dataCategorias.data:', dataCategorias.data);
        //    //console.log('📦 Longitud del array:', dataCategorias.data.length);
        //    if (dataCategorias.data.length > 0) {
        //        console.log('📦 Primera categoría:', dataCategorias.data[0]);
        //    }
        //}

        if (dataCategorias.exito && dataCategorias.data) {
            const selectCategoria = $('#selectCategoriaInventario');
            selectCategoria.empty().append('<option value="">Todas las categorías</option>');

            dataCategorias.data.forEach((categoria, index) => {
                //console.log(`  Categoría ${index}:`, categoria);
                const idCategoria = categoria.id_categoria || categoria.IdCategoria || categoria.id;
                const nombre = categoria.nombre || categoria.Nombre || categoria.name || 'Sin nombre';

                //console.log(`    -> ID: ${idCategoria}, Nombre: "${nombre}"`);

                selectCategoria.append(`<option value="${idCategoria}">${nombre}</option>`);
            });
            //console.log('✅ Categorías cargadas correctamente');
        } else {
            console.error('❌ Error cargando categorías:', dataCategorias);
        }

    } catch (error) {
        console.error('❌ Error al cargar catálogos:', error);
    }
}

// ============================================================
// BUSCAR MOVIMIENTOS
// ============================================================
async function buscarMovimientos(pagina = 1) {
    try {
        // Mostrar indicador de carga
        tablaMovimientos.clear().draw();
        mostrarLoader('#tablaMovimientos tbody', 'Buscando movimientos...');

        // Obtener valores de filtros
        const filtros = {
            fechaInicio: $('#inputFechaInicio').val(),
            fechaFin: $('#inputFechaFin').val(),
            tipoMovimiento: $('#selectTipoMovimiento').val() || 'todos',
            folio: $('#inputFolio').val(),
            idArticulo: $('#selectArticulo').val() || null,
            idAlmacen: centroIDUsuario,
            estatus: $('#selectEstatus').val() || null,
            pagina: pagina,
            registrosPorPagina: 50
        };

        // Construir query string
        const queryString = Object.entries(filtros)
            .filter(([key, value]) => value !== null && value !== '')
            .map(([key, value]) => `${key}=${encodeURIComponent(value)}`)
            .join('&');

        const url = getConsultaUrl(`/Consulta/ObtenerMovimientos?${queryString}`);
        //console.log('🔍 Consultando:', url);

        const response = await fetch(url);
        const data = await response.json();

        // 🔍 DEBUG: Ver qué está llegando
        //console.log('📦 Respuesta completa de la API:', data);
        //console.log('📊 Estructura de data.data:', data.data);
        //if (data.data && data.data.movimientos) {
        //    console.log('✅ Primer movimiento:', data.data.movimientos[0]);
        //}

        if (data.exito && data.data) {
            renderizarMovimientos(data.data);
            $('#totalResultados').text(data.data.totalRegistros || 0);
            $('#btnExportar').prop('disabled', (data.data.totalRegistros || 0) === 0);
        } else {
            //console.error('❌ Error en la respuesta:', data);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: data.error || data.mensaje || 'No se pudieron cargar los movimientos'
            });
            tablaMovimientos.clear().draw();
        }

    } catch (error) {
        console.error('Error al buscar movimientos:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error al conectar con el servidor'
        });
        tablaMovimientos.clear().draw();
    }
}

// ============================================================
// RENDERIZAR MOVIMIENTOS EN DATATABLE
// ============================================================
function renderizarMovimientos(data) {
    //console.log('🎨 Renderizando movimientos. Data recibida:', data);
    tablaMovimientos.clear();

    if (!data.movimientos || data.movimientos.length === 0) {
        console.warn('⚠ No hay movimientos para mostrar');
        tablaMovimientos.draw();
        return;
    }

    //console.log(`📋 Renderizando ${data.movimientos.length} movimientos`);

    data.movimientos.forEach((mov, index) => {
        // 🔍 DEBUG: Ver el movimiento completo y el tipo específicamente
        //console.log(`Movimiento ${index}:`, mov);
        //console.log(`  -> Tipo desde API: "${mov.tipo}", TipoMovimiento alternativo: "${mov.tipoMovimiento}"`);

        // Badge de tipo (compacto, sin saltos de línea)
        let badgeTipo = '';
        const tipo = mov.tipo || mov.tipoMovimiento;
        switch (tipo) {
            case 'entrada':
                badgeTipo = '<span class="badge bg-success text-white" style="white-space: nowrap;"><i class="fas fa-arrow-down"></i> Entrada</span>';
                break;
            case 'salida':
                badgeTipo = '<span class="badge bg-danger text-white" style="white-space: nowrap;"><i class="fas fa-arrow-up"></i> Salida</span>';
                break;
            case 'traspaso':
                badgeTipo = '<span class="badge bg-info text-white" style="white-space: nowrap;"><i class="fas fa-exchange-alt"></i> Traspaso</span>';
                break;
            default:
                badgeTipo = '<span class="badge bg-secondary" style="white-space: nowrap;">' + (tipo || 'N/A') + '</span>';
        }

        // Badge de estatus (compacto)
        let badgeEstatus = '';
        const estatus = (mov.estatus || '').toLowerCase();
        switch (estatus) {
            case 'completado':
            case 'recibido':
            case 'entregada':
            case 'aprobado':
                badgeEstatus = '<span class="badge bg-success text-white" style="white-space: nowrap;">' + mov.estatus + '</span>';
                break;
            case 'pendiente':
            case 'en proceso':
                badgeEstatus = '<span class="badge bg-warning text-dark" style="white-space: nowrap;">' + mov.estatus + '</span>';
                break;
            case 'cancelado':
            case 'rechazado':
                badgeEstatus = '<span class="badge bg-danger text-white" style="white-space: nowrap;">' + mov.estatus + '</span>';
                break;
            default:
                badgeEstatus = `<span class="badge bg-secondary" style="white-space: nowrap;">${mov.estatus || 'N/A'}</span>`;
        }

        // Almacén
        let almacen = mov.almacenOrigen || mov.almacen_origen || '';
        if (mov.almacenDestino || mov.almacen_destino) {
            almacen += ` → ${mov.almacenDestino || mov.almacen_destino}`;
        }

        // Fecha formateada
        const fecha = new Date(mov.fecha);
        const fechaFormateada = fecha.toLocaleDateString('es-MX', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });

        // Botón de acciones
        const tipoMovimiento = mov.tipo || mov.tipoMovimiento || 'entrada';
        const btnAcciones = `
            <button class="btn btn-sm btn-primary btn-ver-detalle" onclick="verDetalleMovimiento('${tipoMovimiento}', ${mov.id})">
                <i class="fas fa-eye"></i> Ver
            </button>
        `;

        const totalArticulos = mov.totalArticulos ?? mov.total_articulos ?? (mov.articulos?.length || 0);
        const cantidadTotal = Number(mov.cantidadTotal ?? mov.cantidad_total ?? 0);

        tablaMovimientos.row.add([
            mov.folio || '-',
            badgeTipo,
            fechaFormateada,
            (mov.descripcion || '-').substring(0, 80) + (mov.descripcion?.length > 80 ? '...' : ''),
            mov.responsable || '-',
            almacen || '-',
            totalArticulos,
            cantidadTotal.toFixed(2),
            badgeEstatus,
            btnAcciones
        ]);
    });

    tablaMovimientos.draw();
    //console.log('✅ Tabla renderizada correctamente');
}

// ============================================================
// VER DETALLE DE MOVIMIENTO
// ============================================================
async function verDetalleMovimiento(tipo, id) {
    try {
        // Mostrar modal con loader
        const modal = new bootstrap.Modal(document.getElementById('modalDetalleMovimiento'));
        modal.show();

        $('#detalleMovimientoContent').html(`
            <div class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Cargando...</span>
                </div>
                <p class="mt-3">Cargando detalles...</p>
            </div>
        `);

        const url = getConsultaUrl(`/Consulta/ObtenerDetalleMovimiento?tipo=${tipo}&id=${id}`);
        const response = await fetch(url);
        const data = await response.json();

        if (data.exito && data.data) {
            renderizarDetalleMovimiento(data.data);
        } else {
            $('#detalleMovimientoContent').html(`
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle"></i> ${data.error || 'Error al cargar el detalle'}
                </div>
            `);
        }

    } catch (error) {
        console.error('Error al ver detalle:', error);
        $('#detalleMovimientoContent').html(`
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle"></i> Error al conectar con el servidor
            </div>
        `);
    }
}

// ============================================================
// RENDERIZAR DETALLE COMPLETO DE MOVIMIENTO
// ============================================================
function renderizarDetalleMovimiento(detalle) {
    //console.log('📦 Detalle completo recibido:', detalle);

    // La estructura real de la API: todo está directamente en detalle, no en datos_principales
    const folio = detalle.folio || '';
    const fecha = new Date(detalle.fecha).toLocaleDateString('es-MX', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
    const tipo = (detalle.tipo_movimiento || detalle.tipo || '').toUpperCase();
    const estatus = detalle.estatus || '';

    // Intentar múltiples formas de obtener el almacén
    let almacenOrigen = '';
    if (detalle.almacen_origen) {
        almacenOrigen = typeof detalle.almacen_origen === 'object' ? detalle.almacen_origen.nombre : detalle.almacen_origen;
    } else if (detalle.almacen) {
        almacenOrigen = typeof detalle.almacen === 'object' ? detalle.almacen.nombre : detalle.almacen;
    } else if (detalle.almacenOrigen) {
        almacenOrigen = typeof detalle.almacenOrigen === 'object' ? detalle.almacenOrigen.nombre : detalle.almacenOrigen;
    }

    const almacenDestino = detalle.almacen_destino?.nombre || detalle.almacen_destino || '';

    const responsable = typeof detalle.responsable === 'object'
        ? (detalle.responsable?.nombre || '')
        : (detalle.responsable || '');
    const tipoResponsable = detalle.responsable?.tipo || detalle.tipoResponsable || '';

    const observaciones = detalle.observaciones || '';

    let html = `
        <div class="row mb-3">
            <div class="col-md-6">
                <p><strong>Folio:</strong> ${folio}</p>
                <p><strong>Fecha:</strong> ${fecha}</p>
                <p><strong>Tipo:</strong> ${tipo}</p>
                <p><strong>Estatus:</strong> <span class="badge bg-success">${estatus}</span></p>
            </div>
            <div class="col-md-6">
                <p><strong>Almacén:</strong> ${almacenOrigen}</p>
    `;

    if (almacenDestino) {
        html += `<p><strong>Almacén Destino:</strong> ${almacenDestino}</p>`;
    }

    if (responsable) {
        const etiqueta = tipoResponsable === 'donador' ? 'Donador' : 'Beneficiario';
        html += `<p><strong>${etiqueta}:</strong> ${responsable}</p>`;
    }

    html += `
            </div>
        </div>
    `;

    if (observaciones) {
        html += `
            <div class="alert alert-info mb-3">
                <strong>Observaciones:</strong> ${observaciones}
            </div>
        `;
    }

    // Artículos: están en detalle.detalles (NO detalle_articulos)
    const articulos = detalle.detalles || [];
    //console.log(`📋 Total artículos: ${articulos.length}`, articulos);

    // Tabla de artículos
    html += `
        <h6 class="fw-bold mb-2">Artículos:</h6>
        <div class="table-responsive">
            <table class="table table-sm table-bordered">
                <thead class="table-light">
                    <tr>
                        <th>Artículo</th>
                        <th class="text-end">Cantidad</th>
                        <th>Unidad</th>
                        <th class="text-end">Valor Unit.</th>
                        <th class="text-end">Total</th>
                    </tr>
                </thead>
                <tbody>
    `;

    let totalGeneral = 0;
    let cantidadGeneral = 0;

    articulos.forEach((art, idx) => {
        //console.log(`  🔹 Artículo ${idx}:`, art);

        const nombreArticulo = art.nombre_articulo || art.nombreArticulo || 'Sin nombre';
        const unidadMedida = art.unidad_medida || art.unidadMedida || 'N/A';
        const cantidad = art.cantidad || 0;
        const valorUnitario = art.valor_unitario || art.valorUnitario || 0;
        const valorTotal = art.valor_total || art.valorTotal || (cantidad * valorUnitario);

        totalGeneral += valorTotal;
        cantidadGeneral += cantidad;

        //console.log(`    ✅ Nombre: "${nombreArticulo}", Unidad: "${unidadMedida}", Cant: ${cantidad}`);

        html += `
            <tr>
                <td>${nombreArticulo}</td>
                <td class="text-end">${cantidad.toFixed(2)}</td>
                <td>${unidadMedida}</td>
                <td class="text-end">$${valorUnitario.toFixed(2)}</td>
                <td class="text-end">$${valorTotal.toFixed(2)}</td>
            </tr>
        `;
    });

    html += `
                </tbody>
                <tfoot class="table-light">
                    <tr>
                        <th>TOTALES</th>
                        <th class="text-end">${cantidadGeneral.toFixed(2)}</th>
                        <th></th>
                        <th></th>
                        <th class="text-end">$${totalGeneral.toFixed(2)}</th>
                    </tr>
                </tfoot>
            </table>
        </div>
    `;

    // Archivos adjuntos
    const archivos = detalle.archivos_adjuntos || detalle.archivosAdjuntos || [];
    if (archivos && archivos.length > 0) {
        console.log(`📎 Archivos adjuntos: ${archivos.length}`, archivos);

        html += `
            <hr class="my-4">
            <h6 class="fw-bold"><i class="fas fa-paperclip"></i> Archivos Adjuntos (${archivos.length}):</h6>
            <div class="row">
        `;

        archivos.forEach((archivo, idx) => {
            const nombreArchivo = archivo.nombre_original || archivo.nombreOriginal || 'Archivo sin nombre';
            const url = archivo.url || archivo.ruta_archivo || archivo.rutaArchivo || '';
            const tamano = archivo.tamano || 0;
            const tamanoKB = (tamano / 1024).toFixed(2);

            // Determinar extensión
            const extension = nombreArchivo.split('.').pop().toLowerCase();

            // Obtener icono según extensión
            let iconoArchivo = 'fas fa-file text-secondary';
            let colorIcono = 'var(--guinda)';
            switch(extension) {
                case 'pdf':
                    iconoArchivo = 'fas fa-file-pdf';
                    colorIcono = '#dc3545';
                    break;
                case 'jpg':
                case 'jpeg':
                case 'png':
                case 'gif':
                    iconoArchivo = 'fas fa-file-image';
                    colorIcono = '#0dcaf0';
                    break;
                case 'doc':
                case 'docx':
                    iconoArchivo = 'fas fa-file-word';
                    colorIcono = '#2b579a';
                    break;
                case 'xls':
                case 'xlsx':
                    iconoArchivo = 'fas fa-file-excel';
                    colorIcono = '#217346';
                    break;
            }

            // Construir URL completa usando el endpoint de Registros/ObtenerArchivo (como en salidas.js)
            // La API devuelve la ruta del archivo que debe pasarse al endpoint
            const archivoParam = encodeURIComponent(url);
            const urlCompleta = url.startsWith('http') 
                ? url 
                : getConsultaUrl(`/Registros/ObtenerArchivo?archivo=${archivoParam}`);

            html += `
                <div class="col-md-6 col-lg-4 mb-3">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body text-center">
                            <i class="${iconoArchivo} fa-3x mb-3" style="color: ${colorIcono};"></i>
                            <h6 class="card-title text-truncate" title="${nombreArchivo}">
                                ${nombreArchivo}
                            </h6>
                            <small class="text-muted">${tamanoKB} KB</small>
                            <div class="mt-3">
            `;

            // Botón Ver para imágenes y PDFs
            if (['jpg', 'jpeg', 'png', 'gif'].includes(extension)) {
                html += `
                    <button type="button" class="btn btn-sm btn-info mb-2 me-2" onclick="visualizarArchivoConsulta('${urlCompleta}', '${nombreArchivo}', 'imagen')">
                        <i class="fas fa-eye"></i> Ver
                    </button>
                `;
            } else if (extension === 'pdf') {
                html += `
                    <button type="button" class="btn btn-sm btn-info mb-2 me-2" onclick="visualizarArchivoConsulta('${urlCompleta}', '${nombreArchivo}', 'pdf')">
                        <i class="fas fa-eye"></i> Ver
                    </button>
                `;
            }

            // Botón Descargar
            html += `
                                <a href="${urlCompleta}" download="${nombreArchivo}" class="btn btn-sm btn-success mb-2" target="_blank">
                                    <i class="fas fa-download"></i> Descargar
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });

        html += `
            </div>
        `;
    }

    // Usuario y fecha de registro
    if (detalle.usuario_registro || detalle.usuarioRegistro) {
        const usuario = detalle.usuario_registro?.nombre || detalle.usuarioRegistro?.nombre || '';
        html += `
            <p class="text-muted mt-3">
                <small>Registrado por: <strong>${usuario || 'Sistema'}</strong></small>
            </p>
        `;
    }

    $('#detalleMovimientoContent').html(html);
    console.log('✅ Detalle renderizado correctamente');
}

// ============================================================
// CARGAR INVENTARIO EN TIEMPO REAL
// ============================================================
async function cargarInventario() {
    if (!centroIDUsuario) {
        Swal.fire({
            icon: 'warning',
            title: 'Atención',
            text: 'No se ha identificado el almacén del usuario'
        });
        return;
    }

    try {
        // Mostrar indicador de carga
        tablaInventario.clear().draw();
        mostrarLoader('#tablaInventario tbody', 'Cargando inventario...');

        // Obtener filtros
        const busqueda = $('#inputBusquedaInventario').val();
        const idCategoria = $('#selectCategoriaInventario').val();
        const soloConStock = $('#selectSoloConStock').val() === 'true';

        // Construir URL
        let urlRelativa = `/Consulta/ObtenerInventario?idAlmacen=${centroIDUsuario}&soloConStock=${soloConStock}`;

        if (busqueda) urlRelativa += `&busqueda=${encodeURIComponent(busqueda)}`;
        if (idCategoria) urlRelativa += `&idCategoria=${idCategoria}`;

        const url = getConsultaUrl(urlRelativa);
        console.log('📦 Cargando inventario:', url);

        const response = await fetch(url);
        const data = await response.json();

        if (data.exito && data.data) {
            renderizarInventario(data.data);
            // ⭐ NO llamar a mostrarEstadisticasInventario() aquí
            // Las estadísticas se cargan solo una vez al entrar a la pestaña
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: data.error || 'No se pudo cargar el inventario'
            });
            tablaInventario.clear().draw();
        }

    } catch (error) {
        console.error('Error al cargar inventario:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error al conectar con el servidor'
        });
        tablaInventario.clear().draw();
    }
}

// ============================================================
// RENDERIZAR INVENTARIO EN DATATABLE
// ============================================================
function renderizarInventario(data) {
    console.log('📦 Renderizando inventario. Data recibida:', data);
    tablaInventario.clear();

    if (!data.articulos || data.articulos.length === 0) {
        console.warn('⚠ No hay artículos en el inventario');
        tablaInventario.draw();
        return;
    }

    console.log(`📋 Renderizando ${data.articulos.length} artículos`);

    data.articulos.forEach((art, index) => {
        // Determinar stock y alertas
        const stock = art.stockActual || art.stock_actual || 0;
        const alertaBajo = stock > 0 && stock < 10;

        // Estado del stock
        let estadoStock = '';
        if (stock === 0) {
            estadoStock = '<span class="stock-alert"><i class="fas fa-exclamation-circle"></i> Sin Stock</span>';
        } else if (alertaBajo) {
            estadoStock = '<span class="stock-warning"><i class="fas fa-exclamation-triangle"></i> Stock Bajo</span>';
        } else {
            estadoStock = '<span class="stock-ok"><i class="fas fa-check-circle"></i> Disponible</span>';
        }

        // Fechas formateadas (soportar ambos formatos)
        const fechaEntrada = art.fechaUltimaEntrada || art.fecha_ultima_entrada;
        const fechaSalida = art.fechaUltimaSalida || art.fecha_ultima_salida;

        const fechaUltimaEntrada = fechaEntrada
            ? new Date(fechaEntrada).toLocaleDateString('es-MX')
            : '-';

        const fechaUltimaSalida = fechaSalida
            ? new Date(fechaSalida).toLocaleDateString('es-MX')
            : '-';

        // Entradas y salidas totales (la API SÍ las retorna)
        const entradasTotales = art.entradas_totales || art.entradasTotales || 0;
        const salidasTotales = art.salidas_totales || art.salidasTotales || 0;

        // Categoria puede ser objeto {id, nombre} o string
        const categoriaNombre = art.categoria?.nombre || art.categoria?.Nombre || art.categoria || art.Categoria || '-';

        tablaInventario.row.add([
            art.nombre || art.Nombre || '-',
            categoriaNombre,
            art.unidadMedida || art.unidad_medida || art.UnidadMedida || '-',
            `<strong>${stock.toFixed(2)}</strong>`,
            entradasTotales.toFixed(2),
            salidasTotales.toFixed(2),
            fechaUltimaEntrada,
            fechaUltimaSalida,
            estadoStock
        ]);
    });

    tablaInventario.draw();
    console.log('✅ Tabla de inventario renderizada correctamente');
}

// ============================================================
// CARGAR ESTADÍSTICAS DEL ALMACÉN (UNA SOLA VEZ)
// ============================================================
async function cargarEstadisticasAlmacen() {
    console.log('📊 Cargando estadísticas del almacén (solo una vez)...');

    $('#statsInventario').show();

    // Resetear todas las estadísticas a 0
    console.log('🔄 Reseteando estadísticas a 0...');
    $('#statTotalArticulos').text('0');
    $('#statConStock').text('0');
    $('#statSinStock').text('0');
    $('#statAlertas').text('0');
    $('#statEntradasHoy').text('0');
    $('#statSalidasHoy').text('0');
    $('#statTraspasosHoy').text('0');
    $('#statEntradasMes').text('0');
    $('#statSalidasMes').text('0');
    $('#statTraspasosMes').text('0');
    $('#statRecibidosMes').text('0');

    try {
        console.log('📡 Llamando al endpoint de estadísticas...');

        const url = getConsultaUrl(`/Consulta/ObtenerEstadisticas?idAlmacen=${centroIDUsuario}`);
        const response = await fetch(url);
        const result = await response.json();

        console.log('📦 Respuesta del endpoint:', result);

        if (result.exito && result.data) {
            const stats = result.data;
            console.log('📊 Estadísticas recibidas:', stats);

            // Resumen de Inventario
            if (stats.resumen) {
                $('#statTotalArticulos').text(stats.resumen.totalArticulosCatalogo || 0);
                $('#statConStock').text(stats.resumen.articulosConStock || 0);
                $('#statSinStock').text(stats.resumen.articulosSinStock || 0);
                $('#statAlertas').text(stats.resumen.alertasStockBajo || 0);

                console.log(`✅ Resumen: Total=${stats.resumen.totalArticulosCatalogo}, Con Stock=${stats.resumen.articulosConStock}, Sin Stock=${stats.resumen.articulosSinStock}, Alertas=${stats.resumen.alertasStockBajo}`);
            }

            // Movimientos del Día
            if (stats.movimientosHoy) {
                $('#statEntradasHoy').text(stats.movimientosHoy.entradas || 0);
                $('#statSalidasHoy').text(stats.movimientosHoy.salidas || 0);
                $('#statTraspasosHoy').text(stats.movimientosHoy.traspasos || 0);

                console.log(`✅ Hoy: Entradas=${stats.movimientosHoy.entradas}, Salidas=${stats.movimientosHoy.salidas}, Traspasos=${stats.movimientosHoy.traspasos}`);
            }

            // Movimientos del Mes
            if (stats.movimientosEsteMes) {
                $('#statEntradasMes').text(stats.movimientosEsteMes.entradas || 0);
                $('#statSalidasMes').text(stats.movimientosEsteMes.salidas || 0);
                $('#statTraspasosMes').text(stats.movimientosEsteMes.traspasos || 0);
                $('#statRecibidosMes').text(stats.movimientosEsteMes.traspasosRecibidos || 0);

                console.log(`✅ Mes: Entradas=${stats.movimientosEsteMes.entradas}, Salidas=${stats.movimientosEsteMes.salidas}, Traspasos=${stats.movimientosEsteMes.traspasos}, Recibidos=${stats.movimientosEsteMes.traspasosRecibidos}`);
            }

            console.log('✅ Estadísticas cargadas correctamente y permanecerán fijas');

        } else {
            console.error('❌ El endpoint no devolvió datos:', result.error || result.mensaje);
            Swal.fire({
                icon: 'warning',
                title: 'Advertencia',
                text: 'No se pudieron cargar las estadísticas del almacén',
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3000
            });
        }

    } catch (error) {
        console.error('❌ Error al cargar estadísticas:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error al obtener estadísticas del almacén',
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000
        });
    }
}

// ============================================================
// MOSTRAR ESTADÍSTICAS DE INVENTARIO (DEPRECATED - YA NO SE USA)
// ============================================================
// ⚠️ ESTA FUNCIÓN YA NO SE USA - Las estadísticas se cargan con cargarEstadisticasAlmacen()
// Se mantiene solo por compatibilidad
async function mostrarEstadisticasInventario(data) {
    console.warn('⚠️ mostrarEstadisticasInventario() está deprecated. Usa cargarEstadisticasAlmacen() en su lugar.');
    // Esta función ya no hace nada, las estadísticas se cargan una sola vez
}

// ============================================================
// LIMPIAR FILTROS
// ============================================================
function limpiarFiltros() {
    // Limpiar filtros de movimientos
    $('#inputFolio').val('');
    $('#selectTipoMovimiento').val('todos');
    $('#selectEstatus').val('');
    $('#selectArticulo').val('');
    establecerFechasPorDefecto();

    // Limpiar filtros de inventario
    $('#inputBusquedaInventario').val('');
    $('#selectCategoriaInventario').val('');
    $('#selectSoloConStock').val('false');

    // Limpiar tablas
    tablaMovimientos.clear().draw();
    tablaInventario.clear().draw();
    $('#totalResultados').text('0');
    $('#btnExportar').prop('disabled', true);

    // ⭐ NO ocultar estadísticas, solo resetear bandera
    // $('#statsInventario').hide(); // ❌ REMOVIDO - no debe ocultar

    // ⭐ Resetear bandera de estadísticas para que se vuelvan a cargar
    estadisticasCargadas = false;

    console.log('🧹 Filtros limpiados. Estadísticas permanecen visibles.');
}

// ============================================================
// EXPORTAR A EXCEL
// ============================================================
async function exportarExcel() {
    console.log('📊 Iniciando exportación a Excel...');

    try {
        // Obtener filtros actuales
        const filtros = {
            fechaInicio: $('#inputFechaInicio').val(),
            fechaFin: $('#inputFechaFin').val(),
            tipoMovimiento: $('#selectTipoMovimiento').val() || 'todos',
            folio: $('#inputFolio').val(),
            idArticulo: $('#selectArticulo').val() || null,
            idAlmacen: centroIDUsuario,
            estatus: $('#selectEstatus').val() || null
        };

        // Construir query string
        const queryString = Object.entries(filtros)
            .filter(([key, value]) => value !== null && value !== '')
            .map(([key, value]) => `${key}=${encodeURIComponent(value)}`)
            .join('&');

        // ⭐ Usar fetch para verificar si hay datos antes de descargar
        console.log('📡 Verificando datos para exportar...');

        const url = getConsultaUrl(`/Consulta/ExportarMovimientos?${queryString}`);

        // ⭐ Crear un iframe oculto para la descarga (no recarga la página)
        const iframe = document.createElement('iframe');
        iframe.style.display = 'none';
        iframe.src = url;
        document.body.appendChild(iframe);

        // ⭐ Remover el iframe después de 5 segundos
        setTimeout(() => {
            document.body.removeChild(iframe);
            console.log('✅ Descarga iniciada correctamente');
        }, 5000);

    } catch (error) {
        console.error('❌ Error al exportar:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error al exportar los datos'
        });
    }
}

// ============================================================
// UTILIDADES
// ============================================================
function mostrarLoader(selector, mensaje = 'Cargando...') {
    $(selector).html(`
        <tr>
            <td colspan="10" class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Cargando...</span>
                </div>
                <p class="mt-3">${mensaje}</p>
            </td>
        </tr>
    `);
}

// ============================================================
// VISUALIZAR ARCHIVOS ADJUNTOS
// ============================================================
/**
 * Visualiza un archivo en un modal (imágenes o PDFs)
 * Copia del patrón de salidas.js
 * @param {string} url - URL del archivo
 * @param {string} nombreArchivo - Nombre del archivo
 * @param {string} tipo - Tipo: 'imagen' o 'pdf'
 */
function visualizarArchivoConsulta(url, nombreArchivo, tipo) {
    console.log('🔍 Visualizando archivo:', { url, nombreArchivo, tipo });

    let contenidoModal = '';

    if (tipo === 'imagen') {
        contenidoModal = `<img src="${url}" alt="${nombreArchivo}" style="max-width: 100%; height: auto;" onerror="console.error('Error cargando imagen:', '${url}')">`;
    } else if (tipo === 'pdf') {
        contenidoModal = `<iframe src="${url}" style="width: 100%; height: 70vh;" frameborder="0" onerror="console.error('Error cargando PDF:', '${url}')"></iframe>`;
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

console.log('✓ Módulo consulta.js cargado completamente');
