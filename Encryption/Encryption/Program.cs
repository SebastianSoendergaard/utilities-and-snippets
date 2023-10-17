// See https://aka.ms/new-console-template for more information
using Encryption;
using System.Text.Json;

string key = Guid.NewGuid().ToString();


string text = Guid.NewGuid().ToString();
Console.WriteLine("Encrypting: " + text);
var encryptedText = Encryptor.Encrypt(key, text);
Console.WriteLine("Encrypted text: " + encryptedText);
var decryptedText = Encryptor.Decrypt(key, encryptedText);
Console.WriteLine("Decrypted text: " + decryptedText);


var obj = new Obj
(
    Id: Guid.NewGuid(),
    Text: Guid.NewGuid().ToString(),
    Number: 1,
    Time: DateTimeOffset.Now
);
Console.WriteLine("Encrypting: " + JsonSerializer.Serialize(obj));
var encryptedObj = Encryptor.Encrypt(key, obj);
Console.WriteLine("Encrypted obj: " + encryptedObj);
var decryptedObj = Encryptor.Decrypt<Obj>(key, encryptedObj);
Console.WriteLine("Decrypted obj: " + JsonSerializer.Serialize(decryptedObj));

record Obj(Guid Id, string Text, long Number, DateTimeOffset Time);