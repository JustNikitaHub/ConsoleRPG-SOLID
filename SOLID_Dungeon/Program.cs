GameManager game = new();
game.Start();

//-----------------------------
/// <summary>
/// Интерфейс для всех персонажей
/// </summary>
public interface ICharacter
{
    string Name { get; }
    string Description { get; }
    int Damage { get; }
    int Health { get; set; }
    int MaxHealth { get; set; }
    int Mana {  get; set; }
    int MaxMana { get; set; }
    InventoryManager InventoryManager { get; }
    void Attack(ICharacter target);
    void TakeDamage(int amount);
    void Use(IConsumeble consumeble);
    void Heal(int heal);
    void HealMana(int healMana);
}

/// <summary>
/// Интерфейс для всей еды
/// </summary>
public interface IConsumeble
{
    string Name { get; }
    string Description { get; }
    int Weight { get; }
    void Use(ICharacter character);
    
}

/// <summary>
/// Интерфейс зелий
/// </summary>
public interface IPotion : IConsumeble
{
    PotionsTypes type { get; }
    List<IEffect> Effects { get; }
}

/// <summary>
/// Интерфейс для эффектов, которые могут быть применены к персонажу.
/// </summary>
public interface IEffect
{
    string Name { get; }
    string Description { get; }
    void Apply(ICharacter character);
}

public class HealingEffect : IEffect
{
    readonly int _heal;
    public string Name => "Восстановление здоровья";
    public string Description => $"Восстанавливает {_heal} здоровья";
    public void Apply(ICharacter character)
    {
        character.Heal(_heal);
    }

    public HealingEffect(int heal=0)
    {
        _heal = heal;
    }
}

public class ManaRestoringEffect : IEffect
{
    readonly int _mana;
    public string Name => "Восстановление маны";
    public string Description => $"Восстанавливает {_mana} маны";
    public void Apply(ICharacter character)
    {
        character.HealMana(_mana);
    }

    public ManaRestoringEffect(int mana = 0)
    {
        _mana = mana;
    }
}

/// <summary>
/// Абстракция зелья, как еды
/// </summary>
public class Potion: IPotion
{
    public string Name { get; } = "Зелье ";
    public string Description { get;  } = "";
    public List<IEffect> Effects { get; } = new ();
    public int Weight { get; set;}

    public PotionsTypes type { get; set; } //зелье знает свой тип
    public virtual void Use(ICharacter character)
    {
        foreach (IEffect effect in Effects)
        {
            effect.Apply(character);
        }
    }
    public Potion(List<IEffect> effects)
    {
        Effects = effects;
    }

    public Potion(string name, string description, List<IEffect> effects)
    {
        Name = name;
        Description = description;
        Effects = effects;
    }
}


/// <summary>
/// Управление инвентарем, добавление, удаление, посредник использования предметов
/// </summary>
public class InventoryManager
{
    string _devname = "inv1";
    byte _size=10;
    List<IConsumeble> _consumebles = new List<IConsumeble>(); //чисто для всего съестного

    public void ShowInventory()
    {
        for (byte i = 0; i < _consumebles.Count; i++)
        {
            Console.WriteLine($"[{i}] - {_consumebles[i].Name} '{_consumebles[i].Description}'");
        }
    }

    public void Use(byte index, ICharacter character)
    {
        _consumebles[index].Use(character);
        RemoveByIndex(index);
    }

    public void Add(IConsumeble consumeble)
    {
        _consumebles.Add(consumeble);
    }

    public void AddRange(List<IConsumeble> consumebles)
    {
        _consumebles.AddRange(consumebles);
    }

    public IConsumeble GetItemByIndex(byte index)
    {
        return _consumebles[index];
    }

    public void RemoveByIndex(int index)
    {
        _consumebles.RemoveAt(index);
    }

    public int CountItems()
    {
        return _consumebles.Count;
    }
}

