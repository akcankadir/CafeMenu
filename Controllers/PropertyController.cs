using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeMenu.Models;
using CafeMenu.Data;
using System.Linq;

namespace CafeMenu.Controllers
{
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropertyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Property
        public async Task<IActionResult> Index()
        {
            var properties = await _context.Properties
                .OrderBy(p => p.Key)
                .ToListAsync();
            return View(properties);
        }

        // GET: Property/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Property/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property)
        {
            if (ModelState.IsValid)
            {
                _context.Add(property);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(property);
        }

        // GET: Property/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }
            return View(property);
        }

        // POST: Property/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Property property)
        {
            if (id != property.PropertyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(property);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(property.PropertyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(property);
        }

        // POST: Property/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.PropertyId == id);
        }

        // GET: Property/AssignToProduct/5
        public async Task<IActionResult> AssignToProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var assignedProperties = await _context.ProductProperties
                .Where(pp => pp.ProductId == id)
                .Select(pp => pp.PropertyId)
                .ToListAsync();

            var availableProperties = await _context.Properties
                .Select(p => new
                {
                    p.PropertyId,
                    p.Key,
                    p.Value,
                    IsAssigned = assignedProperties.Contains(p.PropertyId)
                })
                .ToListAsync();

            ViewBag.Product = product;
            return View(availableProperties);
        }

        // POST: Property/AssignToProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToProduct(int productId, int[] selectedProperties)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Mevcut özellikleri temizle
            var existingProperties = _context.ProductProperties.Where(pp => pp.ProductId == productId);
            _context.ProductProperties.RemoveRange(existingProperties);

            // Yeni özellikleri ekle
            if (selectedProperties != null)
            {
                foreach (var propertyId in selectedProperties)
                {
                    _context.ProductProperties.Add(new ProductProperty
                    {
                        ProductId = productId,
                        PropertyId = propertyId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "Product", new { id = productId });
        }
    }
} 