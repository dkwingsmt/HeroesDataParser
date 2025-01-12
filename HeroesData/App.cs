﻿using Heroes.Models;
using HeroesData.ExtractorData;
using HeroesData.ExtractorImage;
using HeroesData.ExtractorImages;
using HeroesData.FileWriter;
using HeroesData.Loader.XmlGameData;
using HeroesData.Parser;
using HeroesData.Parser.Exceptions;
using HeroesData.Parser.GameStrings;
using HeroesData.Parser.Overrides;
using HeroesData.Parser.XmlData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HeroesData
{
    internal class App
    {
        private readonly List<DataProcessor> DataProcessors = new List<DataProcessor>();

        private GameData GameData;
        private DefaultData DefaultData;
        private XmlDataOverriders XmlDataOverriders;
        private Configuration Configuration;

        /// <summary>
        /// Gets the product version of the application.
        /// </summary>
        public static string Version { get; } = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

        /// <summary>
        /// Gets the assembly path.
        /// </summary>
        public static string AssemblyPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static bool Defaults { get; set; } = true;
        public static bool CreateXml { get; set; } = false;
        public static bool CreateJson { get; set; } = false;
        public static bool ShowValidationWarnings { get; set; } = false;
        public static ExtractDataOption ExtractDataOption { get; set; } = ExtractDataOption.None;
        public static ExtractImageOption ExtractFileOption { get; set; } = ExtractImageOption.None;
        public static bool IsFileSplit { get; set; } = false;
        public static bool IsLocalizedText { get; set; } = false;
        public static bool CreateMinFiles { get; set; } = false;
        public static int? HotsBuild { get; set; } = null;
        public static int? OverrideBuild { get; set; } = null;
        public static int MaxParallelism { get; set; } = -1;
        public static DescriptionType DescriptionType { get; set; } = 0;
        public static string StoragePath { get; set; } = Environment.CurrentDirectory;
        public static string OutputDirectory { get; set; } = string.Empty;

        public static HashSet<string> ValidationIgnoreLines { get; } = new HashSet<string>();

        public StorageMode StorageMode { get; private set; } = StorageMode.None;
        public CASCHotsStorage CASCHotsStorage { get; private set; } = null;
        public List<Localization> Localizations { get; set; } = new List<Localization>();

        /// <summary>
        /// Gets the file path of the verification ignore file.
        /// </summary>
        private static string VerifyIgnoreFilePath => Path.Combine(AssemblyPath, "verifyignore.txt");

        public static void WriteExceptionLog(string fileName, Exception ex)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(AssemblyPath, $"Exception_{fileName}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.txt"), false))
            {
                if (!string.IsNullOrEmpty(ex.Message))
                    writer.Write(ex.Message);

                if (ex is AggregateException)
                {
                    foreach (Exception exception in ((AggregateException)ex).InnerExceptions)
                    {
                        writer.Write(ex);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(ex.StackTrace))
                        writer.Write(ex.StackTrace);
                }
            }
        }

        public void Run()
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine($"Heroes Data Parser ({Version})");
            Console.WriteLine("  --https://github.com/koliva8245/HeroesDataParser");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine();

            int totalLocaleSuccess = 0;

            try
            {
                PreInitialize();
                InitializeGameData();
                InitializeOverrideData();

                SetUpDataProcessors();
                SetupValidationIgnoreFile();

                // set the options for the writers
                FileOutputOptions options = new FileOutputOptions()
                {
                    DescriptionType = DescriptionType,
                    IsFileSplit = IsFileSplit,
                    IsLocalizedText = IsLocalizedText,
                    IsMinifiedFiles = CreateMinFiles,
                    OutputDirectory = OutputDirectory,
                    AllowDataFileWriting = !IsLocalizedText,
                };

                foreach (Localization localization in Localizations)
                {
                    options.Localization = localization;

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"[{localization.GetFriendlyName()}]");
                    Console.ResetColor();

                    LoadGameStrings(localization);

                    // parse gamestrings
                    ParseGameStrings(localization);

                    // parse data
                    DataProcessor((parser) =>
                    {
                        parser.ParsedItems = parser.Parse(localization);
                    });

                    // validate
                    Console.WriteLine("Validating data...");
                    DataProcessor((parser) =>
                    {
                        parser.Validate(localization);
                    });

                    if (!ShowValidationWarnings)
                        Console.WriteLine();

                    // write
                    WriteFileOutput(options);

                    totalLocaleSuccess++;
                }

                // write
                if (IsLocalizedText)
                {
                    options.AllowDataFileWriting = true;
                    WriteFileOutput(options);
                }

                if (ExtractFileOption != ExtractImageOption.None)
                {
                    Console.WriteLine("Extracting files...");
                    DataProcessor((parser) =>
                    {
                        parser.Extract?.Invoke(parser.ParsedItems);
                    });

                    Console.WriteLine();
                }

                if (totalLocaleSuccess == Localizations.Count)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (totalLocaleSuccess > 0)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine($"HDP has completed [{totalLocaleSuccess} out of {Localizations.Count} successful].");
                Console.WriteLine();
            }
            catch (Exception ex) // catch everything
            {
                WriteExceptionLog("Error", ex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.WriteLine();

                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        public void SetCurrentCulture()
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }

        private void PreInitialize()
        {
            LoadConfiguration();
            DetectStoragePathType();

            Console.Write($"Localization(s): ");
            Localizations.ForEach(locale => { Console.Write($"{locale.ToString().ToLower()} "); });
            Console.WriteLine();
            Console.WriteLine();
            Console.ResetColor();

            if (StorageMode == StorageMode.CASC)
            {
                try
                {
                    CASCHotsStorage = CASCHotsStorage.Load(StoragePath);

                    Console.ForegroundColor = ConsoleColor.Cyan;

                    ReadOnlySpan<char> buildName = CASCHotsStorage.CASCHandler.Config.BuildName.AsSpan();
                    int indexOfVersion = buildName.LastIndexOf('.');

                    if (indexOfVersion > -1 && int.TryParse(buildName.Slice(indexOfVersion + 1), out int hotsBuild))
                    {
                        HotsBuild = hotsBuild;
                        Console.WriteLine($"Hots Version Build: {CASCHotsStorage.CASCHandler.Config.BuildName}");
                    }
                    else
                    {
                        Console.WriteLine($"Defaulting to latest build");
                    }
                }
                catch (Exception ex)
                {
                    WriteExceptionLog("casc_storage_loader", ex);
                    ConsoleExceptionMessage("Error: Could not load the Heroes of the Storm data. Check if game is installed correctly.");
                }

                Console.WriteLine();
                Console.ResetColor();
            }
        }

        private void InitializeGameData()
        {
            Stopwatch time = new Stopwatch();

            Console.WriteLine($"Loading xml files...");

            time.Start();
            try
            {
                if (StorageMode == StorageMode.Mods)
                    GameData = new FileGameData(StoragePath, HotsBuild);
                else if (StorageMode == StorageMode.CASC)
                    GameData = new CASCGameData(CASCHotsStorage.CASCHandler, CASCHotsStorage.CASCFolderRoot, HotsBuild);

                GameData.LoadXmlFiles();

                DefaultData = new DefaultData(GameData);
                DefaultData.Load();
            }
            catch (DirectoryNotFoundException ex)
            {
                WriteExceptionLog("gamedata_loader_", ex);
                ConsoleExceptionMessage(ex);
            }

            time.Stop();

            Console.WriteLine($"{GameData.XmlFileCount,6} xml files loaded");
            Console.WriteLine($"{GameData.StormStyleCount,6} storm style files loaded");
            Console.WriteLine($"Finished in {time.Elapsed.TotalSeconds} seconds");
            Console.WriteLine();
        }

        private void InitializeOverrideData()
        {
            Stopwatch time = new Stopwatch();

            Console.WriteLine($"Loading data overriders...");

            time.Start();

            if (OverrideBuild.HasValue)
                XmlDataOverriders = XmlDataOverriders.Load(GameData, OverrideBuild.Value);
            else if (HotsBuild.HasValue)
                XmlDataOverriders = XmlDataOverriders.Load(GameData, HotsBuild.Value);
            else
                XmlDataOverriders = XmlDataOverriders.Load(GameData);

            foreach (string overrideFileName in XmlDataOverriders.LoadedFileNames)
            {
                ReadOnlySpan<char> fileNameNoExtension = Path.GetFileNameWithoutExtension(overrideFileName).AsSpan();

                if (int.TryParse(fileNameNoExtension.Slice(fileNameNoExtension.IndexOf('_') + 1), out int loadedOverrideBuild))
                {
                    if ((StorageMode == StorageMode.Mods && HotsBuild.HasValue && HotsBuild.Value != loadedOverrideBuild) || (StorageMode == StorageMode.CASC && HotsBuild.HasValue && HotsBuild.Value != loadedOverrideBuild))
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else
                        Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else // default override
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }

                Console.WriteLine($"Loaded {Path.GetFileName(overrideFileName)}");
                Console.ResetColor();
            }

            time.Stop();

            Console.WriteLine($"Finished in {time.Elapsed.TotalSeconds} seconds");
            Console.WriteLine();
        }

        private void ParseGameStrings(Localization localization)
        {
            int currentCount = 0;
            int failedCount = 0;
            int totalGameStrings = GameData.GameStringCount + GameData.GameStringMapCount;
            List<string> failedGameStrings = new List<string>();

            Stopwatch time = new Stopwatch();

            GameStringParser gameStringParser = new GameStringParser(Configuration, GameData, HotsBuild);

            Console.WriteLine($"Parsing gamestrings...");

            time.Start();

            Console.Write($"\r{currentCount,6} / {totalGameStrings} total gamestrings");

            try
            {
                Parallel.ForEach(GameData.GameStringIds, new ParallelOptions { MaxDegreeOfParallelism = MaxParallelism }, gamestringId =>
                {
                    if (!gameStringParser.TryParseRawTooltip(gamestringId, GameData.GetGameString(gamestringId), out string parsedGamestring))
                    {
                        failedGameStrings.Add($"{gamestringId}={GameData.GetGameString(gamestringId)}");
                        Interlocked.Increment(ref failedCount);
                    }

                    // always add
                    GameData.AddGameString(gamestringId, parsedGamestring);

                    Console.Write($"\r{Interlocked.Increment(ref currentCount),6} / {totalGameStrings} total gamestrings");
                });

                // map specific data
                foreach (string mapName in GameData.MapIds)
                {
                    GameData mapGameData = GameData.GetMapGameData(mapName);
                    GameData.AppendGameData(mapGameData);

                    Parallel.ForEach(mapGameData.GameStringIds, new ParallelOptions { MaxDegreeOfParallelism = MaxParallelism }, gamestringId =>
                    {
                        if (!gameStringParser.TryParseRawTooltip(gamestringId, mapGameData.GetGameString(gamestringId), out string parsedGamestring))
                        {
                            failedGameStrings.Add($"[{mapName}]:{gamestringId}={GameData.GetGameString(gamestringId)}");
                            Interlocked.Increment(ref failedCount);
                        }

                        // always add
                        GameData.AddMapGameString(mapName, gamestringId, parsedGamestring);

                        Console.Write($"\r{Interlocked.Increment(ref currentCount),6} / {totalGameStrings} total gamestrings");
                    });

                    GameData.RestoreGameData();
                }
            }
            catch (AggregateException ae)
            {
                WriteExceptionLog($"gamestrings_{localization.ToString().ToLower()}", ae);

                ae.Handle(ex =>
                {
                    if (ex is GameStringParseException)
                    {
                        ConsoleExceptionMessage($"{Environment.NewLine}{ex.Message}");
                    }

                    return ex is GameStringParseException;
                });
            }

            Console.WriteLine();

            time.Stop();

            if (failedCount > 0)
            {
                WriteInvalidGameStrings(failedGameStrings, localization);
                Console.ForegroundColor = ConsoleColor.Yellow;
            }

            Console.WriteLine($"{totalGameStrings - failedCount,6} successfully parsed gamestrings");

            Console.ResetColor();
            Console.WriteLine($"Finished in {time.Elapsed.TotalSeconds} seconds");
            Console.WriteLine();
        }

        private void LoadConfiguration()
        {
            Configuration = new Configuration();
            if (!Configuration.ConfigFileExists())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Configuration.ConfigFileName} not found. Unable to continue.");
                Console.ResetColor();
                Environment.Exit(1);
            }

            Configuration.Load();
        }

        /// <summary>
        /// Determine the type of storage, Hots folder or mods extracted.
        /// </summary>
        private void DetectStoragePathType()
        {
            string modsPath = StoragePath;
            string hotsPath = StoragePath;

            if (Defaults)
            {
                modsPath = Path.Combine(StoragePath, "mods");

                if (Directory.GetParent(StoragePath) != null)
                    hotsPath = Directory.GetParent(StoragePath).FullName;
            }

            if (Directory.Exists(modsPath) &&
                Directory.Exists(Path.Combine(modsPath, "core.stormmod")) && Directory.Exists(Path.Combine(modsPath, "heroesdata.stormmod")) && Directory.Exists(Path.Combine(modsPath, "heromods")))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Found 'mods' directory");

                StoragePath = modsPath;
                GetModsDirectoryBuild();
                StorageMode = StorageMode.Mods;
            }
            else if (IsMultiModsDirectory())
            {
                StorageMode = StorageMode.Mods;
            }
            else if (Directory.Exists(hotsPath) && Directory.Exists(Path.Combine(hotsPath, "HeroesData")) && File.Exists(Path.Combine(hotsPath, ".build.info")))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Found 'Heroes of the Storm' directory");

                StoragePath = hotsPath;
                StorageMode = StorageMode.CASC;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not find a 'mods' or 'Heroes of the Storm' storage directory");
                Console.WriteLine();

                Console.ResetColor();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Attempts to get the build number from the mods folder.
        /// </summary>
        private void GetModsDirectoryBuild()
        {
            ReadOnlySpan<char> storagePath = StoragePath.AsSpan();
            ReadOnlySpan<char> lastDirectory = storagePath.Slice(storagePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            int indexOfBuild = lastDirectory.LastIndexOf('_');

            if (indexOfBuild > -1 && int.TryParse(lastDirectory.Slice(indexOfBuild + 1), out int value))
            {
                HotsBuild = value;

                Console.WriteLine($"Hots build: {HotsBuild}");
            }
            else
            {
                Console.WriteLine($"Defaulting to latest build");
            }
        }

        /// <summary>
        /// Checks for multiple mods folders with a number suffix, selects the highest one and sets the storage path.
        /// </summary>
        /// <returns></returns>
        private bool IsMultiModsDirectory()
        {
            ReadOnlySpan<string> directories = Directory.GetDirectories(StoragePath, "mods_*", SearchOption.TopDirectoryOnly).AsSpan();

            if (directories.Length < 1)
                return false;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Found 'mods_*' directory(s)");

            int max = 0;
            ReadOnlySpan<char> selectedDirectory = null;

            foreach (ReadOnlySpan<char> directory in directories)
            {
                ReadOnlySpan<char> lastDirectory = directory.Slice(directory.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                int indexOfBuild = lastDirectory.LastIndexOf('_');

                if (indexOfBuild > -1 && int.TryParse(lastDirectory.Slice(indexOfBuild + 1), out int value) && value >= max)
                {
                    max = value;
                    selectedDirectory = lastDirectory;
                }
            }

            if (!selectedDirectory.IsEmpty)
            {
                StoragePath = Path.Combine(StoragePath, selectedDirectory.ToString());
                HotsBuild = max;

                Console.WriteLine($"Using {StoragePath}");
                Console.WriteLine($"Hots build: {max}");
                Console.WriteLine();
                Console.ResetColor();

                return true;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("mods_* directories are not valid");
            Console.WriteLine();
            Console.ResetColor();
            return false;
        }

        private void LoadGameStrings(Localization localization)
        {
            Stopwatch time = new Stopwatch();

            GameData.GameStringLocalization = localization.GetFriendlyName();

            time.Start();

            try
            {
                GameData.LoadGamestringFiles();

                Console.WriteLine("Loading text files...");
                Console.WriteLine($"{GameData.TextFileCount,6} text files loaded");

                time.Stop();

                Console.WriteLine($"Finished in {time.Elapsed.TotalSeconds} seconds");

                Console.WriteLine();
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
            {
                WriteExceptionLog($"gamestrings_loader_{localization.ToString().ToLower()}", ex);

                if (StorageMode == StorageMode.CASC)
                    ConsoleExceptionMessage($"Gamestrings could not be loaded. Check if localization is installed in the game client.");
                else
                    ConsoleExceptionMessage($"Gamestrings could not be loaded. {ex.Message}.");
            }
        }

        private void WriteInvalidGameStrings(List<string> invalidGameStrings, Localization localization)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(AssemblyPath, $"InvalidGamestrings_{localization.ToString().ToLower()}.txt"), false))
            {
                foreach (string gamestring in invalidGameStrings)
                {
                    writer.WriteLine(gamestring);
                }
            }
        }

        private void SetupValidationIgnoreFile()
        {
            if (File.Exists(VerifyIgnoreFilePath))
            {
                using (StreamReader reader = new StreamReader(VerifyIgnoreFilePath))
                {
                    string line = string.Empty;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (!string.IsNullOrEmpty(line) && !line.StartsWith("#"))
                        {
                            ValidationIgnoreLines.Add(line);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes a message to the console. Will change the text to read and then shutdown the application with exit code 1.
        /// </summary>
        /// <param name="message">The message to outptut.</param>
        private void ConsoleExceptionMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            Environment.Exit(1);
        }

        /// <summary>
        /// Writes a message to the console. Will change the text to read and then shutdown the application with exit code 1.
        /// </summary>
        /// <param name="message">The message to outptut.</param>
        private void ConsoleExceptionMessage(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ResetColor();
            Environment.Exit(1);
        }

        private void DataProcessor(Action<DataProcessor> action)
        {
            foreach (DataProcessor processor in DataProcessors)
            {
                if (processor.IsEnabled)
                {
                    action(processor);
                }
            }
        }

        private void WriteFileOutput(FileOutputOptions options)
        {
            // write
            FileOutput fileOutput = new FileOutput(HotsBuild, options);

            if (options.AllowDataFileWriting)
                Console.WriteLine("Creating output file(s)...");
            else
                Console.WriteLine("Creating gamestring data...");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Directory: {options.OutputDirectory}");
            Console.ResetColor();
            DataProcessor((parser) =>
            {
                if (options.AllowDataFileWriting)
                {
                    if (CreateJson)
                    {
                        Console.Write($"[{parser.Name}] Writing json file(s)...");

                        if (fileOutput.Create((dynamic)parser.ParsedItems, FileOutputType.Json))
                        {
                            Console.WriteLine("Done.");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed.");
                            Console.ResetColor();
                        }
                    }

                    if (CreateXml)
                    {
                        Console.Write($"[{parser.Name}] Writing xml file(s)...");

                        if (fileOutput.Create((dynamic)parser.ParsedItems, FileOutputType.Xml))
                        {
                            Console.WriteLine("Done.");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed.");
                            Console.ResetColor();
                        }
                    }
                }
                else
                {
                    // only need to parsed through one type of file to get the gamestrings
                    Console.Write($"[{parser.Name}] Writing gamestrings...");

                    if (fileOutput.Create((dynamic)parser.ParsedItems, FileOutputType.Json))
                    {
                        Console.WriteLine("Done.");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Failed.");
                        Console.ResetColor();
                    }
                }

                Console.ResetColor();
            });

            Console.WriteLine();
        }

        private void SetUpDataProcessors()
        {
            IXmlDataService xmlDataService = new XmlDataService(Configuration, GameData, DefaultData);

            DataHero dataHero = new DataHero(new HeroDataParser(xmlDataService.GetInstance(), (HeroOverrideLoader)XmlDataOverriders.GetOverrider(typeof(HeroDataParser))));
            DataUnit dataUnit = new DataUnit(new UnitParser(xmlDataService.GetInstance(), (UnitOverrideLoader)XmlDataOverriders.GetOverrider(typeof(UnitParser))));
            DataMatchAward dataMatchAward = new DataMatchAward(new MatchAwardParser(xmlDataService.GetInstance(), (MatchAwardOverrideLoader)XmlDataOverriders.GetOverrider(typeof(MatchAwardParser))));
            DataHeroSkin dataHeroSkin = new DataHeroSkin(new HeroSkinParser(xmlDataService.GetInstance()));
            DataMount dataMount = new DataMount(new MountParser(xmlDataService.GetInstance()));
            DataBanner dataBanner = new DataBanner(new BannerParser(xmlDataService.GetInstance()));
            DataSpray dataSpray = new DataSpray(new SprayParser(xmlDataService.GetInstance()));
            DataAnnouncer dataAnnouncer = new DataAnnouncer(new AnnouncerParser(xmlDataService.GetInstance()));
            DataVoiceLine dataVoiceLine = new DataVoiceLine(new VoiceLineParser(xmlDataService.GetInstance()));
            DataPortrait dataPortrait = new DataPortrait(new PortraitParser(xmlDataService.GetInstance()));
            DataEmoticon dataEmoticon = new DataEmoticon(new EmoticonParser(xmlDataService.GetInstance()));
            DataEmoticonPack dataEmoticonPack = new DataEmoticonPack(new EmoticonPackParser(xmlDataService.GetInstance()));
            DataBehaviorVeterancy dataBehaviorVeterancy = new DataBehaviorVeterancy(new BehaviorVeterancyParser(xmlDataService.GetInstance()));

            ImageHero filesHero = new ImageHero(CASCHotsStorage?.CASCHandler, StoragePath);
            ImageUnit filesUnit = new ImageUnit(CASCHotsStorage?.CASCHandler, StoragePath);
            ImageMatchAward filesMatchAward = new ImageMatchAward(CASCHotsStorage?.CASCHandler, StoragePath);
            ImageAnnouncer filesAnnouncer = new ImageAnnouncer(CASCHotsStorage?.CASCHandler, StoragePath);
            ImageVoiceLine filesVoiceLine = new ImageVoiceLine(CASCHotsStorage?.CASCHandler, StoragePath);
            ImageSpray filesSpray = new ImageSpray(CASCHotsStorage?.CASCHandler, StoragePath);
            ImageEmoticon filesEmoticon = new ImageEmoticon(CASCHotsStorage?.CASCHandler, StoragePath);

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.HeroData),
                Name = dataHero.Name,
                Parse = (localization) => dataHero.Parse(localization),
                Validate = (localization) => dataHero.Validate(localization),
                Extract = (data) => filesHero.ExtractFiles(data),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.Unit),
                Name = dataUnit.Name,
                Parse = (localization) => dataUnit.Parse(localization),
                Validate = (localization) => dataUnit.Validate(localization),
                Extract = (data) => filesUnit.ExtractFiles(data),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.MatchAward),
                Name = dataMatchAward.Name,
                Parse = (localization) => dataMatchAward.Parse(localization),
                Validate = (localization) => dataMatchAward.Validate(localization),
                Extract = (data) => filesMatchAward.ExtractFiles(data),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.HeroSkin),
                Name = dataHeroSkin.Name,
                Parse = (localization) => dataHeroSkin.Parse(localization),
                Validate = (localization) => dataHeroSkin.Validate(localization),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.Mount),
                Name = dataMount.Name,
                Parse = (localization) => dataMount.Parse(localization),
                Validate = (localization) => dataMount.Validate(localization),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.Banner),
                Name = dataBanner.Name,
                Parse = (localization) => dataBanner.Parse(localization),
                Validate = (localization) => dataBanner.Validate(localization),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.Spray),
                Name = dataSpray.Name,
                Parse = (localization) => dataSpray.Parse(localization),
                Validate = (localization) => dataSpray.Validate(localization),
                Extract = (data) => filesSpray.ExtractFiles(data),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.Announcer),
                Name = dataAnnouncer.Name,
                Parse = (localization) => dataAnnouncer.Parse(localization),
                Validate = (localization) => dataAnnouncer.Validate(localization),
                Extract = (data) => filesAnnouncer.ExtractFiles(data),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.VoiceLine),
                Name = dataVoiceLine.Name,
                Parse = (localization) => dataVoiceLine.Parse(localization),
                Validate = (localization) => dataVoiceLine.Validate(localization),
                Extract = (data) => filesVoiceLine.ExtractFiles(data),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.Portrait),
                Name = dataPortrait.Name,
                Parse = (localization) => dataPortrait.Parse(localization),
                Validate = (localization) => dataPortrait.Validate(localization),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.Emoticon),
                Name = dataEmoticon.Name,
                Parse = (localization) => dataEmoticon.Parse(localization),
                Validate = (localization) => dataEmoticon.Validate(localization),
                Extract = (data) => filesEmoticon.ExtractFiles(data),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.EmoticonPack),
                Name = dataEmoticonPack.Name,
                Parse = (localization) => dataEmoticonPack.Parse(localization),
                Validate = (localization) => dataEmoticonPack.Validate(localization),
            });

            DataProcessors.Add(new DataProcessor()
            {
                IsEnabled = ExtractDataOption.HasFlag(ExtractDataOption.Veterancy),
                Name = dataBehaviorVeterancy.Name,
                Parse = (localization) => dataBehaviorVeterancy.Parse(localization),
                Validate = (localization) => dataBehaviorVeterancy.Validate(localization),
            });
        }
    }
}
