using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService;
using SearchService.Consumers;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x => {
    // find consumers
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionDeletedConsumer>();
    // kebab case is a-b-c where a b c are word
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
    x.UsingRabbitMq((context, cfg) => {
        cfg.ReceiveEndpoint("search-auction-created", e => {
            // retry 5 times for 5 second interval if mongodb is done
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });
        cfg.ReceiveEndpoint("search-auction-updated", e => {
            // retry 5 times for 5 second interval if mongodb is done
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
        });
        cfg.ReceiveEndpoint("search-auction-deleted", e => {
            // retry 5 times for 5 second interval if mongodb is done
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<AuctionDeletedConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);   
    }
});

app.Run();

// If we use http sync method to fetch data, 
// we add this policy to refetch the data if error occurs
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));