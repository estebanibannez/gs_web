var idioma = {
    "sProcessing": "Procesando...",
    "sLengthMenu": "Mostrar _MENU_ registros",
    "sZeroRecords": "No se encontraron resultados",
    "sEmptyTable": "Ningún registro disponible en esta tabla",
    "sInfo": "_START_ al _END_ de _TOTAL_ registros",
    "sInfoEmpty": "sin registros a mostrar",
    "sInfoFiltered": "(de _MAX_ registros)",
    "sInfoPostFix": "",
    "sSearch": '<span class="input-group-addon"><i class="glyphicon glyphicon-search"></i></span>',
    "sUrl": "",
    "sInfoThousands": ".",
    "sLoadingRecords": "Cargando...",
    "oPaginate": {
        "sFirst": "Primero",
        "sLast": "Último",
        "sNext": "Siguiente",
        "sPrevious": "Anterior"
    },
    "oAria": {
        "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
        "sSortDescending": ": Activar para ordenar la columna de manera descendente"
    }

};
Number.prototype.padLeft = function (base, chr) {
    var len = (String(base || 10).length - String(this).length) + 1;
    return len > 0 ? new Array(len).join(chr || '0') + this : this;
}

function InicializaGrillaFiltroAjax(grilla, url, parametros, dataColumns, createdRow) {

    
    var otable;
    callBack = typeof callBack !== 'undefined' ? callBack : function (ajax) { return ajax.data };
    function pagefunction() {
        /* BASIC ;*/
        var responsiveHelper_datatable_fixed_column = undefined;
        var responsiveHelper_datatable_col_reorder = undefined;
        var breakpointDefinition = {
            tablet: 1024,
            phone: 480
        };
       
            otable = $('#' + grilla).DataTable({
            "sDom": "<'dt-toolbar'<'col-xs-12 col-sm-6'lf><'table-responsive hidden-xs'C>>" +
        "t" +
        "<'dt-toolbar-footer'<'col-sm-6 col-xs-12 hidden-xs'i><'col-sm-6 col-xs-12'p>>",
            "autoWidth": true,
            "ajax": {
                "url": url,
                "type": "POST",
                "data": parametros,
                "dataSrc": callBack,
            },
            "createdRow": createdRow,
            rowReorder: {
                selector: 'td:nth-child(2)'
            },
            responsive: true,
                
            columns: dataColumns,
            oLanguage: idioma,

        
            "dom": 'C<"clear">lfrtip',

              
            //combo en el final de la tabla para filtrar
            initComplete: function () {
                //this.api().columns().every(function () {
                this.api().columns('.visible').every(function () {
                    var column = this;
                    var select = $('<select class="btn btn-primary dropdown-toggle" data-toggle="dropdown" ><option value="" >Todos <li><span class="glyphicon glyphicon-ok"> </span></li></option></select>')
                        .appendTo($(column.footer()).empty())
                        .on('change', function () {
                            var val = $.fn.dataTable.util.escapeRegex(
                                $(this).val()
                            );

                            column
                                .search(val ? '^' + val + '$' : '', true, false)
                                .draw();
                        });

                    column.data().unique().sort().each(function (d, j) {
                        select.append('<option value="' + d + '">' + d + '</option>')
                    });
                });
            }
            // fin combo 



        });

        // Apply the filter
        $('#' + grilla + ' thead th input[type=text]').on('keyup change', function () {

            otable
                .column($(this).parent().index() + ':visible')
                .search(this.value)
                .draw();

        });

        /* END BASIC */


    };

    // load related plugins

    pagefunction();

    return otable;
}




