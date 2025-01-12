﻿using CASCLib;
using Heroes.Models;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;

namespace HeroesData.ExtractorImage
{
    public class ImageSpray : ImageExtractorBase<Spray>, IImage
    {
        private readonly int ImageMaxHeight = 256;
        private readonly int ImageMaxWidth = 256;
        private readonly HashSet<Spray> Sprays = new HashSet<Spray>();
        private readonly string SprayDirectory = "sprays";

        public ImageSpray(CASCHandler cascHandler, string modsFolderPath)
            : base(cascHandler, modsFolderPath)
        {
        }

        protected override void ExtractFiles()
        {
            if (App.ExtractFileOption.HasFlag(ExtractImageOption.Spray))
                ExtractSprayImages();
        }

        protected override void LoadFileData(Spray spray)
        {
            if (!string.IsNullOrEmpty(spray.ImageFileName))
                Sprays.Add(spray);
        }

        private void ExtractSprayImages()
        {
            if (Sprays == null || Sprays.Count < 1)
                return;

            int count = 0;
            Console.Write($"Extracting spray image files...{count}/{Sprays.Count}");

            string extractFilePath = Path.Combine(ExtractDirectory, SprayDirectory);

            foreach (Spray spray in Sprays)
            {
                bool success = false;
                string filePath = Path.Combine(extractFilePath, spray.ImageFileName);

                if (ExtractStaticImageFile(filePath))
                    success = true;

                if (success && spray.AnimationCount > 0)
                {
                    success = ExtractAnimatedImageFile(filePath, new Size(ImageMaxWidth, ImageMaxHeight), new Size(ImageMaxWidth, ImageMaxHeight), spray.AnimationCount, spray.AnimationDuration / 2);
                }

                if (success)
                    count++;

                Console.Write($"\rExtracting spray image files...{count}/{Sprays.Count}");
            }

            Console.WriteLine(" Done.");
        }
    }
}
