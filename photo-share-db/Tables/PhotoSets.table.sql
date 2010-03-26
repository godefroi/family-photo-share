create table PhotoSets
(
	PhotoSetId int not null identity(1, 1) primary key clustered, 
	PhotoId    int not null foreign key references Photos(PhotoId),
	SetId      int not null foreign key references Sets(SetId)
)