function InicializaMantenedor(grilla) {

    function pagefunction() {
        /* BASIC ;*/
        var responsiveHelper_datatable_col_reorder = undefined;
        var responsiveHelper_dt_basic = undefined;
        var responsiveHelper = undefined;

        var breakpointDefinition = {
            tablet: 1024,
            phone: 480
        };

        $('#' + grilla).dataTable({
                  //"sDom": "<'dt-toolbar'<'col-xs-12 col-sm-6'f><'col-sm-6 col-xs-12 hidden-xs'l>r>" +
                  //   "t" +
                  //   "<'dt-toolbar-footer'<'col-sm-6 col-xs-12 hidden-xs'i><'col-xs-12 col-sm-6'p>>",
            oLanguage: idioma,
            autoWidth: true,
            paging: true,
            searching: true,
            "destroy": true,
            "deferRender": true,
            "autoWidth": true,
            bAutoWidth       : false,
            fnPreDrawCallback: function () {
                // Initialize the responsive datatables helper once.
                //if (!responsiveHelper) {
                //    responsiveHelper = new ResponsiveDatatablesHelper($('#' + grilla), breakpointDefinition);
                //}
            },
            fnRowCallback  : function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
                //responsiveHelper.createExpandIcon(nRow);
            },
            fnDrawCallback : function (oSettings) {
                //responsiveHelper.respond();
            },

             "oTableTools": {
                "aButtons": [
                        "copy",
                            {
                                "sExtends": "csv",
                                "oSelectorOpts": { filter: 'applied', order: 'current', page: "all" },
                                "bShowAll": true,
                                "sFileName": "DescargaCSVTs.csv",
                                "bFooter": false,
                                "mColumns": "visible"
                            },
                            {
                                "sExtends": "xls",
                                "oSelectorOpts": { filter: 'applied', order: 'current', page: "all" },
                                "bShowAll": true,
                                "sFileName": "DescargaExcelTs.xls",
                                "bFooter": false,
                                "mColumns": "visible"
                            },
                            {
                                "sExtends": "pdf",
                                "oSelectorOpts": { filter: 'applied', order: 'current', page: "all" },
                                "bShowAll": true,
                                "sTitle": "TS_PDF",
                                "sPdfMessage": "TS PDF Export",
                                "sPdfSize": "letter",
                                "sPdfOrientation": "landscape",// 
                                "bFooter": false, // se quita el footer del pdf.
                                "mColumns": "visible"
                            },
                             {
                                 "sExtends": "print",
                                 "oSelectorOpts": { filter: 'applied', order: 'current', page: "all" },
                                 "sMessage": "Generado por Timesheets <i>(Presione ESC para salir)</i>",
                                 "bShowAll": false,  // solo muestra lo que contiene la pagina, no todos los datos
                                 "mColumns": "visible"


                             }
                ],
                "sSwfPath": "/js/plugin/datatables/swf/copy_csv_xls_pdf.swf"

            },
            
            'aoColumnDefs': [{
                'bSortable': false,
                'aTargets': ['nosort']
            }],
        });

        /* END BASIC */


    };

    // load related plugins

    pagefunction();
}




$(document).on('ready', function () {
    $(document).on('click', 'a.main-content', function (e) {
        e.preventDefault();
        var url = $(this).attr("href");
        loadAjaxContent(url);
    });

    $("#myModal").on("show.bs.modal", function (e) {
        var link = $(e.relatedTarget);

        if (link.hasClass('modal-ajax-load')) {
            if (link.hasClass('pdf-size')) {
                $(this).find('.modal-dialog').css({
                    position: 'relative',
                    display: 'table',
                    'overflow-y': 'auto',
                    'overflow-x': 'auto',
                    width: 'auto',
                    'min-width': 'calc(100% - 100px)'
                });
            } else {
                $(this).find('.modal-dialog').removeAttr('style');
            }
            var modal_content = $(this).find(".modal-content");
            $.ajax({
                async: true,
                beforeSend: function () {
                    modal_content.html('<h4><i class="fa fa-refresh fa-spin"></i> Cargando Solicitud...</h4>');
                },
                success: function (result) {
                    modal_content.html(result);
                },
                error: function () {
                    modal_content.html('<h4>Se ha generado un error</h4>');
                },
                url: link.attr("href")
            });
            
        }
    });

    $("#myModal3").on("show.bs.modal", function (e) {
        //debugger;
        var link = $(e.relatedTarget);

        if (link.hasClass('modal-ajax-load')) {
            if (link.hasClass('pdf-size')) {
                $(this).find('.modal-dialog').css({
                    position: 'relative',
                    display: 'table',
                    'overflow-y': 'auto',
                    'overflow-x': 'auto',
                    width: 'auto',
                    'min-width': 'calc(100% - 100px)'
                });
            } else {
                $(this).find('.modal-dialog').removeAttr('style');
            }
            var modal_content = $(this).find(".modal-content");
            $.ajax({
                async: true,
                beforeSend: function () {
                    modal_content.html('<h4><i class="fa fa-refresh fa-spin"></i> Cargando Solicitud...</h4>');
                },
                success: function (result) {
                    //debugger;
                    modal_content.html(result);
                },
                error: function () {
                    modal_content.html('<h4>Se ha generado un error</h4>');
                },
                url: link.attr("href")
            });

        }
    });



    var cliente_login_previous;
    $("#cliente_login").on('focus', function () {
        cliente_login_previous = this.value;
    }).change(function () {
        $.ajax({
            data: { "cliente": $(this).val() },
            success: function (result) {
                if (result.success) {
                    mensajeExito("Formulario", "Cliente Cambiado con exito");
                    location.reload(true);
                }else if (result.error) {
                    $(this).val(cliente_login_previous)
                    mensajeError("Formulario", result.mensaje);
                }
            },
            error: function () {
                mensajeError("Formulario", "");
                $(this).val(cliente_login_previous)
            },
            url: $("#base_url").val()+'/home/ChangeCliente'
        });
    });

});


