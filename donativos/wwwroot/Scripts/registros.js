// Variable global para contar artículos
let numeroArticulos = 0;
let donadorSeleccionado = null;
let timerOcultarListaDonadores = null;
let almacenes = [];
let archivosAdjuntos = [];
let modoEdicion = false; // Nueva variable para controlar modo edición
let idDonativoEdicion = null; // Nueva variable para guardar ID del donativo en edición

// Estados disponibles para artículos
const ESTADOS_ARTICULO = [
    { value: 'nuevo', label: 'Nuevo' },
    { value: 'usado_bueno', label: 'Usado - Buen estado' },
    { value: 'usado_regular', label: 'Usado - Estado regular' },
    { value: 'reparado', label: 'Reparado' },
    { value: 'otro', label: 'Otro' }
];

// Tipos de donativo disponibles
const TIPOS_DONATIVO = [
    { value: 'especie', label: 'Especie' },
    { value: 'efectivo', label: 'Efectivo' }
];

// Categorías disponibles
const CATEGORIAS = [
    { id: 1, nombre: 'AGUA' },
    { id: 2, nombre: 'ALIMENTOS ENLATADOS' },
    { id: 3, nombre: 'ARTÍCULOS DE HIGIENE PERSONAL Y SANEAMIENTO' },
    { id: 4, nombre: 'ARTÍCULOS DE LIMPIEZA Y DESINFECCIÓN' },
    { id: 5, nombre: 'ARTÍCULOS DESECHABLES EN GENERAL' },
    { id: 6, nombre: 'ARTÍCULOS PARA BEBÉ' },
    { id: 7, nombre: 'ARTÍCULOS PARA COCINA' },
    { id: 8, nombre: 'ARTÍCULOS Y ALIMENTO PARA MASCOTAS' },
    { id: 9, nombre: 'COLCHONETAS, COLCHONES, COBIJAS, ROPA DE CAMA EN GENERAL' },
    { id: 10, nombre: 'DESPENSA ALIMENTARIA' },
    { id: 11, nombre: 'FRUTAS Y VERDURAS FRESCAS' },
    { id: 12, nombre: 'INSUMOS MÉDICOS' },
    { id: 13, nombre: 'MAQUINARIA, HERRAMIENTAS Y EQUIPO DE SEGURIDAD' },
    { id: 14, nombre: 'MEDICAMENTO' },
    { id: 15, nombre: 'MEDICAMENTOS E INSUMO MEDICO' },
    { id: 16, nombre: 'OTROS' },
    { id: 17, nombre: 'PRODUCTO TERMINADO EN KIT' },
    { id: 18, nombre: 'PRODUCTOS MISCELÁNEOS Y OTROS ARTÍCULOS' },
    { id: 19, nombre: 'ROPA, CALZAZO Y TEXTILES' },
    { id: 20, nombre: 'SEMILLAS, GRANOS Y ESPECIAS' },
    { id: 21, nombre: 'SUEROS, HIDRATANTES Y ENERGIZANTES' },
    { id: 22, nombre: 'ALIMENTOS PREPARADOS' }
];

// ==================== DATATABLES ====================

let dataTable = null;

/**
 * Inicializa DataTables
 */
function inicializarDataTables() {
    if (dataTable) {
        dataTable.destroy();
    }

    dataTable = $('#tablaDonativos').DataTable({
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
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>><"row"<"col-sm-12"tr>><"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        columnDefs: [
            {
                targets: -1, // Última columna (Acciones)
                orderable: false,
                searchable: false
            }
        ],
        initComplete: function () {
            // console.log('✓ DataTables inicializado correctamente');
        }
    });
}

/**
 * Carga todos los donativos del servidor y actualiza DataTables
 */
function cargarDonativos() {
    // console.log('=== Iniciando cargarDonativos ===');

    const tabla = document.getElementById('tablaDonativos');

    if (!tabla) {
        console.error('❌ Tabla #tablaDonativos no encontrada en el DOM');
        return;
    }

    $.ajax({
        type: 'GET',
        url: 'ObtenerDonativos',
        dataType: 'json',
        timeout: 10000,
        beforeSend: function () {
            if (dataTable) {
                dataTable.clear().draw();
            }
            toastr.info('Cargando donativos...', 'Procesando');
        },
        success: function (response) {
            // console.log('=== AJAX Success ===');
            // console.log('Donativos recibidos:', response);

            let donativosArray = [];

            // Extraer array de la respuesta
            if (Array.isArray(response)) {
                donativosArray = response;
            } else if (response && response.data && Array.isArray(response.data)) {
                donativosArray = response.data;
            } else if (response && response.donativos && Array.isArray(response.donativos)) {
                donativosArray = response.donativos;
            }

            if (donativosArray.length > 0) {
                // console.log(`✓ Se cargaron ${donativosArray.length} donativos`);
                llenarTablaConDataTables(donativosArray);
                toastr.success(`Se cargaron ${donativosArray.length} donativos`, 'Éxito');
            } else {
                // console.log('⚠ No hay donativos');
                if (dataTable) {
                    dataTable.clear().draw();
                }
                toastr.info('No hay donativos registrados', 'Información');
            }
        },
        error: function (xhr, status, error) {
            console.error('=== AJAX Error ===');
            console.error('Error:', error);

            toastr.error('Error al cargar los donativos', 'Error');
        }
    });
}

/**
 * Llena DataTables con los donativos
 */
function llenarTablaConDataTables(donativos) {
    // console.log('Llenando DataTables con', donativos.length, 'registros');

    if (!dataTable) {
        inicializarDataTables();
    }

    // Limpiar la tabla
    dataTable.clear();

    // Agregar filas
    donativos.forEach((donativo) => {
        const fechaFormato = donativo.fecha_recepcion
            ? new Date(donativo.fecha_recepcion).toLocaleDateString('es-MX')
            : 'N/A';
        const cantidadArticulos = donativo.cantidad_articulos || 0;

        const botones = `
            <button class="btn btn-sm btn-info" onclick="verDetalles(${donativo.id_donativo})" title="Ver detalles">
                <i class="fas fa-eye"></i>
            </button>
            <button class="btn btn-sm btn-danger" onclick="eliminarDonativo(${donativo.id_donativo})" title="Eliminar">
                <i class="fas fa-trash"></i>
            </button>
            <button class="btn btn-sm btn-primary" onclick="editarDonativo(${donativo.id_donativo})" title="Editar">
                <i class="fas fa-edit"></i>
            </button>
        `;

        dataTable.row.add([
            donativo.id_donativo || '-',
            `<strong>${donativo.folio || '-'}</strong>`,
            donativo.nombre || 'N/A',
            fechaFormato,
            `<span class="badge badge-pill ${obtenerColorEstatus(donativo.tipo)}">${donativo.tipo || 'N/A'}</span>`,
            `<span class="badge badge-pill ${obtenerColorEstatus(donativo.estatus)}">${donativo.estatus || 'Pendiente'}</span>`,
            `<span class="badge badge-pill badge-info">${cantidadArticulos}</span>`,
            botones
        ]);
    });

    // Redibujar la tabla
    dataTable.draw();
    // console.log('✓ DataTables actualizado correctamente with', donativos.length, 'registros');
}

/**
 * Limpia el formulario después de guardar
 */
function limpiarFormulario() {
    const form = document.getElementById('formRegistroDonativos');

    console.log('Limpiando formulario...');

    // Salir del modo edición si está activo
    if (modoEdicion) {
        salirModoEdicion();
    }

    // Limpiar el formulario
    form.reset();
    form.classList.remove('was-validated');

    // Resetear el input hidden del ID
    $('#id_donador').val('');
    $('#textNombreDonador').css('background-color', '');

    // Establecer fecha de recepción a la fecha de hoy
    $('#dateRecepcion').val(obtenerFechaHoy());

    // Limpiar sección de contacto
    $('#seccionContacto').addClass('hidden');
    $('#textNombreContacto').prop('required', false);
    $('#textPuestoContacto').prop('required', false);

     // Limpiar artículos
    $('#contenedorArticulos').empty();
    numeroArticulos = 0;
    donadorSeleccionado = null;

    // Limpiar archivos adjuntos
    $('#fileAdjuntos').val('');
    archivosAdjuntos = [];
    const fileAlert = document.querySelector('#fileAdjuntos').parentElement.querySelector('.alert');
    if (fileAlert) {
        fileAlert.remove();
    }

    // Ocultar lista de donadores
    $('#listaDonadores').addClass('d-none');

    // Limpiar el select de almacenes y volver a cargar
    if (centroIDUsuario && centroIDUsuario !== '' && centroIDUsuario !== '0') {
        $('#selectAlmacenDestino').val(centroIDUsuario.value);
        console.log(`✓ Almacén seleccionado por defecto: ${centroIDUsuario}`);
    }

    // Desplazar al inicio del formulario
    form.scrollIntoView({ behavior: 'smooth', block: 'start' });

    // Refrescar tabla de donativos después de un pequeño retraso
    setTimeout(() => {
        cargarDonativos();
    }, 500);

    console.log('✓ Formulario limpiado correctamente');
}

/**
 * Obtiene el color del badge según el estatus
 */
function obtenerColorEstatus(estatus) {
    // Usar clases estándar de Bootstrap con nombres correctos
    const colores = {
        'Recibido': 'bg-success',
        'recibido': 'bg-success',
        'Pendiente': 'bg-warning',
        'pendiente': 'bg-warning',
        'Cancelado': 'bg-danger',
        'cancelado': 'bg-danger',
        'Rechazado': 'bg-dark',
        'rechazado': 'bg-dark'
    };
    return colores[estatus] || 'bg-secondary';
}

/**
 * Evento personalizado cuando la vista de registros se carga
 */
document.addEventListener('registrosViewLoaded', function () {
    // console.log('=== registrosViewLoaded evento disparado ===');

    setTimeout(function () {
        // console.log('Iniciando cargarDonativos después de registrosViewLoaded');

        // Inicializar DataTables primero
        inicializarDataTables();

        // Luego cargar los datos
        cargarDonativos();

        // Botón refrescar
        const btnRefrescar = document.getElementById('btnRefrescarDonativos');
        if (btnRefrescar) {
            // console.log('✓ Botón refrescar encontrado, agregando listener');
            btnRefrescar.addEventListener('click', function () {
                // console.log('Botón refrescar clickeado');
                cargarDonativos();
            });
        } else {
            console.warn('⚠ Botón refrescar NO encontrado');
        }
    }, 200);
});

// Inicialización para cuando se carga el DOM por primera vez
document.addEventListener('DOMContentLoaded', function () {
    // console.log('=== DOMContentLoaded ===');

    // Si acceden directamente a la página de registros
    const tabla = document.getElementById('tablaDonativos');
    if (tabla) {
        // console.log('Tabla encontrada en carga inicial, disparando evento');
        document.dispatchEvent(new Event('registrosViewLoaded'));
    }
});

// ==================== FIN DATATABLES ====================

/**
 * Carga los almacenes disponibles desde el servidor
 */
function cargarAlmacenes() {
    $.ajax({
        type: 'GET',
        url: 'ObtenerAlmacenes',
        dataType: 'json',
        success: function (response) {
            // console.log('Almacenes recibidos:', response);
            
            // Procesar la respuesta para extraer el array
            let almacenesArray = [];
            
            if (Array.isArray(JSON.parse(response))) {
                almacenesArray = JSON.parse(response);
            } else if (response && response.data && Array.isArray(response.data)) {
                almacenesArray = response.data;
            } else if (response && response.almacenes && Array.isArray(response.almacenes)) {
                almacenesArray = response.almacenes;
            }
            
            // Guardar en variable global
            almacenes = almacenesArray;
            
            // Llenar el select
            llenarSelectAlmacenes(almacenesArray);
        },
        error: function (xhr, status, error) {
            console.error('Error al cargar almacenes:', error);
            console.error('Respuesta:', xhr.responseText);
            toastr.error('Error al cargar los almacenes', 'Error');
        }
    });
}

/**
 * Llena el select de almacenes con los datos obtenidos
 */
function llenarSelectAlmacenes(almacenesArray) {
    const selectAlmacen = $('#selectAlmacenDestino');
    
    if (!selectAlmacen || selectAlmacen.length === 0) {
        console.warn('⚠ Select #selectAlmacenDestino no encontrado');
        return;
    }
    
    // Limpiar opciones previas (excepto la primera que es el placeholder)
    selectAlmacen.find('option:not(:first)').remove();
    
    if (!almacenesArray || almacenesArray.length === 0) {
        console.warn('⚠ No hay almacenes disponibles');
        toastr.warning('No hay almacenes disponibles', 'Información');
        return;
    }
    
    // Obtener el Centro ID del usuario desde la sesión
    const centroIDUsuario = $('#centroIDUsuario').val();
    // console.log('Centro ID del usuario:', centroIDUsuario);
    
    // Agregar las opciones de almacenes
    almacenesArray.forEach((almacen) => {
        // Manejar diferentes estructuras posibles
        const idAlmacen = almacen.id_almacen || almacen.id;
        const nombreAlmacen = almacen.nombre || almacen.name || 'Almacén sin nombre';
        
        if (idAlmacen) {
            selectAlmacen.append(`<option value="${idAlmacen}">${nombreAlmacen}</option>`);
        }
    });
    
    // Seleccionar el almacén por defecto si existe
    if (centroIDUsuario && centroIDUsuario !== '' && centroIDUsuario !== '0') {
        selectAlmacen.val(centroIDUsuario);
        // console.log(`✓ Almacén seleccionado por defecto: ${centroIDUsuario}`);
    }
    
    // console.log(`✓ Select de almacenes llenado con ${almacenesArray.length} opciones`);
}

