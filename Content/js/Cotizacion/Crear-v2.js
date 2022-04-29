var first = true;
var idCotizacion = 0;
$(document).ready(function () {
    if ($("#idCotizacion_R13").val() != null) {
        idCotizacion = $("#idCotizacion_R13").val();
    }
    if (first && idCotizacion == 0) {
        first = false;
    }
    if ($('#cotizacionNoNull').val() === 'true') {
        if ($('#clienteCotizacion').val() !== '0') {
            $("#idSelectRut").trigger("change");
        }
        if ($('#modalidadCotizacionNoNull').val() === 'true') {
            $("#idSelectModalidadCurso").trigger("change");
        }
    }



    $('#idSucursal').change(function () {
        if ($("#idSelectModalidadCurso").val() == 'Abierto') {
            $("#idSelectModalidadCurso").trigger("change");
        }
    });

    $('#cantParticipantes').change(function () {
        if ($("#idSelectCursos").val() != null && $("#idSelectCursos").val() != '') {
            calcularTotal($('#cantParticipantes').val());
        }
    });

    $('#cantParticipantes').keyup(function () {
        if ($("#idSelectCursos").val() != null && $("#idSelectCursos").val() != '') {
            calcularTotal($('#cantParticipantes').val());
        }
    });

    $("#idTableCostoDatos").keyup(function () {
        if ($("#idSelectCursos").val() != null && $("#idSelectCursos").val() != '') {
            calcularTotal($('#cantParticipantes').val());
        }
    });

    $("#idTableCostoDatos").change(function () {
        if ($("#idSelectCursos").val() != null && $("#idSelectCursos").val() != '') {
            calcularTotal($('#cantParticipantes').val());
        }
    });
    if ($('#idSelectTipoCurso').val() !== 'Recertificación') {
        $("#tipoEjecucion option[value='3']").attr('disabled', 'disabled');
        $("#tipoEjecucion option[value='4']").attr('disabled', 'disabled');
        $("#tipoEjecucion option[value='5']").attr('disabled', 'disabled');
    }
    $(document).click(function () {
        $('#cotizacionNoNull').val('false');
    });
    calcularTotal();

    $('#form').submit(function () {
        $('#form input').unmask();
    });
    $("#idSelectTipoCurso").trigger("change");

    $('#idCheckBoxCodigoSENCE').change(function () {
        if ($('#idCheckBoxCodigoSENCE').prop('checked')) {
            $('#cotizacion_tieneCodigoSence').val("on");
        } else {
            $('#cotizacion_tieneCodigoSence').val(null);
        }
    });
});



$('#idSelectRut').on('change', function () {
    var clientesArray = JSON.parse($('#clientesArray').val());
    var cliente = clientesArray.find(o => o.idCliente === Number(this.value));
    $('#cotizacion_nombreEmpresa').val(cliente.nombreEmpresa);
    $('#cotizacion_razonSocial').val(cliente.razonSocial);
    $('#cotizacion_telefonoCorporativo').val(cliente.telefonoCorporativo);
    $('#cotizacion_direccion').val(cliente.direccion);
    obtenerGiro(cliente);
    obtenerFaena(cliente);
    obtenerEncargadoPago(cliente);
    obtenerContacto(cliente);
    obtenerClienteDeudor(cliente);
    obtenerClienteOCPendiente(cliente);
});

$('#idSelectModalidadCurso').on('change', function () {
    obtenerCursos($(this).val());
});

$('#idSelectCursos').on('change', function () {
    obtenerDatosCurso($(this).val());
    if (!first && idCotizacion != 0) {
        obtenerCostos($(this).val(), $('#idSelectModalidadCurso').val(), $('#cantParticipantes').val())
    }
    if (!first && idCotizacion == 0) {
        obtenerCostos($(this).val(), $('#idSelectModalidadCurso').val(), $('#cantParticipantes').val())
    }
    first = false;
});

