$(document).ready(function () {
    var tableComercializaciones = $('#tableComercializaciones').DataTable({
        sDom: 'lrtip',
        paging: false,
        ordering: false,
        info: false,
        searching: true,
        scrollY: "200px",
        //scrollCollapse: true,
        language: {
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
        }
    });

    $('#inptSearchComercializaciones').keyup(function () {
        tableComercializaciones.search($(this).val()).draw();
    })

    var tableCotizaciones = $('#tableCotizaciones').DataTable({
        sDom: 'lrtip',
        paging: false,
        ordering: false,
        info: false,
        searching: true,
        scrollY: "200px",
        //scrollCollapse: true,
        language: {
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
        }
    });

    $('#inptSearchCotizaciones').keyup(function () {
        tableCotizaciones.search($(this).val()).draw();
    })

    var tableSalidasTerreno = $('#tableSalidasTerreno').DataTable({
        sDom: 'lrtip',
        paging: false,
        ordering: false,
        info: false,
        searching: true,
        scrollY: "200px",
        //scrollCollapse: true,
        language: {
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
        }
    });

    $('#inptSearchSalidasTerreno').keyup(function () {
        tableSalidasTerreno.search($(this).val()).draw();
    })

    var tableCursosDsiponibles = $('#tableCursosDisponibles').DataTable({
        sDom: 'lrtip',
        paging: false,
        ordering: false,
        info: false,
        searching: true,
        scrollY: "243px",
        //scrollCollapse: true,
        language: {
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
        }
    });

    $('#btnCerrarParticipantes').click(function () {
        $('#modalParticipantes').modal('hide');
    });

    $('#btnCerrarDetallesComercializacion').click(function () {
        $('#modalDetallesComercializacion').modal('hide');
    });

    $('#btnCerrarDetallesCotizacion').click(function () {
        $('#modalDetallesCotizacion').modal('hide');
    });

    //$('#btnCerrarDetallesSalidaTerreno').click(function () {
    //    $('#modalDetallesSalidaTerreno').modal('hide');
    //});

    //$('#btnCerrarDetallesCursoDisponible').click(function () {
    //    $('#modalDetallesCursoDisponible').modal('hide');
    //});
});

function mostrarParticipantes(idComercializacion) {

    var parametros = {
        "idComercializacion": idComercializacion
    };
    var form = $('#form-ModalParticipanteCliente');
    var url = form.attr('action');
    try {
        $.ajax({

            data: parametros,
            url: url,
            type: 'post',
            beforeSend: function () {
                $('#participantes').html("Procesando, espere por favor...");

            },
            success: function (response) {
                $('#participantes').html(response);

            },
            error: function (request, status, error) {
                $('#participantes').html(request.responseText);
            }
        });
    } catch (e) {
        $('#participantes').html(e);
    }



    




    //$('#participantes').html($('#participantes-' + idComercializacion).html());
    //$('#participantesHead').html($('#participantesHead-' + idComercializacion).html());
    $('#modalParticipantes').modal({ keyboard: true }, 'show');
}

function mostrarDetallesComercializacion(id) {
    $('#detallesComercializacion').html($('#detallesComercializacion-' + id).html());
    $('#modalDetallesComercializacion').modal({ keyboard: true }, 'show');
}

function mostrarDetallesCotizacion(id) {
    $('#detallesCotizacion').html($('#detallesCotizacion-' + id).html());
    $('#modalDetallesCotizacion').modal({ keyboard: true }, 'show');
}

//function mostrarDetallesSalidasTerreno(id) {
//    $('#detallesSalidaTerreno').html($('#detallesSalidaTerreno-' + id).html());
//    $('#modalDetallesSalidaTerreno').modal({ keyboard: true }, 'show');
//}

//function mostrarDetallesCursoDisponible(id) {
//    $('#detallesCursoDisponible').html($('#detallesCursoDisponible-' + id).html());
//    $('#modalDetallesCursoDisponible').modal({ keyboard: true }, 'show');
//}

function cerrarModalParticipantes() {
    $('#modalParticipantes').modal('hide');
}