// ==================== FIN ALMACENES ====================

/**
 * Maneja los archivos adjuntos seleccionados
 * Valida tipo y tamaño de archivos
 */
function manejarArchivosAdjuntos(e) {
    const files = e.target.files;
    const maxFileSize = 5 * 1024 * 1024; // 5 MB por archivo
    const allowedExtensions = ['jpg', 'jpeg', 'png', 'gif', 'pdf', 'doc', 'docx', 'xls', 'xlsx'];
    
    archivosAdjuntos = [];
    
    if (!files || files.length === 0) {
        // console.log('No se seleccionaron archivos');
        return;
    }
    
    // console.log(`Procesando ${files.length} archivo(s)`);
    
    // Validar cada archivo
    for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const fileExtension = file.name.split('.').pop().toLowerCase();
        
        // Validar extensión
        if (!allowedExtensions.includes(fileExtension)) {
            toastr.warning(`Archivo "${file.name}" no permitido. Formato: ${fileExtension}`, 'Tipo de archivo no permitido');
            continue;
        }
        
        // Validar tamaño
        if (file.size > maxFileSize) {
            const sizeMB = (file.size / (1024 * 1024)).toFixed(2);
            toastr.warning(`Archivo "${file.name}" excede el límite de 5MB (${sizeMB}MB)`, 'Archivo demasiado grande');
            continue;
        }
        
        // Agregar archivo válido a la lista
        archivosAdjuntos.push({
            file: file,
            nombre: file.name,
            tamaño: (file.size / 1024).toFixed(2) + ' KB',
            tipo: file.type
        });
    }
    
    if (archivosAdjuntos.length > 0) {
       // console.log(`✓ ${archivosAdjuntos.length} archivo(s) válido(s) seleccionado(s)`);
        mostrarResumenArchivos();
        toastr.success(`${archivosAdjuntos.length} archivo(s) seleccionado(s) correctamente`, 'Éxito');
    } else {
        console.warn('⚠ No hay archivos válidos para procesar');
        toastr.error('No hay archivos válidos para adjuntar', 'Error');
        archivosAdjuntos = [];
    }
}

/**
 * Muestra un resumen de los archivos adjuntos seleccionados
 */
function mostrarResumenArchivos() {
    if (archivosAdjuntos.length === 0) {
        return;
    }
    
    let resumenHTML = '<div class="alert alert-info mt-3" role="alert">';
    resumenHTML += '<i class="fas fa-info-circle"></i> <strong>Archivos seleccionados:</strong><br>';
    resumenHTML += '<ul class="mb-0 mt-2">';
    
    archivosAdjuntos.forEach((archivo, index) => {
        resumenHTML += `<li>${index + 1}. ${archivo.nombre} (${archivo.tamaño})</li>`;
    });
    
    resumenHTML += '</ul></div>';
    
    // Insertar después del input de archivo
    const fileInput = document.getElementById('fileAdjuntos');
    const existingAlert = fileInput.parentElement.querySelector('.alert');
    
    if (existingAlert) {
        existingAlert.remove();
    }
    
    fileInput.parentElement.insertAdjacentHTML('beforeend', resumenHTML);
}
function obtenerFechaHoy() {
    const hoy = new Date();
    const año = hoy.getFullYear();
    const mes = String(hoy.getMonth() + 1).padStart(2, '0');
    const día = String(hoy.getDate()).padStart(2, '0');
    return `${año}-${mes}-${día}`;
}

/**
 * Inicializa el formulario de registro de donativos
 * Se ejecuta cada vez que la vista parcial se carga
 */
function initRegistrosForm() {
    // console.log('Inicializando formulario de registros...');

    // Validar que jQuery esté disponible
    if (typeof $ === 'undefined') {
        console.error('jQuery no está cargado');
        return;
    }

    const formElement = document.getElementById('formRegistroDonativos');
    if (!formElement) {
        console.warn('Formulario no encontrado');
        return;
    }

    // console.log('Formulario encontrado, vinculando eventos...');

    // Remover eventos previos para evitar duplicados
    $('#selectTipoDonador').off('change');
    $('#textNombreDonador').off('input').off('focus').off('blur');
    $('#btnAgregarArticulo').off('click');
    $(document).off('click', '.btn-eliminar-articulo').off('click', '.donador-opcion');
    $('#formRegistroDonativos').off('submit');
    $('#btnCancelar').off('click');

    // Event: Cambio de tipo de donador
    $('#selectTipoDonador').on('change', function () {
        toggleSeccionContacto($(this).val());
    });

    // Event: Búsqueda de donadores
    $('#textNombreDonador').on('input', function () {
        buscarDonadores($(this).val());
    }).on('focus', function () {
        if ($(this).val().length > 0) {
            buscarDonadores($(this).val());
        }
    }).on('blur', function () {
        // Ocultar la lista después de 200ms cuando pierda el foco
        setTimeout(function () {
            $('#listaDonadores').addClass('d-none');
        }, 200);
    });

    // Event: Agregar artículo
    $('#btnAgregarArticulo').on('click', function (e) {
        e.preventDefault();
        numeroArticulos++;
        agregarArticulo(numeroArticulos);
    });

    // Event: Eliminar artículo
    $(document).on('click', '.btn-eliminar-articulo', function (e) {
        e.preventDefault();
        $(this).closest('.articulo-item').remove();
    });

    // Event: Seleccionar donador de la lista
    $(document).on('click', '.donador-opcion', function (e) {
        e.preventDefault();
        seleccionarDonador($(this));
    });

    // Event: Submit del formulario
    $('#formRegistroDonativos').on('submit', function (e) {
        e.preventDefault();
        
        // Verificar modo edición
        if (modoEdicion && idDonativoEdicion) {
            actualizarDonativo(idDonativoEdicion);
        } else {
            guardarRegistro();
        }
    });

    // Event: Botón Cancelar
    $('#btnCancelar').on('click', function (e) {
        e.preventDefault();
        const form = document.getElementById('formRegistroDonativos');
        form.reset();
        document.getElementById('id_donador').value = '';
        document.getElementById('seccionContacto').classList.add('hidden');
        document.getElementById('grupoImporte').style.display = 'none';
        document.getElementById('contenedorArticulos').innerHTML = '';
        form.classList.remove('was-validated');
        $('#textNombreDonador').css('background-color', '');
        numeroArticulos = 0;
        donadorSeleccionado = null;
        $('#listaDonadores').addClass('d-none');
        toastr.info('Formulario limpiado', 'Operación realizada');
    });

    // Establecer fecha de recepción a la fecha de hoy
    $('#dateRecepcion').val(obtenerFechaHoy());

    // Asignar Centro ID del usuario desde la sesión
    if (typeof usuarioSession !== 'undefined' && usuarioSession.centroID) {
        $('#centroIDUsuario').val(usuarioSession.centroID);
        // console.log('✓ Centro ID asignado desde sesión:', usuarioSession.centroID);
    }

    // Resetear contador de artículos
    numeroArticulos = 0;
    donadorSeleccionado = null;
    
    // Cargar almacenes
    cargarAlmacenes();
    
    // Event: Manejar archivos adjuntos
    $('#fileAdjuntos').off('change').on('change', function (e) {
        manejarArchivosAdjuntos(e);
    });
    
    // console.log('Formulario inicializado correctamente');
}

/**
 * Busca donadores por nombre
 */
function buscarDonadores(termino) {
    const listaDonadores = $('#listaDonadores');

    // Limpiar el timer anterior si existe
    if (timerOcultarListaDonadores) {
        clearTimeout(timerOcultarListaDonadores);
        timerOcultarListaDonadores = null;
    }

    if (termino.length < 1) {
        listaDonadores.addClass('d-none');
        return;
    }

    $.ajax({
        type: 'GET',
        url: 'ObtenerDonadores',
        data: { search: termino },
        dataType: 'json',
        success: function (donadores) {
           // console.log('Donadores recibidos:', donadores);
            mostrarResultadosDonadores(donadores, termino);
        },
        error: function (xhr, status, error) {
            console.error('Error al buscar donadores:', error);
            console.error('Respuesta:', xhr.responseText);

            const listaDonadores = $('#listaDonadores');
            listaDonadores.html(`
                <div class="p-3 text-center text-stone-500">
                    <i class="fas fa-exclamation-circle"></i>
                    <p>Error al buscar donadores</p>
                    <small>${error}</small>
                </div>
            `);
            listaDonadores.removeClass('d-none');
        }
    });
}

/**
 * Muestra los resultados de donadores en una lista
 */
