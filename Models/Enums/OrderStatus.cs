namespace EXE02_Backend_RE_CAFE.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Preparing = 2,
        Shipping = 3,
        Completed = 4,
        Cancelled = 5,
        Returned = 6
    }
}
