CREATE TABLE WardenIterations
(
	Id int primary key identity not null,
	WardenName nvarchar(MAX) not null,
	Ordinal bigint not null,
	StartedAt datetime not null,
	CompletedAt datetime not null,
	ExecutionTime time not null,
	IsValid bit not null
)

CREATE TABLE WardenCheckResults
(
	Id int primary key identity not null,
	Iteration_Id int not null,
	IsValid bit not null,
	StartedAt datetime not null,
	CompletedAt datetime not null,
	ExecutionTime time not null,
	foreign key (Iteration_Id) references WardenIterations(Id)
)

CREATE TABLE WatcherCheckResults
(
	Id int primary key identity not null,
	Result_Id int not null,
	WatcherName nvarchar(MAX) not null,
	WatcherType nvarchar(MAX) not null,
	Description nvarchar(MAX) not null,
	IsValid bit not null,
	foreign key (Result_Id) references WardenCheckResults(Id)
)