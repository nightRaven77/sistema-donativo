// Variable global para contar artículos
let numeroArticulos = 0;
let donadorSeleccionado = null;

//TODO: Ocultar el mensaje de donador no encontrado cuando no existe texto en el input

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
    { id: 21, nombre: 'SUEROS, HIDRATANTES Y ENERGIZANTES' }
];

/**
 * Obtiene la fecha de hoy en formato YYYY-MM-DD
 */
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
    console.log('Inicializando formulario de registros...');

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

    console.log('Formulario encontrado, vinculando eventos...');

    // Remover eventos previos para evitar duplicados
    $('#selectTipoDonador').off('change');
    $('#textNombreDonador').off('input').off('focus');
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
        guardarRegistro();
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
        $('#textNombreDonador').css('background-color', '').prop('readonly', false);
        numeroArticulos = 0;
        donadorSeleccionado = null;
        toastr.info('Formulario limpiado', 'Operación realizada');
    });

    // Establecer fecha de recepción a la fecha de hoy
    $('#dateRecepcion').val(obtenerFechaHoy());

    // Resetear contador de artículos
    numeroArticulos = 0;
    donadorSeleccionado = null;
    console.log('Formulario inicializado correctamente');
}

/**
 * Busca donadores por nombre
 */
function buscarDonadores(termino) {
    const listaDonadores = $('#listaDonadores');

    if (termino.length < 1) {
        listaDonadores.addClass('d-none');
        return;
    }

    $.ajax({
        type: 'GET',
        url: '/Registros/ObtenerDonadores',
        data: { search: termino },
        dataType: 'json',
        success: function (donadores) {
            console.log('Donadores recibidos:', donadores);
            mostrarResultadosDonadores(donadores, termino);
        },
        error: function (xhr, status, error) {
            console.error('Error al buscar donadores:', error);

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

    // Validar que sea un array
    if (!Array.isArray(donadores)) {
        console.warn('donadores no es un array:', donadores);
        donadores = [];
    }

    if (!donadores || donadores.length === 0) {
        listaDonadores.html(`
            <div class="p-3 text-center text-stone-500">
                <i class="fas fa-inbox"></i>
                <p>No se encontraron donadores</p>
                <small>Puedes crear uno nuevo escribiendo el nombre</small>
            </div>
        `);
        listaDonadores.removeClass('d-none');
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
            <div class="donador-opcion p-3 border-bottom border-stone-200 cursor-pointer hover:bg-stone-50" data-id="${donador.id_donador || ''}" data-donador='${donadorJSON}'>
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
            <div class="p-3 text-center text-stone-500">
                <i class="fas fa-inbox"></i>
                <p>Sin resultados válidos</p>
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
        url: '/Registros/ObtenerArticulos',
        data: { search: termino },
        dataType: 'json',
        success: function (articulos) {
            console.log('Artículos recibidos:', articulos);
            mostrarResultadosArticulosPorNumero(articulos, termino, numeroArticulo);
        },
        error: function (xhr, status, error) {
            console.error('Error al buscar artículos:', error);
            const listaArticulos = $(`.lista-articulos-${numeroArticulo}`);
            listaArticulos.html(`
                <div class="p-3 text-center text-stone-500">
                    <i class="fas fa-exclamation-circle"></i>
                    <p>Error al buscar artículos</p>
                    <small>${error}</small>
                </div>
            `);
            listaArticulos.removeClass('d-none');
        }
    });
}

/**
 * Muestra los resultados de artículos para un número específico
 */
function mostrarResultadosArticulosPorNumero(articulos, termo, numeroArticulo) {
    const listaArticulos = $(`.lista-articulos-${numeroArticulo}`);

    // Validar que sea un array
    if (!Array.isArray(articulos)) {
        console.warn('articulos no es un array:', articulos);
        articulos = [];
    }

    if (!articulos || articulos.length === 0) {
        listaArticulos.html(`
            <div class="p-3 text-center text-stone-500">
                <i class="fas fa-inbox"></i>
                <p>No se encontraron artículos</p>
                <small>Podes crear uno nuevo escribiendo el nombre</small>
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
            <div class="articulo-opcion-${numeroArticulo} p-3 border-bottom border-stone-200 cursor-pointer hover:bg-stone-50" data-id="${articulo.id_articulo || ''}" data-articulo='${articuloJSON}'>
                <div class="font-bold text-stone-800">${articulo.nombre}</div>
                <small class="text-stone-500">
                    <i class="fas fa-box"></i> ${categoriaNombre} | 
                    <i class="fas fa-ruler"></i> ${articulo.unidad_medida || 'N/A'}
                </small>
            </div>
        `;
    });

    if (html === '') {
        listaArticulos.html(`
            <div class="p-3 text-center text-stone-500">
                <i class="fas fa-inbox"></i>
                <p>Sin resultados válidos</p>
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

        console.log('Donador seleccionado con ID:', donador.id_donador);

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
        
        // Mostrar indicador visual de que el donador fue seleccionado
        $('#textNombreDonador').css('background-color', '#e8f5e9').prop('readonly', true);

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
        url: '/Registros/ObtenerContactos',
        data: { id_donador: idDonador },
        dataType: 'json',
        success: function (contactos) {
            console.log('Contactos recibidos:', contactos);
            mostrarResultadosContactos(contactos, idDonador);
        },
        error: function (xhr, status, error) {
            console.error('Error al buscar donadores:', error);

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
        console.log('Contacto seleccionado con ID:', contacto.id_contacto);

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
 */
function seleccionarArticuloEnContenedor($element, numeroArticulo) {
    try {
        const articulo = JSON.parse($element.attr('data-articulo'));

        if (!articulo.id_articulo) {
            toastr.warning('El artículo no tiene un ID válido', 'Validación');
            return;
        }

        console.log('Artículo seleccionado:', articulo);
        console.log('Categoría:', articulo.categoria);
        console.log('Unidad de medida:', articulo.unidad_medida);

        const $contenedor = $(`[data-articulo="${numeroArticulo}"]`);

        // Llenar campos del artículo
        $contenedor.find('.input-articulo-nombre').val(articulo.nombre || '').prop('readonly', true).css('background-color', '#e8f5e9');
        $contenedor.find('.input-articulo-descripcion').val(articulo.descripcion || '');
        
        // Llenar la unidad de medida
        if (articulo.unidad_medida) {
            $contenedor.find('.input-articulo-unidad').val(articulo.unidad_medida);
            console.log('Unidad de medida establecida a:', articulo.unidad_medida);
        }
        
        // Llenar la categoría - buscar por nombre en el objeto anidado
        if (articulo.categoria && articulo.categoria.nombre) {
            const nombreCategoria = articulo.categoria.nombre;
            $contenedor.find('.input-articulo-categoria-select').val(nombreCategoria);
            console.log('Categoría establecida a:', nombreCategoria);
        }

        // Guardar el ID del artículo
        $contenedor.data('id_articulo', articulo.id_articulo);

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

    console.log('Tipo de donador seleccionado:', tipo);

    if (tipo === 'organizacion' || tipo === 'empresa') {
        console.log('Mostrando sección de contacto');

        // Mostrar la sección
        seccionContacto.removeClass('hidden');

        // Marcar campos específicos como requeridos
        $('#textNombreContacto').prop('required', true);
        $('#textPuestoContacto').prop('required', true);
        $('#textCelularContacto').prop('required', true);
        $('#textEmailContacto').prop('required', true);

        buscarContactoDonador(id_donador);
    } else {
        console.log('Ocultando sección de contacto');

        // Ocultar la sección
        seccionContacto.addClass('hidden');

        // Remover validación y limpiar campos
        $('#textNombreContacto').prop('required', false).val('').removeClass('is-invalid');
        $('#textPuestoContacto').prop('required', false).val('').removeClass('is-invalid');
        $('#textTelefonoContacto').val('');
        $('#textExtensionContacto').val('');
        $('#textCelularContacto').prop('required', false).val('').removeClass('is-invalid');
        $('#textEmailContacto').prop('required', false).val('').removeClass('is-invalid');
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
        <div class="articulo-item border border-stone-300 rounded-lg p-4 bg-stone-50" data-articulo="${numero}">
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
                    <div class="lista-articulos-${numero} position-absolute w-100 bg-white border border-stone-300 rounded-lg mt-1 shadow-lg d-none" style="max-height: 300px; overflow-y: auto; z-index: 1000; top: 100%;">
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
                        ${CATEGORIAS.map(cat => `<option value="${cat.nombre}">${cat.nombre}</option>`).join('')}
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
                    Valor Unitario
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
    });

    // Evento para cambio de categoría manual
    $nuevoArticulo.find('.input-articulo-categoria-select').on('change', function () {
        console.log('Categoría seleccionada:', $(this).val());
    });

    // Evento para cambio de estado
    $nuevoArticulo.find('.input-articulo-estado').on('change', function () {
        console.log('Estado seleccionado:', $(this).val());
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
        nombre: $('#textNombreDonador').val(),
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
    if (tipoDonador === 'organizacion' || tipoDonador === 'empresa') {
        contacto = {
            nombre: $('#textNombreContacto').val(),
            puesto: $('#textPuestoContacto').val(),
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

    // Construir objeto Información de Donativo
    const infoDonativo = {
        fecha_recepcion: $('#dateRecepcion').val(),
        tipo_donativo: $('#selectTipoDonativo').val(),
        observaciones: $('#textObservacionesDonativo').val()
    };

    // Construir array de artículos
    const articulos = [];
    $('#contenedorArticulos').find('.articulo-item').each(function () {
        articulos.push({
            nombre: $(this).find('.input-articulo-nombre').val(),
            cantidad: parseInt($(this).find('.input-articulo-cantidad').val()),
            unidad_medida: $(this).find('.input-articulo-unidad').val(),
            categoria: $(this).find('.input-articulo-categoria-select').val(),
            estado: $(this).find('.input-articulo-estado').val(),
            valor_unitario: parseFloat($(this).find('.input-articulo-valor').val()) || 0,
            descripcion: $(this).find('.input-articulo-descripcion').val()
        });
    });

    console.log('Datos del donador:', donador);
    console.log('Datos del contacto:', contacto);
    console.log('Información de donativo:', infoDonativo);
    console.log('Datos de artículos:', articulos);

    // Enviar datos de forma secuencial
    guardarDonador(donador, contacto, infoDonativo, articulos);
}

/**
 * Guarda el donador y procede con contacto y artículos
 */
function guardarDonador(donador, contacto, infoDonativo, articulos) {
    const datosString = JSON.stringify(donador);
    const idDonadorExistente = $('#id_donador').val();

    console.log('ID de donador existente:', idDonadorExistente);

    // Si ya existe un donador (fue seleccionado de la lista), usar su ID
    if (idDonadorExistente && idDonadorExistente !== '') {
        console.log('Usando donador existente con ID:', idDonadorExistente);
        toastr.info('Usando donador existente', 'Información');
        
        // Proceder directamente con contacto o artículos
        if (contacto) {
            contacto.id_donador = parseInt(idDonadorExistente);
            guardarContacto(contacto, infoDonativo, articulos, parseInt(idDonadorExistente));
        } else {
            guardarArticulos(infoDonativo, articulos, parseInt(idDonadorExistente));
        }
        return;
    }

    // Si no existe ID, es un donador nuevo, hay que guardarlo
    console.log('Guardando donador nuevo');
    
    $.ajax({
        type: 'POST',
        url: '/Registros/insertaDonador',
        data: { datos: datosString },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando donador...', 'Procesando');
        },
        success: function (response) {
            console.log('Respuesta al guardar donador:', response);

            // Parsear la respuesta para obtener el ID del donador
            let donadorId = null;
            try {
                const responseData = typeof response === 'string' ? JSON.parse(response) : response;
                donadorId = responseData.id_donador || responseData.id;
                console.log('ID del donador guardado:', donadorId);
            } catch (e) {
                console.warn('No se pudo extraer ID del donador:', e);
                toastr.error('No se pudo obtener el ID del donador guardado', 'Error');
                return;
            }

            // Proceder con contacto si existe
            if (contacto) {
                contacto.id_donador = donadorId;
                guardarContacto(contacto, infoDonativo, articulos, donadorId);
            } else {
                // Si no hay contacto, proceder directamente con artículos
                guardarArticulos(infoDonativo, articulos, donadorId);
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
function guardarContacto(contacto, infoDonativo, articulos, donadorId) {
    const datosString = JSON.stringify(contacto);

    $.ajax({
        type: 'POST',
        url: '/Registros/insertaContactoDonaodor',
        data: { datos: datosString },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando contacto...', 'Procesando');
        },
        success: function (response) {
            console.log('Respuesta al guardar contacto:', response);

            // Proceder con artículos
            guardarArticulos(infoDonativo, articulos, donadorId);
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar contacto:', error);
            toastr.error('Error al guardar el contacto. Intenta de nuevo.', 'Error');
        }
    });
}

/**
 * Guarda los artículos donados
 */
function guardarArticulos(infoDonativo, articulos, donadorId) {
    if (!articulos || articulos.length === 0) {
        toastr.warning('No hay artículos para guardar', 'Validación');
        return;
    }

    // Agregar el ID del donador a cada artículo
    const articulosConDonador = articulos.map(articulo => ({
        ...articulo,
        id_donador: donadorId
    }));

    const datosString = JSON.stringify(articulosConDonador);

    $.ajax({
        type: 'POST',
        url: '/Registros/insertaArticulos',
        data: { datos: datosString },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Guardando artículos...', 'Procesando');
        },
        success: function (response) {
            console.log('Respuesta al guardar artículos:', response);

            // Aquí puedes guardar la información del donativo si es necesario
            console.log('Información de donativo:', infoDonativo);

            // Limpiar formulario
            limpiarFormulario();

            toastr.success('Registro guardado correctamente', 'Éxito');
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar artículos:', error);
            console.error('Respuesta:', xhr.responseText);

            if (xhr.status === 400) {
                toastr.error('Datos de artículos inválidos. Por favor revisa los campos.', 'Error');
            } else if (xhr.status === 500) {
                toastr.error('Error en el servidor. Intenta de nuevo.', 'Error');
            } else {
                toastr.error('Error al guardar los artículos: ' + error, 'Error');
            }
        }
    });
}

/**
 * Limpia el formulario después de guardar
 */
function limpiarFormulario() {
    const form = document.getElementById('formRegistroDonativos');

    // Limpiar sección de donador
    form.reset();
    form.classList.remove('was-validated');

    // Resetear el input hidden del ID
    $('#id_donador').val('');
    $('#textNombreDonador').css('background-color', '').prop('readonly', false);

    // Establecer fecha de recepción a la fecha de hoy
    $('#dateRecepcion').val(obtenerFechaHoy());

    // Limpiar sección de contacto
    $('#seccionContacto').addClass('hidden');
    $('#textNombreContacto').prop('required', false);
    $('#textPuestoContacto').prop('required', false);
    $('#textCelularContacto').prop('required', false);
    $('#textEmailContacto').prop('required', false);

    // Limpiar artículos
    $('#contenedorArticulos').empty();
    numeroArticulos = 0;
    donadorSeleccionado = null;

    // Ocultar lista de donadores
    $('#listaDonadores').addClass('d-none');

    console.log('Formulario limpiado correctamente');
}