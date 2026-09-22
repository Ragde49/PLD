<%@ Page Title="Consulta PF" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="solicitud_pf.aspx.vb" Inherits="PLD.solicitud_pf" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container-fluid mt-5 pt-3">
        <div class="card shadow-sm">
            <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                <h5 class="mb-0"><i class="fas fa-users"></i> Solicitudes Persona Física</h5>

                <a href="captura_solicitud_credito.aspx" class="btn btn-success btn-sm" type="button">
                    <i class="fas fa-user-plus"></i> Capturar solicitud
                </a>
            </div>

            <div class="card-body">

                <div class="table-responsive">
                    <table id="tablaSolicitudes" class="table table-striped table-bordered table-hover table-sm w-100">
                        <thead class="table-dark text-center">
                            <tr>
                                <th>ID Solicitud</th>
                                <th>Cliente</th>
                                <th>Producto financiero</th>
                                <th>Monto solicitado</th>
                                <th>Estatus</th>
                                <th>Fecha captura</th>
                                <th>Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- Se llena dinámicamente por JS -->
                        </tbody>
                    </table>
                </div>

            </div>
        </div>
    </div>

    <script>
        const H_SOL = '/handlers/solicitudPFHandler.ashx';

        function fmtFecha(dt) {
            try {
                const d = new Date(dt);
                return isNaN(d.getTime()) ? '—' : d.toLocaleString();
            } catch {
                return '—';
            }
        }

        document.addEventListener("DOMContentLoaded", function () {
            cargarSolicitudes();
        });

        function cargarSolicitudes() {
            fetch(H_SOL + '?action=listar')
                .then(r => r.json())
                .then(j => {
                    if (!j.ok) {
                        console.error("Error al cargar solicitudes:", j.error);
                        return;
                    }

                    const tbody = document.querySelector("#tablaSolicitudes tbody");
                    tbody.innerHTML = "";

                    j.data.forEach(reg => {

                        const monto = parseFloat(reg.monto_solicitado || 0).toLocaleString('es-MX', {
                            style: 'currency',
                            currency: 'MXN'
                        });

                        const row = document.createElement("tr");

                        row.innerHTML = `
                            <td class="text-center">${reg.id}</td>
                            <td>${reg.cliente || '—'}</td>
                            <td>${reg.producto || '—'}</td>
                            <td class="text-end">${monto}</td>
                            <td class="text-center">
                                <span class="badge ${reg.estatus === 'FINALIZADA' ? 'bg-success' : 'bg-warning text-dark'}">
                                    ${reg.estatus}
                                </span>
                            </td>
                            <td class="text-center">${fmtFecha(reg.fecha_creacion)}</td>

                            <td class="text-center">
                                <a href="captura_solicitud_credito.aspx?id=${reg.id}"
                                   class="btn btn-primary btn-sm" type="button">
                                    <i class="fas fa-eye"></i> Continuar
                                </a>
                            </td>
                        `;

                        tbody.appendChild(row);
                    });
                })
                .catch(err => console.error("Error fetch:", err));
        }
    </script>

</asp:Content>
