<%@ Page Title="Catálogo Municipios" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <div class="container-fluid mt-4">

    <!-- CARD AZUL (header con título y botón Capturar) -->
    <div class="card mb-3">
      <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
        <div class="d-flex align-items-center">
          <i class="fa fa-list me-2" aria-hidden="true" style="font-size:18px"></i>
          <strong style="font-size:16px">Municipios &amp; Lugar de operación</strong>
        </div>
        <div>
          <button type="button" id="btnCapturar" class="btn btn-success btn-sm">
            <i class="fa fa-plus"></i> Capturar
          </button>
        </div>
      </div>
      <!-- opcional cuerpo vacío (como en la UI original aparece sólo header azul) -->
      <div class="card-body p-2" style="display:none;"></div>
    </div>
    <!-- /CARD AZUL -->

    <!-- Tabla (DataTable muestra su propio buscador y paginación) -->
    <div class="table-responsive">
      <table id="tblMunicipios" class="table table-sm table-striped w-100">
        <thead class="table-light">
          <tr>
            <th style="width:60px">ID</th>
            <th>Descripción</th>
            <th style="width:120px">Impacto</th>
            <th style="width:120px">Ocurrencia</th>
            <th style="width:140px">Nivel Riesgo P.L.D</th>
            <th style="width:110px">Estatus</th>
            <th style="width:90px">Mitigantes</th>
            <th style="width:120px">Acciones</th>
          </tr>
        </thead>
        <tbody>
        </tbody>
      </table>
    </div>

    <!-- Modal: Captura / Edición -->
    <div class="modal fade" id="modalMunicipio" tabindex="-1" aria-labelledby="modalMunicipioLabel" aria-hidden="true">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="modalMunicipioLabel">Capturar Municipio</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
          </div>
          <div class="modal-body">
            <form id="formMunicipio" class="row g-2">
              <input type="hidden" id="hidId" />
              <div class="col-12">
                <label class="form-label">Municipio *</label>
                <input type="text" id="txtDescripcion" class="form-control" maxlength="250" />
              </div>
              <div class="col-md-6">
                <label class="form-label">Cve estado *</label>
                <input type="text" id="txtClaveEstado" class="form-control" maxlength="10" />
              </div>
              <div class="col-md-6">
                <label class="form-label">Clave Municipio *</label>
                <input type="text" id="txtClaveMunicipio" class="form-control" maxlength="50" />
              </div>
              <div class="col-md-4">
                <label class="form-label">Impacto *</label>
                <input type="number" id="txtImpacto" class="form-control" min="0" step="1" />
              </div>
              <div class="col-md-4">
                <label class="form-label">Probabilidad *</label>
                <input type="number" id="txtProbabilidad" class="form-control" min="0" max="100" step="1" />
              </div>
              <div class="col-md-4">
                <label class="form-label">Nivel de riesgo P.L.D *</label>
                <input type="number" id="txtNivel" class="form-control" min="0" step="0.01" />
              </div>
              <div class="col-12 d-flex align-items-center">
                <div class="form-check me-3">
                  <input class="form-check-input" type="checkbox" id="chkEstatus" checked>
                  <label class="form-check-label" for="chkEstatus">Activo</label>
                </div>
                <div>
                  <label class="form-label mb-0">Mitigantes</label>
                  <input type="number" id="txtMitigantes" class="form-control d-inline-block" style="width:100px" min="0" step="1" value="0" />
                </div>
              </div>
            </form>
          </div>
          <div class="modal-footer">
            <button type="button" id="btnGuardar" class="btn btn-primary">
              <i class="fa fa-check"></i> Guardar información
            </button>
            <button type="button" id="btnCancelar" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
          </div>
        </div>
      </div>
    </div>

  </div>

  <!-- Scripts (igual que antes, no toco la lógica del handler) -->
  <script>
    document.addEventListener("DOMContentLoaded", function () {
      const handlerUrl = '/handlers/handler_catalogo_municipios.ashx';
      const tbl = document.getElementById('tblMunicipios');
      const btnCapturar = document.getElementById('btnCapturar');
      const modalEl = document.getElementById('modalMunicipio');
      const bsModal = new bootstrap.Modal(modalEl, { keyboard: false });
      const form = document.getElementById('formMunicipio');
      const btnGuardar = document.getElementById('btnGuardar');

      // Inicializar DataTable (dejamos búsqueda y paginación propios de DataTables)
      const dataTable = $(tbl).DataTable({
        ajax: { url: handlerUrl + '?op=consultar', dataSrc: function (json) { return json.success ? json.data : []; } },
        columns: [
          { data: 'id' },
          { data: 'descripcion' },
          { data: 'impacto' },
          { data: 'probabilidad', render: d => (d || 0) + '%' },
          { data: 'nivel_riesgo_pld', render: d => (parseFloat(d || 0)).toFixed(2) },
          { data: 'estatus', render: d => (d === true || d === 1 || d === '1') ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>' },
          { data: 'mitigantes' },
          {
            data: null, orderable: false, searchable: false,
            render: function (row, type, full) {
              const id = full.id;
              return '' +
                '<button type="button" class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="' + id + '" title="Editar"><i class="fa fa-edit"></i></button>' +
                '<button type="button" class="btn btn-sm btn-outline-danger btn-delete" data-id="' + id + '" title="Eliminar"><i class="fa fa-trash"></i></button>';
            }
          }
        ],
        pageLength: 25,
        lengthChange: true,
        ordering: false
      });

      // Abrir modal nuevo
      btnCapturar.addEventListener('click', function () {
        form.reset();
        document.getElementById('hidId').value = '';
        document.getElementById('txtNivel').value = '0.00';
        document.getElementById('txtMitigantes').value = '0';
        document.getElementById('modalMunicipioLabel').textContent = 'Capturar Municipio';
        bsModal.show();
      });

      // Delegación para editar/eliminar
      $(tbl).on('click', '.btn-edit', function () {
        const id = this.getAttribute('data-id');
        fetch(handlerUrl + '?op=obtener&id=' + encodeURIComponent(id)).then(r => r.json()).then(resp => {
          if (resp.success) {
            const d = resp.data;
            document.getElementById('hidId').value = d.id;
            document.getElementById('txtDescripcion').value = d.descripcion || '';
            document.getElementById('txtClaveMunicipio').value = d.clave_municipio || '';
            document.getElementById('txtClaveEstado').value = d.clave_estado || '';
            document.getElementById('txtImpacto').value = d.impacto || 0;
            document.getElementById('txtProbabilidad').value = d.probabilidad || 0;
            document.getElementById('txtNivel').value = (d.nivel_riesgo_pld || 0).toFixed ? (parseFloat(d.nivel_riesgo_pld).toFixed(2)) : d.nivel_riesgo_pld;
            document.getElementById('chkEstatus').checked = (d.estatus === true || d.estatus === 1 || d.estatus === '1');
            document.getElementById('txtMitigantes').value = d.mitigantes || 0;
            document.getElementById('modalMunicipioLabel').textContent = 'Editar Municipio';
            bsModal.show();
          } else {
            swal.fire('Error', resp.message || 'No se encontró registro', 'error');
          }
        });
      });

      $(tbl).on('click', '.btn-delete', function () {
        const id = this.getAttribute('data-id');
        swal.fire({
          title: 'Confirmar',
          text: '¿Eliminar este registro?',
          icon: 'warning',
          showCancelButton: true,
          confirmButtonText: 'Si, eliminar',
          cancelButtonText: 'Cancelar'
        }).then(result => {
          if (result.isConfirmed) {
            fetch(handlerUrl + '?op=eliminar&id=' + encodeURIComponent(id), { method: 'POST' })
              .then(r => r.json()).then(resp => {
                if (resp.success) { swal.fire('Eliminado', resp.message || 'Registro eliminado', 'success'); dataTable.ajax.reload(); }
                else swal.fire('Error', resp.message || 'No se pudo eliminar', 'error');
              });
          }
        });
      });

      // Guardar (alta/actualizar)
      btnGuardar.addEventListener('click', function () {
        const id = document.getElementById('hidId').value;
        const payload = {
          descripcion: document.getElementById('txtDescripcion').value.trim(),
          clave_municipio: document.getElementById('txtClaveMunicipio').value.trim(),
          clave_estado: document.getElementById('txtClaveEstado').value.trim(),
          estado: document.getElementById('txtClaveEstado').value.trim(),
          impacto: parseInt(document.getElementById('txtImpacto').value) || 0,
          probabilidad: parseInt(document.getElementById('txtProbabilidad').value) || 0,
          nivel_riesgo_pld: parseFloat(document.getElementById('txtNivel').value) || 0.00,
          estatus: document.getElementById('chkEstatus').checked ? 1 : 0,
          mitigantes: parseInt(document.getElementById('txtMitigantes').value) || 0
        };
        let op = 'guardar';
        if (id && id.trim() !== '') { op = 'actualizar'; payload.id = parseInt(id, 10); }

        fetch(handlerUrl + '?op=' + op, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) })
          .then(r => r.json()).then(resp => {
            if (resp.success) { swal.fire('OK', resp.message || 'Guardado', 'success'); bsModal.hide(); dataTable.ajax.reload(); }
            else swal.fire('Error', resp.message || 'No se pudo guardar', 'error');
          }).catch(err => { console.error(err); swal.fire('Error', 'Error en la petición', 'error'); });
      });

    });
  </script>
</asp:Content>
