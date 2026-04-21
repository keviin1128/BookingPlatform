# Frontend Functional Guide

## 1. Objetivo
Este documento describe en detalle lo que el frontend ya implementa para que el backend se construya con el mismo comportamiento esperado.

Incluye:
- Modulos y vistas
- Flujos de usuario
- Componentes e interacciones
- Validaciones de frontend
- Manejo de estado y datos
- Endpoints simulados/esperados
- Estructura de datos (modelos y payloads)

## 2. Alcance tecnico
- Stack: React + React Router + Sonner (toasts) + Lucide Icons
- Patron de datos: capa de servicios con `MOCK_MODE = true`
- Estado global: `AuthContext` (usuario/sesion)
- Estado local: `useState`, `useEffect`, `useMemo`, `useCallback`
- Persistencia de sesion: `localStorage` (`token`, `user`)

## 3. Arquitectura de navegacion

### 3.1 Rutas publicas
- `/login`: inicio de sesion por telefono
- `/register`: registro de cliente
- `/reservar`: flujo de reserva como invitado

### 3.2 Rutas privadas (bajo layout comun)
- `/dashboard`: dashboard dinamico por rol
- `/appointments`: listado de citas (cliente/admin)
- `/appointments/new`: nueva cita (cliente)
- `/loyalty`: programa de lealtad (cliente/admin, pero admin redirige)
- `/profile`: perfil de usuario
- `/admin`: panel administrativo (solo admin)

### 3.3 Reglas de proteccion
- Si no hay sesion: redireccion a `/login`
- Si no cumple rol requerido: redireccion a `/dashboard`
- Compatibilidad legacy: `requireAdmin`

## 4. Modulos y vistas

### 4.1 Autenticacion

#### Login (`/login`)
Objetivo:
- Permitir acceso usando telefono

Comportamiento:
- Campo obligatorio: telefono
- Llama `authService.login({ telefono })`
- Guarda token y usuario en `AuthContext` y `localStorage`
- Redirige:
  - admin -> `/admin`
  - resto -> `/dashboard`

Estados UI:
- `loading` durante submit
- `error` con mensaje validado o de servicio

#### Registro (`/register`)
Objetivo:
- Crear cuenta cliente

Comportamiento:
- Campo obligatorio: telefono
- Campos opcionales: nombre, email
- Llama `authService.register(formData)`
- Al exito: toast y redireccion a `/login`

Regla de negocio derivada:
- Telefono debe ser unico (normalizado, solo digitos)

### 4.2 Layout y navegacion por rol

Objetivo:
- Mostrar menu lateral y topbar segun rol

Links por rol:
- Admin: dashboard, citas, lealtad, perfil, admin
- Trabajador: dashboard (mi jornada), perfil
- Cliente: dashboard, mis citas, nueva cita, lealtad, perfil

Comportamiento:
- Control de sidebar mobile (`sidebarOpen`)
- Logout limpia sesion (`token`, `user`) y estado global

### 4.3 Dashboard cliente/admin (`/dashboard`)

Objetivo:
- Resumen rapido de actividad

Datos cargados:
- `appointmentService.getAll(user)`
- `loyaltyService.getData(user.id)`

Elementos funcionales:
- Proxima cita (no cancelada, ordenada por fecha/hora)
- Conteo de citas completadas
- Puntos de lealtad y nivel
- Lista de proximas citas (top 3 no canceladas)
- Accesos directos a nueva cita y lealtad

Estados:
- `loading`, `error`

### 4.4 Dashboard trabajador (`/dashboard` para rol trabajador)

Objetivo:
- Gestion operativa del trabajador

Datos cargados:
- `adminService.getAppointments()`
- `workerService.getAll()` para identificar perfil actual por `userId`

Funcionalidad:
- Muestra citas asignadas al trabajador (no canceladas)
- Separa pendientes vs completadas
- Permite marcar cita como completada (`appointmentService.complete(id)`)

Reglas:
- Solo se consideran del trabajador autenticado
- Accion "Completar" solo en pendientes

### 4.5 Citas (`/appointments`)

Objetivo:
- Consultar y gestionar citas

Funcionalidad:
- Carga citas de usuario (o todas para admin)
- Filtros por estado: todas, pendiente, confirmada, completada, cancelada
- Cancelar cita si estado no es cancelada/completada (`appointmentService.remove(id)`)

