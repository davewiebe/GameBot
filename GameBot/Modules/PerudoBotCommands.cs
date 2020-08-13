using ArtisanCode.SimpleAesEncryption;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameBot.Data;
using GameBot.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameBot.Modules
{

    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private static string _privateKey;
        private static string _publicKey;
        private static UnicodeEncoding _encoder = new UnicodeEncoding();

        [Command("encrypt")]
        public async Task AesEncrypt(params string[] text)
        {
            var textToEncrypt = text[0];
            var key = text[1];
            var monkey = SimpleAES.AES256.Encrypt(textToEncrypt, key);
            await ReplyAsync($"||{monkey}||");
        }
        [Command("decrypt")]
        public async Task AesDecrypt(params string[] text)
        {
            var ciphertext = text[0];
            var key = text[1];
            var monkey = SimpleAES.AES256.Decrypt(ciphertext, key);
            await ReplyAsync($"{monkey}");
        }


        [Command("sendbotdice")]
        public async Task SendBotDice()
        {
            if (_botType != "perudo") return;

            var rsa = new RSACryptoServiceProvider();
            _privateKey = rsa.ToXmlString(true);
            _publicKey = rsa.ToXmlString(false);

            var text = "Test1";
            await ReplyAsync("RSA // Text to encrypt: " + text);
            var enc = Encrypt(text);
            await ReplyAsync($"RSA // Encrypted Text: ||{enc}||");
            var dec = Decrypt(enc);
            await ReplyAsync("RSA // Decrypted Text: " + dec);
        }

        public static string Decrypt(string data)
        {
            var rsa = new RSACryptoServiceProvider();
           
            var dataByte = Encoding.ASCII.GetBytes(data);

            rsa.FromXmlString(_privateKey);
            var decryptedByte = rsa.Decrypt(dataByte, false);

            return _encoder.GetString(decryptedByte);
        }

        public static string Encrypt(string data)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(_publicKey);
            var dataToEncrypt = _encoder.GetBytes(data);
            var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false).ToArray();

            return Encoding.ASCII.GetString(encryptedByteArray);
        }
    }
}
