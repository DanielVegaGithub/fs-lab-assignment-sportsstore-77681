using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SportsStore.Infrastructure;
using SportsStore.Models;

namespace SportsStore.Pages
{
    public class CartModel : PageModel
    {
        private readonly IStoreRepository repository;
        private readonly ILogger<CartModel> logger;

        public CartModel(IStoreRepository repo, Cart cartService, ILogger<CartModel> loggerService)
        {
            repository = repo;
            Cart = cartService;
            logger = loggerService;
        }

        public Cart Cart { get; set; }
        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl ?? "/";

            logger.LogInformation(
                "Cart page opened. CartItemCount: {CartItemCount}, CartTotal: {CartTotal}, ReturnUrl: {ReturnUrl}",
                Cart.Lines.Sum(l => l.Quantity),
                Cart.ComputeTotalValue(),
                ReturnUrl);
        }

        public IActionResult OnPost(long productId, string returnUrl)
        {
            Product? product = repository.Products
                .FirstOrDefault(p => p.ProductID == productId);

            if (product == null)
            {
                logger.LogWarning(
                    "Attempt to add product to cart failed. ProductId: {ProductId}, ReturnUrl: {ReturnUrl}",
                    productId,
                    returnUrl);

                return RedirectToPage(new { returnUrl = returnUrl });
            }

            Cart.AddItem(product, 1);

            logger.LogInformation(
                "Product added to cart. ProductId: {ProductId}, ProductName: {ProductName}, QuantityAdded: {QuantityAdded}, CartItemCount: {CartItemCount}, CartTotal: {CartTotal}",
                product.ProductID,
                product.Name,
                1,
                Cart.Lines.Sum(l => l.Quantity),
                Cart.ComputeTotalValue());

            return RedirectToPage(new { returnUrl = returnUrl });
        }

        public IActionResult OnPostRemove(long productId, string returnUrl)
        {
            CartLine? line = Cart.Lines.FirstOrDefault(cl => cl.Product.ProductID == productId);

            if (line == null)
            {
                logger.LogWarning(
                    "Attempt to remove product from cart failed. ProductId: {ProductId}, ReturnUrl: {ReturnUrl}",
                    productId,
                    returnUrl);

                return RedirectToPage(new { returnUrl = returnUrl });
            }

            Cart.RemoveLine(line.Product);

            logger.LogInformation(
                "Product removed from cart. ProductId: {ProductId}, ProductName: {ProductName}, CartItemCount: {CartItemCount}, CartTotal: {CartTotal}",
                line.Product.ProductID,
                line.Product.Name,
                Cart.Lines.Sum(l => l.Quantity),
                Cart.ComputeTotalValue());

            return RedirectToPage(new { returnUrl = returnUrl });
        }
    }
}