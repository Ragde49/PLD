<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="clasificacion_riesgo.aspx.vb" Inherits="PLD.clasificacion_riesgo" %>

<head runat="server">
    <meta charset="utf-8" />
    <title>Clasificación de Riesgo</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <link href="https://cdn.datatables.net/1.13.6/css/dataTables.bootstrap5.min.css" rel="stylesheet">
    <link href="https://cdn.jsdelivr.net/npm/daterangepicker/daterangepicker.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</head>


<body class="container-fluid mt-4">
<div class="container-fluid mt-4">
    <h4 class="mb-3">Clasificación de Riesgo PLD</h4>

    <div class="row">
        <!-- Matriz General -->
        <div class="col-md-6">
            <div class="card shadow-sm border-danger mb-3">
                <div class="card-header bg-danger text-white py-2">
                    <small class="fw-bold">Matriz General</small>
                </div>
                <div class="card-body p-2">
                    <table class="table table-sm table-bordered small text-center mb-0" id="tablaGeneral">
                        <thead class="table-light"><tr><th>Categoria</th><th>ALTO</th><th>MEDIO</th><th>BAJO</th></tr></thead>
                        <tbody></tbody>
                        <tfoot class="table-light"><tr><th class="text-end">TOTAL</th><th id="totalGeneralAlto">0.00</th><th id="totalGeneralMedio">0.00</th><th id="totalGeneralBajo">0.00</th></tr></tfoot>
                    </table>
                </div>
            </div>
        </div>

        <!-- Riesgo PF -->
        <div class="col-md-3">
            <div class="card shadow-sm border-warning mb-3">
                <div class="card-header bg-warning py-2"><small class="fw-bold">Riesgo Persona Física</small></div>
                <div class="card-body p-2">
                    <table class="table table-sm table-bordered small text-center mb-0" id="tablaRiesgoPF">
                        <thead class="table-light"><tr><th>Nivel</th><th>De</th><th>A</th></tr></thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>

        <!-- Riesgo PM -->
        <div class="col-md-3">
            <div class="card shadow-sm border-warning mb-3">
                <div class="card-header bg-warning py-2"><small class="fw-bold">Riesgo Persona Moral</small></div>
                <div class="card-body p-2">
                    <table class="table table-sm table-bordered small text-center mb-0" id="tablaRiesgoPM">
                        <thead class="table-light"><tr><th>Nivel</th><th>De</th><th>A</th></tr></thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <div class="row">
        <!-- PF -->
        <div class="col-md-6">
            <div class="card shadow-sm border-success mb-3">
                <div class="card-header bg-success text-white py-2"><small class="fw-bold">Cliente Persona Física</small></div>
                <div class="card-body p-2">
                    <table class="table table-sm table-bordered small text-center mb-0" id="tablaPF">
                        <thead class="table-light"><tr><th>Categoría</th><th>ALTO</th><th>MEDIO</th><th>BAJO</th></tr></thead>
                        <tbody></tbody>
                        <tfoot class="table-light"><tr><th class="text-end">TOTAL</th><th id="totalPFAlto">0.00</th><th id="totalPFMedio">0.00</th><th id="totalPFBajo">0.00</th></tr></tfoot>
                    </table>
                </div>
            </div>
        </div>

        <!-- PM -->
        <div class="col-md-6">
            <div class="card shadow-sm border-danger mb-3">
                <div class="card-header bg-danger text-white py-2"><small class="fw-bold">Cliente Persona Moral</small></div>
                <div class="card-body p-2">
                    <table class="table table-sm table-bordered small text-center mb-0" id="tablaPM">
                        <thead class="table-light"><tr><th>Categoría</th><th>ALTO</th><th>MEDIO</th><th>BAJO</th></tr></thead>
                        <tbody></tbody>
                        <tfoot class="table-light"><tr><th class="text-end">TOTAL</th><th id="totalPMAlto">0.00</th><th id="totalPMMedio">0.00</th><th id="totalPMBajo">0.00</th></tr></tfoot>
                    </table>
                </div>
            </div>
        </div>

        <!-- Productos -->
        <div class="col-md-6">
            <div class="card shadow-sm border-info mb-3">
                <div class="card-header bg-info text-white py-2"><small class="fw-bold">Productos</small></div>
                <div class="card-body p-2">
                    <table class="table table-sm table-bordered small text-center mb-0" id="tablaProducto">
                        <thead class="table-light"><tr><th>Categoría</th><th>ALTO</th><th>MEDIO</th><th>BAJO</th></tr></thead>
                        <tbody></tbody>
                        <tfoot class="table-light"><tr><th class="text-end">TOTAL</th><th id="totalProductoAlto">0.00</th><th id="totalProductoMedio">0.00</th><th id="totalProductoBajo">0.00</th></tr></tfoot>
                    </table>
                </div>
            </div>
        </div>

        <!-- Zona -->
        <div class="col-md-6">
            <div class="card shadow-sm border-primary mb-3">
                <div class="card-header bg-primary text-white py-2"><small class="fw-bold">Zona Geográfica</small></div>
                <div class="card-body p-2">
                    <table class="table table-sm table-bordered small text-center mb-0" id="tablaZona">
                        <thead class="table-light"><tr><th>Categoría</th><th>ALTO</th><th>MEDIO</th><th>BAJO</th></tr></thead>
                        <tbody></tbody>
                        <tfoot class="table-light"><tr><th class="text-end">TOTAL</th><th id="totalZonaAlto">0.00</th><th id="totalZonaMedio">0.00</th><th id="totalZonaBajo">0.00</th></tr></tfoot>
                    </table>
                </div>
            </div>
        </div>

        <!-- Transacciones -->
        <div class="col-md-6">
            <div class="card shadow-sm border-warning mb-3">
                <div class="card-header bg-warning py-2"><small class="fw-bold">Transacciones y Canales</small></div>
                <div class="card-body p-2">
                    <table class="table table-sm table-bordered small text-center mb-0" id="tablaTransacciones">
                        <thead class="table-light"><tr><th>Categoría</th><th>ALTO</th><th>MEDIO</th><th>BAJO</th></tr></thead>
                        <tbody></tbody>
                        <tfoot class="table-light"><tr><th class="text-end">TOTAL</th><th id="totalTransAlto">0.00</th><th id="totalTransMedio">0.00</th><th id="totalTransBajo">0.00</th></tr></tfoot>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>

 <!-- Librerías necesarias -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.6/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.6/js/dataTables.bootstrap5.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/moment@2.29.4/moment.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/daterangepicker/daterangepicker.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>