$('#idSelectTipoCurso').on('change', function () {
    if ($(this).val() == "Duplicado Credencial" || $(this).val() == "Arriendo de Sala" || $(this).val() == "Tramitación Licencia") {
        $("#saveButton").attr("style", "display:block; float:right");
        $("#loading-save").attr("style", "display:none; float:right");
        $('#idCostosCursoDiv').hide();
        $('#idCostosCursoDiv').find('input, textarea, button, select').attr('disabled', true);
        $('#idCostosCursoDivCorto').show();
        $('#idCostosCursoDivCorto').find('input, textarea, button, select').attr('disabled', false);
        $('#idDatosVentaDiv').hide();
        $('#idDatosVentaDiv').find('input, textarea, button, select').attr('disabled', true);
    }
    else {
        $('#idCostosCursoDiv').show();
        $('#idCostosCursoDiv').find('input, textarea, button, select').attr('disabled', false);
        $('#idCostosCursoDivCorto').hide();
        $('#idCostosCursoDivCorto').find('input, textarea, button, select').attr('disabled', true);
        $('#idDatosVentaDiv').show();
        $('#idDatosVentaDiv').find('input, textarea, button, select').attr('disabled', false);
        if ($(this).val() == "Recertificación") {
            if ($('#cotizacionNoNull').val() === 'true') {

            } else {
                $('#tipoEjecucion').val('3');
            }
            if ($('#tipoEjecucionCotizacion').val() === 'Recertificacion') {
                $('#tipoEjecucion').val('3');
            }
            if ($('#tipoEjecucionCotizacion').val() === 'Recertificacion_Sincronica') {
                $('#tipoEjecucion').val('4');
            }
            if ($('#tipoEjecucionCotizacion').val() === 'Recertificacion_Asincronica') {
                $('#tipoEjecucion').val('5');
            }
            $("#tipoEjecucion option[value='0']").attr('disabled', 'disabled');
            $("#tipoEjecucion option[value='1']").attr('disabled', 'disabled');
            $("#tipoEjecucion option[value='2']").attr('disabled', 'disabled');
            $("#tipoEjecucion option[value='3']").removeAttr('disabled');
            $("#tipoEjecucion option[value='4']").removeAttr('disabled');
            $("#tipoEjecucion option[value='5']").removeAttr('disabled');
        }
        else {
            if ($('#cotizacionNoNull').val() === 'true') {

            } else {
                $('#tipoEjecucion').val('0');
            }
            if ($('#tipoEjecucionCotizacion').val() === 'Presencial') {
                $('#tipoEjecucion').val('0');
            }
            if ($('#tipoEjecucionCotizacion').val() === 'Elearning_Sincrono') {
                $('#tipoEjecucion').val('1');
            }
            if ($('#tipoEjecucionCotizacion').val() === 'Elearning_Asincrono') {
                $('#tipoEjecucion').val('2');
            }
            $("#tipoEjecucion option[value='0']").removeAttr('disabled');
            $("#tipoEjecucion option[value='1']").removeAttr('disabled');
            $("#tipoEjecucion option[value='2']").removeAttr('disabled');
            $("#tipoEjecucion option[value='3']").attr('disabled', 'disabled');
            $("#tipoEjecucion option[value='4']").attr('disabled', 'disabled');
            $("#tipoEjecucion option[value='5']").attr('disabled', 'disabled');
        }
    }

    $("#tipoEjecucion").trigger("change");

});

$('#tipoEjecucion').change(function () {
    if ($("#idSelectModalidadCurso").val() != null) {
        $("#idSelectModalidadCurso").trigger("change");
    }
    aportesCapacitacion($(this));
});

function obtenerGiro(cliente) {
    $.ajax({
        url: $('#urlObtenerGiro').val(), // Url
        data: {
            id: cliente.idCliente, // Parámetros
        },
        type: "post"  // Verbo HTTP
    })
        .done(function (result) {
            $("#idSelectGiro").html("");
            //$("#idSelectGiro").append('<option disabled data-tokens="" selected> Seleccionar </option>');
            for (var i = 0; i < result.length; i++) {
                if ($('#cotizacionNoNull').val() === 'true') {
                    if ($('#giroCotizacion').val() == result[i].descripcion) {
                        $("#idSelectGiro").append('<option data-tokens="' + result[i].descripcion + ' "selected>' + result[i].descripcion + '</option>');
                    } else {
                        $("#idSelectGiro").append('<option data-tokens="' + result[i].descripcion + '">' + result[i].descripcion + '</option>');
                    }
                } else {
                    $("#idSelectGiro").append('<option data-tokens="' + result[i].descripcion + '">' + result[i].descripcion + '</option>');
                }
            }
        })
        .fail(function (xhr, status, error) {

        })
        .always(function () {

        });
}

