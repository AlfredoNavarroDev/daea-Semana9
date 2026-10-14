using Microsoft.AspNetCore.Mvc;
using Semana9.Models;

namespace Semana9.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ProductoRepository _repositorio;

        // El repositorio se inyecta por constructor (Dependency Injection).
        // Esta registrado como Singleton en Program.cs, asi que es la MISMA
        // instancia -y la misma lista en memoria- en cada peticion.
        public ProductoController(ProductoRepository repositorio)
        {
            _repositorio = repositorio;
        }

        // GET: /Producto
        public IActionResult Index()
        {
            var productos = _repositorio.ObtenerTodos();
            return View(productos);
        }

        // GET: /Producto/Create
        // Solo muestra el formulario vacio.
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Producto/Create
        // [HttpPost] + [ValidateAntiForgeryToken]: solo acepta envios de formulario (no GET)
        // y exige el token anti-falsificacion que genera el helper asp-action en la vista.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Producto producto)
        {
            // 1) Validacion automatica por Data Annotations.
            // El model binder ya ejecuto cada [Required], [StringLength], [Range],
            // [EmailAddress] y [RegularExpression] del modelo Producto y, si alguno
            // falla, agrego su mensaje de error a ModelState ANTES de llegar aqui.

            // 2) Regla de negocio que NO se puede expresar con un atributo:
            // el codigo debe ser unico en la lista. Se valida a mano con
            // ModelState.AddModelError, asociado al campo "Codigo".
            if (_repositorio.ExisteCodigo(producto.Codigo))
            {
                ModelState.AddModelError(nameof(Producto.Codigo), "Ya existe un producto registrado con ese codigo.");
            }

            // 3) Si algo fallo (atributos o regla de negocio), ModelState.IsValid es false.
            // Se devuelve la MISMA vista con el MISMO objeto 'producto', para que el
            // usuario no pierda lo que ya habia escrito.
            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            // 4) Todo valido: se guarda en la lista en memoria y se redirige
            // (patron Post-Redirect-Get, evita reenvios duplicados al refrescar).
            _repositorio.Agregar(producto);
            return RedirectToAction(nameof(Index));
        }
    }
}
