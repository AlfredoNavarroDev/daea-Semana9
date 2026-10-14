using System.ComponentModel.DataAnnotations;

namespace Semana9.Models
{
    /// <summary>
    /// Representa un producto del almacen.
    /// Cada propiedad lleva Data Annotations para validar los datos
    /// tanto en el cliente (JavaScript, via jquery-validation-unobtrusive)
    /// como en el servidor (ModelState, siempre se ejecuta sin importar el cliente).
    /// </summary>
    public class Producto
    {
        // --- Codigo ---
        // [Required]: el campo no puede quedar vacio.
        // [StringLength]: limita el numero de caracteres (min y max).
        // [RegularExpression]: obliga a un formato exacto (letras+numeros).
        // [Display]: nombre amigable para mostrar en las vistas y mensajes.
        [Required(ErrorMessage = "El codigo es obligatorio.")]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "El codigo debe tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "El codigo solo admite letras mayusculas y numeros, sin espacios.")]
        [Display(Name = "Codigo")]
        public string Codigo { get; set; } = string.Empty;

        // --- Nombre ---
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
        [Display(Name = "Nombre del producto")]
        public string Nombre { get; set; } = string.Empty;

        // --- Precio ---
        // [Range]: define un rango numerico valido (minimo y maximo).
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 100000, ErrorMessage = "El precio debe estar entre {1} y {2}.")]
        [Display(Name = "Precio (S/)")]
        public decimal Precio { get; set; }

        // --- Stock ---
        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, 100000, ErrorMessage = "El stock debe estar entre {1} y {2}.")]
        [Display(Name = "Stock disponible")]
        public int Stock { get; set; }

        // --- CorreoProveedor ---
        // [EmailAddress]: valida formato de correo electronico.
        [Required(ErrorMessage = "El correo del proveedor es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electronico valido (ejemplo@dominio.com).")]
        [Display(Name = "Correo del proveedor")]
        public string CorreoProveedor { get; set; } = string.Empty;
    }
}
