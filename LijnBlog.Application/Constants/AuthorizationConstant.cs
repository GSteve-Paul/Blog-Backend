namespace LijnBlog.Application.Constants;

public static class AuthorizationConstant
{
    public const string Base = "auth:";

    public static class Policy
    {
        public const string Base = AuthorizationConstant.Base + "policy:";

        public const string AdminPolicy = Base + "admin:";

        public const string UserPolicy = Base + "user:";

        public const string RefreshPolicy = Base + "refresh:";
    }

}
