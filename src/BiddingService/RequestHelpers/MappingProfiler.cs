using AutoMapper;
using Contracts;

namespace BiddingService;

public class MappingProfiler: Profile
{
    public MappingProfiler()
    {
        CreateMap<Bid, BidDto>();
        CreateMap<Bid, BidPlaced>();
    }
}
