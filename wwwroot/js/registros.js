// Variable global para contar artículos
let numeroArticulos = 0;

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
    $('#btnAgregarArticulo').off('click');
    $(document).off('click', '.btn-eliminar-articulo');
    $('#formRegistroDonativos').off('submit');

    // Event: Cambio de tipo de donador
    $('#selectTipoDonador').on('change', function() {
        toggleSeccionContacto($(this).val());
    });

    // Event: Agregar artículo
    $('#btnAgregarArticulo').on('click', function(e) {
        e.preventDefault();
        numeroArticulos++;
        agregarArticulo(numeroArticulos);
    });

    // Event: Eliminar artículo
    $(document).on('click', '.btn-eliminar-articulo', function(e) {
        e.preventDefault();
        $(this).closest('.articulo-item').remove();
    });

    // Event: Submit del formulario
    $('#formRegistroDonativos').on('submit', function(e) {
        e.preventDefault();
        guardarRegistro();
    });

    // Resetear contador de artículos
    numeroArticulos = 0;
    console.log('Formulario inicializado correctamente');
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
        RFC: $('#textRFC').val(),
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
            id_donador: null,
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
    $('#contenedorArticulos').find('.articulo-item').each(function() {
        articulos.push({
            nombre: $(this).find('.input-articulo-nombre').val(),
            cantidad: parseInt($(this).find('.input-articulo-cantidad').val()),
            unidad_medida: $(this).find('.input-articulo-unidad').val(),
            valor_unitario: parseFloat($(this).find('.input-articulo-valor').val()) || 0,
            descripcion: $(this).find('.input-articulo-descripcion').val()
        });
    });

    // Objeto final a enviar
    const datosRegistro = {
        donador
    };

    console.log('Datos a enviar:', datosRegistro);
    
    // Enviar al servidor
    enviarRegistroAlServidor(donador);
}

/**
 * Envía los datos del registro al servidor
 */
function enviarRegistroAlServidor(datosRegistro) {
    // Convertir el objeto a string JSON
    const datosString = JSON.stringify(datosRegistro);

    $.ajax({
        type: 'POST',
        url: '/Registros/insertaDonador',
        data: { datos: datosString },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        dataType: 'json',
        beforeSend: function() {
            toastr.info('Guardando registro...', 'Procesando');
        },
        success: function(response) {
            console.log('Respuesta del servidor:', response);
            
            // Limpiar formulario
            limpiarFormulario();
            
            toastr.success('Registro guardado correctamente', 'Éxito');
            
            // Aquí puedes redirigir o hacer otra acción
            // window.location.href = '/Registros/Index';
        },
        error: function(xhr, status, error) {
            console.error('Error:', error);
            console.error('Respuesta:', xhr.responseText);
            
            if (xhr.status === 400) {
                toastr.error('Datos inválidos. Por favor revisa los campos.', 'Error');
            } else if (xhr.status === 500) {
                toastr.error('Error en el servidor. Intenta de nuevo.', 'Error');
            } else {
                toastr.error('Error al guardar el registro: ' + error, 'Error');
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
    
    // Limpiar sección de contacto
    $('#seccionContacto').addClass('hidden');
    $('#textNombreContacto').prop('required', false);
    $('#textPuestoContacto').prop('required', false);
    $('#textCelularContacto').prop('required', false);
    $('#textEmailContacto').prop('required', false);
    
    // Limpiar artículos
    $('#contenedorArticulos').empty();
    numeroArticulos = 0;
}