-- Check if tables exist before inserting data
SET @events_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Events');
SET @participants_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Participants');

-- Only proceed if both tables exist
SET @should_proceed = @events_exists * @participants_exists;

-- Start transaction
START TRANSACTION;

-- Insert data only if tables exist
INSERT INTO Events (Name, EventDate, Location, AdditionalInfo)
SELECT 'Tech Conference', '2024-09-01 10:00:00', 'Tallinn', 'Annual tech event'
WHERE @should_proceed = 1;

INSERT INTO Events (Name, EventDate, Location, AdditionalInfo)
SELECT 'Music Festival', '2024-08-15 18:00:00', 'Tartu', 'Outdoor music festival'
WHERE @should_proceed = 1;

INSERT INTO Participants (EventId, Type, FirstName, LastName, PersonalCode, CompanyName, RegistrationCode, NumberOfParticipants, PaymentMethod, AdditionalInfo)
SELECT 1, 0, 'John', 'Doe', '12345678901', '', '', NULL, 'BankTransfer', 'Vegetarian'
WHERE @should_proceed = 1;

INSERT INTO Participants (EventId, Type, FirstName, LastName, PersonalCode, CompanyName, RegistrationCode, NumberOfParticipants, PaymentMethod, AdditionalInfo)
SELECT 1, 1, '', '', '', 'Acme Corp', 'REG123', 5, 'Cash', 'VIP'
WHERE @should_proceed = 1;

INSERT INTO Participants (EventId, Type, FirstName, LastName, PersonalCode, CompanyName, RegistrationCode, NumberOfParticipants, PaymentMethod, AdditionalInfo)
SELECT 2, 0, 'Jane', 'Smith', '10987654321', '', '', NULL, 'BankTransfer', 'Allergic to nuts'
WHERE @should_proceed = 1;

-- Commit transaction
COMMIT; 