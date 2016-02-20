﻿/*
 * HFM.NET - History Entry CSV Serializer
 * Copyright (C) 2009-2016 Ryan Harlamert (harlam357)
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; version 2
 * of the License. See the included file GPLv2.TXT.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.IO;

using HFM.Core.DataTypes;
using HFM.Core.Plugins;

namespace HFM.Core.Serializers
{
   public class HistoryEntryCsvSerializer : IFileSerializer<List<HistoryEntry>>
   {
      public string FileExtension
      {
         get { return "csv"; }
      }

      public string FileTypeFilter
      {
         get { return "Comma Separated Value Files|*.csv"; }
      }

      public List<HistoryEntry> Deserialize(string fileName)
      {
         throw new NotSupportedException("History entry csv deserialization is not supported.");
      }

      public void Serialize(string fileName, List<HistoryEntry> value)
      {
         using (var writer = new StreamWriter(fileName, false))
         {
            string line = String.Join(",", new[]
            {
               "DatabaseID",
               "ProjectID",
               "ProjectRun",
               "ProjectClone",
               "ProjectGen",
               "Name",
               "Path",
               "Username",
               "Team",
               "CoreVersion",
               "FramesCompleted",
               "FrameTime",
               //"FrameTimeValue",
               "Result",
               //"ResultValue",
               "DownloadDateTime",
               "CompletionDateTime",
               "WorkUnitName",
               "KFactor",
               "Core",
               "Frames",
               "Atoms",
               //"BaseCredit",
               "PreferredDays",
               "MaximumDays",
               "SlotType",
               "PPD",
               "Credit"
            });
            writer.WriteLine(line);
            foreach (var h in value)
            {
               line = String.Join(",", new object[]
               {
                  h.ID,
                  h.ProjectID,
                  h.ProjectRun,
                  h.ProjectClone,
                  h.ProjectGen,
                  h.Name,
                  h.Path,
                  h.Username,
                  h.Team,
                  h.CoreVersion,
                  h.FramesCompleted,
                  h.FrameTime,
                  //h.FrameTimeValue,
                  h.Result,
                  //h.ResultValue,
                  h.DownloadDateTime,
                  h.CompletionDateTime,
                  h.WorkUnitName,
                  h.KFactor,
                  h.Core,
                  h.Frames,
                  h.Atoms,
                  //h.BaseCredit,
                  h.PreferredDays,
                  h.MaximumDays,
                  h.SlotType,
                  h.PPD,
                  h.Credit
               });
               writer.WriteLine(line);
            }
         }
      }
   }
}
