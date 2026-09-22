# Listas PLD / PEP — base técnica no bloqueada

**Estado:** COMPLETADO  
**Fecha:** 2026-09-22

## Objetivo
Implementar la administración y consulta manual de listas PLD/PEP con trazabilidad, sin inventar reglas institucionales de similitud o bloqueo.

## Reglas confirmadas
- Las listas se reciben/cargan manualmente.
- Personas bloqueadas: nombre, RFC y CURP opcional.
- Se requiere búsqueda en listas bloqueadas/PEP.
- No existe proveedor externo contratado.

## Alcance
- versiones históricas por lista;
- una versión vigente seleccionada explícitamente;
- carga XLSX/CSV;
- búsqueda exacta normalizada por nombre/RFC/CURP;
- auditoría de consultas y coincidencias;
- integración de consulta desde identidad del cliente;
- seguridad página-handler.

## Fuera de alcance por cliente
- similitud/fonética;
- porcentaje de coincidencia;
- bloqueo automático;
- criterio para descartar/confirmar homónimos;
- integración con proveedor externo.

## SQL
PLD/SQL/20260922_004_listas_pld.sql


## Resultado

Implementado:

- catálogo de listas con `BLOQUEADAS` y `PEP`;
- histórico de cargas/versiones;
- activación explícita de una versión vigente;
- carga `.xlsx` y `.csv`;
- hash SHA-256 para detectar archivo duplicado;
- encabezados NAME/NOMBRE/NOMBRE COMPLETO, RFC y CURP;
- búsqueda exacta normalizada;
- auditoría de consultas y resultados;
- pantalla de administración/consulta;
- consulta desde la identidad del cliente;
- seguridad página-handler;
- enlace desde Catálogos;
- proyecto y documentación actualizados.

Pendiente únicamente por definición del cliente:

- similitud/fonética;
- porcentaje de coincidencia;
- tratamiento de homónimos;
- bloqueo/autorización automática;
- proveedor externo.

No se implementó ninguna de esas reglas por inferencia.


### Separación de privilegios

Se separaron los endpoints para respetar el modelo default-deny:

- `handler_listas_pld.ashx`: administración de catálogo/cargas/activación; ligado únicamente a la página administrativa.
- `handler_consulta_listas_pld.ashx`: búsqueda exacta auditada; ligado a la página de listas y a Captura de Solicitud.

Esto evita que un usuario con permiso para capturar clientes pueda invocar operaciones administrativas de carga/activación.
