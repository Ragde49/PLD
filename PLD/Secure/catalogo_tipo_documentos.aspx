<%@ Page Title="Catálogo Tipo Documentos" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <div class="container-fluid mt-4">
    <div class="card border-primary">
      <div class="card-header bg-primary text-white">
        <div class="d-flex justify-content-between align-items-center">
          <span><i class="fa fa-list"></i> Tipo de documentos</span>
          <button type="button" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#modalTipoDocumento">
            <i class="fa fa-plus"></i> Capturar
          </button>
        </div>
      </div>
      <div class="card-body">
        <div class="table-responsive">
          <table id="tablaDocumentos" class="table table-bordered table-striped table-sm w-100">
            <thead class="table-light">
              <tr>
                <th>ID</th>
                <th>Documento</th>
                <th>Estatus</th>
                <th style="width: 90px;">Acciones</th>
              </tr>
            </thead>
            <tbody></tbody>
          </table>
        </div>
      </div>
    </div>
  </div>

  <!-- Modal -->
  <div class="modal fade" id="modalTipoDocumento" tabindex="-1" aria-labelledby="modalLabel" aria-hidden="true">
    <div class="modal-dialog">
      <div class="modal-content border-primary">
        <div class="modal-header bg-primary text-white">
          <h5 class="modal-title" id="modalLabel"><i class="fa fa-plus-circle"></i> Nuevo tipo de documento</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
        </div>
        <div class="modal-body">
          <input type="hidden" id="idDocumentoEditar" value="" />
          <div class="mb-3">
            <label for="descripcionDocumento" class="form-label">Descripción</label>
            <input type="text" class="form-control" id="descripcionDocumento" placeholder="Ej. CURP, INE, Comprobante de domicilio" />
          </div>
          <div class="form-check">
            <input class="form-check-input" type="checkbox" id="estatusActivo" checked>
            <label class="form-check-label" for="estatusActivo">
              Activo
            </label>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" id="btnGuardar" class="btn btn-primary btn-sm">
            <i class="fa fa-save"></i> Guardar
          </button>
          <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">
            Cancelar
          </button>
        </div>
      </div>
    </div>
  </div>

  <script>
    document.addEventListener("DOMContentLoaded", function () {
      const tabla = new DataTable('#tablaDocumentos', {
        ajax: {
          url: '/handlers/handler_catalogo_tipo_documentos.ashx?modo=consultar',
          dataSrc: ''
        },
        columns: [
          { data: 'id' },
          { data: 'descripcion' },
          {
            data: 'activo',
            render: function (data) {
              return data
                ? '<span class="badge bg-success">Activo</span>'
                : '<span class="badge bg-secondary">Inactivo</span>';
            }
          },
          {
            data: null,
            orderable: false,
            render: function (data, type, row) {
                return `
                <button type="button" class="btn btn-sm btn-primary me-1 btn-editar" data-id="${row.id}" data-descripcion="${row.descripcion}" data-activo="${row.activo}">
                  <i class="fa fa-edit"></i>
                </button>
                <button type="button" class="btn btn-sm btn-danger btn-eliminar" data-id="${row.id}">
                  <i class="fa fa-trash"></i>
                </button>
              `;
            }
          }
        ],
        language: {
          url: '//cdn.datatables.net/plug-ins/1.13.4/i18n/es-ES.json'
        },
        pageLength: 25
      });

      // Botón guardar
      document.getElementById("btnGuardar").addEventListener("click", function () {
        const id = document.getElementById("idDocumentoEditar").value;
        const descripcion = document.getElementById("descripcionDocumento").value.trim();
        const activo = document.getElementById("estatusActivo").checked;

        if (!descripcion) {
          Swal.fire("Dato requerido", "La descripción no puede estar vacía", "warning");
          return;
        }

        const payload = new URLSearchParams();
        payload.append("modo", id ? "actualizar" : "insertar");
        payload.append("id", id);
        payload.append("descripcion", descripcion);
        payload.append("activo", activo ? "1" : "0");

        fetch("/handlers/handler_catalogo_tipo_documentos.ashx", {
          method: "POST",
          body: payload,
          headers: {
            "Content-Type": "application/x-www-form-urlencoded"
          }
        })
          .then(resp => resp.json())
          .then(data => {
            if (data.ok) {
              Swal.fire("Guardado", "El tipo de documento fue procesado correctamente", "success");
              document.getElementById("descripcionDocumento").value = "";
              document.getElementById("estatusActivo").checked = true;
              document.getElementById("idDocumentoEditar").value = "";
              const modal = bootstrap.Modal.getInstance(document.getElementById("modalTipoDocumento"));
              modal.hide();
              tabla.ajax.reload();
            } else {
              Swal.fire("Error", data.mensaje || "No se pudo guardar", "error");
            }
          })
          .catch(err => {
            console.error(err);
            Swal.fire("Error", "Ocurrió un error inesperado", "error");
          });
      });

      // Evento editar
      document.addEventListener("click", function (e) {
        if (e.target.closest(".btn-editar")) {
          const btn = e.target.closest(".btn-editar");
          document.getElementById("idDocumentoEditar").value = btn.dataset.id;
          document.getElementById("descripcionDocumento").value = btn.dataset.descripcion;
          document.getElementById("estatusActivo").checked = btn.dataset.activo === "true";
          const modal = new bootstrap.Modal(document.getElementById("modalTipoDocumento"));
          modal.show();
        }
      });

      // Evento eliminar
      document.addEventListener("click", function (e) {
        if (e.target.closest(".btn-eliminar")) {
          const id = e.target.closest(".btn-eliminar").dataset.id;

          Swal.fire({
            title: "¿Eliminar?",
            text: "Esta acción no se puede deshacer",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
          }).then(result => {
            if (result.isConfirmed) {
              fetch("/handlers/handler_catalogo_tipo_documentos.ashx", {
                method: "POST",
                headers: {
                  "Content-Type": "application/x-www-form-urlencoded"
                },
                body: new URLSearchParams({
                  modo: "eliminar",
                  id: id
                })
              })
                .then(resp => resp.json())
                .then(data => {
                  if (data.ok) {
                    Swal.fire("Eliminado", "Registro eliminado correctamente", "success");
                    tabla.ajax.reload();
                  } else {
                    Swal.fire("Error", data.mensaje || "No se pudo eliminar", "error");
                  }
                });
            }
          });
        }
      });
    });
  </script>
</asp:Content>
