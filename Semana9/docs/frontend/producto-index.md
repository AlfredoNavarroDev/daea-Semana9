# Producto / Index

Listado de todos los productos registrados (los 2 de la semilla mas los
que se hayan agregado via `Create` desde que el proceso arranco).

**Files:** `Views/Producto/Index.cshtml`.

## Behavior

- `@model IEnumerable<Semana9.Models.Producto>`: recibe la coleccion que
  `ProductoController.Index()` obtiene de `ProductoRepository.ObtenerTodos()`
  (ver `docs/backend/producto.md`). No hace ninguna llamada propia ni
  filtrado; muestra exactamente lo que el repositorio en memoria tiene en
  ese momento.
- Encabezados de tabla con `@Html.DisplayNameFor(m => m.Campo)`: toma el
  texto de `[Display(Name = "...")]` de cada propiedad del modelo
  `Producto`, igual que los `<label>` de la vista `Create` — un solo lugar
  (el modelo) define el texto visible para ambas vistas.
- `foreach` sobre `Model` renderiza una fila por producto; `Precio` se
  formatea con `.ToString("C2")` (moneda, 2 decimales).
- Enlace `<a asp-action="Create">` hacia el formulario de alta.

## Validation

No aplica: esta vista solo lee y muestra datos, no recibe entrada del
usuario.

## Tests

No hay tests automatizados. Verificado manualmente: tras un POST valido en
`Create`, el producto nuevo aparece en esta lista (ver "Tests" en
`docs/backend/producto.md`).