function mostrarResultadosDonadores(donadores, termino) {
    const listaDonadores = $('#listaDonadores');

    // Limpiar el timer anterior si existe
    if (timerOcultarListaDonadores) {
        clearTimeout(timerOcultarListaDonadores);
        timerOcultarListaDonadores = null;
    }

    // Validar que sea un array
    if (!Array.isArray(donadores)) {
        console.warn('donadores no es un array:', donadores);
        donadores = [];
    }

    if (!donadores || donadores.length === 0) {
        listaDonadores.html(`    
            <div class="p-2 text-center text-stone-500" style="font-size: 0.875rem;">
                <i class="fas fa-inbox"></i>
                <p style="margin: 0.25rem 0;">No se encontraron donadores</p>
                <small style="margin: 0;">Puedes crear uno nuevo escribiendo el nombre</small>
            </div>
        `);
        listaDonadores.removeClass('d-none');

        // Ocultar automáticamente después de 5 segundos
        timerOcultarListaDonadores = setTimeout(function () {
            listaDonadores.addClass('d-none');
            timerOcultarListaDonadores = null;
        }, 5000);
        return;
    }

    let html = '';
    donadores.forEach((donador) => {
        if (!donador || !donador.nombre) {
            console.warn('Donador sin nombre válido:', donador);
            return;
        }

        const donadorJSON = JSON.stringify(donador).replace(/'/g, "&apos;");
        html += `
            <div class="donador-opcion p-2 border-bottom border-stone-200 cursor-pointer hover:bg-stone-50" style="font-size: 0.875rem;" data-id="${donador.id_donador || ''}" data-donador='${donadorJSON}'>
                <div class="font-bold text-stone-800">${donador.nombre}</div>
                <small class="text-stone-500">
                    <i class="fas fa-tag"></i> ${donador.tipo || 'N/A'} | 
                    <i class="fas fa-id-card"></i> ${donador.rfc || 'N/A'}
                </small>
            </div>
        `;
    });

    if (html === '') {
        listaDonadores.html(`    
            <div class="p-2 text-center text-stone-500" style="font-size: 0.875rem;">
                <i class="fas fa-inbox"></i>
                <p style="margin: 0.25rem 0;">Sin resultados válidos</p>
            </div>
        `);
    } else {
        listaDonadores.html(html);
    }

    listaDonadores.removeClass('d-none');
}

/**
 * Busca artículos por nombre para un artículo específico
 */
function buscarArticulosPorNumero(termino, numeroArticulo) {
    const listaArticulos = $(`.lista-articulos-${numeroArticulo}`);

    if (termino.length < 1) {
        listaArticulos.addClass('d-none');
        return;
    }

    $.ajax({
        type: 'GET',
        url: 'ObtenerArticulos',
        data: { search: termino },
        dataType: 'json',
        success: function (articulos) {
            // console.log('Artículos recibidos:', articulos);
            mostrarResultadosArticulosPorNumero(articulos, numeroArticulo);
        },
        error: function (xhr, status, error) {
            console.error('Error al buscar artículos:', error);
            const listaArticulos = $(`.lista-articulos-${numeroArticulo}`);
            listaArticulos.html(`    
                <div class="p-2 text-center text-stone-500" style="font-size: 0.875rem;">
                    <i class="fas fa-exclamation-circle"></i>
                    <p style="margin: 0.25rem 0;">Error al buscar artículos</p>
                    <small style="margin: 0;">${error}</small>
                </div>
            `);
            listaArticulos.removeClass('d-none');
        }
    });
}

/**
 * Muestra los resultados de artículos para un número específico
 */
function mostrarResultadosArticulosPorNumero(articulos, numeroArticulo) {
    const listaArticulos = $(`.lista-articulos-${numeroArticulo}`);

    // Validar que sea un array
    if (!Array.isArray(articulos)) {
        console.warn('articulos no es un array:', articulos);
        articulos = [];
    }

    if (!articulos || articulos.length === 0) {
        listaArticulos.html(`    
            <div class="p-2 text-center text-stone-500" style="font-size: 0.875rem;">
                <i class="fas fa-inbox"></i>
                <p style="margin: 0.25rem 0;">No se encontraron artículos</p>
                <small style="margin: 0;">Puedes crear uno nuevo escribiendo el nombre</small>
            </div>
        `);
        listaArticulos.removeClass('d-none');
        return;
    }

    let html = '';
    articulos.forEach((articulo) => {
        if (!articulo || !articulo.nombre) {
            console.warn('Artículo sin nombre válido:', articulo);
            return;
        }

        const articuloJSON = JSON.stringify(articulo).replace(/'/g, "&apos;");
        const categoriaNombre = articulo.categoria?.nombre || 'N/A';

        html += `
            <div class="articulo-opcion-${numeroArticulo} p-2 border-bottom border-stone-200 cursor-pointer hover:bg-stone-50" style="font-size: 0.875rem;" data-id="${articulo.id_articulo || ''}" data-articulo='${articuloJSON}'>
                <div class="font-bold text-stone-800">${articulo.nombre}</div>
                <small class="text-stone-500">
                    <i class="fas fa-box"></i> ${categoriaNombre} | 
                </small>
            </div>
        `;
    });

    if (html === '') {
        listaArticulos.html(`    
            <div class="p-2 text-center text-stone-500" style="font-size: 0.875rem;">
                <i class="fas fa-inbox"></i>
                <p style="margin: 0.25rem 0;">Sin resultados válidos</p>
            </div>
        `);
    } else {
        listaArticulos.html(html);
    }

    listaArticulos.removeClass('d-none');
}

/**
 * Selecciona un donador de la lista y completa el formulario
 */
function seleccionarDonador($element) {
    try {
        const donador = JSON.parse($element.attr('data-donador'));
        donadorSeleccionado = donador;

        // Validar que tenga ID válido
        if (!donador.id_donador) {
            toastr.warning('El donador no tiene un ID válido', 'Validación');
            return;
        }

        // console.log('Donador seleccionado con ID:', donador.id_donador);

        // Guardar el ID en el input hidden
        $('#id_donador').val(donador.id_donador);

        // Llenar campos del formulario directamente
        $('#selectTipoDonador').val(donador.tipo || '');
        $('#textNombreDonador').val(donador.nombre || '');
        $('#textRFC').val(donador.rfc || '');
        $('#textTelefonoDonador').val(donador.telefono || '');
        $('#textEmailDonador').val(donador.email || '');
        $('#textDireccionDonador').val(donador.direccion || '');
        $('#textColoniaDonador').val(donador.colonia || '');
        $('#textCiudadDonador').val(donador.ciudad || '');
        $('#textEstadoDonador').val(donador.estado || '');
        $('#textCodigoPostalDonador').val(donador.codigo_postal || '');
        $('#textNotasDonador').val(donador.notas || '');

        // Ocultar lista de donadores
        $('#listaDonadores').addClass('d-none');

        // Mostrar sección de contacto si es organización o empresa
        toggleSeccionContacto(donador.tipo || '', donador.id_donador || '');

        // Permitir cambiar de opción cuando el campo se vacíe
        $('#textNombreDonador').off('input').on('input', function() {
            if ($(this).val().trim() === '') {
                $(this).css('background-color', '');
                $('#id_donador').val('');
                donadorSeleccionado = null;
            }
        });

        toastr.success(`✓ Donador "${donador.nombre}" seleccionado correctamente`, 'Éxito');
    } catch (error) {
        console.error('Error al seleccionar donador:', error);
        toastr.error('Error al seleccionar donador', 'Error');
    }
}

/**
 * Busca el contacto del donador por el id del donador
 */
function buscarContactoDonador(idDonador) {
    if (idDonador < 1) {
        return;
    }

    $.ajax({
        type: 'GET',
        url: 'ObtenerContactos',
        data: { id_donador: idDonador },
        dataType: 'json',
        success: function (contactos) {
            // console.log('Contactos recibidos:', contactos);
            mostrarResultadosContactos(contactos, idDonador);
        },
        error: function (xhr, status, error) {
            console.error('Error al buscar donadores:', error);
            console.error('Respuesta:', xhr.responseText);

            const listaDonadores = $('#listaDonadores');
            listaDonadores.html(`

                <div class="p-3 text-center text-stone-500">
                    <i class="fas fa-exclamation-circle"></i>
                    <p>Error al buscar donadores</p>
                    <small>${error}</small>
                </div>
            `);
            listaDonadores.removeClass('d-none');
        }
    });
}

function mostrarResultadosContactos(contactosJson, termino) {
    try {
        const contacto = JSON.parse(contactosJson);
        if (!contacto.id_contacto) {
            toastr.warning('El contacto no tiene un ID válido', 'Validación');
            return;
        }
        // console.log('Contacto seleccionado con ID:', contacto.id_contacto);

        $('#textNombreContacto').val(contacto.nombre || '');
        $('#textPuestoContacto').val(contacto.puesto || '');
        $('#textTelefonoContacto').val(contacto.telefono || '');
        $('#textExtensionContacto').val(contacto.extension || '');
        $('#textCelularContacto').val(contacto.celular || '');
        $('#textEmailContacto').val(contacto.email || '');
        $('#textHorarioContacto').val(contacto.horario_contacto || '');
        $('#selectMedioPreferido').val(contacto.medio_preferido || '');
        $('#checkEsPrincipal').val(contacto.es_principal || '');
        $('#checkActivoContacto').val(contacto.activo || '');
        $('#textNotasContacto').val(contacto.notas || '');
        toastr.success(`✓ Contacto "${contacto.nombre}" seleccionado correctamente`, 'Éxito');
    } catch (error) {
        console.error('Error al seleccionar donador:', error);
        toastr.error('Error al seleccionar donador', 'Error');
    }
}

/**
 * Selecciona un artículo en el contenedor especificado
 * Guarda el id_articulo para diferenciarlo como artículo existente
 */
function seleccionarArticuloEnContenedor($element, numeroArticulo) {
    try {
        const articulo = JSON.parse($element.attr('data-articulo'));

        if (!articulo.id_articulo) {
            toastr.warning('El artículo no tiene un ID válido', 'Validación');
            return;
        }

        // console.log('Artículo seleccionado:', articulo);
        // console.log('ID del artículo:', articulo.id_articulo);

        const $contenedor = $(`[data-articulo="${numeroArticulo}"]`);

        // Llenar campos del artículo
        const $nombreField = $contenedor.find('.input-articulo-nombre');
        $nombreField.val(articulo.nombre || '').css('background-color', '#e8f5e9');
        
        // Permitir cambiar de opción cuando el campo se vacíe
        $nombreField.off('input').on('input', function() {
            if ($(this).val().trim() === '') {
                $(this).css('background-color', '');
                $contenedor.data('id_articulo', null);
            }
        });

        $contenedor.find('.input-articulo-descripcion').val(articulo.descripcion || '');

        // Llenar la unidad de medida
        if (articulo.unidad_medida) {
            $contenedor.find('.input-articulo-unidad').val(articulo.unidad_medida);
            // console.log('Unidad de medida establecida a:', articulo.unidad_medida);
        }

        // Llenar la categoría - buscar por nombre en el objeto anidado
        if (articulo.categoria && articulo.categoria.nombre) {
            const idCategoria = articulo.categoria.id_categoria;
            const nombreCategoria = articulo.categoria.nombre;
            $contenedor.find('.input-articulo-categoria-select').val(idCategoria);
            // console.log('Categoría establecida a:', nombreCategoria);
        }

        // Guardar el ID del artículo en el data attribute
        $contenedor.data('id_articulo', articulo.id_articulo);
        // console.log('ID del artículo guardado en data attribute:', $contenedor.data('id_articulo'));

        // Ocultar lista
        $(`.lista-articulos-${numeroArticulo}`).addClass('d-none');

        toastr.success(`✓ Artículo "${articulo.nombre}" seleccionado`, 'Éxito');
    } catch (error) {
        console.error('Error al seleccionar artículo:', error);
        console.error('Data completo:', $element.attr('data-articulo'));
        toastr.error('Error al seleccionar artículo', 'Error');
    }
}

/**
 * Muestra u oculta la sección de contacto según el tipo de donador
 */
function toggleSeccionContacto(tipo, id_donador) {
    const seccionContacto = $('#seccionContacto');

    // console.log('Tipo de donador seleccionado:', tipo);

    if (tipo === 'organizacion' || tipo === 'empresa' || tipo === 'gobierno') {
        // console.log('Mostrando sección de contacto');

        // Mostrar la sección
        seccionContacto.removeClass('hidden');

        // Marcar campos específicos como requeridos
        $('#textNombreContacto').prop('required', true);
        $('#textPuestoContacto').prop('required', true);

        buscarContactoDonador(id_donador);
    } else {
        // console.log('Ocultando sección de contacto');

        // Ocultar la sección
        seccionContacto.addClass('hidden');

        // Remover validación y limpiar campos
        $('#textNombreContacto').prop('required', false).val('').removeClass('is-invalid');
        $('#textPuestoContacto').prop('required', false).val('').removeClass('is-invalid');
        $('#textTelefonoContacto').val('');
        $('#textExtensionContacto').val('');
        $('#textCelularContacto').val('');
        $('#textEmailContacto').val('');
        $('#textHorarioContacto').val('');
        $('#selectMedioPreferido').val('');
        $('#checkEsPrincipal').prop('checked', false);
        $('#checkActivoContacto').prop('checked', true);
        $('#textNotasContacto').val('');
    }
}

/**
 * Agrega un nuevo artículo dinámicamente
 */
function agregarArticulo(numero) {
    const articuloHtml = `
        <div class="articulo-item border border-stone-300 rounded-lg p-4 bg-stone-50" data-articulo="${numero}" data-id_articulo="">
            <div class="flex justify-between items-center mb-3">
                <h5 class="font-bold text-stone-700">Artículo #${numero}</h5>
                <button type="button" class="btn-eliminar-articulo btn btn-sm btn-danger">
                    <i class="fas fa-trash"></i>
                </button>
            </div>

            <!-- Campo de búsqueda/nombre del artículo -->
            <div class="form-group mb-4">
                <label class="block text-sm font-bold text-stone-700 mb-2">
                    <i class="fas fa-box" style="color:var(--dorado)"></i> <b>Nombre del Artículo</b> <span class="text-red-500">*</span>
                </label>
                <div class="position-relative">
                    <input 
                        type="text" 
                        class="form-control input-articulo-nombre" 
                        placeholder="Busca o crea un artículo"
                        autocomplete="off"
                        required>
                    <div class="invalid-feedback">El nombre del artículo es requerido</div>
                    <!-- Lista de resultados de búsqueda de artículos -->
                    <div class="lista-articulos-${numero} position-absolute w-100 bg-white border border-stone-300 rounded-lg mt-1 shadow-lg d-none" style="max-height: 150px; overflow-y: auto; z-index: 1000; top: 100%;">
                    </div>
                </div>
                <small class="text-stone-500 mt-1 block">Comienza a escribir para buscar artículos existentes o crea uno nuevo</small>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-4">
                <div class="form-group">
                    <label class="block text-sm font-bold text-stone-700 mb-2">
                        <i class="fas fa-folder" style="color:var(--dorado)"></i> <b>Categoría</b> <span class="text-red-500">*</span>
                    </label>
                    <select class="form-control input-articulo-categoria-select" required>
                        <option value="">-- Seleccione una Categoría --</option>
                        ${CATEGORIAS.map(cat => `<option value="${cat.id}">${cat.nombre}</option>`).join('')}
                    </select>
                    <div class="invalid-feedback">Selecciona una categoría</div>
                </div>

                <div class="form-group">
                    <label class="block text-sm font-bold text-stone-700 mb-2">
                        <i class="fas fa-check-circle" style="color:var(--dorado)"></i> <b>Estado del Artículo</b> <span class="text-red-500">*</span>
                    </label>
                    <select class="form-control input-articulo-estado" required>
                        <option value="">-- Seleccione Estado --</option>
                        ${ESTADOS_ARTICULO.map(estado => `<option value="${estado.value}">${estado.label}</option>`).join('')}
                    </select>
                    <div class="invalid-feedback">Selecciona un estado</div>
                </div>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-4">
                <div class="form-group">
                    <label class="block text-sm font-bold text-stone-700 mb-2">
                        Cantidad <span class="text-red-500">*</span>
                    </label>
                    <input type="number" class="form-control input-articulo-cantidad" min="1" placeholder="0" required>
                    <div class="invalid-feedback">La cantidad es requerida</div>
                </div>

                <div class="form-group">
                    <label class="block text-sm font-bold text-stone-700 mb-2">
                        Unidad de Medida <span class="text-red-500">*</span>
                    </label>
                    <select class="form-control input-articulo-unidad" required>
                        <option value="">-- Seleccione --</option>
                        <option value="piezas">Piezas</option>
                        <option value="kilos">Kilos</option>
                        <option value="litros">Litros</option>
                        <option value="metros">Metros</option>
                        <option value="cajas">Cajas</option>
                        <option value="otros">Otros</option>
                    </select>
                    <div class="invalid-feedback">Selecciona una unidad de medida</div>
                </div>
            </div>

            <div class="form-group">
                <label class="block text-sm font-bold text-stone-700 mb-2">
                    Valor Unitario (opcional)
                </label>
                <input type="number" class="form-control input-articulo-valor" min="0" step="0.01" placeholder="0.00">
            </div>

            <div class="form-group">
                <label class="block text-sm font-bold text-stone-700 mb-2">
                    Descripción/Notas
                </label>
                <textarea class="form-control input-articulo-descripcion" rows="2" placeholder="Descripción adicional del artículo"></textarea>
            </div>
        </div>
    `;

    $('#contenedorArticulos').append(articuloHtml);

    // Vinculación de eventos para el nuevo artículo
    const $nuevoArticulo = $(`[data-articulo="${numero}"]`);

    $nuevoArticulo.find('.input-articulo-nombre').on('input', function () {
        buscarArticulosPorNumero($(this).val(), numero);
    }).on('focus', function () {
        if ($(this).val().length > 0) {
            buscarArticulosPorNumero($(this).val(), numero);
        }
    }).on('blur', function () {
        // Ocultar la lista después de 200ms cuando pierda el foco
        setTimeout(function () {
            $(`.lista-articulos-${numero}`).addClass('d-none');
        }, 200);
    });

    // Evento para cambio de categoría manual
    $nuevoArticulo.find('.input-articulo-categoria-select').on('change', function () {
        // console.log('Categoría seleccionada:', $(this).val());
    });

    // Evento para cambio de estado
    $nuevoArticulo.find('.input-articulo-estado').on('change', function () {
        // console.log('Estado seleccionado:', $(this).val());
    });

    // Vinculación de evento para seleccionar artículo
    $(document).on('click', `.articulo-opcion-${numero}`, function (e) {
        e.preventDefault();
        seleccionarArticuloEnContenedor($(this), numero);
    });
}

/**
 * Obtiene los datos del formulario y construye los objetos JSON
 */
function guardarRegistro() {
    const form = document.getElementById('formRegistroDonativos');

    // Validar que al menos haya un artículo
    if ($('#contenedorArticulos').find('.articulo-item').length === 0) {
        toastr.warning('Agrega al menos un artículo', 'Validación');
        return;
    }

    // Validar el formulario
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        toastr.error('Por favor completa todos los campos requeridos', 'Validación');
        return;
    }

    // Construir objeto Donador
    const donador = {
        tipo: $('#selectTipoDonador').val(),
        nombre: $('#textNombreDonador').val().trim(),
        rfc: $('#textRFC').val().trim(),
        telefono: $('#textTelefonoDonador').val().trim(),
        email: $('#textEmailDonador').val().trim(),
        direccion: $('#textDireccionDonador').val().trim(),
        colonia: $('#textColoniaDonador').val().trim(),
        ciudad: $('#textCiudadDonador').val().trim(),
        estado: $('#textEstadoDonador').val().trim(),
        codigo_postal: $('#textCodigoPostalDonador').val().trim(),
        notas: $('#textNotasDonador').val().trim()
    };

    // Construir objeto Contacto (si aplica)
    let contacto = null;
    const tipoDonador = $('#selectTipoDonador').val();
    if (tipoDonador === 'organizacion' || tipoDonador === 'empresa' || tipoDonador ==='gobierno') {
        contacto = {
            nombre: $('#textNombreContacto').val().trim(),
            puesto: $('#textPuestoContacto').val().trim(),
            telefono: $('#textTelefonoContacto').val().trim(),
            extension: $('#textExtensionContacto').val().trim(),
            celular: $('#textCelularContacto').val().trim(),
            email: $('#textEmailContacto').val().trim(),
            horario_contacto: $('#textHorarioContacto').val().trim(),
            medio_preferido: $('#selectMedioPreferido').val().trim(),
            es_principal: $('#checkEsPrincipal').is(':checked'),
            notas: $('#textNotasContacto').val(),
            activo: $('#checkActivoContacto').is(':checked')
        };
    }

    // Separar artículos nuevos de existentes
    const articulosNuevos = [];
    const articulosExistentes = [];

    $('#contenedorArticulos').find('.articulo-item').each(function () {
        const $item = $(this);
        const idArticulo = $item.data('id_articulo');

        const articulo = {
            nombre: $item.find('.input-articulo-nombre').val(),
            unidad_medida: $item.find('.input-articulo-unidad').val(),
            id_categoria: parseInt($item.find('.input-articulo-categoria-select').val()),
            descripcion: $item.find('.input-articulo-descripcion').val(),
            // Datos del detalle
            cantidad_recibida: parseInt($item.find('.input-articulo-cantidad').val()) || 0,
            valor_unitario: parseFloat($item.find('.input-articulo-valor').val()) || 0,
            estado: $item.find('.input-articulo-estado').val()
        };

        // Si tiene ID, es un artículo existente
        if (idArticulo) {
            articulo.id_articulo = idArticulo;
            articulosExistentes.push(articulo);
        } else {
            // Si no tiene ID, es un artículo nuevo
            articulosNuevos.push(articulo);
        }
    });

    // console.log('Datos del donador:', donador);
    // console.log('Datos del contacto:', contacto);
    // console.log('Artículos nuevos:', articulosNuevos);
    // console.log('Artículos existentes:', articulosExistentes);

    // Enviar datos de forma secuencial
    guardarDonador(donador, contacto, articulosNuevos, articulosExistentes);
}

/**
 * Guarda el donador y procede con contacto y artículos
 */
function guardarDonador(donador, contacto, articulosNuevos, articulosExistentes) {
    const datosString = JSON.stringify(donador);
    const idDonadorExistente = $('#id_donador').val();

    // console.log('ID de donador existente:', idDonadorExistente);

    // Si ya existe un donador (fue seleccionado de la lista), usar su ID
    if (idDonadorExistente && idDonadorExistente !== '') {
        // console.log('Usando donador existente con ID:', idDonadorExistente);
        toastr.info('Usando donador existente', 'Información');

        // Proceder directamente con contacto o artículos
        if (contacto) {
            contacto.id_donador = parseInt(idDonadorExistente);
            guardarContacto(contacto, articulosNuevos, articulosExistentes, parseInt(idDonadorExistente));
        } else {
            insertarArticulosNuevos(articulosNuevos, articulosExistentes, parseInt(idDonadorExistente));
        }
        return;
    }

    // Si no existe ID, es un donador nuevo, hay que guardarlo
    // console.log('Guardando donador nuevo');

    $.ajax({
        type: 'POST',
        url: 'insertaDonador',
        data: { datos: datosString },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando donador...', 'Procesando');
        },
        success: function (response) {
            // console.log('Respuesta al guardar donador:', response);

            // Parsear la respuesta para obtener el ID del donador
            let donadorId = null;
            try {
                const responseData = typeof response === 'string' ? JSON.parse(response) : response;
                donadorId = responseData.id_donador || responseData.id;
                // console.log('ID del donador guardado:', donadorId);
            } catch (e) {
                console.warn('No se pudo extraer ID del donador:', e);
                toastr.error('No se pudo obtener el ID del donador guardado: ' + e.message, 'Error');
                return;
            }

            // Proceder con contacto si existe
            if (contacto) {
                contacto.id_donador = donadorId;
                guardarContacto(contacto, articulosNuevos, articulosExistentes, donadorId);
            } else {
                // Si no hay contacto, proceder directamente a insertar artículos nuevos
                insertarArticulosNuevos(articulosNuevos, articulosExistentes, donadorId);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar donador:', error);
            console.error('Respuesta del servidor:', xhr.responseText);
            toastr.error('Error al guardar el donador. Intenta de nuevo.', 'Error');
        }
    });
}

/**
 * Guarda el contacto del donador
 */
function guardarContacto(contacto, articulosNuevos, articulosExistentes, donadorId) {
    const datosString = JSON.stringify(contacto);

    $.ajax({
        type: 'POST',
        url: 'insertaContactoDonaodor',
        data: { datos: datosString },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando contacto...', 'Procesando');
        },
        success: function (response) {
            // console.log('Respuesta al guardar contacto:', response);

            // Proceder a insertar artículos nuevos
            insertarArticulosNuevos(articulosNuevos, articulosExistentes, donadorId);
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar contacto:', error);
            console.error('Respuesta del servidor:', xhr.responseText);
            toastr.error('Error al guardar el contacto. Intenta de nuevo.', 'Error');
        }
    });
}

