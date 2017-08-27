using System;
using JetBrains.Annotations;
using XyrusWorx.Diagnostics;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public class WebServiceResult : IResult
	{
		private int mStatusCode = 200;
		private string mErrorDescription;

		public WebServiceResult()
		{
		}
		public WebServiceResult(object data) : this()
		{
			Data = data;
		}
		public WebServiceResult(int statusCode) : this()
		{
			StatusCode = statusCode;
		}
		public WebServiceResult(int statusCode, object data) : this()
		{
			StatusCode = statusCode;
			Data = data;
		}

		public bool HasError => StatusCode >= 400;

		public string ErrorDescription
		{
			get { return mErrorDescription; }
			set { mErrorDescription = value; }
		}

		public ErrorDetails ErrorDetails { get; set; }

		public int StatusCode
		{
			get { return mStatusCode; }
			set
			{
				if (StatusCode < 100 || StatusCode >= 600)
				{
					throw new ArgumentOutOfRangeException();
				}

				mStatusCode = value;
			}
		}
		public object Data { get; set; }
		
		public void ThrowIfError()
		{
			new Result{HasError = HasError, ErrorDescription = ErrorDescription}.ThrowIfError();
		}
	}
}