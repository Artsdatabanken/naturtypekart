CREATE TABLE DBO.TaskQueue ( 
  TaskQueueId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
  Action		varchar(50) NOT NULL,
  Payload		nvarchar(max) NOT NULL,
  Created		datetime NOT NULL
)
GO 

CREATE TABLE DBO.TaskQueueError ( 
  TaskQueueId    INT NOT NULL PRIMARY KEY, 
  Action		 varchar(50) NOT NULL,
  Payload		 nvarchar(max) NOT NULL,
  Exception      nvarchar(max) NOT NULL,
  Created		 datetime NOT NULL,
  Failed		datetime NOT NULL 
)
GO 

CREATE TABLE DBO.TaskLog ( 
  TaskLogId		INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
  Action		varchar(50) NOT NULL,
  Payload		nvarchar(max) NOT NULL,
  ElapsedMilliseconds FLOAT,
  Created		datetime NOT NULL,
  Completed		datetime NOT NULL 
)
GO 

CREATE TABLE DBO.Config ( 
  Name			varchar(50) NOT NULL PRIMARY KEY, 
  Data			nvarchar(max) NOT NULL,
  LastModified	datetime NOT NULL 
)
GO 

ALTER TABLE DBO.Config ADD CONSTRAINT DF_Config DEFAULT GETDATE() FOR LastModified
GO
ALTER TABLE DBO.TaskQueue ADD CONSTRAINT DF_TaskQueue_Created DEFAULT GETDATE() FOR Created
GO
ALTER TABLE DBO.TaskQueueError ADD CONSTRAINT DF_TaskQueueError_Created DEFAULT GETDATE() FOR Created
GO
ALTER TABLE DBO.TaskQueueError ADD CONSTRAINT DF_TaskQueueError_Failed DEFAULT GETDATE() FOR Failed
GO
ALTER TABLE DBO.TaskLog ADD CONSTRAINT DF_TaskLog DEFAULT GETDATE() FOR Completed
GO

INSERT INTO Config (Name, Data) VALUES ('Settings', '{}')
GO

CREATE TABLE DBO.TileQueue ( 
  TileQueueId    INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
  Zoom	INT,
  X	INT,
  Y INT
)

GO 
