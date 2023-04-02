namespace igLibrary.Core
{
	//This'd be called igWin32StorageDevice, igPosixStorageDevice, igPS3StorageDevice, etc but why bother
	public class igWin32StorageDevice : igPhysicalStorageDevice
	{
		private string getPath(igFileWorkItem workItem) => Path.Combine(igFileContext.Singleton._root, workItem._path);
		public override void Close(igFileWorkItem workItem)
		{
			((FileStream)workItem._file._handle).Close();
		}

		public override void Commit(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Exists(igFileWorkItem workItem)
		{
			if(File.Exists(getPath(workItem)))
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
			}
			else
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
		}

		public override void Format(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void GetFileList(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void GetFileListWithSizes(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Mkdir(igFileWorkItem workItem)
		{
			Directory.CreateDirectory(getPath(workItem));
		}

		public override void Open(igFileWorkItem workItem)
		{
			FileAccess access = (FileAccess)(workItem._flags & 0b11);

			FileMode mode = (FileMode)(workItem._flags >> 8);

			try
			{
				workItem._file._handle = File.Open(getPath(workItem), mode, access);
				workItem._file._device = this;
				workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
			}
			catch(ArgumentOutOfRangeException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusBadParam);
			}
			catch(ArgumentException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
			catch(PathTooLongException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
			catch(DirectoryNotFoundException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
			catch(FileNotFoundException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
			catch(IOException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusGeneralError);
			}
			catch(UnauthorizedAccessException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusPermissionDenied);
			}
		}

		public override void Prefetch(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Read(igFileWorkItem workItem)
		{
			FileStream fs = (FileStream)workItem._file._handle;
			long initialOffset = fs.Position;
			try
			{
				fs.Seek((long)workItem._offset, SeekOrigin.Begin);
			}
			catch(IOException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusGeneralError);
				return;
			}
			catch(ArgumentException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusGeneralError);
				return;
			}
			catch(ObjectDisposedException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusStopped);
				return;
			}

  			try
			{
				fs.Write((byte[])workItem._buffer);
				workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
			}
			catch(ArgumentNullException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusBadParam);
			}
			catch(IOException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusReadError);
			}
			finally
			{
				fs.Seek(initialOffset, SeekOrigin.Begin);
			}
		}

		public override void Rename(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Rmdir(igFileWorkItem workItem)
		{
			Directory.Delete(getPath(workItem), true);
		}

		public override void Truncate(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Unlink(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Write(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}
	}
}