# 🔵🔴 2Dots Clone - Test Project

A 7x7 grid-based match game inspired by **Two Dots**, built in Unity. Match same-colored dots by dragging across the grid, activate bombs, and use debug tools to test mechanics.

---

## 🎮 Features

### ✅ Gameplay Mechanics
- Static grid size: **7x7**
- **5 different colored dots**
- Drag to connect adjacent dots **horizontally or vertically** (no diagonals or dot reuse)
- Match and clear **3 or more connected dots** with a simple scale-down animation

### ✅ Grid Refill System
- Dots above fall to fill empty spaces
- New dots spawn from the top
- Includes **smooth downward animation**

### ✅ Special Tiles
- **Bomb Dot**: Connect **6 dots** to create a Bomb. Clicking it explodes a **3x3** area.
- **Colored Bomb**: Connect **9 dots** to create a Colored Bomb. Connect it with any dot to clear **all dots of that color** *(Bugged)*

### ✅ Debug Tools
- **Right-click** any dot to cycle through colors (great for testing!)
- **Shuffle Button**: Randomly rearranges the grid
- **Space Key Hint**: Highlights one possible match on the grid

---

## 🧠 Code Structure

### `Dot`
Handles individual dot properties and behaviors (e.g., color, state).

### `Bomb`
Manages special bomb-type dots and their unique interactions (e.g., explode on click).

### `GridManager`
A monolith-style manager that:
- Initializes the grid
- Spawns dots and bombs
- Handles matching, clearing, refilling, and shuffling
- Controls the core logic and grid state

> 💡 **Design Note**: The `GridManager` centralizes most gameplay logic for simplicity and time constraints, allowing fast iteration. While monolithic, it's kept modular enough for easy refactor.

### `InputManager`
- Handles all player input:
  - Dot connecting via drag
  - Bomb interactions
- Also manages debug tools (shuffle & hint)

### `LineConnector`
- A helper for rendering lines between selected dots
- Focuses on visual polish rather than gameplay logic

---

## 🚧 Known Issues
- **Colored Bomb Effect**: Connecting Colored Bomb with a dot should clear all dots of that color but is currently bugged.

---

## 🛠️ Development Notes

This project was built under a tight scope and timeline. Monolithic design decisions (especially in `GridManager`) were made deliberately for manageability within constraints, while isolating key visual and interaction features (`LineConnector`, `InputManager`) for clarity.

---

## 📁 Requirements
- Unity 2021.3 or later
- DOTween (for animations)

---

## ✨ Future Improvements
- Fix Colored Bomb functionality
- Add more tween animations


