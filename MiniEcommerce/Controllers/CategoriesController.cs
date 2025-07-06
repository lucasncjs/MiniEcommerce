using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniEcommerce.Data;
using MiniEcommerce.Models;

[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Categories.ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        Console.WriteLine("📩 POST recibido para crear categoría");

        if (!ModelState.IsValid)
        {
            foreach (var kvp in ModelState)
            {
                foreach (var error in kvp.Value.Errors)
                {
                    Console.WriteLine($"❌ Campo: {kvp.Key} → Error: {error.ErrorMessage}");
                }
            }

            return View(category);
        }

        _context.Add(category);
        await _context.SaveChangesAsync();
        TempData["SuccessCategoria"] = "Categoría creada correctamente.";
        Console.WriteLine("✅ Categoría creada con nombre: " + category.Name);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            foreach (var kvp in ModelState)
            {
                foreach (var error in kvp.Value.Errors)
                {
                    Console.WriteLine($"❌ Campo: {kvp.Key} → Error: {error.ErrorMessage}");
                }
            }
            return View(category);
        }

        _context.Update(category);
        await _context.SaveChangesAsync();
        TempData["SuccessCategoria"] = "Categoría actualizada correctamente.";
        Console.WriteLine("✏️ Categoría editada: " + category.Name);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        if (category.Products.Any())
        {
            TempData["ErrorCategoria"] = "No se puede eliminar la categoría porque tiene productos asociados.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["SuccessCategoria"] = "Categoría eliminada correctamente.";
        Console.WriteLine("🗑️ Categoría eliminada: " + category.Name);

        return RedirectToAction(nameof(Index));
    }
}