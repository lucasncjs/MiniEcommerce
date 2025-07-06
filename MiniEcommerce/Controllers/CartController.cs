using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniEcommerce.Data;
using MiniEcommerce.Models;
using MiniEcommerce.ViewModels;

namespace MiniEcommerce.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CartController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: /Cart/Add/5
        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock <= 0)
            {
                TempData["Error"] = "El producto no está disponible.";
                return RedirectToAction("Index");
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == user.Id);

            if (cartItem != null)
            {
                if (cartItem.Quantity + 1 > product.Stock)
                {
                    TempData["Error"] = $"No se pueden agregar más unidades de '{product.Name}' por falta de stock.";
                    return RedirectToAction("Index");
                }
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    UserId = user.Id,
                    Quantity = 1
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove/5
        [HttpPost]
        public async Task<IActionResult> Remove(int itemId)
        {
            var cartItem = await _context.CartItems.FindAsync(itemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/Update
        [HttpPost]
        public async Task<IActionResult> Update(Dictionary<int, UpdateCartItemViewModel> items)
        {
            var user = await _userManager.GetUserAsync(User);

            foreach (var entry in items.Values)
            {
                var cartItem = await _context.CartItems
                    .Include(c => c.Product)
                    .FirstOrDefaultAsync(c => c.Id == entry.CartItemId && c.UserId == user.Id);

                if (cartItem != null)
                {
                    var newQty = entry.Quantity;

                    if (newQty > cartItem.Product.Stock)
                    {
                        ModelState.AddModelError(string.Empty, $"La cantidad para '{cartItem.Product.Name}' supera el stock disponible.");
                        continue;
                    }

                    cartItem.Quantity = newQty;
                }
            }

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Carrito actualizado correctamente.";
            }
            else
            {
                TempData["Error"] = "Algunos ítems no se pudieron actualizar por falta de stock.";
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/ConfirmarCompra
        [HttpPost]
        public async Task<IActionResult> ConfirmarCompra()
        {
            var user = await _userManager.GetUserAsync(User);

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction("Index");
            }

            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.Stock)
                {
                    TempData["Error"] = $"No hay suficiente stock de '{item.Product.Name}' para confirmar la compra.";
                    return RedirectToAction("Index");
                }
            }

            var order = new Order
            {
                UserId = user.Id,
                CreatedAt = DateTime.Now,
                Items = new List<OrderItem>()
            };

            decimal total = 0;

            foreach (var item in cartItems)
            {
                var product = item.Product;
                product.Stock -= item.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                total += item.Quantity * product.Price;
                order.Items.Add(orderItem);
            }

            order.TotalAmount = total;

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            TempData["Success"] = "¡Compra confirmada! Tu pedido fue registrado con éxito.";
            return RedirectToAction("Gracias");
        }

        // GET: /Cart/Gracias
        public async Task<IActionResult> Gracias()
        {
            var user = await _userManager.GetUserAsync(User);

            var lastOrder = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastOrder == null)
            {
                TempData["Error"] = "No se encontró ningún pedido reciente.";
                return RedirectToAction("Index");
            }

            return View(lastOrder);
        }
    }
}