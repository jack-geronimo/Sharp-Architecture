USE master
GO

IF DB_ID('TardisBank') IS NULL BEGIN
	CREATE DATABASE [TardisBank]
		CONTAINMENT = NONE
	ON  PRIMARY 
		( NAME = N'TardisBank', FILENAME = N'/var/opt/mssql/data/TardisBank.mdf' , SIZE = 4096KB , MAXSIZE = 65536KB , FILEGROWTH = 10%)
	LOG ON 
		( NAME = N'TardisBank_log', FILENAME = N'/var/opt/mssql/data/TardisBank_log.ldf' , SIZE = 2048KB , MAXSIZE = 131072KB , FILEGROWTH = 10%)
END

GO

USE [TardisBank]
IF NOT EXISTS (SELECT name FROM sys.filegroups WHERE is_default=1 AND name = N'PRIMARY') 
	ALTER DATABASE [TardisBank] MODIFY FILEGROUP [PRIMARY] DEFAULT;

