namespace __PROJECT__.Domain.Enums;

public static class Const
{
    public const string SUCCESS_CODE = "00";
    public const string DATA_NOT_FOUND_CODE = "98";
    public const string ERROR_CODE = "99";
    public const string SESSION_EXPIRED_CODE = "95";

    public const string DEFAULT_GROUP_ID_ADMINISTRATOR_CODE = "00";

    public static class StatusApproval
    {
        public const int DRAFT = 0;
        public const int OPEN = 1;
        public const int WAITING_APPROVAL = 2;
        public const int NEED_CONFIRM = 3;
        public const int APPROVED = 4;
        public const int REJECTED = 5;
        public const int HOLD = 6;
        public const int DROP = 7;
        public const int DELETED = 8;
    }

    public static class ApprovalType
    {
        public const string ADD = "A";
        public const string MODIFIED = "M";
        public const string DELETE = "D";
    }

    public static class ParentLevel
    {
        public const string FIRST_PARENT = "A";
        public const string SECOND_PARENT = "B";
        public const string CHILD = "D";
    }
}
