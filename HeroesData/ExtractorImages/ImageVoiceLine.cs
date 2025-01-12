﻿using CASCLib;
using Heroes.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace HeroesData.ExtractorImage
{
    public class ImageVoiceLine : ImageExtractorBase<VoiceLine>, IImage
    {
        private readonly HashSet<string> VoiceLines = new HashSet<string>();

        private readonly string VoiceDirectory = "voicelines";

        public ImageVoiceLine(CASCHandler cascHandler, string modsFolderPath)
            : base(cascHandler, modsFolderPath)
        {
        }

        protected override void ExtractFiles()
        {
            if (App.ExtractFileOption.HasFlag(ExtractImageOption.VoiceLine))
                ExtractVoiceLineImages();
        }

        protected override void LoadFileData(VoiceLine voiceLine)
        {
            if (!string.IsNullOrEmpty(voiceLine.ImageFileName))
                VoiceLines.Add(voiceLine.ImageFileName);
        }

        private void ExtractVoiceLineImages()
        {
            if (VoiceLines == null || VoiceLines.Count < 1)
                return;

            int count = 0;
            Console.Write($"Extracting voiceline image files...{count}/{VoiceLines.Count}");

            string extractFilePath = Path.Combine(ExtractDirectory, VoiceDirectory);

            foreach (string voiceline in VoiceLines)
            {
                if (ExtractStaticImageFile(Path.Combine(extractFilePath, voiceline)))
                    count++;

                Console.Write($"\rExtracting voiceline image files...{count}/{VoiceLines.Count}");
            }

            Console.WriteLine(" Done.");
        }
    }
}
