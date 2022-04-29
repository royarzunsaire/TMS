$(document).ready(function () {
    ajax_call();
    setInterval(ajax_call, 1000 * 60 * 1);
});
var ajax_call = function () {
    $.ajax({
        url: "/Notificacion/Notificaciones/",
        type: "POST",
        success: function (response) {
            // ---- obtener notificaciones ----
            notificaciones = response.notificaciones;
            notificaciones = notificaciones.replace(/&quot;/g, '"');
            notificaciones = JSON.parse(notificaciones);
            // ---- escribir notificaciones ----
            $("#notificaciones").empty();
            cantNotificacionesEnviadas = 0;
            cantNotificacionesPendientes = 0;
            notificaciones.forEach(function (item) {
                if (item.nombre === 1) {
                    cantNotificacionesEnviadas++;
                }
                if (item.nombre === 2) {
                    cantNotificacionesPendientes++;
                }
                color = 0;
                if (item.color === 0) {
                    color = '';
                }
                if (item.color === 1) {
                    color = 'border-bottom-style: solid; border-color: blue';
                }
                if (item.color === 2) {
                    color = 'border-bottom-style: solid; border-color: red';
                }
                if (item.color === 3) {
                    color = 'border-bottom-style: solid; border-color: orange';
                }
                if (item.color === 4) {
                    color = 'border-bottom-style: solid; border-color: green';
                }
                hoy = Date.parse(JSON.parse($("#hoy").val()));
                //console.log(item.fechaCreacion);
                //console.log(JSON.parse($("#hoy").val()));
                //console.log(hoy);
                //console.log(Date.parse(item.fechaCreacion));
                //console.log(moment(Date.parse(item.fechaCreacion)).startOf('minute').fromNow());
                //console.log(moment(Date.parse(item.fechaCreacion)).startOf('minute').from(hoy));
                $("#notificaciones")
                    .append($('<li>')
                        .append($('<a>')
                            .attr("href", 'javascript:;')
                            .attr("onclick", "marcarComoLeido(" + item.idNotificacion + ")")
                            .append($('<div>')
                                .attr("class", "row")
                                .append($('<div>')
                                    .attr("class", "col-sm-8")
                                    .append($('<div>')
                                        .attr("class", "notificacion-titulo")
                                        .html(item.titulo)
                                    )
                                )
                                .append($('<div>')
                                    .attr("class", "col-sm-4")
                                    .append($('<div>')
                                        .attr("class", "notificacion-fecha")
                                        .html(moment(Date.parse(item.fechaCreacion)).startOf('minute').from(hoy))
                                    )
                                )
                            )
                            .append($('<div>')
                                .attr("class", "row")
                                .append($('<div>')
                                    .attr("class", "col-sm-12")
                                    .append($('<div>')
                                    .attr("style", color)
                                        .attr("class", "notificacion-detalles")
                                        .html(item.mensaje)
                                    )
                                )
                            )
                        )
                    );
                if (cantNotificacionesEnviadas === 0) {
                    $("#cantNotificacionesEnviadas").html('');
                } else {
                    $("#cantNotificacionesEnviadas").html(cantNotificacionesEnviadas);
                }
                $("#cantNotificacionesPendientes").html(cantNotificacionesEnviadas + cantNotificacionesPendientes);
            });
        },
        fail: function (xhr, textStatus, errorThrown) {
            console.log("Error!");
        }
    });
    $("#formVistas").submit(function () {
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                $("#cantNotificacionesEnviadas").html('');
                console.log("Vistas!");
            },
            fail: function (xhr, textStatus, errorThrown) {
                console.log("Error!");
            }
        });
        return false;
    });
    //$("#formLeido").submit(function () {
    //    $.ajax({
    //        url: this.action,
    //        type: this.method,
    //        data: $(this).serialize(),
    //        success: function (response) {
    //            console.log("Leido!");
    //        },
    //        fail: function (xhr, textStatus, errorThrown) {
    //            console.log("Error!");
    //        }
    //    });
    //    return false;
    //});
    $('#marcarComoVistas').click(function () {
        if ($("#cantNotificacionesEnviadas").html() !== '') {
            $("#formVistas").submit();
        }
    });
};
function marcarComoLeido(id) {
    $('#idNotificacionLeida').val(id);
    $("#formLeido").submit();
}