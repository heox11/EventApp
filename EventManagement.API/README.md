# Event Management API

A .NET 9.0 Web API for managing events and participants.

## Prerequisites

- .NET 9.0 SDK
- MySQL Server
- Docker and Docker Compose (optional)
- Visual Studio 2022 or Visual Studio Code

## Setup

### Option 1: Local Development

1. Clone the repository
2. Update the connection string in `appsettings.json` with your MySQL server details:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=eventmanagement;User=your_username;Password=your_password;"
  }
}
```

3. Create the database and tables using the SQL scripts in the `sql` directory:
```bash
mysql -u root -p < sql/01_create_database.sql
mysql -u root -p < sql/02_create_tables.sql
mysql -u root -p < sql/03_sample_data.sql
```

4. Run the following commands in the project directory:
```bash
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. Run the application:
```bash
dotnet run
```

### Option 2: Docker

1. Clone the repository
2. Update the MySQL password in `docker-compose.yml` if needed
3. Run the following command in the root directory:
```bash
docker-compose up --build
```

The API will be available at `https://localhost:5001` and `http://localhost:5000`.

## Database Structure

The database is initialized with the following SQL scripts:
- `01_create_database.sql`: Creates the database
- `02_create_tables.sql`: Creates the tables with proper constraints
- `03_sample_data.sql`: Inserts sample data for testing

### Tables

#### Events
- Id (Primary Key)
- Name (VARCHAR(200), NOT NULL)
- EventDate (DATETIME, NOT NULL)
- Location (VARCHAR(200), NOT NULL)
- AdditionalInfo (VARCHAR(1000))
- CreatedAt (DATETIME, NOT NULL)

#### Participants
- Id (Primary Key)
- EventId (Foreign Key)
- Type (ENUM: 'Individual', 'Company')
- FirstName (VARCHAR(100))
- LastName (VARCHAR(100))
- PersonalCode (VARCHAR(11))
- CompanyName (VARCHAR(200))
- RegistrationCode (VARCHAR(20))
- NumberOfParticipants (INT)
- PaymentMethod (ENUM: 'BankTransfer', 'Cash')
- AdditionalInfo (VARCHAR(5000))
- CreatedAt (DATETIME, NOT NULL)

## API Endpoints

### Events

- GET `/api/Events` - Get all events
- GET `/api/Events/{id}` - Get event by ID
- POST `/api/Events` - Create new event
- PUT `/api/Events/{id}` - Update event
- DELETE `/api/Events/{id}` - Delete event

### Participants

- GET `/api/Participants/event/{eventId}` - Get all participants for an event
- GET `/api/Participants/{id}` - Get participant by ID
- POST `/api/Participants` - Create new participant
- PUT `/api/Participants/{id}` - Update participant
- DELETE `/api/Participants/{id}` - Delete participant

## Features

- Event management (create, read, update, delete)
- Participant management for both individuals and companies
- Validation for Estonian personal codes
- Future date validation for events
- Automatic deletion of participants when an event is deleted
- Support for different payment methods
- Additional information fields for both events and participants
- Docker support for easy deployment
- Automatic database migrations
- Swagger UI for API documentation and testing
- Sample data for testing 