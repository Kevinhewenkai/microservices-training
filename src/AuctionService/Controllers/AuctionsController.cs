using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController: ControllerBase
{
    private readonly IAuctionRepository _repo;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(IAuctionRepository repo, IMapper mapper, 
        IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
        _repo = repo;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        return await _repo.GetAuctionsAsync(date);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _repo.GetAuctionByIdAsync(id);

        if (auction == null) return NotFound();
 
        return _mapper.Map<AuctionDto>(auction);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = User.Identity.Name;

        _repo.AddAuction(auction);

        var newAuction = _mapper.Map<AuctionDto>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _repo.SaveChangesAsync();
        if (!result) return BadRequest("Could not save changes to DB");

        return CreatedAtAction(nameof(GetAuctionById),
            new {auction.Id}, newAuction );
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<AuctionDto>> UpdateAuction(Guid id, UpdateAuctionDTO updateAuctionDTO)
    {
        var auction = await _repo.GetAuctionEntityById(id);       

        if (auction == null) return NotFound();

        if (auction.Seller != User.Identity.Name) return Forbid();

        auction.Item.Make = updateAuctionDTO.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDTO.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDTO.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDTO.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDTO.Year ?? auction.Item.Year;

        // publish to bus
        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _repo.SaveChangesAsync();

        if (result) return Ok();

        return BadRequest("Problem save update change");
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _repo.GetAuctionEntityById(id);

        if (auction == null) return NotFound();

        if (auction.Seller != User.Identity.Name) return Forbid();
        _repo.RemoveAuction(auction);

        // publish to bus
        await _publishEndpoint.Publish(new AuctionDeleted{Id = id.ToString()});

        var result = await _repo.SaveChangesAsync();

        if (result) return Ok();

        return BadRequest("Could not delete in DB");
    }
}
