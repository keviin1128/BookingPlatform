# 📄 Booking Platform API

Backend de una plataforma SaaS para gestión de citas en negocios basados en servicios.

---

## 🧭 Descripción General

Este proyecto corresponde al backend de una plataforma de agendamiento de citas para negocios como:

- 💈 Barberías  
- 💇 Salones de belleza  
- 💆 Centros de estética  
- 🏋️ Gimnasios / entrenadores personales  
- 🐶 Servicios para mascotas  
- 📚 Consultorías / clases  

El sistema está diseñado como un **SaaS multi-negocio**, permitiendo que múltiples empresas utilicen la misma plataforma.

---

## 🎯 Objetivo del Backend

Proveer una API REST robusta que permita:

- 🔐 Autenticación de usuarios (JWT)
- 📅 Gestión de citas
- 🛠️ Gestión de servicios
- 👥 Gestión de clientes
- 🧩 Control de roles (Admin, Worker, Customer)
- 🎁 Sistema de fidelización

---

## 🏗️ Arquitectura

El proyecto implementa:

### ✅ Clean Architecture

### 📌 Principios aplicados

- Separación de responsabilidades  
- Inversión de dependencias  
- Bajo acoplamiento  
- Alta cohesión  
- Testabilidad  

---

## 📦 Estructura del Proyecto

```plaintext
src/
├── BookingPlatform.API
├── BookingPlatform.Application
├── BookingPlatform.Domain
└── BookingPlatform.Infrastructure
```
---

## 📁 Descripción de Capas

### 🌐 API Layer (Presentation)

📁 `BookingPlatform.API`

**Responsabilidades:**

- Exponer endpoints HTTP  
- Configurar middleware  
- Autenticación JWT  
- Swagger  
- Manejo de requests/responses  

**Estructura:**
Controllers/
Middleware/
Filters/
Extensions/


**Ejemplo:**

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginCommand command)
```

### 🧠 Application Layer (Lógica de negocio)

📁 `BookingPlatform.Application`

**Responsabilidades:**

- Casos de uso
- Lógica de negocio
- Validaciones
- DTOs
- Interfaces

**Estructura:**
Authentication/
Appointments/
Services/
Common/

**Sub estructura**
Commands/
Queries/
DTOs/
Validators/
Interfaces/

### 🔁 CQRS Pattern

Separación entre:

- **Commands** → Escritura (crear, actualizar)  
- **Queries** → Lectura  

**Ejemplo:**

- `RegisterCommand`  
- `LoginCommand`  

---

### 🧱 Domain Layer (Core del negocio)

📁 `BookingPlatform.Domain`

**Responsabilidades:**

- Entidades del dominio  
- Enums  
- Reglas de negocio puras  

**Estructura:**
Entities/
Enums/
ValueObjects/


**Ejemplo:**

- `User`  
- `Appointment`  
- `Service`  

⚠️ Esta capa **no depende de ninguna otra**.

---

### 🗄️ Infrastructure Layer

📁 `BookingPlatform.Infrastructure`

**Responsabilidades:**

- Persistencia (EF Core)  
- Repositorios  
- Seguridad (hashing)  
- JWT  
- Integraciones externas  

**Estructura:**
Data/
Repositories/
Authentication/
Security/
Configurations/
ServiceExtensions/


---

## 🧩 Patrones de Diseño Utilizados

- ✅ Clean Architecture  
- ✅ CQRS  
- ✅ Repository Pattern  
- ✅ Dependency Injection  
- ✅ Options Pattern  
- ✅ Middleware Pattern  

---

## 🔐 Autenticación

Se implementa autenticación basada en:

**JWT (JSON Web Token)**

### Características

- Stateless  
- Seguro  
- Escalable  

### 🔑 Flujo de autenticación

Login/Register
↓
Validación credenciales
↓
Generación JWT
↓
Cliente guarda token
↓
Token enviado en headers


### Header requerido
Authorization: Bearer {token}


### Claims incluidos

- `userId`  
- `email`  
- `role`  
- `businessId`  

---

## 👥 Roles del Sistema

| Rol      | Permisos              |
|----------|----------------------|
| Admin    | Control total        |
| Worker   | Ver agenda           |
| Customer | Agendar citas        |
| Guest    | Agenda rápida        |

---

## 🗄️ Base de Datos

**Motor:** PostgreSQL  
**ORM:** Entity Framework Core  

### Enfoque

- Code First  
- Migrations  

### Entidades principales

- `User`  
- `Business`  
- `Service`  
- `Appointment`  
- `Staff`  
- `LoyaltyAccount`  

---

## 🧮 Sistema de Fidelización

Modelo básico:

- 1 servicio = 1 punto  
- 5 puntos = recompensa  

---

## ⚙️ Configuración

Uso de `appsettings.json` con Options Pattern:

```json
{
  "BookingPlatform": {
    "Infrastructure": {
      "PostgreSQL": {
        "ConnectionString": ""
      }
    }
  }
}
```

## Testing

Unit Tests (xUnit)
Integration Tests
Mocking (Moq)

## Buenas Prácticas Adoptadas
- Uso de GUID como PK
- Separación por capas
- Inyección de dependencias
- Validación con FluentValidation
- Manejo de errores centralizado
- Código limpio (Clean Code)

## 🧠 Consideraciones de Diseño
- Sistema pensado como SaaS
- Soporte multi-negocio desde el inicio
- Escalable horizontalmente
- Preparado para frontend desacoplado

