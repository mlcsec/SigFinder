using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SigFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: SigFinder.exe <directoryPath> [-ignore <stringToIgnore1>,<stringToIgnore2>,...] [-recursive] [-domain <domain>]");
                return;
            }

            string directoryPath = args[0];
            bool ignore = args.Contains("-ignore", StringComparer.OrdinalIgnoreCase);
            string[] ignoreStrings = args.SkipWhile(arg => !arg.StartsWith("-ignore")).Skip(1).FirstOrDefault()?.Split(',');

            bool recursive = args.Contains("-recursive", StringComparer.OrdinalIgnoreCase);
            string domainFilter = args.SkipWhile(arg => !arg.StartsWith("-domain")).Skip(1).FirstOrDefault();

            try
            {
                CheckCertificates(directoryPath.Trim('"'), ignore, recursive, domainFilter?.Trim('"'), ignoreStrings?.Select(s => s.Trim('"')).ToArray());
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void CheckCertificates(string directoryPath, bool ignore, bool recursive, string domainFilter, string[] ignoreStrings)
        {
            try
            {
                string[] excludedDirectories = { "Application Data" }; // update/remove

                ProcessDirectory(directoryPath, ignore, domainFilter, ignoreStrings, excludedDirectories, recursive);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ProcessDirectory(string rootDirectory, bool ignore, string domainFilter, string[] ignoreStrings, string[] excludedDirectories, bool recursive)
        {
            try
            {
                if (recursive)
                {
                    ProcessDirectoryRecursive(rootDirectory, ignore, domainFilter, ignoreStrings, excludedDirectories);
                }
                else
                {
                    ProcessDirectoryNonRecursive(rootDirectory, ignore, domainFilter, ignoreStrings, excludedDirectories, recursive);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                //Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ProcessDirectoryNonRecursive(string currentDirectory, bool ignore, string domainFilter, string[] ignoreStrings, string[] excludedDirectories, bool recursive)
        {
            try
            {
                string directoryName = new DirectoryInfo(currentDirectory).Name;
                if (excludedDirectories.Contains(directoryName, StringComparer.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Skipped directory '{currentDirectory}' due to exclusion");
                    return;
                }

                // add/remove
                string[] executableFiles = Directory.GetFiles(currentDirectory, "*.exe", SearchOption.TopDirectoryOnly);
                string[] dllFiles = Directory.GetFiles(currentDirectory, "*.dll", SearchOption.TopDirectoryOnly);
                string[] allFiles = executableFiles.Concat(dllFiles).ToArray();

                if (allFiles.Length == 0)
                {
                    if (!recursive)
                    {
                        Console.WriteLine($"[!] No EXEs or DLLs in directory: {currentDirectory}");
                    }
                    return;
                }

                ProcessFiles(allFiles, ignore, domainFilter, ignoreStrings);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ProcessDirectoryRecursive(string currentDirectory, bool ignore, string domainFilter, string[] ignoreStrings, string[] excludedDirectories)
        {
            ProcessDirectoryNonRecursive(currentDirectory, ignore, domainFilter, ignoreStrings, excludedDirectories, true);

            try
            {
                string[] subDirectories = Directory.GetDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly);
                foreach (string subDirectory in subDirectories)
                {
                    ProcessDirectoryRecursive(subDirectory, ignore, domainFilter, ignoreStrings, excludedDirectories);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                //Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ProcessFiles(string[] files, bool ignore, string domainFilter, string[] ignoreStrings)
        {
            foreach (string filePath in files)
            {
                try
                {
                    Console.WriteLine($"\nChecking certificate for: {filePath}");

                    if (TryGetCertificate(filePath, out X509Certificate2 certificate))
                    {
                        if (ignore && ContainsIgnoreString(certificate.Subject, certificate.Issuer, ignoreStrings))
                        {
                            Console.WriteLine("[-] Certificate ignored. Contains ignore string");
                            continue;
                        }

                        if (!string.IsNullOrEmpty(domainFilter) && !ContainsDomain(certificate.Subject, domainFilter) && !ContainsDomain(certificate.Issuer, domainFilter))
                        {
                            Console.WriteLine($"[-] Certificate ignored. Does not contain domain: {domainFilter}");
                            continue;
                        }

                        Console.WriteLine("[+] Certificate found!");
                        Console.WriteLine($"[*] Subject: {certificate.Subject}");
                        Console.WriteLine($"[*] Issuer: {certificate.Issuer}");
                    }
                    else
                    {
                        Console.WriteLine("[-] No certificate found");
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    //Console.WriteLine($"Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static bool TryGetCertificate(string filePath, out X509Certificate2 certificate)
        {
            certificate = null;

            try
            {
                certificate = new X509Certificate2(filePath);
                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[!] Error checking certificate for {filePath}: {ex.Message}");
                return false;
            }
        }

        static bool ContainsDomain(string text, string domain)
        {
            return text.IndexOf(domain, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool ContainsIgnoreString(string subject, string issuer, string[] ignoreStrings)
        {
            if (ignoreStrings == null || ignoreStrings.Length == 0)
                return false;

            return (subject != null && ignoreStrings.Any(ignore => subject.IndexOf(ignore, StringComparison.OrdinalIgnoreCase) >= 0))
                || (issuer != null && ignoreStrings.Any(ignore => issuer.IndexOf(ignore, StringComparison.OrdinalIgnoreCase) >= 0));
        }
    }
}
