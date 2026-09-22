<%@ Page Title="Catálogo de Promotores" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_promotores.aspx.vb" Inherits="PLD.catalogo_promotores" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
    <h4>Catálogo de Promotores</h4>

    <div class="card border-primary">
        <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
            <span><i class="fa fa-users"></i> Lista de promotores</span>
            <button type="button" class="btn btn-success btn-sm" id="btnNuevoPromotor">
                <i class="fa fa-plus"></i> Capturar
            </button>
        </div>
        <div class="card-body">
            <div class="table-responsive">
                <table id="tablaPromotores" class="table table-sm table-bordered w-100">
                    <thead class="table-light">
                        <tr>
                            <th>ID</th>
                            <th>Nombre completo</th>
                            <th>Correo</th>
                            <th>Sucursal</th>
                            <th>Tipo crédito</th>
                            <th>Impacto</th>
                            <th>Probabilidad</th>
                            <th>Nivel riesgo PLD</th>
                            <th>Estatus</th>
                            <th>Acciones</th>
                        </tr>
                    </thead>
                    <tbody></tbody>
                </table>
            </div>
        </div>
    </div>
</div>

<!-- Modal -->
<div class="modal fade" id="modalPromotor" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header bg-primary text-white">
                <h5 class="modal-title">Capturar/Editar Promotor</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <input type="hidden" id="idPromotor">
                <div class="row g-3">
                    <div class="col-md-3">
                        <label>Primer nombre*</label>
                        <input type="text" class="form-control" id="primer_nombre">
                    </div>
                    <div class="col-md-3">
                        <label>Segundo nombre</label>
                        <input type="text" class="form-control" id="segundo_nombre">
                    </div>
                    <div class="col-md-3">
                        <label>Apellido paterno*</label>
                        <input type="text" class="form-control" id="apellido_paterno">
                    </div>
                    <div class="col-md-3">
                        <label>Apellido materno*</label>
                        <input type="text" class="form-control" id="apellido_materno">
                    </div>
                    <div class="col-md-4">
                        <label>Sucursal*</label>
                        <select class="form-select" id="sucursal"></select>
                    </div>
                    <div class="col-md-4">
                        <label>Tipo de crédito*</label>
                        <select class="form-select" id="tipo_credito"></select>
                    </div>
                    <div class="col-md-4">
                        <label>Correo electrónico*</label>
                        <input type="email" class="form-control" id="correo_electronico">
                    </div>
                    <div class="col-md-3">
                        <label>Impacto*</label>
                        <input type="text" class="form-control" id="impacto">
                    </div>
                    <div class="col-md-3">
                        <label>Probabilidad*</label>
                        <input type="text" class="form-control" id="probabilidad">
                    </div>
                    <div class="col-md-3">
                        <label>Nivel riesgo PLD</label>
                        <input type="text" class="form-control" id="nivel_riesgo_pld">
                    </div>
                    <div class="col-md-3">
                        <label>Estatus</label>
                        <select class="form-select" id="activo">
                            <option value="1">Activo</option>
                            <option value="0">Inactivo</option>
                        </select>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" class="btn btn-success" id="btnGuardarPromotor">Guardar</button>
            </div>
        </div>
    </div>
</div>

