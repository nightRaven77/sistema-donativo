/**
 * ====================================================================
 * REPORTES.JS - Módulo de Generación de Reportes
 * ====================================================================
 * Gestiona la generación y descarga de reportes en formato Excel
 * Tipos de reportes: Entradas, Salidas, Traspasos, Existencias, Kardex
 */

// ===================================================================
// VARIABLES GLOBALES
// ===================================================================
let almacenesReportes = [];
let categoriasReportes = [];
let articulosReportes = [];
let donadoresReportes = [];
let beneficiariosReportes = [];

// Cache de selectores jQuery
let SELECTORS_REPORTES = {};

// ===================================================================
// CONFIGURACIÓN
// ===================================================================
const CONFIG_REPORTES = {
    TIPOS: {
        ENTRADAS: 'entradas',
        SALIDAS: 'salidas',
        TRASPASOS: 'traspasos',
        EXISTENCIAS: 'existencias',
        KARDEX: 'kardex'
    },
    TIMEOUT: 30000, // 30 segundos para generación de reportes
    FECHA_FORMATO: 'YYYY-MM-DD'
};

// ===================================================================
// INICIALIZACIÓN
// ===================================================================

/**
 * Inicializa el módulo de reportes
 */
function inicializarReportes() {
    console.log('✓ Inicializando módulo Reportes');

    // Cachear selectores frecuentes
    SELECTORS_REPORTES = {
        // Parámetros generales
        fechaInicio: $('#fechaInicio'),
        fechaFin: $('#fechaFin'),
        selectAlmacen: $('#selectAlmacen'),
        selectAlmacenOrigen: $('#selectAlmacenOrigen'),
        selectAlmacenDestino: $('#selectAlmacenDestino'),
        selectCategoria: $('#selectCategoria'),
        selectArticulo: $('#selectArticulo'),
        selectTipoSalida: $('#selectTipoSalida'),
        selectEstatus: $('#selectEstatus'),
        inputStockMinimo: $('#inputStockMinimo'),
        buscarArticulo: $('#buscarArticulo'),
        listaArticulos: $('#listaArticulos'),

        // Nuevos parámetros para Entradas
        selectDonador: $('#selectDonador'),
        buscarArticuloOpcional: $('#buscarArticuloOpcional'),
        selectArticuloOpcional: $('#selectArticuloOpcional'),
        listaArticulosOpcional: $('#listaArticulosOpcional'),
        selectOrdenarPor: $('#selectOrdenarPor'),
        inputPagina: $('#inputPagina'),
        inputRegistrosPorPagina: $('#inputRegistrosPorPagina'),

        // Nuevos parámetros para Salidas
        selectBeneficiario: $('#selectBeneficiario'),

        // Nuevos parámetros para Existencias
        fechaCorte: $('#fechaCorte'),
        inputCantidadMinima: $('#inputCantidadMinima'),
        inputCantidadMaxima: $('#inputCantidadMaxima'),
        checkSoloSinExistencia: $('#checkSoloSinExistencia'),
        selectOrdenarPorExist: $('#selectOrdenarPorExist'),

        // Grupos de parámetros
        grupoFechas: $('#grupoFechas'),
        grupoAlmacen: $('#grupoAlmacen'),
        grupoAlmacenes: $('#grupoAlmacenes'),
        grupoCategoria: $('#grupoCategoria'),
        grupoArticulo: $('#grupoArticulo'),
        grupoTipoSalida: $('#grupoTipoSalida'),
        grupoEstatus: $('#grupoEstatus'),
        grupoStockMinimo: $('#grupoStockMinimo'),
        grupoDonador: $('#grupoDonador'),
        grupoArticuloOpcional: $('#grupoArticuloOpcional'),
        grupoTipoEntrada: $('#grupoTipoEntrada'),
        grupoEstatusEntrada: $('#grupoEstatusEntrada'),
        grupoAgrupacion: $('#grupoAgrupacion'),
        grupoOrdenamiento: $('#grupoOrdenamiento'),
        grupoPaginacion: $('#grupoPaginacion'),
        grupoBeneficiario: $('#grupoBeneficiario'),
        grupoEstatusSalida: $('#grupoEstatusSalida'),
        grupoAgrupacionSalidas: $('#grupoAgrupacionSalidas'),
        grupoAgrupacionTraspasos: $('#grupoAgrupacionTraspasos'),
        grupoFechaCorte: $('#grupoFechaCorte'),
        grupoRangoCantidades: $('#grupoRangoCantidades'),
        grupoSoloSinExistencia: $('#grupoSoloSinExistencia'),
        grupoAgrupacionExistencias: $('#grupoAgrupacionExistencias'),
        grupoOrdenamientoExistencias: $('#grupoOrdenamientoExistencias')
    };

    cargarCatalogosReportes();
    configurarEventosReportes();
    establecerFechasPorDefecto();
}

