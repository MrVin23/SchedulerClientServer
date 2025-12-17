# Events Database Structure

This document describes the database structure for the Events system, including the models, relationships, and constraints.

## Overview

The Events system consists of four main models:
- **EventTypesModel**: Defines the types/categories of events
- **EventsModel**: Represents individual events
- **UserEventsModel**: Junction table linking users to events (many-to-many relationship)
- **EventSettingsModel**: User-specific event settings

## Models

### EventTypesModel

Defines the types or categories that events can belong to.

**Properties:**
- `Id` (int) - Primary key (inherited from `ModelBase`)
- `Name` (string) - The name of the event type (e.g., "Meeting", "Task", "Reminder")
- `CreatedAt` (DateTime) - Timestamp when the record was created (inherited from `ModelBase`)
- `UpdatedAt` (DateTime) - Timestamp when the record was last updated (inherited from `ModelBase`)

**Database Constraints:**
- Unique index on `Name` to prevent duplicate event type names

**File:** `EventTypes.cs`

---

### EventsModel

Represents an individual event in the system.

**Properties:**
- `Id` (int) - Primary key (inherited from `ModelBase`)
- `EventTypeId` (int?) - Foreign key to `EventTypesModel` (nullable)
- `EventType` (EventTypesModel?) - Navigation property to the event type
- `Title` (string) - The title of the event (required, default: empty string)
- `Description` (string?) - Optional description of the event
- `CanBePostponed` (bool) - Whether the event can be postponed (default: `true`)
- `IsCompleted` (bool) - Whether the event has been completed (default: `false`)
- `StartDateTime` (DateTime?) - Optional start date and time of the event
- `EndDateTime` (DateTime?) - Optional end date and time of the event
- `CreatedAt` (DateTime) - Timestamp when the record was created (inherited from `ModelBase`)
- `UpdatedAt` (DateTime) - Timestamp when the record was last updated (inherited from `ModelBase`)

**Database Constraints:**
- Foreign key relationship to `EventTypesModel` with `SetNull` delete behavior (if event type is deleted, the event's `EventTypeId` is set to null)

**File:** `EventsModel.cs`

---

### UserEventsModel

Junction table that creates a many-to-many relationship between users and events. This allows multiple users to be associated with the same event, and a single user to be associated with multiple events.

**Properties:**
- `Id` (int) - Primary key (inherited from `ModelBase`)
- `UserId` (int) - Foreign key to `User` model
- `User` (User) - Navigation property to the user
- `EventId` (int) - Foreign key to `EventsModel`
- `Event` (EventsModel) - Navigation property to the event
- `CreatedAt` (DateTime) - Timestamp when the record was created (inherited from `ModelBase`)
- `UpdatedAt` (DateTime) - Timestamp when the record was last updated (inherited from `ModelBase`)

**Database Constraints:**
- Primary key: `Id` (auto-generated)
- Unique constraint on `(UserId, EventId)` to prevent duplicate user-event associations
- Foreign key to `User` with `Cascade` delete behavior (if a user is deleted, all their event associations are deleted)
- Foreign key to `EventsModel` with `Cascade` delete behavior (if an event is deleted, all user associations are deleted)
- Index on `UserId` for query performance
- Index on `EventId` for query performance

**File:** `UserEventsModel.cs`

---

### EventSettingsModel

Stores user-specific settings for events, allowing each user to customize their event experience.

**Properties:**
- `Id` (int) - Primary key (inherited from `ModelBase`)
- `UserId` (int) - Foreign key to `User` model
- `User` (User) - Navigation property to the user
- `FollowUpPeriodDays` (int) - Number of days for follow-up period
- `CreatedAt` (DateTime) - Timestamp when the record was created (inherited from `ModelBase`)
- `UpdatedAt` (DateTime) - Timestamp when the record was last updated (inherited from `ModelBase`)

**Database Constraints:**
- Unique constraint on `UserId` to ensure one settings record per user
- Foreign key to `User` with `Cascade` delete behavior (if a user is deleted, their settings are deleted)
- Index on `UserId` for query performance

**File:** `EventSettingsModel.cs`

---

## Relationships

### EventTypesModel ↔ EventsModel

**Type:** One-to-Many (optional)

- One `EventTypesModel` can have many `EventsModel` records
- One `EventsModel` can optionally belong to one `EventTypesModel`
- Delete behavior: `SetNull` - If an event type is deleted, the event's `EventTypeId` is set to null (the event is not deleted)

### User ↔ EventsModel (via UserEventsModel)

**Type:** Many-to-Many

- One `User` can be associated with many `EventsModel` records
- One `EventsModel` can be associated with many `User` records
- Junction table: `UserEventsModel`
- Delete behavior: `Cascade` - If a user or event is deleted, all associated `UserEventsModel` records are deleted

**Example:**
```
User 1 ──┐
         ├──→ Event 1
User 2 ──┘
         └──→ Event 2
User 3 ──┐
         └──→ Event 1
```

This configuration allows:
- ✅ Multiple users to be linked to the same event
- ✅ One user to be linked to multiple events
- ❌ The same user cannot be linked to the same event twice (prevented by unique constraint)

---

## Database Context Configuration

All three models are registered in the `DatabaseContext`:

```csharp
public DbSet<EventTypesModel> EventTypes { get; set; }
public DbSet<EventsModel> Events { get; set; }
public DbSet<UserEventsModel> UserEvents { get; set; }
public DbSet<EventSettingsModel> EventSettings { get; set; }
```

---

## Usage Examples

### Creating an Event with Type

```csharp
var eventType = new EventTypesModel { Name = "Meeting" };
var newEvent = new EventsModel 
{
    EventTypeId = eventType.Id,
    Title = "Team Standup",
    Description = "Daily team standup meeting",
    StartDateTime = DateTime.Now.AddDays(1),
    EndDateTime = DateTime.Now.AddDays(1).AddHours(1),
    CanBePostponed = true
};
```

### Linking Users to an Event

```csharp
// Link User 1 to Event 1
var userEvent1 = new UserEventsModel 
{
    UserId = 1,
    EventId = 1
};

// Link User 2 to the same Event 1 (allowed)
var userEvent2 = new UserEventsModel 
{
    UserId = 2,
    EventId = 1
};
```

### Querying Events for a User

```csharp
var userEvents = context.UserEvents
    .Where(ue => ue.UserId == userId)
    .Include(ue => ue.Event)
    .ThenInclude(e => e.EventType)
    .Select(ue => ue.Event)
    .ToList();
```

### Querying Users for an Event

```csharp
var eventUsers = context.UserEvents
    .Where(ue => ue.EventId == eventId)
    .Include(ue => ue.User)
    .Select(ue => ue.User)
    .ToList();
```

### Managing User Event Settings

```csharp
// Create settings for a user
var userSettings = new EventSettingsModel
{
    UserId = 1,
    FollowUpPeriodDays = 7
};

// Get settings for a user
var settings = await context.EventSettings
    .FirstOrDefaultAsync(es => es.UserId == userId);

// Update settings
if (settings != null)
{
    settings.FollowUpPeriodDays = 14;
    await context.SaveChangesAsync();
}
```

---

## Notes

- All models inherit from `ModelBase`, which provides `Id`, `CreatedAt`, and `UpdatedAt` properties
- The `UserEventsModel` uses `Id` as the primary key (not a composite key) for consistency with other junction tables in the system
- The unique constraint on `(UserId, EventId)` ensures data integrity by preventing duplicate associations
- Cascade delete behavior ensures referential integrity when users or events are removed

