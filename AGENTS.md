# AGENTS.md — Instrucciones maestras del proyecto PLD

> **OBLIGATORIO:** este archivo debe leerse **completo antes de realizar cualquier trabajo** en este repositorio.
>
> Aplica por igual a **ChatGPT, Codex, otros agentes de IA y desarrolladores humanos**.
>
> Este es el **único archivo de instrucciones permanentes** del proyecto. No crear otro `AGENT.md`, `AGENTE.md`, `AGENTS.local.md` ni archivos equivalentes con reglas paralelas. Si una regla permanente cambia, se modifica este archivo.

---

## 0. Orden de lectura obligatorio

Antes de modificar código, SQL, configuración o documentación:

1. Leer completo este `AGENTS.md`.
2. Leer `README.md` para conocer arquitectura, módulos y puesta en marcha.
3. Si la tarea viene documentada en `cambios_codex/`, leer completo el archivo específico del cambio.
4. Revisar los archivos existentes relacionados con el cambio antes de proponer componentes nuevos.
5. Revisar `BITACORA_CAMBIOS.md` para conocer cambios recientes que puedan afectar la tarea.

No comenzar una implementación basándose únicamente en el nombre de una página, handler, tabla o requerimiento.

---

## 1. Propósito del sistema

PLD es un sistema ASP.NET Web Forms en VB.NET orientado a:

- Prevención de Lavado de Dinero y Financiamiento al Terrorismo (PLD/FT).
- Identificación y conocimiento de clientes.
- Evaluación y clasificación de riesgo PLD.
- Alertas y seguimiento operativo.
- Captura y administración de solicitudes de crédito.
- Productos financieros.
- Pagos de crédito.
- Catálogos operativos.
- Seguridad, roles y autorización.

Tecnología base actual:

- ASP.NET Web Forms.
- VB.NET.
- .NET Framework 4.5.2.
- SQL Server.
- `System.Data.SqlClient`.
- Páginas `.aspx`.
- Handlers `.ashx`.
- Forms Authentication.
- IIS Express para desarrollo.

La arquitectura existente se debe respetar antes de introducir patrones, librerías o componentes nuevos.

---

## 2. Principios no negociables

### 2.1 No inventar

No inventar:

- Tablas.
- Campos.
- Stored procedures.
- Vistas.
- Catálogos.
- Reglas regulatorias.
- Umbrales.
- Ponderaciones.
- Estados.
- APIs.
- Roles.
- Permisos.
- Configuraciones.

Primero buscar si ya existe una estructura utilizable.

### 2.2 Cambiar lo mínimo necesario

Preferir ampliar correctamente una estructura existente antes que crear una nueva.

No hacer refactors amplios, renombrados masivos ni cambios cosméticos no solicitados dentro de una tarea funcional.

### 2.3 Reglas explicables

Las reglas de PLD y crédito deben poder expresarse como:

**Variables → ponderaciones/reglas → cálculo → resultado → clasificación → acción**

Evitar cajas negras y lógica crítica escondida en código cuando pueda parametrizarse de manera auditable.

### 2.4 Trazabilidad

Toda decisión importante debe poder reconstruirse:

**Quién → Qué hizo → Cuándo → Sobre qué cliente/crédito → Valor anterior → Valor nuevo → Motivo → Evidencia**

Cuando el modelo actual no soporte toda la trazabilidad requerida, señalar la limitación; no fingir que existe.

---

## 3. Flujo obligatorio para cualquier cambio

### Antes de programar

1. Entender el requerimiento.
2. Identificar qué ya existe.
3. Localizar UI, handlers, tablas, vistas y scripts relacionados.
4. Identificar impactos en PLD, crédito, seguridad, auditoría y operación.
5. Confirmar si la tarea requiere cambio de esquema o solo código.
6. Si existe un archivo en `cambios_codex/`, respetar su alcance y criterios de aceptación.

### Durante la implementación