/**
 * Establece las fechas por defecto (último mes)
 */
function establecerFechasPorDefecto() {
    const hoy = new Date();
    const haceUnMes = new Date();
    haceUnMes.setMonth(haceUnMes.getMonth() - 1);

    const fechaInicio = haceUnMes.toISOString().split('T')[0];
    const fechaFin = hoy.toISOString().split('T')[0];

    SELECTORS_REPORTES.fechaInicio.val(fechaInicio);
    SELECTORS_REPORTES.fechaFin.val(fechaFin);
}

/**
 * Configura los eventos del módulo
 */
function configurarEventosReportes() {
    // Búsqueda de artículos para Kardex
    let timerBusquedaArticulo = null;
    SELECTORS_REPORTES.buscarArticulo.on('input', function() {
        clearTimeout(timerBusquedaArticulo);
        const termino = $(this).val().trim();

        if (termino.length >= 2) {
            timerBusquedaArticulo = setTimeout(() => {
                buscarArticulosReportes(termino, false);
            }, 500);
        } else {
            SELECTORS_REPORTES.listaArticulos.hide();
        }
    });

    // Búsqueda de artículos opcional (Entradas)
    let timerBusquedaArticuloOpcional = null;
    SELECTORS_REPORTES.buscarArticuloOpcional.on('input', function() {
        clearTimeout(timerBusquedaArticuloOpcional);
        const termino = $(this).val().trim();

        if (termino.length >= 2) {
            timerBusquedaArticuloOpcional = setTimeout(() => {
                buscarArticulosReportes(termino, true);
            }, 500);
        } else {
            SELECTORS_REPORTES.listaArticulosOpcional.hide();
        }
    });

    // Ocultar lista de artículos al hacer clic fuera
    $(document).on('click', function(e) {
        if (!$(e.target).closest('#buscarArticulo, #listaArticulos').length) {
            SELECTORS_REPORTES.listaArticulos.hide();
        }
        if (!$(e.target).closest('#buscarArticuloOpcional, #listaArticulosOpcional').length) {
            SELECTORS_REPORTES.listaArticulosOpcional.hide();
        }
    });
}

// ===================================================================
// CARGA DE CATÁLOGOS
// ===================================================================

/**
 * Carga todos los catálogos necesarios para reportes
 */
function cargarCatalogosReportes() {
    Promise.all([
        cargarAlmacenesReportes(),
        cargarCategoriasReportes(),
        cargarDonadoresReportes(),
        cargarBeneficiariosReportes()
    ])
    .then(() => {
        console.log('✓ Catálogos de reportes cargados correctamente');
    })
    .catch(error => {
        console.error('❌ Error al cargar catálogos:', error);
        notificaciones.error('Error al cargar catálogos', 'Error');
    });
}

/**
 * Carga la lista de almacenes
 */
function cargarAlmacenesReportes() {
    return AjaxService.almacenes.obtenerTodos()
        .then(response => {
            console.log('✓ Almacenes obtenidos:', response);
            const data = parseApiResponse(response);
            almacenesReportes = Array.isArray(data) ? data : [];
            llenarSelectsAlmacenes();
        })
        .catch(error => {
            console.error('❌ Error al cargar almacenes:', error);
            throw error;
        });
}

/**
 * Carga la lista de categorías
 */
function cargarCategoriasReportes() {
    return AjaxService.categorias.obtenerTodas()
        .then(response => {
            console.log('✓ Categorías obtenidas:', response);
            const data = parseApiResponse(response);
            categoriasReportes = Array.isArray(data) ? data : [];
            console.log(`length de array ${categoriasReportes.length}`)
            llenarSelectCategorias();
        })
        .catch(error => {
            console.error('❌ Error al cargar categorías:', error);
            throw error;
        });
}

/**
 * Carga la lista de donadores
 */
function cargarDonadoresReportes() {
    return AjaxService.donadores.obtenerTodos()
        .then(response => {
            console.log('✓ Donadores obtenidos:', response);
            const data = parseApiResponse(response);
            donadoresReportes = Array.isArray(data) ? data : [];
            llenarSelectDonadores();
        })
        .catch(error => {
            console.error('❌ Error al cargar donadores:', error);
            throw error;
        });
}

/**
 * Carga la lista de beneficiarios
 */
function cargarBeneficiariosReportes() {
    return AjaxService.beneficiarios.obtenerTodos()
        .then(response => {
            console.log('✓ Beneficiarios obtenidos:', response);
            const data = parseApiResponse(response);
            beneficiariosReportes = Array.isArray(data) ? data : [];
            llenarSelectBeneficiarios();
        })
        .catch(error => {
            console.error('❌ Error al cargar beneficiarios:', error);
            throw error;
        });
}

/**
 * Llena los selects de almacenes
 */
