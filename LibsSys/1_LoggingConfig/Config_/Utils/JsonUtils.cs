using System.Text.Json;

namespace LoggingConfig.Config_.Utils;


static class JsonUtils
{
	private static readonly JsonSerializerOptions jsonOpt = new()
	{
		WriteIndented = true,
	};

	public static string Ser<T>(T e) => JsonSerializer.Serialize(e, jsonOpt);

	public static T Deser<T>(string str) => JsonSerializer.Deserialize<T>(str, jsonOpt) ?? throw new NullReferenceException($"JsonSerializer.Deserialize returned null for {typeof(T).FullName}");


	

	public static T LoadStr<T>(string str, string file, T defaultVal)
	{
		(bool, T) Load()
		{
			try
			{
				var e = Deser<T>(str);
				return (e is null) switch
				{
					true => (false, defaultVal),
					false => (true, e)
				};
			}
			catch (Exception)
			{
				return (false, defaultVal);
			}
		}

		var (success, data) = Load();
		if (!success)
		{
			data = defaultVal;
			SaveFile(file, data);
		}
		return data;
	}



	public static void SaveFile<T>(string filename, T e) => File.WriteAllText(filename, Ser(e));

	public static T LoadFile<T>(string filename, T defaultVal)
	{
		(bool, T) Load()
		{
			if (!File.Exists(filename)) return (false, defaultVal);
			try
			{
				var str = File.ReadAllText(filename);
				var e = Deser<T>(str);
				return (e is null) switch
				{
					true => (false, defaultVal),
					false => (true, e)
				};
			}
			catch (Exception)
			{
				return (false, defaultVal);
			}
		}

		var (success, data) = Load();
		if (!success)
		{
			data = defaultVal;
			SaveFile(filename, data);
		}
		return data;
	}
}