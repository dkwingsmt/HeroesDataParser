﻿using Heroes.Models;
using Heroes.Models.Veterancy;
using HeroesData.FileWriter.Writers;
using HeroesData.FileWriter.Writers.AnnouncerData;
using HeroesData.FileWriter.Writers.BannerData;
using HeroesData.FileWriter.Writers.BehaviorVeterancyData;
using HeroesData.FileWriter.Writers.EmoticonData;
using HeroesData.FileWriter.Writers.EmoticonPackData;
using HeroesData.FileWriter.Writers.HeroData;
using HeroesData.FileWriter.Writers.HeroSkinData;
using HeroesData.FileWriter.Writers.MatchAwardData;
using HeroesData.FileWriter.Writers.MountData;
using HeroesData.FileWriter.Writers.PortraitData;
using HeroesData.FileWriter.Writers.SprayData;
using HeroesData.FileWriter.Writers.UnitData;
using HeroesData.FileWriter.Writers.VoiceLineData;
using System.Collections.Generic;

namespace HeroesData.FileWriter
{
    public class FileOutput
    {
        private readonly FileOutputOptions FileOutputOptions;
        private readonly int? HotsBuild;

        private readonly Dictionary<FileOutputType, Dictionary<string, IWritable>> Writers = new Dictionary<FileOutputType, Dictionary<string, IWritable>>();

        /// <summary>
        /// Creates the output files.
        /// </summary>
        public FileOutput()
        {
            FileOutputOptions = new FileOutputOptions();

            Initialize();
        }

        /// <summary>
        /// Creates the output files.
        /// </summary>
        /// <param name="hotsBuild">The hots build number.</param>
        public FileOutput(int? hotsBuild)
        {
            HotsBuild = hotsBuild;
            FileOutputOptions = new FileOutputOptions();

            Initialize();
        }

        /// <summary>
        /// Creates the output files.
        /// </summary>
        /// <param name="fileOutputOptions">Configuration options that can be set from the CLI.</param>
        public FileOutput(FileOutputOptions fileOutputOptions)
        {
            FileOutputOptions = fileOutputOptions;

            Initialize();
        }

        /// <summary>
        /// Creates the output files.
        /// </summary>
        /// <param name="hotsBuild">The hots build number.</param>
        /// <param name="fileOutputOptions">Configuration options that can be set from the CLI.</param>
        public FileOutput(int? hotsBuild, FileOutputOptions fileOutputOptions)
        {
            HotsBuild = hotsBuild;
            FileOutputOptions = fileOutputOptions;

            Initialize();
        }

        /// <summary>
        /// Creates the output files.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The parsed items to be used for the creation of the output.</param>
        /// <param name="fileOutputType">The file type.</param>
        /// <returns></returns>
        public bool Create<T>(IEnumerable<T> items, FileOutputType fileOutputType)
            where T : IExtractable
        {
            if (Writers[fileOutputType].TryGetValue(typeof(T).Name, out IWritable writable))
            {
                writable.FileOutputOptions = FileOutputOptions;
                writable.HotsBuild = HotsBuild;

                ((IWriter<T>)writable).CreateOutput(items);

                return true;
            }

            return false;
        }

        private void Initialize()
        {
            SetWriters();
        }

        private void SetWriters()
        {
            Writers.Add(FileOutputType.Json, new Dictionary<string, IWritable>()
            {
                { nameof(Hero), new HeroDataJsonWriter() },
                { nameof(Unit), new UnitDataJsonWriter() },
                { nameof(MatchAward), new MatchAwardDataJsonWriter() },
                { nameof(HeroSkin), new HeroSkinDataJsonWriter() },
                { nameof(Mount), new MountDataJsonWriter() },
                { nameof(Banner), new BannerDataJsonWriter() },
                { nameof(Spray), new SprayDataJsonWriter() },
                { nameof(Announcer), new AnnouncerDataJsonWriter() },
                { nameof(VoiceLine), new VoiceLineDataJsonWriter() },
                { nameof(Portrait), new PortraitDataJsonWriter() },
                { nameof(Emoticon), new EmoticonDataJsonWriter() },
                { nameof(EmoticonPack), new EmoticonPackDataJsonWriter() },
                { nameof(BehaviorVeterancy), new BehaviorVeterancyDataJsonWriter() },
            });

            Writers.Add(FileOutputType.Xml, new Dictionary<string, IWritable>()
            {
                { nameof(Hero), new HeroDataXmlWriter() },
                { nameof(Unit), new UnitDataXmlWriter() },
                { nameof(MatchAward), new MatchAwardDataXmlWriter() },
                { nameof(HeroSkin), new HeroSkinDataXmlWriter() },
                { nameof(Mount), new MountDataXmlWriter() },
                { nameof(Banner), new BannerDataXmlWriter() },
                { nameof(Spray), new SprayDataXmlWriter() },
                { nameof(Announcer), new AnnouncerDataXmlWriter() },
                { nameof(VoiceLine), new VoiceLineDataXmlWriter() },
                { nameof(Portrait), new PortraitDataXmlWriter() },
                { nameof(Emoticon), new EmoticonDataXmlWriter() },
                { nameof(EmoticonPack), new EmoticonPackDataXmlWriter() },
                { nameof(BehaviorVeterancy), new BehaviorVeterancyDataXmlWriter() },
            });
        }
    }
}
