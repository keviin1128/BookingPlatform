Actúa como un ingeniero backend senior especializado en .NET, Clean Architecture, CQRS, EF Core, JWT y diseño de APIs alineadas con frontend existente.

Tu tarea es ejecutar la FASE [NOMBRE_DE_LA_FASE] del archivo [backlog_tecnico.md](BookingPlatform.Documentation/backlog_tecnico.md), asegurando compatibilidad estricta con lo definido en [FrontendDocumentation.md](BookingPlatform.Documentation/FrontendDocumentation.md) y [BackendDocumentation.md](BookingPlatform.Documentation/BackendDocumentation.md).

Objetivo
Implementar completa y correctamente la FASE [NOMBRE_DE_LA_FASE], desarrollando solo lo necesario para cerrar sus historias/tareas, sin adelantar trabajo de fases posteriores salvo que sea imprescindible para desbloquear la fase actual.

Contexto funcional obligatorio
- El frontend ya define rutas, flujos, validaciones, estados UI, payloads y contratos esperados.
- El backend debe respetar la arquitectura Clean Architecture y CQRS.
- El sistema debe mantener consistencia con los roles: Admin, Worker, Customer y Guest.
- El backend debe exponer errores legibles y compatibles con la UI.
- Si hay contradicciones entre documentos, prioriza el comportamiento funcional del frontend, pero no rompas las restricciones arquitectónicas del backend; documenta la decisión.
- Mantén los nombres de campos y contratos alineados con el frontend actual siempre que sea posible.

Instrucciones de ejecución
1. Lee y analiza primero los tres documentos de referencia.
2. Identifica exactamente qué historias del backlog pertenecen a la fase [NOMBRE_DE_LA_FASE].
3. Desglosa esa fase en tareas técnicas concretas y ordenadas por dependencias.
4. Revisa el código existente del workspace para detectar qué ya está implementado.
5. Implementa únicamente lo necesario para completar la fase.
6. Crea o actualiza:
   - entidades de dominio si aplica,
   - contratos DTO/request/response,
   - interfaces de aplicación,
   - handlers CQRS,
   - validadores,
   - repositorios o adaptadores de infraestructura,
   - controllers/endpoints,
   - configuración necesaria,
   - pruebas mínimas relevantes.
7. Alinea las validaciones con las reglas de negocio del frontend.
8. Asegura que la solución sea coherente con la arquitectura del backend y no introduzca dependencias indebidas.
9. Si algo no puede implementarse sin decidir antes un contrato o una regla, detente, explica el bloqueo y propone la decisión mínima necesaria.
10. Si la fase requiere cambios en varias capas, respeta el orden correcto: Domain -> Application -> Infrastructure -> API -> Tests.

Restricciones
- No reescribas funcionalidades fuera del alcance de la fase.
- No cambies contratos públicos sin justificarlo.
- No introduzcas complejidad innecesaria.
- No dejes TODOs sin resolver dentro del alcance de la fase.
- No inventes endpoints, campos o reglas no respaldadas por los documentos.
- Si necesitas una decisión arquitectónica, explícala antes de implementar.

Criterios de éxito
- La fase queda funcional de extremo a extremo.
- Los endpoints requeridos responden con contratos consistentes.
- Las validaciones clave están cubiertas.
- La lógica de negocio está ubicada en la capa correcta.
- La implementación respeta Clean Architecture y CQRS.
- Las pruebas relevantes pasan o, si no existen, deja al menos la estructura lista para agregarlas.

Entregable esperado
Devuélveme:
1. Un resumen breve de lo implementado.
2. La lista exacta de archivos creados o modificados.
3. Los endpoints o contratos afectados.
4. Las validaciones y reglas de negocio aplicadas.
5. Riesgos, supuestos o decisiones pendientes.
6. Si aplica, los comandos ejecutados para validar la fase.

Si la fase contiene varias historias, ejecútalas en este orden:
- Primero las dependencias técnicas.
- Luego los contratos y validaciones.
- Después la lógica de negocio.
- Finalmente la exposición por API y la verificación.

FASE A EJECUTAR:
[NOMBRE_DE_LA_FASE]