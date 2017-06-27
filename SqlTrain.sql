--exec sql dynamic with sql as string
exec sp_executesql  @sql

--proc example
ALTER  PROCEDURE [dbo].[Pro_AddLink]
	@LinkNo	varchar(20),
	@LinkName	nvarchar(200),
	@LinkImageName nvarchar(200),
	@LinkImage 	image,
	@LinkDesc	nvarchar(4000),
	@LinkUrl	nvarchar(4000),
	@LinkParentID	int,
	@ModifyUserID	int,
	@ModifyTime	datetime,
	@PersonalMk	char(1),
	@UsualAppMk	char(1),
	@ForeignerMk 	char(1),
	@ShowMk	char(1),
	@IsNew	char(1),
	@IsAp		char(1),
	@KeyWord 	nvarchar(50),
	@ErrMsg  	nvarchar(100) output
as
begin
SET  NOCOUNT ON
	declare @index int, @ErrId int, @icount int, @CountLinkNo int

	
	begin  transaction 
		
		begin	
			 select @CountLinkNo = count(*) from WebLinks where LinkNo = @LinkNo -- and LinkName = @LinkName
			 select  @icount = count(*) from dbo.WebLinks where  LinkName = @LinkName

			--select  @icount = count (*) from WebLinks where LinkNo = @LinkNo and  LinkName = @LinkName
			if(@CountLinkNo<>0)
				begin
					set   @ErrMsg = ' LinkNo is exit ! '
					set   @ErrId = @@Error
					goto  Error_Handle
				end
			else
				begin
					if(@icount<>0)
						begin
							set   @ErrMsg = ' LinkName is exit ! '
							set   @ErrId = @@Error
							goto  Error_Handle
						end
					else
						begin
	
							set @index= dbo.fn_get_cur_no('WebLinks')		
							insert into   WebLinks(LinkID , LinkNo,  LinkName, LinkImageName, LinkImage,LinkDesc , LinkUrl, LinkParentID, ModifyUserID, 
											ModifyTime, PersonalMk, UsualAppMk, ForeignerMk, ShowMk, IsNew, IsAp, KeyWord  )
							values(@index,  @LinkNo, @LinkName, @LinkImageName, @LinkImage, @LinkDesc , @LinkUrl, @LinkParentID, @ModifyUserID, 
											@ModifyTime, @PersonalMk,@UsualAppMk, @ForeignerMk, @ShowMk, @IsNew, @IsAp, @KeyWord )
							
							if(@@Error<>0)
								begin
									set   @ErrMsg = 'Insert link is unsuccessful !'
									set   @ErrId = @@Error
									goto  Error_Handle
								end
						end
				end
		end
		
		set  @ErrMsg = 'Insert link is successful !'
		SET  NOCOUNT OFF
		commit transaction

		return 0
	Error_Handle:
		SET  NOCOUNT OFF		
		rollback transaction

		return @ErrId
end
--#proc example
