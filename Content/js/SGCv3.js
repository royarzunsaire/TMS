$(document).ready(function () {
    $('.currency').mask("#.##0", {
        reverse: true,
        autoUnmask: true,
        removeMaskOnSubmit: true
    });
    $(".datepicker").datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true
    });
    $(".timepicker").timepicker({
        use24hours: true,
        format: 'HH:mm',
        minuteStep: 10,
    });


});


var handleSelect2 = function () {
    var e = function () {
        function e(e) {
            if (e.loading) return e.text;
            var t = "<div class='select2-result-repository clearfix'><div class='select2-result-repository__avatar'><img src='" + e.owner.avatar_url + "' /></div><div class='select2-result-repository__meta'><div class='select2-result-repository__title'>" + e.full_name + "</div>"; return e.description && (t += "<div class='select2-result-repository__description'>" + e.description + "</div>"), t += "<div class='select2-result-repository__statistics'><div class='select2-result-repository__forks'><span class='glyphicon glyphicon-flash'></span> " + e.forks_count + " Forks</div><div class='select2-result-repository__stargazers'><span class='glyphicon glyphicon-star'></span> " + e.stargazers_count + " Stars</div><div class='select2-result-repository__watchers'><span class='glyphicon glyphicon-eye-open'></span> " + e.watchers_count + " Watchers</div></div></div></div>"
        }
        function t(e) {
            return e.full_name || e.text
        }
        $.fn.select2.defaults.set("theme", "bootstrap");
        var s = "Select a State";
        $('.select3').select2();
        $(".select2, .select2-multiple").select2({ placeholder: s, width: null }),
            $(".select2-allow-clear").select2({ allowClear: !0, placeholder: s, width: null }), $(".js-data-example-ajax").select2({ width: "off", ajax: { url: "https://api.github.com/search/repositories", dataType: "json", delay: 250, data: function (e) { return { q: e.term, page: e.page } }, processResults: function (e, t) { return { results: e.items } }, cache: !0 }, escapeMarkup: function (e) { return e }, minimumInputLength: 1, templateResult: e, templateSelection: t }), $("button[data-select2-open]").click(function () { $("#" + $(this).data("select2-open")).select2("open") }), $(":checkbox").on("click", function () { $(this).parent().nextAll("select").prop("disabled", !this.checked) }), $(".select2, .select2-multiple, .select2-allow-clear, .js-data-example-ajax").on("select2:open", function () { if ($(this).parents("[class*='has-']").length) for (var e = $(this).parents("[class*='has-']")[0].className.split(/\s+/), t = 0; t < e.length; ++t) e[t].match("has-") && $("body > .select2-container").addClass(e[t]) }), $(".js-btn-set-scaling-classes").on("click", function () { $("#select2-multiple-input-sm, #select2-single-input-sm").next(".select2-container--bootstrap").addClass("input-sm"), $("#select2-multiple-input-lg, #select2-single-input-lg").next(".select2-container--bootstrap").addClass("input-lg"), $(this).removeClass("btn-primary btn-outline").prop("disabled", !0) })
    }; return { init: function () { e() } }
}();

