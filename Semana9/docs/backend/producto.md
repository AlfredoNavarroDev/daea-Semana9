# Producto

Recurso del almacen que registra productos y evita que se guarden datos
incompletos o incorrectos (Caso 3: Validacion con Data Annotations). No usa
base de datos: los datos viven en una `List<Producto>` en memoria mientras
el proceso de la aplicacion esta corriendo.

**Files:** `Models/Producto.cs`, `Models/ProductoRepository.cs`,
`Controllers/ProductoController.cs`.

## Endpoints / entry points

| Method | Path | Auth | Input | Output |
|---|---|---|---|---|
| GET | `/Producto` o `/Producto/Index` | ninguna | - | Vista `Index` con `IEnumerable<Producto>` |
| GET | `/Producto/Create` | ninguna | - | Vista `Create` con un `Producto` vacio |
| POST | `/Producto/Create` | token anti-falsificacion (`[ValidateAntiForgeryToken]`) | form-encoded: `Codigo`, `Nombre`, `Precio`, `Stock`, `CorreoProveedor`, `__RequestVerificationToken` | Si es valido: `302` a `/Producto` (patron Post-Redirect-Get). Si no: `200` con la vista `Create` y el mismo objeto `Producto` que el usuario envio |

No hay ningun mecanismo de autenticacion global registrado en `Program.cs`
(`app.UseAuthorization()` esta presente pero no hay `[Authorize]` en ningun
controlador ni politica configurada), asi que ambos endpoints son
publicos.

## Behavior

Orden real de ejecucion en `ProductoController.Create(Producto producto)`
(POST), en `Controllers/ProductoController.cs`:

1. El model binder de ASP.NET Core instancia `producto` a partir del
   formulario y, en el mismo paso, evalua cada Data Annotation declarada en
   `Models/Producto.cs` (`[Required]`, `[StringLength]`, `[Range]`,
   `[EmailAddress]`, `[RegularExpression]`). Cualquier violacion se agrega
   a `ModelState` **antes** de que el cuerpo del metodo se ejecute.
2. Regla de negocio que ningun atributo puede expresar: el codigo debe ser
   unico. El controlador llama a `_repositorio.ExisteCodigo(producto.Codigo)`
   (comparacion case-insensitive, ver `ProductoRepository.cs`) y, si ya
   existe, agrega un error manual con
   `ModelState.AddModelError(nameof(Producto.Codigo), "Ya existe un producto registrado con ese codigo.")`.
   Este error queda asociado al campo `Codigo`, igual que si lo hubiera
   generado un atributo.
3. `if (!ModelState.IsValid) return View(producto);` — si cualquiera de los
   dos pasos anteriores dejo errores (los de los atributos o el de
   negocio), se devuelve la MISMA vista `Create` con el MISMO objeto
   `producto` que el usuario escribio, para que no pierda lo ya ingresado.
4. Si todo es valido: `_repositorio.Agregar(producto)` inserta el objeto en
   la lista en memoria y el controlador redirige a `Index` con
   `RedirectToAction`.

`ProductoRepository` (`Models/ProductoRepository.cs`) se registra como
`Singleton` en `Program.cs` (`builder.Services.AddSingleton<ProductoRepository>();`)
y se inyecta por constructor en `ProductoController`. Al ser singleton, es
la misma instancia -y la misma `List<Producto>`- para todas las peticiones
mientras la app esta corriendo; se reinicia vacia (salvo los 2 productos
semilla) cada vez que la app se reinicia, porque no hay persistencia real.

## Validation

Todas las reglas estan declaradas como Data Annotations en
`Models/Producto.cs`, con mensajes de error en espanol personalizados por
atributo:

| Campo | Reglas | Atributos |
|---|---|---|
| `Codigo` | Obligatorio; 3 a 10 caracteres; solo letras mayusculas (A-Z) y numeros (0-9), sin espacios ni minusculas | `[Required]`, `[StringLength(10, MinimumLength = 3)]`, `[RegularExpression(@"^[A-Z0-9]+$")]`, `[Display(Name = "Codigo")]` |
| `Nombre` | Obligatorio; 3 a 100 caracteres | `[Required]`, `[StringLength(100, MinimumLength = 3)]`, `[Display(Name = "Nombre del producto")]` |
| `Precio` | Obligatorio; numero entre 0.01 y 100000 | `[Required]`, `[Range(0.01, 100000)]`, `[Display(Name = "Precio (S/)")]` |
| `Stock` | Obligatorio; entero entre 0 y 100000 | `[Required]`, `[Range(0, 100000)]`, `[Display(Name = "Stock disponible")]` |
| `CorreoProveedor` | Obligatorio; debe tener formato de correo electronico valido | `[Required]`, `[EmailAddress]`, `[Display(Name = "Correo del proveedor")]` |

Regla adicional que **no** es un atributo y solo se valida en el
controlador: `Codigo` debe ser unico contra lo que ya hay en
`ProductoRepository` (ver paso 2 de Behavior). Esto es intencional: los
Data Annotations solo pueden validar un campo contra si mismo o contra
constantes fijas, no contra el estado actual de la "base de datos" en
memoria.

Estos mismos atributos son la fuente de la validacion de cliente: el Tag
Helper `asp-for` (en `Views/Producto/Create.cshtml`) los lee por reflexion
y genera los atributos `data-val-*` en el HTML; ver
`docs/frontend/producto-create.md`.

## Tests

No hay archivos de prueba automatizados para este recurso (el proyecto no
tiene un proyecto de tests). La verificacion se hizo manualmente contra el
servidor en ejecucion:

- POST con `Codigo=ab`, `Nombre=X`, `Precio=-5`, `Stock=-1`,
  `CorreoProveedor=noesemail` devuelve `200` con los 4 mensajes de error
  esperados en el `ValidationSummary` y en cada `asp-validation-for`.
- POST con `Codigo=ABC123` (ya existe en la semilla de `ProductoRepository`)
  devuelve `200` con el error de negocio "Ya existe un producto registrado
  con ese codigo." asociado al campo `Codigo`.
- POST con datos validos y codigo nuevo (`NEW001`) devuelve `302` hacia
  `/Producto` y el producto aparece en el listado.
