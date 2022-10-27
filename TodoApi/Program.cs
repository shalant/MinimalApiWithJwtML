using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlite(connectionString));

var securityScheme = new OpenApiSecurityScheme()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JWT authentication for MinimalAPI"
};

var securityRequirements = new OpenApiSecurityRequirement()
    {
        {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
        }

    };

var contactInfo = new OpenApiContact()
{
    Name = "Bob",
    Email = "Bob",
    Url = new Uri("https://mohammadlewand.com")
};

var license = new OpenApiLicense()
{
    Name = "Free license"
};

var info = new OpenApiInfo()
{
    Version = "V1",
    Title = "Todo List Api with JWT Authentication",
    Description = "Todo List Api with JWT Authentication",
    Contact = contactInfo,
    License = license
};

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", info);
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(securityRequirements);
});

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer (options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt: Issuer"],
        ValidAudience = builder.Configuration["Jwt: Audience"],
        ValidateAudience = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.getBytes(builder.Configuration["Jwt: Key"]))
    };
});

// builder.Services.AddSingleton<ItemRepository>();
var app = builder.Build();

app.MapGet("/items", async (ApiDbContext db) =>
{
    return await db.Items.ToListAsync();
});

app.MapPost("/items", async (ApiDbContext db, Item item) =>
{
    if (await db.Items.FirstOrDefaultAsync(x => x.Id == item.Id) != null)
    {
        return Results.BadRequest();
    }

    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/Items/{item.Id}", item);
});

app.MapGet("/items/{id}", async (ApiDbContext db, int Id) =>
{
    var item = await db.Items.FirstOrDefaultAsync(x => x.Id == Id);

    return item == null ? Results.NotFound() : Results.Ok(item);
});

app.MapPut("/items/{id}", async (ApiDbContext db, int id, Item item) =>
{
    var existItem = await db.Items.FirstOrDefaultAsync(x => x.Id == id);
    if (existItem == null)
    {
        return Results.BadRequest();
    }

    existItem.Title = item.Title;
    existItem.IsCompleted = item.IsCompleted;

    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapDelete("/items/{id}", async (ApiDbContext db, int Id) =>
{
    var existItem = await db.Items.FirstOrDefaultAsync(x => x.Id == Id);
    if (existItem == null)
    {
        return Results.BadRequest();
    }

    db.Items.Remove(existItem);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => "Hello from Minimal API");
app.Run();

class Item
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}

// record Item(int id, string title, bool IsCompleted);

// class ItemRepository
// {
//     private Dictionary<int, Item> items = new Dictionary<int, Item>();

//     public ItemRepository()
//     {
//         var item1 = new Item(1, "Go to the gym", false);
//         var item2 = new Item(2, "Drink water", true);
//         var item3 = new Item(3, "Watch TV", false);

//         items.Add(item1.id, item1);
//         items.Add(item2.id, item2);
//         items.Add(item3.id, item3);
//     }

//     public IEnumerable<Item> GetAll() => items.Values;

//     public Item GetById(int id)
//     {
//         if (items.ContainsKey(id))
//         {
//             return items[id];
//         }
//         return null;
//     }

//     public void Add(Item item) => items.Add(item.id, item);

//     public void Update(Item item) => items[item.id] = item;

//     public void Delete(int id) => items.Remove(id);
// }

class ApiDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {

    }
}