function obtenerFaena(cliente) {
    $.ajax({
        url: $('#urlObtenerFaena').val(), // Url
        data: {
            id: cliente.idCliente, // Parámetros
        },
        type: "post"  // Verbo HTTP
    })
        .done(function (result) {
            $("#idfaena").html("");
            if (result.length > 0) {
                $("#idFaena").empty().append('<option value="-1">Seleccione una Faena</option>');
                for (var i = 0; i < result.length; i++) {
                    if ($('#cotizacionNoNull').val() === 'true') {
                        if ($('#clienteFaena').val() == result[i].idFaena) {
                            $("#idFaena").append('<option value="' + result[i].idFaena + ' "selected>' + result[i].descripcion + '</option>');
                        } else {
                            $("#idFaena").append('<option value="' + result[i].idFaena + '">' + result[i].descripcion + '</option>');
                        }
                    } else {
                        $("#idFaena").append('<option value="' + result[i].idFaena + '">' + result[i].descripcion + '</option>');
                    }
                }
                $("#faena-container").show();
            } else {
                $("#faena-container").hide();
            }
        })
        .fail(function (xhr, status, error) {

        })
        .always(function () {

        });
}

function obtenerEncargadoPago(cliente) {
    $.ajax({
        url: $('#urlObtenerEncargadoDePago').val(), // Url
        data: {
            id: cliente.idCliente, // Parámetros
        },
        type: "post"  // Verbo HTTP
    })
        .done(function (result) {
            $("#idSelectEncargadoPago").html("");
            //$("#idSelectEncargadoPago").append('<option disabled data-tokens="" selected> Seleccionar </option>');
            for (var i = 0; i < result.length; i++) {

                if ($('#cotizacionNoNull').val() === 'true') {
                    if ($('#contactoEncargadoPago').val() == result[i].idContacto) {
                        $("#idSelectEncargadoPago").append('<option selected value="' + result[i].idContacto + '" data-tokens="' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '">' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '</option>');
                    } else {
                        $("#idSelectEncargadoPago").append('<option value="' + result[i].idContacto + '" data-tokens="' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '">' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '</option>');
                    }
                } else {
                    $("#idSelectEncargadoPago").append('<option value="' + result[i].idContacto + '" data-tokens="' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '">' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '</option>');
                }
            }
        })
        .fail(function (xhr, status, error) {

        })
        .always(function () {

        });
}

function obtenerContacto(cliente) {
    $.ajax({
        url: $('#urlObtenerContacto').val(), // Url
        data: {
            id: cliente.idCliente, // Parámetros
        },
        type: "post"  // Verbo HTTP
    })
        .done(function (result) {
            $("#idSelectContacto").html("");
            //$("#idSelectContacto").append('<option disabled data-tokens="" selected> Seleccionar </option>');
            for (var i = 0; i < result.length; i++) {

                if ($('#cotizacionNoNull').val() === 'true') {
                    if ($('#contactoCotizacion').val() == result[i].idContacto) {
                        $("#idSelectContacto").append('<option selected value="' + result[i].idContacto + '" data-tokens="' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '">' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '</option>');
                    } else {
                        $("#idSelectContacto").append('<option value="' + result[i].idContacto + '" data-tokens="' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '">' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '</option>');
                    }
                } else {
                    $("#idSelectContacto").append('<option value="' + result[i].idContacto + '" data-tokens="' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '">' + result[i].nombres + ' ' + result[i].apellidoPaterno + ' ' + result[i].apellidoMaterno + '</option>');
                }
            }
        })
        .fail(function (xhr, status, error) {

        })
        .always(function () {

        });
}

function obtenerClienteDeudor(cliente) {
    $.ajax({
        url: $('#urlObtenerClienteDeudor').val(), // Url
        data: {
            id: cliente.idCliente, // Parámetros
        },
        type: "post"  // Verbo HTTP
    })
        .done(function (result) {
            if (result) {
                $("#idClienteDeudor").show();
            } else {
                $("#idClienteDeudor").hide();
            }
        })
        .fail(function (xhr, status, error) {

        })
        .always(function () {

        });
}

