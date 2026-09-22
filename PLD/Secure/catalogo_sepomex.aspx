<%@ Page Title="Importar SEPOMEX" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid py-4 mt-4">
    <div class="card border-primary">
        <div class="card-header bg-primary text-white">
            Importar Catálogo SEPOMEX
        </div>
        <div class="card-body">

            <p>Antes de importar, descarga la plantilla oficial:</p>
            <a href="/documentos/CPdescargaxls.zip" class="btn btn-link" download>📥 Descargar plantilla SEPOMEX</a>

            <div class="mt-3 mb-3">
                <label for="fileExcel" class="form-label">Selecciona archivo Excel (.xls)</label>
                <input class="form-control" type="file" id="fileExcel" accept=".xls" />
            </div>

            <button type="button" class="btn btn-success" id="btnImportar">Importar</button>

        </div>
    </div>
    <!-- 🔄 Loading overlay -->
    <div id="overlay" style="display:none; position:fixed; top:0; left:0; width:100%; height:100%; background:rgba(255,255,255,0.8); z-index:9999; justify-content:center; align-items:center;">
      <div class="spinner-border text-primary" role="status" style="width: 4rem; height: 4rem;">
        <span class="visually-hidden">Cargando...</span>
      </div>
      <div class="mt-3 fw-bold">Procesando archivo SEPOMEX, por favor espera el proceso es tardado...</div>
    </div>

</div>

<script>
document.getElementById("btnImportar").addEventListener("click", function () {
    const archivo = document.getElementById("fileExcel").files[0];
    if (!archivo) {
        Swal.fire("Aviso", "Selecciona un archivo Excel primero.", "warning");
        return;
    }

    // 🔒 Mostrar overlay
    document.getElementById("overlay").style.display = "flex";

    const formData = new FormData();
    formData.append("archivo", archivo);
    formData.append("accion", "importar");

    fetch("/handlers/importar_sepomex.ashx", {
        method: "POST",
        body: formData
    })
    .then(r => r.json())
    .then(res => {
        // 🔓 Ocultar overlay
        document.getElementById("overlay").style.display = "none";

        if (res.ok) {
            Swal.fire("Éxito", res.mensaje, "success");
        } else {
            Swal.fire("Error", res.mensaje, "error");
        }
    })
    .catch(error => {
        document.getElementById("overlay").style.display = "none";
        Swal.fire("Error", "Error de red o del servidor.", "error");
    });
});
</script>


</asp:Content>
