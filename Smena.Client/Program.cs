using Microsoft.Extensions.Configuration;
using System;
using System.Windows.Forms;

namespace Smena.Client;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		var config = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
			.AddEnvironmentVariables()
			.Build();

		var address = config["Grpc:Address"] ?? "http://localhost:5001";
		var apiKey =
			Environment.GetEnvironmentVariable("AVA_SMENA_API_KEY") ??
			Environment.GetEnvironmentVariable("Grpc__ApiKey") ??
			config["Grpc:ApiKey"] ??
			string.Empty;

		var grpcService = new GrpcService(address, apiKey);
		Application.Run(new MainForm(grpcService));
	}
}
