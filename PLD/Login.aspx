<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Login.aspx.vb" Inherits="PLD.Login" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <title>Acceso | Sistema PLD</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="shortcut icon" href="/assets/images/favicon.ico" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" />
    <link href="/assets/css/app.min.css" rel="stylesheet" type="text/css" />
    <style>
        body {
            min-height: 100vh;
            background: #eef5f0;
        }
        .login-shell {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 24px;
        }
        .login-card {
            width: 100%;
            max-width: 420px;
            border: 1px solid #d8e8dc;
            border-radius: 8px;
            background: #fff;
            box-shadow: 0 12px 34px rgba(21, 35, 29, .12);
        }
        .brand-logo {
            max-height: 54px;
            width: auto;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-shell">
            <div class="login-card">
                <div class="p-4 p-md-5">
                    <div class="text-center mb-4">
                        <img src="/assets/images/logo-dark.png" alt="Sistema PLD" class="brand-logo mb-3" />
                        <h1 class="h4 mb-1">Sistema PLD</h1>
                        <p class="text-muted mb-0">Ingresa con tu usuario y contrasena.</p>
                    </div>

                    <asp:Literal ID="litMensaje" runat="server" />

                    <div class="mb-3">
                        <label for="<%= txtUsuario.ClientID %>" class="form-label">Usuario</label>
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" MaxLength="80" autocomplete="username" />
                    </div>

                    <div class="mb-3">
                        <label for="<%= txtPassword.ClientID %>" class="form-label">Contrasena</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" autocomplete="current-password" />
                    </div>

                    <div class="d-flex align-items-center justify-content-between mb-4">
                        <div class="form-check">
                            <asp:CheckBox ID="chkRecordar" runat="server" CssClass="form-check-input" />
                            <label class="form-check-label" for="<%= chkRecordar.ClientID %>">Recordarme</label>
                        </div>
                    </div>

                    <asp:Button ID="btnIngresar" runat="server" CssClass="btn btn-success w-100" Text="Ingresar" OnClick="btnIngresar_Click" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>
