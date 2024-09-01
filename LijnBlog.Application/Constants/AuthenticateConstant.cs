namespace LijnBlog.Application.Constants;

public static class AuthenticateConstant
{
    public const string Base = "auth:";

    public static class Cache
    {
        public const string Base = AuthenticateConstant.Base + "cache:";

        public const string RefreshTokenTag = Base + "refresh_token:";

        public const string AccessTokenTag = Base + "access_token:";
    }
    
    public static class JwtClaimTypes
    {
        public const string RefreshTokenIdClaimType = "refresh_token_id";

        public const string BannedClaimType = "banned";
    }

}
