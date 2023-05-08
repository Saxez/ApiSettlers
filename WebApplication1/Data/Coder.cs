using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using System.Text;

namespace Project1.Data
{
    public class Coder
    {
        public static string Encrypt(string InputString)
        {
            MD5 mD5= MD5.Create();

            byte[] b = Encoding.ASCII.GetBytes(InputString);
            byte[] hash = mD5.ComputeHash(b);

            StringBuilder sb = new StringBuilder();
            foreach(var a in hash )
            {
                sb.Append(a.ToString("X2"));
            }
            return Convert.ToString(sb);
        }

    }
}
