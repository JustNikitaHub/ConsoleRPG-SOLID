/* TO-DO LIST для рефакторинга:

1.ИСПРАВИТЬ ИНИЦИАЛИЗАЦИЮ СТАТОВ:
   -Перенести инициализацию BuffManager в начало конструктора Player
   - Заменить _damage = Damage на _baseDamage = base.Damage
   - Для всех базовых статов (_armor, _maxHealth) сделать аналогично

2. ПЕРЕРАБОТАТЬ BattleManager:
   -Добавить проверку на ICharacterModifiable при выводе статов
   - Выводить Player.Damage вместо Enemy.Damage в интерфейсе
   - Убедиться, что BuffManager.UpdateEffects() вызывается корректно

3. ОБНОВИТЬ СИСТЕМУ ЭФФЕКТОВ:
   -Проверить работу StableEffect с новыми базовыми статами
   - Убедиться, что модификаторы применяются к _baseDamage, а не к Damage

4. ТЕСТИРОВАНИЕ:
   -Создать тестового персонажа с известными статами (Damage=10, Armor = 5)
   - Проверить вывод в BattleManager.ShowPlayer()
   - Проверить применение баффов/дебаффов
   - Убедиться, что Archer получает +10 к урону

5. ОПТИМИЗАЦИЯ:
   -Рассмотреть кэширование модифицированных значений
   - Добавить dirty-флаги для пересчета только при изменениях

6. ИНТЕРФЕЙСЫ:
   -Убедиться, что ICharacterModifiable не требует лишних методов
   - Проверить совместимость со старым кодом

Сначала исправляем пункты 1-3, затем тестируем (4), потом оптимизируем (5) */

Archer archer = new Archer("Тестовик");
archer.BuffManager.AddModifier(new StatModifier(StatTypes.Healing, 20));
GameManager game = new();
game.Start();

//-----------------------------
/// <summary>
/// Интерфейс для всех персонажей, базовые поля и действия
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
    int Armor {  get; protected set; }
    InventoryManager InventoryManager { get; }
    void Attack(ICharacter target);
    void TakeDamage(int amount);
    void Use(IConsumeble consumeble);
    void Heal(int heal);
    void HealMana(int healMana);
}

/// <summary>
/// Позволяет менять поля персонажа, усиления
/// </summary>
public interface ICharacterModifiable : ICharacter
{
    BuffManager BuffManager { get; }
    int _damage {  get; }
    int _maxHealth { get; }
    int _armor {  get; }
}

/// <summary>
/// Интерфейс для всей еды, расходных предметов
/// </summary>
public interface IConsumeble
{
    string Name { get; }
    string Description { get; }
    int Weight { get; }
    void Use(ICharacter character);
    
}

/// <summary>
/// Интерфейс зелий с различными эффектами; расходный предмет
/// </summary>
public interface IPotion : IConsumeble
{
    PotionsTypes type { get; }
    List<IEffect> Effects { get; }
}

