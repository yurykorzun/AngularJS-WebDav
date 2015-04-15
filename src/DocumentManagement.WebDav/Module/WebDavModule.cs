using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;
using Common.Logging;
using DocumentManagement.WebDav.Handler;

namespace DocumentManagement.WebDav.Module
{
	public class WebDavModule : IHttpModule
	{
		public void Dispose() { }

		public void Init(HttpApplication httpApp)
		{
			httpApp.BeginRequest += OnBeginRequest;

			var enableHttpInterception = Convert.ToBoolean(ConfigurationManager.AppSettings["enableHttpLogging"]);

			if (enableHttpInterception)
			{
				httpApp.EndRequest += OnEndRequest;
				httpApp.AuthorizeRequest += OnAuthorizeRequest;
			}
		}

		private static void OnBeginRequest(object sender, EventArgs e)
		{
			//logging request
			var log = LogManager.GetCurrentClassLogger();
			
			//intercept response stream
			var response = HttpContext.Current.Response;
			var filter = new OutputFilterStream(response.Filter);
			response.Filter = filter;

			var httpApplication = (HttpApplication)sender;
			log.Debug("---------------------------------");
			log.Debug("Request");
			log.Debug(httpApplication.Request.HttpMethod.ToUpper());
			var headerList = new StringBuilder();
			foreach (string header in httpApplication.Request.Headers)
			{
				headerList.AppendFormat("[{0} : {1}]", header, httpApplication.Request.Headers[header]);
			}
			log.DebugFormat("Headers:{0}", headerList);
			var bodyStream = new StreamReader(httpApplication.Request.InputStream);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
			var bodyText = bodyStream.ReadToEnd();
			log.Debug(bodyText);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
		}

		private static void OnEndRequest(object sender, EventArgs e)
		{
			//logging response
			var log = LogManager.GetCurrentClassLogger();

			var httpApplication = (HttpApplication)sender;
			log.Debug("---------------------------------");
			log.Debug("Response");
			log.Debug(httpApplication.Response.ContentType);
			log.Debug(httpApplication.Response.Status);
			log.Debug(httpApplication.Response.StatusCode);
			var headerList = new StringBuilder();
			foreach (string header in httpApplication.Response.Headers)
			{
				headerList.AppendFormat("[{0} : {1}]", header, httpApplication.Response.Headers[header]);
			}
			log.DebugFormat("Headers:{0}", headerList);
			var filter  = ((OutputFilterStream)httpApplication.Response.Filter).GetCopyStream;
			var bodyStream = new StreamReader(filter);
			bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
			var bodyText = bodyStream.ReadToEnd();
			log.Debug(bodyText);
			bodyStream.Close();
		}

		private static void OnAuthorizeRequest(object sender, EventArgs e)
		{
			var httpApplication = (HttpApplication)sender;
			WebDavHandler.HandleRequest(httpApplication);
		}
	}

	#region Replacement for response stream

	public class OutputFilterStream : Stream
	{
		private readonly Stream InnerStream;
		private readonly MemoryStream CopyStream;

		public OutputFilterStream(Stream inner)
		{
			this.InnerStream = inner;
			this.CopyStream = new MemoryStream();
		}

		public string ReadStream()
		{
			lock (this.InnerStream)
			{
				if (this.CopyStream.Length <= 0L ||
					!this.CopyStream.CanRead ||
					!this.CopyStream.CanSeek)
				{
					return String.Empty;
				}

				long pos = this.CopyStream.Position;
				this.CopyStream.Position = 0L;
				try
				{
					return new StreamReader(this.CopyStream).ReadToEnd();
				}
				finally
				{
					try
					{
						this.CopyStream.Position = pos;
					}
					catch
					{
					}
				}
			}
		}

		public MemoryStream GetCopyStream
		{
			get { return CopyStream; }
		}

		public override bool CanRead
		{
			get { return this.InnerStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return this.InnerStream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return this.InnerStream.CanWrite; }
		}

		public override void Flush()
		{
			this.InnerStream.Flush();
		}

		public override long Length
		{
			get { return this.InnerStream.Length; }
		}

		public override long Position
		{
			get { return this.InnerStream.Position; }
			set { this.CopyStream.Position = this.InnerStream.Position = value; }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.InnerStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			this.CopyStream.Seek(offset, origin);
			return this.InnerStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			this.CopyStream.SetLength(value);
			this.InnerStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.CopyStream.Write(buffer, offset, count);
			this.InnerStream.Write(buffer, offset, count);
		}
	}

	#endregion

}
	