/**
 * Inserta los artículos nuevos en el catálogo
 * Luego construye y envía el JSON completo del donativo
 */
function insertarArticulosNuevos(articulosNuevos, articulosExistentes, donadorId) {
    // Si no hay artículos nuevos, proceder directamente a construir el donativo
    if (!articulosNuevos || articulosNuevos.length === 0) {
        construirYGuardarDonativo([], articulosExistentes, donadorId);
        return;
    }
    
    // Filtrar solo los campos necesarios para insertar artículos
    const articulosParaInsertar = articulosNuevos.map(articulo => ({
        nombre: articulo.nombre,      
        id_categoria: articulo.id_categoria
    }));

    const datosNuevos = JSON.stringify(articulosParaInsertar);

    $.ajax({
        type: 'POST',
        url: 'insertaArticulos',
        data: { datos: datosNuevos },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando artículos nuevos...', 'Procesando');
        },
        success: function (response) {
            // console.log('Respuesta al guardar artículos nuevos:', response);

            // Extraer los IDs de los artículos insertados
            let articulosInsertadosConId = [];
            try {
                // El endpoint retorna un objeto con respuesta como string JSON
                if (response && response.respuesta) {
                    try {
                        const articuloInsertado = JSON.parse(response.respuesta);
                        // console.log('Artículo insertado parseado:', articuloInsertado);
                        
                        if (articuloInsertado && articuloInsertado.id_articulo) {
                            // El endpoint retorna un solo artículo, pero podría haber múltiples en articulosNuevos
                            // Buscar cuál es el artículo que se insertó y emparejarlo
                            const articuloOriginal = articulosNuevos.find(a => 
                                a.nombre === articuloInsertado.nombre && 
                                a.id_categoria === articuloInsertado.id_categoria
                            );
                            

                            if (articuloOriginal) {
                                articulosInsertadosConId.push({
                                    ...articuloOriginal,
                                    id_articulo: articuloInsertado.id_articulo
                                });
                                // console.log('Artículo emparejado con ID:', articuloInsertado.id_articulo);
                            }
                        }
                    } catch (parseError) {
                        console.error('Error al parsear respuesta:', parseError);
                    }
                } else if (response && response.resultados && Array.isArray(response.resultados)) {
                    // Estructura alternativa con array de resultados
                    articulosInsertadosConId = response.resultados
                        .map((resultado, index) => {
                            // console.log(`Procesando resultado ${index}:`, resultado);
                            
                            let idArticulo = null;
                            

                            if (resultado.id_articulo) {
                                idArticulo = resultado.id_articulo;
                            } else if (resultado.id) {
                                idArticulo = resultado.id;
                            } else if (resultado.respuesta) {
                                const parsed = JSON.parse(resultado.respuesta);
                                idArticulo = parsed.id_articulo;
                            }
                            

                            return {
                                ...articulosNuevos[index],
                                id_articulo: idArticulo
                            };
                        })
                        .filter(articulo => articulo.id_articulo !== null && articulo.id_articulo !== undefined);
                } else {
                    console.warn('Respuesta sin estructura esperada:', response);
                    articulosInsertadosConId = articulosNuevos;
                }
            } catch (e) {
                console.error('Error al procesar artículos insertados:', e);
                console.error('Response completa:', response);
                toastr.error('Error al procesar artículos insertados', 'Error');
                return;
            }

            // Validar que se obtuvieron IDs
            if (articulosInsertadosConId.length === 0) {
                console.warn('⚠ No se pudieron extraer los IDs de los artículos insertados');
                toastr.warning('Se insertaron artículos pero no se pudo obtener sus IDs', 'Advertencia');
                // Continuar igualmente con los artículos sin IDs
                articulosInsertadosConId = articulosNuevos;
            }

            // console.log('Artículos finales para donativo:', articulosInsertadosConId);

            // Construir y guardar el donativo con todos los artículos
            construirYGuardarDonativo(articulosInsertadosConId, articulosExistentes, donadorId);
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar artículos nuevos:', error);
            console.error('Status:', status);
            console.error('Respuesta completa:', xhr.responseText);
            toastr.error('Error al guardar los artículos nuevos. Intenta de nuevo.', 'Error');
        }
    });
}

/**
 * Obtiene los datos del formulario y construye los objetos JSON
 */
function guardarRegistro() {
    const form = document.getElementById('formRegistroDonativos');

    // Validar que al menos haya un artículo
    if ($('#contenedorArticulos').find('.articulo-item').length === 0) {
        toastr.warning('Agrega al menos un artículo', 'Validación');
        return;
    }

    // Validar el formulario
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        toastr.error('Por favor completa todos los campos requeridos', 'Validación');
        return;
    }

    // Construir objeto Donador
    const donador = {
        tipo: $('#selectTipoDonador').val(),
        nombre: $('#textNombreDonador').val().trim(),
        rfc: $('#textRFC').val(),
        telefono: $('#textTelefonoDonador').val(),
        email: $('#textEmailDonador').val(),
        direccion: $('#textDireccionDonador').val(),
        colonia: $('#textColoniaDonador').val(),
        ciudad: $('#textCiudadDonador').val(),
        estado: $('#textEstadoDonador').val(),
        codigo_postal: $('#textCodigoPostalDonador').val(),
        notas: $('#textNotasDonador').val()
    };

    // Construir objeto Contacto (si aplica)
    let contacto = null;
    const tipoDonador = $('#selectTipoDonador').val();
    if (tipoDonador === 'organizacion' || tipoDonador === 'empresa' || tipoDonador ==='gobierno') {
        contacto = {
            nombre: $('#textNombreContacto').val().trim(),
            puesto: $('#textPuestoContacto').val().trim(),
            telefono: $('#textTelefonoContacto').val(),
            extension: $('#textExtensionContacto').val(),
            celular: $('#textCelularContacto').val(),
            email: $('#textEmailContacto').val(),
            horario_contacto: $('#textHorarioContacto').val(),
            medio_preferido: $('#selectMedioPreferido').val(),
            es_principal: $('#checkEsPrincipal').is(':checked'),
            notas: $('#textNotasContacto').val(),
            activo: $('#checkActivoContacto').is(':checked')
        };
    }

    // Separar artículos nuevos de existentes
    const articulosNuevos = [];
    const articulosExistentes = [];

    $('#contenedorArticulos').find('.articulo-item').each(function () {
        const $item = $(this);
        const idArticulo = $item.data('id_articulo');

        const articulo = {
            nombre: $item.find('.input-articulo-nombre').val(),
            unidad_medida: $item.find('.input-articulo-unidad').val(),
            id_categoria: parseInt($item.find('.input-articulo-categoria-select').val()),
            descripcion: $item.find('.input-articulo-descripcion').val(),
            // Datos del detalle
            cantidad_recibida: parseInt($item.find('.input-articulo-cantidad').val()) || 0,
            valor_unitario: parseFloat($item.find('.input-articulo-valor').val()) || 0,
            estado: $item.find('.input-articulo-estado').val()
        };

        // Si tiene ID, es un artículo existente
        if (idArticulo) {
            articulo.id_articulo = idArticulo;
            articulosExistentes.push(articulo);
        } else {
            // Si no tiene ID, es un artículo nuevo
            articulosNuevos.push(articulo);
        }
    });

    // console.log('Datos del donador:', donador);
    // console.log('Datos del contacto:', contacto);
    // console.log('Artículos nuevos:', articulosNuevos);
    // console.log('Artículos existentes:', articulosExistentes);

    // Enviar datos de forma secuencial
    guardarDonador(donador, contacto, articulosNuevos, articulosExistentes);
}

