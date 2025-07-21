public class StripeSettings
{
    public bool DemoMode { get; set; }  // Toggles between test/live mode
    public string LivePublicKey { get; set; }  // pk_live_... (for real payments)
    public string LiveSecretKey { get; set; }  // sk_live_... (for real payments)
    public string TestPublicKey { get; set; }  // pk_test_... (free testing)
    public string TestSecretKey { get; set; }  // sk_test_... (free testing)
    
    // These properties auto-switch between test/live keys
    public string PublicKey => DemoMode ? TestPublicKey : LivePublicKey;
    public string SecretKey => DemoMode ? TestSecretKey : LiveSecretKey;
}