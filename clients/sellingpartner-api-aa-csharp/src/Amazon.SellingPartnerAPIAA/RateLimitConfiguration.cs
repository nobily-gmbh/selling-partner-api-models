namespace Amazon.SellingPartnerAPIAA
{
    public interface RateLimitConfiguration
    {
        int getRateLimitPermit();
        int getTimeOut();
    }
}