1. Mantener el patrón del proyecto.
2. Usar SQL parametrizado.
3. No agregar secretos.
4. No romper compatibilidad con .NET Framework 4.5.2 sin autorización expresa.
5. Mantener autorización y relaciones página-handler.
6. Si hay regla de negocio nueva, hacerla explícita y trazable.
7. Si hay SQL nuevo, crear script incremental.

### Al terminar

1. Revisar todos los archivos modificados.
2. Compilar `PLD.sln` cuando el cambio afecte código, proyecto o referencias.
3. Ejecutar las pruebas funcionales razonables del módulo afectado.
4. Validar acceso autorizado y no autorizado cuando se toque UI/handlers/seguridad.
5. **Actualizar `README.md` si el cambio modifica arquitectura, módulos, flujos, configuración, requisitos, endpoints, seguridad, reglas de negocio, dependencias o forma de operación.** No dejar el README desfasado respecto al código.
6. **Si hubo cualquier cambio de base de datos, actualizar en la misma tarea el SQL incremental y `PLD/SQL/ESTRUCTURA_BASE_DATOS.md`.**
7. Actualizar el archivo de tarea de `cambios_codex/`, si aplica.
8. Registrar el resultado en `BITACORA_CAMBIOS.md`.
9. Reportar claramente:
   - qué cambió;
   - archivos modificados;
   - SQL requerido;
   - pruebas realizadas;
   - pendientes o riesgos conocidos.

Un cambio no se considera terminado mientras falte la bitácora correspondiente.

---

## 4. Carpeta `cambios_codex/`

Esta carpeta contiene requerimientos concretos que se entregarán a Codex o a otro agente.

### Nombre recomendado

`YYYY-MM-DD_NNN_descripcion-corta.md`

Ejemplo:

`2026-09-22_001_alerta-operaciones-fraccionadas.md`

### Cada archivo de cambio debe contener

- Estado.
- Fecha.
- Objetivo.
- Contexto.
- Alcance.
- Fuera de alcance.
- Reglas de negocio confirmadas.
- Criterios de aceptación.
- Archivos/objetos conocidos relacionados.
- Impacto SQL, si se conoce.
- Pruebas esperadas.
- Resultado de implementación.
- Commit o PR, cuando exista.
- Pendientes/riesgos.

### Estados

Usar únicamente:

- `PENDIENTE`
- `EN_PROGRESO`
- `BLOQUEADO`
- `COMPLETADO`
- `CANCELADO`

El archivo de cambio describe **qué se pidió**. Las reglas permanentes de cómo trabajar viven únicamente en este `AGENTS.md`.

No borrar los archivos de cambios completados: forman parte de la trazabilidad del proyecto.

---

## 5. Bitácora de cambios

El archivo oficial es:

`BITACORA_CAMBIOS.md`

Todo cambio terminado debe agregar una entrada al inicio de la bitácora.

Cada entrada debe indicar como mínimo:

- Fecha.
- Autor/agente.
- Resumen.
- Archivos principales modificados.
- Base de datos / SQL.
- Validación realizada.
- Commit o PR, si existe.
- Riesgos, observaciones o pendientes.

La bitácora no sustituye Git. Complementa el historial con contexto funcional y operativo.

---

## 6. Arquitectura y código

### 6.1 Páginas

Las páginas protegidas se encuentran principalmente en:

`PLD/Secure/`

Antes de crear una página nueva, buscar si la función corresponde a una pantalla existente.

### 6.2 Handlers

La lógica HTTP se encuentra principalmente en:

`PLD/Handlers/`

Los handlers suelen recibir una acción mediante parámetros de request.

Al agregar acciones:

- conservar el estilo del handler existente;
- validar parámetros;
- usar consultas parametrizadas;
- devolver códigos HTTP coherentes;
- evitar devolver HTML cuando el consumidor espera JSON;
- no confiar únicamente en validación JavaScript.

### 6.3 Lógica crítica

Las validaciones críticas de PLD, crédito, seguridad o integridad deben existir del lado servidor.

JavaScript puede mejorar UX, pero no debe ser la única barrera para una regla crítica.