/**
 * Guarda el donador y procede con contacto y artículos
 */
function guardarDonador(donador, contacto, articulosNuevos, articulosExistentes) {
    const datosString = JSON.stringify(donador);
    const idDonadorExistente = $('#id_donador').val();

    // console.log('ID de donador existente:', idDonadorExistente);

    // Si ya existe un donador (fue seleccionado de la lista), usar su ID
    if (idDonadorExistente && idDonadorExistente !== '') {
        // console.log('Usando donador existente con ID:', idDonadorExistente);
        toastr.info('Usando donador existente', 'Información');

        // Proceder directamente con contacto o artículos
        if (contacto) {
            contacto.id_donador = parseInt(idDonadorExistente);
            guardarContacto(contacto, articulosNuevos, articulosExistentes, parseInt(idDonadorExistente));
        } else {
            insertarArticulosNuevos(articulosNuevos, articulosExistentes, parseInt(idDonadorExistente));
        }
        return;
    }

    // Si no existe ID, es un donador nuevo, hay que guardarlo
    // console.log('Guardando donador nuevo');

    $.ajax({
        type: 'POST',
        url: 'insertaDonador',
        data: { datos: datosString },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando donador...', 'Procesando');
        },
        success: function (response) {
            // console.log('Respuesta al guardar donador:', response);

            // Parsear la respuesta para obtener el ID del donador
            let donadorId = null;
            try {
                const responseData = typeof response === 'string' ? JSON.parse(response) : response;
                donadorId = responseData.id_donador || responseData.id;
                // console.log('ID del donador guardado:', donadorId);
            } catch (e) {
                console.warn('No se pudo extraer ID del donador:', e);
                toastr.error('No se pudo obtener el ID del donador guardado: ' + e.message, 'Error');
                return;
            }

            // Proceder con contacto si existe
            if (contacto) {
                contacto.id_donador = donadorId;
                guardarContacto(contacto, articulosNuevos, articulosExistentes, donadorId);
            } else {
                // Si no hay contacto, proceder directamente a insertar artículos nuevos
                insertarArticulosNuevos(articulosNuevos, articulosExistentes, donadorId);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar donador:', error);
            console.error('Respuesta del servidor:', xhr.responseText);
            toastr.error('Error al guardar el donador. Intenta de nuevo.', 'Error');
        }
    });
}

/**
 * Guarda el contacto del donador
 */
function guardarContacto(contacto, articulosNuevos, articulosExistentes, donadorId) {
    const datosString = JSON.stringify(contacto);

    $.ajax({
        type: 'POST',
        url: 'insertaContactoDonaodor',
        data: { datos: datosString },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando contacto...', 'Procesando');
        },
        success: function (response) {
            // console.log('Respuesta al guardar contacto:', response);

            // Proceder a insertar artículos nuevos
            insertarArticulosNuevos(articulosNuevos, articulosExistentes, donadorId);
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar contacto:', error);
            console.error('Respuesta del servidor:', xhr.responseText);
            toastr.error('Error al guardar el contacto. Intenta de nuevo.', 'Error');
        }
    });
}

/**
 * Inserta los artículos nuevos en el catálogo
 * Luego construye y envía el JSON completo del donativo
 */
function insertarArticulosNuevos(articulosNuevos, articulosExistentes, donadorId) {
    // Si no hay artículos nuevos, proceder directamente a construir el donativo
    if (!articulosNuevos || articulosNuevos.length === 0) {
        construirYGuardarDonativo([], articulosExistentes, donadorId);
        return;
    }
    
    // Filtrar solo los campos necesarios para insertar artículos
    const articulosParaInsertar = articulosNuevos.map(articulo => ({
        nombre: articulo.nombre,      
        id_categoria: articulo.id_categoria
    }));

    const datosNuevos = JSON.stringify(articulosParaInsertar);

    $.ajax({
        type: 'POST',
        url: 'insertaArticulos',
        data: { datos: datosNuevos },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando artículos nuevos...', 'Procesando');
        },
        success: function (response) {
            // console.log('Respuesta al guardar artículos nuevos:', response);

            // Extraer los IDs de los artículos insertados
            let articulosInsertadosConId = [];
            try {
                // El endpoint retorna un objeto con respuesta como string JSON
                if (response && response.respuesta) {
                    try {
                        const articuloInsertado = JSON.parse(response.respuesta);
                        // console.log('Artículo insertado parseado:', articuloInsertado);
                        
                        if (articuloInsertado && articuloInsertado.id_articulo) {
                            // El endpoint retorna un solo artículo, pero podría haber múltiples en articulosNuevos
                            // Buscar cuál es el artículo que se insertó y emparejarlo
                            const articuloOriginal = articulosNuevos.find(a => 
                                a.nombre === articuloInsertado.nombre && 
                                a.id_categoria === articuloInsertado.id_categoria
                            );
                            

                            if (articuloOriginal) {
                                articulosInsertadosConId.push({
                                    ...articuloOriginal,
                                    id_articulo: articuloInsertado.id_articulo
                                });
                                // console.log('Artículo emparejado con ID:', articuloInsertado.id_articulo);
                            }
                        }
                    } catch (parseError) {
                        console.error('Error al parsear respuesta:', parseError);
                    }
                } else if (response && response.resultados && Array.isArray(response.resultados)) {
                    // Estructura alternativa con array de resultados
                    articulosInsertadosConId = response.resultados
                        .map((resultado, index) => {
                            // console.log(`Procesando resultado ${index}:`, resultado);
                            
                            let idArticulo = null;
                            

                            if (resultado.id_articulo) {
                                idArticulo = resultado.id_articulo;
                            } else if (resultado.id) {
                                idArticulo = resultado.id;
                            } else if (resultado.respuesta) {
                                const parsed = JSON.parse(resultado.respuesta);
                                idArticulo = parsed.id_articulo;
                            }
                            

                            return {
                                ...articulosNuevos[index],
                                id_articulo: idArticulo
                            };
                        })
                        .filter(articulo => articulo.id_articulo !== null && articulo.id_articulo !== undefined);
                } else {
                    console.warn('Respuesta sin estructura esperada:', response);
                    articulosInsertadosConId = articulosNuevos;
                }
            } catch (e) {
                console.error('Error al procesar artículos insertados:', e);
                console.error('Response completa:', response);
                toastr.error('Error al procesar artículos insertados', 'Error');
                return;
            }

            // Validar que se obtuvieron IDs
            if (articulosInsertadosConId.length === 0) {
                console.warn('⚠ No se pudieron extraer los IDs de los artículos insertados');
                toastr.warning('Se insertaron artículos pero no se pudo obtener sus IDs', 'Advertencia');
                // Continuar igualmente con los artículos sin IDs
                articulosInsertadosConId = articulosNuevos;
            }

            // console.log('Artículos finales para donativo:', articulosInsertadosConId);

            // Construir y guardar el donativo con todos los artículos
            construirYGuardarDonativo(articulosInsertadosConId, articulosExistentes, donadorId);
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar artículos nuevos:', error);
            console.error('Status:', status);
            console.error('Respuesta completa:', xhr.responseText);
            toastr.error('Error al guardar los artículos nuevos. Intenta de nuevo.', 'Error');
        }
    });
}

/**
 * Construye el JSON completo del donativo y lo envía
 * Estructura: {id_donador, fecha_recepcion, tipo, estatus, observaciones, detalles[], archivos[]}
 */
function construirYGuardarDonativo(articulosInsertados, articulosExistentes, donadorId) {
    // Combinar todos los artículos (nuevos insertados + existentes)
    const todosLosArticulos = [...articulosInsertados, ...articulosExistentes];

    if (todosLosArticulos.length === 0) {
        toastr.warning('No hay artículos para registrar', 'Validación');
        return;
    }

    // Si hay archivos adjuntos, subirlos primero
    if (archivosAdjuntos.length > 0) {
        // console.log('Subiendo archivos antes de guardar donativo...');
        subirArchivos(articulosInsertados, articulosExistentes, donadorId);
        return;
    }

    // Si no hay archivos, proceder directamente con el donativo
    guardarDonativoCompleto(todosLosArticulos, donadorId, []);
}


/**
 * Guarda el donativo completo con los detalles y archivos
 */
function guardarDonativoCompleto(todosLosArticulos, donadorId, archivosGuardados) {
    // Construir los detalles con todos los artículos
    const detalles = todosLosArticulos.map(articulo => ({
        id_articulo: articulo.id_articulo,
        cantidad_recibida: articulo.cantidad_recibida,
        valor_unitario: articulo.valor_unitario,
        estado: articulo.estado,
        unidad_medida: articulo.unidad_medida,
        descripcion: articulo.descripcion
    }));

    // Construir el JSON completo del donativo
    const donativoCompleto = {
        id_donador: donadorId,
        fecha_recepcion: $('#dateRecepcion').val() + 'T10:30:00',
        tipo: $('#selectTipoDonativo').val(),
        estatus: 'Recibido',
        observaciones: $('#textObservacionesDonativo').val(),
        id_almacen: $('#selectAlmacenDestino').val(),
        detalles: detalles
    };

    // Agregar archivos si los hay
    if (archivosGuardados && archivosGuardados.length > 0) {
        donativoCompleto.archivos = archivosGuardados;
    }

    const datosString = JSON.stringify(donativoCompleto);

    // console.log('Donativo completo a enviar:', donativoCompleto);

    $.ajax({
        type: 'POST',
        url: 'insertaDonativo',
        data: { datos: datosString },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando donativo...', 'Procesando');
        },
        success: function (response) {
            console.log('Respuesta al guardar donativo:', response);

            if (response.exito) {
                // Mostrar SweetAlert de éxito
                Swal.fire({
                    icon: 'success',
                    title: '¡Donativo Registrado!',
                    text: 'El donativo se ha registrado correctamente',
                    confirmButtonColor: '#9c223c',
                    confirmButtonText: 'Aceptar'
                }).then((result) => {
                    if (result.isConfirmed) {
                        // Ejecutar limpiar formulario después de confirmar
                        limpiarFormulario();
                    }
                });

                toastr.success('Donativo registrado correctamente', 'Éxito');
            } else {
                toastr.error(response.mensaje || 'Error al registrar', 'Error');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar donativo:', error);
            console.error('Respuesta:', xhr.responseText);

            if (xhr.status === 400) {
                toastr.error('Datos de donativo inválidos. Por favor revisa los campos.', 'Error');
            } else if (xhr.status === 500) {
                toastr.error('Error en el servidor. Intenta de nuevo.' & xhr.statusText, 'Error');
            } else {
                toastr.error('Error al guardar el donativo: ' + error, 'Error');
            }
        }
    });
}

/**
 * Actualiza un donativo existente
 * Construye el JSON con la estructura requerida por el endpoint actualizaDonativo
 */
function actualizarDonativo(idDonativo) {
    const form = document.getElementById('formRegistroDonativos');

    // Validar que al menos haya un artículo
    if ($('#contenedorArticulos').find('.articulo-item').length === 0) {
        toastr.warning('Agrega al menos un artículo', 'Validación');
        return;
    }

    // Validar el formulario
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        toastr.error('Por favor completa todos los campos requeridos', 'Validación');
        return;
    }

    // Separar artículos nuevos de existentes
    const articulosNuevos = [];
    const articulosExistentes = [];

    $('#contenedorArticulos').find('.articulo-item').each(function () {
        const $item = $(this);
        const idArticulo = $item.data('id_articulo');
        const idDonativoDetalle = $item.data('id_donativo_detalle');

        const articulo = {
            nombre: $item.find('.input-articulo-nombre').val(),
            unidad_medida: $item.find('.input-articulo-unidad').val(),
            id_categoria: parseInt($item.find('.input-articulo-categoria-select').val()),
            descripcion: $item.find('.input-articulo-descripcion').val(),
            // Datos del detalle
            cantidad_recibida: parseInt($item.find('.input-articulo-cantidad').val()) || 0,
            valor_unitario: parseFloat($item.find('.input-articulo-valor').val()) || 0,
            estado: $item.find('.input-articulo-estado').val(),
            id_donativo_detalle: idDonativoDetalle || null
        };

        // Si tiene id_articulo, es un artículo existente
        if (idArticulo) {
            articulo.id_articulo = idArticulo;
            articulosExistentes.push(articulo);
        } else {
            // Si no tiene id_articulo, es un artículo nuevo que hay que guardar primero
            articulosNuevos.push(articulo);
        }
    });

    // console.log('Artículos nuevos a guardar:', articulosNuevos);
    // console.log('Artículos existentes:', articulosExistentes);

    // Si hay artículos nuevos, guardarlos primero
    if (articulosNuevos.length > 0) {
        insertarArticulosNuevosParaActualizacion(articulosNuevos, articulosExistentes, idDonativo);
    } else {
        // Si no hay artículos nuevos, proceder directamente con la actualización
        if (archivosAdjuntos.length > 0) {
            // console.log('Subiendo nuevos archivos antes de actualizar...');
            subirArchivosParaActualizacion(idDonativo, articulosExistentes);
        } else {
            enviarActualizacionDonativo(idDonativo, articulosExistentes, []);
        }
    }
}

