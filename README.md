# Archero Clone

## 🎮 About the Game

Archero Clone is a dynamic action roguelike game developed in Unity where the player controls a hero fighting through rooms filled with enemies. Drawing inspiration from the popular mobile game "Archero," this clone reimagines the core mechanics with new features and enhancements.

### Key Features

- **Intuitive Controls**: Move with joystick, automatically attack when standing still
- **Dynamic Combat System**: Various projectile types including bouncing and burning effects
- **Status Effects**: Enemies can be inflicted with status effects like burning
- **Skill System**: Upgrade your character with various skills and abilities

## 🛠️ Technologies Used

- **Unity** - Game engine
- **C#** - Programming language
- **Dependency Injection System** - Custom DI implementation for better code architecture
- **Component-Based Architecture** - Modular design for flexibility and reusability
- **Strategy and Factory Patterns** - For weapon and projectile systems
- **JMO Assets** - For visual effects
- **Joystick Pack** - For mobile controls

## 📋 Project Structure

### Folder Structure
- **Player** - Player controller, data, and skill system
- **Combat** - Weapon and projectile system, including status effects
- **Enemies** - Enemy behaviors and AI
- **Managers** - Game state and enemy management
- **UI** - User interface elements
- **Core** - Core functionality and utilities

### Architectural Patterns
- **Strategy Pattern**: `IProjectileStrategy` interface defines different projectile behaviors
- **Factory Pattern**: `ProjectileStrategyFactory` creates different projectile strategies
- **Singleton Pattern**: `GameManager` implements a singleton for global game state management
- **Dependency Injection**: Custom DI system with `InjectedMonoBehaviour` class

## 🧠 Game Mechanics

### Control Mechanics
- Character is controlled using an on-screen joystick
- When the player releases the joystick, the character stops moving and attacks are automatically triggered

### Combat System
The game features a robust combat system with different projectile behaviors:
- **Standard Projectiles** - Basic straight-line attacks
- **Bouncing Projectiles** - Bounce between enemies
- **Burning Projectiles** - Apply burn damage over time

### Status Effects
Enemies can be inflicted with various status effects:
- **Burn** - Deals damage over time, can stack up to 3 times by default
- Status effects are implemented as MonoBehaviour components on enemies

### Skill System
The game includes an extensible skill system:
- Skills can be unlocked and upgraded
- Each skill has multiple levels
- Skills modify the player's combat abilities

### Enemy System
- Enemies derive from the `Enemy` class
- Each enemy has a health value
- Enemies are managed by the `EnemyManager`

### Game Management System
- `GameManager` handles game state and flow
- Implements the Singleton pattern for global access
- Manages connections between game state and other managers

## 📦 Code Features

### Dependency Injection System
- Contains a custom DI system to manage dependencies between game components
- `InjectedMonoBehaviour` class extends Unity MonoBehaviour to add DI support

### Modular Weapon System
- `WeaponController` manages weapon behaviors
- Different projectile strategies provide different attack patterns
- Projectile behaviors are easily extensible

### Status Effects
- Status effects applicable to enemies are designed modularly
- Each effect contains its own behavior and logic (e.g., BurnEffect)
- Effects can be configured and stacked as desired

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

<div align="center">
  <h3>Enjoy the game!</h3>
</div> 