Estados:
- `loading`, `error`, `cancellingId`

### 4.6 Nueva cita (`/appointments/new` y `/reservar`)

Objetivo:
- Flujo wizard de 5 pasos para crear cita

Pasos:
1. Servicio
2. Trabajador (filtrado por servicio)
3. Fecha
4. Horario disponible
5. Confirmacion

Modo invitado (`/reservar`):
- Requiere nombre + telefono
- Email opcional

Datos cargados:
- Catalogo: `serviceService.getAll()`, `workerService.getAll()`
- Slots: `appointmentService.getSlots(date, { workerId, serviceId, duration })`

Payload enviado en confirmacion:
- `servicio`, `servicioId`, `fecha`, `hora`, `precio`, `duracion`
- `trabajadorId`, `trabajadorNombre`
- Datos cliente/invitado (`clienteNombre`, `clienteEmail`, `clienteTelefono`)
- `clienteRegistrado`

Post-exito:
- Invitado -> `/login`
- Usuario autenticado -> `/appointments`

### 4.7 Lealtad (`/loyalty`)

Objetivo:
- Consultar progreso y canjear recompensas

Regla de rol:
- Si usuario es admin: redireccion a `/admin`

Funcionalidad:
- Carga `loyaltyService.getData(user.id)`
- Calcula progreso a siguiente nivel
- Identifica proxima recompensa disponible
- Permite canje: `loyaltyService.redeem(user.id, rewardId)`
- Muestra historial de canjes

Reglas:
- Canje solo si puntos >= puntos requeridos y recompensa disponible

### 4.8 Perfil (`/profile`)

Objetivo:
- Visualizar y editar datos de usuario

Funcionalidad:
- Carga inicial `userService.me(user.id)`
- Modo edicion habilitable
- Guardado `userService.updateMe(user.id, formData)`
- Sincroniza contexto (`updateUser`) y `localStorage`

Validacion:
- En guardado son obligatorios nombre, email y telefono

### 4.9 Administracion (`/admin`)

Objetivo:
- Operacion central para admin

Tabs:
- `overview`: KPIs + agenda general
- `services`: CRUD de servicios
- `loyalty`: CRUD de planes de lealtad
- `customers`: listado + busqueda de clientes
- `workers`: alta y listado de trabajadores

#### 4.9.1 Overview
- `adminService.getStats()`
- `adminService.getAppointments()`
- Puede marcar citas como completadas

#### 4.9.2 ManageServicesTab
- Lista servicios
- Crear/editar/eliminar servicio
- Campos obligatorios: nombre, descripcion, duracion, precio

#### 4.9.3 ManageLoyaltyTab
- Lista planes y estadisticas
- Crear/editar/eliminar plan
- Campos obligatorios:
  - nombre del plan
  - puntos por cita
  - puntos por dolar
  - niveles completos (nombre, min, max)

#### 4.9.4 ManageCustomersTab
- Tabla de clientes
- Filtro por nombre/email

#### 4.9.5 ManageWorkersTab
- Crear trabajador
- Campo obligatorio: telefono
- Opcionales: nombre, email, especialidad, servicios asignados
- Lista con busqueda por nombre/telefono/especialidad

## 5. User journeys

### 5.1 Journey cliente registrado
1. Login por telefono
2. Ve dashboard con resumen
3. Crea cita en wizard
4. Consulta citas y puede cancelar
5. Revisa lealtad y canjea recompensas
6. Edita perfil

### 5.2 Journey invitado
1. Entra a `/reservar`
2. Completa nombre + telefono (email opcional)
3. Completa wizard de cita
4. Confirma y se redirige a `/login`

### 5.3 Journey trabajador
1. Login con telefono de rol trabajador
2. Accede a "Mi jornada"
3. Consulta citas asignadas
4. Marca pendientes como completadas
5. Consulta historial de completadas

### 5.4 Journey admin
1. Login admin
2. Entra a panel admin
3. Revisa KPIs y agenda general
4. Gestiona servicios, planes, clientes y trabajadores

## 6. Estructura de componentes e interaccion

### 6.1 Arbol principal
- `App`
  - `AuthProvider`
  - `RouterProvider`
    - Rutas publicas
    - Rutas protegidas bajo `Layout`
      - `ProtectedRoute`
      - `Layout`
        - Sidebar/topbar
        - `Outlet`

