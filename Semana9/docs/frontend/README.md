# Frontend

Vistas Razor (`.cshtml`) del proyecto MVC. Usan Tag Helpers
(`asp-for`, `asp-validation-for`, `asp-validation-summary`, `asp-action`)
en vez de los antiguos helpers `Html.XxxFor`; son la version moderna de lo
que el caso describe como "ValidationMessageFor" / "ValidationSummary".

## Unidades

| Unidad | Doc | Archivo |
|---|---|---|
| Producto / Create | [producto-create.md](./producto-create.md) | `Views/Producto/Create.cshtml` |
| Producto / Index | [producto-index.md](./producto-index.md) | `Views/Producto/Index.cshtml` |

`Views/Home/*` y `Views/Shared/Error.cshtml` son el scaffold por defecto
de la plantilla MVC y no se documentan como unidades propias.

## Convenciones compartidas

- **Layout**: `Views/Shared/_Layout.cshtml` define el `<head>`, la barra
  de navegacion (con el enlace a "Productos" agregado para este caso) y el
  punto de extension `@await RenderSectionAsync("Scripts", required: false)`
  donde cada vista puede inyectar sus propios `<script>`.
- **Activacion de validacion de cliente**: `Views/Shared/_ValidationScriptsPartial.cshtml`
  carga `jquery.validate.min.js` + `jquery.validate.unobtrusive.min.js`
  (librerias en `wwwroot/lib/jquery-validation*`). Solo `Create.cshtml` lo
  incluye, porque es la unica vista con un formulario que valida.
- **Una sola fuente de texto para etiquetas**: los `[Display(Name = "...")]`
  del modelo `Producto` (ver `docs/backend/producto.md`) alimentan tanto
  los `<label asp-for>` de `Create` como los `@Html.DisplayNameFor` de
  `Index` — cambiar el texto en el modelo lo cambia en ambas vistas.

## Known gaps

- No hay tests de UI/end-to-end automatizados; toda la verificacion de
  esta iteracion fue manual via `curl` contra el servidor en ejecucion.
