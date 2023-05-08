namespace Project1.Email
{
    public class Passworder
    {
        public static string GeneratePass(int length)
        {
            var result = new char[length];
            var r = new Random();
            for (int i = 0; i < result.Length; i++)
            {
                do
                    result[i] = (char)r.Next(127);
                while (result[i] < '!');
            }
            return new string(result);
        }
    }
}
