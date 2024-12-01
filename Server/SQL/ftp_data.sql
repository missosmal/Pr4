CREATE DATABASE  IF NOT EXISTS `ftp_data` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `ftp_data`;
-- MySQL dump 10.13  Distrib 8.0.26, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: ftp_data
-- ------------------------------------------------------
-- Server version	8.0.30

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `history`
--

DROP TABLE IF EXISTS `history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `history` (
  `id` int NOT NULL AUTO_INCREMENT,
  `username` varchar(100) DEFAULT NULL,
  `password` varchar(255) DEFAULT NULL,
  `command` varchar(45) DEFAULT NULL,
  `sendDate` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=93 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `history`
--

LOCK TABLES `history` WRITE;
/*!40000 ALTER TABLE `history` DISABLE KEYS */;
INSERT INTO `history` VALUES (1,'sasha','Asdfg123',NULL,NULL),(2,'sasha','Asdfg123','connect sasha Asdfg123','2024-12-01 16:40:30'),(3,'sasha','Asdfg123','cd','2024-12-01 16:41:13'),(4,'sasha','Asdfg123','cd \\RAMMap/','2024-12-01 16:42:33'),(5,'sasha','Asdfg123','get Eula.txt','2024-12-01 16:43:01'),(6,'sasha','Asdfg123','get Eula.txt','2024-12-01 16:45:50'),(7,'sasha','Asdfg123','System.String[]','2024-12-01 16:47:59'),(8,'sasha','Asdfg123','cd','2024-12-01 16:48:01'),(9,'sasha','Asdfg123','cd \\icons8-whole-apple-50.png','2024-12-01 16:48:03'),(10,'sasha','Asdfg123','cd \\icons8-whole-apple-50.png','2024-12-01 16:48:05'),(11,'sasha','Asdfg123','connect','2024-12-01 16:57:06'),(12,'sasha','Asdfg123','cd','2024-12-01 16:57:07'),(13,'sasha','Asdfg123','get','2024-12-01 16:57:11'),(14,'sasha','Asdfg123','connect','2024-12-01 16:59:18'),(15,'sasha','Asdfg123','cd','2024-12-01 16:59:19'),(16,'sasha','Asdfg123','get','2024-12-01 16:59:20'),(17,'sasha','Asdfg123','connect','2024-12-01 17:08:29'),(18,'sasha','Asdfg123','cd','2024-12-01 17:08:30'),(19,'sasha','Asdfg123','cd','2024-12-01 17:08:31'),(20,'sasha','Asdfg123','connect','2024-12-01 17:09:20'),(21,'sasha','Asdfg123','cd','2024-12-01 17:09:21'),(22,'sasha','Asdfg123','cd','2024-12-01 17:09:22'),(23,'sasha','Asdfg123','get','2024-12-01 17:09:24'),(24,'sasha','Asdfg123','connect','2024-12-01 17:09:47'),(25,'sasha','Asdfg123','cd','2024-12-01 17:09:48'),(26,'sasha','Asdfg123','connect','2024-12-01 17:09:54'),(27,'sasha','Asdfg123','cd','2024-12-01 17:09:55'),(28,'sasha','Asdfg123','connect','2024-12-01 17:10:01'),(29,'sasha','Asdfg123','cd','2024-12-01 17:10:02'),(30,'sasha','Asdfg123','connect','2024-12-01 17:11:47'),(31,'sasha','Asdfg123','cd','2024-12-01 17:11:48'),(32,'sasha','Asdfg123','cd','2024-12-01 17:11:49'),(33,'sasha','Asdfg123','get','2024-12-01 17:11:51'),(34,'sasha','Asdfg123','get','2024-12-01 17:11:57'),(35,'sasha','Asdfg123','get','2024-12-01 17:12:03'),(36,'sasha','Asdfg123','connect','2024-12-01 17:12:18'),(37,'sasha','Asdfg123','cd','2024-12-01 17:12:19'),(38,'sasha','Asdfg123','get','2024-12-01 17:12:20'),(39,'sasha','Asdfg123','connect','2024-12-01 17:13:32'),(40,'sasha','Asdfg123','cd','2024-12-01 17:13:33'),(41,'sasha','Asdfg123','get','2024-12-01 17:13:35'),(42,'sasha','Asdfg123','connect','2024-12-01 17:16:57'),(43,'sasha','Asdfg123','cd','2024-12-01 17:16:59'),(44,'sasha','Asdfg123','get','2024-12-01 17:17:00'),(45,'sasha','Asdfg123','connect','2024-12-01 17:18:53'),(46,'sasha','Asdfg123','cd','2024-12-01 17:18:54'),(47,'sasha','Asdfg123','get','2024-12-01 17:19:17'),(48,'sasha','Asdfg123','connect','2024-12-01 17:23:32'),(49,'sasha','Asdfg123','cd','2024-12-01 17:23:33'),(50,'sasha','Asdfg123','get','2024-12-01 17:23:42'),(51,'sasha','Asdfg123','connect','2024-12-01 17:26:08'),(52,'sasha','Asdfg123','cd','2024-12-01 17:26:09'),(53,'sasha','Asdfg123','get','2024-12-01 17:26:19'),(54,'sasha','Asdfg123','connect','2024-12-01 17:33:01'),(55,'sasha','Asdfg123','cd','2024-12-01 17:33:02'),(56,'sasha','Asdfg123','get','2024-12-01 17:33:11'),(57,'sasha','Asdfg123','connect','2024-12-01 17:35:08'),(58,'sasha','Asdfg123','cd','2024-12-01 17:35:09'),(59,'sasha','Asdfg123','get','2024-12-01 17:35:10'),(60,'sasha','Asdfg123','connect','2024-12-01 17:37:26'),(61,'sasha','Asdfg123','cd','2024-12-01 17:37:27'),(62,'sasha','Asdfg123','get','2024-12-01 17:37:36'),(63,'sasha','Asdfg123','connect','2024-12-01 17:44:19'),(64,'sasha','Asdfg123','cd','2024-12-01 17:44:20'),(65,'sasha','Asdfg123','get','2024-12-01 17:44:29'),(66,'sasha','Asdfg123','connect','2024-12-01 17:47:23'),(67,'sasha','Asdfg123','cd','2024-12-01 17:47:24'),(68,'sasha','Asdfg123','get','2024-12-01 17:47:32'),(69,'sasha','Asdfg123','connect','2024-12-01 17:54:54'),(70,'sasha','Asdfg123','cd','2024-12-01 17:54:55'),(71,'sasha','Asdfg123','get','2024-12-01 17:55:03'),(72,'sasha','Asdfg123','get','2024-12-01 17:57:51'),(73,'sasha','Asdfg123','get','2024-12-01 17:58:31'),(74,'sasha','Asdfg123','connect','2024-12-01 18:00:16'),(75,'sasha','Asdfg123','cd','2024-12-01 18:00:18'),(76,'sasha','Asdfg123','get','2024-12-01 18:00:27'),(77,'sasha','Asdfg123','connect','2024-12-01 18:05:06'),(78,'sasha','Asdfg123','cd','2024-12-01 18:05:07'),(79,'sasha','Asdfg123','cd','2024-12-01 18:05:20'),(80,'sasha','Asdfg123','get','2024-12-01 18:05:29'),(81,'sasha','Asdfg123','get','2024-12-01 18:05:44'),(82,'sasha','Asdfg123','get','2024-12-01 18:05:52'),(83,'sasha','Asdfg123','get','2024-12-01 18:06:24'),(84,'sasha','Asdfg123','connect','2024-12-01 18:07:15'),(85,'sasha','Asdfg123','cd','2024-12-01 18:07:16'),(86,'sasha','Asdfg123','cd','2024-12-01 18:07:20'),(87,'sasha','Asdfg123','get','2024-12-01 18:07:28'),(88,'sasha','Asdfg123','connect','2024-12-01 18:08:40'),(89,'sasha','Asdfg123','cd','2024-12-01 18:08:41'),(90,'sasha','Asdfg123','cd','2024-12-01 18:08:44'),(91,'sasha','Asdfg123','get','2024-12-01 18:08:52'),(92,'sasha','Asdfg123','get','2024-12-01 18:09:27');
/*!40000 ALTER TABLE `history` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-12-01 18:16:55
