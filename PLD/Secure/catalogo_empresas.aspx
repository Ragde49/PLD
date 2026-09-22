<%@ Page Title="Catálogo de Empresas" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_empresas.aspx.vb" Inherits="PLD.catalogo_empresas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
    <h4>Catálogo de Empresas</h4>
    <div class="card">
        <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
            <span>Empresas Registradas</span>
            <button type="button" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#modalEmpresa">
                <i class="fa fa-plus"></i> Capturar
            </button>
        </div>
        <div class="card-body">
            <table id="tablaEmpresas" class="table table-bordered table-sm w-100">
                <thead class="table-light">
                    <tr>
                        <th>ID</th>
                        <th>Empresa</th>
                        <th>Impacto</th>
                        <th>Probabilidad</th>
                        <th>Nivel PLD</th>
                        <th>Estatus</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
    </div>
</div>

<!-- Modal Empresa -->
<div class="modal fade" id="modalEmpresa" tabindex="-1" aria-labelledby="modalEmpresaLabel" aria-hidden="true">
  <div class="modal-dialog modal-lg">
    <div class="modal-content">
      <div class="modal-header bg-primary text-white">
        <h5 class="modal-title" id="modalEmpresaLabel">Captura de Empresa</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
      </div>
      <div class="modal-body">
        <input type="hidden" id="empresaId">
        <div id="formEmpresa">
          <div class="row">
            <div class="col-md-6"><label>Empresa *</label><input type="text" id="txtEmpresa" class="form-control"></div>
            <div class="col-md-6"><label>Código postal</label><input type="text" id="txtCP" class="form-control"></div>
            <div class="col-md-6"><label>Colonia</label><select id="ddlColonia" class="form-select"><option value="">Seleccione una colonia</option></select></div>
            <div class="col-md-6"><label>Estado</label><input type="text" id="txtEstado" class="form-control" readonly></div>
            <div class="col-md-6"><label>Municipio</label><input type="text" id="txtMunicipio" class="form-control" readonly></div>
            <div class="col-md-6"><label>Ciudad</label><input type="text" id="txtCiudad" class="form-control" readonly></div>
            <div class="col-md-6"><label>País domicilio empresa *</label><select id="ddlPais" class="form-select"></select></div>
            <div class="col-md-6"><label>Calle</label><input type="text" id="txtCalle" class="form-control"></div>
            <div class="col-md-6"><label>Número</label><input type="text" id="txtNumero" class="form-control"></div>
            <div class="col-md-6"><label>Teléfono</label><input type="text" id="txtTelefono" class="form-control"></div>
            <div class="col-md-6"><label>Extensión</label><input type="text" id="txtExtension" class="form-control"></div>
            <div class="col-md-6"><label>Impacto *</label><input type="text" id="txtImpacto" class="form-control"></div>
            <div class="col-md-6"><label>Probabilidad *</label><input type="text" id="txtProbabilidad" class="form-control"></div>
            <div class="col-md-6"><label>Nivel de riesgo P.L.D *</label><input type="text" id="txtNivelRiesgo" class="form-control"></div>
            <div class="col-md-6 mt-4"><div class="form-check"><input class="form-check-input" type="checkbox" id="chkActivo" checked><label class="form-check-label" for="chkActivo">Activo</label></div></div>
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-primary" id="btnGuardarEmpresa"><i class="fa fa-save"></i> Guardar</button>
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
      </div>
    </div>
  </div>
</div>

<script>
function riesgoTextoANumero(valor) {
    switch (valor.toUpperCase()) {
        case "BAJO": return 1;
        case "MEDIO": return 2;
        case "ALTO": return 3;
        default:
            const num = parseFloat(valor.replace(",", "."));
            return isNaN(num) ? 0 : num;
    }
}