var handleTables = function () {
    var initTables = function () {
        var table = $('#sys-table');
        var oTable = table.dataTable({
            stateSave: true,
            //"language": {
            //    "aria": {
            //        "sortAscending": ": activate to sort column ascending",
            //        "sortDescending": ": activate to sort column descending"
            //    },
            //    "emptyTable": "No data available in table",
            //    "info": "Showing _START_ to _END_ of _TOTAL_ entries",
            //    "infoEmpty": "No entries found",
            //    "infoFiltered": "(filtered1 from _MAX_ total entries)",
            //    "lengthMenu": "_MENU_ entries",
            //    "search": "Filter:",
            //    "zeroRecords": "No matching records found"
            //},
            scrollY: "900px",
        scrollCollapse: true,
            "language": {
                "lengthMenu": "Mostrar _MENU_ registros",
                "zeroRecords": "No se encontraron resultados",
                "info": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
                "infoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
                "infoFiltered": "(filtrado de un total de _MAX_ registros)",
                "sSearch": "Buscar:",
                "oPaginate": {
                    "sFirst": "Primero",
                    "sLast": "Último",
                    "sNext": "Siguiente",
                    "sPrevious": "Anterior"
                },
                "sProcessing": "Procesando...",
            },
            buttons: [
                //{ extend: 'print', className: 'btn dark btn-outline' },
                //{ extend: 'pdf', className: 'btn green btn-outline' },
                //{ extend: 'csv', className: 'btn purple btn-outline ' }
            ],
            responsive: {
                details: {

                }
            },
            //responsive: true,
            //order: false,
            "order": [
                //[0, 'asc']
            ],

            "lengthMenu": [
                [5, 10, 15, 20, -1],
                [5, 10, 15, 20, "All"]
            ],
            "pageLength": 10,

            "dom": "<'row' <'col-md-12'B>><'row'<'col-md-6 col-sm-12'l><'col-md-6 col-sm-12'f>r><'table-scrollable't><'row'<'col-md-5 col-sm-12'i><'col-md-7 col-sm-12'p>>", // horizobtal scrollable datatable
        });
        $('.dataTables_length select').addClass("select2");
    };

    return {
        init: function () {
            if (!jQuery().dataTable) {
                return;
            }

            initTables();
        }

    };
}();

var UIModals = function () {

    var handleModals = function () {
        $(".modalx").draggable({
            handle: ".modal-header"
        });
    }

    return {
        init: function () {
            handleModals();
        }

    };

}();

function playAudio(file) {
    if (file === 'alert')
        document.getElementById('audio-alert').play();

    if (file === 'fail')
        document.getElementById('audio-fail').play();
}

function showNotification(msg, msgType) {
    if (msg.length > 3) {
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "positionClass": "toast-bottom-right",
            "onclick": null,
            "showDuration": "1000",
            "hideDuration": "1000",
            "timeOut": "10000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        }
        if (msgType === 'S') {
            playAudio('alert');
            toastr.success(msg, "Operacion Exitosa")
        }
        else if (msgType === 'E') {
            playAudio('fail');
            toastr.error(msg, "Error!")
        }
        else if (msgType === 'I') {
            playAudio('fail');
            toastr.info(msg, "Informacion!")
        }
        else if (msgType === 'W') {
            playAudio('alert');
            toastr.warning(msg, "Alerta!")
        }
        else {
            playAudio('fail');
            toastr.error(msg, "Error Desconocido")
        }
    }
}

function showDialog(controller, action, formType) {
    var url = "/" + controller + "/" + action;
    $('#modal-content').load(url, function () {
        $('#modal-container').modal({ keyboard: true }, 'show');
    });

}

function showEditDialog(controller, action, itemID) {
    var url = "/" + controller + "/" + action + "/" + itemID;
    $('#modal-content').load(url, function () {
        $('#modal-container').modal({ keyboard: true }, 'show');
    });

}

function loadModal(modalName, modalContent, controllerurl, parameters) {
    console.log(parameters );

    if (parameters.idDropDownList == "idSelectRepresentanteLegal") {
        if (parameters.idCliente == null) {
            alert("Seleccione un rut cliente");
            return;
        }
    }
    if (parameters.idDropDownList == "idSelectEncargadoPago") {
        if (parameters.idCliente == null) {
            alert("Seleccione un rut cliente");
            return;
        }
    }
    if (parameters.idDropDownList == "idSelectContacto") {
        if (parameters.idCliente == null) {
            alert("Seleccione un rut cliente");
            return;
        }
    }
    $.ajax({
        type: "POST",
        url: controllerurl,
        contentType: "application/json; charset=utf-8",
        headers: { "RequestVerificationToken": $(document).find('input[name="__RequestVerificationToken"]').val() },
        data: JSON.stringify(parameters),
        datatype: "json",
        success: function (data) {

            $('#' + modalContent).html(data);

            $('#' + modalName).modal('show');
        },
        error: function () {
            alert("Dynamic content load failed.");
        }
    });
}



