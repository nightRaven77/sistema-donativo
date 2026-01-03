// Variable global para contar artículos
let numeroArticulos = 0;
let donadorSeleccionado = null;

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
        toggleSeccionContacto(donador.tipo || '');

        // Mostrar indicador visual de que el donador fue seleccionado
        $('#textNombreDonador').css('background-color', '#e8f5e9').prop('readonly', true);

        toastr.success(`✓ Donador "${donador.nombre}" seleccionado correctamente`, 'Éxito');
    } catch (error) {
        console.error('Error al seleccionar donador:', error);
        toastr.error('Error al seleccionar donador', 'Error');
    }
}

/**
 * Muestra u oculta la sección de contacto según el tipo de donador
 */
function toggleSeccionContacto(tipo) {
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
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
                <div class="form-group">
                    <label class="block text-sm font-bold text-stone-700 mb-2">
                        Nombre del Artículo <span class="text-red-500">*</span>
                    </label>
                    <input type="text" class="form-control input-articulo-nombre" placeholder="Nombre del artículo" required>
                    <div class="invalid-feedback">El nombre del artículo es requerido</div>
                </div>
                <div class="form-group">
                    <label class="block text-sm font-bold text-stone-700 mb-2">
                        Cantidad <span class="text-red-500">*</span>
                    </label>
                    <input type="number" class="form-control input-articulo-cantidad" min="1" placeholder="0" required>
                    <div class="invalid-feedback">La cantidad es requerida</div>
                </div>
            </div>
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
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
                    </select>
                    <div class="invalid-feedback">Selecciona una unidad de medida</div>
                </div>
                <div class="form-group">
                    <label class="block text-sm font-bold text-stone-700 mb-2">
                        Valor Unitario
                    </label>
                    <input type="number" class="form-control input-articulo-valor" min="0" step="0.01" placeholder="0.00">
                </div>
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

    // Construir array de artículos
    const articulos = [];
    $('#contenedorArticulos').find('.articulo-item').each(function () {
        articulos.push({
            nombre: $(this).find('.input-articulo-nombre').val(),
            cantidad: parseInt($(this).find('.input-articulo-cantidad').val()),
            unidad_medida: $(this).find('.input-articulo-unidad').val(),
            valor_unitario: parseFloat($(this).find('.input-articulo-valor').val()) || 0,
            descripcion: $(this).find('.input-articulo-descripcion').val()
        });
    });

    console.log('Datos del donador:', donador);
    console.log('Datos del contacto:', contacto);
    console.log('Datos de artículos:', articulos);

    // Enviar datos de forma secuencial
    guardarDonador(donador, contacto, articulos);
}

/**
 * Guarda el donador y procede con contacto y artículos
 */
function guardarDonador(donador, contacto, articulos) {
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
            guardarContacto(contacto, articulos, parseInt(idDonadorExistente));
        } else {
            guardarArticulos(articulos, parseInt(idDonadorExistente));
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
                guardarContacto(contacto, articulos, donadorId);
            } else {
                // Si no hay contacto, proceder directamente con artículos
                guardarArticulos(articulos, donadorId);
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
function guardarContacto(contacto, articulos, donadorId) {
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
            guardarArticulos(articulos, donadorId);
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
function guardarArticulos(articulos, donadorId) {
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