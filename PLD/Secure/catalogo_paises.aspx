<%@ Page Title="Catálogo de Países" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <h4 class="mb-4">Catálogo de Países</h4>

        <div class="card border-primary">
            <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                <span><i class="fas fa-flag"></i> Lista de países</span>
                <button type="button" class="btn btn-light btn-sm" data-bs-toggle="modal" data-bs-target="#modalPais">
                    <i class="fas fa-plus"></i> Capturar
                </button>
            </div>
            <div class="card-body">
                <table id="tablaPaises" class="table table-sm table-bordered table-striped w-100">
                    <thead class="table-primary text-center">
                        <tr>
                            <th>ID</th>
                            <th>País</th>
                            <th>Activo</th>
                            <th>Fecha de creación</th>
                            <th>Acciones</th>
                        </tr>
                    </thead>
                    <tbody></tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- Modal Alta País -->
    <div class="modal fade" id="modalPais" tabindex="-1" aria-labelledby="modalPaisLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="modalPaisLabel">Nuevo País</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <form id="formPais">
                        <div class="mb-3">
                            <label for="txtPais" class="form-label">Nombre del país</label>
                            <input type="text" class="form-control" id="txtPais" maxlength="100" required>
                        </div>
                        <div class="mb-3">
                            <label for="txtIso" class="form-label">Código ISO (opcional)</label>
                            <input type="text" class="form-control" id="txtIso" maxlength="5">
                        </div>
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox" id="chkActivo" checked>
                            <label class="form-check-label" for="chkActivo">
                                Activo
                            </label>
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" class="btn btn-primary btn-sm" id="btnGuardarPais">Guardar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Script -->
<script>
  document.addEventListener("DOMContentLoaded", function () {
    let paisId = null;

    const tabla = $('#tablaPaises').DataTable({
      ajax: {
        url: '/handlers/handler_paises.ashx?op=lista',
        dataSrc: 'data'
      },
      pageLength: 25,
      columns: [
        { data: 'id' },
        { data: 'pais' },
        {
          data: 'activo',
          className: 'text-center',
          render: data => data ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>'
        },
        {
          data: 'fecha_creacion',
          render: data => moment(data).format('DD/MM/YYYY HH:mm')
        },
        {
          data: null,
          className: 'text-center',
          orderable: false,
          render: data => `
            <button type="button" class="btn btn-sm btn-info me-1 btn-editar" data-id="${data.id}" title="Editar"><i class="fas fa-edit"></i></button>
            <button type="button" class="btn btn-sm btn-danger btn-eliminar" data-id="${data.id}" title="Eliminar"><i class="fas fa-trash-alt"></i></button>
          `
        }
      ],
      language: {
        url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
      }
    });

    // Botón "Capturar" limpia modal
    document.querySelector('[data-bs-target="#modalPais"]').addEventListener("click", function () {
      paisId = null;
      const form = document.getElementById("formPais");
      if (form) form.reset();
      document.getElementById("chkActivo").checked = true;
    });

    // Guardar país (alta o edición)
    document.getElementById("btnGuardarPais").addEventListener("click", function () {
      const pais = document.getElementById("txtPais").value.trim();
      const iso = document.getElementById("txtIso").value.trim();
      const activo = document.getElementById("chkActivo").checked ? 1 : 0;

      if (pais === "") {
        Swal.fire("Campo obligatorio", "El nombre del país no puede estar vacío.", "warning");
        return;
      }

      const params = new URLSearchParams({
        op: paisId ? 'editar' : 'insertar',
        pais: pais,
        iso: iso,
        activo: activo
      });

      if (paisId) params.append("id", paisId);

      fetch('/handlers/handler_paises.ashx', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: params
      })
      .then(res => {
        console.log("HTTP STATUS:", res.status);
        if (!res.ok) throw new Error("Respuesta HTTP no válida");
        return res.json();
      })
      .then(data => {
        if (data.ok) {
          Swal.fire("Éxito", data.mensaje, "success");
          tabla.ajax.reload();

          paisId = null;
          const form = document.getElementById("formPais");
          if (form) form.reset();

          const modal = bootstrap.Modal.getInstance(document.getElementById("modalPais"));
          if (modal) modal.hide();
        } else {
          Swal.fire("Error", data.mensaje || "Ocurrió un error.", "error");
        }
      })
      .catch(err => {
        console.error("🧨 ERROR EN FETCH:", err);
        Swal.fire("Error", "Error de red o del servidor.", "error");
      });
    });

    // Botón Editar
    $('#tablaPaises tbody').on('click', '.btn-editar', function () {
      const rowData = tabla.row($(this).parents('tr')).data();
      paisId = rowData.id;
      document.getElementById("txtPais").value = rowData.pais;
      document.getElementById("txtIso").value = rowData.iso || "";
      document.getElementById("chkActivo").checked = rowData.activo;

      const modal = new bootstrap.Modal(document.getElementById("modalPais"));
      modal.show();
    });

    // Botón Eliminar
    $('#tablaPaises tbody').on('click', '.btn-eliminar', function () {
      const id = this.getAttribute("data-id");

      Swal.fire({
        title: "¿Eliminar país?",
        text: "Esta acción no se puede deshacer.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "Cancelar"
      }).then(result => {
        if (result.isConfirmed) {
          fetch('/handlers/handler_paises.ashx', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({ op: 'eliminar', id: id })
          })
          .then(res => {
            if (!res.ok) throw new Error("Respuesta HTTP no válida");
            return res.json();
          })
          .then(data => {
            if (data.ok) {
              Swal.fire("Eliminado", data.mensaje, "success");
              tabla.ajax.reload();
            } else {
              Swal.fire("Error", data.mensaje || "No se pudo eliminar.", "error");
            }
          })
          .catch(err => {
            console.error("🧨 ERROR AL ELIMINAR:", err);
            Swal.fire("Error", "Error de red o del servidor.", "error");
          });
        }
      });
    });
  });
</script>



</asp:Content>
