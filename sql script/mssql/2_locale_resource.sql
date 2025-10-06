DECLARE @LanguageId INT
SELECT @LanguageId = Id FROM [Language] WHERE [LanguageCulture] = 'en-IN'
IF @LanguageId > 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'AccessDenied.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'AccessDenied.Description','Access to this page is denied, please contact your system administrator.')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'AccessDenied.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'AccessDenied.Title','Access Denied!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.Assignee')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.Assignee','Developer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.Button.BatchUpdate')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.Button.BatchUpdate','Update')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.Button.Download')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.Button.Download','Download')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.CreatedBy')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.CreatedBy','Created By')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.CreatedOn')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.CreatedOn','Created On')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.DueDate')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.DueDate','Due Date')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.Module')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.Module','Module')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.Project')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.Project','Project')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.Sprint')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.Sprint','Sprint')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.Severity')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.Severity','Severity')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.Status')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.Status','Status')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.SubModule')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.SubModule','Sub-Module')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.TaskId')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.TaskId','#')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.TaskType')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.TaskType','Type')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.Tester')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.Tester','Tester')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogGrid.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogGrid.Title','Title')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.AttachmentsTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.AttachmentsTabTitle','Attachments')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.Button.Comments')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.Button.Comments','Comments')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.Button.History')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.Button.History','History')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.CommentsTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.CommentsTabTitle','Comments')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.DescriptionTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.DescriptionTabTitle','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.DetailsTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.DetailsTabTitle','Details')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.DeveloperNotesTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.DeveloperNotesTabTitle','Developer Notes')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.HistoryTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.HistoryTabTitle','History')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.NoAccessibleProject.Msg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.NoAccessibleProject.Msg','You are not member of any project. Please contact IT support.')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.QaNotesTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.QaNotesTabTitle','QA Notes')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogPage.Title','Backlog')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogSprint.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogSprint.Title','Sprint')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Assignee')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Assignee','Developer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.CreatedBy')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.CreatedBy','Created By')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.DueDate')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.DueDate','Due Date')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.ExternalNotes')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.ExternalNotes','Reported by')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Files')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Files','Attachments')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Module')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Module','Module')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Parent')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Parent','Parent Task')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Project')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Project','Project')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Sprint')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Sprint','Sprint')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'backlogtask.Reporter')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'backlogtask.Reporter','Reporter')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Severity')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Severity','Severity')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Status')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Status','Status')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.StatusGroup')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.StatusGroup','Status Group')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.SubModule')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.SubModule','Sub-Module')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.TaskType')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.TaskType','Type')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Tester')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Tester','Tester')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogTask.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogTask.Title','Title')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogModel.module.requiredmsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogModel.module.requiredmsg','Module is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogModel.Project.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogModel.Project.RequiredMsg','Project is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogModel.severity.requiredmsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogModel.severity.requiredmsg','Severity is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogModel.submodule.requiredmsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogModel.submodule.requiredmsg','Sub Module is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogModel.TaskType.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogModel.TaskType.RequiredMsg','Type is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogModel.Title.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogModel.Title.MaxLengthMsg','The length of title must be 500 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'BacklogModel.Title.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'BacklogModel.Title.RequiredMsg','Title is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Button.AddNew')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Button.AddNew','Add New')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Button.Back')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Button.Back','Back')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Button.Cancel')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Button.Cancel','Cancel')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Button.Delete')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Button.Delete','Delete')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Button.Edit')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Button.Edit','Edit')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Button.Reset')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Button.Reset','Reset')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Button.Save')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Button.Save','Save')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Button.SaveAndContinue')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Button.SaveAndContinue','Save & Add New')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Button.Search')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Button.Search','Search')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Button.View')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Button.View','View')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ButtonUser.ResetPassword')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ButtonUser.ResetPassword','Reset Password')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.ConfirmNewPassword')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.ConfirmNewPassword','Confirm New Password')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.ConfirmNewPassword.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.ConfirmNewPassword.MaxLengthMsg','The length of confirm password must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.ConfirmNewPassword.NotMatch')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.ConfirmNewPassword.NotMatch','New password and confirm password must match')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.ConfirmNewPassword.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.ConfirmNewPassword.RequiredMsg','Confirm password is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.CurrentPassword')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.CurrentPassword','Current Password')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.CurrentPassword.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.CurrentPassword.MaxLengthMsg','The length of current password must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.CurrentPassword.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.CurrentPassword.RequiredMsg','Current password is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.CurrentPassword.WrongPassword')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.CurrentPassword.WrongPassword','Your current password is not valid')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.NewPassword')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.NewPassword','New Password')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.NewPassword.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.NewPassword.MaxLengthMsg','The length of new password must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ChangePasswordModel.NewPassword.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ChangePasswordModel.NewPassword.RequiredMsg','New password is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Client.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Client.Menu','Client')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientGrid.ContactPerson')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientGrid.ContactPerson','Contact Person')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientGrid.Email')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientGrid.Email','Email')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientGrid.PhoneNumber')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientGrid.PhoneNumber','Phone Number')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.ContactPerson')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.ContactPerson','Contact Person')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.ContactPerson.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.ContactPerson.MaxLengthMsg','The length of contact person must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.Email')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.Email','Email')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.Email.InvalidEmailMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.Email.InvalidEmailMsg','Invalid email address')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.Email.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.Email.MaxLengthMsg','The length of email must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.Name.MaxLengthMsg','The length of name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.Name.UniqueMsg','Name already exists!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.PhoneNumber')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.PhoneNumber','Phone Number')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.PhoneNumber.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.PhoneNumber.MaxLengthMsg','The length of phone no must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.WebSite')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.WebSite','Website')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientModel.WebSite.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientModel.WebSite.MaxLengthMsg','The length of website must be 750 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ClientPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ClientPage.Title','Client')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.ContactPerson')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.ContactPerson','Contact Person')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.ContactPerson.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.ContactPerson.MaxLengthMsg','The length of contact person must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.ContactPerson.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.ContactPerson.RequiredMsg','Contact person is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.Currency')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.Currency','Currency')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.Email')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.Email','Email')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.Email.InvalidEmailMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.Email.InvalidEmailMsg','Invalid email address')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.Email.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.Email.MaxLengthMsg','The length of email must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.Language')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.Language','Language')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.LicenseNumber')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.LicenseNumber','License Number')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.PhoneNumber')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.PhoneNumber','Phone Number')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.PhoneNumber.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.PhoneNumber.MaxLengthMsg','The length of phone number must be 20 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.PhoneNumber.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.PhoneNumber.RequiredMsg','Phone number is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.RegisteredName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.RegisteredName','Registered Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.RegisteredName.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.RegisteredName.MaxLengthMsg','The length of registered name must be 750 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.RegisteredName.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.RegisteredName.RequiredMsg','Registered name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.RegisteredOn')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.RegisteredOn','Registered On')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.TaxNumber')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.TaxNumber','Tax Number')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.TradeName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.TradeName','Trade Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.TradeName.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.TradeName.MaxLengthMsg','The length of trade name must be 750 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.TradeName.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.TradeName.RequiredMsg','Trade name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyModel.WebSite')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyModel.WebSite','Website')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CompanyPage.AddressSectionTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CompanyPage.AddressSectionTitle','Address')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Companypage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Companypage.Title','Company')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Configuration.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Configuration.Menu','Configuration')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Confirmation.Delete')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Confirmation.Delete','Ae you sure you want to delete?')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Currency.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Currency.Menu','Currency')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyGrid.CurrencyCode')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyGrid.CurrencyCode','Code')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyGrid.CurrencySymbol')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyGrid.CurrencySymbol','Symbol')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyGrid.DecimalPlace')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyGrid.DecimalPlace','Decimal Places')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyGrid.DisplayOrder')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyGrid.DisplayOrder','Display Order')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.CurrencyCode')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.CurrencyCode','Code')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.CurrencyCode.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.CurrencyCode.MaxLengthMsg','The length of Code must be 10 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.CurrencyCode.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.CurrencyCode.RequiredMsg','Code is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.CurrencyCode.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.CurrencyCode.UniqueMsg','Code already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.CurrencySymbol')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.CurrencySymbol','Symbol')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.DecimalPlace')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.DecimalPlace','Decimal Places')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.DecimalPlace.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.DecimalPlace.RequiredMsg','Decimal place is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.DisplayOrder')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.DisplayOrder','Display Order')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.DisplayOrder.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.DisplayOrder.RequiredMsg','Display order is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.Name.MaxLengthMsg','The length of Name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyModel.Name.UniqueMsg','Currency already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CurrencyPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CurrencyPage.Title','Currency')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomField.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomField.Title','Custom Field')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldGrid.ColumnClass')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldGrid.ColumnClass','Column Width')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldGrid.FieldType')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldGrid.FieldType','Type')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldGrid.Label')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldGrid.Label','Label')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldGrid.Mandatory')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldGrid.Mandatory','Mandatory')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldGrid.Placement')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldGrid.Placement','Placement')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldGrid.ResourceKey')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldGrid.ResourceKey','Resource Key')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.ColumnClass')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.ColumnClass','Column Width')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.ColumnClass.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.ColumnClass.RequiredMsg','Column width is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.FieldType')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.FieldType','Type')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.FieldType.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.FieldType.RequiredMsg','Type is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.Label')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.Label','Label')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.Label.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.Label.MaxLengthMsg','The length of label must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.Label.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.Label.RequiredMsg','Label is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.Label.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.Label.UniqueMsg','Label already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.Mandatory')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.Mandatory','Mandatory')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.Options')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.Options','Options (coma seperated)')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.Placement')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.Placement','Placement')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.Placement.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.Placement.RequiredMsg','Placement is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.ResourceKey')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.ResourceKey','Resource Key')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldModel.ResourceKey.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldModel.ResourceKey.RequiredMsg','Resource key is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'CustomFieldValueModel.Label.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'CustomFieldValueModel.Label.RequiredMsg','Required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccount.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccount.Menu','Email Account')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountGrid.EnableSsl')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountGrid.EnableSsl','Enable Ssl')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountGrid.FromEmail')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountGrid.FromEmail','From Email')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountGrid.FromName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountGrid.FromName','From Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountGrid.Host')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountGrid.Host','Host')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountGrid.UseDefaultCredentials')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountGrid.UseDefaultCredentials','Use Default Credentials')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.EmailAccount')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.EmailAccount','Account')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.EmainBody')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.EmainBody','Body')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.EmailSubject')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.EmailSubject','Subject')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.EnableSsl')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.EnableSsl','Enable SSL')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.FromEmail')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.FromEmail','From Email')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.FromEmail.InvalidEmailMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.FromEmail.InvalidEmailMsg','Invalid email address')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.FromEmail.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.FromEmail.MaxLengthMsg','The length of from email must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.FromEmail.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.FromEmail.RequiredMsg','Frome mail is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.FromName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.FromName','From Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.FromName.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.FromName.MaxLengthMsg','The length of from name must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.FromName.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.FromName.RequiredMsg','From name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Host')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Host','Host')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Host.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Host.RequiredMsg','Host is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Name.MaxLengthMsg','The length of Name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Name.UniqueMsg','Email account already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Password')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Password','Password')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Password.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Password.MaxLengthMsg','The length of password must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Password.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Password.RequiredMsg','Password is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Port')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Port','Port')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.Port.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.Port.RequiredMsg','Port is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.UnableToDeleteMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.UnableToDeleteMsg','This email account cannot be deleted since the following email templates is using this..')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.UseDefaultCredentials')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.UseDefaultCredentials','Use Default Credentials')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.UserName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.UserName','User Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.UserName.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.UserName.MaxLengthMsg','The length of user name must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountModel.UserName.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountModel.UserName.RequiredMsg','User name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailAccountPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailAccountPage.Title','Email Account')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplate.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplate.Menu','Email Template')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateGrid.EmailAccount')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateGrid.EmailAccount','SMTP Account')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateGrid.EmailSubject')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateGrid.EmailSubject','Email Subject')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.DefaultAccount.NotExistMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.DefaultAccount.NotExistMsg','No default email account exists, please add at least one email account!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.EmailAccount')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.EmailAccount','SMTP Account')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.EmailAccount.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.EmailAccount.RequiredMsg','Email account is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.EmailBody')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.EmailBody','Email Body')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.EmailSubject')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.EmailSubject','Email Subject')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.EmailSubject.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.EmailSubject.MaxLengthMsg','The length of email subject must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.EmailSubject.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.EmailSubject.RequiredMsg','Sbject is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.Name.MaxLengthMsg','The length of Name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplateModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplateModel.Name.UniqueMsg','Email template already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'EmailTemplatePage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'EmailTemplatePage.Title','Email Template')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'User.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'User.Menu','User')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserGrid.Country')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserGrid.Country','Country')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserGrid.Email')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserGrid.Email','Email')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserGrid.FirstName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserGrid.FirstName','First Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserGrid.Gender')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserGrid.Gender','Gender')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserGrid.LastName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserGrid.LastName','Last Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Email')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Email','Email')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Email.InvalidMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Email.InvalidMsg','Invalid email address')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Email.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Email.RequiredMsg','Email is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Email.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Email.UniqueMsg','Email already in use')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Gender')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Gender','Gender')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Gender.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Gender.RequiredMsg','Gender is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.EmailWelcomeKit')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.EmailWelcomeKit','Email Welcome Kit')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.FirstName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.FirstName','First name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.FirstName.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.FirstName.MaxLengthMsg','The length of first name must be 50 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.FirstName.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.FirstName.RequiredMsg','First name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Gender')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Gender','Gender')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Language')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Language','Language')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.LastName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.LastName','Last name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.LastName.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.LastName.MaxLengthMsg','The length of last name must be 50 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.LastName.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.LastName.RequiredMsg','Last name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Role')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Role','Role')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Role.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Role.RequiredMsg','Role is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Status')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Status','Status')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserModel.Status.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserModel.Status.RequiredMsg','Status is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserPage.Title','User Registration')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserProjectModel.CanClose')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserProjectModel.CanClose','Can Close')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserProjectModel.CanComment')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserProjectModel.CanComment','Can Comment')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserProjectModel.CanEdit')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserProjectModel.CanEdit','Can Edit')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserProjectModel.CanEditOthersTask')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserProjectModel.CanEditOthersTask','Can Edit Others Task')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserProjectModel.CanReOpen')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserProjectModel.CanReOpen','Can Reopen')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserProjectModel.CanReport')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserProjectModel.CanReport','Can Report')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserProjectModel.CanViewOthersTask')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserProjectModel.CanViewOthersTask','Can View Others Task')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserProjectModel.CanDelete')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserProjectModel.CanDelete','Can Delete')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserProjectModel.User')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserProjectModel.User','Member')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRole.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRole.Menu','User Role')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleGrid.Button.Permission')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleGrid.Button.Permission','Permission')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleGrid.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleGrid.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleGrid.SystemDefined')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleGrid.SystemDefined','System Defined')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleGrid.SystemName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleGrid.SystemName','System Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModal.SystemRoleMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModal.SystemRoleMsg','System role permission cannot be modified')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.Name.MaxLengthMsg','The length of name must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.Name.UniqueMsg','Role already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.SystemName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.SystemName','System Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.SystemName.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.SystemName.MaxLengthMsg','The length of system name must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.SystemName.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.SystemName.RequiredMsg','System name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.SystemName.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.SystemName.UniqueMsg','System name already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRoleModel.SystemRole')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRoleModel.SystemRole','System Role')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRolePage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRolePage.Title','User Role')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'UserRolePermissionModal.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'UserRolePermissionModal.Title','Permission')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Error.Failed')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Error.Failed','Unable to process the request')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Error.Failed.UserRegistration')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Error.Failed.UserRegistration','Unable to register the User')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Error.ProjectAccessDenied')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Error.ProjectAccessDenied','You don’t have access to the project <b>{0}</b>. Please contact Support for assistance.')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ErrorPage.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ErrorPage.Description','The server encountered an internal error or misconfiguration and was unable to complete your request!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ErrorPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ErrorPage.Title','Internal Server Error!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'FormNoData.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'FormNoData.Description','We are not able to find the data matching the criteria.')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'FormNoData.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'FormNoData.Title','No data!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Grid.Actions')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Grid.Actions','Actions')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Grid.FooterInfo')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Grid.FooterInfo','Showing: Page (_PAGE_ of _PAGES_) Total Rows: (_START_ to _END_ of _TOTAL_)')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Grid.FooterInfoWhenEmpty')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Grid.FooterInfoWhenEmpty','No records available')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Grid.LengthChangeLabel')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Grid.LengthChangeLabel','Show')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Grid.SearchLabel')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Grid.SearchLabel','Filter')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Grid.SearchPlaceholder')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Grid.SearchPlaceholder','Type to filter...')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Grid.ZeroRecords')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Grid.ZeroRecords','Nothing found')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'HeaderMenu.ChangePassword')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'HeaderMenu.ChangePassword','Change Password')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'HeaderMenu.Logout')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'HeaderMenu.Logout','Logout')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'HeaderMenu.MyProfile')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'HeaderMenu.MyProfile','My Profile')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'HeaderMenu.ResetCache')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'HeaderMenu.ResetCache','Reset Cache')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'HeaderMenu.Settings')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'HeaderMenu.Settings','Settings')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Home.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Home.Menu','Home')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Language.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Language.Menu','Language')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageGrid.Button.Resource')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageGrid.Button.Resource','Resource')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageGrid.DisplayName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageGrid.DisplayName','Display Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageGrid.DisplayOrder')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageGrid.DisplayOrder','Display Order')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageGrid.LanguageCulture')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageGrid.LanguageCulture','Culture')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageGrid.Rtl')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageGrid.Rtl','RTL')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.DisplayName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.DisplayName','Display Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.DisplayOrder')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.DisplayOrder','Display Order')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.DisplayOrder.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.DisplayOrder.RequiredMsg','Display order is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.LanguageCulture')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.LanguageCulture','Culture')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.LanguageCulture.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.LanguageCulture.MaxLengthMsg','The length of Culture must be 10 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.LanguageCulture.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.LanguageCulture.RequiredMsg','Culture is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.LanguageCulture.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.LanguageCulture.UniqueMsg','Culture already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.Name.MaxLengthMsg','The length of Name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.Name.UniqueMsg','Language already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguageModel.Rtl')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguageModel.Rtl','RTL')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguagePage.LanguageTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguagePage.LanguageTabTitle','Details')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguagePage.ResourceTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguagePage.ResourceTabTitle','Resources')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguagePage.SaveToAddResourceMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguagePage.SaveToAddResourceMsg','You need to save the language before you can add resource for this language page.')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LanguagePage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LanguagePage.Title','Language')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LocaleResourceGrid.ResourceKey')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LocaleResourceGrid.ResourceKey','Resource Key')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LocaleResourceGrid.ResourceValue')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LocaleResourceGrid.ResourceValue','Resource Value')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LocaleResourceModel.ResourceKey')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LocaleResourceModel.ResourceKey','Resource Key')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LocaleResourceModel.ResourceKey.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LocaleResourceModel.ResourceKey.MaxLengthMsg','The length of Resource Key must be 150 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LocaleResourceModel.ResourceKey.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LocaleResourceModel.ResourceKey.RequiredMsg','Resource Key is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LocaleResourceModel.ResourceKey.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LocaleResourceModel.ResourceKey.UniqueMsg','Resource Key already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LocaleResourceModel.ResourceValue')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LocaleResourceModel.ResourceValue','Resource Value')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LocaleResourceModel.ResourceValue.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LocaleResourceModel.ResourceValue.MaxLengthMsg','The length of Resource Value must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LocaleResourceModel.ResourceValue.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LocaleResourceModel.ResourceValue.RequiredMsg','Resource Value is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LocaleResourcePage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LocaleResourcePage.Title','Locale Resource')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Log.RecordCreated')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Log.RecordCreated','Record created -> {0}')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Log.RecordDeleted')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Log.RecordDeleted','Record deleted -> {0}')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Log.RecordError')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Log.RecordError',' Unable to process the request -> {0}')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Log.RecordUpdated')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Log.RecordUpdated','Record updated -> {0}')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LoginModel.Password')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LoginModel.Password','Password')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LoginModel.RememberMe')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LoginModel.RememberMe','Remember Me')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'LoginModel.UserName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'LoginModel.UserName','Username')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Message.DeleteSuccess')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Message.DeleteSuccess','Deleted successfully')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Message.SaveSuccess')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Message.SaveSuccess','Saved successfully')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Message.UpdateSuccess')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Message.UpdateSuccess','Updated successfully')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleGrid.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleGrid.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleGrid.ProjectName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleGrid.ProjectName','Project')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleModel.Name.MaxLengthMsg','The length of name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleModel.Name.UniqueMsg','Name already exists!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModuleModel.Project')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModuleModel.Project','Project')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ModulePage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ModulePage.Title','Module')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'PageNotFound.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'PageNotFound.Description','Oops, we cannot find the page you are looking for!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'PageNotFound.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'PageNotFound.Title','404 Error!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'PowerBIReport.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'PowerBIReport.Title','Power BI')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Project.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Project.Menu','Project')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectAccessDenied.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectAccessDenied.Title','Project Access Denied')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectUserModel.CanClose')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectUserModel.CanClose','Can Close')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectUserModel.CanReport')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectUserModel.CanReport','Can Report')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectUserModel.User')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectUserModel.User','Member')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectGrid.ClientName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectGrid.ClientName','Client')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectGrid.StartDate')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectGrid.StartDate','Start Date')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectGrid.EndDate')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectGrid.EndDate','End Date')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMember.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMember.Title','Members ')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMemberGrid.CanClose')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMemberGrid.CanClose','Can Close')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMemberGrid.CanComment')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMemberGrid.CanComment','Can Comment')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMemberGrid.CanEdit')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMemberGrid.CanEdit','Can Edit')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMemberGrid.CanEditOthersTask')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMemberGrid.CanEditOthersTask','Can Edit Others Task')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMemberGrid.CanReopen')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMemberGrid.CanReopen','Can Reopen')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMemberGrid.CanReport')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMemberGrid.CanReport','Can Report')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMemberGrid.CanViewOthersTask')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMemberGrid.CanViewOthersTask','Can View Others Task')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMemberGrid.UserName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMemberGrid.UserName','Member')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMemberModel.User.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMemberModel.User.RequiredMsg','Member is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectMemberModel.User.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectMemberModel.User.UniqueMsg','Member already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.Client')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.Client','Client')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.Client.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.Client.RequiredMsg','Client is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.EndDate')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.EndDate','End Date')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.GitProjectReference')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.GitProjectReference','Git Project Reference')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.Name.MaxLengthMsg','The length of name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.Name.UniqueMsg','Name already exists!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.StartDate')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.StartDate','Start Date')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectModel.WebhookUrl')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectModel.WebhookUrl','Webhook Url')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectPage.CustomFieldTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectPage.CustomFieldTabTitle','Custom Fields')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectPage.IntegrationTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectPage.IntegrationTabTitle','Integration')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectPage.MemberTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectPage.MemberTabTitle','Members')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectPage.ProjectTabTitle')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectPage.ProjectTabTitle','Project Details')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectPage.SaveToAddMemberMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectPage.SaveToAddMemberMsg','You need to save the project details before you can add members.')
    END
     IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectPage.SaveToAddCustomFieldMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectPage.SaveToAddCustomFieldMsg','You need to save the project details before you can add custom fields.')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ProjectPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ProjectPage.Title','Project')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintGrid.Button.Plan')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintGrid.Button.Plan','Plan')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintGrid.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintGrid.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Sprintgrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Sprintgrid.Name','Version')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintGrid.Progress')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintGrid.Progress','Progress')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintGrid.Project')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintGrid.Project','Project')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintGrid.SprintDate')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintGrid.SprintDate','Sprint Date')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintGrid.SprintType')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintGrid.SprintType','Sprint Type')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintGrid.Status')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintGrid.Status','Status')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintGrid.Version')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintGrid.Version','Version')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Name','Version')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Name.RequiredMsg','Version Required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Name.UniqueMsg','Version already exists')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Project')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Project','Project')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Project.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Project.RequiredMsg','Project is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.StartDate')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.StartDate','Start')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.StartDate.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.StartDate.RequiredMsg','Start date is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.SprintType')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.SprintType','Sprint Type')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.StartDate.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.StartDate.RequiredMsg','Date Required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Status')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Status','Status')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Version')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Version','Version')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Version.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Version.MaxLengthMsg','The length of version must be 50 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Version.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Version.RequiredMsg','Version is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintModel.Version.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintModel.Version.UniqueMsg','Version already exists!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SprintPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SprintPage.Title','Sprint')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterModel.Name.MaxLengthMsg','The length of name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterModel.Name.UniqueMsg','Name already exists!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterModel.Project')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterModel.Project','Project')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ReporterPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ReporterPage.Title','Reporter')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ResetPasswordModel.Password')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ResetPasswordModel.Password','New Password')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ResourceGrid.ResourceKey')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ResourceGrid.ResourceKey','Resource Key')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ResourceGrid.ResourceValue')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ResourceGrid.ResourceValue','Resource Value')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'ResourcePage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'ResourcePage.Title','Locale Resource')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'Setting.Menu')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'Setting.Menu','Settings')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SettingModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SettingModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SettingModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SettingModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SettingModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SettingModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SettingModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SettingModel.Name.MaxLengthMsg','The length of name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SettingModel.Value')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SettingModel.Value','Value')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SettingModel.Value.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SettingModel.Value.MaxLengthMsg','The length of value must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityGrid.BackgroundColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityGrid.BackgroundColor','Background Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityGrid.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityGrid.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityGrid.Group')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityGrid.Group','Group')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityGrid.IconClass')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityGrid.IconClass','Icon Class')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityGrid.TextColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityGrid.TextColor','Text Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.BackgroundColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.BackgroundColor','Background Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.BackgroundColor.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.BackgroundColor.MaxLengthMsg','The length of background color must be 20 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.Group')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.Group','Group')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.Group.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.Group.RequiredMsg','Group is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.IconClass')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.IconClass','Icon Class')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.IconClass.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.IconClass.MaxLengthMsg','The length of icon class must be 50 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.Name.MaxLengthMsg','The length of name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.Name.UniqueMsg','Name already exists!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.TextColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.TextColor','Text Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityModel.TextColor.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityModel.TextColor.MaxLengthMsg','The length of text color must be 20 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SeverityPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SeverityPage.Title','Severity')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusGrid.BackgroundColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusGrid.BackgroundColor','Background Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusGrid.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusGrid.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusGrid.Group')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusGrid.Group','Group')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusGrid.IconClass')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusGrid.IconClass','Icon Class')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusGrid.SystemDefined')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusGrid.SystemDefined','System Defined')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusGrid.TextColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusGrid.TextColor','Text Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.BackgroundColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.BackgroundColor','Background Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.BackgroundColor.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.BackgroundColor.MaxLengthMsg','The length of background color must be 20 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.ExternalUse')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.ExternalUse','External Use')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.Group')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.Group','Group')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.Group.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.Group.RequiredMsg','Group is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.IconClass')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.IconClass','Icon Class')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.IconClass.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.IconClass.MaxLengthMsg','The length of icon class must be 50 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.Name.MaxLengthMsg','The length of name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.Name.UniqueMsg','Name already exists!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.RequiredMrNumberForBug')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.RequiredMrNumberForBug','Required MR # For Bug')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.RequiredMrNumberForCr')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.RequiredMrNumberForCr','Required MR # For CR')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.RequiredResolutionDetails')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.RequiredResolutionDetails','Required Resolution Details')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.RequiredResolutionForBug')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.RequiredResolutionForBug','Required Resolution For Bug')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.RequiredResolutionForCr')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.RequiredResolutionForCr','Required Resolution For CR')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.RequiredRootCauseDetails')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.RequiredRootCauseDetails','Required Root Cause Details')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.RequiredRootCauseForBug')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.RequiredRootCauseForBug','Required Root Cause For Bug')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.RequiredRootCauseForCr')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.RequiredRootCauseForCr','Required Root Cause For CR')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.TextColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.TextColor','Text Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusModel.TextColor.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusModel.TextColor.MaxLengthMsg','The length of text color must be 20 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusPage.SystemDefinedMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusPage.SystemDefinedMsg','System status cannot be modified')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'StatusPage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'StatusPage.Title','Status')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleGrid.ModuleName')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleGrid.ModuleName','Module')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleModel.Module')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleModel.Module','Module')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleModel.Name.MaxLengthMsg','The length of name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleModel.Name.UniqueMsg','Name already exists!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModuleModel.Owner')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModuleModel.Owner','Owner')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'SubModulePage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'SubModulePage.Title','Sub Module')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeGrid.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeGrid.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeGrid.BackgroundColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeGrid.BackgroundColor','Background Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeGrid.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeGrid.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeGrid.Group')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeGrid.Group','Group')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeGrid.IconClass')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeGrid.IconClass','Icon Class')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeGrid.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeGrid.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeGrid.TextColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeGrid.TextColor','Text Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.Active')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.Active','Active')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.BackgroundColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.BackgroundColor','Background Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.BackgroundColor.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.BackgroundColor.MaxLengthMsg','The length of background color must be 20 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.Description')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.Description','Description')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.Description.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.Description.MaxLengthMsg','The length of description must be 250 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.Group')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.Group','Group')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.Group.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.Group.RequiredMsg','Group is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.IconClass')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.IconClass','Icon Class')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.IconClass.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.IconClass.MaxLengthMsg','The length of icon class must be 50 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.Name')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.Name','Name')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.Name.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.Name.MaxLengthMsg','The length of name must be 100 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.Name.RequiredMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.Name.RequiredMsg','Name is required')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.Name.UniqueMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.Name.UniqueMsg','Name already exists!')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.TextColor')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.TextColor','Text Color')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypeModel.TextColor.MaxLengthMsg')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypeModel.TextColor.MaxLengthMsg','The length of text color must be 20 characters or fewer')
    END
    IF NOT EXISTS (SELECT 1 FROM [LocaleResource] WHERE [ResourceKey] = 'TaskTypePage.Title')
    BEGIN
        INSERT INTO [LocaleResource] ([LanguageId],[ResourceKey],[ResourceValue])
        VALUES(@LanguageId,'TaskTypePage.Title','Task Type')
    END
END