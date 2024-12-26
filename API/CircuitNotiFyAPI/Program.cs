using CircuitNotiFyAPI.Services.Intrerfaces;
using CircuitNotiFyAPI.Services;
using CircuitNotiFyAPI.Helpers;
using CircuitNotiFyAPI.Model;
using Microsoft.AspNetCore.SignalR;
using Polly.Extensions.Http;
using Polly;
using CircuitNotiFyAPI.Helpers.Interfaces;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddTransient<PollyPoliciesHelper>();

// Registering the PayPal Policy
builder.Services.AddKeyedSingleton<IAsyncPolicy<HttpResponseMessage>>("PayPalPolicy", (provider, _) =>
{
    var pollyHelper = provider.GetRequiredService<PollyPoliciesHelper>();
    var hubContext = provider.GetRequiredService<IHubContext<NotificationHub, INotificationHub>>();
    return pollyHelper.GetCircuitBreakerPolicy("PayPal", hubContext);
});

// Registering the Stripe Policy
builder.Services.AddKeyedSingleton<IAsyncPolicy<HttpResponseMessage>>("StripePolicy", (provider, _) =>
{
    var pollyHelper = provider.GetRequiredService<PollyPoliciesHelper>();
    var hubContext = provider.GetRequiredService<IHubContext<NotificationHub, INotificationHub>>();
    return pollyHelper.GetCircuitBreakerPolicy("Stripe", hubContext);
});

// Adding Paypal Policy to the Paypal service
builder.Services.AddHttpClient<IPaymentService,PayPalPaymentService>((serviceProvider, client) =>
{
    client.BaseAddress = new Uri("https://httpbin.org/status/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddPolicyHandler((provider, _) =>
{
    var policy = provider.GetKeyedService<IAsyncPolicy<HttpResponseMessage>>("PayPalPolicy");
    return policy;
});

// Adding Stripe Policy to the Stripe service
builder.Services.AddHttpClient<IPaymentService, StripePaymentService>((serviceProvider, client) =>
{
    client.BaseAddress = new Uri("https://httpbin.org/status/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddPolicyHandler((provider, _) =>
{
    var policy = provider.GetKeyedService<IAsyncPolicy<HttpResponseMessage>>("StripePolicy");
    return policy;
});



var app = builder.Build();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
