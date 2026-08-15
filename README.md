# BlackJackGame

![Main Build Status](https://github.com/thairangov-oss/BlackJackGame/actions/workflows/BuildAction.yml/badge.svg?branch=main)
![NewFeature-Branch Build Status](https://github.com/thairangov-oss/BlackJackGame/actions/workflows/BuildAction.yml/badge.svg?branch=NewFeature-Branch)

## Overview
BlackJackGame is a C#/.NET application that simulates the classic Blackjack card game while showcasing principles of clean software architecture, API-driven design, and rigorous testing practices.  
It includes a CLI client, a dedicated BlackJackCore library, a Web API, and comprehensive unit tests with xUnit. GitHub Actions is used for continuous integration and delivery.

## Features
- 🎮 **Gameplay**
  - Play Blackjack against the dealer in a console-based interface
  - Supports scoring, betting, blackjack detection, insurance, dealer rules, doubling, and hand comparisons
  - Tracks wins, losses, and draws across sessions

- 🧩 **Architecture**
  - **CLI Client**: Console interface for player interaction, communicating with a remote Blackjack API through game start and game action endpoints
  - **BlackJackCore Library**: Modular game engine logic with reusable models (`Card`, `Deck`, `Hand`, `Player`, `Game`)
  - **Web API**: Exposes endpoints for starting games, retrieving state, and performing player actions (Hit, Stand, Double, Insurance)
  - Built with **.NET 9**, **C#**, and **JSON serialization/deserialization** for robust API communication and error handling

- ✅ **Testing**
  - Comprehensive unit and integration tests with **xUnit**
  - Covers scenarios including card/deck behavior, scoring, betting, blackjack-on-deal, doubling, insurance handling, dealer behavior, and hand comparisons
  - Dedicated **API.Tests** project to validate API endpoints and integrations
  - Focused on improving test coverage and confidence in core game behavior

- ⚙️ **Continuous Integration**
  - Automated builds and tests via **GitHub Actions**
  - Separate build pipelines for `main` and `NewFeature-Branch`
  - CI/CD ensures consistent, error-free delivery

- 📦 **Maintainability**
  - Clean separation of concerns between CLI, API, Core, and Tests
  - SOLID principles applied for extensibility and reusability
  - Strong emphasis on writing reliable, maintainable code and catching issues early through testing

## Getting Started

### Clone the repository
```bash
git clone https://github.com/thairangov-oss/BlackJackGame.git
cd BlackJackGame
dotnet build
dotnet run
```

cd BlackJackGame.Tests
dotnet test

License
MIT License

---

## Notes
- The **two badges** at the top track build status for:
  - `main` branch
  - `NewFeature-Branch`
- The **Running Tests** section is explicit so contributors know how to validate changes.
- The **License** section specifies MIT for clarity and professionalism.
- The **Contributing** section provides clear steps for new contributors to fork, branch, test, and submit PRs.

---

- The **Features** section now reflects:
  - CLI client communicating with a remote API
  - BlackJackCore library with reusable models (`Card`, `Deck`, `Hand`, `Player`, `Game`)
  - JSON serialization/deserialization and robust API error handling
  - Expanded testing scenarios (scoring, betting, blackjack-on-deal, doubling, insurance, dealer logic, hand comparisons)
  - Dedicated **API.Tests** project
- GitHub Actions pipelines are explicitly tied to both `main` and `NewFeature-Branch` for CI/CD.
- Clean separation of concerns (CLI, API, Core, Tests) highlights maintainability and SOLID principles.
- Strong emphasis on writing reliable, maintainable code and catching issues early through testing.

---
## Project Structure
BlackJackGame/
BlackJackGame/
├── BlackJackGame.CLI/            # Console client (renamed from BlackJack1)
│   ├── Program.cs                # Entry point for the CLI
│   ├── Services/                 # CLI helpers (if present)
│   └── Models/                   # CLI-specific models (if present)

├── BlackJackGame.Api/            # Web API
│   ├── Controllers/              # API endpoints
│   │   └── GameController.cs     # Handles game start and player actions
│   ├── Models/                   # API request/response models (if present)
│   └── Services/                 # API services (if present)
# (No Startup.cs present in repo)

├── BlackJackGame.Core/           # Core library (renamed from BlackJackCore.Core)
│   ├── Card.cs                   # Represents a playing card
│   ├── Deck.cs                   # Deck management logic
│   ├── Hand.cs                   # Player/dealer hand logic
│   ├── Player.cs                 # Player model
│   └── Game.cs                   # Core game rules and flow

├── BlackJackGame.Tests/          # xUnit test project
│   └── UnitTest1.cs              # Consolidated tests for card, deck, and game
# (No HandComparisonTests.cs present)

├── .github/workflows/            # GitHub Actions CI/CD pipelines
│   └── BuildAction.yml           # Automated build and test workflow

└── README.md                     # Project documentation





