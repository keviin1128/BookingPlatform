Backlog Técnico

Fase 0. Alineación de contrato y base técnica

BT-001: Definir contrato canónico de autenticación. Cerrar la discrepancia entre login por teléfono del frontend y login por email del backend documental, y dejar una única fuente de verdad para request/response.
BT-002: Definir tipo de identificador definitivo. Confirmar si el backend expondrá GUID, int o una capa de compatibilidad para el frontend, y documentar la decisión.
BT-003: Definir estrategia SaaS/multi-negocio. Confirmar si businessId es obligatorio en todo el dominio o si la implementación inicial será single-tenant.
BT-004: Corregir el wiring base de la API. Registrar MediatR, autenticación JWT, autorización, AppDbContext y configuración de infraestructura de forma consistente.
BT-005: Implementar manejo estándar de errores. Unificar formato de validación y errores de negocio para que el frontend pueda consumirlos sin lógica especial.
BT-006: Crear semilla inicial mínima. Cargar admin, cliente, trabajador, servicios, citas y datos de lealtad para pruebas funcionales.
Criterio de cierre de la fase: la API arranca, autentica, responde con errores consistentes y la base de configuración queda estable.

Fase 1. Autenticación y perfil

BT-101: Implementar registro de usuario. Crear el caso de uso y endpoint para alta de cliente con teléfono obligatorio y validación de unicidad.
BT-102: Implementar login por teléfono. Crear el caso de uso y endpoint de acceso con generación de JWT y respuesta compatible con el frontend.
BT-103: Implementar generación real de JWT. Completar JwtTokenGenerator con claims, expiración y configuración desde options.
BT-104: Implementar obtención del usuario actual. Exponer GET /users/me o equivalente para hidratar sesión y perfil.
BT-105: Implementar actualización de perfil. Permitir edición de nombre, email y teléfono con sincronización de contexto.
BT-106: Agregar autorización por rol. Proteger rutas y políticas para admin, worker y client.
Criterio de cierre de la fase: un usuario puede registrarse, iniciar sesión, mantener sesión y editar su perfil con autorización real.

Fase 2. Catálogo de servicios y trabajadores

BT-201: Crear entidad y repositorio de servicios. Modelar el catálogo base con estado activo/inactivo.
BT-202: Implementar CRUD de servicios. Exponer alta, edición, listado, detalle y eliminación de servicios.
BT-203: Crear entidad y repositorio de trabajadores. Modelar asignación de usuario, especialidad, servicios y agenda.
BT-204: Implementar alta y listado de trabajadores. Permitir búsqueda por nombre, teléfono y especialidad.
BT-205: Implementar asignación de servicios a trabajador. Vincular trabajador con servicios habilitados.
BT-206: Implementar lectura de agenda por trabajador. Dejar lista la base para cálculo de slots.
Criterio de cierre de la fase: el panel admin puede administrar servicios y trabajadores, y el sistema ya conoce qué trabajadores ofrecen qué servicios.

Fase 3. Gestión de citas

BT-301: Crear entidad y repositorio de citas. Modelar estados, cliente, trabajador, servicio, fecha, hora, precio y duración.
BT-302: Implementar creación de cita autenticada. Soportar flujo de cliente registrado con validación de datos obligatorios.
BT-303: Implementar creación de cita como invitado. Soportar reserva sin cuenta con nombre y teléfono obligatorios.
BT-304: Implementar cálculo de slots disponibles. Filtrar por fecha, trabajador, servicio, duración y solapamientos.
BT-305: Implementar listado de citas por rol. Cliente ve sus citas; admin ve todas; trabajador ve las asignadas.
BT-306: Implementar detalle de cita. Exponer campos completos para dashboard, listado y administración.
BT-307: Implementar cancelación de cita. Bloquear cancelación si la cita ya está completada o cancelada.
BT-308: Implementar completar cita. Permitir cambio a completada desde trabajador o admin según reglas definidas.
Criterio de cierre de la fase: el flujo principal del frontend ya funciona end-to-end: reservar, listar, cancelar, completar y consultar disponibilidad.

Fase 4. Dashboard por rol

BT-401: Implementar dashboard de cliente. Resumir próximas citas, completadas, puntos de lealtad y acceso rápido.
BT-402: Implementar dashboard de trabajador. Mostrar jornada, pendientes, completadas y acción de completar cita.
BT-403: Implementar dashboard de admin. Mostrar KPIs, agenda general y estado operacional.
BT-404: Normalizar consultas reutilizables. Evitar duplicar lógica entre dashboard, citas y admin.
Criterio de cierre de la fase: cada rol ve exactamente la información que el frontend espera, usando las mismas fuentes de datos.

Fase 5. Lealtad

BT-501: Crear modelo de lealtad. Definir plan, niveles, puntos, recompensas e historial.
BT-502: Implementar consulta de lealtad. Exponer puntos, nivel, siguiente nivel, recompensas disponibles e historial.
BT-503: Implementar canje de recompensas. Validar puntos suficientes y disponibilidad de la recompensa.
BT-504: Registrar historial de canjes. Persistir cada redención con fecha y puntos gastados.
BT-505: Vincular puntos a citas completadas. Definir y ejecutar la regla de acumulación según negocio.
Criterio de cierre de la fase: el frontend puede mostrar progreso y canjear recompensas con reglas reales y persistencia.

Fase 6. Administración

BT-601: Implementar overview admin. Exponer KPIs, agenda general y citas administrables.
BT-602: Implementar gestión de clientes. Listado y búsqueda por nombre o email.
BT-603: Implementar gestión de trabajadores en admin. Alta, listado y búsqueda.
BT-604: Implementar CRUD de planes de lealtad. Incluir niveles completos y estadísticas.
BT-605: Implementar estadísticas de lealtad. Exponer métricas para el panel admin.
BT-606: Permitir completar citas desde admin. Alinear esta acción con la lógica de citas central.
Criterio de cierre de la fase: el panel administrativo queda funcional para operar servicios, personas, citas y lealtad.

Fase 7. Validación, pruebas y endurecimiento

BT-701: Crear validadores por comando. Cubrir login, registro, perfil, citas, servicios, trabajadores y lealtad.
BT-702: Crear pruebas unitarias de handlers. Cubrir escenarios felices y de error de auth, citas y lealtad.
BT-703: Crear pruebas de repositorio o integración. Validar unicidad, consultas y persistencia crítica.
BT-704: Verificar contratos OpenAPI. Asegurar que Swagger refleje el contrato consumido por el frontend.
BT-705: Revisar seguridad y autorización. Confirmar protección de rutas y claims de roles.
BT-706: Revisar migraciones y seed. Dejar la base lista para ejecución reproducible.
Criterio de cierre de la fase: el backend queda verificable, documentado y estable para integración continua con frontend.