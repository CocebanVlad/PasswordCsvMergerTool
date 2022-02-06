using PasswordCsvMergerTool;

var chromePasswords = Utils.LoadPasswords(@"C:\Users\vlad\OneDrive\Desktop\Chrome Passwords.csv");
var edgePasswords = Utils.LoadPasswords(@"C:\Users\vlad\OneDrive\Desktop\Microsoft Autofill Passwords.csv");

var passwords = new Dictionary<string, Dictionary<string, PasswordRow>>(StringComparer.InvariantCultureIgnoreCase);

Utils.CollectPasswords(passwords, chromePasswords);
Utils.CollectPasswords(passwords, edgePasswords);

Utils.SavePasswords(
    passwords.Values.SelectMany(v => v.Values).ToList(),
    @"C:\Users\vlad\OneDrive\Desktop\passwords.csv");

Console.WriteLine("Done!");
Console.ReadKey();