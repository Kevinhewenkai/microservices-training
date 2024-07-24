

using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _service;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider service)
    {
        _logger = logger;
        _service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting check for finished auctions");
        stoppingToken.Register(() => _logger.LogInformation("===> Auction Checking is stopping"));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuction(stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CheckAuction(CancellationToken stoppingToken)
    {
        var finishedAuctions = await DB.Find<Auction>()
            .Match(x => x.AuctionEnd <= DateTime.UtcNow)
            .Match(x => !x.Finished)
            .ExecuteAsync(stoppingToken);
    
        if (finishedAuctions.Count == 0) return;

        _logger.LogInformation($"Found {finishedAuctions.Count} auctions that have completed");

        using var scope = _service.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        foreach (var auction in finishedAuctions)
        {
            auction.Finished = true;
            await auction.SaveAsync(null, stoppingToken);

            var winningBid = await DB.Find<Bid>()
                .Match(x => x.AuctionId == auction.ID)
                .Match(b => b.BidStatus == BidStatus.Accepted)
                .Sort(x => x.Descending(b => b.Amount))
                .ExecuteFirstAsync(stoppingToken);

            await endpoint.Publish(new AuctionFinished
            {
                ItemSold = winningBid != null,
                AuctionId = auction.ID,
                Winner = winningBid?.Bidder,
                Amount = winningBid?.Amount,
                Seller = auction.Seller
            }, stoppingToken);
        }
    }
}
