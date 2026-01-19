// Variable global para contar artículos
let numeroArticulos = 0;
let donadorSeleccionado = null;
let timerOcultarListaDonadores = null;

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
        $('#listaDonadores').addClass('d-none');
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
            console.log('Artículos recibidos:', articulos);
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
                <small style="margin: 0;">Podes crear uno nuevo escribiendo el nombre</small>
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
                    <i class="fas fa-ruler"></i> ${articulo.unidad_medida || 'N/A'}
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
        url: 'ObtenerContactos',
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
 * Guarda el id_articulo para diferenciarlo como artículo existente
 */
function seleccionarArticuloEnContenedor($element, numeroArticulo) {
    try {
        const articulo = JSON.parse($element.attr('data-articulo'));

        if (!articulo.id_articulo) {
            toastr.warning('El artículo no tiene un ID válido', 'Validación');
            return;
        }

        console.log('Artículo seleccionado:', articulo);
        console.log('ID del artículo:', articulo.id_articulo);

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
            const idCategoria = articulo.categoria.id_categoria;
            const nombreCategoria = articulo.categoria.nombre;
            $contenedor.find('.input-articulo-categoria-select').val(idCategoria);
            console.log('Categoría establecida a:', nombreCategoria);
        }

        // Guardar el ID del artículo en el data attribute
        $contenedor.data('id_articulo', articulo.id_articulo);
        console.log('ID del artículo guardado en data attribute:', $contenedor.data('id_articulo'));

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

    console.log('Datos del donador:', donador);
    console.log('Datos del contacto:', contacto);
    console.log('Artículos nuevos:', articulosNuevos);
    console.log('Artículos existentes:', articulosExistentes);

    // Enviar datos de forma secuencial
    guardarDonador(donador, contacto, articulosNuevos, articulosExistentes);
}

/**
 * Guarda el donador y procede con contacto y artículos
 */
function guardarDonador(donador, contacto, articulosNuevos, articulosExistentes) {
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
            guardarContacto(contacto, articulosNuevos, articulosExistentes, parseInt(idDonadorExistente));
        } else {
            insertarArticulosNuevos(articulosNuevos, articulosExistentes, parseInt(idDonadorExistente));
        }
        return;
    }

    // Si no existe ID, es un donador nuevo, hay que guardarlo
    console.log('Guardando donador nuevo');

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
            console.log('Respuesta al guardar contacto:', response);

            // Proceder a insertar artículos nuevos
            insertarArticulosNuevos(articulosNuevos, articulosExistentes, donadorId);
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar contacto:', error);
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
        unidad_medida: articulo.unidad_medida,
        id_categoria: articulo.id_categoria,
        descripcion: articulo.descripcion
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
            console.log('Respuesta al guardar artículos nuevos:', response);

            // Extraer los IDs de los artículos insertados
            let articulosInsertadosConId = [];
            try {
                // Validar que la respuesta contenga los artículos con IDs
                if (response && response.resultados) {
                    articulosInsertadosConId = response.resultados
                        .filter(r => r.exito)
                        .map((r, index) => ({
                            ...articulosNuevos[index],
                            id_articulo: r.id_articulo || null
                        }));

                    console.log('Artículos insertados con IDs:', articulosInsertadosConId);
                }
            } catch (e) {
                console.warn('No se pudo extraer IDs de artículos:', e);
                toastr.error('Error al procesar artículos insertados', 'Error');
                return;
            }

            // Construir y guardar el donativo con todos los artículos
            construirYGuardarDonativo(articulosInsertadosConId, articulosExistentes, donadorId);
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar artículos nuevos:', error);
            console.error('Respuesta:', xhr.responseText);
            toastr.error('Error al guardar los artículos nuevos. Intenta de nuevo.', 'Error');
        }
    });
}

/**
 * Construye el JSON completo del donativo y lo envía
 * Estructura: {id_donador, fecha_recepcion, tipo, estatus, observaciones, detalles[]}
 */
