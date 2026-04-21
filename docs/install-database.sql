-- ============================================================================
-- ICTD Workflow - Database Installation / Update Script
-- ============================================================================
-- This script is SAFE to run on both new and existing databases.
-- All statements use CREATE TABLE IF NOT EXISTS and INSERT IGNORE
-- so existing data will NOT be overwritten.
--
-- Database: MySQL 8.x / MariaDB 10.x
-- Run as:   mysql -u root -p < install-database.sql
-- ============================================================================

-- ----------------------------
-- 1. Create Database & User
-- ----------------------------
CREATE DATABASE IF NOT EXISTS `VMWorkflow`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_general_ci;

-- Create application user (skip if exists)
CREATE USER IF NOT EXISTS 'vmworkflow'@'localhost' IDENTIFIED BY 'Password@123';
GRANT ALL PRIVILEGES ON `VMWorkflow`.* TO 'vmworkflow'@'localhost';

-- If the app connects from another host (e.g., IIS on a different server):
-- CREATE USER IF NOT EXISTS 'vmworkflow'@'%' IDENTIFIED BY 'Password@123';
-- GRANT ALL PRIVILEGES ON `VMWorkflow`.* TO 'vmworkflow'@'%';

FLUSH PRIVILEGES;

USE `VMWorkflow`;

-- ============================================================================
-- 2. Tables
-- ============================================================================

-- ----------------------------
-- Users
-- ----------------------------
CREATE TABLE IF NOT EXISTS `Users` (
    `UserId`        CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `Username`      VARCHAR(100) NOT NULL,
    `DisplayName`   VARCHAR(200) NOT NULL,
    `Email`         VARCHAR(200) NOT NULL,
    `Role`          VARCHAR(50)  NOT NULL,
    `PasswordHash`  VARCHAR(200) NOT NULL,
    `IsBlocked`     TINYINT(1)   NOT NULL DEFAULT 0,
    `CreatedAt`     DATETIME(6)  NOT NULL,
    `UpdatedAt`     DATETIME(6)  NOT NULL,
    PRIMARY KEY (`UserId`)
) CHARACTER SET utf8mb4;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_Users_Username` ON `Users` (`Username`);

-- ----------------------------
-- Requests (central entity)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `Requests` (
    `RequestId`           CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `ApplicationName`     VARCHAR(200) NOT NULL,
    `ObjectSlug`          VARCHAR(150) NOT NULL,
    `Environment`         VARCHAR(50)  NOT NULL,
    `Status`              VARCHAR(50)  NOT NULL,
    `ExternalSyncStatus`  VARCHAR(50)  NOT NULL,
    `SLA`                 VARCHAR(50)  NOT NULL,
    `ProgrammingLanguage` VARCHAR(100) NULL,
    `Framework`           VARCHAR(200) NULL,
    `Purpose`             TEXT         NULL,
    `ExpectedUsers`       INT          NULL,
    `DBMS`                VARCHAR(100) NULL,
    `GitRepoLink`         VARCHAR(500) NULL,
    `AccessGroup`         VARCHAR(200) NULL,
    `FQDNSuggestion`      VARCHAR(300) NULL,
    `AuthenticationMethod` VARCHAR(200) NULL,
    `IocComments`         TEXT         NULL,
    `CisoDecision`        VARCHAR(50)  NULL,
    `CisoComments`        TEXT         NULL,
    `CisoApprovedBy`      VARCHAR(100) NULL,
    `CisoApprovedAt`      DATETIME(6)  NULL,
    `OpsDecision`         VARCHAR(50)  NULL,
    `OpsComments`         TEXT         NULL,
    `OpsApprovedBy`       VARCHAR(100) NULL,
    `OpsApprovedAt`       DATETIME(6)  NULL,
    `NetBoxId`            VARCHAR(100) NULL,
    `FortiGatePolicyId`   VARCHAR(100) NULL,
    `CreatedBy`           VARCHAR(100) NOT NULL,
    `CreatedAt`           DATETIME(6)  NOT NULL,
    `UpdatedAt`           DATETIME(6)  NOT NULL,
    PRIMARY KEY (`RequestId`)
) CHARACTER SET utf8mb4;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_Requests_ObjectSlug` ON `Requests` (`ObjectSlug`);
CREATE INDEX IF NOT EXISTS `IX_Requests_Status` ON `Requests` (`Status`);
CREATE INDEX IF NOT EXISTS `IX_Requests_CreatedBy` ON `Requests` (`CreatedBy`);
CREATE INDEX IF NOT EXISTS `IX_Requests_CreatedAt` ON `Requests` (`CreatedAt`);

