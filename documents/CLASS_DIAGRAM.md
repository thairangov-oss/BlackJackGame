# Blackjack Class Diagrams

This document illustrates the domain logic and API infrastructure layer for the Blackjack project.

## 1. Domain Class Diagram (Core Engine)

```mermaid
classDiagram
class Game {
  - Deck Deck
  - Player Player
  - Player Dealer
  - bool IsRoundComplete
  - string OutcomeMessage
  + Start(int bet)
  + Hit()
  + Stand()
  + Double()
  + TakeInsurance()
  + DealerTurn()
  + CompareHands()
}

class Player {
  - Hand Hand
  - int Balance
  - int Bet
  + PlaceBet(int)
  + Double(Deck)
  + TakeInsurance()
}

class Deck {
  - List~Card~ Cards
  + Shuffle()
  + Deal() Card
}

class Hand {
  - List~Card~ Cards
  + AddCard(Card)
  + IsBlackjack()
}

class Card {
  + Value() int
}

Game --> Player : has Player and Dealer
Game --> Deck : uses
Player --> Hand : has
Hand --> Card : contains
```

## 2. API & Infrastructure Diagram (Runtime Layer)

```mermaid
classDiagram
class GameController {
  + Start(int bet)
  + Action(Guid, ActionRequest)
}

class GameStore {
  + Create(Game) Guid
  + TryGet(Guid) Game?
  + ExecuteWithLock(Guid, Action~Game~) Game
  + CleanupOldGames(TimeSpan) int
}

class GameCleanupHostedService {
  + ExecuteAsync(CancellationToken)
}

class Game {
  + Start(int bet)
  + Hit()
  + Stand()
}

GameController --> GameStore : uses
GameStore --> Game : stores
GameCleanupHostedService --> GameStore : cleanup
```

### Architecture Notes

- **GameStore**: Registered as a singleton in dependency injection to manage active game sessions in memory.
- **Concurrency**: ExecuteWithLock acquires per-game lock instances to prevent race conditions during concurrent player actions.
- **Background Processing**: GameCleanupHostedService runs as a periodic background service to invoke CleanupOldGames(...) and drop inactive sessions.