function loadAjaxContent(url) {
    $.ajax({
        async: true,
        beforeSend: function () {
            $("#content").html('<h4><i class="fa fa-spinner fa-pulse"></i> Cargando Solicitud...</h4>');
        },
        success: function (result) {
            $("#content").html(result);
            window.history.pushState("", "PayRoll-Portal", url);
        },
        error: function () {
            $("#content").html('<h2>Se ha generado un error</h2>');
        },
        url: url
    });
}

function setFormulario(form, rules, messages, contenedor) {
    if (contenedor === undefined || contenedor === null) {
        contenedor = "#remoteModal .modal-content";
    }

    var $orderForm = $("#" + form).validate({
        rules: rules,
        messages: messages,
        // Do not change code below
        errorPlacement: function (error, element) {
            error.insertAfter(element.parent());
        }
    });

    $("button[action=save]").click(function () {
        $("button[action=save]").prop('disabled', true);
        if ($('#' + form).valid()) {
            var formJS = document.getElementById(form);
            var formData = new FormData(formJS);

            for(var pair of formData.entries()) {
                
                if (pair[0].includes(".monto")) {
                    formData.set(pair[0], pair[1].replace(new RegExp('\\.', 'g'), ''));
                }
            }

            $.ajax({
                url: document.getElementById(form).action,
                type: document.getElementById(form).method,
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    if (result.success) {
                        switch (result.mensaje) {
                            case "Enviado":
                                CorreoExitoso("Correo", "Enviado con éxito!");
                                break;
                            case "Evento":
                                mensajeExito("Evento", "Creado con éxito!");
                                break;
                            case "Actualizado":
                                mensajeExito("Actualizado", "Datos actualizados con éxito!");
                                break;
                            case "Eliminado":
                                mensajeExito("Registro", "Eliminado con éxito!");
                                break;
                            case "":
                                mensajeExito("Formulario", "Registro con exito");
                                break;
                            case "Comprobante":
                                mensajeExito("Comprobante éxitoso, N° ", result.numero);
                                break;
                            case "SinCambios":
                                mensajeExito("Sin cambios","No hubo cambios en el documento");
                                break;
                            case "Asignacion":
                                mensajeExito("Assignment", "Assignment Made Successfully");
                                break;
                            case "Eliminacion":
                                mensajeExito("Elimination", "Record Deleted Successfully");
                                break;
                            case "Creado":
                                mensajeExito("Contact", "Contact Successfully Created");
                                break;
                            case "Editado":
                                mensajeExito("Edited", "Successfully Edited Contact");
                                break;
                            case "ing_Generico":
                                mensajeExito("Form", "Successful Registration");
                                break;
                            default:
                                mensajeExito("Formulario", "Registro con exito");
                                //mensajeExito("Comprobante ingresado con numero ", + result.mensaje);
                        }
                        loadAjaxContent(window.location.pathname);
                    }
                    else {
                        if (result.error) {
                            switch (result.mensaje) {
                                case "No enviado":
                                    CorreoError("Correo", "No fue posible enviar correo...")
                                    break;
                                case "Evento Fallido":
                                    mensajeError("Evento", "No fue posible crear Evento...")
                                    break;
                                case "":
                                    mensajeError("Error", "Error al registrar...")
                                    break;
                                default:
                                    mensajeError("Error", result.mensaje)
                            }
                        } else {
                            //console.log("form", "retorno html", contenedor, result);
                            $(contenedor).html(result);
                        }
                    }
                },complete: function () {
                    $("button[action=save]").prop('disabled', false);
                }
            });
        } else {
            console.log("error formulario");
            $("button[action=save]").prop('disabled', false);
        }
    });
}

