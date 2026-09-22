<%@ Page Language="vb" AutoEventWireup="false" %>

<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <title>Matriz de riesgo</title>

    <!-- Bootstrap -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <!-- SweetAlert -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <!-- JQuery -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</head>
<body>
    <div class="container-fluid mt-4">
        <h4 class="mb-4">Configuración matriz de riesgo por tipo de persona</h4>

        <div class="row">
            <div class="col-md-6">
                <h5>Persona Física</h5>
                <table class="table table-sm table-bordered table-striped" id="tablaPF">
                    <thead class="table-primary">
                        <tr>
                            <th>Nivel Riesgo</th>
                            <th>De</th>
                            <th>A</th>
                        </tr>
                    </thead>
                    <tbody></tbody>
                </table>
            </div>

            <div class="col-md-6">
                <h5>Persona Moral</h5>
                <table class="table table-sm table-bordered table-striped" id="tablaPM">
                    <thead class="table-primary">
                        <tr>
                            <th>Nivel Riesgo</th>
                            <th>De</th>
                            <th>A</th>
                        </tr>
                    </thead>
                    <tbody></tbody>
                </table>
            </div>
        </div>
    </div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            cargarRangos();
        });

        function cargarRangos() {
            fetch("/handlers/handler_puntaje_categoria.ashx?accion=consultar")
                .then(resp => resp.json())
                .then(data => {
                    const pfBody = document.querySelector("#tablaPF tbody");
                    const pmBody = document.querySelector("#tablaPM tbody");
                    pfBody.innerHTML = "";
                    pmBody.innerHTML = "";

                    const orden = { "BAJO": 1, "MEDIO": 2, "ALTO": 3 };

                    // PF
                    data.filter(r => r.tipo_persona === "PF")
                        .sort((a, b) => orden[a.nivel_riesgo] - orden[b.nivel_riesgo])
                        .forEach(r => {
                            const tr = document.createElement("tr");
                            tr.innerHTML = `
                                <td>${r.nivel_riesgo}</td>
                                <td><input type="number" step="0.01" class="form-control form-control-sm" value="${r.valor_minimo}" data-id="${r.id}" data-campo="valor_minimo"></td>
                                <td><input type="number" step="0.01" class="form-control form-control-sm" value="${r.valor_maximo}" data-id="${r.id}" data-campo="valor_maximo"></td>
                            `;
                            pfBody.appendChild(tr);
                        });

                    // PM
                    data.filter(r => r.tipo_persona === "PM")
                        .sort((a, b) => orden[a.nivel_riesgo] - orden[b.nivel_riesgo])
                        .forEach(r => {
                            const tr = document.createElement("tr");
                            tr.innerHTML = `
                                <td>${r.nivel_riesgo}</td>
                                <td><input type="number" step="0.01" class="form-control form-control-sm" value="${r.valor_minimo}" data-id="${r.id}" data-campo="valor_minimo"></td>
                                <td><input type="number" step="0.01" class="form-control form-control-sm" value="${r.valor_maximo}" data-id="${r.id}" data-campo="valor_maximo"></td>
                            `;
                            pmBody.appendChild(tr);
                        });

                    agregarEventosInputs();
                });
        }

        function agregarEventosInputs() {
            const inputs = document.querySelectorAll("input[data-id]");

            inputs.forEach(input => {
                input.addEventListener("change", function () {
                    const id = this.getAttribute("data-id");
                    const campo = this.getAttribute("data-campo");
                    const valor = parseFloat(this.value);
                    this.disabled = true;

                    const fila = this.closest("tr");
                    const nuevoMin = fila.querySelector('input[data-campo="valor_minimo"]').value;
                    const nuevoMax = fila.querySelector('input[data-campo="valor_maximo"]').value;

                    const data = {
                        accion: "actualizar",
                        id: id,
                        valor_minimo: nuevoMin,
                        valor_maximo: nuevoMax
                    };

                    fetch("/handlers/handler_puntaje_categoria.ashx", {
                        method: "POST",
                        headers: { "Content-Type": "application/x-www-form-urlencoded" },
                        body: new URLSearchParams(data)
                    })
                    .then(resp => resp.json())
                    .then(() => {
                        Swal.fire({
                            toast: true,
                            position: "top-end",
                            icon: "success",
                            title: "Guardado",
                            showConfirmButton: false,
                            timer: 1000
                        });
                        input.disabled = false;
                    })
                    .catch(() => {
                        Swal.fire("Error", "No se pudo guardar el cambio", "error");
                        input.disabled = false;
                    });
                });
            });
        }
    </script>
</body>
</html>