function llenarSelectsAlmacenes() {
    // Select único de almacén
    const $selectAlmacen = SELECTORS_REPORTES.selectAlmacen;
    $selectAlmacen.empty().append('<option value="">Todos los almacenes</option>');

    // Select de almacén origen
    const $selectOrigen = SELECTORS_REPORTES.selectAlmacenOrigen;
    $selectOrigen.empty().append('<option value="">Todos los almacenes</option>');

    // Select de almacén destino
    const $selectDestino = SELECTORS_REPORTES.selectAlmacenDestino;
    $selectDestino.empty().append('<option value="">Todos los almacenes</option>');

    almacenesReportes.forEach(almacen => {
        const id = almacen.id_almacen || almacen.idAlmacen || almacen.id;
        const nombre = almacen.nombre || `Almacén ${id}`;
        const option = `<option value="${id}">${nombre}</option>`;

        $selectAlmacen.append(option);
        $selectOrigen.append(option);
        $selectDestino.append(option);
    });

    console.log(`✓ Selects de almacenes llenados con ${almacenesReportes.length} opciones`);
}

/**
 * Llena el select de categorías
 */
function llenarSelectCategorias() {
    const $select = SELECTORS_REPORTES.selectCategoria;
    $select.empty().append('<option value="">Todas las categorías</option>');

    categoriasReportes.forEach(categoria => {
        const id = categoria.id_categoria || categoria.idCategoria || categoria.id;
        const nombre = categoria.nombre || `Categoría ${id}`;
        $select.append(`<option value="${id}">${nombre}</option>`);
    });

    console.log(`✓ Select de categorías llenado con ${categoriasReportes.length} opciones`);
}

/**
 * Llena el select de donadores
 */
function llenarSelectDonadores() {
    const $select = SELECTORS_REPORTES.selectDonador;
    $select.empty().append('<option value="">Todos los donadores</option>');

    donadoresReportes.forEach(donador => {
        const id = donador.id_donador || donador.idDonador || donador.id;
        const nombre = donador.nombre || `Donador ${id}`;
        $select.append(`<option value="${id}">${nombre}</option>`);
    });

    console.log(`✓ Select de donadores llenado con ${donadoresReportes.length} opciones`);
}

/**
 * Llena el select de beneficiarios
 */
function llenarSelectBeneficiarios() {
    const $select = SELECTORS_REPORTES.selectBeneficiario;
    $select.empty().append('<option value="">Todos los beneficiarios</option>');

    beneficiariosReportes.forEach(beneficiario => {
        const id = beneficiario.id_beneficiario || beneficiario.idBeneficiario || beneficiario.id;
        const nombre = beneficiario.nombre || `Beneficiario ${id}`;
        $select.append(`<option value="${id}">${nombre}</option>`);
    });

    console.log(`✓ Select de beneficiarios llenado con ${beneficiariosReportes.length} opciones`);
}

/**
 * Busca artículos para el reporte Kardex
 * @param {string} termino - Término de búsqueda
 * @param {boolean} esOpcional - Si es para el campo opcional (Entradas) o obligatorio (Kardex)
 */
function buscarArticulosReportes(termino, esOpcional = false) {
    console.log(`🔍 Buscando artículos: "${termino}" (Opcional: ${esOpcional})`);

    const $lista = esOpcional ? SELECTORS_REPORTES.listaArticulosOpcional : SELECTORS_REPORTES.listaArticulos;
    $lista.html('<div class="list-group-item text-center"><i class="fas fa-spinner fa-spin"></i> Buscando...</div>').show();

    AjaxService.articulos.buscar(termino)
        .then(response => {
            console.log('✓ Artículos encontrados:', response);
            const articulos = Array.isArray(response) ? response : [];
            mostrarListaArticulos(articulos, esOpcional);
        })
        .catch(error => {
            console.error('❌ Error al buscar artículos:', error);
            $lista.html('<div class="list-group-item text-danger">Error al buscar</div>');
        });
}

/**
 * Muestra la lista de artículos encontrados
 * @param {Array} articulos - Lista de artículos
 * @param {boolean} esOpcional - Si es para el campo opcional (Entradas) o obligatorio (Kardex)
 */
function mostrarListaArticulos(articulos, esOpcional = false) {
    const $lista = esOpcional ? SELECTORS_REPORTES.listaArticulosOpcional : SELECTORS_REPORTES.listaArticulos;
    $lista.empty();

    if (articulos.length === 0) {
        $lista.html('<div class="list-group-item text-warning">No se encontraron artículos</div>');
        setTimeout(() => $lista.hide(), 2000);
        return;
    }

    articulos.forEach(art => {
        const id = art.id_articulo || art.idArticulo || art.id;
        const nombre = art.nombre || 'Sin nombre';
        const categoria = art.nombre_categoria || art.nombreCategoria || '';

        $lista.append(`
            <button type="button" class="list-group-item list-group-item-action" onclick="seleccionarArticuloReporte(${id}, '${nombre.replace(/'/g, "\\'")}', ${esOpcional})">
                <div>
                    <strong>${nombre}</strong>
                    ${categoria ? `<span class="badge badge-secondary ml-2">${categoria}</span>` : ''}
                </div>
            </button>
        `);
    });

    $lista.show();
}