function CierraToastExito() {
    var toast = document.querySelector('.iziToast');
    iziToast.hide({
        transitionOut: 'fadeOutUp'
    }, toast);
}

function mensajeExitoComprobante(titulo, mensaje, ncomprobante) {
    $('#myModal').modal('hide');
    iziToast.info({
        timeout: 20000,
        overlay: true,
        displayMode: 'once',
        id: 'inputs',
        title: titulo,
        message: mensaje,
        position: 'center',
        drag: false,
        transitionIn: 'fadeInUp',
        inputs: [
            ['<input type="checkbox">', 'change', function (instance, toast, input, e) {
                console.info(input.checked);
            }],
            ['<input type="text">', 'keyup', function (instance, toast, input, e) {
                console.info(input.value);
            }, true],
            ['<input type="number">', 'keydown', function (instance, toast, input, e) {
                console.info(input.value);
            }],
        ]
    });
}
function mensajeExito(titulo, mensaje) {
    $('#myModal').modal('hide');
    iziToast.success({
        timeout: 3000,
        title: titulo,
        message: "<i class='fa fa-clock-o'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'bounceInLeft'
    });
}

function mensajeExitoModal(titulo, mensaje, req) {
    switch (req) {
        case "1":
            iziToast.success({
                timeout: 3000,
                title: titulo,
                message: "<i class='fa fa-plus-circle'></i> <i>" + mensaje + "...</i>",
                position: 'topRight',
                transitionIn: 'bounceInLeft'
            });
            break;
        case "2":
            iziToast.info({
                timeout: 3000,
                title: titulo,
                message: "<i class='glyphicon glyphicon-facetime-video'></i> <i>" + mensaje + "...</i>",
                position: 'center',
                transitionIn: 'flipInX',
                transitionOut: 'flipOutX'
            });
            break;
        case "3":
            iziToast.warning({
                timeout: 8000,
                title: titulo,
                message: "<i>" + mensaje + "<br/></i>",
                position: 'center',
                transitionIn: 'flipInX',
                transitionOut: 'flipOutX'
            });
            break;
        case "4":
            iziToast.info({
                timeout: 8000,
                title: titulo,
                message: "<i class='fa fa-plus-circle'></i> <i>" + mensaje + "</i>",
                position: 'topRight',
                transitionIn: 'bounceInLeft'
            });
            break;
        case "5":
            iziToast.warning({
                timeout: 8000,
                title: titulo,
                message: "<i class='fa fa-minus-circle'></i> <i>" + mensaje + "</i>",
                position: 'topRight',
                transitionIn: 'bounceInLeft'
            });
            break;
        case "6":
            iziToast.show({
                timeout: false,
                color: 'dark',
                icon: 'fa fa-file-text',
                title: titulo,
                close: false,
                message: 'Folio N° : '+ mensaje ,
                position: 'center', // bottomRight, bottomLeft, topRight, topLeft, topCenter, bottomCenter
                progressBarColor: 'rgb(0, 255, 184)',
                buttons: [
                    ['<button>Close</button>', function (instance, toast) {
                        iziToast.hide({
                            transitionOut: 'flipOutX'
                        }, toast);
                        setTimeout((e) => {
                            loadAjaxContent(window.location.pathname);
                        }, 1000)
                       
                    }]
                ]
            });
            break;
        default:
            iziToast.warning({
                timeout: 3000,
                title: titulo,
                message: "<i class='fa fa-minus-circle'></i> <i>" + mensaje + "...</i>",
                position: 'topRight',
                transitionIn: 'bounceInLeft'
            });
    }

}