/// <summary>
/// База для дальнейших эффектов: одноразовые/продолжительные. Не использовать standalone!
/// </summary>
public interface IEffect
{
    string Name { get; }
    string Description { get; }
    void Apply(ICharacterModifiable character);
    bool Update(ICharacterModifiable character);
    void Remove(ICharacterModifiable character);
}
/// <summary>
/// Моментальный эффект с дейтвием в один раунд для таких эффектов как зелья
/// </summary>
public class InstantEffect : IEffect
{
    private readonly Action<ICharacterModifiable> _action; //делегат на... делегат скрытый пиздец... нахуй кароче я не знаю что он делает, но по другому не ало!!!
    public string Name { get; } = "Эффект ";
    public string Description { get; } = "Моментальное действие. ";
    public void Apply(ICharacterModifiable character) => _action?.Invoke(character); //что там оно вызывается, что то вертится, что то мутит кароче
    public bool Update(ICharacterModifiable character) => true;    //тухнет сразу при использовании
    public void Remove(ICharacterModifiable character) { }          //нет смысла убирать
    public InstantEffect(string name, string descr, Action<ICharacterModifiable> action)
    {
        Name += name;
        Description += descr;
        _action = action;
    }
}
/// <summary>
/// Постоянный эффект без времени длительности для таких эффектов как усиления
/// </summary>
public class StableEffect : IEffect
{
    private readonly StatModifier _modifier; //бафы с поддержкой процентов(finally!!!)
    public string Name { get; } = "Эффект ";
    public string Description { get; } = "Длительное действие. ";
    public void Apply(ICharacterModifiable character) => character.BuffManager.AddModifier(_modifier);       //добавляется в список модификаторов
    public bool Update(ICharacterModifiable character) => false;                                 //не тухнет сразу
    public void Remove(ICharacterModifiable character) => character.BuffManager.RemoveModifier(_modifier);  //можно спокойной убирать
    /// <summary>
    /// Конструктор для постоянного эффекта
    /// </summary>
    /// <param name="name">Имя эффетка</param>
    /// <param name="descr">Лорное описание</param>
    /// <param name="stat">Модификатор для изменения</param>
    public StableEffect(string name, string descr, StatModifier stat)
    {
        Name += name;
        Description += descr;
        _modifier = stat;
    }
}
/// <summary>
/// Временный эффект с применением каждый ход для таких эффектов как регенерация или восстановление
/// </summary>
public class TimeEffect : IEffect
{
    private readonly int _duration;
    private int _timeLeft;
    private readonly Action<ICharacterModifiable> _action;
    public string Name { get; } = "Эффект ";
    public string Description { get; } = "Временное действие. ";
    public void Apply(ICharacterModifiable character) => _action(character);
    public bool Update(ICharacterModifiable character)
    {
        _action(character);
        return --_timeLeft<=0;
    }
    public void Remove(ICharacterModifiable character) { }
    public TimeEffect(string name, string descr, int time ,Action<ICharacterModifiable> action)
    {
        Name += name;
        Description += descr;
        _action = action;
        _timeLeft = time;
    }

}
public interface IEquipment                 //любая экипировка имеет
{
    string Name { get; }                    //имя
    string Description { get; }             //описание
    void Equip(ICharacter character);       //надеть её и добавить свойства
    void Unequip(ICharacter character);     //снять и убрать свойства
    DamageTypes type { get; }               //тип магический или физический

}

public interface IArmor : IEquipment        //любая броня имеет
{
    int Defence {  get; }
    //TO-DO
}

public interface IGun : IEquipment          //любое вооружение имеет
{
    int Damage { get; }
    //TO-DO
}

//----------!---------------ЭФФЕКТЫ----------------!-------------

//---------!----------------------------------------------!-------------

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
        if (character is ICharacterModifiable characterModifiable)
        {
            foreach (IEffect effect in Effects)
            {
                effect.Apply(characterModifiable);
            }
        }
        else
        {
            Console.Error.WriteLine("БЛЯДЬ");
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



public class BuffManager
{
    List<IEffect> _effects = new List<IEffect>();             //активные эффекты
    Dictionary<StatTypes, List<StatModifier>> typeModiferValues = new Dictionary<StatTypes, List<StatModifier>>(); //модификаторы
    ICharacterModifiable _modifiable;    //чувак с этими бафами

    public void AddModifier(StatModifier modifier)
    {
        if (!typeModiferValues.ContainsKey(modifier.Type))
            typeModiferValues[modifier.Type] = new List<StatModifier>();
        typeModiferValues[modifier.Type].Add(modifier);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        if (typeModiferValues.TryGetValue(modifier.Type, out var modifiers))
        {
            modifiers.Remove(modifier);
        }
    }

    public float GetModifiedValue(float baseValue, StatTypes type)
    {
        if (!typeModiferValues.TryGetValue(type, out var modifiers))
            return baseValue;

        float additive = modifiers.Where(m => !m.IsMult).Sum(m => m.Value);
        float multiplicative = modifiers.Where(m => m.IsMult)
                                     .Aggregate(1f, (acc, m) => acc * m.Value);

        return (baseValue + additive) * multiplicative;
    }
    public void AddEffect(IEffect effect)
    {
        _effects.Add(effect);
        effect.Apply(_modifiable);
    }

    public void RemoveEffect(IEffect effect)
    {
        effect.Remove(_modifiable);
        _effects.Remove(effect);
    }

    public void UpdateEffects()
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            if (_effects[i].Update(_modifiable))
            {
                RemoveEffect(_effects[i]);
            }
        }
    }
    public BuffManager(ICharacterModifiable modifiable)
    {
        _modifiable = modifiable;
    }
}

