using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using BusinessObject.Models;
using System.Text.Json.Serialization;
using BusinessObject.Mail;
using VanillaCakeStoreWebAPI.Mail;

static IEdmModel GetEdmModel()
{
    ODataConventionModelBuilder modelBuilder = new();
    modelBuilder.EntitySet<Product>("ProductOData");
    return modelBuilder.GetEdmModel();
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<VanillaCakeStoreContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("MyDB")
));

builder.Services.AddControllers().AddOData(options =>
{
    options.Expand().Select().Filter().Count().OrderBy().Expand().SetMaxTop(100);

    options.EnableQueryFeatures();
    var routeOptions = options.AddRouteComponents("odata", GetEdmModel()).RouteOptions;

    routeOptions.EnableQualifiedOperationCall = true;
    routeOptions.EnableKeyAsSegment = true;
    routeOptions.EnableKeyInParenthesis = false;
}).AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
                      policy.RequireClaim("Role", "1"));
    options.AddPolicy("Customer", policy =>
                      policy.RequireClaim("Role", "2"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Vanilla Cake Store WebAPI", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddCors();
builder.Services.AddSession();

builder.Services.AddOptions();                                         
var mailsettings = builder.Configuration.GetSection("MailSettings"); 
builder.Services.Configure<MailSettings>(mailsettings);
builder.Services.AddTransient<IEmailService, EmailService>();


// create mapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(builder => {
    builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});
app.UseODataBatching();
app.UseRouting();
app.UseHttpsRedirection();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
