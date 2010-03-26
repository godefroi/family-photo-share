create table PhotoTags
(
	PhotoTagId int not null identity(1, 1) primary key clustered, 
	PhotoId    int not null foreign key references Photos(PhotoId),
	TagId      int not null foreign key references Tags(TagId)
)