document.addEventListener("DOMContentLoaded", function () {
    const tabla = $('#tablaEmpresas').DataTable({
        ajax: {
            url: '/handlers/handler_empresas.ashx?action=listar',
            dataSrc: 'data'
        },
        columns: [
            { data: 'id' },
            { data: 'empresa' },
            { data: 'impacto' },
            { data: 'probabilidad' },
            { data: 'nivel_riesgo_pld' },
            {
                data: 'estatus',
                render: data => data ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>'
            },
            {
                data: null,
                render: row => `
                    <button type="button" class="btn btn-sm btn-outline-primary btn-editar" data-id="${row.id}"><i class="fa fa-edit"></i></button>
                    <button type="button" class="btn btn-sm btn-outline-danger btn-eliminar" data-id="${row.id}"><i class="fa fa-trash"></i></button>`
            }
        ],
        pageLength: 25,
        lengthChange: false,
        searching: false
    });

    fetch('/handlers/handler_paises.ashx?op=combo')
      .then(response => response.json())
      .then(data => {
          const ddl = document.getElementById("ddlPais");
          ddl.innerHTML = '<option value="">Seleccione un país</option>';
          data.forEach(item => {
              const opt = document.createElement("option");
              opt.value = item.pais;
              opt.textContent = item.pais;
              ddl.appendChild(opt);
          });
      })
      .catch(error => {
          console.error("Error al cargar países:", error);
          Swal.fire("Error", "No se pudieron cargar los países.", "error");
      });

    document.getElementById("txtCP").addEventListener("keyup", function () {
      const cp = this.value.trim();
      if (cp.length === 5) {
        fetch(`/handlers/handler_sepomex.ashx?accion=buscarcp&cp=${cp}`)
          .then(resp => resp.json())
          .then(data => {
            if (data.length > 0) {
              const primer = data[0];
              document.getElementById("txtEstado").value = primer.estado;
              document.getElementById("txtMunicipio").value = primer.municipio;
              document.getElementById("txtCiudad").value = primer.ciudad;
              const ddlColonia = document.getElementById("ddlColonia");
              ddlColonia.innerHTML = "";
              data.forEach(col => {
                const opt = document.createElement("option");
                opt.value = col.colonia;
                opt.textContent = col.colonia;
                ddlColonia.appendChild(opt);
              });
            }
          });
      }
    });

    document.getElementById("btnGuardarEmpresa").addEventListener("click", function () {
        const payload = {
            accion: document.getElementById("empresaId").value ? "editar" : "guardar",
            id: document.getElementById("empresaId").value,
            empresa: document.getElementById("txtEmpresa").value.trim(),
            codigo_postal: document.getElementById("txtCP").value.trim(),
            estado: document.getElementById("txtEstado").value.trim(),
            municipio: document.getElementById("txtMunicipio").value.trim(),
            ciudad: document.getElementById("txtCiudad").value.trim(),
            colonia: document.getElementById("ddlColonia").value.trim(),
            calle: document.getElementById("txtCalle").value.trim(),
            numero: document.getElementById("txtNumero").value.trim(),
            pais_domicilio_empresa: document.getElementById("ddlPais").value.trim(),
            telefono: document.getElementById("txtTelefono").value.trim(),
            extension: document.getElementById("txtExtension").value.trim(),
            impacto: riesgoTextoANumero(document.getElementById("txtImpacto").value.trim()),
            probabilidad: riesgoTextoANumero(document.getElementById("txtProbabilidad").value.trim()),
            nivel_riesgo_pld: riesgoTextoANumero(document.getElementById("txtNivelRiesgo").value.trim()),
            estatus: document.getElementById("chkActivo").checked ? 1 : 0
        };

        if (!payload.empresa || !payload.pais_domicilio_empresa || payload.impacto === 0 || payload.probabilidad === 0 || payload.nivel_riesgo_pld === 0) {
            Swal.fire('Faltan campos obligatorios', 'Verifica los campos marcados con *', 'warning');
            return;
        }

        fetch('/handlers/handler_empresas.ashx', {
            method: 'POST',
            body: JSON.stringify(payload),
            headers: { 'Content-Type': 'application/json' }
        })
        .then(resp => resp.json())
        .then(data => {
            if (data.success) {
                Swal.fire('Guardado', 'La empresa se guardó correctamente.', 'success').then(() => {
                    tabla.ajax.reload();
                    document.querySelectorAll('#formEmpresa input, #formEmpresa select').forEach(el => {
                        if (el.type === 'checkbox') el.checked = false;
                        else el.value = '';
                    });
                    document.getElementById("empresaId").value = "";
                    const modal = bootstrap.Modal.getInstance(document.getElementById("modalEmpresa"));
                    modal.hide();
                });
            } else {
                Swal.fire('Error', data.error || 'No se pudo guardar la empresa.', 'error');
            }
        });
    });

    $('#tablaEmpresas tbody').on('click', '.btn-editar', function () {
        const data = tabla.row($(this).parents('tr')).data();
        document.getElementById("empresaId").value = data.id;
        document.getElementById("txtEmpresa").value = data.empresa;
        document.getElementById("txtImpacto").value = data.impacto;
        document.getElementById("txtProbabilidad").value = data.probabilidad;
        document.getElementById("txtNivelRiesgo").value = data.nivel_riesgo_pld;
        document.getElementById("chkActivo").checked = data.estatus;
        document.getElementById("txtCP").value = data.codigo_postal;
        document.getElementById("txtCalle").value = data.calle;
        document.getElementById("txtNumero").value = data.numero;
        document.getElementById("txtTelefono").value = data.telefono;
        document.getElementById("txtExtension").value = data.extension;
        setTimeout(() => document.getElementById("ddlPais").value = data.pais_domicilio_empresa, 300);

        fetch(`/handlers/handler_sepomex.ashx?accion=buscarcp&cp=${data.codigo_postal}`)
        .then(resp => resp.json())
        .then(res => {
            const ddlColonia = document.getElementById("ddlColonia");
            ddlColonia.innerHTML = "";
            res.forEach(col => {
                const opt = document.createElement("option");
                opt.value = col.colonia;
                opt.textContent = col.colonia;
                ddlColonia.appendChild(opt);
            });
            document.getElementById("txtEstado").value = res[0]?.estado || '';
            document.getElementById("txtMunicipio").value = res[0]?.municipio || '';
            document.getElementById("txtCiudad").value = res[0]?.ciudad || '';
            ddlColonia.value = data.colonia;
        });

        new bootstrap.Modal(document.getElementById("modalEmpresa")).show();
    });

    $('#tablaEmpresas tbody').on('click', '.btn-eliminar', function () {
        const id = $(this).data('id');
        Swal.fire({
            title: '¿Estás seguro?',
            text: 'Esta acción eliminará la empresa de forma permanente.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        }).then(result => {
            if (result.isConfirmed) {
                fetch('/handlers/handler_empresas.ashx', {
                    method: 'POST',
                    body: JSON.stringify({ accion: "eliminar", id }),
                    headers: { 'Content-Type': 'application/json' }
                })
                .then(resp => resp.json())
                .then(data => {
                    if (data.success) {
                        Swal.fire('Eliminado', 'La empresa fue eliminada.', 'success');
                        tabla.ajax.reload();
                    } else {
                        Swal.fire('Error', 'No se pudo eliminar.', 'error');
                    }
                });
            }
        });
    });
});
</script>
</asp:Content>