function obtenerClienteOCPendiente(cliente) {
    $.ajax({
        url: $('#urlObtenerClienteOCPendiente').val(), // Url
        data: {
            id: cliente.idCliente, // Parámetros
        },
        type: "post"  // Verbo HTTP
    })
        .done(function (result) {
            if (result) {
                $("#idClienteOCPendiente").show();
            } else {
                $("#idClienteOCPendiente").hide();
            }
        })
        .fail(function (xhr, status, error) {

        })
        .always(function () {

        });
}

function obtenerCursos(modalidad) {
    $.ajax({
        url: $('#urlObtenerCursos').val(), // Url
        data: {
            modalidad: modalidad, // Parámetros
            idSucursal: $('#idSucursal').val(),
            tipoEjecucion: $('#tipoEjecucion').val()
        },
        type: "post"  // Verbo HTTP
    })
        // Se ejecuta si todo fue bien.
        .done(function (result) {
            $("#idSelectCursos").html("");
            $("#idSelectCursos").append('<option disabled data-tokens="" selected> Seleccionar </option>');
            for (var i = 0; i < result.length; i++) {
                if ($('#cotizacionNoNull').val() === 'true') {
                    if (Number($('#idCursoCotizacion').val()) === result[i].curso.idCurso) {
                        $("#idSelectCursos").append('<option selected data-tokens="' + result[i].curso.nombreCurso + '" value= "' + result[i].curso.idCurso + '">' + result[i].curso.nombreCurso + ' (' + result[i].horas + ' hrs)</option>');
                        $("#idSelectCursos").trigger("change");
                    } else {
                        $("#idSelectCursos").append('<option data-tokens="' + result[i].curso.nombreCurso + '" value= "' + result[i].curso.idCurso + '">' + result[i].curso.nombreCurso + ' (' + result[i].horas + ' hrs)</option>');
                    }
                } else {
                    $("#idSelectCursos").append('<option data-tokens="' + result[i].curso.nombreCurso + '" value="' + result[i].curso.idCurso + '">' + result[i].curso.nombreCurso + ' (' + result[i].horas + ' hrs)</option>');
                }
            }
        })
        .fail(function (xhr, status, error) {

        })
        .always(function () {

        });
    $("#saveButton").attr("style", "display:block; float:right");
    $("#loading-save").attr("style", "display:none; float:right");
}

