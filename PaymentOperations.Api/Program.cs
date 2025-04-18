using PaymentOperations.Api.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://192.168.121.147:8080") // Replace with your frontend's address
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentPlanService, PaymentPlanService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();

// Configure Stripe
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Add Stripe services
builder.Services.AddScoped<PaymentIntentService>();
builder.Services.AddScoped<PaymentMethodService>();
builder.Services.AddScoped<RefundService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<PriceService>();
builder.Services.AddScoped<ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS
app.UseCors("AllowSpecificOrigin"); // Apply the CORS policy

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