/**
 * Selecciona un artículo de la lista
 * @param {number} id - ID del artículo
 * @param {string} nombre - Nombre del artículo
 * @param {boolean} esOpcional - Si es para el campo opcional (Entradas) o obligatorio (Kardex)
 */
function seleccionarArticuloReporte(id, nombre, esOpcional = false) {
    console.log(`✓ Artículo seleccionado: ${nombre} (ID: ${id}), Opcional: ${esOpcional}`);

    if (esOpcional) {
        SELECTORS_REPORTES.selectArticuloOpcional.data('id-articulo', id);
        SELECTORS_REPORTES.buscarArticuloOpcional.val(nombre);
        SELECTORS_REPORTES.listaArticulosOpcional.hide();
    } else {
        SELECTORS_REPORTES.selectArticulo.data('id-articulo', id);
        SELECTORS_REPORTES.buscarArticulo.val(nombre);
        SELECTORS_REPORTES.listaArticulos.hide();
    }
}

// ===================================================================
// MODAL DE PARÁMETROS
// ===================================================================

/**
 * Abre el modal de parámetros según el tipo de reporte
 */
function abrirModalParametros(tipoReporte) {
    console.log(`📊 Abriendo modal para reporte: ${tipoReporte}`);

    // Resetear formulario
    $('#formParametrosReporte')[0].reset();
    establecerFechasPorDefecto();

    // Limpiar búsquedas de artículos
    SELECTORS_REPORTES.buscarArticulo.val('');
    SELECTORS_REPORTES.buscarArticuloOpcional.val('');
    SELECTORS_REPORTES.selectArticulo.removeData('id-articulo');
    SELECTORS_REPORTES.selectArticuloOpcional.removeData('id-articulo');

    // Resetear radio buttons a valores por defecto
    $('input[name="tipoEntrada"][value=""]').prop('checked', true);
    $('input[name="estatusEntrada"][value=""]').prop('checked', true);
    $('input[name="agruparPor"][value=""]').prop('checked', true);  // Sin agrupar por defecto
    $('input[name="orden"][value="DESC"]').prop('checked', true);

    // Radio buttons de Salidas
    $('input[name="tipoSalida"][value=""]').prop('checked', true);
    $('input[name="estatusSalida"][value=""]').prop('checked', true);
    $('input[name="agruparPorSalidas"][value=""]').prop('checked', true);

    // Radio buttons de Traspasos
    $('input[name="estatusTraspasos"][value=""]').prop('checked', true);
    $('input[name="agruparPorTraspasos"][value=""]').prop('checked', true);

    // Radio buttons de Existencias
    $('input[name="agruparPorExistencias"][value=""]').prop('checked', true); // Sin agrupar
    $('input[name="ordenExistencias"][value="DESC"]').prop('checked', true); // DESC por defecto
    $('#checkSoloSinExistencia').prop('checked', false); // Checkbox desmarcado

    // Ocultar todos los grupos de parámetros
    $('.grupo-parametro').hide();

    // Resetear configuración de almacén (puede ser requerido u opcional según el reporte)
    $('#selectAlmacen').removeAttr('required');
    $('#grupoAlmacen label').html('<i class="fas fa-warehouse"></i> Almacén');
    $('#grupoAlmacen small').text('Opcional: Filtra por un almacén específico');

    // Configurar título del modal
    let titulo = '';
    switch(tipoReporte) {
        case CONFIG_REPORTES.TIPOS.ENTRADAS:
            titulo = 'Reporte de Entradas';
            SELECTORS_REPORTES.grupoFechas.show();
            SELECTORS_REPORTES.grupoAlmacen.show();
            SELECTORS_REPORTES.grupoDonador.show();
            SELECTORS_REPORTES.grupoArticuloOpcional.show();
            SELECTORS_REPORTES.grupoCategoria.show();
            SELECTORS_REPORTES.grupoTipoEntrada.show();
            SELECTORS_REPORTES.grupoEstatusEntrada.show();
            SELECTORS_REPORTES.grupoAgrupacion.show();
            SELECTORS_REPORTES.grupoOrdenamiento.show();
            SELECTORS_REPORTES.grupoPaginacion.show();
            break;

        case CONFIG_REPORTES.TIPOS.SALIDAS:
            titulo = 'Reporte de Salidas';
            SELECTORS_REPORTES.grupoFechas.show();
            SELECTORS_REPORTES.grupoAlmacen.show();
            SELECTORS_REPORTES.grupoBeneficiario.show();
            SELECTORS_REPORTES.grupoArticuloOpcional.show();
            SELECTORS_REPORTES.grupoCategoria.show();
            SELECTORS_REPORTES.grupoTipoSalida.show();
            SELECTORS_REPORTES.grupoEstatusSalida.show();
            SELECTORS_REPORTES.grupoAgrupacionSalidas.show();
            SELECTORS_REPORTES.grupoPaginacion.show();
            break;

        case CONFIG_REPORTES.TIPOS.TRASPASOS:
            titulo = 'Reporte de Traspasos';
            SELECTORS_REPORTES.grupoFechas.show();
            SELECTORS_REPORTES.grupoAlmacenes.show();
            SELECTORS_REPORTES.grupoArticuloOpcional.show();
            SELECTORS_REPORTES.grupoCategoria.show();
            SELECTORS_REPORTES.grupoEstatus.show();
            SELECTORS_REPORTES.grupoAgrupacionTraspasos.show();
            SELECTORS_REPORTES.grupoPaginacion.show();
            break;

        case CONFIG_REPORTES.TIPOS.EXISTENCIAS:
            titulo = 'Reporte de Existencias';
            SELECTORS_REPORTES.grupoFechaCorte.show();
            SELECTORS_REPORTES.grupoAlmacen.show();
            SELECTORS_REPORTES.grupoArticuloOpcional.show();
            SELECTORS_REPORTES.grupoCategoria.show();
            SELECTORS_REPORTES.grupoRangoCantidades.show();
            SELECTORS_REPORTES.grupoSoloSinExistencia.show();
            SELECTORS_REPORTES.grupoAgrupacionExistencias.show();
            SELECTORS_REPORTES.grupoOrdenamientoExistencias.show();
            SELECTORS_REPORTES.grupoPaginacion.show();
            break;

        case CONFIG_REPORTES.TIPOS.KARDEX:
            titulo = 'Reporte Kardex de Inventario';
            SELECTORS_REPORTES.grupoFechas.show();
            SELECTORS_REPORTES.grupoArticulo.show();
            SELECTORS_REPORTES.grupoAlmacen.show();

            // Kardex requiere almacén obligatorio (modificar UI dinámicamente)
            $('#selectAlmacen').attr('required', true);
            $('#grupoAlmacen label').html('<i class="fas fa-warehouse"></i> Almacén <span class="text-danger">*</span>');
            $('#grupoAlmacen small').text('Requerido: Selecciona el almacén para el Kardex');
            break;

        default:
            console.error('❌ Tipo de reporte desconocido:', tipoReporte);
            return;
    }

    $('#tituloModalParametros').text(titulo);
    $('#modalParametrosReporte').data('tipo-reporte', tipoReporte);
    $('#modalParametrosReporte').modal('show');
}

