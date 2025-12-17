var salidasUtils = (function () {
    // Almacenar datos originales para filtrado
    var conductoresData = [];
    var destinosData = [];
    var articulosData = [];

    return {
        cargarConductores: cargarConductores,
        cargarDestinos: cargarDestinos,
        cargarArticulos: cargarArticulos,
        cargaSalidas: cargaSalidas,
        agregaConductor: agregaConductor,
        agregaDestino: agregaDestino,
        agregaArticulos: agregaArticulos,
        agregaSalidas: agregaSalidas,
        telefonoConductor: telefonoConductor,
        editaRegistro: editaRegistro,
        cargarSelectsModal: cargarSelectsModal,
        guardarEdicionRegistro: guardarEdicionRegistro,
        eliminaRegistro: eliminaRegistro,
        filtrarConductores: filtrarConductores,
        filtrarDestinos: filtrarDestinos,
        filtrarArticulos: filtrarArticulos
    };

    // Función para filtrar conductores
    function filtrarConductores(searchText) {
        var $select = $("#selectConductor");
        $select.empty();
        $select.append('<option value="">--SIN ESPECIFICAR--</option>');

        var filtered = conductoresData.filter(function (conduc) {
            return conduc.nombre.toLowerCase().includes(searchText.toLowerCase());
        });

        $.each(filtered, function (i, conduc) {
            $select.append('<option value="' + conduc.conductorID + '">' + conduc.nombre + '</option>');
        });
    }

    // Función para filtrar destinos
    function filtrarDestinos(searchText) {
        var $select = $("#selectDestino");
        $select.empty();
        $select.append('<option value="">--SIN ESPECIFICAR--</option>');

        var filtered = destinosData.filter(function (desti) {
            return desti.nombre.toLowerCase().includes(searchText.toLowerCase());
        });

        $.each(filtered, function (i, desti) {
            $select.append('<option value="' + desti.destinoID + '">' + desti.nombre + '</option>');
        });
    }

    // Función para filtrar artículos
    function filtrarArticulos(searchText) {
        var $select = $("#selectArticulo");
        $select.empty();
        $select.append('<option value="">--SIN ESPECIFICAR--</option>');

        var filtered = articulosData.filter(function (arti) {
            return arti.nombre.toLowerCase().includes(searchText.toLowerCase());
        });

        $.each(filtered, function (i, arti) {
            $select.append('<option value="' + arti.articuloSalidaID + '">' + arti.nombre + '</option>');
        });
    }

    //Payloads
    function cargarConductores(conductoresJson) {
        conductoresData = conductoresJson;
        var $select = $("#selectConductor");
        $select.empty();
        $select.append('<option value="">--SIN ESPECIFICAR--</option>');

        $.each(conductoresData, function (i, conduc) {
            $select.append('<option value="' + conduc.conductorID + '">' + conduc.nombre + '</option>');
        });
    }

    function cargarDestinos(destinosJson) {
        destinosData = destinosJson;
        var $select = $("#selectDestino");

        $select.empty();
        $select.append('<option value="">--SIN ESPECIFICAR--</option>');

        $.each(destinosData, function (i, desti) {
            $select.append('<option value="' + desti.destinoID + '">' + desti.nombre + '</option>');
        });
    }

    function cargarArticulos(ArticulosSalidasJson) {
        articulosData = ArticulosSalidasJson;
        var $select = $("#selectArticulo");

        $select.empty();
        $select.append('<option value="">--SIN ESPECIFICAR--</option>');

        $.each(articulosData, function (i, arti) {
            $select.append('<option value="' + arti.articuloSalidaID + '">' + arti.nombre + '</option>');
        });
    }

    function cargaSalidas(aValue) {

        const urlSalidas = '/Salidas/buscarResgistroSalidasList';

        $.ajax({
            type: 'GET',
            url: urlSalidas,
            dataType: 'json',
            data: { a: aValue },
            success: function (registros) {
                const registrosArray = tryParse(registros);
                if (Array.isArray(registrosArray)) {
                    SalidasTable.setData(registrosArray);
                }
            },
            error: function () {
                console.error("Error al recargar los registros");
            }
        });

    }

    //Inserts
    function agregaConductor(urlInsertaConductor, aValue) {
        const nombre = $("#txtConductor").val();
        const telefono = $("#txtTelConductor").val();

        if (!nombre) {
            swal("Atención", "Ingrese el nombre del conductor.", "warning");
            return;
        }

        const $btn = $("#btnAgregarConductor");
        $btn.prop("disabled", true);

        $.ajax({
            type: 'POST',
            url: urlInsertaConductor,
            dataType: 'json',
            data: {
                a: aValue,
                nombreConductor: nombre.trim(),
                telefonoConductor: telefono
            },
            success: function (conductor) {
                const data = tryParse(conductor);
                if (!data || !data.conductorID) {
                    swal("Error", "Respuesta inesperada del servidor.", "error");
                    return;
                }

                const id = data.conductorID;
                const nombreResp = data.nombre || nombre;
                const telefonoResp = data.telefono;

                // Agregar a los datos almacenados
                conductoresData.push(data);

                $("#selectConductor")
                    .append(`<option value="${id}">${$("<div>").text(nombreResp).html()}</option>`)
                    .val(id);
                $("#txtTelConductor").val(telefonoResp);
                $("#txtConductor").val("");

                swal("Exito!", "Se ha dado de alta el Conductor", "success");
            },
            error: function (xhr, status, errorThrown) {
                const serverMsg = xhr.responseJSON?.message || xhr.responseText || errorThrown || status;
                swal("Error", serverMsg, "error");
            },
            complete: function () {
                $btn.prop("disabled", false);
            }
        });
    }

    function agregaDestino(urlInsertaDestino, aValue) {
        const nombre = $("#txtDestino").val();

        if (!nombre) {
            swal("Atención", "Ingrese el nombre del destino.", "warning");
            return;
        }

        const $btn = $("#btnAgregarDestino");
        $btn.prop("disabled", true);

        $.ajax({
            type: 'POST',
            url: urlInsertaDestino,
            dataType: 'json',
            data: {
                a: aValue,
                nombreDestino: nombre.trim()
            },
            success: function (destino) {
                const data = tryParse(destino);
                if (!data || !data.destinoID) {
                    swal("Error", "Respuesta inesperada del servidor.", "error");
                    return;
                }

                const id = data.destinoID;
                const nombreResp = data.nombre || nombre;

                // Agregar a los datos almacenados
                destinosData.push(data);

                $("#selectDestino")
                    .append(`<option value="${id}">${$("<div>").text(nombreResp).html()}</option>`)
                    .val(id);

                swal("Exito!", "Se ha dado de alta el Destino", "success");
            },
            error: function (xhr, status, errorThrown) {
                const serverMsg = xhr.responseJSON?.message || xhr.responseText || errorThrown || status;
                swal("Error", serverMsg, "error");
            },
            complete: function () {
                $btn.prop("disabled", false);
            }
        });
    }

    function agregaArticulos(urlInsertaArticulo, aValue) {
        const nombreArticulo = $("#txtArticulo").val();

        if (!nombreArticulo) {
            swal("Atención", "Ingrese el nombre del Artículo.", "warning");
            return;
        }

        const $btn = $("#btnAgregarArticulo");
        $btn.prop("disabled", true);

        $.ajax({
            type: 'POST',
            url: urlInsertaArticulo,
            dataType: 'json',
            data: {
                a: aValue,
                nombreArticuloSalida: nombreArticulo.trim()
            },
            success: function (articuloSalida) {
                const data = tryParse(articuloSalida);
                if (!data || !data.articuloSalidaID) {
                    swal("Error", "Respuesta inesperada del servidor.", "error");
                    return;
                }

                const id = data.articuloSalidaID;
                const nombreResp = data.nombre || nombreArticulo;

                // Agregar a los datos almacenados
                articulosData.push(data);

                $("#selectArticulo")
                    .append(`<option value="${id}">${$("<div>").text(nombreResp).html()}</option>`)
                    .val(id);

                swal("Exito!", "Se ha dado de alta el Artículo", "success");
            },
            error: function (xhr, status, errorThrown) {
                const serverMsg = xhr.responseJSON?.message || xhr.responseText || errorThrown || status;
                swal("Error", serverMsg, "error");
            },
            complete: function () {
                $btn.prop("disabled", false);
            }
        });

    }

    function agregaSalidas(urlInsertaSalida, aValue) {
        const conductor = $("#selectConductor").val();
        const destino = $("#selectDestino").val();
        const articulo = $("#selectArticulo").val();
        const cantidad = $("#txtCantidad").val();
        const placaVehiculo = $("#txtPlacaVehiculo").val();
        const localidad = $("#txtLocalidad").val();
        const observaciones = $("#txtObservaciones").val();
        const salida = $("#selectedSalida").val();
        const dependencia = $("#txtDependencia").val();

        // Validación dinámica: definir reglas aquí y ampliar según sea necesario
        var requiredFields = [
            { selector: "#selectConductor", message: "Ingrese el nombre del conductor." },
            { selector: "#selectDestino", message: "Seleccione un destino." },
            { selector: "#selectArticulo", message: "Seleccione un artículo." },
            { selector: "#txtCantidad", message: "Ingrese la cantidad.", validator: function (v) { return $.isNumeric(v) && Number(v) > 0; } }
            // Agregar más reglas si se requieren otros campos como placa, localidad, etc.
        ];

        var missing = validateRequired(requiredFields);
        if (missing.length) {
            // muestra el primer error y enfoca el campo
            swal("Atención", missing[0].message, "warning");
            $(missing[0].selector).focus();
            return;
        }

        const $btn = $("#btnAgregarRegistro");
        $btn.prop("disabled", true);

        $.ajax({
            type: 'POST',
            url: urlInsertaSalida,
            dataType: 'json',
            data: {
                a: aValue,
                conductorInt: conductor,
                articuloInt: articulo,
                cantidadInt: cantidad,
                destinoInt: destino,
                placaVehiculoStr: placaVehiculo,
                localidadStr: localidad,
                observacionesStr: observaciones,
                dependeciaStr: dependencia,
                salidaStr: salida
            },
            success: function (registro) {
                const data = tryParse(registro);
                if (!data || !data.registroSalidaID) {
                    swal("Error", "Respuesta inesperada del servidor.", "error");
                    return;
                }

                $("#selectArticulo").val("");
                $("#txtCantidad").val("");

                swal("Exito!", "Se ha guardado el registro correctamente", "success");

                // Recargar la tabla con los nuevos registros
                cargaSalidas(aValue);

            },
            error: function (xhr, status, errorThrown) {
                const serverMsg = xhr.responseJSON?.message || xhr.responseText || errorThrown || status;
                swal("Error", serverMsg, "error");
            },
            complete: function () {
                $btn.prop("disabled", false);
            }
        });
    }

    //Utils
    function telefonoConductor(urlBusca, aValue) {
        const conductorID = $("#selectConductor").val();
        const $txtTelefono = $("#txtTelConductor");

        $txtTelefono.val("");

        $.ajax({
            type: 'GET',
            url: urlBusca,
            dataType: 'json',
            data: {
                a: aValue,
                conductorID: conductorID
            },
            success: function (conductor) {
                const data = tryParse(conductor);
                if (!data || !data.conductorID) {
                    swal("Error", "Respuesta inesperada del servidor.", "error");
                    return;
                }


                const telefonoResp = data.telefono;

                $("#txtTelConductor").val(telefonoResp);
            },
            error: function (xhr, status, errorThrown) {
                const serverMsg = xhr.responseJSON?.message || xhr.responseText || errorThrown || status;
                swal("Error", serverMsg, "error");
            },
            complete: function () {
            }
        });

    }

    //Convierte el objeto json
    function tryParse(jsonString) {
        try {
            return JSON.parse(jsonString);
        } catch (e) {
            console.error("Invalid JSON:", e);
            return null;
        }
    }
    
    // Helper: valida dinámicamente un listado de campos requeridos.
    function validateRequired(fields) {
        var missing = [];
        fields.forEach(function (f) {
            var $el = $(f.selector);
            var val = $el.val();
            var isEmpty = (val === undefined || val === null || val === '');
            if (f.validator && typeof f.validator === 'function') {
                if (!f.validator(val)) isEmpty = true; else isEmpty = false;
            }
            if (isEmpty) {
                missing.push(f);
                $el.addClass('is-invalid');
            } else {
                $el.removeClass('is-invalid');
            }
        });
        return missing;
    }

    // Función para cargar y mostrar modal de edición
    function editaRegistro(registroID, aValue) {
        $.ajax({
            type: 'GET',
            url: "/Salidas/buscarResgistroSalidas",
            dataType: 'json',
            data: {
                a: aValue,
                registroSalidaID: registroID
            },
            success: function (response) {
                const data = tryParse(response);
                if (!data || !data.registroSalidaID) {
                    swal("Error", "No se pudo cargar el registro.", "error");
                    return;
                }

                // Llenar los campos del modal
                $("#editRegistroID").val(data.registroSalidaID);
                $("#editConductor").val(data.conductorID);
                $("#editDestino").val(data.destinoID);
                $("#editArticulo").val(data.articuloID);
                $("#editCantidad").val(data.cantidad);
                $("#editPlacaVehiculo").val(data.placaVehiculo);
                $("#editLocalidad").val(data.localidad);
                $("#editObservaciones").val(data.observaciones);
                $("#editDependencia").val(data.dependencia);

                // Seleccionar el radio button de tipo de salida
                $(`input[name="editSalidaTipo"][value="${data.salida}"]`).prop('checked', true);

                // Mostrar el modal
                const modal = new bootstrap.Modal(document.getElementById('editarSalidaModal'));
                modal.show();
            },
            error: function (xhr, status, errorThrown) {
                const serverMsg = xhr.responseJSON?.message || xhr.responseText || errorThrown || status;
                swal("Error", serverMsg, "error");
            }
        });
    }

    // Función para cargar los selects del modal
    function cargarSelectsModal(ArticulosSalidasJson, destinosJson, conductoresJson) {
        var conductor = conductoresJson;
        var destino = destinosJson;
        var articulo = ArticulosSalidasJson;

        // Cargar Conductores en Modal
        const $selectConductorModal = $("#editConductor");
        $selectConductorModal.empty();
        $selectConductorModal.append('<option value="">--SIN ESPECIFICAR--</option>');
        $.each(conductor, function (i, conduc) {
            $selectConductorModal.append('<option value="' + conduc.conductorID + '">' + conduc.nombre + '</option>');
        });

        // Cargar Destinos en Modal
        const $selectDestinoModal = $("#editDestino");
        $selectDestinoModal.empty();
        $selectDestinoModal.append('<option value="">--SIN ESPECIFICAR--</option>');
        $.each(destino, function (i, desti) {
            $selectDestinoModal.append('<option value="' + desti.destinoID + '">' + desti.nombre + '</option>');
        });

        // Cargar Artículos en Modal
        const $selectArticuloModal = $("#editArticulo");
        $selectArticuloModal.empty();
        $selectArticuloModal.append('<option value="">--SIN ESPECIFICAR--</option>');
        $.each(articulo, function (i, arti) {
            $selectArticuloModal.append('<option value="' + arti.articuloSalidaID + '">' + arti.nombre + '</option>');
        });
    }

    // Función para guardar los cambios del registro editado
    function guardarEdicionRegistro(aValue) {
        const registroID = $("#editRegistroID").val();
        const conductor = $("#editConductor").val();
        const destino = $("#editDestino").val();
        const articulo = $("#editArticulo").val();
        const cantidad = $("#editCantidad").val();
        const placaVehiculo = $("#editPlacaVehiculo").val();
        const localidad = $("#editLocalidad").val();
        const observaciones = $("#editObservaciones").val();
        const dependencia = $("#editDependencia").val();
        const salida = $('input[name="editSalidaTipo"]:checked').val();

        if (!registroID) {
            swal("Error", "ID de registro no válido.", "error");
            return;
        }

        var requiredFields = [
            { selector: "#editConductor", message: "Seleccione un conductor." },
            { selector: "#editDestino", message: "Seleccione un destino." },
            { selector: "#editArticulo", message: "Seleccione un artículo." },
            { selector: "#editCantidad", message: "Ingrese la cantidad.", validator: function (v) { return $.isNumeric(v) && Number(v) > 0; } }
        ];

        var missing = validateRequired(requiredFields);
        if (missing.length) {
            swal("Atención", missing[0].message, "warning");
            $(missing[0].selector).focus();
            return;
        }

        const $btn = $("#btnGuardarEdicion");
        $btn.prop("disabled", true);

        const urlActualizar = '/Salidas/actualizaRegistroSalida';

        $.ajax({
            type: 'POST',
            url: urlActualizar,
            dataType: 'json',
            data: {
                a: aValue,
                registroSalidaInt: registroID,
                conductorInt: conductor,
                articuloInt: articulo,
                cantidadInt: cantidad,
                destinoInt: destino,
                placaVehiculoStr: placaVehiculo,
                localidadStr: localidad,
                observacionesStr: observaciones,
                dependeciaStr: dependencia,
                salidaStr: salida
            },
            success: function (response) {
                const data = tryParse(response);
                if (!data || !data.registroSalidaID) {
                    swal("Error", "Respuesta inesperada del servidor.", "error");
                    return;
                }

                swal("Éxito!", "Se ha actualizado el registro correctamente", "success");

                // Cerrar el modal
                bootstrap.Modal.getInstance(document.getElementById('editarSalidaModal')).hide();

                // Recargar la tabla
                cargaSalidas(aValue);
            },
            error: function (xhr, status, errorThrown) {
                const serverMsg = xhr.responseJSON?.message || xhr.responseText || errorThrown || status;
                swal("Error", serverMsg, "error");
            },
            complete: function () {
                $btn.prop("disabled", false);
            }
        });
    }

    function eliminaRegistro(id, aValue) {
        swal({
            title: "Eliminar registro",
            text: "¿Deseas eliminar el registro " + id + "?",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((willDelete) => {
            if (willDelete) {
                var url = '/Salidas/eliminaRegistroSalida';
                $.get(url, { a: aValue, registroSalidaID: id })
                    .done(function (resp) {
                        const newData = (SalidasTable.data || []).filter(
                            (x) => String(x.registroSalidaID ?? x.id ?? "") !== String(id)
                        );
                        SalidasTable.setData(newData);
                        swal("Registro eliminado", { icon: "success" });
                    })
                    .fail(function () {
                        swal("Error al eliminar", { icon: "error" });
                    });
            }
        });
    }
})();