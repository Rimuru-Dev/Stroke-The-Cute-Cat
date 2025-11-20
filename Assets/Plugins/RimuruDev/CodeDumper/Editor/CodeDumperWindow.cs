// Resharper disable all

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace RimuruDev.CodeDumper
{
    public sealed class CodeDumperWindow : EditorWindow
    {
        private const string PrefsPrefix = "AbyssMoth.CodeDumper.";
        public string SourceFolder = "Assets";
        public string OutputDirectory = "CodeDumps";
        public string BaseFileName = "CodeDump";
        public string Extensions = ".cs,.shader,.hlsl,.cginc,.compute,.jslib,.json,.uxml,.uss,.glsl,.py,.js,.ts,.java,.kt,.swift,.cpp,.h,.m,.mm,.rb,.php,.sql,.yml,.yaml,.toml,.ini,.gradle,.xml,.ps1,.sh,.bat,.md";
        public string ExcludeDirs = "Library,Temp,Obj,Logs,.git,.idea,.vs,Build,Builds,Demos,Demo,Samples,Sample,Examples,Example,UserSettings,MemoryCaptures,Records,Recordings";
        public string IncludeNameContains = "";
        public string ExcludeNameContains = "";
        public bool UseUtcTime = false;
        public bool WriteHeader = true;
        public bool OpenAfterSave = false;

        private string status;
        private Vector2 scroll;

        [MenuItem("RimuruDev Tools/CODE/Code Dumper")]
        public static void ShowWindow()
        {
            var w = GetWindow<CodeDumperWindow>();
            w.titleContent = new GUIContent("Code Dumper");
            w.minSize = new Vector2(560, 300);
            w.LoadPrefs();
            w.Show();
        }

        [MenuItem("RimuruDev Tools/CODE/Code Dumper • Quick Dump")]
        public static void QuickDump()
        {
            var cfg = LoadPrefsStatic();
            var now = cfg.UseUtcTime ? DateTime.UtcNow : DateTime.Now;
            var stamp = cfg.UseUtcTime ? now.ToString("yyyy-MM-dd_HH-mm-ss") + "Z" : now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = cfg.BaseFileName + "_" + stamp + ".md";
            var outDir = ResolvePath(cfg.OutputDirectory, true);
            var outPath = Path.Combine(outDir, fileName);
            var result = Dump(new DumpConfig
            {
                SourceFolder = cfg.SourceFolder,
                OutputPath = outPath,
                Extensions = cfg.Extensions,
                ExcludeDirs = cfg.ExcludeDirs,
                IncludeNameContains = cfg.IncludeNameContains,
                ExcludeNameContains = cfg.ExcludeNameContains,
                WriteHeader = cfg.WriteHeader,
                Timestamp = now,
                UseUtcTime = cfg.UseUtcTime
            });
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Code Dumper", "Готово\nФайлов: " + result.FileCount + "\nКлассов: " + result.ClassCount + "\nСтрок: " + result.LineCount + "\n" + result.OutputPath, "OK");
            if (cfg.OpenAfterSave) EditorUtility.RevealInFinder(result.OutputPath);
        }

        private void OnEnable()
        {
            LoadPrefs();
        }

        private void OnDisable()
        {
            SavePrefs();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            SourceFolder = EditorGUILayout.TextField("Source Folder", SourceFolder);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pick Source", GUILayout.Width(120)))
            {
                var p = EditorUtility.OpenFolderPanel("Select Source", Application.dataPath, "");
                if (!string.IsNullOrEmpty(p)) SourceFolder = ToRelativePath(p);
            }
            EditorGUILayout.EndHorizontal();

            OutputDirectory = EditorGUILayout.TextField("Output Directory", OutputDirectory);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pick Output Dir", GUILayout.Width(120)))
            {
                var p = EditorUtility.OpenFolderPanel("Select Output Directory", Application.dataPath, "");
                if (!string.IsNullOrEmpty(p)) OutputDirectory = ToRelativePath(p);
            }
            EditorGUILayout.EndHorizontal();

            BaseFileName = EditorGUILayout.TextField("Base File Name", BaseFileName);
            Extensions = EditorGUILayout.TextField("Extensions (comma)", Extensions);
            ExcludeDirs = EditorGUILayout.TextField("Exclude Dirs (comma)", ExcludeDirs);
            IncludeNameContains = EditorGUILayout.TextField("Include Name Contains", IncludeNameContains);
            ExcludeNameContains = EditorGUILayout.TextField("Exclude Name Contains", ExcludeNameContains);
            UseUtcTime = EditorGUILayout.Toggle("Use UTC Timestamp", UseUtcTime);
            WriteHeader = EditorGUILayout.Toggle("Write Header", WriteHeader);
            OpenAfterSave = EditorGUILayout.Toggle("Reveal In Finder", OpenAfterSave);

            EditorGUILayout.Space();
            var now = UseUtcTime ? DateTime.UtcNow : DateTime.Now;
            var stamp = UseUtcTime ? now.ToString("yyyy-MM-dd_HH-mm-ss") + "Z" : now.ToString("yyyy-MM-dd_HH-mm-ss");
            var preview = BaseFileName + "_" + stamp + ".md";
            EditorGUILayout.LabelField("Preview File Name", preview);

            EditorGUILayout.Space();
            if (GUILayout.Button("Dump"))
            {
                try
                {
                    SavePrefs();
                    var outDir = ResolvePath(OutputDirectory, true);
                    var outPath = Path.Combine(outDir, preview);
                    var result = Dump(new DumpConfig
                    {
                        SourceFolder = SourceFolder,
                        OutputPath = outPath,
                        Extensions = Extensions,
                        ExcludeDirs = ExcludeDirs,
                        IncludeNameContains = IncludeNameContains,
                        ExcludeNameContains = ExcludeNameContains,
                        WriteHeader = WriteHeader,
                        Timestamp = now,
                        UseUtcTime = UseUtcTime
                    });
                    AssetDatabase.Refresh();
                    status = "Готово • Файлов: " + result.FileCount + " • Классов: " + result.ClassCount + " • Строк: " + result.LineCount + " • " + ToRelativePath(result.OutputPath);
                    if (OpenAfterSave) EditorUtility.RevealInFinder(result.OutputPath);
                }
                catch (Exception e)
                {
                    status = "Ошибка: " + e.Message;
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(string.IsNullOrEmpty(status) ? "Готов" : status, MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        private void LoadPrefs()
        {
            SourceFolder = EditorPrefs.GetString(PrefsPrefix + nameof(SourceFolder), SourceFolder);
            OutputDirectory = EditorPrefs.GetString(PrefsPrefix + nameof(OutputDirectory), OutputDirectory);
            BaseFileName = EditorPrefs.GetString(PrefsPrefix + nameof(BaseFileName), BaseFileName);
            Extensions = EditorPrefs.GetString(PrefsPrefix + nameof(Extensions), Extensions);
            ExcludeDirs = EditorPrefs.GetString(PrefsPrefix + nameof(ExcludeDirs), ExcludeDirs);
            IncludeNameContains = EditorPrefs.GetString(PrefsPrefix + nameof(IncludeNameContains), IncludeNameContains);
            ExcludeNameContains = EditorPrefs.GetString(PrefsPrefix + nameof(ExcludeNameContains), ExcludeNameContains);
            UseUtcTime = EditorPrefs.GetBool(PrefsPrefix + nameof(UseUtcTime), UseUtcTime);
            WriteHeader = EditorPrefs.GetBool(PrefsPrefix + nameof(WriteHeader), WriteHeader);
            OpenAfterSave = EditorPrefs.GetBool(PrefsPrefix + nameof(OpenAfterSave), OpenAfterSave);
        }

        private void SavePrefs()
        {
            EditorPrefs.SetString(PrefsPrefix + nameof(SourceFolder), SourceFolder);
            EditorPrefs.SetString(PrefsPrefix + nameof(OutputDirectory), OutputDirectory);
            EditorPrefs.SetString(PrefsPrefix + nameof(BaseFileName), BaseFileName);
            EditorPrefs.SetString(PrefsPrefix + nameof(Extensions), Extensions);
            EditorPrefs.SetString(PrefsPrefix + nameof(ExcludeDirs), ExcludeDirs);
            EditorPrefs.SetString(PrefsPrefix + nameof(IncludeNameContains), IncludeNameContains);
            EditorPrefs.SetString(PrefsPrefix + nameof(ExcludeNameContains), ExcludeNameContains);
            EditorPrefs.SetBool(PrefsPrefix + nameof(UseUtcTime), UseUtcTime);
            EditorPrefs.SetBool(PrefsPrefix + nameof(WriteHeader), WriteHeader);
            EditorPrefs.SetBool(PrefsPrefix + nameof(OpenAfterSave), OpenAfterSave);
        }

        private static DumpConfig LoadPrefsStatic()
        {
            var cfg = new DumpConfig();
            cfg.SourceFolder = EditorPrefs.GetString(PrefsPrefix + nameof(SourceFolder), "Assets");
            cfg.OutputPath = "";
            cfg.Extensions = EditorPrefs.GetString(PrefsPrefix + nameof(Extensions), ".cs");
            cfg.ExcludeDirs = EditorPrefs.GetString(PrefsPrefix + nameof(ExcludeDirs), "");
            cfg.IncludeNameContains = EditorPrefs.GetString(PrefsPrefix + nameof(IncludeNameContains), "");
            cfg.ExcludeNameContains = EditorPrefs.GetString(PrefsPrefix + nameof(ExcludeNameContains), "");
            cfg.UseUtcTime = EditorPrefs.GetBool(PrefsPrefix + nameof(UseUtcTime), false);
            cfg.WriteHeader = EditorPrefs.GetBool(PrefsPrefix + nameof(WriteHeader), true);
            cfg.Timestamp = cfg.UseUtcTime ? DateTime.UtcNow : DateTime.Now;
            cfg.BaseFileName = EditorPrefs.GetString(PrefsPrefix + nameof(BaseFileName), "CodeDump");
            cfg.OutputDirectory = EditorPrefs.GetString(PrefsPrefix + nameof(OutputDirectory), "CodeDumps");
            cfg.OpenAfterSave = EditorPrefs.GetBool(PrefsPrefix + nameof(OpenAfterSave), false);
            return cfg;
        }

        private static DumpResult Dump(DumpConfig cfg)
        {
            var srcPath = ResolvePath(cfg.SourceFolder, false);
            if (!Directory.Exists(srcPath)) throw new DirectoryNotFoundException(srcPath);

            var exts = SplitSet(cfg.Extensions);
            var excludes = SplitSet(cfg.ExcludeDirs);
            var includeSub = (cfg.IncludeNameContains ?? "").Trim();
            var excludeSub = (cfg.ExcludeNameContains ?? "").Trim();

            var files = EnumerateFiles(srcPath, exts, excludes)
                .Where(p =>
                {
                    var name = Path.GetFileName(p);
                    if (!string.IsNullOrEmpty(includeSub) && name.IndexOf(includeSub, StringComparison.OrdinalIgnoreCase) < 0) return false;
                    if (!string.IsNullOrEmpty(excludeSub) && name.IndexOf(excludeSub, StringComparison.OrdinalIgnoreCase) >= 0) return false;
                    return true;
                })
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sb = new StringBuilder(1024 * 1024);
            var lineCount = 0L;
            var classCount = 0L;

            if (cfg.WriteHeader)
            {
                sb.AppendLine("# " + cfg.BaseFileName);
                var ts = cfg.UseUtcTime ? cfg.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ") : cfg.Timestamp.ToString("yyyy-MM-dd HH:mm:ss zzz");
                sb.AppendLine();
                sb.AppendLine("Generated: " + ts);
                sb.AppendLine("Source: " + ToProjectRelative(srcPath));
                sb.AppendLine("Extensions: " + string.Join(", ", exts.OrderBy(x => x)));
                sb.AppendLine("Exclude Dirs: " + string.Join(", ", excludes.OrderBy(x => x)));
                if (!string.IsNullOrEmpty(includeSub)) sb.AppendLine("Include Filter: " + includeSub);
                if (!string.IsNullOrEmpty(excludeSub)) sb.AppendLine("Exclude Filter: " + excludeSub);
                sb.AppendLine();
            }

            foreach (var path in files)
            {
                var rel = ToProjectRelative(path);
                var lang = DetectLang(path);
                sb.AppendLine("## " + rel);
                sb.AppendLine();
                sb.AppendLine("```" + lang);
                var text = File.ReadAllText(path, new UTF8Encoding(false));
                sb.Append(text);
                if (!text.EndsWith("\n")) sb.AppendLine();
                sb.AppendLine("```");
                sb.AppendLine();

                var lines = CountLinesFast(text);
                lineCount += lines;
                classCount += CountTypes(text, path);
            }

            if (cfg.WriteHeader)
            {
                sb.Insert(0, "Files: " + files.Count + "\nLines: " + lineCount + "\nClasses: " + classCount + "\n\n");
            }

            var outPath = cfg.OutputPath;
            var outDir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(false));

            return new DumpResult
            {
                OutputPath = outPath,
                FileCount = files.Count,
                LineCount = lineCount,
                ClassCount = classCount
            };
        }

        private static IEnumerable<string> EnumerateFiles(string root, HashSet<string> exts, HashSet<string> excludes)
        {
            var stack = new Stack<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                var name = new DirectoryInfo(dir).Name;
                if (excludes.Contains(name)) continue;
                foreach (var sub in Directory.GetDirectories(dir)) stack.Push(sub);
                foreach (var file in Directory.GetFiles(dir))
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext == ".meta") continue;
                    if (exts.Count == 0 || exts.Contains(ext)) yield return file;
                }
            }
        }

        private static HashSet<string> SplitSet(string csv)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parts = (csv ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var s = p.Trim();
                if (s.Length == 0) continue;
                if (s.StartsWith(".")) set.Add(s.ToLowerInvariant());
                else set.Add(s);
            }
            return set;
        }

        private static string DetectLang(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".cs") return "csharp";
            if (ext == ".shader") return "shaderlab";
            if (ext == ".hlsl" || ext == ".cginc" || ext == ".compute") return "hlsl";
            if (ext == ".glsl") return "glsl";
            if (ext == ".jslib" || ext == ".js") return "javascript";
            if (ext == ".ts") return "typescript";
            if (ext == ".json") return "json";
            if (ext == ".uxml" || ext == ".xml") return "xml";
            if (ext == ".uss" || ext == ".css") return "css";
            if (ext == ".py") return "python";
            if (ext == ".java") return "java";
            if (ext == ".kt") return "kotlin";
            if (ext == ".swift") return "swift";
            if (ext == ".cpp" || ext == ".cc" || ext == ".cxx" || ext == ".c") return "cpp";
            if (ext == ".h" || ext == ".hpp") return "cpp";
            if (ext == ".m" || ext == ".mm") return "objectivec";
            if (ext == ".rb") return "ruby";
            if (ext == ".php") return "php";
            if (ext == ".sql") return "sql";
            if (ext == ".yml" || ext == ".yaml") return "yaml";
            if (ext == ".toml") return "toml";
            if (ext == ".ini") return "ini";
            if (ext == ".gradle") return "groovy";
            if (ext == ".ps1") return "powershell";
            if (ext == ".sh" || ext == ".bash") return "bash";
            if (ext == ".bat" || ext == ".cmd") return "bat";
            if (ext == ".md") return "markdown";
            return "";
        }

        private static long CountLinesFast(string text)
        {
            var count = 0L;
            for (var i = 0; i < text.Length; i++) if (text[i] == '\n') count++;
            if (text.Length > 0 && text[^1] != '\n') count++;
            return count;
        }

        private static long CountTypes(string text, string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".cs" || ext == ".java" || ext == ".kt" || ext == ".ts" || ext == ".js" || ext == ".swift" || ext == ".cpp" || ext == ".h")
            {
                var rx = new Regex(@"\b(class|struct|interface)\s+[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Multiline);
                return rx.Matches(text).Count;
            }
            return 0;
        }

        private static string ResolvePath(string maybeRelative, bool createDirs)
        {
            if (string.IsNullOrEmpty(maybeRelative)) return Directory.GetCurrentDirectory();
            if (Path.IsPathRooted(maybeRelative)) return maybeRelative;
            var full = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), maybeRelative));
            if (createDirs)
            {
                var dir = full;
                var attr = File.Exists(full) ? File.GetAttributes(full) : FileAttributes.Directory;
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                }
                else
                {
                    var parent = Path.GetDirectoryName(full);
                    if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent)) Directory.CreateDirectory(parent);
                }
            }
            return full;
        }

        private static string ToRelativePath(string absolute)
        {
            try
            {
                var proj = Directory.GetCurrentDirectory().Replace('\\', '/');
                var abs = Path.GetFullPath(absolute).Replace('\\', '/');
                if (!abs.StartsWith(proj, StringComparison.OrdinalIgnoreCase)) return absolute;
                var rel = abs.Substring(proj.Length).TrimStart('/');
                return rel;
            }
            catch
            {
                return absolute;
            }
        }

        private static string ToProjectRelative(string absolute)
        {
            var proj = Directory.GetCurrentDirectory().Replace('\\', '/');
            var abs = Path.GetFullPath(absolute).Replace('\\', '/');
            return abs.StartsWith(proj, StringComparison.OrdinalIgnoreCase) ? abs.Substring(proj.Length).TrimStart('/') : absolute;
        }

        private struct DumpConfig
        {
            public string SourceFolder;
            public string OutputDirectory;
            public string BaseFileName;
            public string OutputPath;
            public string Extensions;
            public string ExcludeDirs;
            public string IncludeNameContains;
            public string ExcludeNameContains;
            public bool WriteHeader;
            public bool UseUtcTime;
            public bool OpenAfterSave;
            public DateTime Timestamp;
        }

        private struct DumpResult
        {
            public string OutputPath;
            public int FileCount;
            public long LineCount;
            public long ClassCount;
        }
    }
}