// This file is part of iRacingReplayOverlay.
//
// Copyright 2014 Dean Netherton
// https://github.com/vipoo/iRacingReplayOverlay.net
//
// iRacingReplayOverlay is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// iRacingReplayOverlay is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with iRacingReplayOverlay.  If not, see <http://www.gnu.org/licenses/>.
//

using iRacingReplayOverlay.Phases.Transcoding;
using iRacingSDK;
using iRacingSDK.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace iRacingReplayOverlay.Phases.Capturing
{
    public class CapturedVideoFile
    {
        public string FileName;

        public bool isIntroVideo = false;

        public override string ToString()
        {
            return isIntroVideo ? "{0} (intro)".F(FileName) : FileName;
        }

    }

    public static class CapturedVideoFileExtensions
    {
        public static CapturedVideoFile Intro(this IEnumerable<CapturedVideoFile> files)
        {
            return files.FirstOrDefault(f => f.isIntroVideo);
        }

        public static IEnumerable<SourceReaderExtra> ToSourceReaderExtra(this IEnumerable<CapturedVideoFile> files)
        {
            return files.Select(f => new SourceReaderExtra(f.FileName, f));
        }
    }

    public class OverlayData
    {
        public class RaceEvent
        {
            public double StartTime;
            public double EndTime;
            public InterestState Interest;
            public bool WithOvertake;

            public override int GetHashCode()
            {
                return StartTime.GetHashCode() | EndTime.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var other = obj as RaceEvent;
                if (other == null)
                    return false;

                return StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime) && Interest.Equals(other.Interest);
            }

            public override string ToString()
            {
                return "StartTime: {0}, EndTime: {1}, Interest: {2}".F(StartTime, EndTime, Interest.ToString());
            }

            public double Duration { get { return EndTime - StartTime; } }
        }

        public class MessageState
        {
            public string[] Messages;
            public double Time;
        }

        public class CamDriver
        {
            public double StartTime;
            public Driver CurrentDriver;
        }

        public class LeaderBoard
        {
            public double StartTime;
            public Driver[] Drivers;
            public string RacePosition;
			public string LapCounter;

            public LeaderBoard Clone()
            {
                return (LeaderBoard)base.MemberwiseClone();
            }
        }

        public class Driver
        {
            public int? Position;
            public string CarNumber;
            public string UserName;
            public int PitStopCount;

            [XmlIgnore]
            public int CarIdx;

            [XmlIgnore]
            public string ShortName
            {
                get
                {
                    if (UserName.Length <= 4)
                        return UserName;

                    var names = UserName.Split(' ');
                    var firstName = names.First();
                    var lastName = names.Last();

                    var name = firstName.Substring(0, 1).ToUpper()
                        + lastName.Substring(0, 1).ToUpper()
                        + lastName.Substring(1, Math.Min(3, lastName.Length-1));

                    return name;
                }
            }

            public Driver Clone()
            {
                return (Driver)base.MemberwiseClone();
            }
        }

        public class FastLap
        {
            public long StartTime;
            public Driver Driver;
            public double Time;
        }

        public List<LeaderBoard> LeaderBoards = new List<LeaderBoard>();
        public List<CamDriver> CamDrivers = new List<CamDriver>();
        public List<FastLap> FastestLaps = new List<FastLap>();
        public List<MessageState> MessageStates = new List<MessageState>();
        public SessionData SessionData;
        public List<RaceEvent> RaceEvents = new List<RaceEvent>();
        public double? TimeForOutroOverlay = null;
        public List<CapturedVideoFile> VideoFiles;
        public string CapturedVersion = null;

        public void SaveTo(string fileName)
        {
            var writer = new XmlSerializer(typeof(OverlayData));

            using (var file = new StreamWriter(fileName))
                writer.Serialize(file, this);
        }

        public static OverlayData FromFile(string fileName)
        {
            var reader = new XmlSerializer(typeof(OverlayData));
            using (var file = new StreamReader(fileName))
            {
                return (OverlayData)reader.Deserialize(file);
            }
        }
    }
}