// ===================================================================
// GENERACIÓN DE REPORTES
// ===================================================================

/**
 * Genera el reporte seleccionado
 */
function generarReporte() {
    const tipoReporte = $('#modalParametrosReporte').data('tipo-reporte');
    console.log(`📊 Generando reporte: ${tipoReporte}`);

    // Validar parámetros según el tipo de reporte
    const parametros = obtenerParametrosReporte(tipoReporte);

    if (!parametros) {
        notificaciones.warning('Completa los parámetros requeridos', 'Validación');
        return;
    }

    // Mostrar loading
    const $btnGenerar = $('#btnGenerarReporte');
    const textoOriginal = $btnGenerar.html();
    $btnGenerar.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Generando...');

    // Llamar al servicio correspondiente
    let promesa = null;
    switch(tipoReporte) {
        case CONFIG_REPORTES.TIPOS.ENTRADAS:
            promesa = AjaxService.reportes.generarEntradas(parametros);
            break;
        case CONFIG_REPORTES.TIPOS.SALIDAS:
            promesa = AjaxService.reportes.generarSalidas(parametros);
            break;
        case CONFIG_REPORTES.TIPOS.TRASPASOS:
            promesa = AjaxService.reportes.generarTraspasos(parametros);
            break;
        case CONFIG_REPORTES.TIPOS.EXISTENCIAS:
            promesa = AjaxService.reportes.generarExistencias(parametros);
            break;
        case CONFIG_REPORTES.TIPOS.KARDEX:
            promesa = AjaxService.reportes.generarKardex(parametros);
            break;
        default:
            notificaciones.error('Tipo de reporte inválido', 'Error');
            $btnGenerar.prop('disabled', false).html(textoOriginal);
            return;
    }

    promesa
        .done(function(blob) {
            console.log('✓ Reporte generado correctamente');

            // Construir nombre del archivo
            const nombreArchivo = construirNombreArchivo(tipoReporte);

            // Descargar archivo
            descargarArchivo(blob, nombreArchivo);

            notificaciones.success('Reporte generado correctamente', 'Éxito');
            $('#modalParametrosReporte').modal('hide');
        })
        .fail(function(xhr, status, error) {
            console.error('❌ Error al generar reporte:', error);
            console.error('   Status:', status);
            console.error('   XHR:', xhr);

            // Intentar extraer mensaje de error del blob
            if (xhr.responseText) {
                try {
                    const errorObj = JSON.parse(xhr.responseText);
                    const mensaje = errorObj.mensaje || errorObj.error || 'Error al generar el reporte';
                    notificaciones.error(mensaje, 'Error');
                } catch (e) {
                    notificaciones.error('Error al generar el reporte', 'Error');
                }
            } else {
                notificaciones.error('Error al generar el reporte', 'Error');
            }
        })
        .always(function() {
            $btnGenerar.prop('disabled', false).html(textoOriginal);
        });
}