### 6.4 Dependencias

No actualizar framework ni paquetes históricos por iniciativa propia.

El proyecto usa .NET Framework 4.5.2 y dependencias antiguas. Un cambio de framework/paquete puede tener efectos amplios y requiere tarea explícita.

---

## 7. Seguridad: páginas, handlers y permisos

Estas reglas son obligatorias.

### Página nueva

- Registrar la página en `seguridad_paginas` con `es_handler = 0`.
- Si debe aparecer en el menú, registrarla también en `seguridad_menu`.

### Handler nuevo

- Registrar el handler en `seguridad_paginas` con `es_handler = 1`.
- Ligar el handler a su página mediante `seguridad_pagina_handler`.

### Pantalla de permisos

- Mostrar únicamente el árbol de menú y permiso de acceso por página.
- No mostrar ni guardar permisos visibles de alta, cambio, baja, crear, editar o eliminar.
- Los handlers nunca deben mostrarse como filas ni checkboxes editables.
- Un handler sin relación activa en `seguridad_pagina_handler` queda bloqueado por defecto para roles no administradores.

### Autorización

No eliminar ni puentear los controles de:

- `SeguridadAuth`
- `SeguridadAutorizacion`
- `SeguridadMenu`
- `Global.asax`

No crear endpoints alternos para evitar autorización.

### Contraseñas

No sustituir PBKDF2 por almacenamiento reversible o texto plano.

Nunca incluir contraseñas reales en documentación, código, bitácora o archivos de cambio.

---

## 8. SQL y base de datos

### 8.1 Conexión

La conexión esperada se llama:

`PLDConnection`

No hardcodear connection strings.

### 8.2 SQL parametrizado

Toda entrada proveniente de usuario/request debe pasar mediante parámetros de `SqlCommand`.

No concatenar valores externos directamente en SQL.

### 8.3 Cambios de esquema/datos

Los cambios nuevos se deben entregar mediante scripts incrementales en:

`PLD/SQL/`

Nombre recomendado:

`YYYYMMDD_NNN_descripcion.sql`

Los scripts deben ser seguros para el ambiente objetivo e, idealmente, idempotentes cuando sea razonable.

No modificar scripts históricos ya aplicados para esconder un cambio nuevo; agregar un incremental, salvo que la tarea indique expresamente otra cosa.

### 8.4 Estructura de base de datos siempre sincronizada

El archivo canónico de documentación de estructura es:

`PLD/SQL/ESTRUCTURA_BASE_DATOS.md`

**ChatGPT, Codex o cualquier agente que cambie la base de datos debe actualizar ese archivo en la misma tarea.**

Se considera cambio de estructura, entre otros:

- crear, modificar o eliminar tablas;
- crear, modificar o eliminar columnas;
- cambiar tipos de datos, nullability, defaults o identity;
- crear, modificar o eliminar llaves primarias/foráneas;
- crear, modificar o eliminar índices o constraints;
- crear, modificar o eliminar vistas;
- crear, modificar o eliminar stored procedures o funciones;
- modificar catálogos estructurales o datos semilla necesarios para que funcione el sistema;
- modificar objetos de seguridad de base de datos utilizados por la aplicación.

Para cada cambio de base de datos es obligatorio:

1. Revisar primero la estructura existente.
2. Crear el script incremental correspondiente en `PLD/SQL/`.
3. Actualizar `PLD/SQL/ESTRUCTURA_BASE_DATOS.md` con el estado resultante.
4. Actualizar `README.md` cuando el cambio afecte instalación, arquitectura, módulos, objetos centrales o forma de operación.
5. Registrar el cambio en `BITACORA_CAMBIOS.md`.
6. Registrar dependencias y orden de ejecución cuando aplique.

**Nunca dejar código que dependa de una columna, tabla, vista o procedimiento nuevo sin versionar también el cambio de base de datos.**

Si el repositorio todavía no contiene definición completa de un objeto existente, documentar únicamente lo confirmado y señalar lo desconocido; no inventar columnas ni relaciones.

