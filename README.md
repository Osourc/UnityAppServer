####SAMPLE#####
# Unity App Server (C# + MongoDB)

This project is a server-side application built entirely in **C# within Unity**, using **MongoDB C# Driver** to manage user data, inventory, game states, and other backend services. It’s designed for Unity games requiring a lightweight, internal backend without third-party server frameworks.

---

## 🚀 Features

- 🔐 **User Authentication** (Register, Login, Guest Login)
- 📦 **Inventory Management** (Store and retrieve player items)
- 💰 **Shop System** (Buy/apply items like backgrounds, music, SFX)
- 📄 **MongoDB Integration** using official C# Driver
- 🔄 **Player Data Sync** with MongoDB Atlas or self-hosted MongoDB
- 🛡️ **Ban System** support
- 🎮 **Game Data Management** (DNA, applied items, etc.)

---

## 🛠 Tech Stack

| Tech       | Description                                 |
|------------|---------------------------------------------|
| Unity      | Game engine and runtime environment         |
| C#         | Main programming language                   |
| MongoDB    | NoSQL database for player and game data     |
| MongoDB.Driver | Official MongoDB C# driver             |

---

## 📂 Project Structure (Server Scripts)
# This is just a sample structure
/Assets/Scripts/Server
│
├── MongoDBHandler.cs // Manages DB connection
├── Player.cs // Player model
├── AuthManager.cs // Login, register, guest login
├── InventoryManager.cs // Inventory CRUD operations
├── ShopManager.cs // Buy/apply items logic
├── DnaManager.cs // DNA currency management
├── BanManager.cs // Ban system logic
└── Utility.cs // Reusable helper methods

---

## 🔧 Setup Instructions

### 1. 🧱 Requirements

- Unity 2021+ (or newer)
- MongoDB Atlas account (or local MongoDB instance)
- Internet access (if using MongoDB Atlas)
- Official MongoDB C# Driver:
  - Install via NuGet (recommended via external .NET project) or
  - Manually import `MongoDB.Driver.dll` and dependencies

### 2. 🗃️ MongoDB Setup

Create a collection named `Players` in your MongoDB. Sample document schema:

```json
{
  "_id": "player123",
  "Username": "user",
  "ShaPassword": "hashed_pass",
  "GameName": "CoolPlayer",
  "DnaCount": 1000,
  "IsBanned": false,
  "Inventory": {
    "Background": [],
    "Sound": [],
    "SoundEffect": []
  },
  "AppliedBackground": "Default"
}
