# Backend

Proyecto ASP.NET Core MVC (`Semana9.csproj`, .NET 10). Sin base de datos:
el unico recurso de negocio (`Producto`) guarda sus datos en una lista en
memoria durante la vida del proceso.

## Unidades

| Unidad | Doc | Archivos |
|---|---|---|
| Producto | [producto.md](./producto.md) | `Models/Producto.cs`, `Models/ProductoRepository.cs`, `Controllers/ProductoController.cs` |

`Controllers/HomeController.cs` y `Models/ErrorViewModel.cs` son el
scaffold por defecto de la plantilla MVC (paginas `Index`, `Privacy`,
`Error`) y no forman parte del caso de negocio: no se documentan como
unidad propia.

## Convenciones compartidas

- **Inyeccion de dependencias / ciclo de vida**: `ProductoRepository` se
  registra `Singleton` en `Program.cs`. Es una decision deliberada para
  este caso (sin BD): todas las peticiones comparten la misma lista en
  memoria. En un proyecto con base de datos real, el repositorio normalmente
  seria `Scoped` y delegaria en un `DbContext`.
- **Validacion en dos capas**: cada campo de `Producto` se valida por Data
  Annotations (automatico, vía `ModelState`) y, ademas, el controlador
  valida a mano cualquier regla que dependa del estado de la lista en
  memoria (unicidad de `Codigo`) con `ModelState.AddModelError`. Ver el
  detalle en [producto.md](./producto.md).
- **Patron Post-Redirect-Get**: todo POST exitoso termina en
  `RedirectToAction`, nunca en `return View()` directo, para evitar
  reenvios duplicados del formulario al refrescar el navegador.

## Known gaps

- No hay proyecto de tests automatizados (xUnit/NUnit) para el backend; la
  verificacion de las reglas de validacion se hizo manualmente (ver
  seccion "Tests" en [producto.md](./producto.md)).
- Los datos no persisten entre reinicios de la aplicacion (es el
  comportamiento esperado del caso: "no se requiere base de datos").