function obtenerDatosCurso(id) {
    $.ajax({
        url: $('#urlObtenerDatosCurso').val(), // Url
        data: {
            id: id, // Parámetros
            modalidad: $('#idSelectModalidadCurso').val()
        },
        type: "post"  // Verbo HTTP
    })
        .done(function (result) {
            if ($('#cotizacionNoNull').val() === 'true') {
                if ($('#idCursoCotizacion').val() === id) {
                    if ($('#tieneCodigoSenceCotizacion').val() === "on") {
                        $('#cotizacion_tieneCodigoSence').val("on");
                        $("#uniform-idCheckBoxCodigoSENCE span").attr('class', 'checked');
                        if ($('#cursoTieneCodigoSenceCotizacion').val() === '') {
                            $("#idCheckBoxCodigoSENCE").attr("disabled", true);
                        }
                    } else {
                        $('#cotizacion_tieneCodigoSence').val("off");
                    }
                    if ($('#cursoTieneCodigoSenceCotizacion').val() !== '') {
                        $("#idButtonSENCE").css("display", "block");
                        $("#idButtonSENCE").attr("disabled", true);
                    }
                } else {
                    //Tiene R11
                    if (result !== null && result !== '') {
                        $("#idButtonR51").css("display", "none");
                        $("#idNombreDiploma").val(result.nombreDiploma);
                        //Tiene codigo SENCE
                        if (result.r11.codigoSence != null) {
                            $("#idCodigoSENCE").val(result.r11.codigoSence);
                            $("#idButtonSENCE").css("display", "block");
                            $("#idButtonSENCE").attr("disabled", true);
                            $('#idCheckBoxCodigoSENCE').attr('disabled', false);
                            $("#uniform-idCheckBoxCodigoSENCE span").attr('class', '');
                        } else {
                            $("#idButtonSENCE").css("display", "none");
                            $("#idCodigoSENCE").val("");
                            $("#uniform-idCheckBoxCodigoSENCE span").attr('class', 'checked');
                            $('#idCheckBoxCodigoSENCE').prop('checked', true);
                            $('#idCheckBoxCodigoSENCE').click(function () {
                                $("#uniform-idCheckBoxCodigoSENCE span").attr('class', 'checked');
                                return false;
                            });
                        }
                    } else {
                        $("#idNombreDiploma").val("");
                        $("#idCodigoSENCE").val("");
                        $("#idButtonR51").css("display", "block");
                        $("#idButtonSENCE").css("display", "none");
                        $("#uniform-idCheckBoxCodigoSENCE span").attr('class', 'checked');
                    }
                }
            } else {
                //Tiene R11
                if (result !== null && result !== '') {
                    $("#idButtonR51").css("display", "none");
                    $("#idNombreDiploma").val(result.nombreCurso);
                    //Tiene codigo SENCE
                    if (result.r11.codigoSence != null) {
                        $("#idCodigoSENCE").val(result.r11.codigoSence);
                        $("#idButtonSENCE").css("display", "block");
                        $('#idCheckBoxCodigoSENCE').attr('disabled', false);
                        $("#uniform-idCheckBoxCodigoSENCE span").attr('class', '');
                    } else {
                        $("#idButtonSENCE").css("display", "none");
                        $("#idCodigoSENCE").val("");
                        $("#uniform-idCheckBoxCodigoSENCE span").attr('class', 'checked');
                        $('#idCheckBoxCodigoSENCE').prop('checked', true);
                        $('#idCheckBoxCodigoSENCE').click(function () {
                            $("#uniform-idCheckBoxCodigoSENCE span").attr('class', 'checked');
                            return false;
                        });

                    }
                } else {
                    $("#idNombreDiploma").val("");
                    $("#idCodigoSENCE").val("");
                    $("#idButtonR51").css("display", "block");
                    $("#idButtonSENCE").css("display", "none");
                    $("#uniform-idCheckBoxCodigoSENCE span").attr('class', 'checked');
                }
            }
        })
        .fail(function (xhr, status, error) {

        })
        .always(function () {

        });
}

function obtenerCostos(id, modalidad, cantParticipantes, action) {

    var idCotizacion = 0;
    if (cantParticipantes == "") {
        cantParticipantes = 0;
    }
    if ($("#idCotizacion_R13").val() != null) {
        idCotizacion = $("#idCotizacion_R13").val();
    }

    $.ajax({
        url: $('#urlObtenerCostos').val(), // Url
        data: {
            id: id, // Parámetros
            modalidad: modalidad,
            cantParticipantes: cantParticipantes,
            idCotizacion,
            action
        },
        type: "post"  // Verbo HTTP
    })
        .done(function (result) {
            $("#idTableCostoDatos").html('');
            for (var i = 0; i < result.length; i++) {
                var htmlTr = '<tr><td style="border: 1px solid #e7ecf1;">' + result[i].detalle;
                var htmlTr = htmlTr + '<input id="costo_detalle_' + i + '" type="hidden" name="cotizacion.costo[' + i + '].detalle" value="' + result[i].detalle + '" /></td>';
                var htmlTr = htmlTr + '<input id="costo_valorMinimo_' + i + '" type="hidden" name="cotizacion.costo[' + i + '].valorMinimo" value="' + result[i].valorMinimo + '" /></td>';
                var htmlTr = htmlTr + '<input id="costo_valorMaximo_' + i + '" type="hidden" name="cotizacion.costo[' + i + '].valorMaximo" value="' + result[i].valorMaximo + '" /></td>';
                var htmlTr = htmlTr + '<td><input id="costo_cantidad_' + i + '" class="form-control tdCantidad" onchange="cambioCantidad(this)" type="number" min="-1"  max="99999" name="cotizacion.costo[' + i + '].cantidad" value="' + result[i].cantidad + '" /></td>';
                var htmlTr = htmlTr + '<td><input id="costo_valor_' + i + '" class="form-control tdValor currency" onchange="cambioValor(this)" type="text" min="' + result[i].valorMinimo + '"  max="' + result[i].valorMaximo + '" name="cotizacion.costo[' + i + '].valor" value="' + result[i].valor + '" /></td>';
                var htmlTr = htmlTr + '<td><input id="costo_total_' + i + '" readonly class="form-control tdTotal currency" type="text" name="cotizacion.costo[' + i + '].total" value="' + result[i].total + '" /></td></tr>';
                $("#idTableCostoDatos").append(htmlTr)
            }
            calcularTotal(cantParticipantes);
            $("#saveButton").attr("style", "display:block; float:right");
            $("#loading-save").attr("style", "display:none; float:right");
        })
        .fail(function (xhr, status, error) {

        })
        .always(function () {

        });

}

