using System;
using System.Collections.Generic;

[Serializable]
public class BeatmapData
{
    public GeneralData general;
    public MetadataData metadata;
    public DifficultyData difficulty;
    public List<TimingPoint> timingpoints;
    public List<HitObject> hitobjects;
}

[Serializable]
public class GeneralData
{
    public string AudioFilename;
    public string AudioLeadIn;
    public string PreviewTime;
    public string Countdown;
    public string SampleSet;
    public string StackLeniency;
    public string Mode;
    public string LetterboxInBreaks;
    public string SpecialStyle;
    public string WidescreenStoryboard;
    public string DistanceSpacing;
    public string BeatDivisor;
    public string GridSize;
    public string TimelineZoom;
}

[Serializable]
public class MetadataData
{
    public string Title;
    public string TitleUnicode;
    public string Artist;
    public string ArtistUnicode;
    public string Creator;
    public string Version;
    public string Source;
    public string Tags;
    public string BeatmapID;
    public string BeatmapSetID;
}

[Serializable]
public class DifficultyData
{
    public string HPDrainRate;
    public string CircleSize;
    public string OverallDifficulty;
    public string ApproachRate;
    public string SliderMultiplier;
    public string SliderTickRate;
}

[Serializable]
public class TimingPoint
{
    public string offset;
    public string millperbeat;
}

[Serializable]
public class HitObject
{
    public string x;
    public string y;
    public string time;
    public string type;
    public string hitsound;
    public string extras;
}