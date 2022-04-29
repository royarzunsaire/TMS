$(document).ready(function () {
    // -------- submit --------
    $("#btnGuardar").click(function () {
        $('#modalConfirmar').modal({ keyboard: true }, 'show');
    });
    $("#btnConfirmar").click(function () {
        $("#form").submit();
        $("#btnConfirmar").prop("disabled", true)
    });
    // -------- tipo pago --------
    mostrarOtic();
    cantPago = $("#cantPago").val();
    $("#btnPago").click(function () {
        pago = {
            tipo: $("#tipoPago option:selected").text(),
            value: $($("#tipoPago")).val()
        };
        docCompromiso = {
            tipo: "OC",
            value: 0
        };
        if ($($("#tipoPago")).val() === "2") {
            if ($("#tipoDocCompromiso option:selected").text() === "") {
                return;
            }
            docCompromiso = {
                tipo: $("#tipoDocCompromiso option:selected").text(),
                value: $("#tipoDocCompromiso").val()
            };
        }
        if ($($("#tipoPago")).val() === "0") {
            otic = {
                tipo: $("#otic option:selected").text(),
                value: $("#otic").val()
            };
            //if (existeTipoPagoOtic(pago, otic, cantPago)) {
            //    return;
            //}
            $("#pago").find('tbody')
                .append($('<tr>')
                    .attr("id", "rowPago" + cantPago)
                    .append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .text(pago.tipo)
                        .append($('<input>')
                            .attr("name", "tipoPago" + cantPago)
                            .attr("id", "tipoPago" + cantPago)
                            .attr("type", "hidden")
                            .val(pago.value)
                        )
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .text(otic.tipo)
                        .append($('<input>')
                            .attr("name", "otic" + cantPago)
                            .attr("id", "otic" + cantPago)
                            .attr("type", "hidden")
                            .val(otic.value)
                        )
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .text(docCompromiso.tipo)
                        .append($('<input>')
                            .attr("name", "tipoDocCompromiso" + cantPago)
                            .attr("type", "hidden")
                            .val(docCompromiso.value)
                        )
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .append($('<input>')
                            .attr("class", "form-control")
                            .attr("name", "identificadorDocCompromiso" + cantPago)
                        )
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .append($('<input>')
                            .attr("class", "form-control currency")
                            .attr("name", "montoDocCompromiso" + cantPago)
                            .attr("type", "text")
                        )
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .append($('<input>')
                            .attr("name", "docCompromiso" + cantPago)
                            .attr("type", "file")
                            .attr("accept", "application/pdf")
                        )
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .attr("class", "text-center")
                        .append($('<button>')
                            .append($('<i>')
                                .attr("class", "fa fa-trash")
                            )
                            .attr("class", "btn btn-danger btn-sm delete")
                            .attr("type", "button")
                            .attr("onclick", "borrarPago(" + cantPago + ")")
                        )
                    )
                );
        } else {
            otic = {
                tipo: $("#otic option:selected").text(),
                value: $("#otic").val()
            };
            //if (existeTipoPago(pago, cantPago)) {
            //    return;
            //}
            $("#pago").find('tbody')
                .append($('<tr>')
                    .attr("id", "rowPago" + cantPago)
                    .append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .text(pago.tipo)
                        .append($('<input>')
                            .attr("name", "tipoPago" + cantPago)
                            .attr("id", "tipoPago" + cantPago)
                            .attr("type", "hidden")
                            .val(pago.value)
                        )
                    ).append($('<td>')
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .text(docCompromiso.tipo)
                        .append($('<input>')
                            .attr("name", "tipoDocCompromiso" + cantPago)
                            .attr("type", "hidden")
                            .val(docCompromiso.value)
                        )
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .append($('<input>')
                            .attr("class", "form-control")
                            .attr("name", "identificadorDocCompromiso" + cantPago)
                        )
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .append($('<input>')
                            .attr("class", "form-control currency")
                            .attr("name", "montoDocCompromiso" + cantPago)
                            .attr("type", "text")
                        )
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .append($('<input>')
                            .attr("name", "docCompromiso" + cantPago)
                            .attr("type", "file")
                            .attr("accept", "application/pdf")
                        )
                    ).append($('<td>')
                        .attr("style", "vertical-align: middle;")
                        .attr("class", "text-center")
                        .append($('<button>')
                            .append($('<i>')
                                .attr("class", "fa fa-trash")
                            )
                            .attr("class", "btn btn-danger btn-sm delete")
                            .attr("type", "button")
                            .attr("onclick", "borrarPago(" + cantPago + ")")
                        )
                    )
                );
        }
        cantPago++;
        $("#cantPago").val(cantPago);
    });
    
    $($("#tipoPago")).change(function () {
        mostrarOtic();
        mostrarTiposDocCompromiso();
    });
    // -------- relator --------
    cantRelator = $("#cantRelator").val();
    $("#btnRelator").click(function () {
        datosRelator = $("#relatores option:selected").text().split('] ');
        datosRelator[0] = datosRelator[0].replace('[', '');
        relator = {
            nombre: datosRelator[1],
            rut: datosRelator[0],
            value: $("#relatores").val()
        };
        if (relator.value === null) {
            return;
        }
        if (existe(relator, cantRelator)) {
            return;
        }
        agregarRelator(relator, cantRelator);
        cantRelator++;
        $("#cantRelator").val(cantRelator);
        actualizarRelatoresBloque();
    });
    $("#btnRelatorSence").click(function () {
        datosRelator = $("#relatoresSence option:selected").text().split('] ');
        datosRelator[0] = datosRelator[0].replace('[', '');
        relator = {
            nombre: datosRelator[1],
            rut: datosRelator[0],
            value: $("#relatoresSence").val()
        };
        if (relator.value === null) {
            return;
        }
        if (existe(relator, cantRelator)) {
            return;
        }
        agregarRelatorSence(relator, cantRelator, 'Sí');
        cantRelator++;
        $("#cantRelator").val(cantRelator);
    });
    $("#btnRelatorNoSence").click(function () {
        datosRelator = $("#relatoresNoSence option:selected").text().split('] ');
        datosRelator[0] = datosRelator[0].replace('[', '');
        relator = {
            nombre: datosRelator[1],
            rut: datosRelator[0],
            value: $("#relatoresNoSence").val()
        };
        if (relator.value === null) {
            return;
        }
        if (existe(relator, cantRelator)) {
            return;
        }
        agregarRelatorSence(relator, cantRelator, 'No');
        cantRelator++;
        $("#cantRelator").val(cantRelator);
    });
    $("#btnInfoRelatores").click(function () {
        relatores = $("#relator").val();
        $('#modalInfoRelatores').modal({ keyboard: true }, 'show');
    });
    mostrarTiposDocCompromiso();
});
// -------- funciones --------
function borrarPago(numeroPago) {
    $("#rowPago" + numeroPago).empty();
}
function mostrarOtic() {
    if ($($("#tipoPago")).val() === "0") {
        $("#oticForm").show();
    }
    else {
        $("#oticForm").hide();
    }
}
function mostrarTiposDocCompromiso() {
    if ($($("#tipoPago")).val() === "2") {
        $("#tipoDocCompromisoForm").show();
    }
    else {
        $("#tipoDocCompromisoForm").hide();
    }
}
function existe(relator, cantRelator) {
    for (i = 0; i < cantRelator; i++) {
        if ($('#relator' + i).val() === relator.value) {
            return true;
        }
    }
    return false;
}
function existeTipoPago(pago, cantPago) {
    for (i = 0; i < cantPago; i++) {
        if ($('#tipoPago' + i).val() === pago.value) {
            return true;
        }
    }
    return false;
}
function existeTipoPagoOtic(pago, otic, cantPago) {
    for (i = 0; i < cantPago; i++) {
        if ($('#otic' + i).val() === otic.value) {
            return true;
        }
    }
    return false;
}
function agregarRelatorSence(relator, cantRelator, sence) {
    if (sence === "Sí") {
        alerta = "label label-success bg-green-soft font-white";
    } else {
        alerta = "";
    }
    $("#relator").find('tbody')
        .append($('<tr>')
            .attr("id", "rowRelator" + cantRelator)
            .append($('<td>')
                .attr("style", "vertical-align: middle;")
                .text(relator.nombre)
                .append($('<input>')
                    .attr("name", "relator" + cantRelator)
                    .attr("id", "relator" + cantRelator)
                    .attr("type", "hidden")
                    .val(relator.value)
                ).append($('<input>')
                    .attr("id", "relatorNombre" + cantRelator)
                    .attr("type", "hidden")
                    .val(relator.nombre)
                )
            ).append($('<td>')
                .attr("style", "vertical-align: middle;")
                .text(relator.rut)
            ).append($('<td>')
                .attr("style", "vertical-align: middle;")
                .append($('<div>')
                    .attr("class", alerta)
                    .text(sence)
                    )
            ).append($('<td>')
                .attr("style", "vertical-align: middle;")
                .attr("class", "text-center")
                .append($('<button>')
                    .append($('<i>')
                        .attr("class", "fa fa-trash")
                    )
                    .attr("class", "btn btn-danger btn-sm")
                    .attr("type", "button")
                    .attr("onclick", "borrarRelator(" + cantRelator + ")")
                )
            )
        );
}
function agregarRelator(relator, cantRelator) {
    $("#relator").find('tbody')
        .append($('<tr>')
            .attr("id", "rowRelator" + cantRelator)
            .append($('<td>')
                .attr("style", "vertical-align: middle;")
                .text(relator.nombre)
                .append($('<input>')
                    .attr("name", "relator" + cantRelator)
                    .attr("id", "relator" + cantRelator)
                    .attr("type", "hidden")
                    .val(relator.value)
                ).append($('<input>')
                    .attr("id", "relatorNombre" + cantRelator)
                    .attr("type", "hidden")
                    .val(relator.nombre)
                )
            ).append($('<td>')
                .attr("style", "vertical-align: middle;")
                .text(relator.rut)
            ).append($('<td>')
                .attr("style", "vertical-align: middle;")
                .attr("class", "text-center")
                .append($('<button>')
                    .append($('<i>')
                        .attr("class", "fa fa-trash")
                    )
                    .attr("class", "btn btn-danger btn-sm")
                    .attr("type", "button")
                    .attr("onclick", "borrarRelator(" + cantRelator + ")")
                )
            )
        );
}
function borrarRelator(numeroRelator) {
    $("#rowRelator" + numeroRelator).empty();
    actualizarRelatoresBloque();
}