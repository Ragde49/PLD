# Reglas del agente para PLD

Cuando se cree una pagina, modulo o handler nuevo en el sistema PLD:

- Registrar la pagina en `seguridad_paginas` con `es_handler = 0`.
- Si debe aparecer en el menu, registrarla tambien en `seguridad_menu`.
- Registrar cada handler en `seguridad_paginas` con `es_handler = 1`.
- Ligar cada handler a su pagina en `seguridad_pagina_handler`.
- La pantalla de permisos por rol debe mostrar solo el arbol del menu y un permiso de acceso por pagina.
- No mostrar ni guardar permisos visibles de alta, cambio, baja, crear, editar o eliminar en la pantalla de permisos.
- Los handlers nunca deben mostrarse como filas ni como checkboxes editables en permisos.
- Un handler sin relacion activa en `seguridad_pagina_handler` debe quedar bloqueado por defecto.
- Despues de agregar paginas o handlers, compilar `PLD.sln` y actualizar los scripts SQL incrementales correspondientes.
