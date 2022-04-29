$(document).ready(function () {
    $('#mostrarAgregarPregunta').click(function () {
        $('#idTipoRespuesta').trigger('change');
    });

    $(function () {
        $("#RespuestasAlternativas").sortable();
        $("#RespuestasAlternativas").disableSelection();
        $("#idPreguntasCreadas").sortable();
        $("#idPreguntasCreadas").disableSelection();
    });

    $('#idTipoRespuesta').on('change', function () {
        if ($(this).val() == "Alternativas") {
            $("#idBotonAgregarRespuesta").show();
            $("#idRespuestaUnica").show();
        } else {
            $("#idBotonAgregarRespuesta").hide();
            $("#idRespuestaUnica").hide();
            $("#RespuestasAlternativas").html('');
        }
    });

    $("#RespuestasAlternativas").sortable({
        stop: function (event, ui) {
            //alert("New position: " + ui.item.index());
        }
    });

    $('#idBotonAgregarRespuesta').click(function () {
        var htmlRespuesta = '<div class="row"><div class="col-md-12">';
        htmlRespuesta += '<div class="row" style="margin-bottom:5px;">';
        htmlRespuesta += '<div class="col-md-7"><input class="form-control" name="respuestaAgregarTexto" placeholder="Respuesta"/></div>';
        htmlRespuesta += '<div class="col-md-3"><input class="form-control" onchange="validarPuntaje($(this))" placeholder="Puntaje" type="number" name="puntajeAgregarTexto" value="0"/></div>';
        htmlRespuesta += '<div class="col-md-2 text-center"><button type="button" class="btn btn-danger glyphicon glyphicon-trash" onClick="return eliminarRespuesta($(this))"></button></div>'
        htmlRespuesta += '</div></div></div>';
        $("#RespuestasAlternativas").append(htmlRespuesta);
    });

    $('#idAgregarPreguntaFinal').click(function () {
        //if ($("#idPreguntaIngresada").val() == null || $("#idPreguntaIngresada").val() == '') {
        //    alert("Debe ingresar una pregunta");
        //    return;
        //}
        var cerrarModal = true;
        var htmlAgregarPregunta = '<div class="row"><div class="col-md-12">';
        htmlAgregarPregunta += '<hr style="margin:15px" />';
        htmlAgregarPregunta += '<div class="row">';


        if ($('#idTipoRespuesta').val() == "Abierta") {
            if ($("#idObligatoria").prop('checked')) {
                htmlAgregarPregunta += '<div class="col-md-10">' + $("#idPreguntaIngresada").val() + ' *</div>';
                htmlAgregarPregunta += '<div class="col-md-2"><button type="button" class="btn btn-danger btn-sm pull-right glyphicon glyphicon-trash" onClick="return eliminarRespuesta($(this))"></button></div>'
                htmlAgregarPregunta += '<input type="hidden" name="pregunta" obligatoria="true" tipo="0" value="' + $("#idPreguntaIngresada").val() + '" >';
            } else {
                htmlAgregarPregunta += '<div class="col-md-10">' + $("#idPreguntaIngresada").val() + '</div>';
                htmlAgregarPregunta += '<div class="col-md-2"><button type="button" class="btn btn-danger btn-sm pull-right glyphicon glyphicon-trash" onClick="return eliminarRespuesta($(this))"></button></div>'
                htmlAgregarPregunta += '<input type="hidden" name="pregunta" obligatoria="false" tipo="0" value="' + $("#idPreguntaIngresada").val() + '" >';
            }
        }
        if ($('#idTipoRespuesta').val() == "Corta") {
            if ($("#idObligatoria").prop('checked')) {
                htmlAgregarPregunta += '<div class="col-md-10">' + $("#idPreguntaIngresada").val() + ' *</div>';
                htmlAgregarPregunta += '<div class="col-md-2"><button type="button" class="btn btn-danger btn-sm pull-right glyphicon glyphicon-trash" onClick="return eliminarRespuesta($(this))"></button></div>'
                htmlAgregarPregunta += '<input type="hidden" name="pregunta" obligatoria="true" tipo="1" value="' + $("#idPreguntaIngresada").val() + '" >';
            } else {
                htmlAgregarPregunta += '<div class="col-md-10">' + $("#idPreguntaIngresada").val() + '</div>';
                htmlAgregarPregunta += '<div class="col-md-2"><button type="button" class="btn btn-danger btn-sm pull-right glyphicon glyphicon-trash" onClick="return eliminarRespuesta($(this))"></button></div>'
                htmlAgregarPregunta += '<input type="hidden" name="pregunta" obligatoria="false" tipo="1" value="' + $("#idPreguntaIngresada").val() + '" >';
            }
        }
        if ($('#idTipoRespuesta').val() == "Alternativas") {
            if ($("#idObligatoria").prop('checked')) {
                htmlAgregarPregunta += '<div class="col-md-10">' + $("#idPreguntaIngresada").val() + ' *</div>';
                htmlAgregarPregunta += '<div class="col-md-2"><button type="button" class="btn btn-danger btn-sm pull-right glyphicon glyphicon-trash" onClick="return eliminarRespuesta($(this))"></button></div>'
                htmlAgregarPregunta += '<input type="hidden" name="pregunta" obligatoria="true" tipo="2" value="' + $("#idPreguntaIngresada").val() + '" >';
            } else {
                htmlAgregarPregunta += '<div class="col-md-10">' + $("#idPreguntaIngresada").val() + '</div>';
                htmlAgregarPregunta += '<div class="col-md-2"><button type="button" class="btn btn-danger btn-sm pull-right glyphicon glyphicon-trash" onClick="return eliminarRespuesta($(this))"></button></div>'
                htmlAgregarPregunta += '<input type="hidden" name="pregunta" obligatoria="false" tipo="2" value="' + $("#idPreguntaIngresada").val() + '" >';
            }
        }

        htmlAgregarPregunta += '</div>';
        htmlAgregarPregunta += '<div class="row"><div class="col-md-10">';
        if ($('#idTipoRespuesta').val() == "Abierta") {
            htmlAgregarPregunta += '<textarea class="form-control" style="margin-top: 10px;" disabled></textarea>';
            htmlAgregarPregunta += '<input type="hidden" name="respuesta" value="Abierta" >';
        }
        if ($('#idTipoRespuesta').val() == "Corta") {
            htmlAgregarPregunta += '<textarea class="form-control" style="margin-top: 10px;" rows="1" disabled></textarea>';
            htmlAgregarPregunta += '<input type="hidden" name="respuesta" value="Corta" >';
        }
        if ($('#idTipoRespuesta').val() == "Alternativas") {
            $.each($('#RespuestasAlternativas').find('input'), function () {
                if ($(this).attr('name') == 'respuestaAgregarTexto') {
                    if ($(this).val() == null || $(this).val() == '') {
                        alert("Debe ingresar la alternativa");
                        cerrarModal = false;
                        return;
                    }
                    htmlAgregarPregunta += '<div class="form-check disabled">';
                    htmlAgregarPregunta += '<input class="form-check-input" type="radio" id="idRespuesta" value="optionCheckBoxUnique" style="margin-top: 10px;" disabled>';
                    htmlAgregarPregunta += '<label class="form-check-label" for="idRespuesta" style="display: inline;">' + $(this).val() + '</label>';
                    htmlAgregarPregunta += '</div>';
                    htmlAgregarPregunta += '<input type="hidden" name="respuesta" value="' + $(this).val() + '" >';
                } else if ($(this).attr('name') == 'puntajeAgregarTexto') {
                    if ($(this).val() == null || $(this).val() == '') {
                        alert("Debe ingresar el puntaje de la alternativa");
                        cerrarModal = false;
                        return;
                    }
                    htmlAgregarPregunta += '<input type="hidden" name="puntaje" value="' + $(this).val() + '" >';
                } else {
                    return;
                }
            });
        }
        htmlAgregarPregunta += '</div></div></div></div>';
        if (cerrarModal) {
            $('#idModalAgregarPregunta').modal('toggle');
            $("#idPreguntasCreadas").append(htmlAgregarPregunta);
        }
    });
});

function eliminarRespuesta(componente) {
    componente.parent('div').parent('.row').parent('div').parent('.row').remove()
};

function validarPuntaje(puntaje) {
    $('#errorPuntaje').hide()
    re = /^[0-9]?[0-9]?[0-9]$/;
    if (!re.test(String(puntaje.val()))) {
        puntaje.val(0);
        $('#errorPuntaje').show()
    }
    if (puntaje.val() < 0) {
        puntaje.val(0);
        $('#errorPuntaje').show()
    }
    if (puntaje.val() > 100) {
        puntaje.val(0);
        $('#errorPuntaje').show()
    }
}