using System.Threading;

namespace PDR.PatientBookingApi.FunctionalTests.Bookings
{
    public static class UniqueSequence
    {
        private static int seq = 1;

        public static int Next()
        {
            return Interlocked.Increment(ref seq);
        }
    }
}