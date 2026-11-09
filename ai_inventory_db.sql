-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Nov 09, 2026 at 09:31 AM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `ai_inventory_db`
--

-- --------------------------------------------------------

--
-- Table structure for table `activity_logs`
--

CREATE TABLE `activity_logs` (
  `LogID` int(11) NOT NULL,
  `TransactionDate` datetime DEFAULT current_timestamp(),
  `Username` varchar(50) NOT NULL,
  `Role` varchar(50) NOT NULL,
  `ActionType` varchar(50) NOT NULL,
  `Description` text NOT NULL,
  `RecordID` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `activity_logs`
--

INSERT INTO `activity_logs` (`LogID`, `TransactionDate`, `Username`, `Role`, `ActionType`, `Description`, `RecordID`) VALUES
(10, '2026-10-29 10:28:26', 'Marc Bamboo', 'Staff', 'STOCK-OUT', 'Finalized POS sale cart checkout totaling ₱ 450.00 using FEFO enforcement routing.', 'REC-24454068'),
(11, '2026-10-29 11:12:58', 'Justine Cedric', 'Admin', 'PRODUCT-REG', 'Registered product: Jackhammer with Stock of 50.', '4800012345678'),
(12, '2026-11-06 20:28:03', 'Marc Bamboo', 'Staff', 'STOCK-OUT', 'Finalized POS sale cart checkout totaling ₱ 10.00 using FEFO enforcement routing.', 'REC-72667677'),
(13, '2026-11-08 13:42:36', 'Marc Bamboo', 'Staff', 'STOCK-OUT', 'Finalized POS sale cart checkout totaling ₱ 40.00 using FEFO enforcement routing.', 'REC-81405327'),
(14, '2026-11-09 13:13:00', 'Marc Bamboo', 'Staff', 'STOCK-OUT', 'Finalized POS sale cart checkout totaling ₱ 144.00 using FEFO enforcement routing.', 'REC-36766686'),
(15, '2026-11-09 14:05:01', 'Marc Bamboo', 'Staff', 'STOCK-OUT', 'Finalized POS sale cart checkout totaling ₱ 120.00 using FEFO enforcement routing.', 'REC-07318011');

-- --------------------------------------------------------

--
-- Table structure for table `categories`
--

CREATE TABLE `categories` (
  `CategoryID` int(11) NOT NULL,
  `CategoryName` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `categories`
--

INSERT INTO `categories` (`CategoryID`, `CategoryName`) VALUES
(1, 'Appliances'),
(3, 'Containers'),
(6, 'Food & Canteen Merchandise'),
(4, 'Medicines & Clinic Supplies'),
(2, 'Office Supplies'),
(5, 'School Uniforms');

-- --------------------------------------------------------

--
-- Table structure for table `customers`
--

CREATE TABLE `customers` (
  `CustomerID` int(11) NOT NULL,
  `CustomerName` varchar(100) NOT NULL,
  `ContactNumber` varchar(20) DEFAULT NULL,
  `PurchaseCount` int(11) DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `customers`
--

INSERT INTO `customers` (`CustomerID`, `CustomerName`, `ContactNumber`, `PurchaseCount`) VALUES
(1, 'Walk-In Customer', 'N/A', 7);

-- --------------------------------------------------------

--
-- Table structure for table `products`
--

CREATE TABLE `products` (
  `ProductID` int(11) NOT NULL,
  `Barcode` varchar(50) DEFAULT NULL,
  `ProductName` varchar(150) NOT NULL,
  `Description` text NOT NULL,
  `CategoryID` int(11) DEFAULT NULL,
  `Price` decimal(10,2) NOT NULL,
  `CurrentStock` int(11) DEFAULT 0,
  `ExpirationDate` date DEFAULT NULL,
  `LowStockThreshold` int(11) DEFAULT 5,
  `ProductEmbedding` text DEFAULT NULL,
  `CreatedAt` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `products`
--

INSERT INTO `products` (`ProductID`, `Barcode`, `ProductName`, `Description`, `CategoryID`, `Price`, `CurrentStock`, `ExpirationDate`, `LowStockThreshold`, `ProductEmbedding`, `CreatedAt`) VALUES
(1, '4800012345678', 'Paracetamol 500mg Biogesic', 'Biogesic paracetamol tablet for common headache relief, minor body pains, and quick clinical fever reduction. Location: [Medicine Cabinet A, Shelf 1]', 4, 10.00, 50, '2029-01-01', 20, 'WtEywSeoM0CY0ZVBRBNOwQwnJUElegPAu0WrQZR4yEH8fqe/rXmqv1FrM7+dpDvBFAMlQUCumkAmUoXAc0wpwUIzMUHz/vJA1VWBwYYnmUF4wAZAHeiLQNnurD4fcKRAYeYDQICorj8SOWNBduhewSxXD8HCXALCLOpDQUDVXkEQOB5BogvtwJiO9kDsw6NACbiOQBfCakFsZdw/oscHQZQa/0Di0odADUWPQPzqor9ZM+I//n7hwHHdUMEWGK1AZekTQpHKycB1kpDAI88RwWqmSsCgr/dAbCCcQChyYMHX7NzAcg1+v9te1sC3XWZBfoKgQFQ6NEFQayTCmiGzQQIoXMCK7TjBLhMBQQSrAD8exUpBcYmbQSMilsFbJS/AiekZQTlBKEEpMfBAH1n0wPnEX0C5ow/Ba06mQPjEB8DOmV/ArFZOvxYFhUDE+cbBRIbnPxMaqMHd0lVBdguGQB4xJ0GPehfBqrMhQJUoh8DBPGzBg2D+wLxqqcEbESzBgsXewGk0F0HXLcPBu87pQQLsQ8F1Lb4+O+wvwTtwUsEUHh5BuEekwSDfZL+Fk0LBa+dYweoe90A9+VbAOmQpwMAtkkAGcys+Lg8VwPK85T+5uqFABkz+vq2assD1CoPBgVI8P9TqvsCpMopBMiDOwK0+SUFfUAjAe+NYwP23UkHLIYhAjgn4QCmOv0FgnrI9HEMPv5p9ZcD/V6xAExUdwYnMfkB8NQFB5HdEQcfl0EBI0IE/3o60wc75KsH1OqbBZLrswTdZGUEyGc7Ayb0JQXASjMH4Yc8+MtjNv1Q5s0HAcLnAO1m3QX4QikF3FUJBXdqXQY0WW8HIklLBAdGjwDXvJUAhHCrAWGAHwMwoOUJH39bA1ys1wfuHj8AfGABBYYBewGBnukFC8f8/vFFmQQC5gL/QQPBAaMNXQByWv0CB4Yk/OqlPwXQB40D58/W/idigQaAoQUAey5/Awm4yP+Uy1z5spRbADWfEQGNGij89yyBBIoOLwaYMmcEgs48/0rmYwE0iiEEJDk/AUIvnwKfk2cABNi3A2AzmQAHGX8BV/utBNnIEwEK5aD9WSb5BYT2tQWefMsAoDFu/x7E7v1mWBUEZfS+/KEaJQPCOSUFeh5VB/SHPwKkSV8EPFjZBAiHWwNEbyT/hIDO/XYZEQYqszz9+E5rAbKWaPZofGMIud1JBhBbxQOxZbcHNB71BX3Aiwe+Hs8BKOTJAYNZ7vwypxj8AX2VBLMiMwF4ZjsHyq4bAR8EawZ5wNkFg0D1BWqnXv9eB28DW8DBBUqctQTiSCcHPjEvBhKd/P7SXpMAsE7fAuZgGQpKclr+HGaO/+tdVwT/KNUGhsUNB0noawOuYrsHIk0vARB04wPJKVsHsxBA+beCDwOgMpz+I2Fw/WZ+PQftQ20A32pjB8jVcwTiIScHEAGJA2BYhweglo8BH5blAlEVwQPmOeMD9nCfBUsgOwegENUExZnXBIspWwT+YMMAS8onB5X1XQGgyBcCRDCNBaWnbQGbqasHYesrAUTqcQIO6iT8qXmRAesCoQQcn4cBvDAhB4ZTJQR3pLMFvPIhBk/ydQXGIn0BMkDxBzMejQIj7fsE3oRo/zdcywUkwIEGS3qlAb0awQLX9EUDDr57ANKQ0Qel7C8ErgFZBS8BTwRvwMcHIazrAuwR7wLncOkDPK0hBbe7uwNYXdsJBo9BA3xgQQcrLpsGEcEbBtFNpQSxrmsASjRO/M91vwZqW0EAZtDfAVk/Ov6SuakH/3Oc/0MsaQUpia8BCfThA0wejwVuHAEHUAj/BiLxBQDQUOkCApApCXRaLP7OJWcBP/wZB7DayQPobkUDniCFAaG4RQaWzMMEk4C/BAi0KQZo7WMH9gDNBDFeCQXbvZcFlXUJAY4kbQbc9t0C4QsQ/KArkvyjeMcBqC7HAtcffQDQ3mMHQyVDB93q4wTl0W0AYqby//RezwEe20cCS3ZY/Mpi8QMM6gkCnVmm/yYDJP7pRE8FPw4XBxUBFwRj3FMFkTzvB8LeowIFGBL94OppA', '2026-10-20 12:35:29'),
(2, '4800012345679', 'Mefenamic Acid 500mg Dolfenal', 'Dolfenal capsule indicated for the treatment of severe toothache, dysmenorrhea, postoperative pain, and acute muscle body pain relief. Location: [Medicine Cabinet A, Shelf 2]', 4, 10.00, 72, '2027-01-28', 5, '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', '2026-10-20 12:36:12'),
(3, '4800012345680', 'Isopropyl Alcohol 70% Green Cross', '70% Isopropyl Alcohol antiseptic topical solution for everyday sanitation, hand sterilization, and emergency first aid wound disinfecting Location: [Clinic Desk 1]', 4, 20.00, 951, '2028-06-23', 5, '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', '2026-10-20 12:37:08'),
(4, '4800098765431', 'Coca-Cola Can 325ml', 'Chilled aluminum can of sweet carbonated cola soda refreshment drink. Best served cold with ice. Location: [Store Fridge, Row 1]', 1, 150.00, 6, '2027-10-20', 15, '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', '2026-10-20 12:37:53'),
(16, '4800012345679', 'Mefenamic Acid 500mg Dolfenal', 'Dolfenal capsule indicated for the treatment of severe toothache, dysmenorrhea, postoperative pain, and acute muscle body pain relief. Location: [Medicine Cabinet A, Shelf 2]', 4, 10.00, 8, '2026-10-27', 5, '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', '2026-10-27 10:52:21'),
(17, '4800098765431', 'Coca-Cola Can 325ml', 'Chilled aluminum can of sweet carbonated cola soda refreshment drink. Best served cold with ice. Location: [Store Fridge, Row 1]', 1, 150.00, 10, '2028-10-27', 15, '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', '2026-10-27 10:52:35'),
(18, '4800012345678', 'Jackhammer', 'Yummy, Juicy, Tender, Engine, War Location: [Rack C, Shelf 5]', 1, 100.00, 50, '2026-10-29', 5, '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', '2026-10-29 03:12:58');

-- --------------------------------------------------------

--
-- Table structure for table `roles`
--

CREATE TABLE `roles` (
  `RoleID` int(11) NOT NULL,
  `RoleName` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `roles`
--

INSERT INTO `roles` (`RoleID`, `RoleName`) VALUES
(1, 'Admin'),
(2, 'Inventory Manager'),
(3, 'Staff');

-- --------------------------------------------------------

--
-- Table structure for table `stocktransactions`
--

CREATE TABLE `stocktransactions` (
  `TransactionID` int(11) NOT NULL,
  `ProductID` int(11) DEFAULT NULL,
  `OrderGroupID` varchar(50) DEFAULT NULL,
  `Barcode` varchar(50) DEFAULT NULL,
  `UserID` int(11) DEFAULT 1,
  `CustomerID` int(11) DEFAULT 1,
  `TransactionType` varchar(10) NOT NULL,
  `DiscountType` varchar(50) DEFAULT 'None',
  `DiscountAmount` decimal(10,2) DEFAULT 0.00,
  `FinalTotal` decimal(10,2) DEFAULT 0.00,
  `CashTendered` decimal(10,2) DEFAULT 0.00,
  `ChangeDue` decimal(10,2) DEFAULT 0.00,
  `Quantity` int(11) NOT NULL,
  `Remarks` text DEFAULT NULL,
  `TransactionDate` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `stocktransactions`
--

INSERT INTO `stocktransactions` (`TransactionID`, `ProductID`, `OrderGroupID`, `Barcode`, `UserID`, `CustomerID`, `TransactionType`, `DiscountType`, `DiscountAmount`, `FinalTotal`, `CashTendered`, `ChangeDue`, `Quantity`, `Remarks`, `TransactionDate`) VALUES
(23, 4, 'REC-24454068', '4800098765431', 1, 1, 'STOCK-OUT', 'None (Standard Rate)', 0.00, 450.00, 500.00, 50.00, 3, 'POS Sale', '2026-10-29 02:28:26'),
(24, 16, 'REC-72667677', '4800012345679', 1, 1, 'STOCK-OUT', 'None (Standard Rate)', 0.00, 10.00, 20.00, 10.00, 1, 'POS Sale', '2026-11-06 12:28:03'),
(25, 3, 'REC-81405327', '4800012345680', 1, 1, 'STOCK-OUT', 'None (Standard Rate)', 0.00, 40.00, 50.00, 10.00, 2, 'POS Sale', '2026-11-08 05:42:36'),
(26, 16, 'REC-36766686', '4800012345679', 1, 1, 'STOCK-OUT', '10% Student Discount', 16.00, 144.00, 150.00, 6.00, 1, 'POS Sale', '2026-11-09 05:13:00'),
(27, 4, 'REC-36766686', '4800098765431', 1, 1, 'STOCK-OUT', '10% Student Discount', 16.00, 144.00, 150.00, 6.00, 1, 'POS Sale', '2026-11-09 05:13:00'),
(28, 4, 'REC-07318011', '4800098765431', 1, 1, 'STOCK-OUT', '20% Senior Citizen / PWD', 30.00, 120.00, 150.00, 30.00, 1, 'POS Sale', '2026-11-09 06:05:01');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `UserID` int(11) NOT NULL,
  `Username` varchar(50) NOT NULL,
  `Password` varchar(100) NOT NULL,
  `FullName` varchar(100) NOT NULL,
  `Role` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`UserID`, `Username`, `Password`, `FullName`, `Role`) VALUES
(1, 'admin', 'admin123', 'Justine Cedric', 'Admin'),
(5, 'managerEmhil', 'manager123', 'Emhil Paul Reyes', 'Manager'),
(6, 'Paul', 'admin111', 'Emhil Paul Reyes', 'Admin'),
(7, 'Staff1', 'staff123', 'Marc Bamboo', 'Staff'),
(8, 'manager12', '123123', 'Manager 1', 'Inventory Manager'),
(9, 'StaffPaul', 'paul123', 'Paul Reyes', 'Staff');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `activity_logs`
--
ALTER TABLE `activity_logs`
  ADD PRIMARY KEY (`LogID`);

--
-- Indexes for table `categories`
--
ALTER TABLE `categories`
  ADD PRIMARY KEY (`CategoryID`),
  ADD UNIQUE KEY `CategoryName` (`CategoryName`);

--
-- Indexes for table `customers`
--
ALTER TABLE `customers`
  ADD PRIMARY KEY (`CustomerID`);

--
-- Indexes for table `products`
--
ALTER TABLE `products`
  ADD PRIMARY KEY (`ProductID`),
  ADD KEY `CategoryID` (`CategoryID`);

--
-- Indexes for table `roles`
--
ALTER TABLE `roles`
  ADD PRIMARY KEY (`RoleID`),
  ADD UNIQUE KEY `RoleName` (`RoleName`);

--
-- Indexes for table `stocktransactions`
--
ALTER TABLE `stocktransactions`
  ADD PRIMARY KEY (`TransactionID`),
  ADD KEY `ProductID` (`ProductID`),
  ADD KEY `fk_transaction_customer` (`CustomerID`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`UserID`),
  ADD UNIQUE KEY `Username` (`Username`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `activity_logs`
--
ALTER TABLE `activity_logs`
  MODIFY `LogID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=16;

--
-- AUTO_INCREMENT for table `categories`
--
ALTER TABLE `categories`
  MODIFY `CategoryID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `customers`
--
ALTER TABLE `customers`
  MODIFY `CustomerID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `products`
--
ALTER TABLE `products`
  MODIFY `ProductID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT for table `roles`
--
ALTER TABLE `roles`
  MODIFY `RoleID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `stocktransactions`
--
ALTER TABLE `stocktransactions`
  MODIFY `TransactionID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=29;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `UserID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `products`
--
ALTER TABLE `products`
  ADD CONSTRAINT `products_ibfk_1` FOREIGN KEY (`CategoryID`) REFERENCES `categories` (`CategoryID`);

--
-- Constraints for table `stocktransactions`
--
ALTER TABLE `stocktransactions`
  ADD CONSTRAINT `fk_transaction_customer` FOREIGN KEY (`CustomerID`) REFERENCES `customers` (`CustomerID`),
  ADD CONSTRAINT `stocktransactions_ibfk_1` FOREIGN KEY (`ProductID`) REFERENCES `products` (`ProductID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
