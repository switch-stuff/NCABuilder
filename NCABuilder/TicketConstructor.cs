namespace NCABuilder
{
    using static Utils;

    internal class TicketConstructor
    {
        public static byte[] Ticket(string Issuer, byte[] Key, string TitleID, byte Generation)
        {
            return Structs.Ticket("Root-CA00000003-XS00000020", Key, 0, Generation, StringToBytes($"{ TitleID}000000000000000{Generation}"));
        }
    }
}