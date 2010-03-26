create table Users
(
	UserId   int          not null identity (1, 1) primary key clustered,
	Username nvarchar(50) not null,
	Password varchar(100) not null
)
go

create index ix_users_username_password on Users ( Username, Password )
go