### 8.5 No crear tablas por comodidad

Antes de crear una tabla:

1. buscar tablas existentes;
2. revisar si una existente puede ampliarse correctamente;
3. revisar el impacto en datos históricos;
4. documentar por qué la nueva tabla es necesaria.

---

## 9. PLD/FT

PLD y crédito son riesgos distintos.

### Riesgo PLD

Posibilidad de que cliente, recursos u operaciones estén relacionados con lavado de dinero, financiamiento al terrorismo u otras actividades ilícitas/riesgos aplicables.

### Reglas

- Una alerta **no significa** automáticamente lavado de dinero.
- Una alerta significa: **existe una condición que requiere revisión**.
- No rechazar automáticamente crédito solo por existir una alerta PLD, salvo que una regla aprobada y documentada así lo determine.
- Mantener explicabilidad del resultado.
- Los factores y ponderaciones deben ser configurables cuando la arquitectura ya lo permita.
- Registrar qué factores provocaron una clasificación.
- No asumir que una disposición regulatoria, monto, plazo o umbral sigue vigente.
- No introducir límites regulatorios basados únicamente en conocimiento del modelo.
- Para una regla regulatoria nueva se requiere fuente/criterio confirmado por el usuario o documentación aprobada para la tarea.

Ejemplo de resultado explicable:

`Riesgo PLD ALTO debido a PEP + actividad de riesgo + operaciones superiores al perfil esperado.`

No usar ese ejemplo como regla real sin configuración aprobada.

---

## 10. Crédito

### Riesgo de crédito

Posibilidad de que el acreditado no cumpla con sus obligaciones de pago.

No mezclarlo con riesgo PLD.

Al cambiar procesos de crédito considerar, cuando aplique:

- solicitud;
- identidad;
- ingresos;
- gastos;
- capacidad de pago;
- endeudamiento;
- historial crediticio;
- garantías;
- producto;
- tasa;
- plazo;
- amortización;
- pagos;
- reestructura;
- cobranza.

No declarar que un crédito debe aprobarse o rechazarse sin una regla verificable y datos suficientes.

Los cálculos financieros deben ser reproducibles y conservar precisión/redondeo conforme a la regla funcional documentada.

---

## 11. Alertas PLD

Cuando se modifique el módulo de alertas, preservar conceptualmente:

- tipo/categoría;
- regla que la generó;
- fecha;
- datos que la originaron;
- severidad;
- responsable;
- investigación;
- evidencia;
- resolución;
- fecha de cierre;

en la medida en que el modelo existente lo soporte.

No borrar historial o bitácora para representar un cambio de estado actual.

Preferir transición trazable a sobrescritura sin evidencia.

---

## 12. Auditoría e integridad

No realizar cambios que destruyan trazabilidad sin una razón explícita.

Para datos sensibles/decisorios:

- preferir altas y cambios auditables;
- evitar borrado físico si el modelo ya usa activación/desactivación;
- conservar usuario/fecha cuando ya existen esos campos;
- no reutilizar un campo con semántica distinta solo para evitar una migración.

---

## 13. Configuración, secretos y archivos locales

`Web.config` está ignorado y es configuración local.

No subir:

- connection strings con credenciales;
- passwords;
- tokens;
- secretos SMTP;
- llaves privadas;
- certificados privados;
- archivos `.env`;
- respaldos de base de datos.

Usar valores de ejemplo únicamente en documentación.

---

## 14. Correo SMTP

El servicio se encuentra en:

`PLD/Services/PldEmailService.vb`

La activación se controla mediante configuración.

No habilitar envío real por defecto en una tarea que no sea específicamente de correo.

No enviar información PLD sensible a destinatarios de prueba o no aprobados.

---

## 15. Amortización CONDUSEF

Existe documentación específica en:

`PLD/Docs md/pasos_codex_amortizacion_condusef.md`

Antes de modificar la acción `amortizacion_condusef` o su UI, leer completa esa especificación y contrastarla con el código actual.

