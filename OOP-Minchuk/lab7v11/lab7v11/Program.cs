using lab7v11;

IRepository<Movie> repo = new InMemoryRepository<Movie>();

repo.Add(new Movie { Title = "Inception", Director = "Christopher Nolan", ReleaseYear = 2010 });
repo.Add(new Movie { Title = "The Matrix", Director = "Lana Wachowski", ReleaseYear = 1999 });
repo.Add(new Movie { Title = "Pulp Fiction", Director = "Quentin Tarantino", ReleaseYear = 1994 });

Console.WriteLine("Усі фільми після додавання:");
foreach (var m in repo.GetAll()) Console.WriteLine("  " + m);

Console.WriteLine("\nGetById(2): " + repo.GetById(2));
Console.WriteLine("GetById(99): " + (repo.GetById(99)?.ToString() ?? "не знайдено"));

repo.Update(new Movie { Id = 2, Title = "The Matrix", Director = "Lilly & Lana Wachowski", ReleaseYear = 1999 });
Console.WriteLine("\nПісля Update(2):");
foreach (var m in repo.GetAll()) Console.WriteLine("  " + m);

repo.Delete(1);
Console.WriteLine("\nПісля Delete(1):");
foreach (var m in repo.GetAll()) Console.WriteLine("  " + m);

try
{
    repo.Delete(99);
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine("\nОчікувана помилка: " + ex.Message);
}
