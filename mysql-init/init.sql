-- Create database if it doesn't exist
CREATE DATABASE IF NOT EXISTS `guestregistration`;
USE `guestregistration`;

-- Create Events table
CREATE TABLE IF NOT EXISTS `Events` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(200) NOT NULL,
    `EventDate` DATETIME(6) NOT NULL,
    `Location` VARCHAR(200) NOT NULL,
    `AdditionalInfo` TEXT NOT NULL,
    `CreatedAt` TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create Participants table
CREATE TABLE IF NOT EXISTS `Participants` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `EventId` INT NOT NULL,
    `Type` TINYINT NOT NULL, -- Enum: 0=Individual, 1=Company
    `FirstName` VARCHAR(200) NOT NULL,
    `LastName` VARCHAR(200) NOT NULL,
    `PersonalCode` VARCHAR(100) NOT NULL,
    `CompanyName` VARCHAR(200) NOT NULL,
    `RegistrationCode` VARCHAR(100) NOT NULL,
    `NumberOfParticipants` INT NULL,
    `PaymentMethod` TINYINT NOT NULL, -- Enum: 0=BankTransfer, 1=Cash
    `AdditionalInfo` TEXT NULL,
    `CreatedAt` TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Participants_Events` FOREIGN KEY (`EventId`) REFERENCES `Events`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert test events
INSERT INTO `Events` (`Name`, `EventDate`, `Location`, `AdditionalInfo`)
VALUES 
    ('Tech Conference 2024', '2024-06-15 09:00:00', 'Tallinn Conference Center', 'Annual technology conference with workshops and networking'),
    ('Summer Music Festival', '2024-07-20 14:00:00', 'Pirita Beach', 'Outdoor music festival with food vendors'),
    ('Business Networking Event', '2024-08-10 18:00:00', 'Radisson Blu Hotel', 'Networking event for business professionals'),
    ('Startup Pitch Competition', '2024-09-05 10:00:00', 'Tallinn Creative Hub', 'Annual startup competition with prizes');

-- Insert participants for each event
-- Tech Conference participants
INSERT INTO `Participants` (`EventId`, `Type`, `FirstName`, `LastName`, `PersonalCode`, `CompanyName`, `RegistrationCode`, `NumberOfParticipants`, `PaymentMethod`, `AdditionalInfo`)
VALUES 
    (1, 0, 'John', 'Smith', '12345678901', '', 'TECH001', NULL, 0, 'Interested in AI workshops'),
    (1, 1, '', '', '', 'TechCorp Estonia', 'TECH002', 3, 1, 'Team of developers attending');

-- Summer Music Festival participants
INSERT INTO `Participants` (`EventId`, `Type`, `FirstName`, `LastName`, `PersonalCode`, `CompanyName`, `RegistrationCode`, `NumberOfParticipants`, `PaymentMethod`, `AdditionalInfo`)
VALUES 
    (2, 0, 'Maria', 'Kask', '23456789012', '', 'MUSIC001', NULL, 0, 'VIP ticket holder'),
    (2, 1, '', '', '', 'EventPro OÜ', 'MUSIC002', 5, 1, 'Event staff');

-- Business Networking Event participants
INSERT INTO `Participants` (`EventId`, `Type`, `FirstName`, `LastName`, `PersonalCode`, `CompanyName`, `RegistrationCode`, `NumberOfParticipants`, `PaymentMethod`, `AdditionalInfo`)
VALUES 
    (3, 0, 'Peter', 'Tamm', '34567890123', '', 'NET001', NULL, 0, 'Looking for business partners'),
    (3, 1, '', '', '', 'Business Solutions Ltd', 'NET002', 2, 1, 'Company representatives');

-- Startup Pitch Competition participants
INSERT INTO `Participants` (`EventId`, `Type`, `FirstName`, `LastName`, `PersonalCode`, `CompanyName`, `RegistrationCode`, `NumberOfParticipants`, `PaymentMethod`, `AdditionalInfo`)
VALUES 
    (4, 0, 'Anna', 'Kallas', '45678901234', '', 'START001', NULL, 0, 'Pitching a new app'),
    (4, 1, '', '', '', 'Innovation Hub', 'START002', 4, 1, 'Investors attending'); 