/**
 * Inserta artículos nuevos antes de actualizar el donativo
 */
function insertarArticulosNuevosParaActualizacion(articulosNuevos, articulosExistentes, idDonativo) {
    // Filtrar solo los campos necesarios para insertar artículos
    const articulosParaInsertar = articulosNuevos.map(articulo => ({
        nombre: articulo.nombre,      
        id_categoria: articulo.id_categoria
    }));

    const datosNuevos = JSON.stringify(articulosParaInsertar);

    $.ajax({
        type: 'POST',
        url: 'insertaArticulos',
        data: { datos: datosNuevos },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando nuevos artículos...', 'Procesando');
        },
        success: function (response) {
            //console.log('Respuesta al guardar artículos nuevos:', response);

            // Extraer los IDs de los artículos insertados
            let articulosInsertadosConId = [];
            try {
                // El endpoint retorna un objeto con respuesta como string JSON
                if (response && response.respuesta) {
                    try {
                        const articuloInsertado = JSON.parse(response.respuesta);
                        //console.log('Artículo insertado parseado:', articuloInsertado);
                        
                        if (articuloInsertado && articuloInsertado.id_articulo) {
                            // Buscar el artículo original y agregarle el ID
                            const articuloOriginal = articulosNuevos.find(a => 
                                a.nombre === articuloInsertado.nombre && 
                                a.id_categoria === articuloInsertado.id_categoria
                            );
                            
                            if (articuloOriginal) {
                                articulosInsertadosConId.push({
                                    ...articuloOriginal,
                                    id_articulo: articuloInsertado.id_articulo
                                });
                                //console.log('Artículo emparejado con ID:', articuloInsertado.id_articulo);
                            }
                        }
                    } catch (parseError) {
                        console.error('Error al parsear respuesta:', parseError);
                    }
                } else if (response && response.resultados && Array.isArray(response.resultados)) {
                    // Estructura alternativa con array de resultados
                    articulosInsertadosConId = response.resultados
                        .map((resultado, index) => {
                            //console.log(`Procesando resultado ${index}:`, resultado);
                            
                            let idArticulo = null;
                            

                            if (resultado.id_articulo) {
                                idArticulo = resultado.id_articulo;
                            } else if (resultado.id) {
                                idArticulo = resultado.id;
                            } else if (resultado.respuesta) {
                                const parsed = JSON.parse(resultado.respuesta);
                                idArticulo = parsed.id_articulo;
                            }
                            

                            return {
                                ...articulosNuevos[index],
                                id_articulo: idArticulo
                            };
                        })
                        .filter(articulo => articulo.id_articulo !== null && articulo.id_articulo !== undefined);
                } else {
                    console.warn('Respuesta sin estructura esperada:', response);
                    toastr.error('No se pudo obtener el ID de los artículos nuevos', 'Error');
                    return;
                }
            } catch (e) {
                console.error('Error al procesar artículos insertados:', e);
                console.error('Response completa:', response);
                toastr.error('Error al procesar artículos insertados', 'Error');
                return;
            }

            // Validar que se obtuvieron IDs
            if (articulosInsertadosConId.length === 0) {
                console.warn('⚠ No se pudieron extraer los IDs de los artículos insertados');
                toastr.warning('Se insertaron artículos pero no se pudo obtener sus IDs', 'Advertencia');
                // Continuar igualmente con los artículos sin IDs
                articulosInsertadosConId = articulosNuevos;
            }

           // console.log('Artículos nuevos con IDs:', articulosInsertadosConId);

            // Combinar artículos insertados con existentes
            const todosLosArticulos = [...articulosInsertadosConId, ...articulosExistentes];

            // Proceder con archivos o actualización directa
            if (archivosAdjuntos.length > 0) {
                //console.log('Subiendo nuevos archivos antes de actualizar...');
                subirArchivosParaActualizacion(idDonativo, todosLosArticulos);
            } else {
                enviarActualizacionDonativo(idDonativo, todosLosArticulos, []);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar artículos nuevos:', error);
            console.error('Status:', status);
            console.error('Respuesta completa:', xhr.responseText);
            toastr.error('Error al guardar los artículos nuevos. Intenta de nuevo.', 'Error');
        }
    });
}

/**
 * Sube archivos nuevos para la actualización
 */
function subirArchivosParaActualizacion(idDonativo, todosLosArticulos) {
    const formData = new FormData();
    
    // Agregar cada archivo al FormData
    archivosAdjuntos.forEach((archivoObj, index) => {
        formData.append(`archivos`, archivoObj.file);
        formData.append(`nombres_originales`, archivoObj.nombre);
    });

    $.ajax({
        type: 'POST',
        url: 'GuardarArchivos',
        data: formData,
        processData: false,
        contentType: false,
        beforeSend: function () {
            toastr.info(`Subiendo ${archivosAdjuntos.length} archivo(s)...`, 'Procesando');
        },
        success: function (response) {
            console.log('Respuesta de subida de archivos:', response);

            // Procesar respuesta para obtener URLs
            let archivosGuardados = [];
            
            try {
                if (Array.isArray(response)) {
                    archivosGuardados = response;
                } else if (response && response.archivos && Array.isArray(response.archivos)) {
                    archivosGuardados = response.archivos;
                } else if (response && response.data && Array.isArray(response.data)) {
                    archivosGuardados = response.data;
                }

                console.log('Archivos guardados:', archivosGuardados);

                // Proceder a actualizar donativo con archivos
                enviarActualizacionDonativo(idDonativo, todosLosArticulos, archivosGuardados);
            } catch (e) {
                console.error('Error al procesar respuesta de archivos:', e);
                toastr.error('Error al procesar los archivos subidos', 'Error');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error al subir archivos:', error);
            console.error('Respuesta:', xhr.responseText);
            toastr.error('Error al subir los archivos', 'Error');
        }
    });
}

/**
 * Envía la actualización del donativo al servidor
 * Estructura del JSON:
 * {
 *   fecha_recepcion, tipo, estatus, observaciones,
 *   detalles: [{ id_donativo_detalle, id_articulo, cantidad_recibida, valor_unitario, estado, descripcion, unidad_medida }],
 *   archivos: [{ id_archivo, url, nombre_original }]
 * }
 */
function enviarActualizacionDonativo(idDonativo, todosLosArticulos, archivosNuevos) {
    // Construir detalles de artículos
    const detalles = todosLosArticulos.map(articulo => ({
        id_donativo_detalle: articulo.id_donativo_detalle || null, // null para nuevos artículos agregados
        id_articulo: articulo.id_articulo,
        cantidad_recibida: articulo.cantidad_recibida,
        valor_unitario: articulo.valor_unitario,
        estado: articulo.estado,
        descripcion: articulo.descripcion,
        unidad_medida: articulo.unidad_medida
    }));

    // Construir archivos (incluir existentes + nuevos)
    const archivos = [];
    
    // Agregar archivos nuevos si los hay
    if (archivosNuevos && archivosNuevos.length > 0) {
        archivosNuevos.forEach(archivo => {
            archivos.push({
                id_archivo: null,
                url: archivo.url,
                nombre_original: archivo.nombre_original
            });
        });
    }

    // Construir el JSON completo para actualización
    const donativoActualizado = {
        fecha_recepcion: $('#dateRecepcion').val() + 'T10:30:00',
        tipo: $('#selectTipoDonativo').val(),
        estatus: 'Recibido', // O mantener el estatus actual
        observaciones: $('#textObservacionesDonativo').val() || '',
        detalles: detalles,
        archivos: archivos
    };

    const datosString = JSON.stringify(donativoActualizado);

    console.log('Actualizando donativo ID:', idDonativo);
    console.log('Datos a enviar:', donativoActualizado);

    $.ajax({
        type: 'POST',
        url: 'actualizaDonativo',
        data: { 
            id_donativo: idDonativo,
            datos: datosString 
        },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Actualizando donativo...', 'Procesando');
        },
        success: function (response) {
            console.log('Respuesta al actualizar donativo:', response);

            if (response.exito) {
                // Mostrar SweetAlert de éxito
                Swal.fire({
                    icon: 'success',
                    title: '¡Donativo Actualizado!',
                    text: 'El donativo se ha actualizado correctamente',
                    confirmButtonColor: '#9c223c',
                    confirmButtonText: 'Aceptar'
                }).then((result) => {
                    if (result.isConfirmed) {
                        // Salir del modo edición y limpiar formulario
                        salirModoEdicion();
                        limpiarFormulario();
                    }
                });

                toastr.success('Donativo actualizado correctamente', 'Éxito');
            } else {
                toastr.error(response.mensaje || 'Error al actualizar', 'Error');
            }   
        },
        error: function (xhr, status, error) {
            console.error('Error al actualizar donativo:', error);
            console.error('Respuesta:', xhr.responseText);

            if (xhr.status === 400) {
                toastr.error('Datos de donativo inválidos. Por favor revisa los campos.', 'Error');
            } else if (xhr.status === 500) {
                toastr.error('Error en el servidor. Intenta de nuevo.', 'Error');
            } else {
                toastr.error('Error al actualizar el donativo: ' + error, 'Error');
            }
        }
    });
}

/**
 * Evento personalizado cuando la vista de registros se carga
 */
document.addEventListener('registrosViewLoaded', function () {
    // console.log('=== registrosViewLoaded evento disparado ===');

    setTimeout(function () {
        // console.log('Iniciando cargarDonativos después de registrosViewLoaded');
        cargarDonativos();
        cargarAlmacenes();

        // Botón refrescar
        const btnRefrescar = document.getElementById('btnRefrescarDonativos');
        if (btnRefrescar) {
            // console.log('✓ Botón refrescar encontrado, agregando listener');
            btnRefrescar.addEventListener('click', function () {
                // console.log('Botón refrescar clickeado');
                cargarDonativos();
            });
        } else {
            console.warn('⚠ Botón refrescar NO encontrado');
        }
    }, 200);
});

//// Botón refrescar
//document.getElementById('btnRefrescarDonativos').addEventListener('click', function () {
//    cargarDonativos();
//});

// Inicialización para cuando se carga el DOM por primera vez
document.addEventListener('DOMContentLoaded', function () {
    // console.log('=== DOMContentLoaded ===');

    // Si acciden directamente a la página de registros
    const tabla = document.getElementById('tablaDonativos');
    if (tabla) {
        // console.log('Tabla encontrada en carga inicial, disparando evento');
        document.dispatchEvent(new Event('registrosViewLoaded'));
    }
});

/**
 * Obtiene el color del badge según el estatus
 */
function obtenerColorEstatus(estatus) {
    // Usar clases estándar de Bootstrap con nombres correctos
    const colores = {
        'Recibido': 'bg-success',
        'recibido': 'bg-success',
        'Pendiente': 'bg-warning',
        'pendiente': 'bg-warning',
        'Cancelado': 'bg-danger',
        'cancelado': 'bg-danger',
        'Rechazado': 'bg-dark',
        'rechazado': 'bg-dark'
    };
    return colores[estatus] || 'bg-secondary';
}


/**
 * Obtiene el color del badge según el estado del artículo
 */
function obtenerColorEstado(estado) {
    const colores = {
        'nuevo': 'badge-success',
        'Nuevo': 'badge-success',
        'usado_bueno': 'badge-info',
        'Usado - Buen estado': 'badge-info',
        'usado_regular': 'badge-warning',
        'Usado - Estado regular': 'badge-warning',
        'reparado': 'badge-primary',
        'Reparado': 'badge-primary',
        'otro': 'badge-secondary',
        'Otro': 'badge-secondary'
    };
    return colores[estado] || 'badge-secondary';
}

/**
 * Editar un donativo
 * Obtiene los datos del donativo y los carga en el formulario
 */
function editarDonativo(idDonativo) {
    // console.log('Editando donativo ID:', idDonativo);

    $.ajax({
        type: 'GET',
        url: 'obtenerDonativo',
        data: { id_donativo: idDonativo },
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Cargando donativo para edición...', 'Procesando');
        },
        success: function (response) {
            // console.log('Donativo obtenido para edición:', response);

            // Cargar los datos en el formulario
            cargarDonativoEnFormulario(JSON.parse(response));

            // Hacer scroll al formulario
            document.getElementById('formRegistroDonativos').scrollIntoView({ behavior: 'smooth', block: 'start' });

            toastr.success('Donativo cargado para editar', 'Éxito');
        },
        error: function (xhr, status, error) {
            console.error('Error al obtener donativo para editar:', error);
            console.error('Respuesta:', xhr.responseText);
            toastr.error('Error al cargar el donativo para edición', 'Error');
        }
    });
}

/**
 * Carga los datos del donativo en el formulario para edición
 */
