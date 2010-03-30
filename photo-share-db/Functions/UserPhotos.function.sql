create function UserPhotos(@uid int) returns table as return
(
	select
		iq.photoid,
		iq.title,
		iq.filename,
		iq.description,
		iq.uploadedby,
		iq.contenttype,
		iq.filedate,
		iq.uploaddate
	from
		(
			-- get photos directly owned by the user
			select
				p.photoid,
				p.title,
				p.filename,
				p.description,
				p.uploadedby,
				p.contenttype,
				p.filedate,
				p.uploaddate
			from
				photos p
			where
				p.uploadedby = @uid

			union all

			-- plus any photos in sets the user has access to
			select
				p.photoid,
				p.title,
				p.filename,
				p.description,
				p.uploadedby,
				p.contenttype,
				p.filedate,
				p.uploaddate
			from
				photos p
					inner join photosets ps on ps.photoid = p.photoid
					inner join usersets  us on us.setid   = ps.setid
			where
				us.userid = @uid
		) iq
)
