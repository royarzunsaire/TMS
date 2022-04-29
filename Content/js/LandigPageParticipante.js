$(document).ready(function () {
    //var tableComercializaciones = $('#tableComercializaciones').DataTable({
    //    sDom: 'lrtip',
    //    paging: false,
    //    ordering: false,
    //    info: false,
    //    searching: true,
    //    scrollY: "200px",
    //    //scrollCollapse: true,
    //    language: {
    //        "lengthMenu": "Mostrar _MENU_ registros",
    //        "zeroRecords": "No se encontraron resultados",
    //        "info": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
    //        "infoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
    //        "infoFiltered": "(filtrado de un total de _MAX_ registros)",
    //        "sSearch": "Buscar:",
    //        "oPaginate": {
    //            "sFirst": "Primero",
    //            "sLast": "Último",
    //            "sNext": "Siguiente",
    //            "sPrevious": "Anterior"
    //        },
    //        "sProcessing": "Procesando...",
    //    }
    //});

    //$('#inptSearchComercializaciones').keyup(function () {
    //    tableComercializaciones.search($(this).val()).draw();
    //})
});

function mostrarDetallesCurso(idComercializacion) {
    console.log(idComercializacion);
    $('#divDetallesCursoSeleccionado').html($('#divDetallesCurso-' + idComercializacion).html());
    if ($(window).width() < 975) {
        window.scrollTo(0, $('#divTituloDetallesCurso').offset().top);
    }
}

function volverCursos() {
    window.scrollTo(0, $('#divComercializaciones').offset().top);
    if ($(window).width() < 975) {
        window.scrollTo(0, $('#divComercializaciones').offset().top);
    }
}