using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;

    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        var item = _mapper.Map<Item>(context.Message);

        var result = await DB.Update<Item>()
            .Match(a => a.ID == context.Message.Id)
            .ModifyOnly(a => new 
            { 
                a.Make,
                a.Model,
                a.Year, 
                a.Color,
                a.Mileage
            }, _mapper.Map<Item>(context.Message))
            .ExecuteAsync();

        if (!result.IsAcknowledged)
         throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb" );
    }
}