### 6.2 Seleccion de dashboard
- `DashboardSelector`:
  - rol `trabajador` -> `WorkerDashboardPage`
  - otros -> `DashboardPage`

### 6.3 Componentes admin
- `AdminDashboardPage` coordina tabs
- Cada tab consume su servicio de dominio
- Recarga datos tras acciones CRUD

### 6.4 Patron de interaccion UI -> servicio
1. Componente dispara accion
2. Ejecuta validaciones locales
3. Llama servicio async
4. Actualiza estado local
5. Muestra toast/error

## 7. Validaciones implementadas en frontend

### 7.1 Validador comun
Helper `buildMissingFieldsMessage(fields, prefix)`:
- Recorta valores (`trim`)
- Construye mensaje "falta/faltan ..."

### 7.2 Validaciones por modulo
- Login: telefono obligatorio
- Registro: telefono obligatorio
- Nueva cita (usuario): servicio, trabajador, fecha, horario
- Nueva cita (invitado): nombre, telefono, servicio, trabajador, fecha, horario
- Perfil: nombre, email, telefono obligatorios al guardar
- Servicios admin: nombre, descripcion, duracion, precio
- Planes lealtad admin: nombre, puntos, niveles completos
- Trabajadores admin: telefono obligatorio

### 7.3 Validaciones/guardas de negocio en servicios mock
- Telefono unico en registro
- Telefono obligatorio al crear trabajador
- Canje de recompensa con puntos suficientes
- Reserva invitado requiere nombre y telefono
- Error si recurso inexistente (ej. servicio/plan/usuario no encontrado)

## 8. Manejo de estado y datos

### 8.1 Estado global (AuthContext)
Estado:
- `token`, `user`, `loading`

Derivados:
- `isAuthenticated`, `isAdmin`, `isWorker`, `isClient`

Acciones:
- `login(credentials)`
- `register(payload)`
- `logout()`
- `updateUser(nextUser)`

Persistencia:
- Lee/escribe `token` y `user` en `localStorage`

### 8.2 Estado local por vista
Patron repetido:
- `loading` para carga inicial
- `error` para fallas de servicio
- flags de accion (`saving`, `submitting`, `redeemingId`, etc.)
- memoizacion (`useMemo`) para derivados de lista/filtro/progreso

### 8.3 Control de concurrencia y limpieza
- Varias vistas usan bandera `active` en `useEffect` para evitar setState tras unmount

## 9. Endpoints simulados y contratos esperados

Nota: hoy `MOCK_MODE = true` en todos los servicios de dominio. Si se pasa a backend real, estos son los endpoints ya esperados por el frontend.

### 9.1 Auth
- `POST /auth/register`
- `POST /auth/login`

### 9.2 Appointments
- `GET /appointments`
- `GET /appointments/:id`
- `POST /appointments`
- `DELETE /appointments/:id`
- `GET /appointments/slots?date=...&workerId=...&serviceId=...&duration=...`
- `PUT /appointments/:id/complete`

### 9.3 Services
- `GET /services`
- `GET /services/:id`
- `POST /services`
- `PUT /services/:id`
- `DELETE /services/:id`

### 9.4 Workers
- `GET /workers`
- `GET /workers/:id`
- `GET /workers?serviceId=:id`

### 9.5 Users
- `GET /users/me`
- `PUT /users/me`
- `GET /users/me/history`

### 9.6 Loyalty cliente
- `GET /loyalty`
- `POST /loyalty/redeem`

### 9.7 Admin
- `GET /admin/appointments`
- `GET /admin/customers`
- `GET /admin/workers`
- `POST /admin/workers`

### 9.8 Admin loyalty plans
- `GET /admin/loyalty/plans`
- `GET /admin/loyalty/plans/:planId`
- `POST /admin/loyalty/plans`
- `PUT /admin/loyalty/plans/:planId`
- `DELETE /admin/loyalty/plans/:planId`
- `GET /admin/loyalty/stats`

## 10. Modelos de datos y payloads

### 10.1 Usuario
```ts
type User = {
  id: number;
  nombre: string;
  email: string;
  telefono: string;
  rol: 'admin' | 'cliente' | 'trabajador';
}
```

### 10.2 Login
```ts
type LoginRequest = { telefono: string };
type LoginResponse = { token: string; user: User };
```

