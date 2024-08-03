using AuctionService;
using AuctionService.Controllers;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit.Sdk;

namespace AuctionServices.UnitTests;

public class AuctionControllerTest
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;
    public AuctionControllerTest()
    {
        _fixture = new Fixture();
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(mc => 
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        _mapper = new Mapper(mockMapper);
        _controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
        // for the [Authorize] methods in the controller
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext{User = Helpers.GetClaimsPrincipal()}
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_Returns10Auctions()
    {
        // arrange
        var auction = _fixture.CreateMany<AuctionDto>(10).ToList();
        _auctionRepo.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(auction);

        // act
        var result = await _controller.GetAllAuctions(null);

        // assert
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
    {
        // arrange
        var auction = _fixture.Create<AuctionDto>();
        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auction);

        // act
        var result = await _controller.GetAuctionById(auction.Id);

        // assert
        Assert.Equal(auction.Make, result.Value.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithInValidGuid_ReturnsNotFound()
    {
        // arrange
        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        // act
        var result = await _controller.GetAuctionById(Guid.NewGuid());

        // assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtAction()
    {
        // arrange
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.CreateAuction(auction);
        // need the result.Result for the <ActionResult> in the controller
        var createdResult = result.Result as CreatedAtActionResult;

        // assert
        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDto>(createdResult.Value);
    }

    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        // arrange
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        // act
        var result = await _controller.CreateAuction(auction);

        // assert
        Assert.IsType<BadRequestObjectResult>(result.Result); 
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "test";
        var updateAuctionDto = _fixture.Create<UpdateAuctionDTO>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

        // assert
        Assert.IsType<OkResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        // auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "wrong user";

        var updateAuctionDto = _fixture.Create<UpdateAuctionDTO>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        // _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

        // assert
        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        // auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        var updateAuctionDto = _fixture.Create<UpdateAuctionDTO>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);
        // _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

        // assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        // auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
                
        auction.Seller = "test";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
        _auctionRepo.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));

        // act
        var result = await _controller.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _controller.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<NotFoundResult>(result);    
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        // auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();        
        auction.Seller = "Wrong user";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        // _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
        // _auctionRepo.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));

        // act
        var result = await _controller.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<ForbidResult>(result);
    }
}