function loadDropdrownList(idElement, controllerurl, parameters) {


    $.ajax({
        type: "POST",
        url: controllerurl,
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(parameters),
        datatype: "json",
        success: function (data) {
            $('#' + idElement).html('')
            //debugger
            var div_data = "<option value=''>Seleccionar</option>";
            $(div_data).appendTo('#' + idElement);
            $.each(data, function (i, obj) {
                var div_data = "<option value=" + obj.Value + ">" + obj.Text + "</option>";
                $(div_data).appendTo('#' + idElement);
            });
        },
        error: function () {
            toastr.error("Hubo en error al cargar el DropdownList " + idElement, "Error")
        }
    });
}

function saveModalCreateContacto(idDropdownList) {
    console.log(idDropdownList);
    event.preventDefault();

    var form = $("#formModalCreate");
    $('#modalCreateSubmit').addClass("btn-disabled");
    $.ajax({
        url: form.attr("action"),
        method: form.attr("method"),  // post
        headers: { "RequestVerificationToken": form.find('input[name="__RequestVerificationToken"]').val() },
        data: form.serialize(),
        success: function (data, textStatus, jqXHR) {
            console.log(textStatus + ',' + jqXHR);
            $("#myModalContent").html(data);
        }
        ,
        error: function (error) {
            $('#myModal').modal('hide');
            //playAudio('fail');
            toastr.error("Hubo un error al crear el usuario error:" + error.responseText, "Error!")
        },
        statusCode: {
            202: function () {
                $('#myModal').modal('hide');
                if ("idSelectEncargadoPago" == idDropdownList) {
                    $("#idSelectRut").trigger("change");

                } else if ("idSelectRepresentanteLegal" == idDropdownList) {
                    $("#idSelectRut").trigger("change");

                } else if ("idSelectContacto" == idDropdownList) {
                    $("#idSelectRut").trigger("change");

                } else if ("idSelectContactoC" == idDropdownList) {
                    loadDropdrownList(idDropdownList, "/Comercializacions/CargarContactos", { id : $("#idCotizacion").val() })

                } else if ("idSelectEncargadoPagoC" == idDropdownList) {
                    loadDropdrownList(idDropdownList, "/Comercializacions/CargarEncargadosPago", { id : $("#idCotizacion").val() })

                } else {
                    loadDropdrownList(idDropdownList, "/Contacto/List")

                }
                toastr.success('Se creo exitosamente', 'Contacto');
            }
        }
    });
}
function saveModalCreateMandante (idDropdownList) {
    event.preventDefault();

    var form = $("#formModalCreate");
    $('#modalCreateSubmit').addClass("btn-disabled");
    $.ajax({
        url: form.attr("action"),
        method: form.attr("method"),  // post
        headers: { "RequestVerificationToken": form.find('input[name="__RequestVerificationToken"]').val() },
        data: form.serialize(),
        success: function (data, textStatus, jqXHR) {
            console.log(textStatus + ',' + jqXHR);
            console.log('aahh');
            $("#myModalContent").html(data);
        },
        error: function (error) {
            $('#myModal').modal('hide');
            playAudio('fail');
            toastr.error("Hubo un error al crear el usuario error:" + error.responseText, "Error!")
        },
        statusCode: {
            202: function () {
                console.log(idDropdownList);
                $('#myModal').modal('hide');
                loadDropdrownList(idDropdownList, "/Mandante/List")
                toastr.success('Se creo exitosamente', 'Mandante');
            }
        }
    });
}
jQuery(document).ready(function () {
    handleSelect2.init();
    handleTables.init();
    UIModals.init();
    $('.dataTable').dataTable();

});

