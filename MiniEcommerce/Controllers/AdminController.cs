using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniEcommerce.Data;

namespace MiniEcommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Agrupar ventas por año/mes (sin formatear aún)
            var ventasPorMesRaw = await _context.Orders
                .Where(o => o.CreatedAt >= DateTime.Now.AddMonths(-6))
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    Año = g.Key.Year,
                    Mes = g.Key.Month,
                    Total = g.Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            // Ahora sí formateamos en memoria
            var ventasPorMes = ventasPorMesRaw
                .Select(v => new
                {
                    Mes = $"{v.Mes}/{v.Año}",
                    Total = v.Total
                })
                .OrderBy(v => v.Mes)
                .ToList();

            ViewBag.VentasPorMesLabels = ventasPorMes.Select(v => v.Mes).ToArray();
            ViewBag.VentasPorMesData = ventasPorMes.Select(v => v.Total).ToArray();

            var totalPedidos = await _context.Orders.CountAsync();
            var totalUsuarios = await _context.Users.CountAsync();
            var ingresosTotales = await _context.Orders.SumAsync(o => o.TotalAmount);
            var productosBajoStock = await _context.Products
                .Where(p => p.Stock < 5)
                .ToListAsync();

            var productoMasVendido = await _context.OrderItems
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new
                {
                    Nombre = g.Key,
                    TotalVendido = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(g => g.TotalVendido)
                .FirstOrDefaultAsync();

            ViewBag.Metricas = new
            {
                TotalPedidos = totalPedidos,
                TotalUsuarios = totalUsuarios,
                IngresosTotales = ingresosTotales,
                BajoStock = productosBajoStock,
                ProductoTop = productoMasVendido
            };

            return View();
        }
    }
}