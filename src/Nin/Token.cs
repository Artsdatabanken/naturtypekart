using System;
using System.Text.RegularExpressions;

namespace Types
{
    public struct Token
    {
        public string clientIp;
        public string value;
        public DateTime expires;

        public Token(string clientIp, string value, int ttl)
        {
            this.clientIp = clientIp;
            if (Regex.IsMatch(value, @"^[A-Z0-9]+$"))
            {
                this.value = value;
                expires = DateTime.UtcNow.AddMinutes(ttl);
            }
            else
            {
                this.value = "";
                expires = DateTime.UtcNow;
            }
        }

        public Token(string clientIp, string value, DateTime expires)
        {
            this.clientIp = clientIp;
            if (Regex.IsMatch(value, @"^[A-Z0-9]+$"))
            {
                this.value = value;
                this.expires = expires;
            }
            else
            {
                this.value = "";
                this.expires = DateTime.UtcNow;
            }
        }
    }
}