function CorreoExitoso(titulo, mensaje) {
    $('#myModal').modal('hide');
    iziToast.info({
        timeout: 3000,
        title: titulo,
        message: "<i class='fa fa-envelope'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'bounceInLeft'
    });
}

function CorreoError(titulo, mensaje) {
    $('#myModal').modal('hide');
    iziToast.warning({
        timeout: 3000,
        title: titulo,
        message: "<i class='fa fa-envelope'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'bounceInLeft'
    });
}
function mensajeExito(titulo, mensaje) {
    $('#myModal3').modal('hide');
    iziToast.success({
        timeout: 3000,
        title: titulo,
        message: "<i class='fa fa-clock-o'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'bounceInLeft'
    });
}
function CorreoExitoso(titulo, mensaje) {
    $('#myModal3').modal('hide');
    iziToast.info({
        timeout: 3000,
        title: titulo,
        message: "<i class='fa fa-envelope'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'bounceInLeft'
    });
}

function CorreoError(titulo, mensaje) {
    $('#myModal3').modal('hide');
    iziToast.warning({
        timeout: 3000,
        title: titulo,
        message: "<i class='fa fa-envelope'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'bounceInLeft'
    });
}

function mensajeError(titulo, mensaje) {
    iziToast.error({
        title: titulo,
        message: "<i class='fa fa-clock-o'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'fadeInDown'
    });
}
//funcion para modal de confirmacion de eliminacion
function confirmDelete(mensaje, url) {
    var msg = mensaje;
    if (msg == null || msg == "") {
        msg = "¿Desea eliminar el registro?";
    }
    $('#Modal_Background').modal({ backdrop: 'static', keyboard: false }).modal('show');
    iziToast.show({
        color: 'dark',
        icon: 'icon-person',
        title: 'Eliminación',
        transitionIn: 'flipInX',
        message: msg,
        position: 'center', // bottomRight, bottomLeft, topRight, topLeft, topCenter, bottomCenter
        progressBarColor: 'rgb(0, 255, 184)',
        buttons: [
            ['<button>Ok</button>', function (instance, toast) {
                $.ajax({
                    url: url,
                    error: function () { },
                    success: function (result) {
                        if (result.success) {
                            $('#Modal_Background').modal('hide');
                            mensajeExito("Formulario", "Registro procesado con exito");
                            iziToast.hide({
                                transitionOut: 'flipOutX', 
                            }, toast);
                        } else {
                            if (result.error) {
                                $('#Modal_Background').modal('hide');
                                mensajeError("Formulario", "Registro con errores<br>" + result.mensaje);
                                iziToast.hide({
                                    transitionOut: 'flipOutX'
                                }, toast);    
                            } 
                        }
                    },
                    complete: function () {
                       loadAjaxContent(window.location.pathname);                     
                    }
                });
            }],
            ['<button>Close</button>', function (instance, toast) {
                iziToast.hide({
                    transitionOut: 'flipOutX'
                }, toast); 
                $('#Modal_Background').modal('hide');
            }]           
        ],
        onClose: function (instance, toast) { //esconde el modal si la barra de progreso del alert termina
            $('#Modal_Background').modal('hide');                  
        }
    });
}

