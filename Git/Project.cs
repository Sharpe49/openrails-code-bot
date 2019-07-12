using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Open_Rails_Code_Bot.Git
{
    public class Project
    {
        string GitPath;
        bool Verbose;

        public Project(string gitPath, bool verbose)
        {
            GitPath = gitPath;
            Verbose = verbose;
        }

        public void Init(string repository)
        {
            if (!Directory.Exists(GitPath))
            {
                Directory.CreateDirectory(GitPath);
                RunCommand($"clone --mirror {repository} .");
            }
        }

        public void Fetch()
        {
            RunCommand("fetch --update-head-ok");
        }

        public string ParseRef(string reference)
        {
            foreach (var line in GetCommandOutput($"rev-parse {reference}"))
            {
                return line;
            }
            throw new ApplicationException("Unable to find ref");
        }

        public string Describe(string options)
        {
            foreach (var line in GetCommandOutput($"describe {options}"))
            {
                return line;
            }
            throw new ApplicationException("Unable to describe commit");
        }

        public string CommitTree(string treeRef, IEnumerable<string> parentRefs, string message)
        {
            var parents = String.Join(" ", parentRefs.Select(parentRef => $"-p {parentRef}"));
            foreach (var line in GetCommandOutput($"commit-tree {treeRef} {parents} -m {message}"))
            {
                return line;
            }
            throw new ApplicationException("Unable to describe commit");
        }

        public void CheckoutDetached(string reference)
        {
            RunCommand($"checkout --quiet --detach {reference}");
        }

        public void ResetHard()
        {
            RunCommand("reset --hard");
        }

        public void Merge(string reference)
        {
            RunCommand($"merge --no-edit --no-ff {reference}");
        }

        public void SetBranchRef(string branch, string reference)
        {
            RunCommand($"branch -f {branch} {reference}");
        }

        void RunCommand(string command)
        {
            foreach (var line in GetCommandOutput(command))
            {
            }
        }

        IEnumerable<string> GetCommandOutput(string command)
        {
            var args = $"--no-pager {command}";
            if (Verbose)
            {
                Console.WriteLine("```shell");
                Console.WriteLine($"{GitPath}> git {args}");
            }
            var git = Process.Start(new ProcessStartInfo()
            {
                WorkingDirectory = GitPath,
                FileName = "git",
                Arguments = args,
                StandardOutputEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
            });
            while (!git.StandardOutput.EndOfStream)
            {
                yield return git.StandardOutput.ReadLine();
            }
            git.WaitForExit();
            if (git.ExitCode != 0)
            {
                throw new ApplicationException($"git {command} failed: {git.ExitCode}");
            }
            if (Verbose)
            {
                Console.WriteLine("```");
            }
        }
    }
}
