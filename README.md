# Caso 3: Validación con Data Annotations

Proyecto ASP.NET Core MVC (`Semana9/`, .NET 10) que resuelve el escenario:
un almacén necesita registrar productos y evitar que se guarden datos
incompletos o incorrectos. No usa base de datos: los productos se
almacenan en una lista en memoria (`ProductoRepository`, registrado como
`Singleton`) mientras la aplicación está en ejecución.

El código se explica directamente desde el proyecto (ver
`Semana9/docs/backend/` y `Semana9/docs/frontend/` para el detalle técnico
de qué hace cada archivo, cómo lo hace y dónde). Este README cubre la
parte teórica del caso.

## 1. Validación en el cliente vs. validación en el servidor

| | Validación en el cliente | Validación en el servidor |
|---|---|---|
| **Dónde corre** | JavaScript, en el navegador del usuario (`jquery.validate` + `jquery.validate.unobtrusive`) | C#, en el servidor (`ModelState` dentro de `ProductoController`) |
| **Cuándo actúa** | Antes de enviar el formulario (tecleo a tecleo / al perder foco) | Al recibir el POST, antes de ejecutar cualquier lógica de negocio |
| **Quién la controla** | El usuario: puede desactivar JavaScript, editar el HTML, o enviar la petición con otra herramienta (curl, Postman) sin pasar por el formulario | El servidor: nadie fuera del equipo que despliega la app puede modificarla |
| **Para qué sirve** | Mejorar la experiencia: feedback inmediato, sin esperar un viaje de ida y vuelta al servidor | Ser la única fuente de verdad: garantizar que ningún dato inválido llegue a guardarse, sin importar de dónde vino la petición |
| **¿Se puede omitir?** | Sí, siempre es opcional y nunca debe ser la única defensa | No, siempre debe estar presente |

**Por qué se necesitan las dos:** la validación de cliente no es
seguridad, es conveniencia. Cualquier petición HTTP puede construirse sin
pasar por el navegador ni por el JavaScript de la página (un script,
una extensión del navegador, `curl`, Postman, un atacante). Si la única
validación fuera la del cliente, bastaría con omitir ese paso para
guardar cualquier dato. Por eso el servidor **repite** las mismas
validaciones de forma independiente — no confía en que el cliente ya
las aplicó.

### Ejemplo real de lo que puede pasar si solo se valida en el navegador

Un formulario de "Nuevo producto" que solo valida con JavaScript que
`Precio` sea mayor a cero. Un usuario (o un script automatizado) abre las
herramientas de desarrollador, desactiva el JavaScript de la página o
hace la petición POST directamente contra la URL del formulario, y envía
`Precio=-500`. Como no hay ninguna verificación en el servidor, el precio
negativo se guarda igual. Consecuencia típica en un sistema real: el
catálogo público muestra un producto "gratis" o con precio negativo, o un
reporte de inventario calcula totales incorrectos porque incluyó un valor
que nunca debió existir. La validación del servidor es la que cierra esa
puerta, porque corre sin importar qué hizo o dejó de hacer el navegador.

## 2. Flujo de `ModelState` desde el envío del formulario hasta la respuesta

```
Usuario llena el formulario (Create.cshtml)
        │
        ▼
[Validación de cliente] jquery.validate + unobtrusive
  lee los atributos data-val-* generados por asp-for
  a partir de las Data Annotations del modelo
        │
        ├── Si falla ──► muestra el error junto al campo,
        │                NO envía el formulario, el servidor
        │                nunca se entera de este intento
        │
        └── Si pasa (o el cliente no tiene JS) ──► POST real a /Producto/Create
                        │
                        ▼
        [Model Binding] ASP.NET Core construye el objeto Producto
        a partir del formulario y, en el mismo paso, evalúa cada
        Data Annotation ([Required], [StringLength], [Range],
        [EmailAddress], [RegularExpression]). Cada violación se
        agrega a ModelState ANTES de que el código del controlador
        se ejecute.
                        │
                        ▼
        [Controlador: ProductoController.Create(Producto producto)]
        Regla de negocio que ningún atributo puede expresar:
        ¿el Código ya existe en la lista en memoria?
        → si sí: ModelState.AddModelError("Codigo", "Ya existe...")
                        │
                        ▼
              ModelState.IsValid ?
                /                  \
             NO                     SÍ
              │                      │
              ▼                      ▼
     return View(producto)   _repositorio.Agregar(producto)
     (misma vista, mismos    RedirectToAction("Index")
     datos que escribió el   (patrón Post-Redirect-Get)
     usuario, + todos los           │
     errores de ModelState          ▼
     visibles en                Vista Index
     ValidationSummary y        muestra el
     en cada campo)             producto nuevo
```