function cambioCantidad(object) {
    var total = $(object).parent().parent().find(".tdTotal").unmask();
    var valor = $(object).parent().parent().find(".tdValor").unmask().val();
    var totalNumero = $(object).unmask().val() * valor;
    total.val(totalNumero);
    var totalTotal = 0;
    $(".tdTotal").each(function () {
        totalTotal += Number($(this).unmask().val());
    });
    $('#idTotalTotal').html(new Intl.NumberFormat('es-CL', { currency: 'CLP', style: 'currency' }).format(totalTotal));
};

function cambioValor(object) {
    var total = $(object).parent().parent().find(".tdTotal").unmask();
    var cantidad = $(object).parent().parent().find(".tdCantidad").unmask().val();
    var totalNumero = $(object).unmask().val() * cantidad;
    total.val(totalNumero);
    var totalTotal = 0;
    $(".tdTotal").each(function () {
        totalTotal += Number($(this).unmask().val());
    });
    $('#idTotalTotal').html(new Intl.NumberFormat('es-CL', { currency: 'CLP', style: 'currency' }).format(totalTotal));
};

function aportesCapacitacion(tipoEjecucion) {
    if ($('#idSelectTipoCurso').val() === "Arriendo de Sala" || $('#idSelectTipoCurso').val() === "Duplicado Credencial" || $('#idSelectTipoCurso').val() === "Tramitación Licencia") {
        $('#tbodyPresencial').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodySincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyAsincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacion').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionSincronica').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionAsincronica').find('input, textarea, button, select').attr('disabled', true);
        return;
    }
    if (tipoEjecucion.val() === '0') {
        $('#tbodyPresencial').show();
        $('#tbodySincrono').hide();
        $('#tbodyAsincrono').hide();
        $('#tbodyRecertificacion').hide();
        $('#tbodyRecertificacionSincronica').hide();
        $('#tbodyRecertificacionAsincronica').hide();
        $('#tbodyPresencial').find('input, textarea, button, select').attr('disabled', false);
        $('#tbodySincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyAsincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacion').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionSincronica').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionAsincronica').find('input, textarea, button, select').attr('disabled', true);

        //preseleccionarInsecap($('#tbodyPresencial'));
    }
    if (tipoEjecucion.val() === '1') {
        $('#tbodyPresencial').hide();
        $('#tbodySincrono').show();
        $('#tbodyAsincrono').hide();
        $('#tbodyRecertificacion').hide();
        $('#tbodyRecertificacionSincronica').hide();
        $('#tbodyRecertificacionAsincronica').hide();
        $('#tbodyPresencial').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodySincrono').find('input, textarea, button, select').attr('disabled', false);
        $('#tbodyAsincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacion').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionSincronica').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionAsincronica').find('input, textarea, button, select').attr('disabled', true);

        //preseleccionarInsecap($('#tbodySincrono'));
    }
    if (tipoEjecucion.val() === '2') {
        $('#tbodyPresencial').hide();
        $('#tbodySincrono').hide();
        $('#tbodyAsincrono').show();
        $('#tbodyRecertificacion').hide();
        $('#tbodyRecertificacionSincronica').hide();
        $('#tbodyRecertificacionAsincronica').hide();
        $('#tbodyPresencial').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodySincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyAsincrono').find('input, textarea, button, select').attr('disabled', false);
        $('#tbodyRecertificacion').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionSincronica').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionAsincronica').find('input, textarea, button, select').attr('disabled', true);

        //preseleccionarInsecap($('#tbodyAsincrono'));
    }
    if (tipoEjecucion.val() === '3') {
        $('#tbodyPresencial').hide();
        $('#tbodySincrono').hide();
        $('#tbodyAsincrono').hide();
        $('#tbodyRecertificacion').show();
        $('#tbodyRecertificacionSincronica').hide();
        $('#tbodyRecertificacionAsincronica').hide();
        $('#tbodyPresencial').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodySincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyAsincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacion').find('input, textarea, button, select').attr('disabled', false);
        $('#tbodyRecertificacionSincronica').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionAsincronica').find('input, textarea, button, select').attr('disabled', true);

        //preseleccionarInsecap($('#tbodyRecertificacion'));
    }
    if (tipoEjecucion.val() === '4') {
        $('#tbodyPresencial').hide();
        $('#tbodySincrono').hide();
        $('#tbodyAsincrono').hide();
        $('#tbodyRecertificacion').hide();
        $('#tbodyRecertificacionSincronica').show();
        $('#tbodyRecertificacionAsincronica').hide();
        $('#tbodyPresencial').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodySincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyAsincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacion').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionSincronica').find('input, textarea, button, select').attr('disabled', false);
        $('#tbodyRecertificacionAsincronica').find('input, textarea, button, select').attr('disabled', true);

        //preseleccionarInsecap($('#tbodyRecertificacionSincronica'));
    }
    if (tipoEjecucion.val() === '5') {
        $('#tbodyPresencial').hide();
        $('#tbodySincrono').hide();
        $('#tbodyAsincrono').hide();
        $('#tbodyRecertificacion').hide();
        $('#tbodyRecertificacionSincronica').hide();
        $('#tbodyRecertificacionAsincronica').show();
        $('#tbodyPresencial').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodySincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyAsincrono').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacion').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionSincronica').find('input, textarea, button, select').attr('disabled', true);
        $('#tbodyRecertificacionAsincronica').find('input, textarea, button, select').attr('disabled', false);

        //preseleccionarInsecap($('#tbodyRecertificacionAsincronica'));
    }
}

