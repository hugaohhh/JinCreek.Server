namespace Admin
{
    public class AppSettings
    {
        public string Secret { get; set; }

        public int AccessTokenExpiration { get; set; }

        public int RefreshTokenExpiration { get; set; }
    }
}