<script>
document.addEventListener("DOMContentLoaded", function () {
    const matrices = [
        { id: "tablaPF", handler: "handler_peso_cliente_pf.ashx", prefix: "totalPF" },
        { id: "tablaPM", handler: "handler_peso_cliente_pm.ashx", prefix: "totalPM" },
        { id: "tablaProducto", handler: "handler_peso_producto.ashx", prefix: "totalProducto" },
        { id: "tablaZona", handler: "handler_peso_zona.ashx", prefix: "totalZona", zonaGeografica: true },
        { id: "tablaTransacciones", handler: "handler_peso_transacciones.ashx", prefix: "totalTrans" },
        { id: "tablaGeneral", handler: "handler_peso_general.ashx", prefix: "totalGeneral" }
    ];

    matrices.forEach(({ id, handler, prefix, zonaGeografica }) => {
        fetch("/handlers/" + handler + "?op=consultar")
            .then(r => r.json()).then(data => {
                const tbody = document.querySelector(`#${id} tbody`);
                tbody.innerHTML = "";
                let alto = 0, medio = 0, bajo = 0;

                data.data.forEach(row => {
                    alto += parseFloat(row.puntaje_alto || row.valor_alto || 0);
                    medio += parseFloat(row.puntaje_medio || row.valor_medio || 0);
                    bajo += parseFloat(row.puntaje_bajo || row.valor_bajo || 0);

                    if (zonaGeografica) {
                        const zona = row.categoria;
                        tbody.innerHTML += `
                            <tr>
                                <td>${zona}</td>
                                <td><input type="number" class="form-control form-control-sm text-end" data-zona="${zona}" data-nivel="ALTO" value="${row.puntaje_alto}" /></td>
                                <td><input type="number" class="form-control form-control-sm text-end" data-zona="${zona}" data-nivel="MEDIO" value="${row.puntaje_medio}" /></td>
                                <td><input type="number" class="form-control form-control-sm text-end" data-zona="${zona}" data-nivel="BAJO" value="${row.puntaje_bajo}" /></td>
                            </tr>`;
                    } else {
                        tbody.innerHTML += `
                            <tr>
                                <td>${row.categoria}</td>
                                <td><input type="number" class="form-control form-control-sm text-end" data-categoria="${row.categoria}" data-campo="puntaje_alto" value="${row.puntaje_alto}" /></td>
                                <td><input type="number" class="form-control form-control-sm text-end" data-categoria="${row.categoria}" data-campo="puntaje_medio" value="${row.puntaje_medio}" /></td>
                                <td><input type="number" class="form-control form-control-sm text-end" data-categoria="${row.categoria}" data-campo="puntaje_bajo" value="${row.puntaje_bajo}" /></td>
                            </tr>`;
                    }
                });

                document.getElementById(prefix + "Alto").textContent = alto.toFixed(2);
                document.getElementById(prefix + "Medio").textContent = medio.toFixed(2);
                document.getElementById(prefix + "Bajo").textContent = bajo.toFixed(2);

                // Eventos para guardar cambios
                document.querySelectorAll(`#${id} input`).forEach(input => {
                    input.addEventListener("change", function () {
                        this.disabled = true;
                        let payload = `op=actualizar&valor=${this.value}`;
                        if (zonaGeografica) {
                            payload += `&zona=${this.dataset.zona}&nivel=${this.dataset.nivel}`;
                        } else {
                            payload += `&categoria=${this.dataset.categoria}&campo=${this.dataset.campo}`;
                        }

                        fetch("/handlers/" + handler, {
                            method: "POST",
                            headers: { "Content-Type": "application/x-www-form-urlencoded" },
                            body: payload
                        }).then(r => r.json()).then(res => {
                            this.disabled = false;
                            if (res.success) {
                                Swal.fire({
                                    toast: true,
                                    position: "top-end",
                                    icon: "success",
                                    title: "Guardado",
                                    showConfirmButton: false,
                                    timer: 1000
                                });
                            }
                        });
                    });
                });
            });
    });

    // Riesgo PF / PM solo lectura
    fetch("/handlers/handler_puntaje_categoria.ashx?accion=consultar")
        .then(r => r.json())
        .then(data => {
            const pf = data.filter(r => r.tipo_persona === "PF");
            const pm = data.filter(r => r.tipo_persona === "PM");
            renderRiesgo("tablaRiesgoPF", pf);
            renderRiesgo("tablaRiesgoPM", pm);
        });

    function renderRiesgo(id, data) {
        const tbody = document.querySelector(`#${id} tbody`);
        tbody.innerHTML = "";
        const orden = { "BAJO": 1, "MEDIO": 2, "ALTO": 3 };
        data.sort((a, b) => orden[a.nivel_riesgo] - orden[b.nivel_riesgo]);

        data.forEach(r => {
            tbody.innerHTML += `
                <tr>
                    <td>${r.nivel_riesgo}</td>
                    <td>${parseFloat(r.valor_minimo).toFixed(2)}</td>
                    <td>${parseFloat(r.valor_maximo).toFixed(2)}</td>
                </tr>`;
        });
    }
});
</script>


</body>