function confirmDeletePost(mensaje, url, id) {
    
    var msg = mensaje;
    if (msg == null || msg == "") {
        msg = "¿Desea eliminar el registro?";
    }
    $('#Modal_Background').modal({ backdrop: 'static', keyboard: false }).modal('show');
    iziToast.show({
        color: 'dark',
        icon: 'icon-person',
        title: 'Eliminación',
        transitionIn: 'flipInX',
        message: msg,
        position: 'center', // bottomRight, bottomLeft, topRight, topLeft, topCenter, bottomCenter
        progressBarColor: 'rgb(0, 255, 184)',
        buttons: [
            ['<button>Ok</button>', function (instance, toast) {
                $.ajax({
                    url: url,
                    data: { id: id },
                    type: 'POST',
                    error: function () { },
                    success: function (result) {
                        if (result.success) {
                            $('#Modal_Background').modal('hide');
                            mensajeExito("Formulario", "Registro procesado con exito");
                            iziToast.hide({
                                transitionOut: 'flipOutX',
                            }, toast);
                        } else {
                            if (result.error) {
                                $('#Modal_Background').modal('hide');
                                mensajeError("Formulario", "Registro con errores<br>" + result.mensaje);
                                iziToast.hide({
                                    transitionOut: 'flipOutX'
                                }, toast);
                            }
                        }
                    },
                    complete: function () {
                        loadAjaxContent(window.location.pathname);
                    }
                });
            }],
            ['<button>Close</button>', function (instance, toast) {
                iziToast.hide({
                    transitionOut: 'flipOutX'
                }, toast);
                $('#Modal_Background').modal('hide');
            }]
        ],
        onClose: function (instance, toast) { //esconde el modal si la barra de progreso del alert termina
            $('#Modal_Background').modal('hide');
        }
    });
}


$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
    var success = options.success;
    options.success = function (data, textStatus, jqXHR) {
        // override success handling
        if (typeof (success) === "function") {
            if (jqXHR.status == 200) {
                return success(data, textStatus, jqXHR);
            } else if (jqXHR.status == 295) {
                document.open("text/html", "replace");
                document.write(data);
                document.close();
                return null;
            } else if (jqXHR.status >= 400 && jqXHR.status <= 499) {
                document.open("text/html", "replace");
                document.write(data);
                document.close();
                return null;
            } else if (jqXHR.status >= 500 && jqXHR.status <= 599) {
                document.open("text/html", "replace");
                document.write(data);
                document.close();
                return null;
            }
        }
    };
    var error = options.error;
    options.error = function (jqXHR, textStatus, errorThrown) {
        // override error handling
        if (typeof (error) === "function") {
            if (jqXHR.status == 200) {
                return error(jqXHR, textStatus, errorThrown);
            } else if (jqXHR.status >= 400 && jqXHR.status <= 499) {
                document.open("text/html", "replace");
                document.write(jqXHR.responseText);
                document.close();
                return null;
            } else if (jqXHR.status >= 500 && jqXHR.status <= 599) {
                document.open("text/html", "replace");
                document.write(jqXHR.responseText);
                document.close();
                return null;
            }
        }
    };
});

function copiaTexto(identificador) {
    $(identificador).click(function () {
        $(this).select();
        var text = $.trim($(this).text());
        var textArea = document.createElement("textarea");        
        var range = document.createRange();
        range.selectNodeContents(this);
        var sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
        textArea.value = text;
        document.execCommand('copy');
        mensajeExitoPrueba("El texto", "seleccionado se a copiado");
    });
}

function Suma_montos(valor) {
    
    total = 0;
    parcial = 0;
    $(".monto").each(
        function (index, value) {
            parcial = Number(value.value.replace(/\./g, ''));
            total = parcial + total;
        });
    $(".total_monto").val(total);
    $('.total_monto').unmask();
    $('.total_monto').mask('000.000.000.000', { reverse: true });
}


function mensajeExitoPrueba(titulo, mensaje) {
    //$('#myModal').modal('hide');
    iziToast.success({
        timeout: 5000,
        title: titulo,
        message: "<i class='fa fa-clock-o'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'bounceInLeft'
    });
}

