CREATE TABLE SysLog (
	id int identity(1,1)  NOT NULL,
	opprettet datetime NOT NULL DEFAULT(GETDATE()),
	priority int NOT NULL,
	tag varchar(256) NOT NULL,
	msg nvarchar(max) NOT NULL,
)