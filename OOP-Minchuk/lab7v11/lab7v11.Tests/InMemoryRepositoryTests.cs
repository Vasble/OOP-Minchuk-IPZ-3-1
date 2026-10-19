using lab7v11;

namespace lab7v11.Tests;

public class InMemoryRepositoryTests
{
    private static Movie NewMovie(string title = "Inception", string director = "Nolan", int year = 2010) =>
        new() { Title = title, Director = director, ReleaseYear = year };


    [Fact]
    public void Add_NewItem_AssignsIdAndStoresIt()
    {
        var repo = new InMemoryRepository<Movie>();
        var movie = NewMovie();

        repo.Add(movie);

        Assert.NotEqual(0, movie.Id);
        Assert.Single(repo.GetAll());
        Assert.Same(movie, repo.GetById(movie.Id));
    }

    [Fact]
    public void Add_MultipleItems_AssignsUniqueIds()
    {
        var repo = new InMemoryRepository<Movie>();
        var a = NewMovie("A");
        var b = NewMovie("B");

        repo.Add(a);
        repo.Add(b);

        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void Add_DuplicateId_Throws()
    {
        var repo = new InMemoryRepository<Movie>();
        repo.Add(new Movie { Id = 5, Title = "A" });

        Assert.Throws<InvalidOperationException>(() => repo.Add(new Movie { Id = 5, Title = "B" }));
    }

    [Fact]
    public void Add_Null_Throws()
    {
        var repo = new InMemoryRepository<Movie>();

        Assert.Throws<ArgumentNullException>(() => repo.Add(null!));
    }


    [Fact]
    public void GetById_Existing_ReturnsItem()
    {
        var repo = new InMemoryRepository<Movie>();
        var movie = NewMovie("Matrix", "Wachowski", 1999);
        repo.Add(movie);

        var result = repo.GetById(movie.Id);

        Assert.NotNull(result);
        Assert.Equal("Matrix", result!.Title);
        Assert.Equal("Wachowski", result.Director);
        Assert.Equal(1999, result.ReleaseYear);
    }

    [Fact]
    public void GetById_NonExisting_ReturnsNull()
    {
        var repo = new InMemoryRepository<Movie>();
        repo.Add(NewMovie());

        Assert.Null(repo.GetById(999));
    }


    [Fact]
    public void GetAll_EmptyRepository_ReturnsEmpty()
    {
        var repo = new InMemoryRepository<Movie>();

        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void GetAll_ReturnsAllAddedItems()
    {
        var repo = new InMemoryRepository<Movie>();
        var a = NewMovie("A");
        var b = NewMovie("B");
        var c = NewMovie("C");
        repo.Add(a);
        repo.Add(b);
        repo.Add(c);

        var all = repo.GetAll().ToList();

        Assert.Equal(3, all.Count);
        Assert.Contains(a, all);
        Assert.Contains(b, all);
        Assert.Contains(c, all);
    }

    [Fact]
    public void Update_Existing_ReplacesData()
    {
        var repo = new InMemoryRepository<Movie>();
        var movie = NewMovie("Old", "Someone", 2000);
        repo.Add(movie);

        repo.Update(new Movie { Id = movie.Id, Title = "New", Director = "Other", ReleaseYear = 2020 });

        var updated = repo.GetById(movie.Id);
        Assert.NotNull(updated);
        Assert.Equal("New", updated!.Title);
        Assert.Equal("Other", updated.Director);
        Assert.Equal(2020, updated.ReleaseYear);
        Assert.Single(repo.GetAll());
    }

    [Fact]
    public void Update_NonExisting_ThrowsKeyNotFound()
    {
        var repo = new InMemoryRepository<Movie>();
        repo.Add(NewMovie());

        Assert.Throws<KeyNotFoundException>(() => repo.Update(new Movie { Id = 999, Title = "X" }));
    }

    [Fact]
    public void Delete_Existing_RemovesItem()
    {
        var repo = new InMemoryRepository<Movie>();
        var a = NewMovie("A");
        var b = NewMovie("B");
        repo.Add(a);
        repo.Add(b);

        repo.Delete(a.Id);

        Assert.Null(repo.GetById(a.Id));
        Assert.Single(repo.GetAll());
        Assert.NotNull(repo.GetById(b.Id));
    }

    [Fact]
    public void Delete_NonExisting_ThrowsKeyNotFound()
    {
        var repo = new InMemoryRepository<Movie>();
        repo.Add(NewMovie());

        Assert.Throws<KeyNotFoundException>(() => repo.Delete(999));
        Assert.Single(repo.GetAll());
    }
}
