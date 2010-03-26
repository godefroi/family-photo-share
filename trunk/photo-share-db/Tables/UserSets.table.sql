create table UserSets
(
	UserSetId int not null identity(1, 1) primary key clustered, 
	UserId    int not null foreign key references Users(UserId),
	SetId     int not null foreign key references Sets(SetId)
)
