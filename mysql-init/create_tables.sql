CREATE TABLE `Events` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(200) NOT NULL,
    `EventDate` DATETIME(6) NOT NULL,
    `Location` VARCHAR(200) NOT NULL,
    `AdditionalInfo` TEXT NOT NULL,
    `CreatedAt` TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Participants` (
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
