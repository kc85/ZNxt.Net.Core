namespace ZNxt.Net.Core.Interfaces
{
    public interface IEncryption
    {
        string GetHash(string inputString);

        string GetHash(string inputString, string salt);

        string Encrypt(string inputString);

        string Encrypt(string inputString, string encryptionKey);

        byte[] Encrypt(byte[] data, string encryptionKey);

        string Decrypt(string inputString);

        string Decrypt(string inputString, string encryptionKey);

        byte[] Decrypt(byte[] data, string encryptionKey);
    }
}