using IndependentWork7;

namespace IndependentWork7.Tests;

public class CacheTests
{

    [Fact]
    public void Constructor_NewCache_IsEmpty()
    {
        var cache = new Cache<string, int>(3);

        Assert.Equal(0, cache.Count);
        Assert.Equal(3, cache.Capacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_NonPositiveCapacity_ThrowsArgumentOutOfRangeException(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Cache<string, int>(capacity));
    }


    [Fact]
    public void Add_SingleItem_ItemIsStored()
    {
        var cache = new Cache<string, int>(3);

        cache.Add("a", 1);

        Assert.True(cache.ContainsKey("a"));
        Assert.Equal(1, cache.Get("a"));
    }

    [Fact]
    public void Add_ExistingKey_UpdatesValueWithoutIncreasingCount()
    {
        var cache = new Cache<string, int>(3);
        cache.Add("a", 1);

        cache.Add("a", 100);

        Assert.Equal(100, cache.Get("a"));
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void Get_ExistingKey_ReturnsCorrectValue()
    {
        var cache = new Cache<int, string>(3);
        cache.Add(1, "one");
        cache.Add(2, "two");

        Assert.Equal("one", cache.Get(1));
        Assert.Equal("two", cache.Get(2));
    }

    [Fact]
    public void Get_MissingKey_ThrowsKeyNotFoundException()
    {
        var cache = new Cache<string, int>(3);
        cache.Add("a", 1);

        Assert.Throws<KeyNotFoundException>(() => cache.Get("missing"));
    }

    [Fact]
    public void Get_EmptyCache_ThrowsKeyNotFoundException()
    {
        var cache = new Cache<string, int>(3);

        Assert.Throws<KeyNotFoundException>(() => cache.Get("a"));
    }


    [Fact]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        var cache = new Cache<string, int>(2);
        cache.Add("a", 1);

        Assert.True(cache.ContainsKey("a"));
    }

    [Fact]
    public void ContainsKey_MissingKey_ReturnsFalse()
    {
        var cache = new Cache<string, int>(2);
        cache.Add("a", 1);

        Assert.False(cache.ContainsKey("b"));
    }


    [Fact]
    public void Count_AfterAddingItems_EqualsNumberOfItems()
    {
        var cache = new Cache<string, int>(5);

        cache.Add("a", 1);
        cache.Add("b", 2);
        cache.Add("c", 3);

        Assert.Equal(3, cache.Count);
    }

    [Fact]
    public void Count_AfterOverflow_NeverExceedsCapacity()
    {
        var cache = new Cache<int, int>(3);

        for (int i = 0; i < 10; i++)
        {
            cache.Add(i, i * i);
        }

        Assert.Equal(3, cache.Count);
    }


    [Fact]
    public void Add_WhenFull_RemovesOldestItem()
    {
        var cache = new Cache<string, int>(3);
        cache.Add("a", 1);
        cache.Add("b", 2);
        cache.Add("c", 3);

        cache.Add("d", 4);

        Assert.False(cache.ContainsKey("a"));
        Assert.True(cache.ContainsKey("b"));
        Assert.True(cache.ContainsKey("c"));
        Assert.True(cache.ContainsKey("d"));
        Assert.Equal(4, cache.Get("d"));
    }

    [Fact]
    public void Add_SeveralOverflows_RemovesItemsInInsertionOrder()
    {
        var cache = new Cache<string, int>(2);
        cache.Add("a", 1);
        cache.Add("b", 2);

        cache.Add("c", 3); 
        Assert.False(cache.ContainsKey("a"));
        Assert.True(cache.ContainsKey("b"));

        cache.Add("d", 4); // витісняє "b"
        Assert.False(cache.ContainsKey("b"));
        Assert.True(cache.ContainsKey("c"));
        Assert.True(cache.ContainsKey("d"));
    }

    [Fact]
    public void Add_WhenFull_GetOfEvictedKeyThrowsKeyNotFoundException()
    {
        var cache = new Cache<string, int>(1);
        cache.Add("a", 1);

        cache.Add("b", 2);

        Assert.Throws<KeyNotFoundException>(() => cache.Get("a"));
        Assert.Equal(2, cache.Get("b"));
    }

    [Fact]
    public void Get_DoesNotChangeEvictionOrder_FifoNotLru()
    {
        var cache = new Cache<string, int>(2);
        cache.Add("a", 1);
        cache.Add("b", 2);

        cache.Get("a");  
        cache.Add("c", 3); 

        Assert.False(cache.ContainsKey("a"));
        Assert.True(cache.ContainsKey("b"));
    }

    [Fact]
    public void Add_UpdatingExistingKey_DoesNotMoveItInQueue()
    {
        var cache = new Cache<string, int>(2);
        cache.Add("a", 1);
        cache.Add("b", 2);

        cache.Add("a", 10);
        cache.Add("c", 3);  

        Assert.False(cache.ContainsKey("a"));
        Assert.True(cache.ContainsKey("b"));
        Assert.True(cache.ContainsKey("c"));
    }
}
