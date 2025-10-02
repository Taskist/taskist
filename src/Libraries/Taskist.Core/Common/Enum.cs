namespace Taskist.Core.Common;

public enum GenderEnum
{
    Male = 1,
    Female,
    Others
}

public enum MaritalStatusEnum
{
    Single = 1,
    Married
}

public enum UserStatusEnum
{
    Active = 1,
    Resigned,
    Terminated,
    Deceased
}

public enum ValidationResultEnum
{
    Success,
    UserNotExist,
    InValidPassword,
    NotActive,
    Locked,
    Deleted,
    Failed
}

public enum ChangePasswordResultEnum
{
    Success,
    CurrentPasswordNotMatch
}

public enum LoginResultEnum
{
    Successful = 1,
    NotExist,
    WrongPassword,
    NotActive,
    NotRegistered,
    LockedOut,
    Deleted
}

public enum RegistrationResultEnum
{
    Successful = 1,
    InvalidRequest,
    Failed,
    InvalidEmpCode
}

public enum DataTypeEnum
{
    INT = 1,
    DECIMAL = 2,
    STRING = 3,
    BOOLEAN = 4
}

public enum OperatorEnum
{
    EQUAL = 1,
    NOT_EQUAL = 2,
    LESSTHAN = 3,
    LESSTHAN_EQUALTO = 4,
    GREATERTHAN = 5,
    GREATERTHAN_EQUALTO = 6,
    EMPTY = 7
}

public enum FormModeEnum
{
    CREATE = 1,
    UPDATE,
    DELETE,
    VIEW
}

public enum NavigationTypeEnum
{
    STANDARD = 1,
    MODAL,
    OFFCANVAS
}

public enum HyperLinkTypeEnum
{
    BUTTON_TEXT = 1,
    BUTTON_ICON = 2,
    BUTTON_TEXT_ICON = 3,
    HYPERLINK_TEXT = 4,
    HYPERLINK_ICON = 5,
    HYPERLINK_TEXT_ICON = 6
}

public enum ButtonColorEnum
{
    PRIMARY,
    SECONDARY,
    SUCCESS,
    DANGER,
    INFO,
    WARNING,
    LIGHT,
    DARK,
    PRIMARY_OUTLINE,
    SECONDARY_OUTLINE,
    SUCCESS_OUTLINE,
    DANGER_OUTLINE,
    INFO_OUTLINE,
    WARNING_OUTLINE,
    LIGHT_OUTLINE,
    DARK_OUTLINE,
}

public enum EmailTemplateTypeEnum
{
    WelcomeKit,
    RegistrationKit,
    Activation,
    ResetPassword,
    Notification,
    Invite,
    NewTaskNotification,
    TaskNotification
}

public enum FileUploadLocationEnum
{
    General,
    UserProfile,
    EmployeeProfile,
    EmployeeDocs
}

public enum SettingsEnum
{
    EncryptionKey,
    SaltLength
}

public enum NotifyTypeEnum
{
    Success,
    Info,
    Error,
    Warning
}

public enum HttpStatusCodeEnum
{
    Success = 2000,
    ValidationError = 2001,
    InternalServerError = 2002,
    NoData = 2003
}

public enum SeverityGroupEnum
{
    Minor = 1,
    Major,
    Critical,
    ShowStopper
}

public enum StatusGroupEnum
{
    New = 1,
    InProgress,
    Archived,
    ReOpened,
    Resolved
}

public enum TaskTypeGroupEnum
{
    ToDo = 1,
    CR,
    Bug
}

public enum CustomFieldPlacementEnum
{
    MainSection = 1,
    StatusSection
}

public enum FieldTypeEnum
{
    TextBox = 1,
    Dropdown,
    Checkbox,
    Radio
}