/**
 * Obtiene los parámetros del reporte según el tipo
 */
function obtenerParametrosReporte(tipoReporte) {
    const parametros = {};

    switch(tipoReporte) {
        case CONFIG_REPORTES.TIPOS.ENTRADAS:
            parametros.fecha_Inicio = SELECTORS_REPORTES.fechaInicio.val();
            parametros.fecha_Fin = SELECTORS_REPORTES.fechaFin.val();

            if (!parametros.fecha_Inicio || !parametros.fecha_Fin) {
                notificaciones.warning('Las fechas son requeridas', 'Validación');
                return null;
            }

            // Parámetros opcionales
            const idAlmacen = SELECTORS_REPORTES.selectAlmacen.val();
            const idDonador = SELECTORS_REPORTES.selectDonador.val();
            const idArticuloOpc = SELECTORS_REPORTES.selectArticuloOpcional.data('id-articulo');
            const idCategoria = SELECTORS_REPORTES.selectCategoria.val();

            // Leer valores de radio buttons
            const tipoEntrada = $('input[name="tipoEntrada"]:checked').val();
            const estatusEntrada = $('input[name="estatusEntrada"]:checked').val();
            const agruparPor = $('input[name="agruparPor"]:checked').val();
            const ordenarPor = SELECTORS_REPORTES.selectOrdenarPor.val();
            const orden = $('input[name="orden"]:checked').val();

            const pagina = SELECTORS_REPORTES.inputPagina.val();
            const registrosPorPagina = SELECTORS_REPORTES.inputRegistrosPorPagina.val();

            if (idAlmacen) parametros.id_Almacen = parseInt(idAlmacen);
            if (idDonador) parametros.id_donador = parseInt(idDonador);
            if (idArticuloOpc) parametros.id_articulo = parseInt(idArticuloOpc);
            if (idCategoria) parametros.id_categoria = parseInt(idCategoria);
            if (tipoEntrada) parametros.tipo = tipoEntrada;
            if (estatusEntrada) parametros.estatus = estatusEntrada;
            if (agruparPor) parametros.agrupar_por = agruparPor; // Solo enviar si tiene valor (no vacío)
            if (ordenarPor) parametros.ordenar_Por = ordenarPor;
            if (orden) parametros.orden = orden;
            if (pagina) parametros.pagina = parseInt(pagina);
            if (registrosPorPagina) parametros.registros_Por_Pagina = parseInt(registrosPorPagina);

            // Valores por defecto si no se especifican
            // NOTA: agrupar_por ya NO tiene valor por defecto (Sin agrupar es no enviar el parámetro)
            if (!parametros.ordenar_Por) parametros.ordenar_Por = 'fecha_recepcion';
            if (!parametros.orden) parametros.orden = 'DESC';
            if (!parametros.pagina) parametros.pagina = 1;
            if (!parametros.registros_Por_Pagina) parametros.registros_Por_Pagina = 50;

            break;

        case CONFIG_REPORTES.TIPOS.SALIDAS:
            parametros.fecha_Inicio = SELECTORS_REPORTES.fechaInicio.val();
            parametros.fecha_Fin = SELECTORS_REPORTES.fechaFin.val();

            if (!parametros.fecha_Inicio || !parametros.fecha_Fin) {
                notificaciones.warning('Las fechas son requeridas', 'Validación');
                return null;
            }

            // Parámetros opcionales
            const idAlmacenSalida = SELECTORS_REPORTES.selectAlmacen.val();
            const idBeneficiario = SELECTORS_REPORTES.selectBeneficiario.val();
            const idArticuloSalida = SELECTORS_REPORTES.selectArticuloOpcional.data('id-articulo');
            const idCategoriaSalida = SELECTORS_REPORTES.selectCategoria.val();

            // Leer valores de radio buttons
            const tipoSalida = $('input[name="tipoSalida"]:checked').val();
            const estatusSalida = $('input[name="estatusSalida"]:checked').val();
            const agruparPorSalidas = $('input[name="agruparPorSalidas"]:checked').val();

            const paginaSalida = SELECTORS_REPORTES.inputPagina.val();
            const registrosPorPaginaSalida = SELECTORS_REPORTES.inputRegistrosPorPagina.val();

            if (idAlmacenSalida) parametros.id_Almacen = parseInt(idAlmacenSalida);
            if (idBeneficiario) parametros.id_beneficiario = parseInt(idBeneficiario);
            if (idArticuloSalida) parametros.id_articulo = parseInt(idArticuloSalida);
            if (idCategoriaSalida) parametros.id_categoria = parseInt(idCategoriaSalida);
            if (tipoSalida) parametros.tipo_Salida = tipoSalida;
            if (estatusSalida) parametros.estatus = estatusSalida;
            if (agruparPorSalidas) parametros.agrupar_por = agruparPorSalidas;
            if (paginaSalida) parametros.pagina = parseInt(paginaSalida);
            if (registrosPorPaginaSalida) parametros.registros_Por_Pagina = parseInt(registrosPorPaginaSalida);

            // Valores por defecto
            if (!parametros.pagina) parametros.pagina = 1;
            if (!parametros.registros_Por_Pagina) parametros.registros_Por_Pagina = 50;

            break;

        case CONFIG_REPORTES.TIPOS.TRASPASOS:
            parametros.fecha_inicio = SELECTORS_REPORTES.fechaInicio.val();
            parametros.fecha_fin = SELECTORS_REPORTES.fechaFin.val();

            if (!parametros.fecha_inicio || !parametros.fecha_fin) {
                notificaciones.warning('Las fechas son requeridas', 'Validación');
                return null;
            }

            // Parámetros opcionales
            const idAlmacenOrigen = SELECTORS_REPORTES.selectAlmacenOrigen.val();
            const idAlmacenDestino = SELECTORS_REPORTES.selectAlmacenDestino.val();
            const idArticuloTrasp = SELECTORS_REPORTES.selectArticuloOpcional.data('id-articulo');
            const idCategoriaTrasp = SELECTORS_REPORTES.selectCategoria.val();

            // Leer valores de radio buttons
            const estatusTraspasos = $('input[name="estatusTraspasos"]:checked').val();
            const agruparPorTraspasos = $('input[name="agruparPorTraspasos"]:checked').val();

            const paginaTrasp = SELECTORS_REPORTES.inputPagina.val();
            const registrosPorPaginaTrasp = SELECTORS_REPORTES.inputRegistrosPorPagina.val();

            if (idAlmacenOrigen) parametros.id_almacen_origen = parseInt(idAlmacenOrigen);
            if (idAlmacenDestino) parametros.id_almacen_destino = parseInt(idAlmacenDestino);
            if (idArticuloTrasp) parametros.id_articulo = parseInt(idArticuloTrasp);
            if (idCategoriaTrasp) parametros.id_categoria = parseInt(idCategoriaTrasp);
            if (estatusTraspasos) parametros.estatus = estatusTraspasos;
            if (agruparPorTraspasos) parametros.agrupar_por = agruparPorTraspasos;
            if (paginaTrasp) parametros.pagina = parseInt(paginaTrasp);
            if (registrosPorPaginaTrasp) parametros.registros_Por_Pagina = parseInt(registrosPorPaginaTrasp);

            // Valores por defecto
            if (!parametros.pagina) parametros.pagina = 1;
            if (!parametros.registros_Por_Pagina) parametros.registros_Por_Pagina = 50;

            break;

        case CONFIG_REPORTES.TIPOS.EXISTENCIAS:
            // NOTA: Todos los parámetros son opcionales en Existencias
            const fechaCorte = SELECTORS_REPORTES.fechaCorte.val();
            const idAlmacenExist = SELECTORS_REPORTES.selectAlmacen.val();
            const idArticuloExist = SELECTORS_REPORTES.selectArticuloOpcional.data('id-articulo');
            const idCategoriaExist = SELECTORS_REPORTES.selectCategoria.val();
            const cantidadMinima = SELECTORS_REPORTES.inputCantidadMinima.val();
            const cantidadMaxima = SELECTORS_REPORTES.inputCantidadMaxima.val();
            const soloSinExistencia = SELECTORS_REPORTES.checkSoloSinExistencia.is(':checked');

            // Leer valores de radio buttons
            const agruparPorExist = $('input[name="agruparPorExistencias"]:checked').val();
            const ordenarPorExist = SELECTORS_REPORTES.selectOrdenarPorExist.val();
            const ordenExist = $('input[name="ordenExistencias"]:checked').val();

            const paginaExist = SELECTORS_REPORTES.inputPagina.val();
            const registrosPorPaginaExist = SELECTORS_REPORTES.inputRegistrosPorPagina.val();

            // Construir JSON con todos los parámetros opcionales
            if (fechaCorte) parametros.fecha_corte = fechaCorte;
            if (idAlmacenExist) parametros.id_almacen = parseInt(idAlmacenExist);
            if (idArticuloExist) parametros.id_articulo = parseInt(idArticuloExist);
            if (idCategoriaExist) parametros.id_categoria = parseInt(idCategoriaExist);
            if (cantidadMinima) parametros.cantidad_minima = parseInt(cantidadMinima);
            if (cantidadMaxima) parametros.cantidad_maxima = parseInt(cantidadMaxima);
            parametros.solo_sin_existencia = soloSinExistencia; // Siempre enviar (boolean)
            if (agruparPorExist) parametros.agrupar_por = agruparPorExist;
            if (ordenarPorExist) parametros.ordenar_por = ordenarPorExist;
            if (ordenExist) parametros.orden = ordenExist;
            if (paginaExist) parametros.pagina = parseInt(paginaExist);
            if (registrosPorPaginaExist) parametros.registros_por_pagina = parseInt(registrosPorPaginaExist);

            // Valores por defecto
            if (!parametros.pagina) parametros.pagina = 1;
            if (!parametros.registros_por_pagina) parametros.registros_por_pagina = 50;

            break;

        case CONFIG_REPORTES.TIPOS.KARDEX:
            // NOTA: Kardex requiere fechas (capitalizadas) y artículo obligatorio
            parametros.fecha_Inicio = SELECTORS_REPORTES.fechaInicio.val();
            parametros.fecha_Fin = SELECTORS_REPORTES.fechaFin.val();

            if (!parametros.fecha_Inicio || !parametros.fecha_Fin) {
                notificaciones.warning('Las fechas son requeridas', 'Validación');
                return null;
            }

            const idArticuloKardex = SELECTORS_REPORTES.selectArticulo.data('id-articulo');
            if (!idArticuloKardex) {
                notificaciones.warning('Debes seleccionar un artículo para generar el Kardex', 'Validación');
                return null;
            }

            parametros.id_Articulo = parseInt(idArticuloKardex);

            // Almacén obligatorio en Kardex
            const idAlmacenKardex = SELECTORS_REPORTES.selectAlmacen.val();
            if (!idAlmacenKardex) {
                notificaciones.warning('Debes seleccionar un almacén para generar el Kardex', 'Validación');
                return null;
            }
            parametros.id_Almacen = parseInt(idAlmacenKardex);
            break;

        default:
            console.error('❌ Tipo de reporte desconocido:', tipoReporte);
            return null;
    }

    console.log('📋 Parámetros construidos:', parametros);
    return parametros;
}

