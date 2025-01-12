﻿using Heroes.Models;
using HeroesData.Parser;
using System.Linq;

namespace HeroesData.ExtractorData
{
    public class DataEmoticonPack : DataExtractorBase<EmoticonPack, EmoticonPackParser>, IData
    {
        public DataEmoticonPack(EmoticonPackParser parser)
            : base(parser)
        {
        }

        public override string Name => "emoticonpacks";

        protected override void Validation(EmoticonPack emoticonPack)
        {
            if (string.IsNullOrEmpty(emoticonPack.Name))
                AddWarning($"{nameof(emoticonPack.Name)} is empty");

            if (string.IsNullOrEmpty(emoticonPack.Id))
                AddWarning($"{nameof(emoticonPack.Id)} is empty");

            if (string.IsNullOrEmpty(emoticonPack.HyperlinkId))
                AddWarning($"{nameof(emoticonPack.HyperlinkId)} is empty");

            if (string.IsNullOrEmpty(emoticonPack.CollectionCategory))
                AddWarning($"{nameof(emoticonPack.CollectionCategory)} is empty");

            if (!emoticonPack.ReleaseDate.HasValue)
                AddWarning($"{nameof(emoticonPack.ReleaseDate)} is null");

            if (emoticonPack.EmoticonIds == null || !emoticonPack.EmoticonIds.Any())
                AddWarning($"{nameof(emoticonPack.EmoticonIds)} is null or does not contain any emoticons");

            if (!emoticonPack.EmoticonIds.Any())
                AddWarning($"{nameof(emoticonPack.EmoticonIds)} does not contain any aliases.");
        }
    }
}