-- ----------------------------
-- SysAdminDetails
-- ----------------------------
CREATE TABLE IF NOT EXISTS `SysAdminDetails` (
    `SysAdminDetailsId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `RequestId`         CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `SensitivityLevel`  VARCHAR(100) NOT NULL,
    `ServerResources`   VARCHAR(50)  NULL,
    `WebServer`         VARCHAR(50)  NOT NULL,
    `DatabaseNameType`  VARCHAR(20)  NOT NULL DEFAULT 'none',
    `DatabaseName`      VARCHAR(200) NULL,
    `DatabaseUsername`   VARCHAR(200) NULL,
    `Hostname`          VARCHAR(200) NOT NULL,
    `SubmittedBy`       VARCHAR(100) NOT NULL,
    `SubmittedAt`       DATETIME(6)  NOT NULL,
    PRIMARY KEY (`SysAdminDetailsId`),
    CONSTRAINT `FK_SysAdminDetails_Requests` FOREIGN KEY (`RequestId`) REFERENCES `Requests` (`RequestId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_SysAdminDetails_RequestId` ON `SysAdminDetails` (`RequestId`);

-- ----------------------------
-- ServiceEntry (child of SysAdminDetails)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `ServiceEntries` (
    `ServiceEntryId`    CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `SysAdminDetailsId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `ServiceName`       VARCHAR(100) NOT NULL,
    `Port`              VARCHAR(50)  NOT NULL,
    `Protocol`          VARCHAR(50)  NOT NULL,
    PRIMARY KEY (`ServiceEntryId`),
    CONSTRAINT `FK_ServiceEntries_SysAdminDetails` FOREIGN KEY (`SysAdminDetailsId`) REFERENCES `SysAdminDetails` (`SysAdminDetailsId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE INDEX IF NOT EXISTS `IX_ServiceEntries_SysAdminDetailsId` ON `ServiceEntries` (`SysAdminDetailsId`);

-- ----------------------------
-- DataCenterDetails
-- ----------------------------
CREATE TABLE IF NOT EXISTS `DataCenterDetails` (
    `DataCenterDetailsId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `RequestId`           CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `Environment`         VARCHAR(50)  NOT NULL,
    `UplinkSpeed`         VARCHAR(50)  NOT NULL,
    `BareMetalType`       VARCHAR(50)  NOT NULL,
    `PortNumber`          VARCHAR(50)  NOT NULL,
    `DC`                  VARCHAR(100) NOT NULL,
    `RackRoom`            VARCHAR(100) NOT NULL,
    `RackNumber`          VARCHAR(100) NOT NULL,
    `Cluster`             VARCHAR(50)  NOT NULL,
    `SubmittedBy`         VARCHAR(100) NOT NULL,
    `SubmittedAt`         DATETIME(6)  NOT NULL,
    PRIMARY KEY (`DataCenterDetailsId`),
    CONSTRAINT `FK_DataCenterDetails_Requests` FOREIGN KEY (`RequestId`) REFERENCES `Requests` (`RequestId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_DataCenterDetails_RequestId` ON `DataCenterDetails` (`RequestId`);

-- ----------------------------
-- NOCDetails
-- ----------------------------
CREATE TABLE IF NOT EXISTS `NOCDetails` (
    `NOCDetailsId`  CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `RequestId`     CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `IPAddress`     VARCHAR(50)  NOT NULL,
    `SubnetMask`    VARCHAR(50)  NOT NULL,
    `VLANID`        VARCHAR(20)  NOT NULL,
    `Gateway`       VARCHAR(50)  NOT NULL,
    `Port`          VARCHAR(50)  NOT NULL,
    `VIP`           VARCHAR(50)  NOT NULL,
    `FQDN`          VARCHAR(200) NOT NULL,
    `VirtualIP`     VARCHAR(50)  NULL,
    `VirtualPort`   VARCHAR(50)  NULL,
    `VirtualFQDN`   VARCHAR(200) NULL,
    `SubmittedBy`   VARCHAR(100) NOT NULL,
    `SubmittedAt`   DATETIME(6)  NOT NULL,
    PRIMARY KEY (`NOCDetailsId`),
    CONSTRAINT `FK_NOCDetails_Requests` FOREIGN KEY (`RequestId`) REFERENCES `Requests` (`RequestId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_NOCDetails_RequestId` ON `NOCDetails` (`RequestId`);

-- ----------------------------
-- NetworkPathEntry (child of NOCDetails)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `NetworkPathEntries` (
    `NetworkPathEntryId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `NOCDetailsId`       CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `SwitchName`         VARCHAR(200) NOT NULL,
    `Port`               VARCHAR(100) NOT NULL,
    `LinkSpeed`          VARCHAR(50)  NULL,
    PRIMARY KEY (`NetworkPathEntryId`),
    CONSTRAINT `FK_NetworkPathEntries_NOCDetails` FOREIGN KEY (`NOCDetailsId`) REFERENCES `NOCDetails` (`NOCDetailsId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE INDEX IF NOT EXISTS `IX_NetworkPathEntries_NOCDetailsId` ON `NetworkPathEntries` (`NOCDetailsId`);

-- ----------------------------
-- SOCDetails
-- ----------------------------
CREATE TABLE IF NOT EXISTS `SOCDetails` (
    `SOCDetailsId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `RequestId`    CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `SubmittedBy`  VARCHAR(100) NOT NULL,
    `SubmittedAt`  DATETIME(6)  NOT NULL,
    PRIMARY KEY (`SOCDetailsId`),
    CONSTRAINT `FK_SOCDetails_Requests` FOREIGN KEY (`RequestId`) REFERENCES `Requests` (`RequestId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_SOCDetails_RequestId` ON `SOCDetails` (`RequestId`);

-- ----------------------------
-- FirewallEntry (child of SOCDetails)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `FirewallEntries` (
    `FirewallEntryId`      CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `SOCDetailsId`         CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `PolicyName`           VARCHAR(200) NOT NULL,
    `VDOM`                 VARCHAR(200) NOT NULL,
    `SourceInterface`      VARCHAR(200) NULL,
    `DestinationInterface` VARCHAR(200) NULL,
    `SourceIP`             VARCHAR(100) NULL,
    `DestinationIP`        VARCHAR(100) NULL,
    `Schedule`             VARCHAR(100) NULL,
    `Action`               VARCHAR(20)  NOT NULL,
    PRIMARY KEY (`FirewallEntryId`),
    CONSTRAINT `FK_FirewallEntries_SOCDetails` FOREIGN KEY (`SOCDetailsId`) REFERENCES `SOCDetails` (`SOCDetailsId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE INDEX IF NOT EXISTS `IX_FirewallEntries_SOCDetailsId` ON `FirewallEntries` (`SOCDetailsId`);

-- ----------------------------
-- FirewallServiceEntry (child of FirewallEntry)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `FirewallServiceEntries` (
    `FirewallServiceEntryId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `FirewallEntryId`        CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `Port`                   VARCHAR(10)  NOT NULL,
    `Protocol`               VARCHAR(10)  NOT NULL,
    `ServiceName`            VARCHAR(100) NULL,
    PRIMARY KEY (`FirewallServiceEntryId`),
    CONSTRAINT `FK_FirewallServiceEntries_FirewallEntries` FOREIGN KEY (`FirewallEntryId`) REFERENCES `FirewallEntries` (`FirewallEntryId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE INDEX IF NOT EXISTS `IX_FirewallServiceEntries_FirewallEntryId` ON `FirewallServiceEntries` (`FirewallEntryId`);

-- ----------------------------
-- SecurityProfile (lookup)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `SecurityProfiles` (
    `SecurityProfileId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `Name`              VARCHAR(100) NOT NULL,
    `CreatedAt`         DATETIME(6)  NOT NULL,
    `CreatedBy`         VARCHAR(200) NULL,
    PRIMARY KEY (`SecurityProfileId`)
) CHARACTER SET utf8mb4;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_SecurityProfiles_Name` ON `SecurityProfiles` (`Name`);

-- ----------------------------
-- FirewallEntrySecurityProfile (many-to-many join)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `FirewallEntrySecurityProfiles` (
    `FirewallEntryId`   CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `SecurityProfileId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    PRIMARY KEY (`FirewallEntryId`, `SecurityProfileId`),
    CONSTRAINT `FK_FESP_FirewallEntries` FOREIGN KEY (`FirewallEntryId`) REFERENCES `FirewallEntries` (`FirewallEntryId`) ON DELETE CASCADE,
    CONSTRAINT `FK_FESP_SecurityProfiles` FOREIGN KEY (`SecurityProfileId`) REFERENCES `SecurityProfiles` (`SecurityProfileId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

-- ----------------------------
-- ResourceGroup (lookup)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `ResourceGroups` (
    `ResourceGroupId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `Name`            VARCHAR(50)  NOT NULL,
    `VCpu`            INT          NOT NULL DEFAULT 0,
    `Ram`             VARCHAR(20)  NOT NULL DEFAULT '',
    `Hdd`             VARCHAR(20)  NOT NULL DEFAULT '',
    `CreatedAt`       DATETIME(6)  NOT NULL,
    `CreatedBy`       VARCHAR(200) NULL,
    PRIMARY KEY (`ResourceGroupId`)
) CHARACTER SET utf8mb4;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_ResourceGroups_Name` ON `ResourceGroups` (`Name`);

-- ----------------------------
-- Vdom (lookup)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `Vdoms` (
    `VdomId`    CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `Name`      VARCHAR(100) NOT NULL,
    `CreatedAt` DATETIME(6)  NOT NULL,
    `CreatedBy` VARCHAR(200) NULL,
    PRIMARY KEY (`VdomId`)
) CHARACTER SET utf8mb4;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_Vdoms_Name` ON `Vdoms` (`Name`);

-- ----------------------------
-- DropdownOption (lookup)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `DropdownOptions` (
    `DropdownOptionId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `Category`         VARCHAR(50)  NOT NULL,
    `Value`            VARCHAR(100) NOT NULL,
    `SortOrder`        INT          NOT NULL DEFAULT 0,
    `CreatedAt`        DATETIME(6)  NOT NULL,
    `CreatedBy`        VARCHAR(200) NULL,
    PRIMARY KEY (`DropdownOptionId`)
) CHARACTER SET utf8mb4;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_DropdownOptions_Category_Value` ON `DropdownOptions` (`Category`, `Value`);

-- ----------------------------
-- StatusHistory
-- ----------------------------
CREATE TABLE IF NOT EXISTS `StatusHistories` (
    `StatusHistoryId` CHAR(36)      NOT NULL COLLATE ascii_general_ci,
    `RequestId`       CHAR(36)      NOT NULL COLLATE ascii_general_ci,
    `OldStatus`       VARCHAR(50)   NOT NULL,
    `NewStatus`       VARCHAR(50)   NOT NULL,
    `ChangedBy`       VARCHAR(100)  NOT NULL,
    `Comments`        VARCHAR(1000) NULL,
    `Timestamp`       DATETIME(6)   NOT NULL,
    PRIMARY KEY (`StatusHistoryId`),
    CONSTRAINT `FK_StatusHistories_Requests` FOREIGN KEY (`RequestId`) REFERENCES `Requests` (`RequestId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE INDEX IF NOT EXISTS `IX_StatusHistories_RequestId_Timestamp` ON `StatusHistories` (`RequestId`, `Timestamp`);

-- ----------------------------
-- AutomationLog
-- ----------------------------
CREATE TABLE IF NOT EXISTS `AutomationLogs` (
    `AutomationLogId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `RequestId`       CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `Action`          VARCHAR(200) NOT NULL,
    `Payload`         TEXT         NULL,
    `Response`        TEXT         NULL,
    `Success`         TINYINT(1)   NOT NULL DEFAULT 0,
    `Timestamp`       DATETIME(6)  NOT NULL,
    PRIMARY KEY (`AutomationLogId`),
    CONSTRAINT `FK_AutomationLogs_Requests` FOREIGN KEY (`RequestId`) REFERENCES `Requests` (`RequestId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE INDEX IF NOT EXISTS `IX_AutomationLogs_RequestId` ON `AutomationLogs` (`RequestId`);

-- ----------------------------
-- AuditLog
-- ----------------------------
CREATE TABLE IF NOT EXISTS `AuditLogs` (
    `AuditLogId`  CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `User`        VARCHAR(100) NOT NULL,
    `HttpMethod`  VARCHAR(10)  NOT NULL,
    `Path`        VARCHAR(500) NOT NULL,
    `StatusCode`  INT          NOT NULL,
    `Timestamp`   DATETIME(6)  NOT NULL,
    PRIMARY KEY (`AuditLogId`)
) CHARACTER SET utf8mb4;

CREATE INDEX IF NOT EXISTS `IX_AuditLogs_Timestamp` ON `AuditLogs` (`Timestamp`);
CREATE INDEX IF NOT EXISTS `IX_AuditLogs_User` ON `AuditLogs` (`User`);

-- ----------------------------
-- Scripts
-- ----------------------------
CREATE TABLE IF NOT EXISTS `Scripts` (
    `ScriptId`    CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `RequestId`   CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `ScriptType`  VARCHAR(50)  NOT NULL DEFAULT 'FortiGate',
    `Content`     TEXT         NOT NULL,
    `FileName`    VARCHAR(250) NOT NULL,
    `GeneratedBy` VARCHAR(100) NOT NULL,
    `GeneratedAt` DATETIME(6)  NOT NULL,
    PRIMARY KEY (`ScriptId`),
    CONSTRAINT `FK_Scripts_Requests` FOREIGN KEY (`RequestId`) REFERENCES `Requests` (`RequestId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE INDEX IF NOT EXISTS `IX_Scripts_RequestId` ON `Scripts` (`RequestId`);
CREATE INDEX IF NOT EXISTS `IX_Scripts_GeneratedAt` ON `Scripts` (`GeneratedAt`);

-- ----------------------------
-- ApplicationDependency
-- ----------------------------
CREATE TABLE IF NOT EXISTS `ApplicationDependencies` (
    `ApplicationDependencyId` CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `RequestId`               CHAR(36)     NOT NULL COLLATE ascii_general_ci,
    `DependencyName`          VARCHAR(200) NOT NULL,
    `Port`                    INT          NOT NULL DEFAULT 0,
    `Protocol`                VARCHAR(20)  NOT NULL DEFAULT 'TCP',
    PRIMARY KEY (`ApplicationDependencyId`),
    CONSTRAINT `FK_ApplicationDependencies_Requests` FOREIGN KEY (`RequestId`) REFERENCES `Requests` (`RequestId`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;

CREATE INDEX IF NOT EXISTS `IX_ApplicationDependencies_RequestId` ON `ApplicationDependencies` (`RequestId`);


-- ============================================================================
-- 3. Seed Data (INSERT IGNORE = skip if already exists)
-- ============================================================================

-- ----------------------------
-- Default Users (password: Password@123)
-- ----------------------------
-- BCrypt hash for "Password@123" - generated by the application
-- The app seeds these automatically on startup, but we include them here
-- for manual database setup without running the app first.

-- NOTE: If using Active Directory, these local users serve as fallback only.
-- AD users are auto-provisioned on first login.

-- ----------------------------
-- Default VDOMs
-- ----------------------------
INSERT IGNORE INTO `Vdoms` (`VdomId`, `Name`, `CreatedAt`, `CreatedBy`) VALUES
    (UUID(), 'root',     NOW(6), 'system'),
    (UUID(), 'DMZ',      NOW(6), 'system'),
    (UUID(), 'Internal', NOW(6), 'system'),
    (UUID(), 'Guest',    NOW(6), 'system');

-- ----------------------------
-- Default Security Profiles
-- ----------------------------
INSERT IGNORE INTO `SecurityProfiles` (`SecurityProfileId`, `Name`, `CreatedAt`, `CreatedBy`) VALUES
    (UUID(), 'Web-Filter-Default',  NOW(6), 'system'),
    (UUID(), 'AV-Default',          NOW(6), 'system'),
    (UUID(), 'IPS-Default',         NOW(6), 'system'),
    (UUID(), 'App-Control-Default', NOW(6), 'system');

-- ----------------------------
-- Default Resource Groups
-- ----------------------------
INSERT IGNORE INTO `ResourceGroups` (`ResourceGroupId`, `Name`, `VCpu`, `Ram`, `Hdd`, `CreatedAt`, `CreatedBy`) VALUES
    (UUID(), 'Production-RG',  0, '', '', NOW(6), 'system'),
    (UUID(), 'Staging-RG',     0, '', '', NOW(6), 'system'),
    (UUID(), 'Development-RG', 0, '', '', NOW(6), 'system');

-- ----------------------------
-- Default Dropdown Options
-- ----------------------------
INSERT IGNORE INTO `DropdownOptions` (`DropdownOptionId`, `Category`, `Value`, `SortOrder`, `CreatedAt`, `CreatedBy`) VALUES
    -- SensitivityLevel
    (UUID(), 'SensitivityLevel', 'Low',      0, NOW(6), 'system'),
    (UUID(), 'SensitivityLevel', 'Medium',   1, NOW(6), 'system'),
    (UUID(), 'SensitivityLevel', 'High',     2, NOW(6), 'system'),
    (UUID(), 'SensitivityLevel', 'Critical', 3, NOW(6), 'system'),
    -- WebServer
    (UUID(), 'WebServer', 'IIS',    0, NOW(6), 'system'),
    (UUID(), 'WebServer', 'Apache', 1, NOW(6), 'system'),
    (UUID(), 'WebServer', 'Nginx',  2, NOW(6), 'system'),
    -- DBMS
    (UUID(), 'DBMS', 'PostgreSQL', 0, NOW(6), 'system'),
    (UUID(), 'DBMS', 'MySQL',      1, NOW(6), 'system'),
    (UUID(), 'DBMS', 'SQL Server', 2, NOW(6), 'system'),
    (UUID(), 'DBMS', 'Oracle',     3, NOW(6), 'system'),
    (UUID(), 'DBMS', 'MongoDB',    4, NOW(6), 'system'),
    (UUID(), 'DBMS', 'MariaDB',    5, NOW(6), 'system'),
    (UUID(), 'DBMS', 'SQLite',     6, NOW(6), 'system'),
    (UUID(), 'DBMS', 'None',       7, NOW(6), 'system'),
    -- SLA
    (UUID(), 'SLA', 'Standard',        0, NOW(6), 'system'),
    (UUID(), 'SLA', 'Critical',        1, NOW(6), 'system'),
    (UUID(), 'SLA', 'MissionCritical', 2, NOW(6), 'system'),
    -- Environment
    (UUID(), 'Environment', 'Development', 0, NOW(6), 'system'),
    (UUID(), 'Environment', 'Staging',     1, NOW(6), 'system'),
    (UUID(), 'Environment', 'Production',  2, NOW(6), 'system'),
    (UUID(), 'Environment', 'DR',          3, NOW(6), 'system'),
    -- ServerEnvironment
    (UUID(), 'ServerEnvironment', 'Dell',    0, NOW(6), 'system'),
    (UUID(), 'ServerEnvironment', 'HyperV',  1, NOW(6), 'system'),
    -- BareMetalType
    (UUID(), 'BareMetalType', 'VM',       0, NOW(6), 'system'),
    (UUID(), 'BareMetalType', 'Physical', 1, NOW(6), 'system'),
    -- Cluster
    (UUID(), 'Cluster', 'HyperFlex', 0, NOW(6), 'system'),
    (UUID(), 'Cluster', 'VxRail',    1, NOW(6), 'system');


-- ============================================================================
-- 4. Schema Updates (safe to re-run on existing databases)
-- ============================================================================
-- Add columns that may be missing from older installations.
-- ALTER TABLE ... ADD COLUMN IF NOT EXISTS requires MariaDB 10.0.2+ or MySQL 8.0+
-- If your MySQL version doesn't support IF NOT EXISTS on ALTER, wrap in a procedure.

-- Example: If you add new columns in future releases, add them here:
-- ALTER TABLE `Requests` ADD COLUMN IF NOT EXISTS `NewColumn` VARCHAR(100) NULL AFTER `UpdatedAt`;


-- ============================================================================
-- 5. Verification
-- ============================================================================
SELECT 'Tables created:' AS `Status`;
SELECT TABLE_NAME, TABLE_ROWS
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'VMWorkflow'
ORDER BY TABLE_NAME;

SELECT '=== ICTD Workflow database installation complete ===' AS `Status`;
