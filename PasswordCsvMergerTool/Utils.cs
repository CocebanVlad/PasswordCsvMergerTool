using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.ObjectPool;
using System.Globalization;
using System.Text;

namespace PasswordCsvMergerTool
{
    internal static class Utils
    {
        public static readonly ObjectPool<StringBuilder> _stringBuilderPool;

        static Utils()
        {
            var objectPoolProvider = new DefaultObjectPoolProvider();
            _stringBuilderPool = objectPoolProvider.Create(new StringBuilderPooledObjectPolicy());
        }

        public static List<PasswordRow> LoadPasswords(string filename)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };

            List<PasswordRow> passwords;

            using (var reader = new StreamReader(filename))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                _ = csv.GetRecord<PasswordRow>();
                passwords = csv.GetRecords<PasswordRow>().ToList();
            }

            return passwords;
        }

        public static string ComputeKey(string? url, string? username)
        {
            var strBuilder = _stringBuilderPool.Get();

            if (!string.IsNullOrEmpty(url))
            {
                var uri = new Uri(url);
                strBuilder.Append(uri.Host);
            }

            if (!string.IsNullOrEmpty(username))
            {
                strBuilder.Append(username);
            }

            var key = strBuilder.ToString().ToLower();
            _stringBuilderPool.Return(strBuilder);

            return key;
        }

        public static void CollectPasswords(Dictionary<string, Dictionary<string, PasswordRow>> bag, List<PasswordRow> passwords)
        {
            foreach (var row in passwords)
            {
                var key = ComputeKey(row.Url, row.Username);
                if (!bag.TryGetValue(key, out var cluster) || cluster == null)
                {
                    cluster = new Dictionary<string, PasswordRow>(StringComparer.InvariantCultureIgnoreCase);
                    bag.Add(key, cluster);
                }

                var subkey = row.Username ?? "";
                if (cluster.TryGetValue(subkey, out var existingRow) && existingRow != null)
                {
                    if (existingRow.Password?.Equals(row.Password, StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        Console.WriteLine($"Discard duplicate for: {row.Url}->{row.Username}");
                        continue;
                    }

                    var askToChoose = true;
                    var keepCurrent = false;
                    while (askToChoose)
                    {
                        Console.Write("You must choose which password is the right one for:\n" +
                            $"{row.Url}->{row.Username}\n" +
                            $"[1] password: {row.Password}\n" +
                            $"[2] existing password: {existingRow.Password}\n> ");

                        var option = Console.ReadLine();
                        switch (option)
                        {
                            case "1":
                                askToChoose = false;
                                keepCurrent = false;
                                break;

                            case "2":
                                askToChoose = false;
                                keepCurrent = true;
                                break;
                        }
                    }

                    if (keepCurrent)
                    {
                        continue;
                    }
                }

                cluster[subkey] = row;
            }
        }

        public static void SavePasswords(List<PasswordRow> passwords, string filename)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };

            using (var reader = new StreamWriter(filename))
            using (var csv = new CsvWriter(reader, config))
            {
                csv.WriteRecord(new PasswordRow()
                {
                    Name = "name",
                    Url = "url",
                    Username = "username",
                    Password = "password"
                });
                csv.NextRecord();

                foreach (var password in passwords)
                {
                    csv.WriteRecord(password);
                    csv.NextRecord();
                }
            }
        }
    }
}