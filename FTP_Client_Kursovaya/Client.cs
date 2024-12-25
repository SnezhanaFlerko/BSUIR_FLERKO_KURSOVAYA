using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FTP_Client_Kursovaya
{


	public class FileDirectoryInfo
	{
		string fileSize;
		string type;
		string name;
		string date;
		public string adress;

		public string FileSize
		{
			get { return fileSize; }
			set { fileSize = value; }
		}

		public string Type
		{
			get { return type; }
			set { type = value; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string Date
		{
			get { return date; }
			set { date = value; }
		}

		public FileDirectoryInfo() { }

		public FileDirectoryInfo(string fileSize, string type, string name, string date, string adress)
		{
			FileSize = fileSize;
			Type = type;
			Name = name;
			Date = date;
			this.adress = adress;
		}

	}
	public class Client
	{
		private string password;
		private string userName;
		private string uri;
		private int bufferSize = 1024;

		public bool Passive = true;
		public bool Binary = true;
		public bool EnableSsl = false;
		public bool Hash = false;

		public Client(string uri, string userName, string password)
		{
			this.uri = uri;
			this.userName = userName;
			this.password = password;
		}

		public string ChangeWorkingDirectory(string path)
		{
			uri = combine(uri, path);
			return PrintWorkingDirectory();
		}


		public string DeleteFile(string fileName)
		{
			var request = createRequest(combine(uri, fileName), WebRequestMethods.Ftp.DeleteFile);
			return getStatusDescription(request);
		}

		public string DownloadFile(string source, string dest)
		{
			var request = createRequest(combine(uri, source), WebRequestMethods.Ftp.DownloadFile);
			byte[] buffer = new byte[bufferSize];
			using (var response = (FtpWebResponse)request.GetResponse())
			{
				using (var stream = response.GetResponseStream())
				{
					using (var fs = new FileStream(dest, FileMode.OpenOrCreate))
					{
						int readCount = stream.Read(buffer, 0, bufferSize);
						while (readCount > 0)
						{
							if (Hash)
								Console.Write("#");

							fs.Write(buffer, 0, readCount);
							readCount = stream.Read(buffer, 0, bufferSize);
						}
					}
				}
				return response.StatusDescription;
			}
		}


		public string[] ListDirectoryDetails()
		{
			var list = new List<string>();
			var request = createRequest(WebRequestMethods.Ftp.ListDirectoryDetails);
			using (var response = (FtpWebResponse)request.GetResponse())
			{
				using (var stream = response.GetResponseStream())
				{
					using (var reader = new StreamReader(stream, true))
					{
						while (!reader.EndOfStream)
						{
							list.Add(reader.ReadLine());
						}
					}
				}
			}
			return list.ToArray();
		}

		public string MakeDirectory(string directoryName)
		{
			var request = createRequest(combine(uri, directoryName), WebRequestMethods.Ftp.MakeDirectory);
			return getStatusDescription(request);
		}

		public string PrintWorkingDirectory()
		{
			var request = createRequest(WebRequestMethods.Ftp.PrintWorkingDirectory);
			return getStatusDescription(request);
		}

		public string RemoveDirectory(string directoryName)
		{
			var request = createRequest(combine(uri, directoryName), WebRequestMethods.Ftp.RemoveDirectory);
			return getStatusDescription(request);
		}


		public string UploadFile(string source, string destination)
		{
			var request = createRequest(combine(uri, destination), WebRequestMethods.Ftp.UploadFile);
			using (var stream = request.GetRequestStream())
			{
				using (var fileStream = System.IO.File.Open(source, FileMode.Open))
				{
					int num;
					byte[] buffer = new byte[bufferSize];
					while ((num = fileStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						if (Hash)
							Console.Write("#");

						stream.Write(buffer, 0, num);
					}
				}
			}

			return getStatusDescription(request);
		}

		public string ReturnUri()
		{
			return uri;
		}


		private FtpWebRequest createRequest(string method)
		{
			return createRequest(uri, method);
		}

		private FtpWebRequest createRequest(string uri, string method)
		{
			var r = (FtpWebRequest)WebRequest.Create(uri);

			r.Credentials = new NetworkCredential(userName, password);
			r.Method = method;
			r.UseBinary = Binary;
			r.EnableSsl = EnableSsl;
			r.UsePassive = Passive;

			return r;
		}

		private string getStatusDescription(FtpWebRequest request)
		{
			using (var response = (FtpWebResponse)request.GetResponse())
			{
				return response.StatusDescription;
			}
		}

		private string combine(string path1, string path2)
		{
			return Path.Combine(path1, path2).Replace("\\", "/");
		}
	}
}
