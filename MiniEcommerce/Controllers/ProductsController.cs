using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniEcommerce.Data;
using MiniEcommerce.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace MiniEcommerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var productosAgrupados = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            return View(productosAgrupados);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,ImageUrl,Stock,CategoryId")] Product product, IFormFile ImageFile)
        {
            Console.WriteLine("📩 POST recibido para crear producto");

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"❌ Campo: {kvp.Key} → {error.ErrorMessage}");
                    }
                }

                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
                return View(product);
            }

            // Imagen subida desde dispositivo
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                var extension = Path.GetExtension(ImageFile.FileName);
                var uniqueName = $"{fileName}_{Guid.NewGuid()}{extension}";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = $"/images/{uniqueName}";
            }
            else if (string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                product.ImageUrl = "wwwroot/images/default.jpg";
            }

            _context.Add(product);
            await _context.SaveChangesAsync();
            TempData["SuccessProducto"] = "✅ Producto creado exitosamente.";
            Console.WriteLine("✔️ Producto guardado con nombre: " + product.Name);

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,Stock,CategoryId")] Product product, IFormFile ImageFile)
        {
            if (id != product.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"❌ Campo: {kvp.Key} → {error.ErrorMessage}");
                    }
                }

                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", product.CategoryId);
                return View(product);
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                var extension = Path.GetExtension(ImageFile.FileName);
                var uniqueName = $"{fileName}_{Guid.NewGuid()}{extension}";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = $"/images/{uniqueName}";
            }
            else if (string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                product.ImageUrl = "/images/default.jpg";
            }

            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessProducto"] = "✅ Producto actualizado correctamente.";
                Console.WriteLine("✏️ Producto actualizado: " + product.Name);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessProducto"] = "🗑️ Producto eliminado correctamente.";
                Console.WriteLine("🗑️ Producto eliminado: " + product.Name);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        public async Task<IActionResult> FiltrarPorCategoria(int id)
        {
            var categoria = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null) return NotFound();

            return View("FiltradoPorCategoria", categoria);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(); // ⚠️ Esto causa el 404 si no hay coincidencia

            return View(product);
        }
    }
}