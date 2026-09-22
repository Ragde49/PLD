<%@ Page Title="Catálogo Fondeador" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
    <div class="card border-primary">
        <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center px-3 py-2">
            <strong>Fuente de Fondeo</strong>
            <button type="button" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#modalFondeador" onclick="nuevoFondeador()">
                <i class="fa fa-plus"></i> Capturar
            </button>
        </div>
        <div class="card-body">
            <table id="tablaFondeadores" class="table table-bordered table-striped table-sm w-100">
                <thead class="table-light">
                    <tr>
                        <th>ID</th>
                        <th>Fondeador</th>
                        <th>Observaciones</th>
                        <th>Monto</th>
                        <th>Estatus</th>
                        <th class="text-center">Acciones</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
    </div>
</div>

<!-- Modal -->
<div class="modal fade" id="modalFondeador" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-md modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header bg-primary text-white">
        <h5 class="modal-title">Captura de Fondeador</h5>
        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <input type="hidden" id="idFondeador" />
          <div class="mb-3">
            <label for="tipo_credito_id" class="form-label">Tipo de Crédito</label>
            <select class="form-select" id="tipo_credito_id">
                <option value="">SELECCIONAR TIPO DE CRÉDITO</option>
            </select>
        </div>
        <div class="mb-3">
            <label for="descripcion" class="form-label">Descripción *</label>
            <input type="text" class="form-control" id="descripcion" required />
        </div>
        <div class="mb-3">
            <label for="observaciones" class="form-label">Observaciones</label>
            <textarea class="form-control" id="observaciones" rows="2"></textarea>
        </div>
        <div class="mb-3">
            <label for="monto" class="form-label">Monto</label>
            <input type="number" class="form-control" id="monto" step="0.01" />
        </div>
        <div class="mb-3">
            <label for="estatus" class="form-label">Estatus</label>
            <select class="form-select" id="estatus">
                <option value="Activo">Activo</option>
                <option value="Inactivo">Inactivo</option>
                <option value="Pendiente">Pendiente</option>
            </select>
        </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
        <button type="button" class="btn btn-success" id="btnGuardarFondeador">Guardar</button>
      </div>
    </div>
  </div>
</div>

<script>
    document.addEventListener("DOMContentLoaded", function () {
        cargarTiposCredito();

        window.tablaFondeadores = $('#tablaFondeadores').DataTable({
            pageLength: 25,
            lengthChange: false,
            searching: false,
            ajax: {
                url: '/handlers/handler_fondeador.ashx?accion=lista',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'descripcion' },
                { data: 'observaciones' },
                { data: 'monto', render: d => parseFloat(d || 0).toLocaleString('es-MX', { minimumFractionDigits: 2 }) },
                {
                    data: 'estatus',
                    render: function (estatus) {
                        if (estatus === 'Activo') return '<span class="badge bg-success">Activo</span>';
                        if (estatus === 'Inactivo') return '<span class="badge bg-danger">Inactivo</span>';
                        return '<span class="badge bg-warning text-dark">Pendiente</span>';
                    }
                },
                {
                    data: null,
                    className: 'text-center',
                    orderable: false,
                    render: function (data) {
                        return `
                            <button type="button" class="btn btn-sm btn-info me-1" onclick="ver(${data.id})" title="Ver"><i class="fa fa-search"></i></button>
                            <button type="button" class="btn btn-sm btn-warning me-1" onclick="editar(${data.id})" title="Editar"><i class="fa fa-edit"></i></button>
                            <button type="button" class="btn btn-sm btn-danger" onclick="eliminar(${data.id})" title="Eliminar"><i class="fa fa-trash"></i></button>
                        `;
                    }
                }
            ],
            language: {
                paginate: {
                    previous: '&laquo;',
                    next: '&raquo;'
                },
                info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
                infoEmpty: "Sin registros",
                emptyTable: "No hay datos disponibles",
                lengthMenu: "Visualizar _MENU_ registros"
            }
        });

        document.getElementById("btnGuardarFondeador").addEventListener("click", function () {
            const id = document.getElementById("idFondeador").value || 0;
            const tipo_credito_id = document.getElementById("tipo_credito_id").value;
            const descripcion = document.getElementById("descripcion").value.trim();
            const observaciones = document.getElementById("observaciones").value.trim();
            const monto = parseFloat(document.getElementById("monto").value) || 0;
            const estatus = document.getElementById("estatus").value;

            if (!descripcion) {
                Swal.fire("Aviso", "La descripción es obligatoria", "warning");
                return;
            }

            fetch('/handlers/handler_fondeador.ashx', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    accion: 'guardar',
                    id,
                    descripcion,
                    observaciones,
                    monto,
                    estatus,
                    tipo_credito_id
                })
            })
            .then(r => r.json())
            .then(res => {
                if (res.ok) {
                    Swal.fire("Correcto", res.mensaje, "success");
                    tablaFondeadores.ajax.reload();
                    bootstrap.Modal.getInstance(document.getElementById('modalFondeador')).hide();
                } else {
                    Swal.fire("Error", res.mensaje, "error");
                }
            });
        });
    });

    function nuevoFondeador() {
        document.getElementById("idFondeador").value = '';
        document.getElementById("descripcion").value = '';
        document.getElementById("observaciones").value = '';
        document.getElementById("monto").value = '';
        document.getElementById("estatus").value = 'Activo';
    }

    function editar(id) {
        fetch(`/handlers/handler_fondeador.ashx?accion=detalle&id=${id}`)
        .then(r => r.json())
        .then(data => {
            document.getElementById("idFondeador").value = data.id;
            document.getElementById("descripcion").value = data.descripcion;
            document.getElementById("observaciones").value = data.observaciones;
            document.getElementById("monto").value = data.monto;
            document.getElementById("estatus").value = data.estatus;
            new bootstrap.Modal(document.getElementById('modalFondeador')).show();
        });
    }

    function ver(id) {
        Swal.fire("Ver fondeador", `ID: ${id}`, "info");
    }

    function eliminar(id) {
        Swal.fire({
            title: "¿Eliminar?",
            text: "Esta acción no se puede deshacer",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
        }).then((result) => {
            if (result.isConfirmed) {
                fetch('/handlers/handler_fondeador.ashx', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ accion: 'eliminar', id })
                })
                .then(r => r.json())
                .then(res => {
                    if (res.ok) {
                        Swal.fire("Eliminado", res.mensaje, "success");
                        tablaFondeadores.ajax.reload();
                    } else {
                        Swal.fire("Error", res.mensaje, "error");
                    }
                });
            }
        });
    }
    function cargarTiposCredito() {
        const select = document.getElementById("tipo_credito_id");
        select.innerHTML = '<option value="">SELECCIONAR TIPO DE CRÉDITO</option>';

        fetch('/handlers/handler_tipo_credito.ashx')
            .then(response => response.json())
            .then(data => {
                data.forEach(item => {
                    const option = document.createElement("option");
                    option.value = item.id;
                    option.textContent = item.descripcion;
                    select.appendChild(option);
                });
            })
            .catch(err => {
                console.error("Error al cargar tipos de crédito", err);
                Swal.fire("Error", "No se pudieron cargar los tipos de crédito", "error");
            });
    }
</script>
</asp:Content>
