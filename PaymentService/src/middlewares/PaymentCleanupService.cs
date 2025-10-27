using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using src.Data;
using Microsoft.EntityFrameworkCore;

public class PaymentCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentCleanupService> _logger;

    public PaymentCleanupService(IServiceScopeFactory scopeFactory, ILogger<PaymentCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment cleanup background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                // Tìm transaction pending quá 10 giây
                var expiredPayments = await dbContext.Payments
                    .Where(p => p.Status == "pending" &&
                                p.CreateAt <= DateTime.UtcNow.AddMinutes(-15))
                    .ToListAsync();

                if (expiredPayments.Any())
                {
                    foreach (var payment in expiredPayments)
                    {
                        payment.Status = "cancel";
                        payment.UpdateAt = DateTime.UtcNow;
                    }

                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"[TEST] Auto-cancel {expiredPayments.Count} expired payments (>10s).");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during payment cleanup.");
            }

            // Giảm thời gian quét xuống 10 giây cho test
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
