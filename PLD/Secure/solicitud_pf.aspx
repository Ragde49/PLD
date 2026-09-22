<%@ Page Title="Consulta PF" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="solicitud_pf.aspx.vb" Inherits="PLD.solicitud_pf" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
<div class="card shadow-sm">
<div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
<h5 class="mb-0"><i class="fas fa-users"></i> Solicitudes Persona Física</h5>
<a href="captura_solicitud_credito.aspx" class="btn btn-success btn-sm"><i class="fas fa-user-plus"></i> Capturar solicitud</a>
</div>
<div class="card-body"><div class="table-responsive">
<table id="tablaSolicitudes" class="table table-striped table-bordered table-hover table-sm w-100">
<thead class="table-dark text-center"><tr>
<th>ID Solicitud</th><th>Cliente</th><th>Producto financiero</th><th>Tipo crédito</th><th>Monto solicitado</th><th>Disponible</th><th>Estatus</th><th>Fecha captura</th><th>Acciones</th>
</tr></thead><tbody></tbody></table>
</div></div></div></div>
<script>
document.addEventListener("DOMContentLoaded", function () {
 const H_SOL="/handlers/solicitudPFHandler.ashx";
 const esc=v=>String(v??"").replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;");
 const money=v=>(v===null||v===undefined||v==="")?"—":Number(v||0).toLocaleString("es-MX",{style:"currency",currency:"MXN"});
 const date=v=>{try{const d=new Date(v);return isNaN(d.getTime())?"—":d.toLocaleString("es-MX");}catch(e){return "—";}};
 async function cargar(){
  try{
   const r=await fetch(H_SOL+"?action=listar",{cache:"no-store"}); const j=await r.json();
   if(!r.ok||!j.ok) throw new Error(j.error||"No fue posible cargar las solicitudes.");
   const tb=document.querySelector("#tablaSolicitudes tbody"); tb.innerHTML="";
   (j.data||[]).forEach(reg=>{
    const rev=reg.es_revolvente===true||reg.es_revolvente===1||reg.es_revolvente==="1";
    let acciones='<a href="captura_solicitud_credito.aspx?id='+encodeURIComponent(reg.id)+'" class="btn btn-primary btn-sm me-1"><i class="fas fa-eye"></i> Continuar</a>';
    if(rev) acciones+='<a href="credito_revolvente.aspx?id='+encodeURIComponent(reg.id)+'" class="btn btn-outline-primary btn-sm"><i class="fas fa-sync-alt"></i> Revolvente</a>';
    const tr=document.createElement("tr");
    tr.innerHTML='<td class="text-center">'+esc(reg.id)+'</td><td>'+esc(reg.cliente||"—")+'</td><td>'+esc(reg.producto||"—")+'</td><td>'+(rev?'<span class="badge bg-primary">'+esc(reg.tipo_credito||"Revolvente")+'</span>':esc(reg.tipo_credito||"—"))+'</td><td class="text-end">'+esc(money(reg.monto_solicitado))+'</td><td class="text-end">'+(rev?esc(money(reg.disponible)):"—")+'</td><td class="text-center"><span class="badge '+(reg.estatus==="FINALIZADA"?"bg-success":"bg-warning text-dark")+'">'+esc(reg.estatus||"")+'</span></td><td class="text-center">'+esc(date(reg.fecha_creacion))+'</td><td class="text-center">'+acciones+'</td>';
    tb.appendChild(tr);
   });
  }catch(e){console.error(e);Swal.fire("Error",e.message,"error");}
 }
 cargar();
});
</script>
</asp:Content>