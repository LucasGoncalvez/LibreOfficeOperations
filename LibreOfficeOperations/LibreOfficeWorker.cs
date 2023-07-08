using System;
using System.Diagnostics;
using System.Threading;

namespace LibreOfficeOperations
{
	public class LibreOfficeWorker
	{
		private class Worker
		{
			public string ErrorMessage { get; private set; }
			public Worker(object parameterObject)
			{
				Process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						WindowStyle = ProcessWindowStyle.Hidden,
						FileName = GetLibreofficeAppName(),
						Arguments = (string)parameterObject
					}
				};
			}
			public void DoWork()
			{
				ErrorMessage = string.Empty;
				try
				{
					using (Process)
					{
						Process.Start();
						Process.WaitForExit();
					}
				}
				catch (Exception e)
				{
					ErrorMessage = e.ToString();
				}
			}
			public Process Process { get; }
		}
		public void DoWork(string argumentString, int? timeForWaiting = null)
		{
			var worker = new Worker(argumentString);
			var thread = new Thread(worker.DoWork)
			{
				IsBackground = true
			};
			thread.Start();

			if (timeForWaiting.HasValue && !thread.Join(TimeSpan.FromSeconds(timeForWaiting.Value)))
			{
				worker.Process.Kill();
				worker.Process.Dispose();
				thread.Abort();
				throw new ConvertDocumentException("LibreOffice process didn't respond within the expected time");
			}

			if (timeForWaiting.HasValue)
				return;

			try
			{
				thread.Join();
			}
			catch (ThreadAbortException)
			{
				worker.Process.Kill();
				worker.Process.Dispose();
				thread.Abort();
				throw;
			}
			catch (Exception)
			{
				if (thread.IsAlive)
					thread.Abort();
			}

			if (!string.IsNullOrEmpty(worker.ErrorMessage))
			{
				throw new ConvertDocumentException($"LibreOffice process error {worker.ErrorMessage}");
			}
		}

		private static bool IsLinux()
		{
            return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
        }

		private static string GetLibreofficeAppName()
		{
            return IsLinux() ? "libreoffice" : "soffice.exe";
        }
	}
}
