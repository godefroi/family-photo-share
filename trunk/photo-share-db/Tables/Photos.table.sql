create table Photos
(
	PhotoId     int            not null identity (1, 1) primary key clustered,
	Title       nvarchar(500)      null,
	Filename    nvarchar(255)  not null,
	UploadedBy  int            not null references Users(UserId),
	Description nvarchar(max)      null,
	Picture     varbinary(max) not null,
	Thumbnail   varbinary(max) not null,
	ContentType nvarchar(100)  not null,
	FileDate    datetime       not null,
	UploadDate  datetime       not null,
	Hash        nvarchar(50)   not null unique
)

/*
create index
	ix_photos_uploadedby_uploaddate
on
	photos ( uploadedby, uploaddate )
*/
