# Archero Clone

A Unity-based clone of the popular mobile game Archero, featuring archer-style combat with physics-based projectiles.

## Game Mechanics

### Control Mechanics
- Character is controlled using an on-screen joystick (using an external package)
- When the player releases the joystick, the character stops moving and attacks are triggered automatically

### Combat Mechanics
- When stationary, the character automatically targets and attacks the nearest enemy
- Attacks are archer-style ranged attacks with physics-based projectile trajectories
- Projectiles are affected by gravity for realistic arcing shots

### Enemies
- Enemies are stationary "dummy" cubes that receive damage
- Each enemy has a health bar displayed above it
- When defeated, enemies respawn at random positions
- At least five enemies are visible on screen at any time

### Camera and Map
- The map is a limited area visible within a 16:9 portrait view
- The camera remains fixed

## Architecture

This project implements several design patterns:

### Strategy Pattern
- `IProjectileStrategy` interface defines different projectile behaviors
- `StandardProjectileStrategy` implements single projectile firing
- `MultiShotProjectileStrategy` implements multiple projectile firing in a spread pattern

### Factory Pattern
- `ProjectileStrategyFactory` creates different projectile strategies
- Allows for easy extension with new projectile types

### Observer Pattern
- `GameEvent` ScriptableObject and `GameEventListener` implement a flexible event system
- Used for skill upgrades and other game events

### Singleton Pattern
- `GameManager` implements a singleton for global game state management

## Project Structure

- **Core**: Core interfaces and utilities
  - **Events**: Observer pattern implementation
  - **Interfaces**: Core interfaces like IDamageable
- **Combat**: Combat-related classes
  - **Strategies**: Projectile strategy implementations
  - **Factories**: Factory for creating strategies
- **Player**: Player-related classes
  - PlayerController
  - PlayerSkillSystem
- **Enemies**: Enemy-related classes
- **UI**: UI components like health bars
- **Managers**: Game management classes

## Skills System

The game features a flexible skill system that can be expanded:

- Skills can be unlocked and upgraded
- Each skill has multiple levels
- Skills modify the player's combat abilities
- Currently implemented: Multi-Shot skill

## External Dependencies

- Joystick controller package (imported separately)

## Future Improvements

- Additional projectile strategies (piercing, homing, explosive)
- More enemy types with different behaviors
- Level progression system
- Power-up pickups
- Visual effects for projectiles and impacts 