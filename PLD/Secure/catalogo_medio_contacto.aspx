<%@ Page Title="Catálogo Medios de Contacto" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
    <h4>Catálogo de medios de contacto</h4>

    <div class="card border-primary">
        <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
            <span><i class="fas fa-address-book"></i> Lista de medios</span>
            <button type="button" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#modalCaptura">
                <i class="fas fa-plus"></i> Capturar
            </button>
        </div>
        <div class="card-body">
            <table id="tablaMedios" class="table table-sm table-bordered table-striped w-100">
                <thead class="table-light">
                    <tr>
                        <th>ID</th>
                        <th>Forma de contacto</th>
                        <th>Impacto</th>
                        <th>Probabilidad</th>
                        <th>Nivel de riesgo P.L.D</th>
                        <th>Estatus</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
    </div>
</div>

<!-- Modal Captura -->
<div class="modal fade" id="modalCaptura" tabindex="-1" aria-labelledby="modalCapturaLabel" aria-hidden="true">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header bg-primary text-white">
                <h5 class="modal-title" id="modalCapturaLabel">Nuevo medio de contacto</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
            </div>
            <div class="modal-body">
                <div class="mb-2">
                    <label class="form-label">Forma de contacto *</label>
                    <input type="text" id="txtFormaContacto" class="form-control" />
                </div>
                <div class="mb-2">
                    <label class="form-label">Impacto *</label>
                    <select id="ddlImpacto" class="form-select">
                        <option value="BAJO">BAJO</option>
                        <option value="MEDIO">MEDIO</option>
                        <option value="ALTO">ALTO</option>
                    </select>
                </div>
                <div class="mb-2">
                    <label class="form-label">Probabilidad *</label>
                    <select id="ddlProbabilidad" class="form-select">
                        <option value="BAJA">BAJA</option>
                        <option value="MEDIA">MEDIA</option>
                        <option value="ALTA">ALTA</option>
                    </select>
                </div>
                <div class="mb-2">
                    <label class="form-label">Nivel de riesgo P.L.D *</label>
                    <select id="ddlRiesgoPLD" class="form-select">
                        <option value="BAJO">BAJO</option>
                        <option value="MEDIO">MEDIO</option>
                        <option value="ALTO">ALTO</option>
                    </select>
                </div>
                <div class="form-check mb-2">
                    <input type="checkbox" class="form-check-input" id="chkActivo" checked />
                    <label class="form-check-label" for="chkActivo">Activo</label>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" id="btnGuardar" class="btn btn-primary" type="button">Guardar</button>
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" type="button">Cancelar</button>
            </div>
        </div>
    </div>
</div>

<!-- Script -->
<script>
document.addEventListener("DOMContentLoaded", function () {
    const tabla = $("#tablaMedios").DataTable({
        ajax: {
            url: "/handlers/handler_medios_contacto.ashx?action=listar",
            dataSrc: json => json.data || []
        },
        columns: [
            { data: "id" },
            { data: "forma_contacto" },
            { data: "impacto" },
            { data: "probabilidad" },
            { data: "nivel_riesgo_pld" },
            {
                data: "activo",
                render: d => d ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>'
            },
            {
                data: null,
                render: (data, type, row) => `
                    <button type="button" class="btn btn-sm btn-primary btnEditar" data-id="${row.id}"><i class="fas fa-edit"></i></button>
                    <button type="button" class="btn btn-sm btn-danger btnEliminar" data-id="${row.id}"><i class="fas fa-trash-alt"></i></button>
                `
            }
        ],
        pageLength: 10,
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
        }
    });

    // Guardar o actualizar
    document.getElementById("btnGuardar").addEventListener("click", function () {
        const datos = new FormData();
        const esNuevo = (this.dataset.id || "") === "";

        datos.append("action", "guardar");
        datos.append("id", this.dataset.id || "");
        datos.append("forma_contacto", document.getElementById("txtFormaContacto").value.trim());
        datos.append("impacto", document.getElementById("ddlImpacto").value);
        datos.append("probabilidad", document.getElementById("ddlProbabilidad").value);
        datos.append("nivel_riesgo_pld", document.getElementById("ddlRiesgoPLD").value);
        datos.append("activo", document.getElementById("chkActivo").checked);

        fetch("/handlers/handler_medios_contacto.ashx", {
            method: "POST",
            body: datos
        })
        .then(res => res.json())
        .then(resp => {
            if (resp.success) {
                swal.fire("¡Listo!", "Registro guardado correctamente", "success");
                $("#modalCaptura").modal("hide");
                tabla.ajax.reload();

                if (esNuevo) {
                    document.getElementById("txtFormaContacto").value = "";
                    document.getElementById("ddlImpacto").value = "BAJO";
                    document.getElementById("ddlProbabilidad").value = "BAJA";
                    document.getElementById("ddlRiesgoPLD").value = "BAJO";
                    document.getElementById("chkActivo").checked = true;
                    this.dataset.id = "";
                }
            } else {
                swal.fire("Error", resp.error || "Ocurrió un error al guardar", "error");
            }
        });
    });

    // Editar
    $('#tablaMedios tbody').on('click', '.btnEditar', function () {
        const id = this.getAttribute("data-id");
        const fila = tabla.rows().data().toArray().find(x => x.id == id);

        if (!fila) {
            swal.fire("Error", "No se pudo obtener la fila para editar", "error");
            return;
        }

        document.getElementById("txtFormaContacto").value = fila.forma_contacto || "";
        document.getElementById("ddlImpacto").value = fila.impacto || "BAJO";
        document.getElementById("ddlProbabilidad").value = fila.probabilidad || "BAJA";
        document.getElementById("ddlRiesgoPLD").value = fila.nivel_riesgo_pld || "BAJO";
        document.getElementById("chkActivo").checked = fila.activo ? true : false;
        document.getElementById("btnGuardar").dataset.id = fila.id;

        const modal = new bootstrap.Modal(document.getElementById("modalCaptura"));
        modal.show();
    });

    // Eliminar
    $('#tablaMedios tbody').on('click', '.btnEliminar', function () {
        const id = this.getAttribute("data-id");

        swal.fire({
            title: '¿Estás seguro?',
            text: "Esto dará de baja el registro.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                const datos = new FormData();
                datos.append("action", "eliminar");
                datos.append("id", id);

                fetch("/handlers/handler_medios_contacto.ashx", {
                    method: "POST",
                    body: datos
                })
                .then(res => res.json())
                .then(resp => {
                    if (resp.success) {
                        swal.fire("¡Eliminado!", "El registro fue dado de baja.", "success");
                        tabla.ajax.reload();
                    } else {
                        swal.fire("Error", resp.error || "No se pudo eliminar", "error");
                    }
                });
            }
        });
    });
});
</script>
</asp:Content>
