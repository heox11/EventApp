-- Insert a test event
INSERT INTO `Events` (`Name`, `EventDate`, `Location`, `AdditionalInfo`)
VALUES (
    'Test Event',
    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 7 DAY),
    'Test Location',
    'This is a test event'
); 