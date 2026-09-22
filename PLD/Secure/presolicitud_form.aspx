<%@ Page Title="Captura de Presolicitud" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="presolicitud_form.aspx.vb" Inherits="PLD.presolicitud_form" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container-fluid mt-5 pt-3">

        <h4><i class="fas fa-file-alt"></i> Captura de Presolicitud</h4>

        <!-- Nav tabs -->
        <ul class="nav nav-tabs mt-4" id="tabsPresolicitud" role="tablist">
            <li class="nav-item">
                <a class="nav-link active" id="tab-contacto-tab" data-bs-toggle="tab" href="#tab-contacto" role="tab">Medio de Contacto</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="tab-identificacion-tab" data-bs-toggle="tab" href="#tab-identificacion" role="tab">Identificación</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="tab-laboral-tab" data-bs-toggle="tab" href="#tab-laboral" role="tab">Laboral</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="tab-operacion-tab" data-bs-toggle="tab" href="#tab-operacion" role="tab">Operación</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="tab-contacto2-tab" data-bs-toggle="tab" href="#tab-contacto2" role="tab">Contacto</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="tab-pld-tab" data-bs-toggle="tab" href="#tab-pld" role="tab">P.L.D.</a>
            </li>
        </ul>

        <!-- Tab panes -->
        <div class="tab-content border p-4 bg-white">
            <div class="tab-pane fade show active" id="tab-contacto" role="tabpanel">
                <!-- Aquí va el contenido de Medio de Contacto -->
                <div class="row">
                    <div class="col-md-4">
                        <label for="medioContacto" class="form-label">Medio de contacto</label>
                        <select id="medioContacto" class="form-select" required>
                            <option value="">Seleccione una opción</option>
                            <option value="1">Internet</option>
                            <option value="2">Radio</option>
                            <option value="3">Promotor</option>
                            <option value="4">Folleto</option>
                            <option value="5">Amigos</option>
                            <option value="6">Periódico</option>
                            <option value="7">Empresa</option>
                        </select>
                    </div>
                    <div class="col-md-4">
                        <label for="promotor" class="form-label">Promotor</label>
                        <select id="promotor" class="form-select">
                            <option value="">Seleccione</option>
                        </select>
                    </div>
                    <div class="col-md-4">
                        <label for="telefono" class="form-label">Teléfono del contacto</label>
                        <input type="text" id="telefono" class="form-control" placeholder="Ej. 55 1234 5678">
                    </div>
                </div>
            </div>

            <div class="tab-pane fade" id="tab-identificacion" role="tabpanel">
                <p class="text-muted">Aquí van los datos de identificación...</p>
            </div>

            <div class="tab-pane fade" id="tab-laboral" role="tabpanel">
                <p class="text-muted">Aquí van los datos laborales...</p>
            </div>

            <div class="tab-pane fade" id="tab-operacion" role="tabpanel">
                <p class="text-muted">Aquí va la solicitud de operación...</p>
            </div>

            <div class="tab-pane fade" id="tab-contacto2" role="tabpanel">
                <p class="text-muted">Aquí van los datos de contacto personales...</p>
            </div>

            <div class="tab-pane fade" id="tab-pld" role="tabpanel">
                <p class="text-muted">Aquí van los datos de prevención de lavado de dinero...</p>
            </div>
        </div>

        <div class="mt-4">
            <button type="button" id="btnGuardar" class="btn btn-success">
                <i class="fas fa-save"></i> Guardar
            </button>
        </div>

    </div>

    <script>
document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("btnGuardar").addEventListener("click", function () {
        const medioContacto = document.getElementById("medioContacto").value;
        const promotor = document.getElementById("promotor").value;
        const telefono = document.getElementById("telefono").value;

        if (!medioContacto || !telefono) {
            Swal.fire("Campos requeridos", "Debes llenar al menos el medio de contacto y teléfono", "warning");
            return;
        }

        const datos = {
            medioContacto,
            promotor,
            telefono
        };

        fetch("handlers/presolicitudHandler.ashx?action=guardar", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(datos)
        })
        .then(response => response.json())
        .then(res => {
            if (res.status === "ok") {
                Swal.fire("¡Guardado!", "La presolicitud fue registrada correctamente.", "success");
            } else {
                Swal.fire("Error", res.mensaje || "No se pudo guardar", "error");
            }
        })
        .catch(err => {
            console.error("Error al guardar:", err);
            Swal.fire("Error", "Fallo la comunicación con el servidor", "error");
        });
    });
});
</script>


</asp:Content>