/// <summary>
/// Общий класс для персонажей, есть инвентарь
/// </summary>
public abstract class Character : ICharacter
{
    
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int Mana { get; set; } = 10;
    public int MaxMana { get; set; } = 10;
    public int Damage { get; set; } = 10;
    public InventoryManager InventoryManager { get; }
    public virtual void Attack(ICharacter target)
    {
        Console.WriteLine($"{Name} атакует {target.Name} и наносит {Damage} урона!");
        target.TakeDamage(Damage);
    }

    public virtual void TakeDamage(int damage)
    { 
        Health -= damage;
        Console.WriteLine($"{Name} получает {damage} урона. Осталось здоровья: {Health}");
    }

    public virtual void Use(IConsumeble consumeble) //ну э, типо лучше через инвентарь, там и логика есть
    {
        consumeble.Use(this);
    }


    public virtual void Use(byte index)
    {
        IConsumeble consumeble = InventoryManager.GetItemByIndex(index);
        consumeble.Use(this);
    }

    public virtual void Heal(int heal)
    {
        Health = Math.Min(MaxHealth, Health+heal);
        Console.WriteLine($"{Name} восстановил {heal} здоровья!");
    }

    public virtual void HealMana(int healMana)
    {
        Mana = Math.Min(MaxMana, Health + healMana);
        Console.WriteLine($"{Name} восстановил {healMana} маны!");
    }

    public Character(string name="defname", string description="defdescr", int hp=100, int damage=20)
    {
        Name = name;
        Description = description;
        Health = hp;
        Damage = damage;
        InventoryManager = new InventoryManager();
    }
}

/// <summary>
/// Общий класс для игрока, предполагается ульта
/// </summary>
public abstract class Player : Character
{
    public string Nickname { get; protected set; }
    public abstract void SpecialAbility();
    public Player(string nickname="defplr")
    {
        Nickname = nickname;
    }
}

/// <summary>
/// Базовый воин, пример
/// </summary>
public class Warrior : Player
{
    //все еще может пить зелья
    public const int HpBonus = 10;
    public override void SpecialAbility()
    {
        Console.WriteLine("Яху!");
    }
    
    public Warrior(string nick="defWar1") : base(nick)
    {
        Health += HpBonus;
        Name = nick;
    }
}

/// <summary>
/// Базовый маг
/// </summary>
public class Mage : Player
{
    public override void SpecialAbility()
    {
        Console.WriteLine("ТрТр!");
    }

    public Mage(string nick = "defMage1") : base(nick) { Name = nick; }
}

/// <summary>
/// Базовый лучник
/// </summary>
public class Archer : Player
{
    public const int DamageBonus = 10;
    public override void SpecialAbility()
    {
        Console.WriteLine("Приау!");
    }

    public Archer(string nick = "defArc1") : base(nick)
    {
        Damage += DamageBonus;
        Name = nick;
    }
}

public class BattleManager //эээ, ****, переделать
{
    string _devName = "btl1";
    ICharacter Player { get;  }
    ICharacter Enemy { get;  }
    public void PlayerChoice(ICharacter player)
    {
        Console.WriteLine($"\nВаш ход {player.Name}. Что вы делаете?");
        Console.WriteLine("[0] Атаковать\n[1] Показать инвентарь и использовать предмет");
        int choice = Convert.ToInt16(Console.ReadLine());
        if(choice == 0) { NextTurn(); }
        else {
            Console.WriteLine("[9] Отмена");
            player.InventoryManager.ShowInventory();
            int invChoice = Convert.ToInt16(Console.ReadLine());
            if (invChoice < 0 || invChoice == 9) PlayerChoice(player);
            else
            {
                player.InventoryManager.Use((byte)invChoice, player);
                PlayerChoice(player);
            }
        }
    }
    private void NextTurn()
    {
        Player.Attack(Enemy);
        if (Enemy.Health <= 0) { Console.WriteLine($"{Player.Name} победил!"); Thread.Sleep(1000); return; }
        Enemy.Attack(Player);
        if(Player.Health <= 0) { Console.WriteLine($"{Enemy.Name} победил!"); Thread.Sleep(1000000); return; }
        PlayerChoice(Player);
    }


