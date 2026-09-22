<%@ Page Title="Alta de Colonia Manual" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid py-4 mt-4">
        <h3 class="mb-4">Alta de Colonia Manual</h3>
       
        <form id="formColonia">
            <div class="row g-3 align-items-end">
                <div class="col-md-2">
                    <label class="form-label">Código Postal</label>
                    <input type="text" class="form-control" id="cp" />
                </div>
                <div class="col-md-2">
                    <button type="button" class="btn btn-primary" id="btnBuscar">Buscar</button>
                </div>
            </div>

           <div class="row g-3 mt-3">
                <div class="col-md-4">
                    <label class="form-label">Municipio</label>
                    <input type="text" class="form-control" id="municipio" readonly />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Ciudad</label>
                    <input type="text" class="form-control" id="ciudad" readonly />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Estado</label>
                    <input type="text" class="form-control" id="estado" readonly />
                </div>
            </div>


            <div class="mt-4">
                <label class="form-label">Colonias existentes</label>
                <div id="coloniasExistentes" class="border rounded p-3" style="background-color: #f8f9fa;"></div>
            </div>

           <hr>
            <h5 class="mt-4">Nueva Colonia</h5>
            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">Nombre de la Colonia</label>
                    <input type="text" class="form-control" id="asentamiento" />
                </div>
            </div>

            <div class="mt-4">
                <button type="button" class="btn btn-success" id="btnGuardar">Guardar</button>
                <button type="button" class="btn btn-secondary" id="btnCancelar">Cancelar</button>
            </div>
        </form>
    </div>

<script>
    document.addEventListener("DOMContentLoaded", function () {
        document.getElementById("btnBuscar").addEventListener("click", function () {
            const cp = document.getElementById("cp").value.trim();
            if (cp === "") {
                Swal.fire("Aviso", "Ingresa un código postal", "info");
                return;
            }

            fetch("/handlers/handler_colonia.ashx", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ accion: "buscar_cp", cp: cp })
            })
            .then(response => response.json())
            .then(res => {
                console.log("✅ JSON parseado:", res);

                if (res.ok) {
                    const data = res.data;
                    document.getElementById("estado").value = data.estado;
                    document.getElementById("municipio").value = data.municipio;
                    document.getElementById("ciudad").value = data.ciudad;

                    const colonias = data.colonias.map(c => `<li>${c}</li>`).join("");
                    document.getElementById("coloniasExistentes").innerHTML = `<ul>${colonias}</ul>`;
                } else {
                    Swal.fire("Error", res.mensaje || "Error al buscar código postal", "error");
                }
            })
            .catch(err => {
                console.error("🧨 ERROR DE FETCH:", err);
                Swal.fire("Error", "Error de red o del servidor.", "error");
            });
        });

        document.getElementById("btnGuardar").addEventListener("click", function () {
            const datos = {
                accion: "guardar",
                cp: document.getElementById("cp").value,
                asentamiento: document.getElementById("asentamiento").value,
                tipo_asentamiento: "",
                municipio: document.getElementById("municipio").value,
                estado: document.getElementById("estado").value,
                ciudad: document.getElementById("ciudad").value,
                zona: "",
                clave_estado: "",
                clave_municipio: ""
            };

            fetch("/handlers/handler_colonia.ashx", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(datos)
            })
            .then(response => response.json())
            .then(res => {
                console.log("✅ JSON parseado:", res);

                Swal.fire(
                    res.ok ? "Correcto" : "Error",
                    res.mensaje || "Respuesta inválida",
                    res.ok ? "success" : "error"
                );

                if (res.ok) {
                    const form = document.getElementById("formColonia");
                    if (form) {
                        form.reset();
                        form.querySelector("#cp").focus(); // Regresa foco al CP
                    }

                    const contenedor = document.getElementById("coloniasExistentes");
                    if (contenedor) contenedor.innerHTML = "";

                    ["estado", "municipio", "ciudad"].forEach(id => {
                        const campo = document.getElementById(id);
                        if (campo) campo.value = "";
                    });
                }
            })
            .catch(err => {
                console.error("🧨 ERROR EN FETCH:", err);
                Swal.fire("Error", "Error de red o del servidor.", "error");
            });
        });

        document.getElementById("btnCancelar").addEventListener("click", function () {
            const form = document.getElementById("formColonia");
            if (form) form.reset();

            const contenedor = document.getElementById("coloniasExistentes");
            if (contenedor) contenedor.innerHTML = "";

            ["estado", "municipio", "ciudad"].forEach(id => {
                const campo = document.getElementById(id);
                if (campo) campo.value = "";
            });
        });
    });
</script>



</asp:Content>
