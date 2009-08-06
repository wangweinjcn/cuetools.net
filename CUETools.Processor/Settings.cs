// ****************************************************************************
// 
// CUE Tools
// Copyright (C) 2006-2007  Moitah (moitah@yahoo.com)
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// 
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CUETools.Processor
{
	static class SettingsShared 
	{
		public static string GetMyAppDataDir(string appName) {
			string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string myAppDataDir = Path.Combine(appDataDir, appName);

			if (Directory.Exists(myAppDataDir) == false) {
				Directory.CreateDirectory(myAppDataDir);
			}

			return myAppDataDir;
		}

		public static string GetProfileDir(string appName, string appPath)
		{
			bool userProfilesEnabled = (appPath == null || File.Exists(Path.Combine(Path.GetDirectoryName(appPath), "user_profiles_enabled")));
			string appDataDir = userProfilesEnabled ? 
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) :
				Path.GetDirectoryName(appPath);
			string myAppDataDir = Path.Combine(appDataDir, appName);
			if (!Directory.Exists(myAppDataDir))
				Directory.CreateDirectory(myAppDataDir);
			return myAppDataDir;
		}
	}

	public class SettingsReader {
		Dictionary<string, string> _settings;
		string profilePath;

		public string ProfilePath
		{
			get
			{
				return profilePath;
			}
		}

		public SettingsReader(string appName, string fileName, string appPath) {
			_settings = new Dictionary<string, string>();
			profilePath = SettingsShared.GetProfileDir(appName, appPath);
			string path = Path.Combine(profilePath, fileName);
			if (!File.Exists(path))
				return;

			using (StreamReader sr = new StreamReader(path, Encoding.UTF8)) {
				string line, name = null, val;
				int pos;

				while ((line = sr.ReadLine()) != null) {
					pos = line.IndexOf('=');
					if (pos != -1) {
						if (pos > 0)
						{
							name = line.Substring(0, pos);
							val = line.Substring(pos + 1);
							if (!_settings.ContainsKey(name))
								_settings.Add(name, val);
						}
						else
						{
							val = line.Substring(pos + 1);
							if (_settings.ContainsKey(name))
								_settings[name] += "\r\n" + val;
						}
					}
				}
			}
		}

		public string Load(string name) {
			return _settings.ContainsKey(name) ? _settings[name] : null;
		}

		public bool? LoadBoolean(string name) {
			string val = Load(name);
			if (val == "0") return false;
			if (val == "1") return true;
			return null;
		}

		public int? LoadInt32(string name, int? min, int? max) {
			int val;
			if (!Int32.TryParse(Load(name), out val)) return null;
			if (min.HasValue && (val < min.Value)) return null;
			if (max.HasValue && (val > max.Value)) return null;
			return val;
		}

		public uint? LoadUInt32(string name, uint? min, uint? max) {
			uint val;
			if (!UInt32.TryParse(Load(name), out val)) return null;
			if (min.HasValue && (val < min.Value)) return null;
			if (max.HasValue && (val > max.Value)) return null;
			return val;
		}

		public long? LoadLong(string name, long? min, long? max)
		{
			long val;
			if (!long.TryParse(Load(name), out val)) return null;
			if (min.HasValue && (val < min.Value)) return null;
			if (max.HasValue && (val > max.Value)) return null;
			return val;
		}

		public DateTime? LoadDate(string name)
		{
			long? val = LoadLong(name, null, null);
			if (!val.HasValue) return null;
			return DateTime.FromBinary(val.Value);
		}
	}

	public class SettingsWriter {
		StreamWriter _sw;

		public SettingsWriter(string appName, string fileName, string appPath)
		{
			string path = Path.Combine(SettingsShared.GetProfileDir(appName, appPath), fileName);
			_sw = new StreamWriter(path, false, Encoding.UTF8);
		}

		public void Save(string name, string value) {
			_sw.WriteLine(name + "=" + value);
		}

		public void SaveText(string name, string value)
		{
			_sw.Write(name);
			if (value == "")
			{
				_sw.WriteLine("=");
				return;
			}
			using (StringReader sr = new StringReader(value))
			{
				string lineStr;
				while ((lineStr = sr.ReadLine()) != null)
					_sw.WriteLine("=" + lineStr);
			}
		}

		public void Save(string name, bool value) {
			Save(name, value ? "1" : "0");
		}

		public void Save(string name, int value) {
			Save(name, value.ToString());
		}

		public void Save(string name, uint value) {
			Save(name, value.ToString());
		}

		public void Save(string name, long value)
		{
			Save(name, value.ToString());
		}

		public void Save(string name, DateTime value)
		{
			Save(name, value.ToBinary());
		}

		public void Close() {
			_sw.Close();
		}
	}
}