/// <summary>
/// Управление инвентарем, добавление, удаление, посредник использования предметов
/// </summary>
public class InventoryManager
{
    string _devname = "inv1";
    public byte Size=9;
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
        if (_consumebles.Count + 1 > Size) { Console.WriteLine($"В вашем инвантаре нет места! Предмет {consumeble.Name} не был добавлен!"); return; }
        _consumebles.Add(consumeble);
    }

    public void AddRange(List<IConsumeble> consumebles)
    {
        foreach (IConsumeble item in consumebles)
        {
            Add(item);
        }
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

public class StatModifier
{
    public StatTypes Type { get; }
    public bool IsMult {  get; set; }
    public float Value { get; set; }
    public StatModifier(StatTypes type, float value, bool mult = false)
    {
        Type = type;
        Value = value;
        IsMult = mult;
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
    public int Armor {  get; set; } = 10;
    public InventoryManager InventoryManager { get; }
    public virtual void Attack(ICharacter target)
    {
        Console.WriteLine($"{Name} атакует {target.Name} и наносит {Damage} урона!");
        target.TakeDamage(Damage);
    }

    public virtual void TakeDamage(int damage)
    { 
        Health -= Math.Max(damage-Armor,1);
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
        Mana = Math.Min(MaxMana, Mana + healMana);
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
public abstract class Player : Character, ICharacterModifiable
{
    public int _damage { get; protected set; }
    public int _maxHealth {get; protected set; }
    public int _armor {get; protected set; }
    public BuffManager BuffManager { get; }
    public string Nickname { get; protected set; }
    public abstract void SpecialAbility();
    public new int Damage => (int)BuffManager.GetModifiedValue(_damage, StatTypes.Damage);
    public new int Armor => (int)BuffManager.GetModifiedValue(_armor, StatTypes.Armor);
    public new int MaxHealth => (int)BuffManager.GetModifiedValue(_maxHealth, StatTypes.Health);
    public Player(string nickname="defplr")
    {
        BuffManager = new BuffManager(this);
        Nickname = nickname;
        _damage = base.Damage;
        _maxHealth = base.MaxHealth;
        _armor = base.Armor;
    }

}

/// <summary>
/// Базовый воин
/// </summary>
public class Warrior : Player
{
    public const int HpBonus = 10;
    public override void SpecialAbility()
    {
        //сюда буст к урону
    }
    
    public Warrior(string nick="defWar1") : base(nick)
    {
        _maxHealth += HpBonus;
        MaxMana = 0;
        Mana = 0;
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
        _damage += DamageBonus;
        Name = nick;
    }
}

public class BattleManager //эээ, ****, переделать
{
    string _devName = "btl1";
    ICharacter Player { get;  }
    ICharacter Enemy { get;  }
    BuffManager BuffManager { get; }
    public void PlayerChoice(ICharacter player)
    {
        Console.WriteLine($"\nВаш ход {player.Name}. Что вы делаете?");
        Console.WriteLine("[0] Атаковать\n[1] Открыть инвентарь\n[2] Показать информацию о противнике\n[3] Показать информацию о вас");
        int choice = Convert.ToInt16(Console.ReadLine());
        switch (choice)
        {
            //атака
            case 0:
                NextTurn();
                break;
            //открытие инвентаря
            case 1:
                Console.WriteLine("[9] Отмена");
                player.InventoryManager.ShowInventory();
                int invChoice = Convert.ToInt16(Console.ReadLine());
                if (invChoice < 0 || invChoice == 9) PlayerChoice(player);
                else
                {
                    player.InventoryManager.Use((byte)invChoice, player);
                    PlayerChoice(player);
                }
                break;
            //узнать о враге
            case 2:
                ShowEnemy();
                break;
            //узнать о себе
            case 3:
                ShowPlayer();
                break;
            default:
                break;
        }
    }
    private void NextTurn()
    {
        Console.Clear();
        Player.Attack(Enemy);
        if (Enemy.Health <= 0) { Console.WriteLine($"{Player.Name} победил!"); Thread.Sleep(1000); return; }
        Enemy.Attack(Player);
        if(Player.Health <= 0) { Console.WriteLine($"{Enemy.Name} победил!"); Thread.Sleep(1000000); return; }
        BuffManager.UpdateEffects();
        PlayerChoice(Player);
    }

    private void ShowEnemy()
    {
        Console.Clear();
        Console.WriteLine($"#Противник: {Enemy.Name}\n#Урон: {Enemy.Damage}\n#Здоровье: {Enemy.Health}\n#Инвентарь:");
        Enemy.InventoryManager.ShowInventory();
        PlayerChoice(Player);
    }

    private void ShowPlayer()
    {
        Console.Clear();
        Console.WriteLine($"#Ваше имя: {Player.Name}\n#Урон: {Player.Damage}\n#Броня: {Player.Armor}\n#Здоровье: {Player.Health}\n#Инвентарь: {Player.InventoryManager.CountItems()} из {Player.InventoryManager.Size}");
        PlayerChoice(Player);
    }


    public BattleManager(ICharacter player, ICharacter enemy, BuffManager buffManager)
    {
        Player = player;
        Enemy = enemy;
        BuffManager = buffManager;
        PlayerChoice(Player);
    }
}

public class GameManager
{
    Random GameRandom = new();
    ICharacterModifiable player = null;
    BuffManager PlayerBuffManager = null;
    public void Start()
    {
        Console.WriteLine("Привет! Пойдем играть в эту консольную штуку по мотивам SOLID!");
        Console.WriteLine("Выбери класс: 0-Воин     1-Лучник");
        int choice = Convert.ToInt16(Console.ReadLine());
        Console.WriteLine("Выбери имячко:");
        string name = Console.ReadLine();
        switch (choice)
        {
            case 1:
                player = new Archer(name);
                break;
            default:
                player = new Warrior(name);
                break;
        }
        PlayerBuffManager = player.BuffManager;
        Console.WriteLine($"Все записано! Привет, {player.Name}!\n\n\n");
        Thread.Sleep(1000);
        NextTurn(player);
    }

    private void NextTurn(ICharacter player)
    {
        byte rnd = (byte)GameRandom.Next(0, 3);
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
    private void FightSeq(ICharacter player)
    {
        ICharacter hostile = null;
        uint enemyType = (uint)GameRandom.Next(0, 2);
        if (enemyType == 0) { hostile = new Warrior("Мобстер") { Health = 60 }; }
        else { hostile = new Archer("Стреляка") { Health = 30 }; }
        Console.WriteLine($"----------{player.Name} сталкивается с мобами!-----------");
        Console.WriteLine($"Это же {hostile.Name}!");
        Thread.Sleep(1000);
        BattleManager battleManager = new BattleManager(player, hostile, PlayerBuffManager);
    }
    private void HealSeq(ICharacter player)
    {
        Console.WriteLine($"------------{player.Name} приходит к источнику!---------------");
        byte healType = (byte)GameRandom.Next(0, 6);
        if (healType == 0)
        {
            Thread.Sleep(3000);
        }
        else if (healType == 1) 
        {
            Thread.Sleep(3000);
        }
    }
    private void BoostSeq(ICharacter player)
    {
        //Console.WriteLine($"----------{player.Name} находит алтарь и усиливается!-------------");
    }
}

public enum PotionsTypes { Health, Mana } //сюда добавлять(глобальная штука)
public enum DamageTypes { Physics, Magic }
public enum StatTypes { Damage, Health, Healing , Mana, Armor }