function onRutBlur(obj) {
    if (!VerificaRut(obj.value)) {

        $('#errorRut').html('Error: Formato Incorrecto');
        $('#errorRut').show();
    }
    else {
        $('#errorRut').hide();
    }

}
$("#rut,#run").focusout(function () {
    //if (Fn.validaRut($("#rut,#run").val())) {
    if (rutValido($("#rut,#run").val())) {
        $("#msgerror").html("");
        $("#rut,#run").val(rutFormato($("#rut,#run").val()));
    } else {
        toastr.error("ingrese un RUT valido", "RUT no valido");
        $("#rut,#run").val("");
    }
});

//function VerificaRut(rut) {
//    if (rut.toString().trim() != '' && rut.toString().indexOf('-') > 0) {
//        var caracteres = new Array();
//        var serie = new Array(2, 3, 4, 5, 6, 7);
//        var dig = rut.toString().substr(rut.toString().length - 1, 1);
//        rut = rut.toString().substr(0, rut.toString().length - 2);

//        for (var i = 0; i < rut.length; i++) {
//            caracteres[i] = parseInt(rut.charAt((rut.length - (i + 1))));
//        }

//        var sumatoria = 0;
//        var k = 0;
//        var resto = 0;

//        for (var j = 0; j < caracteres.length; j++) {
//            if (k == 6) {
//                k = 0;
//            }
//            sumatoria += parseInt(caracteres[j]) * parseInt(serie[k]);
//            k++;
//        }

//        resto = sumatoria % 11;
//        dv = 11 - resto;

//        if (dv == 10) {
//            dv = "K";
//        }
//        else if (dv == 11) {
//            dv = 0;
//        }

//        $("#run").val(rut);
//        $("#dv").val(dv);

//        if (dv.toString().trim().toUpperCase() == dig.toString().trim().toUpperCase())
//            return true;
//        else

//            $('#idRutValidator').val("");
//        $("#run").val("");
//        $("#dv").val("");
//        return false;

//    }
//    else {
//        $('#idRutValidator').val("");
//        $("#run").val("");
//        $("#dv").val("");
//        return false;
//    }
//}


function revisarDigito(dvr) {
    dv = dvr + ""
    if (dv != '0' && dv != '1' && dv != '2' && dv != '3' && dv != '4' && dv != '5' && dv != '6' && dv != '7' && dv != '8' && dv != '9' && dv != 'k' && dv != 'K') {
        return false;
    }
    return true;
}

function revisarDigito2(crut) {
    largo = crut.length;
    if (largo < 2) {
        return false;
    }
    if (largo > 2) {
        rut = crut.substring(0, largo - 1);
    } else {
        rut = crut.charAt(0);
    }
    dv = crut.charAt(largo - 1);
    revisarDigito(dv);

    if (rut == null || dv == null) {
        return 0;
    }

    var dvr = '0';
    suma = 0;
    mul = 2;

    for (i = rut.length - 1; i >= 0; i--) {
        suma = suma + rut.charAt(i) * mul;
        if (mul == 7) {
            mul = 2;
        } else {
            mul++;
        }
    }
    res = suma % 11;
    if (res == 1) {
        dvr = 'k';
    } else if (res == 0) {
        dvr = '0';
    } else {
        dvi = 11 - res;
        dvr = dvi + "";
    }
    if (dvr != dv.toLowerCase()) {
        return false
    }

    return true
}

/*
 * Validar el rut
 */
