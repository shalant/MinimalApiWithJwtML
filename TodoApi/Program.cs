var app = WebApplication.CreateBuilder(args).Build();
app.MapGet("/", () => "Hello from Minimal API");
app.Run();

record Item(int id, string title, bool IsCompleted);

class ItemRepository
{
    private Dictionary<int, Item> items = new Dictionary<int, Item>();

    public ItemRepository() 
    {
        var item1 = new Item(1, "Go to the gym", false);
        var item2 = new Item(1, "Drink water", true);
        var item3 = new Item(1, "Watch TV", false);

        items.Add(item1.id, item1);
        items.Add(item2.id, item2);
        items.Add(item3.id, item3);
    }

    public IEnumerable<Item> GetAll() => items.Values;

    public Item GetById(int id) => items[id];

    public void Add(Item item) => items.Add(item.id, item);

    public void Update(Item item) => items[item.id] = item;

    public void Delete(int id) => items.Remove(id);
}