function construirYGuardarDonativo(articulosInsertados, articulosExistentes, donadorId) {
    // Combinar todos los artículos (nuevos insertados + existentes)
    const todosLosArticulos = [...articulosInsertados, ...articulosExistentes];

    if (todosLosArticulos.length === 0) {
        toastr.warning('No hay artículos para registrar', 'Validación');
        return;
    }

    // Construir los detalles con todos los artículos
    const detalles = todosLosArticulos.map(articulo => ({
        id_articulo: articulo.id_articulo,
        cantidad_recibida: articulo.cantidad_recibida,
        valor_unitario: articulo.valor_unitario,
        estado: articulo.estado
    }));

    // Construir el JSON completo del donativo
    const donativoCompleto = {
        id_donador: donadorId,
        fecha_recepcion: $('#dateRecepcion').val() + 'T10:30:00',
        tipo: $('#selectTipoDonativo').val(),
        estatus: 'Recibido',
        observaciones: $('#textObservacionesDonativo').val(),
        detalles: detalles
    };

    const datosString = JSON.stringify(donativoCompleto);

    console.log('Donativo completo a enviar:', donativoCompleto);

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

            //// Limpiar formulario
            //limpiarFormulario();

            toastr.success('Registro guardado correctamente', 'Éxito');
        },
        error: function (xhr, status, error) {
            console.error('Error al guardar donativo:', error);
            console.error('Respuesta:', xhr.responseText);

            if (xhr.status === 400) {
                toastr.error('Datos de donativo inválidos. Por favor revisa los campos.', 'Error');
            } else if (xhr.status === 500) {
                toastr.error('Error en el servidor. Intenta de nuevo.', 'Error');
            } else {
                toastr.error('Error al guardar el donativo: ' + error, 'Error');
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

/**
 * Evento personalizado cuando la vista de registros se carga
 */
document.addEventListener('registrosViewLoaded', function () {
    console.log('=== registrosViewLoaded evento disparado ===');

    setTimeout(function () {
        console.log('Iniciando cargarDonativos después de registrosViewLoaded');
        cargarDonativos();

        // Botón refrescar
        const btnRefrescar = document.getElementById('btnRefrescarDonativos');
        if (btnRefrescar) {
            console.log('✓ Botón refrescar encontrado, agregando listener');
            btnRefrescar.addEventListener('click', function () {
                console.log('Botón refrescar clickeado');
                cargarDonativos();
            });
        } else {
            console.warn('⚠ Botón refrescar NO encontrado');
        }
    }, 200);
});

// Botón refrescar
document.getElementById('btnRefrescarDonativos').addEventListener('click', function () {
    cargarDonativos();
});

// Inicialización para cuando se carga el DOM por primera vez
document.addEventListener('DOMContentLoaded', function () {
    console.log('=== DOMContentLoaded ===');

    // Si acciden directamente a la página de registros
    const tabla = document.getElementById('tablaDonativos');
    if (tabla) {
        console.log('Tabla encontrada en carga inicial, disparando evento');
        document.dispatchEvent(new Event('registrosViewLoaded'));
    }
});

/**
 * Carga los donativos desde el servidor
 */
function cargarDonativos() {
    console.log('=== Iniciando cargarDonativos ===');

    // Validar que la tabla exista
    const tabla = document.getElementById('tablaDonativos');
    const tbody = tabla ? tabla.querySelector('tbody') : null;

    if (!tabla) {
        console.error('❌ Tabla #tablaDonativos no encontrada en el DOM');
        return;
    }

    if (!tbody) {
        console.error('❌ tbody en la tabla no encontrado');
        return;
    }

    console.log('✓ Tabla y tbody encontrados, iniciando AJAX');

    $.ajax({
        type: 'GET',
        url: 'ObtenerDonativos',
        dataType: 'json',
        timeout: 10000,
        beforeSend: function () {
            console.log('AJAX beforeSend');
            toastr.info('Cargando donativos...', 'Procesando');
        },
        success: function (response) {
            console.log('=== AJAX Success ===');
            console.log('Respuesta recibida:', response);
            console.log('Tipo de respuesta:', typeof response);
            console.log('¿Es array?:', Array.isArray(response));

            // Log detallado de la estructura
            response && typeof response === 'object' && console.log('Propiedades de la respuesta:', Object.keys(response));

            let donativosArray = [];

            // Manejar respuesta como array directo
            if (Array.isArray(response)) {
                donativosArray = response;
                console.log('✓ Respuesta es array con ' + donativosArray.length + ' elementos');
            }
            // Manejar respuesta con error
            else if (response && response.error) {
                console.error('❌ Error en respuesta:', response.error);
                toastr.error(`Error: ${response.error}`, 'Error');
                tbody.innerHTML = `<tr><td colspan="8" class="text-center text-danger">
                    <i class="fas fa-exclamation-triangle"></i> ${response.error}
                </td></tr>`;
                return;
            }
            // Si es objeto pero no array
            else if (response && typeof response === 'object') {
                console.warn('⚠ Respuesta no es array directo');

                // Intentar buscar array en propiedades comunes
                if (Array.isArray(response.data)) {
                    donativosArray = response.data;
                    console.log('✓ Array extraído de response.data');
                } else if (Array.isArray(response.donativos)) {
                    donativosArray = response.donativos;
                    console.log('✓ Array extraído de response.donativos');
                } else if (Array.isArray(response.items)) {
                    donativosArray = response.items;
                    console.log('✓ Array extraído de response.items');
                } else if (Array.isArray(response.registros)) {
                    donativosArray = response.registros;
                    console.log('✓ Array extraído de response.registros');
                } else {
                    console.warn('No se encontró array en la respuesta');
                    tbody.innerHTML = `<tr><td colspan="8" class="text-center text-warning">
                        Formato de respuesta inesperado
                    </td></tr>`;
                    return;
                }
            }

            // Llenar tabla con datos
            if (donativosArray && donativosArray.length > 0) {
                console.log('✓ Llenando tabla con ' + donativosArray.length + ' donativos');
                llenarTablaDonativos(donativosArray);
                toastr.success(`Se cargaron ${donativosArray.length} donativos`, 'Éxito');
            } else {
                console.log('⚠ No hay donativos para mostrar');
                tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted"><i class="fas fa-inbox"></i> No hay donativos registrados</td></tr>';
                toastr.info('No hay donativos registrados', 'Información');
            }
        },
        error: function (xhr, status, error) {
            console.error('=== AJAX Error ===');
            console.error('Status:', status);
            console.error('Error:', error);
            console.error('XHR Status Code:', xhr.status);
            console.error('Response Text:', xhr.responseText);

            let mensajeError = 'Error al cargar los donativos';
            if (status === 'timeout') {
                mensajeError = 'Tiempo de espera agotado';
            } else if (xhr.status === 0) {
                mensajeError = 'Sin conexión al servidor';
            } else if (xhr.status === 401) {
                mensajeError = 'Sesión expirada';
            }

            toastr.error(mensajeError, 'Error');

            const tbody = document.querySelector('#tablaDonativos tbody');
            if (tbody) {
                tbody.innerHTML = `<tr><td colspan="8" class="text-center text-danger">
                    <i class="fas fa-exclamation-triangle"></i> ${mensajeError}
                </td></tr>`;
            }
        }
    });
}

/**
 * Llena la tabla con los donativos
 */
function llenarTablaDonativos(donativos) {
    console.log('Iniciando llenarTablaDonativos con', donativos.length, 'registros');

    const tbody = document.querySelector('#tablaDonativos tbody');
    if (!tbody) {
        console.error('❌ tbody no encontrado al llenar tabla');
        return;
    }

    tbody.innerHTML = '';

    donativos.forEach((donativo, index) => {
        const fila = document.createElement('tr');

        const fechaFormato = donativo.fecha_recepcion
            ? new Date(donativo.fecha_recepcion).toLocaleDateString('es-MX')
            : 'N/A';
        const cantidadArticulos = donativo.cantidad_articulos

        fila.innerHTML = `
            <td>${donativo.id_donativo || '-'}</td>
            <td><strong>${donativo.folio || '-'}</strong></td>
            <td>${donativo.nombre || 'N/A'}</td>
            <td>${fechaFormato}</td>
            <td>
                <span class="badge badge-pill ${obtenerColorEstatus(donativo.tipo)}">${donativo.tipo || 'N/A'}</span>
            </td>
            <td>
                <span class="badge badge-pill ${obtenerColorEstatus(donativo.estatus)}">
                    ${donativo.estatus || 'Pendiente'}
                </span>
            </td>
            <td>
                <span class="badge badge-pill badge-info">${cantidadArticulos}</span>
            </td>
            <td>
                <button class="btn btn-sm btn-info" onclick="verDetalles(${donativo.id_donativo})" title="Ver detalles">
                    <i class="fas fa-eye"></i>
                </button>
                <button class="btn btn-sm btn-warning" onclick="editarDonativo(${donativo.id_donativo})" title="Editar">
            <i class="fas fa-edit"></i>
            </button>
                <button class="btn btn-sm btn-danger" onclick="eliminarDonativo(${donativo.id_donativo})" title="Eliminar">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;
        //Se oculta el boton editar mientras no se implemente la funcion
        //<button class="btn btn-sm btn-warning" onclick="editarDonativo(${donativo.id_donativo})" title="Editar">
        //    <i class="fas fa-edit"></i>
        //</button>
        tbody.appendChild(fila);
    });

    console.log('✓ Tabla llenada exitosamente');
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
 * Ver detalles del donativo
 * Obtiene los detalles completos del servidor y los muestra en un modal
 */
function verDetalles(idDonativo) {
    console.log('Obteniendo detalles del donativo ID:', idDonativo);

    $.ajax({
        type: 'GET',
        url: 'obtenerDonativoDetalle',
        data: { id_donativo: idDonativo },
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Cargando detalles...', 'Procesando');
        },
        success: function (response) {
            console.log('Detalles obtenidos:', response);

            // Procesar el JSON con la estructura de artículos
            procesarYMostrarDetalles(JSON.parse(response), idDonativo);

            // Mostrar el modal
            $('#modalDetallesDonativo').modal('show');
            toastr.success('Detalles cargados correctamente', 'Éxito');
        },
        error: function (xhr, status, error) {
            console.error('Error al obtener detalles:', error);
            console.error('Respuesta:', xhr.responseText);
            toastr.error('Error al cargar los detalles del donativo', 'Error');
        }
    });
}

/**
 * Procesa y muestra los detalles del donativo en el modal
 * Estructura esperada: array de artículos con información completa
 */
function procesarYMostrarDetalles(detalles, idDonativo) {
    const contenido = document.getElementById('contenidoDetalles');

    if (!detalles || detalles.length === 0) {
        contenido.innerHTML = `
            <div class="alert alert-info" role="alert">
                <i class="fas fa-info-circle"></i> No hay detalles disponibles para este donativo
            </div>
        `;
        return;
    }

    let detallesHtml = `
        <div class="row mb-4">
            <div class="col-12">
                <h6 class="font-bold text-stone-700">
                    <i class="fas fa-box"></i> Artículos Donados
                </h6>
                <div class="table-responsive">
                    <table class="table table-sm table-striped table-hover">
                        <thead style="background-color: var(--dorado); color: white;">
                            <tr>
                                <th>Artículo</th>
                                <th>Categoría</th>
                                <th>Unidad de Medida</th>
                                <th>Cantidad</th>
                                <th>Valor Unitario</th>
                                <th>Valor Total</th>
                                <th>Estado</th>
                            </tr>
                        </thead>
                        <tbody>
    `;

    let totalGeneral = 0;

    // Iterar sobre los artículos
    detalles.forEach((detalle) => {
        const subtotal = detalle.valor_total || (detalle.cantidad_recibida * detalle.valor_unitario);
        totalGeneral += parseFloat(subtotal);

        detallesHtml += `
            <tr>
                <td>
                    <strong>${detalle.nombre_articulo || 'N/A'}</strong>
                    <br>
                    <small class="text-muted">ID: ${detalle.id_articulo}</small>
                </td>
                <td>${detalle.categoria_articulo || 'N/A'}</td>
                <td>${detalle.unidad_medida || 'N/A'}</td>
                <td class="text-center">
                    <span class="badge badge-primary">${detalle.cantidad_recibida}</span>
                </td>
                <td class="text-right">$${parseFloat(detalle.valor_unitario).toFixed(2)}</td>
                <td class="text-right">
                    <strong>$${parseFloat(subtotal).toFixed(2)}</strong>
                </td>
                <td>
                    <span class="badge ${obtenerColorEstado(detalle.estado)}">
                        ${detalle.estado || 'N/A'}
                    </span>
                </td>
            </tr>
        `;
    });

    // Fila de totales
    detallesHtml += `
        <tr style="background-color: #f9f9f9; font-weight: bold;">
            <td colspan="5" class="text-right">
                <strong>TOTAL GENERAL:</strong>
            </td>
            <td class="text-right">
                <strong style="font-size: 1.2rem; color: var(--guinda);">
                    $${totalGeneral.toFixed(2)}
                </strong>
            </td>
            <td></td>
        </tr>
    `;

    detallesHtml += `
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <hr>

        <div class="row">
            <div class="col-12">
                <h6 class="font-bold text-stone-700">
                    <i class="fas fa-file-alt"></i> Resumen
                </h6>
                <div class="alert alert-light" role="alert">
                    <p class="mb-1"><strong>Cantidad Total de Artículos:</strong> ${detalles.length}</p>
                    <p class="mb-0"><strong>Valor Total del Donativo:</strong> <span style="color: var(--guinda); font-weight: bold;">$${totalGeneral.toFixed(2)}</span></p>
                </div>
            </div>
        </div>
    `;

    contenido.innerHTML = detallesHtml;
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
    console.log('Editando donativo ID:', idDonativo);

    $.ajax({
        type: 'GET',
        url: 'obtenerDonativo',
        data: { id_donativo: idDonativo },
        dataType: 'json',
        beforeSend: function () {
            toastr.info('Cargando donativo para edición...', 'Procesando');
        },
        success: function (response) {
            console.log('Donativo obtenido para edición:', response);

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
    if (donativo.donador?.tipo === 'organizacion' || donativo.donador?.tipo === 'empresa') {
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
    $('#selectTipoDonativo').val(donativo.tipo || 'especie');
    $('#textObservacionesDonativo').val(donativo.observaciones || '');

    // Si es efectivo, mostrar el campo de importe
    if (donativo.tipo === 'efectivo') {
        $('#grupoImporte').show();
        // Si existe un importe, cargarlo (generalmente viene del primer detalle o de un campo separado)
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
            $articulo.find('.input-articulo-unidad').val(detalle.unidad_medida || '');
            $articulo.find('.input-articulo-categoria-select').val(detalle.categoria || '');
            $articulo.find('.input-articulo-cantidad').val(detalle.cantidad_recibida || 0);
            $articulo.find('.input-articulo-valor').val(parseFloat(detalle.valor_unitario).toFixed(2));
            $articulo.find('.input-articulo-estado').val(detalle.estado || '');

            // Guardar el ID del artículo
            $articulo.data('id_articulo', detalle.id_articulo);
        });
    }

    console.log('✓ Donativo cargado en el formulario correctamente');
    toastr.success('Donativo cargado en el formulario', 'Éxito');
}

/**
 * Elimina el donativo dado su ID (soft delete)
 */
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
            console.log('Eliminando donativo ID:', idDonativo);

            $.ajax({
                type: 'GET',
                url: 'EliminarDonativo',
                data: { id_donativo: idDonativo },
                dataType: 'json',
                beforeSend: function () {
                    toastr.info('Eliminando donativo...', 'Procesando');
                },
                success: function (response) {
                    console.log('Donativo eliminado:', response);
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
