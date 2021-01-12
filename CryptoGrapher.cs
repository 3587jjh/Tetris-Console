using System;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class CryptoGrapher {
  const string KEY = "9876"; // 암복호화를 위한 키
  static string EncryptString(string InputText, string Password) {
    RijndaelManaged RijndaelCipher = new RijndaelManaged();
    byte[] PlainText = Encoding.Unicode.GetBytes(InputText);
    byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());
    PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);
    ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32),
        SecretKey.GetBytes(16));
    MemoryStream memoryStream = new MemoryStream();
    CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);
    cryptoStream.Write(PlainText, 0, PlainText.Length);
    cryptoStream.FlushFinalBlock();
    byte[] Cipherbytes = memoryStream.ToArray();
    memoryStream.Close();
    cryptoStream.Close();
    string ret = Convert.ToBase64String(Cipherbytes);
    return ret;
  }
  static string DecryptString(string InputText, string Password) {
    RijndaelManaged RijndaelCipher = new RijndaelManaged();
    byte[] EncryptedData = Convert.FromBase64String(InputText);
    byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());
    PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);
    ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32),
        SecretKey.GetBytes(16));
    MemoryStream memoryStream = new MemoryStream(EncryptedData);
    CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);
    byte[] PlainText = new byte[EncryptedData.Length];
    int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);
    memoryStream.Close();
    cryptoStream.Close();
    string ret = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);
    return ret;
  }
  public static void ReadFile(ref string[] s, string fileName) {
    string txtFile = fileName+".txt";
    string dllFile = fileName+".dll";
    File.Move(dllFile, txtFile);
    s=File.ReadAllLines(txtFile);
    for (int i = 0; i<s.Length; ++i)
      s[i]=DecryptString(s[i], KEY+i.ToString());
    File.Move(txtFile, dllFile);
  }
  public static void WriteFile(string[] s, string fileName) {
    string txtFile = fileName+".txt";
    string dllFile = fileName+".dll";
    File.Move(dllFile, txtFile);
    for (int i = 0; i<s.Length; ++i)
      s[i]=EncryptString(s[i], KEY+i.ToString());
    File.WriteAllLines(txtFile, s);
    File.Move(txtFile, dllFile);
  }
}