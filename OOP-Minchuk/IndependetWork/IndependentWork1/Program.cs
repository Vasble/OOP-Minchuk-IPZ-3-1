using System;

class Program
{
    static void Main()
    {
        // mobilka
        Phone phone1 = new Phone("samsung", "s24", 40.000);

        Console.Write("Введіть заряд: ");
        int battery = int.Parse(Console.ReadLine()!);

        phone1.PowerBattery(battery);

        Console.WriteLine($"Заряд батареї {phone1.Battery}.");

        // tvarinka
        Animal sobaka = new Animal("bobik", 5);

        Console.Write("Введіть звук: ");
        string sound = Console.ReadLine()!;

        sobaka.VoiceGo(sound);

        Console.WriteLine($"Каже {sobaka.Voice}");

        // hero
        Character vadim = new Character("Вадим", 34);

        Console.Write("Введіть HP: ");
        int health = int.Parse(Console.ReadLine()!);

        Console.Write("Введіть damage: ");
        int damage = int.Parse(Console.ReadLine()!);

        vadim.CharacterDo(health);
        vadim.Damage = damage;

        Console.WriteLine($"Має HP {vadim.Health}");
        Console.WriteLine($"Damage {vadim.Damage}");
    }
}

class Phone
{
    private string _brand;
    private string _model;
    private double _price;
    private int _battery = 60;

    public int Battery
    {
        get { return _battery; }
        set { _battery = value; }
    }

    public Phone(string brand, string model, double price)
    {
        _brand = brand;
        _model = model;
        _price = price;
    }

    public int PowerBattery(int power)
    {
        return _battery += power;
    }
}

class Animal
{
    private string _name;
    private int _age;
    private string _voice = "";

    public string Voice
    {
        get { return _voice; }
        set { _voice = value; }
    }

    public Animal(string name, int age)
    {
        _name = name;
        _age = age;
    }

    public string VoiceGo(string sound)
    {
        return _voice += sound;
    }
}

class Character
{
    private string _name;
    private int _age;
    private int _health;
    private int _damage;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public int Health
    {
        get { return _health; }
        set { _health = value; }
    }

    public int Damage
    {
        get { return _damage; }
        set { _damage = value; }
    }

    public Character(string name, int age)
    {
        _name = name;
        _age = age;
    }

    public int CharacterDo(int health)
    {
        return Health += health;
    }
}