function cargarDonativoEnFormulario(donativo) {
    // Limpiar formulario primero
    const form = document.getElementById('formRegistroDonativos');
    form.reset();
    form.classList.remove('was-validated');

    // Limpiar artículos previos
    $('#contenedorArticulos').empty();
    numeroArticulos = 0;

    // Activar modo edición
    activarModoEdicion(donativo.id_donativo);

    // Información del donador
    console.log('Cargando información del donador:', donativo.donador);

    $('#id_donador').val(donativo.donador?.id_donador || '');
    $('#selectTipoDonador').val(donativo.donador?.tipo || '');
    $('#textNombreDonador').val(donativo.donador?.nombre || '');
    $('#textRFC').val(donativo.donador?.rfc || '');
    $('#textTelefonoDonador').val(donativo.donador?.telefono || '');
    $('#textEmailDonador').val(donativo.donador?.email || '');
    $('#textDireccionDonador').val(donativo.donador?.direccion || '');
    $('#textColoniaDonador').val(donativo.donador?.colonia || '');
    $('#textCiudadDonador').val(donativo.donador?.ciudad || '');
    $('#textEstadoDonador').val(donativo.donador?.estado || '');
    $('#textCodigoPostalDonador').val(donativo.donador?.codigo_postal || '');
    $('#textNotasDonador').val(donativo.donador?.notas || '');

    // Indicar que fue seleccionado
    $('#textNombreDonador').css('background-color', '#e8f5e9').prop('readonly', true);

    // Información del contacto (si aplica)
    if (donativo.donador?.tipo === 'organizacion' || donativo.donador?.tipo === 'empresa' || donativo.donador?.tipo === 'gobierno') {
        if (donativo.contacto) {
            console.log('Cargando información de contacto:', donativo.contacto);

            $('#textNombreContacto').val(donativo.contacto.nombre || '');
            $('#textPuestoContacto').val(donativo.contacto.puesto || '');
            $('#textTelefonoContacto').val(donativo.contacto.telefono || '');
            $('#textExtensionContacto').val(donativo.contacto.extension || '');
            $('#textCelularContacto').val(donativo.contacto.celular || '');
            $('#textEmailContacto').val(donativo.contacto.email || '');
            $('#textHorarioContacto').val(donativo.contacto.horario_contacto || '');
            $('#selectMedioPreferido').val(donativo.contacto.medio_preferido || '');
            $('#checkEsPrincipal').prop('checked', donativo.contacto.es_principal === true || donativo.contacto.es_principal === 1);
            $('#checkActivoContacto').prop('checked', donativo.contacto.activo === true || donativo.contacto.activo === 1);
            $('#textNotasContacto').val(donativo.contacto.notas || '');
        }

        // Mostrar sección de contacto
        $('#seccionContacto').removeClass('hidden');
    }

    // Información del donativo
    console.log('Cargando información del donativo');

    $('#dateRecepcion').val(donativo.fecha_recepcion ? donativo.fecha_recepcion.split('T')[0] : obtenerFechaHoy());
    $('#selectTipoDonativo').val(donativo.tipo.toLowerCase() || 'especie');
    $('#selectAlmacenDestino').val(donativo.id_almacen || centroIDUsuario.value);
    $('#textObservacionesDonativo').val(donativo.observaciones || '');

    // Si es efectivo, mostrar el campo de importe
    if (donativo.tipo === 'efectivo') {
        $('#grupoImporte').show();
    }

    // Cargar artículos
    console.log('Cargando artículos:', donativo.detalles);

    if (donativo.detalles && donativo.detalles.length > 0) {
        donativo.detalles.forEach((detalle, index) => {
            numeroArticulos++;
            agregarArticulo(numeroArticulos);

            const $articulo = $(`[data-articulo="${numeroArticulos}"]`);

            // Llenar datos del artículo
            $articulo.find('.input-articulo-nombre').val(detalle.nombre_articulo || '').prop('readonly', true).css('background-color', '#e8f5e9');
            $articulo.find('.input-articulo-descripcion').val(detalle.descripcion || '');
            $articulo.find('.input-articulo-unidad').val(detalle.unidad_medida.toLowerCase() || '');
            $articulo.find('.input-articulo-categoria-select').val(detalle.categoria || '');
            $articulo.find('.input-articulo-cantidad').val(detalle.cantidad_recibida || 0);
            $articulo.find('.input-articulo-valor').val(parseFloat(detalle.valor_unitario || 0).toFixed(2));
            $articulo.find('.input-articulo-estado').val(detalle.estado.toLowerCase() || '');

            // ⭐ IMPORTANTE: Guardar el ID del artículo Y el ID del detalle del donativo
            $articulo.data('id_articulo', detalle.id_articulo);
            $articulo.data('id_donativo_detalle', detalle.id_donativo_detalle);
            
            // Log para verificar que se guardó correctamente
            console.log(`✓ Artículo #${numeroArticulos}:`, {
                nombre: detalle.nombre_articulo,
                id_articulo: detalle.id_articulo,
                id_donativo_detalle: detalle.id_donativo_detalle
            });
        });
    }

    console.log('✓ Donativo cargado en el formulario correctamente para edición');
    toastr.success('Donativo cargado para editar. Modifica los campos necesarios.', 'Modo Edición');
}

/**
 * Elimina el donativo dado su ID
 */
function eliminarDonativo(idDonativo) {
    Swal.fire({
        title: '¿Eliminar donativo?',
        text: 'Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            // console.log('Eliminando donativo ID:', idDonativo);

            $.ajax({
                type: 'GET',
                url: 'EliminarDonativo',
                data: { id_donativo: idDonativo },
                dataType: 'json',
                beforeSend: function () {
                    toastr.info('Eliminando donativo...', 'Procesando');
                },
                success: function (response) {
                    // console.log('Donativo eliminado:', response);
                    toastr.success('Donativo eliminado correctamente', 'Éxito');
                    cargarDonativos(); // Refrescar tabla
                },
                error: function (xhr, status, error) {
                    console.error('Error al eliminar donativo:', error);
                    toastr.error('Error al eliminar el donativo', 'Error');
                }
            });
        }
    });
}

/**
 * Actualiza un donativo existente
 * Construye el JSON con la estructura requerida por el endpoint actualizaDonativo
 */
function actualizarDonativo(idDonativo) {
    const form = document.getElementById('formRegistroDonativos');

    // Validar que al menos haya un artículo
    if ($('#contenedorArticulos').find('.articulo-item').length === 0) {
        toastr.warning('Agrega al menos un artículo', 'Validación');
        return;
    }

    // Validar el formulario
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        toastr.error('Por favor completa todos los campos requeridos', 'Validación');
        return;
    }

    // Separar artículos nuevos de existentes
    const articulosNuevos = [];
    const articulosExistentes = [];

    $('#contenedorArticulos').find('.articulo-item').each(function () {
        const $item = $(this);
        const idArticulo = $item.data('id_articulo');
        const idDonativoDetalle = $item.data('id_donativo_detalle');

        const articulo = {
            nombre: $item.find('.input-articulo-nombre').val(),
            unidad_medida: $item.find('.input-articulo-unidad').val(),
            id_categoria: parseInt($item.find('.input-articulo-categoria-select').val()),
            descripcion: $item.find('.input-articulo-descripcion').val(),
            // Datos del detalle
            cantidad_recibida: parseInt($item.find('.input-articulo-cantidad').val()) || 0,
            valor_unitario: parseFloat($item.find('.input-articulo-valor').val()) || 0,
            estado: $item.find('.input-articulo-estado').val(),
            id_donativo_detalle: idDonativoDetalle || null
        };

        // Si tiene id_articulo, es un artículo existente
        if (idArticulo) {
            articulo.id_articulo = idArticulo;
            articulosExistentes.push(articulo);
        } else {
            // Si no tiene id_articulo, es un artículo nuevo que hay que guardar primero
            articulosNuevos.push(articulo);
        }
    });

    console.log('Artículos nuevos a guardar:', articulosNuevos);
    console.log('Artículos existentes:', articulosExistentes);

    // Si hay artículos nuevos, guardarlos primero
    if (articulosNuevos.length > 0) {
        insertarArticulosNuevosParaActualizacion(articulosNuevos, articulosExistentes, idDonativo);
    } else {
        // Si no hay artículos nuevos, proceder directamente con la actualización
        if (archivosAdjuntos.length > 0) {
            console.log('Subiendo nuevos archivos antes de actualizar...');
            subirArchivosParaActualizacion(idDonativo, articulosExistentes);
        } else {
            enviarActualizacionDonativo(idDonativo, articulosExistentes, []);
        }
    }
}

/**
 * Inserta artículos nuevos antes de actualizar el donativo
 */
function insertarArticulosNuevosParaActualizacion(articulosNuevos, articulosExistentes, idDonativo) {
    // Filtrar solo los campos necesarios para insertar artículos
    const articulosParaInsertar = articulosNuevos.map(articulo => ({
        nombre: articulo.nombre,      
        id_categoria: articulo.id_categoria
    }));

    const datosNuevos = JSON.stringify(articulosParaInsertar);

    $.ajax({
        type: 'POST',
        url: 'insertaArticulos',
        data: { datos: datosNuevos },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando nuevos artículos...', 'Procesando');
        },
        success: function (response) {
            console.log('Respuesta al guardar artículos nuevos:', response);

            // Extraer los IDs de los artículos insertados
            let articulosInsertadosConId = [];
            try {
                // El endpoint retorna un objeto con respuesta como string JSON
                if (response && response.respuesta) {
                    try {
                        const articuloInsertado = JSON.parse(response.respuesta);
                        console.log('Artículo insertado parseado:', articuloInsertado);
                        
                        if (articuloInsertado && articuloInsertado.id_articulo) {
                            // Buscar el artículo original y agregarle el ID
                            const articuloOriginal = articulosNuevos.find(a => 
                                a.nombre === articuloInsertado.nombre && 
                                a.id_categoria === articuloInsertado.id_categoria
                            );
                            
                            if (articuloOriginal) {
                                articulosInsertadosConId.push({
                                    ...articuloOriginal,
                                    id_articulo: articuloInsertado.id_articulo
                                });
                                console.log('Artículo emparejado con ID:', articuloInsertado.id_articulo);
                            }
                        }
                    } catch (parseError) {
                        console.error('Error al parsear respuesta:', parseError);
                    }
                } else if (response && response.resultados && Array.isArray(response.resultados)) {
                    // Estructura alternativa con array de resultados
                    articulosInsertadosConId = response.resultados
                        .map((resultado, index) => {
                            console.log(`Procesando resultado ${index}:`, resultado);
                            
                            let idArticulo = null;
                            

                            if (resultado.id_articulo) {
                                idArticulo = resultado.id_articulo;
                            } else if (resultado.id) {
                                idArticulo = resultado.id;
                            } else if (resultado.respuesta) {
                                const parsed = JSON.parse(resultado.respuesta);
                                idArticulo = parsed.id_articulo;
                            }
                            

                            return {
                                ...articulosNuevos[index],
                                id_articulo: idArticulo
                            };
                        })
                        .filter(articulo => articulo.id_articulo !== null && articulo.id_articulo !== undefined);
                } else {
                    console.warn('Respuesta sin estructura esperada:', response);
                    articulosInsertadosConId = articulosNuevos;
                }
            } catch (e) {
                console.error('Error al procesar artículos insertados:', e);
                console.error('Response completa:', response);
                toastr.error('Error al procesar artículos insertados', 'Error');
                return;
            }

            // Validar que se obtuvieron IDs
            if (articulosInsertadosConId.length === 0) {
                console.warn('⚠ No se pudieron extraer los IDs de los artículos insertados');
                toastr.warning('Se insertaron artículos pero no se pudo obtener sus IDs', 'Advertencia');
                // Continuar igualmente con los artículos sin IDs
                articulosInsertadosConId = articulosNuevos;
            }

            console.log('Artículos nuevos con IDs:', articulosInsertadosConId);

            // Combinar artículos insertados con existentes
            const todosLosArticulos = [...articulosInsertadosConId, ...articulosExistentes];

            // Proceder con archivos o actualización directa
            if (archivosAdjuntos.length > 0) {
                console.log('Subiendo nuevos archivos antes de actualizar...');
                subirArchivosParaActualizacion(idDonativo, todosLosArticulos);
            } else {
                enviarActualizacionDonativo(idDonativo, todosLosArticulos, []);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar artículos nuevos:', error);
            console.error('Status:', status);
            console.error('Respuesta completa:', xhr.responseText);
            toastr.error('Error al guardar los artículos nuevos. Intenta de nuevo.', 'Error');
        }
    });
}

/**
 * Sube archivos nuevos para la actualización
 */
