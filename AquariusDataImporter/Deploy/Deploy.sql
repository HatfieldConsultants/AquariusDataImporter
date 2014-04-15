CREATE TABLE `importtask` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `DefinitionJsonString` mediumtext,
  `Name` varchar(255) DEFAULT NULL,
  `HandlerName` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`)
);
