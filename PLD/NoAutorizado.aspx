<%@ Page Language="VB" AutoEventWireup="false" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <title>No autorizado | Sistema PLD</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="shortcut icon" href="/assets/images/favicon.ico" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" />
    <link href="/assets/css/app.min.css" rel="stylesheet" type="text/css" />
    <style>
        body {
            min-height: 100vh;
            background: #eef5f0;
        }
        .status-shell {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 24px;
        }
        .status-card {
            max-width: 520px;
            border: 1px solid #d8e8dc;
            border-radius: 8px;
            background: #fff;
            box-shadow: 0 12px 34px rgba(21, 35, 29, .12);
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="status-shell">
            <div class="status-card p-4 p-md-5 text-center">
                <h1 class="h4 mb-3">No autorizado</h1>
                <p class="text-muted mb-4">Tu rol no tiene permiso para abrir esta pagina o recurso.</p>
                <div class="d-flex justify-content-center gap-2">
                    <a href="/Default.aspx" class="btn btn-success">Ir al inicio</a>
                    <a href="/Logout.aspx" class="btn btn-outline-secondary">Salir</a>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
