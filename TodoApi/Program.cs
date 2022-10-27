using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlite(connectionString));

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
    if(existItem == null)
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
    if(existItem == null)
    {
        return Results.BadRequest();
    }

    db.Items.Remove(existItem);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/", () => "Hello from Minimal API");
app.Run();

class Item
{
    public int Id { get; set; }
    public string Title {get;set;}
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
    public DbSet<Item> Items {get;set;}

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
        
    }
}