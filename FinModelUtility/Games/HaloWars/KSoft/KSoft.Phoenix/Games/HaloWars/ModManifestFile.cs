using System.Collections.ObjectModel;
using System.IO;

namespace KSoft.Phoenix.HaloWars
{
	public sealed class ModManifestFile
		: ObjectModel.BasicViewModel
	{
		#region Sku
		DefinitiveEditionSku mSku_ = DefinitiveEditionSku.UNDEFINED;
		public DefinitiveEditionSku Sku
		{
			get { return this.mSku_; }
			set
			{
				if (this.SetFieldEnum(ref this.mSku_, value))
				{
					this.FilePath = this.Sku.GetModManifestPath();
				}
			}
		}
		#endregion

		#region FilePath
		string mFilePath_;
		public string FilePath
		{
			get { return this.mFilePath_; }
			set
			{
				if (this.SetFieldObj(ref this.mFilePath_, value))
				{
					this.ContainingFolder = this.ContainingFolder;
					this.DisplayTitle = this.DisplayTitle;
				}
			}
		}
		#endregion

		public string ContainingFolder
		{
			get
			{
				if (this.FilePath.IsNullOrEmpty())
					return null;

				string path = this.FilePath;
				path = Path.GetDirectoryName(path);
				return path;
			}
			private set { this.OnPropertyChanged(); }
		}

		public string DisplayTitle
		{
			get { return string.Format("{0} ModManifest - {1}", this.Sku, this.FilePath); }
			set { this.OnPropertyChanged(); }
		}

		public ObservableCollection<ModManifestDirectory> Directories { get; private set; }
			= [];

		public void ReadFromFile()
		{
			if (!File.Exists(this.FilePath))
				return;

			string[] lines = File.ReadAllLines(this.FilePath);

			this.Directories.Clear();

			for (int x = 0; x < lines.Length; x++)
			{
				string line = lines[x];

				var dir = new ModManifestDirectory();
				if (!dir.ReadFromLine(line))
				{
					continue;
				}

				this.Directories.Add(dir);
			}
		}

		public void WriteToFile()
		{
			if (!Directory.Exists(this.ContainingFolder))
				return;

			using (var sw = new StreamWriter(this.FilePath))
			{
				var line = new System.Text.StringBuilder(512);

				foreach (var dir in this.Directories)
				{
					line.Clear();
					if (!dir.WriteToLine(line))
						continue;

					sw.WriteLine(line);
				}
			}
		}
	};

	public sealed class ModManifestDirectory
		: ObjectModel.BasicViewModel
	{
		const char K_DISABLED_PREFIX_ = ';';

		#region IsDisabled
		bool mIsDisabled_;
		public bool IsDisabled
		{
			get { return this.mIsDisabled_; }
			set { this.SetFieldVal(ref this.mIsDisabled_, value); }
		}
		#endregion

		#region Directory
		string mDirectory_;
		public string Directory
		{
			get { return this.mDirectory_; }
			set
			{
				if (this.SetFieldObj(ref this.mDirectory_, value))
				{
					// refresh validity
					this.IsValid = this.IsValid;
					this.DoesExist = this.DoesExist;
				}
			}
		}
		#endregion

		#region IsValid
		public bool IsValid
		{
			get { return this.Directory.IsNotNullOrEmpty(); }
			private set
			{
				this.OnPropertyChanged();
			}
		}
		#endregion

		#region DoesExist
		public bool DoesExist
		{
			get { return this.IsValid && System.IO.Directory.Exists(this.Directory); }
			private set
			{
				this.OnPropertyChanged();
			}
		}
		#endregion

		public bool ReadFromLine(string line)
		{
			if (line.IsNullOrEmpty())
				return false;

			this.IsDisabled = false;

			int directoryStartIndex = 0;
			if (line.StartsWith(K_DISABLED_PREFIX_))
			{
				this.IsDisabled = true;
				directoryStartIndex = 1;
			}

			string dir = line.Substring(directoryStartIndex);
			var invalidChars = Path.GetInvalidPathChars();

			foreach (char c in dir)
			{
				foreach (char invalidChar in invalidChars)
				{
					if (c == invalidChar)
					{
						return false;
					}
				}
			}

			this.Directory = dir;

			return true;
		}

		public bool WriteToLine(System.Text.StringBuilder line)
		{
			if (!this.IsValid)
				return false;

			if (this.IsDisabled)
			{
				line.Append(K_DISABLED_PREFIX_);
			}

			line.Append(this.Directory);

			return true;
		}
	};
}