    public BattleManager(ICharacter player, ICharacter enemy)
    {
        Player = player;
        Enemy = enemy;
        PlayerChoice(Player);
    }
}

public class GameManager
{
    Random rnd = new();
    Player player = null;
    public void Start()
    {
        Console.WriteLine("Привет! Пойдем играть в эту консольную штуку по мотивам SOLID!");
        Console.WriteLine("Выбери класс: 0-Воин     1-Лучник");
        int choice = Convert.ToInt16(Console.ReadLine());
        Console.WriteLine("Выбери имячко:");
        string name = Console.ReadLine();
        switch (choice)
        {
            case 0:
                player = new Warrior(name);
                break;
            case 1:
                player = new Archer(name);
                break;
            default:
                player = new Warrior(name);
                break;
        }
        List<IConsumeble> consumebles = new();
        List<IEffect> effects = new() { new HealingEffect(50)};
        List<Potion> potions = new() { new Potion("Зелье лечения","Обычное лечение на 50 пунктов",effects), new Potion("Зелье лечения", "Обычное лечение на 50 пунктов", effects)};
        consumebles.AddRange(potions);
        player.InventoryManager.AddRange(consumebles);
        Console.WriteLine($"Все записано! Привет, {player.Name}!\n\n\n");
        Thread.Sleep(1000);
        NextTurn(player);
    }

    private void NextTurn(Player player)
    {
        Random random = new Random();
        byte rnd = (byte)random.Next(0, 3);
        switch (rnd)
        {
            case 0:
                FightSeq(player);
                break;
            case 1:
                HealSeq(player);
                break;
            case 2:
                BoostSeq(player);
                break;
            default:
                break;
        }
        NextTurn(player);
    }

    //снизу наброски
    private void FightSeq(Player player)
    {
        Player hostile = null;
        uint enemyType = (uint)rnd.Next(0, 2);
        if (enemyType == 0) { hostile = new Warrior("Мобстер") { Damage = 5, Health = 30 }; }
        else { hostile = new Archer("Стреляка") { Damage = 45, Health = 10 }; }
        Console.WriteLine($"----------{player.Name} сталкивается с мобами!-----------");
        Console.WriteLine($"Это же {hostile.Name}!");
        Thread.Sleep(1000);
        BattleManager battleManager = new BattleManager(player, hostile);
    }
    private void HealSeq(Player player)
    {
        Console.WriteLine($"------------{player.Name} приходит к источнику!---------------");
        if (player.InventoryManager.CountItems() >= 8) { Console.WriteLine("Эх! Инвентарь полон!"); return; }
        byte healType = (byte)rnd.Next(0, 2);
        if (healType == 0)
        {
            Potion potion = new Potion("Зелье лечения", "Обычное лечение на 50 пунктов", new List<IEffect>() { new HealingEffect(50) });
            Console.WriteLine($"\\_('o')_/НАХОДКА! {potion.Name} - {potion.Description}.");
            player.InventoryManager.Add(potion);
            Thread.Sleep(3000);
        }
        else
        {
            Potion potion = new Potion("Зелье большого лечения", "Сильное лечение на 100 пунктов", new List<IEffect>() { new HealingEffect(100) });
            Console.WriteLine($"\\_('o')_/НАХОДКА! {potion.Name} - {potion.Description}.");
            player.InventoryManager.Add(potion);
            Thread.Sleep(3000);
        }
    }
    private void BoostSeq(Player player)
    {
        Console.WriteLine($"----------{player.Name} находит алтарь и усиливается!-------------");
    }
}

public enum PotionsTypes { Health, Mana } //сюда добавлять типы зелий(глобальная штука)