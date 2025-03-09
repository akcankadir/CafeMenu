using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeMenu.Models;
using CafeMenu.Data;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using CafeMenu.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace CafeMenu.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IProductCacheService _cacheService;

        public ProductController(
            ApplicationDbContext context,
            IWebHostEnvironment hostEnvironment,
            IProductCacheService cacheService)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _cacheService = cacheService;
        }

        // GET: Product
        public async Task<IActionResult> Index(int page = 1)
        {
            int tenantId = GetTenantId();
            var products = await _cacheService.GetAllProductsAsync(tenantId, page, 10);
            return View(products);
        }

        // GET: Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();

            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                int tenantId = GetTenantId();
                product.TenantId = tenantId;
                product.CreatedDate = DateTime.Now;
                product.CreatorUserId = GetUserId();
                
                if (imageFile != null)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    product.ImagePath = "/images/products/" + uniqueFileName;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                
                await _cacheService.CacheProductAsync(product, tenantId);
                
                TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();

            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            int tenantId = GetTenantId();
            var product = await _cacheService.GetProductAsync(id, tenantId);
            if (product == null)
            {
                return NotFound();
            }
            
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => !c.IsDeleted), "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageFile)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int tenantId = GetTenantId();
                    var existingProduct = await _context.Products.FindAsync(id);
                    
                    if (imageFile != null)
                    {
                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(existingProduct.ImagePath))
                        {
                            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingProduct.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Yeni resmi kaydet
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        Directory.CreateDirectory(uploadsFolder);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        existingProduct.ImagePath = "/images/products/" + uniqueFileName;
                    }

                    existingProduct.ProductName = product.ProductName;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.Price = product.Price;

                    await _context.SaveChangesAsync();
                    
                    // Cache'i güncelle
                    await _cacheService.RemoveProductFromCacheAsync(id, tenantId);
                    await _cacheService.CacheProductAsync(existingProduct, tenantId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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

            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int tenantId = GetTenantId();
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                
                await _cacheService.RemoveProductFromCacheAsync(id, tenantId);
                await _cacheService.CacheProductAsync(product, tenantId);
                
                TempData["SuccessMessage"] = "Ürün başarıyla silindi.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        private int GetTenantId()
        {
            return HttpContext.Items.ContainsKey("TenantId") ? (int)HttpContext.Items["TenantId"] : 0;
        }

        private int GetUserId()
        {
            return User.Identity.IsAuthenticated ? int.Parse(User.FindFirst("UserId")?.Value ?? "0") : 0;
        }
    }
} 