function calcularTotal(cantParticipantes) {
    var total = 0;
    $(".tdTotal").each(function () {
        total += Number($(this).unmask().val());
    });

    if (typeof (cantParticipantes) === "undefined" || cantParticipantes == 0) {
        cantParticipantes = 1;
    }

    var valorPorParticipante = total / cantParticipantes;

    $('#idTotalTotal').html(new Intl.NumberFormat('es-CL', { currency: 'CLP', style: 'currency' }).format(total));
    $('#idValorPorParticipante').html(new Intl.NumberFormat('es-CL', { currency: 'CLP', style: 'currency' }).format(valorPorParticipante));
}

//function preseleccionarInsecap(elemento) {
//    seleccionado = false;
//    $.each(elemento.find('input[value="0"]'), function (i, val) {
//        console.log("0 " + elemento.find('input[value="0"]').get(i).checked);
//        if (elemento.find('input[value="0"]').get(i).checked) {
//            seleccionado = true;
//        }
//        console.log("1 " + elemento.find('input[value="1"]').get(i).checked);
//        if (elemento.find('input[value="1"]').get(i).checked) {
//            seleccionado = true;
//        }
//        console.log("2 " + elemento.find('input[value="2"]').get(i).checked);
//        if (elemento.find('input[value="2"]').get(i).checked) {
//            seleccionado = true;
//        }
//    });
//    console.log(seleccionado);
//    if (!seleccionado) {
//        elemento.find('input[value="0"]').parent().attr('class', "iradio_minimal-grey checked");
//    }
//}

$(window).on('load', function () {
    var total = 0;
    $(".tdTotal").each(function () {
        total += Number($(this).unmask().val());
    });

    var cantParticipantes = $("#cantParticipantes").val();
    if ($("#cantParticipantes").val() == "") {
        cantParticipantes = 1;
    }

    var valorPorParticipante = total / cantParticipantes;

    $('#idValorPorParticipante').html(new Intl.NumberFormat('es-CL', { currency: 'CLP', style: 'currency' }).format(valorPorParticipante));
});