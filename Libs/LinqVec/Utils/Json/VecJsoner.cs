﻿using LinqVec.Utils.Json.Converters;
using PowBasics.Json_;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace LinqVec.Utils.Json;

public static class VecJsoner
{
	private static readonly JsonSerializerOptions configJsonOpt = new()
	{
		WriteIndented = true,
		AllowTrailingCommas = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver
		{
			Modifiers = {
				typeInfo => {
					foreach (var prop in typeInfo.Properties)
					{
						prop.IsRequired = prop.AttributeProvider!.GetCustomAttributes(true).Any(e => e is JsonIgnoreAttribute) switch {
							false => true,
							true => false
						};
					}
				}
			}
		}
	};

	private static readonly JsonSerializerOptions vecJsonOpt = new()
	{
		WriteIndented = true,
		AllowTrailingCommas = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};

	static VecJsoner()
	{
		AddConverters(configJsonOpt);
		AddConverters(vecJsonOpt);
	}
	
	private static void AddConverters(JsonSerializerOptions opt)
	{
		opt.Converters.Add(ConverterMaker.ColorConverter);
		opt.Converters.Add(new OptionConverterFactory());
		opt.Converters.Add(new JsonStringEnumConverter());
	}

	public static readonly Jsoner Config = new(configJsonOpt);
	public static readonly Jsoner Vec = new(vecJsonOpt);
}