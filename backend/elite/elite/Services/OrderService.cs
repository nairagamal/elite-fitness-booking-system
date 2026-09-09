using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Microsoft.EntityFrameworkCore;

namespace elite.Services
{
    // Services/OrderService.cs
    public class OrderService : IOrderService
    {
        private readonly GymDbContext _context;
        private readonly IPromotionService _promotionService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(GymDbContext context, IPromotionService promotionService, ILogger<OrderService> logger)
        {
            _context = context;
            _promotionService = promotionService;
            _logger = logger;
        }

        public async Task<OrderDto> CreateOrderAsync(OrderCreateDto orderCreateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users.FindAsync(orderCreateDto.UserId);
                if (user == null) throw new ArgumentException("User not found");

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in orderCreateDto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null) throw new ArgumentException($"Product with ID {item.ProductId} not found");

                    if (product.StockQuantity < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for product: {product.Name}");

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = product.Price
                    };

                    totalAmount += product.Price * item.Quantity;
                    orderItems.Add(orderItem);

                    // Update product stock
                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);
                }

                // Apply promotion if provided
                decimal discount = 0;
                if (!string.IsNullOrEmpty(orderCreateDto.PromotionCode))
                {
                    try
                    {
                        // Pass userId to promotion service
                        discount = await _promotionService.CalculateDiscountAsync(
                            orderCreateDto.PromotionCode, totalAmount, orderCreateDto.UserId);
                        totalAmount -= discount;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to apply promotion code: {Code}", orderCreateDto.PromotionCode);
                    }
                }

                var order = new Order
                {
                    UserId = orderCreateDto.UserId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = "Completed"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order items with order ID
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                    _context.OrderItems.Add(item);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order");
                throw;
            }
        }


        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) throw new ArgumentException("Order not found");

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = order.User.Name,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserName = o.User.Name,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserName = o.User.Name,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) throw new ArgumentException("Order not found");

            order.Status = status;
            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null) throw new ArgumentException("Order not found");

                // Restore product stock
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _context.Products.Update(product);
                    }
                }

                order.Status = "Cancelled";
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling order");
                throw;
            }
        }

        public async Task<decimal> ApplyPromotionAsync(string code, decimal orderAmount, int userId)
        {
            return await _promotionService.CalculateDiscountAsync(code, orderAmount, userId);
        }

        public Task<decimal> ApplyPromotionAsync(string code, decimal orderAmount)
        {
            throw new NotImplementedException();
        }
    }
}
