namespace Semana9.Models
{
    /// <summary>
    /// "Base de datos" en memoria. Guarda los productos en una List&lt;Producto&gt;
    /// mientras la aplicacion esta en ejecucion (se reinicia si la app se reinicia).
    /// Se registra como Singleton en Program.cs para que la misma lista
    /// sea compartida por todas las peticiones HTTP.
    /// </summary>
    public class ProductoRepository
    {
        private readonly List<Producto> _productos = new()
        {
            new Producto { Codigo = "ABC123", Nombre = "Taladro percutor", Precio = 349.90m, Stock = 15, CorreoProveedor = "ventas@ferreteria.com" },
            new Producto { Codigo = "XYZ789", Nombre = "Casco de seguridad", Precio = 29.50m, Stock = 80, CorreoProveedor = "contacto@proveedor-epp.com" },
        };

        public IEnumerable<Producto> ObtenerTodos() => _productos;

        // Regla de negocio: el codigo es la clave unica del producto.
        public bool ExisteCodigo(string codigo) =>
            _productos.Any(p => p.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));

        public void Agregar(Producto producto) => _productos.Add(producto);
    }
}
