using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;

    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    // When we get <AuctionCreated> Type item from the bus
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("----> Consuming Auction Created" + context.Message);

        var item = _mapper.Map<Item>(context.Message);

        if (item.Model == "Foo") throw new ArgumentException("Cannot sell cars with name of foo");

        await item.SaveAsync();
    }
}