Puntos clave del flujo:

- **`ModelState` se llena en dos momentos distintos**: automáticamente
  durante el model binding (los atributos del modelo) y manualmente dentro
  del controlador (la regla de negocio del código duplicado). Ambos tipos
  de error terminan en el mismo lugar y se muestran de la misma forma.
- **`ModelState.IsValid` es la única decisión que importa** en el
  servidor: no se revisa campo por campo a mano, se revisa una sola vez
  después de que todas las fuentes de error (atributos + reglas de
  negocio) ya escribieron lo que tenían que escribir.
- **Si no es válido, se devuelve la misma vista con el mismo modelo**
  (`return View(producto)`), nunca una redirección — así el usuario no
  pierde lo que escribió y ve exactamente qué campos fallaron.

## 3. Atributos de Data Annotations usados y qué valida cada uno

| Atributo | Qué valida | Campo(s) donde se usó | Mensaje de error (ejemplo) |
|---|---|---|---|
| `[Required]` | El campo no puede quedar vacío / nulo | Todos: `Codigo`, `Nombre`, `Precio`, `Stock`, `CorreoProveedor` | "El código es obligatorio." |
| `[StringLength(max, MinimumLength = min)]` | Longitud del texto dentro de un rango | `Codigo` (3–10), `Nombre` (3–100) | "El código debe tener entre 3 y 10 caracteres." |
| `[Range(min, max)]` | Valor numérico dentro de un rango | `Precio` (0.01–100000), `Stock` (0–100000) | "El precio debe estar entre 0.01 y 100000." |
| `[EmailAddress]` | El texto tiene formato de correo electrónico válido | `CorreoProveedor` | "Ingrese un correo electrónico válido (ejemplo@dominio.com)." |
| `[RegularExpression(patrón)]` | El texto cumple un patrón exacto (expresión regular) | `Codigo` (`^[A-Z0-9]+$`: solo mayúsculas y números) | "El código solo admite letras mayúsculas y números, sin espacios." |
| `[Display(Name = "...")]` | No valida nada: define el nombre amigable que se muestra en `<label>` y en los mensajes de error | Todos | — |

Todos los mensajes de error se definieron en español directamente en el
atributo (`ErrorMessage = "..."`), en `Semana9/Models/Producto.cs`.

Además, hay una regla de negocio que **no** es un Data Annotation porque
depende del estado de la lista en memoria (no de un valor fijo ni del
propio campo): `Codigo` debe ser único. Esta se valida a mano en el
controlador con `ModelState.AddModelError`.

## Dónde mirar en el código

- Reglas de validación y mensajes: `Semana9/Models/Producto.cs`
- Lista en memoria y regla de código único: `Semana9/Models/ProductoRepository.cs`
- Flujo de `ModelState` y Post-Redirect-Get: `Semana9/Controllers/ProductoController.cs`
- `ValidationSummary` + `ValidationMessageFor` por campo + activación de
  `_ValidationScriptsPartial`: `Semana9/Views/Producto/Create.cshtml`
- Documentación técnica detallada: `Semana9/docs/backend/` y `Semana9/docs/frontend/`

## Demostración en vivo sugerida

1. Mostrar el formulario funcionando normal: datos inválidos rechazados al
   instante por JavaScript (validación de cliente).
2. Desactivar JavaScript en el navegador (o quitar la carga de
   `_ValidationScriptsPartial` de la vista) y repetir el envío con los
   mismos datos inválidos.
3. El formulario ahora sí se envía al servidor, pero `ModelState.IsValid`
   sigue siendo `false` → la página se recarga con los mismos errores,
   mostrando que el rechazo no dependía del navegador.
4. Repetir con un `Codigo` que ya existe en la lista, para mostrar la
   regla de negocio que corre exclusivamente en el servidor.
