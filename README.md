## 📌 Цель проекта

Этот учебный проект демонстрирует применение принципов SOLID при разработке консольной rogue-like игры. Основные задачи:

- Создать гибкую архитектуру
- Реализовать систему предметов и эффектов
- Показать применение ООП-принципов
- Обеспечить легкое добавление новых типов:
  - Персонажей
  - Предметов
  - Эффектов

Проект служит как одним из примеров изучения SOLID на практике. Черновой вариант проекта показывает способность к дальнейшему изучению и совершествованию solid архитектуры.

## 🎮 Организация пользовательского интерфейса

Программа использует консольный интерфейс с пошаговым взаимодействием:

1. **Начало игры**:
   - Выбор класса персонажа (Воин/Лучник)
   - Ввод имени персонажа
   - Начальная комплектация предметами

2. **Основной игровой цикл**:
   - Случайные события:
     - Сражения с противниками
     - Находки зелий
     - Усиления персонажа(заглушка)
   - Пошаговое управление

3. **Управление в бою**:
  - [0] - Атаковать
  - [1] - Открыть инвентарь
    - [номер] - Выбрать предмет
  - [9] - Отмена
## 🏛 Иерархия объектов

```mermaid
classDiagram
    %% Interfaces
    ICharacter <|-- Character
    IConsumeble <|-- IPotion
    IPotion <|-- Potion
    IEffect <|-- HealingEffect
    IEffect <|-- ManaRestoringEffect
    
    %% Main Character Hierarchy
    Character <|-- Player
    Player <|-- Warrior
    Player <|-- Archer
    Player <|-- Mage
    
    %% Interface Definitions
    class ICharacter {
        +string Name
        +string Description
        +int Damage
        +int Health
        +int MaxHealth
        +int Mana
        +int MaxMana
        +InventoryManager InventoryManager
        +Attack(ICharacter target)
        +TakeDamage(int amount)
        +Use(IConsumeble consumeble)
        +Heal(int heal)
        +HealMana(int healMana)
    }
    
    class IConsumeble {
        +string Name
        +string Description
        +int Weight
        +Use(ICharacter character)
    }
    
    class IPotion {
        +PotionsTypes type
        +List~IEffect~ Effects
    }
    
    class IEffect {
        +string Name
        +string Description
        +Apply(ICharacter character)
    }
    
    %% Class Definitions
    class Character {
        +string Name
        +string Description
        +int Health
        +int MaxHealth
        +int Mana
        +int MaxMana
        +int Damage
        +InventoryManager InventoryManager
        +Attack(ICharacter target)
        +TakeDamage(int damage)
        +Use(IConsumeble consumeble)
        +Heal(int heal)
        +HealMana(int healMana)
    }
    
    class Player {
        +string Nickname
        +SpecialAbility()
    }
    
    class Warrior {
        +const int HpBonus
        +SpecialAbility() "Console: 'Яху!'"
    }
    
    class Archer {
        +const int DamageBonus
        +SpecialAbility() "Console: 'Приау!'"
    }
    
    class Mage {
        +SpecialAbility() "Console: 'ТрТр!'"
    }
    
    class Potion {
        +string Name
        +string Description
        +int Weight
        +PotionsTypes type
        +List~IEffect~ Effects
        +Use(ICharacter character)
    }
    
    class HealingEffect {
        +string Name
        +string Description
        +Apply(ICharacter character)
    }
    
    class ManaRestoringEffect {
        +string Name
        +string Description
        +Apply(ICharacter character)
    }
