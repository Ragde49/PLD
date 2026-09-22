<%@ Page Title="Centro de Trabajo" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <h4 class="mb-4"><i class="fa fa-building"></i> Catálogo: Centro de Trabajo</h4>

        <div class="mb-3">
            <button type="button" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#modalCentroTrabajo">
                <i class="fa fa-plus"></i> Capturar
            </button>
        </div>

        <table id="tablaCentroTrabajo" class="table table-bordered table-striped w-100">
            <thead class="table-primary text-center">
                <tr>
                    <th>ID</th>
                    <th>Centro de Trabajo</th>
                    <th>Estatus</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody></tbody>
        </table>
    </div>

    <!-- Modal -->
    <div class="modal fade" id="modalCentroTrabajo" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title">Captura de Centro de Trabajo</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="idCentro" />
                    <div class="mb-3">
                        <label for="descripcion" class="form-label">Centro de Trabajo *</label>
                        <input type="text" class="form-control" id="descripcion" required />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Estatus</label><br />
                        <input type="checkbox" id="activo" checked />
                        <label for="activo">Activo</label>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" id="btnGuardar" class="btn btn-primary">Guardar</button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                </div>
            </div>
        </div>
    </div>

    <script>
    document.addEventListener("DOMContentLoaded", function () {
        const tabla = $('#tablaCentroTrabajo').DataTable({
            ajax: {
                url: '/handlers/handler_centro_trabajo.ashx?accion=lista',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'descripcion' },
                {
                    data: 'activo',
                    render: function (data) {
                        return data === "Activo"
                            ? '<span class="badge bg-success">Activo</span>'
                            : '<span class="badge bg-danger">Inactivo</span>';
                    }
                },
                {
                    data: null,
                    className: 'text-center',
                    render: function (data) {
                        const isActivo = data.activo === "Activo";
                        return `
                            <button type="button" class="btn btn-sm btn-warning me-1" onclick="editar(${data.id}, '${data.descripcion.replace(/'/g, "\\'")}', ${isActivo})">
                                <i class="fa fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-danger" onclick="eliminar(${data.id})">
                                <i class="fa fa-trash"></i>
                            </button>`;
                    }
                }
            ],
            language: {
                emptyTable: "No existen registros disponibles"
            }
        });

        document.getElementById("btnGuardar").addEventListener("click", function () {
            const id = document.getElementById("idCentro").value || "0";
            const descripcion = document.getElementById("descripcion").value.trim();
            const activo = document.getElementById("activo").checked;

            if (!descripcion) {
                swal.fire("Aviso", "El campo 'Centro de Trabajo' es obligatorio", "warning");
                return;
            }

            fetch('/handlers/handler_centro_trabajo.ashx', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ accion: 'guardar', id, descripcion, activo })
            })
            .then(r => r.json())
            .then(res => {
                if (res.ok) {
                    swal.fire("Correcto", res.mensaje, "success");
                    tabla.ajax.reload();
                    limpiarModal();
                } else {
                    swal.fire("Error", res.mensaje, "error");
                }
            })
            .catch(err => {
                swal.fire("Error", "Error de conexión con el servidor", "error");
            });
        });
    });

    function editar(id, descripcion, activo) {
        document.getElementById("idCentro").value = id;
        document.getElementById("descripcion").value = descripcion;
        document.getElementById("activo").checked = activo;

        const modal = new bootstrap.Modal(document.getElementById('modalCentroTrabajo'));
        modal.show();
    }

    function eliminar(id) {
        swal.fire({
            title: "¿Eliminar?",
            text: "Esta acción eliminará el registro.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
        }).then((result) => {
            if (result.isConfirmed) {
                fetch('/handlers/handler_centro_trabajo.ashx', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ accion: 'eliminar', id })
                })
                .then(r => r.json())
                .then(res => {
                    if (res.ok) {
                        swal.fire("Correcto", res.mensaje, "success");
                        $('#tablaCentroTrabajo').DataTable().ajax.reload();
                    } else {
                        swal.fire("Error", res.mensaje, "error");
                    }
                })
                .catch(err => {
                    swal.fire("Error", "Error de conexión con el servidor", "error");
                });
            }
        });
    }

    function limpiarModal() {
        document.getElementById("idCentro").value = "";
        document.getElementById("descripcion").value = "";
        document.getElementById("activo").checked = true;
        const modal = bootstrap.Modal.getInstance(document.getElementById('modalCentroTrabajo'));
        modal.hide();
    }
</script>

</asp:Content>