/**
 * Construye el nombre del archivo Excel
 */
function construirNombreArchivo(tipoReporte) {
    const fecha = new Date().toISOString().split('T')[0]; // YYYY-MM-DD
    let nombre = '';

    switch(tipoReporte) {
        case CONFIG_REPORTES.TIPOS.ENTRADAS:
            nombre = `Reporte_Entradas_${fecha}.xlsx`;
            break;
        case CONFIG_REPORTES.TIPOS.SALIDAS:
            nombre = `Reporte_Salidas_${fecha}.xlsx`;
            break;
        case CONFIG_REPORTES.TIPOS.TRASPASOS:
            nombre = `Reporte_Traspasos_${fecha}.xlsx`;
            break;
        case CONFIG_REPORTES.TIPOS.EXISTENCIAS:
            nombre = `Reporte_Existencias_${fecha}.xlsx`;
            break;
        case CONFIG_REPORTES.TIPOS.KARDEX:
            nombre = `Reporte_Kardex_${fecha}.xlsx`;
            break;
        default:
            nombre = `Reporte_${fecha}.xlsx`;
    }

    return nombre;
}

/**
 * Descarga un archivo blob
 * @param {Blob} blob - Blob del archivo
 * @param {string} nombreArchivo - Nombre del archivo a descargar
 */
function descargarArchivo(blob, nombreArchivo) {
    console.log(`📥 Descargando archivo: ${nombreArchivo}`);

    // Crear URL del blob
    const url = window.URL.createObjectURL(blob);

    // Crear elemento <a> temporal
    const link = document.createElement('a');
    link.href = url;
    link.download = nombreArchivo;

    // Agregar al DOM, hacer clic y remover
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    // Liberar la URL del blob
    setTimeout(() => {
        window.URL.revokeObjectURL(url);
    }, 100);

    console.log('✓ Archivo descargado correctamente');
}

// ===================================================================
// EXPORTAR FUNCIONES GLOBALES
// ===================================================================
window.inicializarReportes = inicializarReportes;
window.abrirModalParametros = abrirModalParametros;
window.generarReporte = generarReporte;
window.seleccionarArticuloReporte = seleccionarArticuloReporte;

// ===================================================================
// LOG DE CARGA
// ===================================================================
console.log('✓ reportes.js cargado completamente');
console.log('✓ Funciones exportadas:', {
    inicializarReportes: typeof inicializarReportes,
    abrirModalParametros: typeof abrirModalParametros,
    generarReporte: typeof generarReporte
});
