# Producto / Create

Formulario para registrar un producto nuevo. Es la vista central del Caso
3: muestra errores de validacion por campo y un resumen al inicio, y
conserva lo que el usuario escribio si el servidor rechaza el envio.

**Files:** `Views/Producto/Create.cshtml`, `Views/Shared/_ValidationScriptsPartial.cshtml`.

## Behavior

- `@model Semana9.Models.Producto`: la vista esta fuertemente tipada al
  modelo `Producto`, por eso los Tag Helpers `asp-for`/`asp-validation-for`
  pueden leer sus Data Annotations por reflexion.
- `<form asp-action="Create" method="post">`: el Tag Helper `asp-action`
  genera el `action` del formulario y, automaticamente, un campo oculto
  `__RequestVerificationToken` (requerido porque el POST en
  `ProductoController.Create` tiene `[ValidateAntiForgeryToken]`).
- `<div asp-validation-summary="All" class="text-danger"></div>` al
  **inicio** del formulario: renderiza un `<ul>` con **todos** los mensajes
  de `ModelState` — tanto los generados por los atributos del modelo como
  el error manual de "codigo duplicado" que agrega el controlador. Es el
  equivalente moderno (Tag Helper) de `@Html.ValidationSummary()`.
- Por cada campo (`Codigo`, `Nombre`, `Precio`, `Stock`,
  `CorreoProveedor`) se repite el mismo patron de 3 lineas:
  1. `<label asp-for="Campo">` — usa el texto de `[Display(Name = "...")]`
     del modelo, no el nombre de la propiedad en C#.
  2. `<input asp-for="Campo">` — genera `id`, `name`, `value` (con el dato
     que el usuario escribio, si el POST fallo) y los atributos
     `data-val-*` que describen cada regla de validacion para el cliente.
  3. `<span asp-validation-for="Campo">` — equivalente Tag Helper de
     `Html.ValidationMessageFor(m => m.Campo)`: muestra **solo** el error
     de ese campo especifico, pegado al input.
- Seccion `@section Scripts { ... Html.RenderPartialAsync("_ValidationScriptsPartial") }`:
  carga `jquery.validate.min.js` y
  `jquery.validate.unobtrusive.min.js` (ver
  `Views/Shared/_ValidationScriptsPartial.cshtml`). Estos scripts leen los
  atributos `data-val-*` generados por `asp-for` y validan en el navegador
  **antes** de enviar el formulario, mostrando los mismos mensajes sin
  recargar la pagina.
- La seccion `Scripts` se renderiza dentro de `Views/Shared/_Layout.cshtml`
  via `@await RenderSectionAsync("Scripts", required: false)`, despues de
  cargar `jquery.min.js` (jQuery es requisito de jquery-validation).

## Validation

Toda la validacion de **campo individual** mostrada aqui viene de las
Data Annotations del modelo (ver tabla completa en
`docs/backend/producto.md`); esta vista no agrega reglas propias, solo las
refleja via `asp-for`/`asp-validation-for`.

Cliente vs. servidor — por que hacen falta los dos:

- **Cliente** (jquery.validate + unobtrusive): feedback instantaneo, sin
  ida y vuelta al servidor, mejor experiencia de usuario. Pero corre
  JavaScript en el navegador del usuario, que el usuario controla por
  completo.
- **Servidor** (`ModelState.IsValid` en `ProductoController.Create`):
  **unica fuente de verdad**. Si se quita `_ValidationScriptsPartial` de
  esta vista, o el usuario desactiva JavaScript, el formulario se envia
  igual por POST normal y el controlador sigue evaluando `ModelState` —
  los datos invalidos se siguen rechazando, solo que sin feedback
  inmediato en pantalla (el usuario ve la pagina recargarse con los
  errores). Esto es lo que la demostracion en vivo del caso busca probar.
- La regla de negocio de codigo duplicado **no existe en el cliente**: no
  hay forma de que JavaScript sepa que codigos ya estan en la lista en
  memoria del servidor sin otra llamada HTTP. Por eso ese error solo puede
  aparecer despues de un roundtrip real al servidor (visible en el
  `ValidationSummary` tras el POST).

## Tests

No hay tests automatizados de UI/cliente. Verificado manualmente: POST
invalido devuelve `200` con `ValidationSummary` + los `span` de cada campo
poblados; POST con `Codigo` repetido devuelve el error de negocio; POST
valido redirige a `/Producto`. Ver el detalle exacto en la seccion "Tests"
de `docs/backend/producto.md`.
