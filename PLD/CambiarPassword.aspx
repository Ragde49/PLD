<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="CambiarPassword.aspx.vb" Inherits="PLD.CambiarPassword" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <title>Cambiar contrasena | Sistema PLD</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="shortcut icon" href="/assets/images/favicon.ico" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" />
    <link href="/assets/css/app.min.css" rel="stylesheet" type="text/css" />
    <style>
        body {
            min-height: 100vh;
            background: #eef5f0;
        }
        .password-shell {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 24px;
        }
        .password-card {
            width: 100%;
            max-width: 460px;
            border: 1px solid #d8e8dc;
            border-radius: 8px;
            background: #fff;
            box-shadow: 0 12px 34px rgba(21, 35, 29, .12);
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="password-shell">
            <div class="password-card">
                <div class="p-4 p-md-5">
                    <h1 class="h4 mb-1">Cambiar contrasena</h1>
                    <p class="text-muted mb-4">Actualiza tu contrasena para continuar.</p>

                    <asp:Literal ID="litMensaje" runat="server" />

                    <div class="mb-3">
                        <label for="<%= txtActual.ClientID %>" class="form-label">Contrasena actual</label>
                        <asp:TextBox ID="txtActual" runat="server" CssClass="form-control" TextMode="Password" autocomplete="current-password" />
                    </div>

                    <div class="mb-3">
                        <label for="<%= txtNueva.ClientID %>" class="form-label">Nueva contrasena</label>
                        <asp:TextBox ID="txtNueva" runat="server" CssClass="form-control" TextMode="Password" autocomplete="new-password" />
                    </div>

                    <div class="mb-4">
                        <label for="<%= txtConfirmar.ClientID %>" class="form-label">Confirmar nueva contrasena</label>
                        <asp:TextBox ID="txtConfirmar" runat="server" CssClass="form-control" TextMode="Password" autocomplete="new-password" />
                    </div>

                    <div class="d-flex gap-2">
                        <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-success flex-fill" Text="Guardar" OnClick="btnGuardar_Click" />
                        <a href="/Logout.aspx" class="btn btn-outline-secondary">Salir</a>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
