CREATE TABLE [dbo].[Servers]
(
	[Id] INT NOT NULL IDENTITY(1, 1) PRIMARY KEY , 
   	[Name] NVARCHAR(100) NULL, 
    	[Distance] INT NOT NULL
)