function rutValido(rut) {
    var tmpstr = "";
    for (i = 0; i < rut.length; i++) {
        if (rut.charAt(i) != ' ' && rut.charAt(i) != '.' && rut.charAt(i) != '-') {
            tmpstr = tmpstr + rut.charAt(i);
        }
    }
    rut = tmpstr;
    largo = rut.length;

    if (largo < 2) {
        return false;
    }

    for (i = 0; i < largo; i++) {
        if (rut.charAt(i) != "0" && rut.charAt(i) != "1" && rut.charAt(i) != "2" && rut.charAt(i) != "3" && rut.charAt(i) != "4" && rut.charAt(i) != "5" && rut.charAt(i) != "6" && rut.charAt(i) != "7" && rut.charAt(i) != "8" && rut.charAt(i) != "9" && rut.charAt(i) != "k" && rut.charAt(i) != "K") {
            return false;
        }
    }

    var invertido = "";
    for (i = (largo - 1), j = 0; i >= 0; i-- , j++) {
        invertido = invertido + rut.charAt(i);
    }
    var dRut = "";
    dRut = dRut + invertido.charAt(0);
    dRut = dRut + '-';
    cnt = 0;

    for (i = 1, j = 2; i < largo; i++ , j++) {
        if (cnt == 3) {
            dRut = dRut + '.';
            j++;
            dRut = dRut + invertido.charAt(i);
            cnt = 1;
        } else {
            dRut = dRut + invertido.charAt(i);
            cnt++;
        }
    }

    invertido = "";
    for (i = (dRut.length - 1), j = 0; i >= 0; i-- , j++) {
        invertido = invertido + dRut.charAt(i);
    }
    console.log(rut);
    if (revisarSoloCeros(rut)) {
        return false;
    }

    if (revisarDigito2(rut)) {
        return true;
    }

}

function revisarSoloCeros(rut) {
    cont = 0;
    for (i = 0; i < rut.length; i++) {
        if (rut.charAt(i) === '0') {
            cont++;
        }
    }
    console.log(cont);
    if (cont === rut.length) {
        return true;
    }
    return false;
}

/*
 * Formato el rut
 */
function rutFormato(rut) {

    if (rut === '') {
        return '';
    }

    var tmpstr = "";
    for (i = 0; i < rut.length; i++) {
        if (rut.charAt(i) != ' ' && rut.charAt(i) != '.' && rut.charAt(i) != '-') {
            tmpstr = tmpstr + rut.charAt(i);
        }
    }
    rut = tmpstr;
    largo = rut.length;

    if (largo < 2) {
        return false;
    }

    for (i = 0; i < largo; i++) {
        if (rut.charAt(i) != "0" && rut.charAt(i) != "1" && rut.charAt(i) != "2" && rut.charAt(i) != "3" && rut.charAt(i) != "4" && rut.charAt(i) != "5" && rut.charAt(i) != "6" && rut.charAt(i) != "7" && rut.charAt(i) != "8" && rut.charAt(i) != "9" && rut.charAt(i) != "k" && rut.charAt(i) != "K") {
            return false;
        }
    }

    var invertido = "";
    for (i = (largo - 1), j = 0; i >= 0; i-- , j++) {
        invertido = invertido + rut.charAt(i);
    }
    var dRut = "";
    dRut = dRut + invertido.charAt(0);
    dRut = dRut + '-';
    cnt = 0;

    for (i = 1, j = 2; i < largo; i++ , j++) {
        if (cnt == 3) {
            dRut = dRut + '.';
            j++;
            dRut = dRut + invertido.charAt(i);
            cnt = 1;
        } else {
            dRut = dRut + invertido.charAt(i);
            cnt++;
        }
    }

    invertido = "";
    for (i = (dRut.length - 1), j = 0; i >= 0; i-- , j++) {
        invertido = invertido + dRut.charAt(i);
    }
    
    return invertido.toLowerCase();
}

$(".nombre-propio").focusout(function () {
    $(this).val(capitalizeEachFirstLetter($(this).val()));
});

function capitalizeEachFirstLetter(string) {
    string = string.split(" ");
    result = "";
    string.forEach(function (e) {
        result += ' ' + capitalizeFirstLetter(e);
    });
    return result.slice(1);
}

function capitalizeFirstLetter(string) {
    return string.charAt(0).toUpperCase() + string.slice(1).toLowerCase();
}