No cambiar fórmulas, orden de columnas, cargos iniciales o reglas de redondeo sin requerimiento explícito.

---

## 16. Pruebas mínimas esperadas

Según el tipo de cambio:

### Código VB / proyecto

- Compilar `PLD.sln`.
- Corregir errores introducidos por el cambio.

### Página + handler

- Usuario autenticado y autorizado: acceso correcto.
- Usuario no autenticado: respuesta/redirección esperada.
- Usuario autenticado sin permiso: 403/No autorizado.
- Handler AJAX: respuesta JSON coherente para 401/403.

### SQL

- Revisar sintaxis.
- Revisar dependencias.
- Verificar que el script no destruya datos existentes.
- Registrar orden de ejecución si depende de otro script.

### PLD / crédito

- Caso normal.
- Caso límite relevante.
- Caso con dato faltante.
- Verificar explicación del resultado.
- Verificar que PLD y crédito no se mezclen accidentalmente.

### Documentación solamente

No es obligatorio compilar si no se tocó código/proyecto/SQL; registrar en bitácora que la validación fue documental.

---

## 17. Criterio de terminado

Una tarea está terminada únicamente si:

- cumple el alcance;
- no rompe reglas de seguridad;
- tiene SQL incremental si lo necesita;
- si modificó base de datos, actualizó `PLD/SQL/ESTRUCTURA_BASE_DATOS.md`;
- revisó y actualizó `README.md` cuando el cambio afectó la documentación general del sistema;
- fue validada de forma proporcional al cambio;
- actualizó `BITACORA_CAMBIOS.md`;
- actualizó su archivo en `cambios_codex/` si existe;
- no dejó secretos;
- se reportaron pendientes y riesgos reales.

No afirmar “compila”, “probado”, “ejecutado” o “validado” si no se realizó realmente esa acción.

---

## 18. Restricciones y hallazgos actuales que no deben olvidarse

- `Web.config` no está versionado por diseño.
- El repositorio no contiene actualmente un script completo para crear toda la base operativa; hay dependencias de una base existente.
- Existen diferencias entre algunas referencias del `.vbproj` y lo declarado en `packages.config`; no actualizar dependencias sin evaluar impacto.
- `Handlers/handler_prueba_correo_pld.ashx` existe, pero al momento de establecer estas reglas no aparece registrado en los scripts actuales de seguridad. No asumir que está disponible para roles no administradores.
- Hay archivos grandes `CPdescarga.xls` en más de una ubicación; no duplicarlos innecesariamente.

---

## 19. Formato de cierre de un agente

Al terminar una tarea, el agente debe dejar un resumen breve con:

1. **Cambio realizado**
2. **Archivos modificados**
3. **SQL / migraciones**
4. **Estructura de BD actualizada** — sí/no/no aplica
5. **README actualizado** — sí/no/no aplica
6. **Validación**
7. **Bitácora**
8. **Pendientes o riesgos**
9. **Estado actualizado de fases/pendientes** cuando el trabajo forme parte de un plan por fases.

### Regla de cierre por fases

Cuando Edgar autorice desarrollar una fase de un roadmap o plan:

- cerrar la fase completamente dentro del alcance aprobado;
- actualizar el archivo de `cambios_codex/` correspondiente;
- actualizar README/SQL/estructura/bitácora cuando aplique;
- subir los cambios a GitHub;
- **reportar automáticamente al terminar la lista actualizada de fases completadas, siguiente fase y pendientes/bloqueos**, sin esperar que Edgar vuelva a solicitarla.

No ocultar fallas de compilación, pruebas no ejecutadas o dependencias faltantes.

---

## 20. Regla final

Ante duda entre “hacer algo nuevo” y “entender lo existente”, **primero entender lo existente**.

Ante duda entre una regla inventada y una regla confirmada, **usar únicamente la confirmada**.

Ante duda entre rapidez y trazabilidad en PLD/crédito, **preservar trazabilidad, seguridad, integridad y explicabilidad**.