<script>
document.addEventListener("DOMContentLoaded", function () {

    // Cargar dropdown tipo crédito
    fetch('/handlers/handler_tipo_credito.ashx')
        .then(r => r.json())
        .then(data => {
            const $select = document.getElementById("tipo_credito");
            $select.innerHTML = '<option value="">Seleccionar...</option>';
            data.forEach(item => {
                const option = document.createElement("option");
                option.value = item.descripcion;
                option.textContent = item.descripcion;
                $select.appendChild(option);
            });
        });

    // Cargar dropdown sucursales
    fetch('/handlers/handler_sucursales.ashx')
        .then(r => r.json())
        .then(data => {
            const $select = document.getElementById("sucursal");
            $select.innerHTML = '<option value="">Seleccionar...</option>';
            data.forEach(item => {
                const option = document.createElement("option");
                option.value = item.nombre;
                option.textContent = item.nombre;
                $select.appendChild(option);
            });
        });

    let tabla = $('#tablaPromotores').DataTable({
        ajax: {
            url: '/handlers/handler_promotores.ashx',
            type: 'POST',
            data: { action: 'consultar' },
            dataSrc: ''
        },
        columns: [
            { data: 'id' },
            {
                data: null,
                render: data => `${data.primer_nombre} ${data.segundo_nombre ?? ''} ${data.apellido_paterno} ${data.apellido_materno}`
            },
            { data: 'correo_electronico' },
            { data: 'sucursal' },
            { data: 'tipo_credito' },
            { data: 'impacto' },
            { data: 'probabilidad' },
            { data: 'nivel_riesgo_pld' },
            {
                data: 'activo',
                render: val => val ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>'
            },
            {
                data: null,
                render: data => `
                    <button type="button" class="btn btn-warning btn-sm btnEditar" data-id="${data.id}"><i class="fa fa-edit"></i></button>
                    <button type="button" class="btn btn-danger btn-sm btnEstatus" data-id="${data.id}" data-estado="${data.activo}"><i class="fa fa-toggle-${data.activo ? 'off' : 'on'}"></i></button>
                `
            }
        ]
    });

    document.getElementById("btnNuevoPromotor").addEventListener("click", function () {
        limpiarModal();
        $('#modalPromotor').modal('show');
    });

    document.getElementById("btnGuardarPromotor").addEventListener("click", function () {
        const id = document.getElementById("idPromotor").value;

        if (!validarCampos()) return;

        const datos = {
            primer_nombre: $("#primer_nombre").val(),
            segundo_nombre: $("#segundo_nombre").val(),
            apellido_paterno: $("#apellido_paterno").val(),
            apellido_materno: $("#apellido_materno").val(),
            sucursal: $("#sucursal").val(),
            tipo_credito: $("#tipo_credito").val(),
            correo_electronico: $("#correo_electronico").val(),
            impacto: $("#impacto").val(),
            probabilidad: $("#probabilidad").val(),
            nivel_riesgo_pld: $("#nivel_riesgo_pld").val(),
            action: id ? "editar" : "insertar"
        };

        if (id) datos.id = id;

        fetch('/handlers/handler_promotores.ashx', {
            method: 'POST',
            body: new URLSearchParams(datos)
        })
        .then(r => r.json())
        .then(() => {
            Swal.fire('Éxito', 'Promotor guardado correctamente', 'success');
            $('#modalPromotor').modal('hide');
            tabla.ajax.reload();
        });
    });

    $('#tablaPromotores tbody').on('click', '.btnEditar', function () {
        const data = tabla.row($(this).parents('tr')).data();
        $("#idPromotor").val(data.id);
        $("#primer_nombre").val(data.primer_nombre);
        $("#segundo_nombre").val(data.segundo_nombre);
        $("#apellido_paterno").val(data.apellido_paterno);
        $("#apellido_materno").val(data.apellido_materno);
        $("#sucursal").val(data.sucursal);
        $("#tipo_credito").val(data.tipo_credito);
        $("#correo_electronico").val(data.correo_electronico);
        $("#impacto").val(data.impacto);
        $("#probabilidad").val(data.probabilidad);
        $("#nivel_riesgo_pld").val(data.nivel_riesgo_pld);
        $("#activo").val(data.activo ? "1" : "0");
        $('#modalPromotor').modal('show');
    });

    $('#tablaPromotores tbody').on('click', '.btnEstatus', function () {
        const id = $(this).data("id");
        const estado = $(this).data("estado");

        fetch('/handlers/handler_promotores.ashx', {
            method: 'POST',
            body: new URLSearchParams({
                action: 'cambiar_estatus',
                id: id,
                activo: estado ? 0 : 1
            })
        })
        .then(r => r.json())
        .then(() => {
            Swal.fire('Actualizado', 'Estatus cambiado correctamente', 'success');
            tabla.ajax.reload();
        });
    });

    function limpiarModal() {
        document.querySelectorAll("#modalPromotor input").forEach(el => el.value = '');
        document.getElementById("activo").value = "1";
        document.getElementById("idPromotor").value = '';
    }

    function validarCampos() {
        const obligatorios = [
            "primer_nombre", "apellido_paterno", "apellido_materno",
            "sucursal", "tipo_credito", "correo_electronico",
            "impacto", "probabilidad"
        ];
        for (let id of obligatorios) {
            if (!document.getElementById(id).value) {
                Swal.fire("Campo obligatorio", `Debes llenar: ${id.replaceAll("_", " ")}`, "warning");
                return false;
            }
        }
        return true;
    }
});
</script>
</asp:Content>
