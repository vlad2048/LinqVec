using System.Reflection;

namespace LoggingConfig.File_;

public static class FileUtils
{
	public static string GetFilenameRelativeToExe(string folder, string file) =>
		Path.Combine(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new ArgumentException(),
			folder,
			file
		).CreateFolderIFN();


	private static string CreateFolderIFN(this string filename)
	{
		var folder = Path.GetDirectoryName(filename) ?? throw new ArgumentException();
		if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
		return filename;
	}
}