function confirmActualizar(mensaje, url) {    
    var msg = mensaje;
    if (msg == null || msg == "") {
        msg = "¿Desea reiniciar su Clave?";
    }
    $('#Modal_Background').modal({ backdrop: 'static', keyboard: false }).modal('show');
    iziToast.show({
        color: 'dark',
        icon: 'icon-person',
        title: 'Reinicio Clave',
        transitionIn: 'flipInX',
        message: msg,
        position: 'center', // bottomRight, bottomLeft, topRight, topLeft, topCenter, bottomCenter
        progressBarColor: 'rgb(0, 255, 184)',
        buttons: [
            ['<button>Ok</button>', function (instance, toast) {
                $.ajax({
                    url: url,
                    error: function () { },
                    success: function (result) {
                        if (result.success) {
                            $('#Modal_Background').modal('hide');
                            mensajeExito("Solicitud", "Solicitud procesado con exito. Clave por defecto de Softland");
                            iziToast.hide({
                                transitionOut: 'flipOutX',
                            }, toast);
                        } else {
                            if (result.error) {
                                $('#Modal_Background').modal('hide');
                                mensajeError("Solicitud", "Solicitud con errores.<br>" + result.mensaje);
                                iziToast.hide({
                                    transitionOut: 'flipOutX'
                                }, toast);
                            }
                        }
                    },
                    complete: function () {
                        loadAjaxContent(window.location.pathname);
                    }
                });
            }],
            ['<button>Close</button>', function (instance, toast) {
                iziToast.hide({
                    transitionOut: 'flipOutX'
                }, toast);
                $('#Modal_Background').modal('hide');
            }]
        ],
        onClose: function (instance, toast) { //esconde el modal si la barra de progreso del alert termina
            $('#Modal_Background').modal('hide');
        }
    });
}

function pruebaExito(titulo, mensaje) {
    $('#myModal').modal('hide');
    iziToast.success({
        timeout: 3000,
        title: titulo,
        message: "<i class='fa fa-clock-o'></i> <i>" + mensaje + "...</i>",
        position: 'topRight',
        transitionIn: 'bounceInLeft'
    });
}

function refreshAuthorizeToken() {
    $.ajax({
        type: 'POST',
        url: 'Declaraciones/GetToken',
        dataType: 'json',
        contentType: 'json',
        async: false,
        success: function (result) {
            localStorage["access_token_"] = result.access_token;
            localStorage["token_type_"] = result.token_type;
            localStorage["expires_in_"] = result.expires_in - 99;
            localStorage["date_access_token_"] = new Date().getTime() / 1000;
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            console.log(textStatus + " - " + errorThrown);
        }
    })
}

function getToken() {
    if (((parseFloat(localStorage["date_access_token_"]) + parseFloat(localStorage["expires_in_"])) < (new Date().getTime() / 1000))
        || (localStorage["expires_in_"] == undefined || localStorage["date_access_token_"] == undefined)) {
        refreshAuthorizeToken();
        return localStorage["access_token_"];
    } else {
        return localStorage["access_token_"];
    }
}

function confirmAprobacionBeta(mensaje, url) {
    var msg = mensaje;
    if (msg == null || msg == "") {
        msg = "¿Desea Iniciar el Beta?";
    }
    $('#Modal_Background').modal({ backdrop: 'static', keyboard: false }).modal('show');
    iziToast.show({
        color: 'blue',
        icon: 'icon-person',
        title: 'Aprobación',
        transitionIn: 'flipInX',
        message: msg,
        position: 'center', // bottomRight, bottomLeft, topRight, topLeft, topCenter, bottomCenter
        progressBarColor: 'white',
        buttons: [
            ['<button>Ok</button>', function (instance, toast) {
                $.ajax({
                    url: url,
                    error: function () { },
                    success: function (result) {
                        if (result.success) {
                            $('#Modal_Background').modal('hide');
                            mensajeExito("Formulario", "Registro procesado con exito");
                            iziToast.hide({
                                transitionOut: 'flipOutX',
                            }, toast);
                        } else {
                            if (result.error) {
                                $('#Modal_Background').modal('hide');
                                mensajeError("Formulario", "Registro con errores<br>" + result.mensaje);
                                iziToast.hide({
                                    transitionOut: 'flipOutX'
                                }, toast);
                            }
                        }
                    },
                    complete: function () {
                        loadAjaxContent(window.location.pathname);
                    }
                });
            }],
            ['<button>Close</button>', function (instance, toast) {
                iziToast.hide({
                    transitionOut: 'flipOutX'
                }, toast);
                $('#Modal_Background').modal('hide');
            }]
        ],
        onClose: function (instance, toast) { //esconde el modal si la barra de progreso del alert termina
            $('#Modal_Background').modal('hide');
        }
    });
}

