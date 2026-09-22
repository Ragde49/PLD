<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" Inherits="System.Web.UI.Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">

  <h4 class="mb-4"><i class="fas fa-sliders-h"></i> Catálogo Scoring</h4>

  <!-- 🔹 1. Nivel de Atraso -->
  <div class="card border-primary mb-4">
    <div class="card-header bg-primary text-white">Parámetros: Nivel de Atraso</div>
    <div class="card-body p-3">
      <table class="table table-bordered table-hover mb-0" id="tablaAtraso">
        <thead class="table-light text-center">
          <tr><th>Rango</th><th>Valor</th></tr>
        </thead>
        <tbody id="tbodyAtraso"></tbody>
      </table>
    </div>
  </div>

  <!-- 🔹 2. Capacidad de Pago -->
  <div class="card border-success mb-4">
    <div class="card-header bg-success text-white">Parámetros: Capacidad de Pago</div>
    <div class="card-body p-3">
      <table class="table table-bordered table-hover mb-0" id="tablaPago">
        <thead class="table-light text-center">
          <tr><th>Rango</th><th>Valor</th></tr>
        </thead>
        <tbody id="tbodyPago"></tbody>
      </table>
    </div>
  </div>

  <!-- 🔹 3. Círculo de Crédito -->
  <div class="card border-warning mb-4">
    <div class="card-header bg-warning text-dark">Parámetros: Círculo de Crédito</div>
    <div class="card-body p-3">
      <table class="table table-bordered table-hover mb-0" id="tablaCirculo">
        <thead class="table-light text-center">
          <tr>
            <th>Valor Inicial</th><th>Valor Final</th><th>Valor CC</th><th>Score Máx</th>
          </tr>
        </thead>
        <tbody id="tbodyCirculo"></tbody>
      </table>
    </div>
  </div>

  <!-- 🔹 4. Porcentaje del Monto -->
  <div class="card border-info mb-4">
    <div class="card-header bg-info text-white">Parámetros: Porcentaje del Monto</div>
    <div class="card-body p-3">
      <table class="table table-bordered table-hover mb-0" id="tablaMonto">
        <thead class="table-light text-center">
          <tr><th>Rango</th><th>Multiplicador</th></tr>
        </thead>
        <tbody id="tbodyMonto"></tbody>
      </table>
    </div>
  </div>

  <!-- 🔹 5. Producto de Crédito -->
  <div class="card border-danger mb-4">
    <div class="card-header bg-danger text-white">Parámetros: Producto de Crédito</div>
    <div class="card-body p-3">
      <table class="table table-bordered table-hover mb-0" id="tablaProducto">
        <thead class="table-light text-center">
          <tr>
            <th>Producto</th><th>Opción</th><th>Antigüedad (meses)</th><th>Sueldo X</th>
            <th>Monto Mín</th><th>Monto Máx</th>
          </tr>
        </thead>
        <tbody id="tbodyProducto"></tbody>
      </table>
    </div>
  </div>

</div>

<script>
document.addEventListener("DOMContentLoaded", function () {
  // carga las secciones con el nuevo formato
  cargarSimple("atraso", "tablaAtraso", "tbodyAtraso");
  cargarSimple("pago", "tablaPago", "tbodyPago");
  cargarSimple("monto", "tablaMonto", "tbodyMonto");
  cargarComplejo("circulo", "tablaCirculo", "tbodyCirculo");
  cargarComplejo("producto", "tablaProducto", "tbodyProducto");

  function cargarSimple(tipo, tablaID, tbodyID) {
    fetch(`/handlers/scoring_${tipo}.ashx?op=consulta`)
      .then(r => r.json())
      .then(data => {
        const tbody = document.getElementById(tbodyID);
        tbody.innerHTML = "";
        data.forEach(row => {
          const tr = document.createElement("tr");
          tr.innerHTML = `
            <td class="text-center align-middle">${row.etiqueta}</td>
            <td><input type="number" step="0.01" class="form-control text-center" 
                       data-id="${row.id}" data-campo="valor" data-tipo="${tipo}" value="${row.valor}"></td>
          `;
          tbody.appendChild(tr);
        });
        activarEventos(tablaID);
      });
  }

  function cargarComplejo(tipo, tablaID, tbodyID) {
    fetch(`/handlers/scoring_${tipo}.ashx?op=consulta`)
      .then(r => r.json())
      .then(data => {
        const tbody = document.getElementById(tbodyID);
        tbody.innerHTML = "";
        data.forEach(row => {
          const campos = Object.keys(row).filter(k => k !== "id");
          const tr = document.createElement("tr");
          tr.innerHTML = campos.map(c => `
            <td><input type="number" step="0.01" class="form-control text-center" 
                       data-id="${row.id}" data-campo="${c}" data-tipo="${tipo}" value="${row[c]}"></td>
          `).join("");
          tbody.appendChild(tr);
        });
        activarEventos(tablaID);
      });
  }

  function activarEventos(tablaID) {
    document.querySelectorAll(`#${tablaID} input`).forEach(input => {
      input.addEventListener("blur", function () {
        const { id, campo, tipo } = this.dataset;
        const valor = this.value;

        fetch(`/handlers/scoring_${tipo}.ashx`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ op: "guardar", id, campo, valor })
        })
        .then(r => r.json())
        .then(resp => {
            if (!resp.ok) {
                console.error("Error al guardar:", resp.msg);
                // Puedes agregar un fallback discreto si deseas más adelante
            }
        });
      });
    });
  }
});
</script>
</asp:Content>