function subirArchivosParaActualizacion(idDonativo, todosLosArticulos) {
    const formData = new FormData();
    
    // Agregar cada archivo al FormData
    archivosAdjuntos.forEach((archivoObj, index) => {
        formData.append(`archivos`, archivoObj.file);
        formData.append(`nombres_originales`, archivoObj.nombre);
    });

    $.ajax({
        type: 'POST',
        url: 'GuardarArchivos',
        data: formData,
        processData: false,
        contentType: false,
        beforeSend: function () {
            toastr.info(`Subiendo ${archivosAdjuntos.length} archivo(s)...`, 'Procesando');
        },
        success: function (response) {
            console.log('Respuesta de subida de archivos:', response);

            // Procesar respuesta para obtener URLs
            let archivosGuardados = [];
            
            try {
                if (Array.isArray(response)) {
                    archivosGuardados = response;
                } else if (response && response.archivos && Array.isArray(response.archivos)) {
                    archivosGuardados = response.archivos;
                } else if (response && response.data && Array.isArray(response.data)) {
                    archivosGuardados = response.data;
                }

                console.log('Archivos guardados:', archivosGuardados);

                // Proceder a actualizar donativo con archivos
                enviarActualizacionDonativo(idDonativo, todosLosArticulos, archivosGuardados);
            } catch (e) {
                console.error('Error al procesar respuesta de archivos:', e);
                toastr.error('Error al procesar los archivos subidos', 'Error');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error al subir archivos:', error);
            console.error('Respuesta:', xhr.responseText);
            toastr.error('Error al subir los archivos', 'Error');
        }
    });
}

/**
 * Envía la actualización del donativo al servidor
 * Estructura del JSON:
 * {
 *   fecha_recepcion, tipo, estatus, observaciones,
 *   detalles: [{ id_donativo_detalle, id_articulo, cantidad_recibida, valor_unitario, estado, descripcion, unidad_medida }],
 *   archivos: [{ id_archivo, url, nombre_original }]
 * }
 */
function enviarActualizacionDonativo(idDonativo, todosLosArticulos, archivosNuevos) {
    // Construir detalles de artículos
    const detalles = todosLosArticulos.map(articulo => ({
        id_donativo_detalle: articulo.id_donativo_detalle || null, // null para nuevos artículos agregados
        id_articulo: articulo.id_articulo,
        cantidad_recibida: articulo.cantidad_recibida,
        valor_unitario: articulo.valor_unitario,
        estado: articulo.estado,
        descripcion: articulo.descripcion,
        unidad_medida: articulo.unidad_medida
    }));

    // Construir archivos (incluir existentes + nuevos)
    const archivos = [];
    
    // Agregar archivos nuevos si los hay
    if (archivosNuevos && archivosNuevos.length > 0) {
        archivosNuevos.forEach(archivo => {
            archivos.push({
                id_archivo: null,
                url: archivo.url,
                nombre_original: archivo.nombre_original
            });
        });
    }

    // Construir el JSON completo para actualización
    const donativoActualizado = {
        fecha_recepcion: $('#dateRecepcion').val() + 'T10:30:00',
        tipo: $('#selectTipoDonativo').val(),
        estatus: 'Recibido', // O mantener el estatus actual
        observaciones: $('#textObservacionesDonativo').val() || '',
        detalles: detalles,
        archivos: archivos
    };

    const datosString = JSON.stringify(donativoActualizado);

    console.log('Actualizando donativo ID:', idDonativo);
    console.log('Datos a enviar:', donativoActualizado);

    $.ajax({
        type: 'POST',
        url: 'actualizaDonativo',
        data: { 
            id_donativo: idDonativo,
            datos: datosString 
        },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Actualizando donativo...', 'Procesando');
        },
        success: function (response) {
            console.log('Respuesta al actualizar donativo:', response);

            if (response.exito) {
                // Mostrar SweetAlert de éxito
                Swal.fire({
                    icon: 'success',
                    title: '¡Donativo Actualizado!',
                    text: 'El donativo se ha actualizado correctamente',
                    confirmButtonColor: '#9c223c',
                    confirmButtonText: 'Aceptar'
                }).then((result) => {
                    if (result.isConfirmed) {
                        // Salir del modo edición y limpiar formulario
                        salirModoEdicion();
                        limpiarFormulario();
                    }
                });

                toastr.success('Donativo actualizado correctamente', 'Éxito');
            } else {
                toastr.error(response.mensaje || 'Error al actualizar', 'Error');
            }   
        },
        error: function (xhr, status, error) {
            console.error('Error al actualizar donativo:', error);
            console.error('Respuesta:', xhr.responseText);

            if (xhr.status === 400) {
                toastr.error('Datos de donativo inválidos. Por favor revisa los campos.', 'Error');
            } else if (xhr.status === 500) {
                toastr.error('Error en el servidor. Intenta de nuevo.', 'Error');
            } else {
                toastr.error('Error al actualizar el donativo: ' + error, 'Error');
            }
        }
    });
}

/**
 * Activa el modo edición del formulario
 */
function activarModoEdicion(idDonativo) {
    modoEdicion = true;
    idDonativoEdicion = idDonativo;
    
    // Cambiar el botón de "Guardar" a "Actualizar"
    const btnGuardar = $('#formRegistroDonativos').find('button[type="submit"]');
    btnGuardar.html('<i class="fas fa-save"></i> Actualizar Donativo');
    btnGuardar.removeClass('btn-primary').addClass('btn-warning');
    
    // Agregar badge indicando modo edición
    const formularioHeader = $('#formRegistroDonativos').find('.card-header, h4').first();
    if (formularioHeader.find('.badge-modo-edicion').length === 0) {
        formularioHeader.append(' <span class="badge badge-warning badge-modo-edicion">Modo Edición</span>');
    }
    
    console.log('✓ Modo edición activado para donativo ID:', idDonativo);
}

/**
 * Desactiva el modo edición y restaura el formulario
 */
function salirModoEdicion() {
    modoEdicion = false;
    idDonativoEdicion = null;
    
    // Restaurar el botón a "Guardar"
    const btnGuardar = $('#formRegistroDonativos').find('button[type="submit"]');
    btnGuardar.html('<i class="fas fa-save"></i> Guardar Donativo');
    btnGuardar.removeClass('btn-warning').addClass('btn-primary');
    
    // Remover badge de modo edición
    $('.badge-modo-edicion').remove();
    
    // Habilitar campos de donador nuevamente
    $('#textNombreDonador').prop('readonly', false);
    
    console.log('✓ Modo edición desactivado');
}

/**
 * Muestra los detalles completos de un donativo en un modal
 * Incluye información del donador, artículos y archivos adjuntos
 */
function verDetalles(idDonativo) {
    console.log('Cargando detalles del donativo ID:', idDonativo);

    $.ajax({
        type: 'GET',
        url: 'obtenerDonativoDetalle',
        data: { id_donativo: idDonativo },
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Cargando detalles...', 'Procesando');
        },
        success: function (response) {
            console.log('Detalles recibidos:', response);

            let datosCompletos = null;

            // Parsear respuesta
            if (typeof response === 'string') {
                try {
                    datosCompletos = JSON.parse(response);
                } catch (e) {
                    console.error('Error al parsear respuesta:', e);
                    toastr.error('Error al procesar los detalles', 'Error');
                    return;
                }
            } else {
                datosCompletos = response;
            }

            // Validar estructura
            if (!datosCompletos || !datosCompletos.detalles) {
                console.error('Formato de respuesta inesperado:', datosCompletos);
                toastr.error('Formato de detalles inesperado', 'Error');
                return;
            }

            // Construir HTML del modal con la nueva estructura
            mostrarModalDetalles(datosCompletos.detalles, datosCompletos.archivos || [], idDonativo);
        },
        error: function (xhr, status, error) {
            console.error('Error al cargar detalles:', error);
            toastr.error('Error al cargar los detalles del donativo', 'Error');
        }
    });
}

/**
 * Construye y muestra el contenido del modal de detalles
 * @param {Array} detalles - Array de artículos donados
 * @param {Array} archivos - Array de archivos adjuntos del donativo
 * @param {number} idDonativo - ID del donativo
 */
function mostrarModalDetalles(detalles, archivos, idDonativo) {
    if (!detalles || detalles.length === 0) {
        $('#contenidoDetalles').html(`
            <div class="alert alert-warning text-center">
                <i class="fas fa-exclamation-triangle"></i>
                No se encontraron detalles para este donativo
            </div>
        `);
        $('#modalDetallesDonativo').modal('show');
        return;
    }

    let html = '<div class="container-fluid">';

    // Sección de artículos donados
    html += '<h4 class="mb-4"><i class="fas fa-box-open"></i> Artículos Donados</h4>';

    detalles.forEach((detalle, index) => {
        const valorTotal = (detalle.cantidad_recibida * detalle.valor_unitario).toFixed(2);

        html += `
            <div class="card mb-4 shadow-sm">
                <div class="card-header" style="background-color: var(--guinda); color: white;">
                    <h5 class="mb-0">
                        <i class="fas fa-box"></i> ${detalle.nombre_articulo}
                        <span class="badge badge-light float-right">${detalle.categoria_articulo}</span>
                    </h5>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-6">
                            <p class="mb-2"><strong>Categoría:</strong> ${detalle.categoria_articulo}</p>
                            <p class="mb-2"><strong>Unidad de Medida:</strong> ${detalle.unidad_medida}</p>
                            <p class="mb-2"><strong>Estado:</strong> <span class="badge badge-info">${detalle.estado}</span></p>
                        </div>
                        <div class="col-md-6">
                            <p class="mb-2"><strong>Cantidad:</strong> ${detalle.cantidad_recibida}</p>
                            <p class="mb-2"><strong>Valor Unitario:</strong> $${parseFloat(detalle.valor_unitario).toFixed(2)}</p>
                            <p class="mb-2"><strong>Valor Total:</strong> $${valorTotal}</p>
                        </div>
                    </div>
                </div>
            </div>`;
    });

    // Sección de archivos adjuntos (común para todo el donativo)
    if (archivos && archivos.length > 0) {
        html += `
            <hr class="my-4">
            <h4 class="mb-4"><i class="fas fa-paperclip"></i> Archivos Adjuntos del Donativo (${archivos.length})</h4>
            <div class="row">`;

        archivos.forEach((archivo) => {
            const extension = archivo.nombre_archivo.split('.').pop().toLowerCase();
            const iconoArchivo = obtenerIconoArchivo(extension);
            const urlCompleta = `/Registros/ObtenerArchivo?archivo=${encodeURIComponent(archivo.url_archivo)}`;

            html += `
                <div class="col-md-6 col-lg-4 mb-3">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body text-center">
                            <i class="${iconoArchivo} fa-3x mb-3" style="color: var(--dorado);"></i>
                            <h6 class="card-title text-truncate" title="${archivo.nombre_archivo}">
                                ${archivo.nombre_archivo}
                            </h6>
                            <small class="text-muted">ID: ${archivo.id_archivo}</small>
                            <div class="mt-3">
                                ${extension === 'jpg' || extension === 'jpeg' || extension === 'png' || extension === 'gif'
                    ? `<button type="button" class="btn btn-sm btn-info mb-2" onclick="visualizarArchivo('${urlCompleta}', '${archivo.nombre_archivo}', 'imagen')">
                                        <i class="fas fa-eye"></i> Ver
                                       </button>`
                    : extension === 'pdf'
                        ? `<button type="button" class="btn btn-sm btn-info mb-2" onclick="visualizarArchivo('${urlCompleta}', '${archivo.nombre_archivo}', 'pdf')">
                                            <i class="fas fa-eye"></i> Ver
                                           </button>`
                        : ''}
                                <a href="${urlCompleta}" download="${archivo.nombre_archivo}" class="btn btn-sm btn-success mb-2">
                                    <i class="fas fa-download"></i> Descargar
                                </a>
                            </div>
                        </div>
                    </div>
                </div>`;
        });

        html += `
            </div>`;
    } else {
        html += `
            <hr class="my-4">
            <div class="alert alert-info text-center">
                <i class="fas fa-info-circle"></i> Este donativo no tiene archivos adjuntos
            </div>`;
    }

    html += '</div>';

    $('#contenidoDetalles').html(html);
    $('#modalDetallesDonativo').modal('show');
}

/**
 * Obtiene el icono de FontAwesome según la extensión del archivo
 */
function obtenerIconoArchivo(extension) {
    const iconos = {
        'pdf': 'fas fa-file-pdf text-danger',
        'doc': 'fas fa-file-word text-primary',
        'docx': 'fas fa-file-word text-primary',
        'xls': 'fas fa-file-excel text-success',
        'xlsx': 'fas fa-file-excel text-success',
        'jpg': 'fas fa-file-image text-info',
        'jpeg': 'fas fa-file-image text-info',
        'png': 'fas fa-file-image text-info',
        'gif': 'fas fa-file-image text-info',
        'zip': 'fas fa-file-archive text-warning',
        'rar': 'fas fa-file-archive text-warning'
    };

    return iconos[extension] || 'fas fa-file text-secondary';
}

/**
 * Visualiza un archivo en un modal (imágenes o PDFs)
 * @param {string} url - URL del archivo
 * @param {string} nombreArchivo - Nombre original del archivo
 * @param {string} tipo - Tipo de archivo ('imagen' o 'pdf')
 */
function visualizarArchivo(url, nombreArchivo, tipo) {
    let contenido = '';

    if (tipo === 'imagen') {
        contenido = `
            <div class="text-center">
                <img src="${url}" class="img-fluid rounded" alt="${nombreArchivo}" style="max-height: 70vh;">
            </div>`;
    } else if (tipo === 'pdf') {
        contenido = `
            <embed src="${url}" type="application/pdf" width="100%" height="600px" class="rounded">
            <p class="text-center mt-3">
                Si no puedes ver el PDF, 
                <a href="${url}" download="${nombreArchivo}" class="btn btn-sm btn-primary">
                    <i class="fas fa-download"></i> Descárgalo aquí
                </a>
            </p>`;
    }

    Swal.fire({
        title: nombreArchivo,
        html: contenido,
        width: '90%',
        showCloseButton: true,
        showConfirmButton: false,
        customClass: {
            container: 'archivo-preview-modal'
        }
    });
}