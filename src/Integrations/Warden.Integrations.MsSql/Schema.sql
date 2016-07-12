CREATE TABLE WardenIterations
(
	Id bigint primary key identity not null,
	WardenName nvarchar(MAX) not null,
	Ordinal bigint not null,
	StartedAt datetime not null,
	CompletedAt datetime not null,
	ExecutionTime time not null,
	IsValid bit not null
)

CREATE TABLE WardenCheckResults
(
	Id bigint primary key identity not null,
	WardenIteration_Id bigint not null,
	IsValid bit not null,
	StartedAt datetime not null,
	CompletedAt datetime not null,
	ExecutionTime time not null,
	foreign key (WardenIteration_Id) references WardenIterations(Id)
)

CREATE TABLE WatcherCheckResults
(
	Id bigint primary key identity not null,
	WardenCheckResult_Id bigint not null,
	WatcherName nvarchar(MAX) not null,
	WatcherType nvarchar(MAX) not null,
	Description nvarchar(MAX) not null,
	IsValid bit not null,
	foreign key (WardenCheckResult_Id) references WardenCheckResults(Id)
)

CREATE TABLE Exceptions
(
	Id bigint primary key identity not null,
	WardenCheckResult_Id bigint not null,
	ParentException_Id bigint null,
	Message nvarchar(MAX) null,
	Source nvarchar(MAX) null,
	StackTrace nvarchar(MAX) null,
	foreign key (WardenCheckResult_Id) references WardenCheckResults(Id),
	foreign key (ParentException_Id) references Exceptions(Id)
)