### 10.3 Registro
```ts
type RegisterRequest = {
  nombre?: string;
  email?: string;
  telefono: string;
}
```

### 10.4 Servicio
```ts
type Service = {
  id: number;
  nombre: string;
  descripcion: string;
  duracion: number;
  precio: number;
  activo: boolean;
}
```

### 10.5 Trabajador
```ts
type Worker = {
  id: number;
  userId: number;
  nombre: string;
  especialidad: string;
  servicios: number[];
  horarioResumen: string;
  agenda?: Record<number, [string, string][]>;
}
```

### 10.6 Cita
```ts
type Appointment = {
  id: number;
  servicio: string;
  servicioId: number;
  fecha: string; // YYYY-MM-DD
  hora: string;  // HH:mm
  estado: 'pendiente' | 'confirmada' | 'completada' | 'cancelada';
  precio: number;
  duracion: number;
  trabajadorId: number;
  trabajadorNombre: string;
  clienteId: number | null;
  clienteNombre: string;
  clienteEmail?: string;
  clienteTelefono?: string;
  clienteRegistrado?: boolean;
}
```

### 10.7 Crear cita
```ts
type CreateAppointmentRequest = {
  servicio: string;
  servicioId: number;
  fecha: string;
  hora: string;
  precio: number;
  duracion: number;
  trabajadorId: number;
  trabajadorNombre: string;
  clienteNombre?: string;
  clienteEmail?: string;
  clienteTelefono?: string;
  clienteRegistrado?: boolean;
}
```

### 10.8 Lealtad
```ts
type LoyaltyReward = {
  id: number;
  nombre: string;
  descripcion: string;
  puntosRequeridos: number;
  disponible: boolean;
}

type LoyaltyHistoryItem = {
  id: number;
  nombre: string;
  fechaCanjeada: string;
  puntosGastados: number;
}

type LoyaltyData = {
  userId: number;
  planId: number;
  puntos: number;
  nivel: string;
  puntosParaSiguienteNivel: number;
  siguienteNivel: string;
  recompensasDisponibles: LoyaltyReward[];
  historialRecompensas: LoyaltyHistoryItem[];
}
```

### 10.9 Plan de lealtad (admin)
```ts
type LoyaltyLevel = {
  nombre: string;
  puntosMinimos: number;
  puntosMaximos: number;
}

type LoyaltyPlan = {
  id: number;
  nombre: string;
  descripcion: string;
  puntosXCita: number;
  puntosXDolar: number;
  niveles: LoyaltyLevel[];
  activo: boolean;
  createdAt?: string;
}
```

## 11. Reglas de negocio identificadas
1. La autenticacion se hace por telefono.
2. El telefono es unico para registro y para crear trabajadores.
3. Los permisos dependen de `rol` y se aplican en rutas protegidas.
4. Una cita cancelada no cuenta como activa para dashboards.
5. Una cita completada/cancelada no puede volver a cancelarse desde listado cliente.
6. En reserva invitado, nombre y telefono son obligatorios.
7. Slots dependen de agenda del trabajador, duracion del servicio y solapamientos.
8. Canje de lealtad descuenta puntos y registra historial.
9. Admin puede marcar citas como completadas desde agenda general.
10. En perfil, para guardar cambios se exigen todos los campos principales.

## 12. Requerimientos backend derivados (checklist)
1. Implementar todos los endpoints listados con contratos compatibles.
2. Mantener enums/estados de cita (`pendiente`, `confirmada`, `completada`, `cancelada`).
3. Devolver errores de validacion legibles para mostrarse en UI.
4. Soportar reserva de cliente autenticado e invitado.
5. Exponer disponibilidad horaria por trabajador con control de solapamiento.
6. Soportar capa admin para servicios, trabajadores, clientes y lealtad.
7. Incluir payloads con campos ya consumidos por UI (nombres de propiedad exactos).
8. Mantener consistencia de IDs numericos usados por el frontend actual.

## 13. Semilla actual mock (referencia)
El mock incluye:
- Usuarios admin/cliente/trabajador
- Servicios iniciales
- Trabajadores con agenda semanal
- Citas de ejemplo
- Plan de lealtad base y data de lealtad por usuario

Esta semilla sirve como referencia funcional para pruebas de integracion cuando el backend real este disponible.